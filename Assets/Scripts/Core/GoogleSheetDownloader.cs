// Uncomment to turn on debug lines
//#define DEBUG_LOGS

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using WebGLLocalStorage;
using Arena.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class PersistentDataPathOpener
{
    // Adds a new menu item to the Unity Editor's top menu bar.
    // The path "Tools/Open Persistent Data Path" creates a "Tools" menu with an option "Open Persistent Data Path".
    [MenuItem("Tools/Open Persistent Data Path")]
    public static void OpenPersistentDataPath()
    {
        string path = Application.persistentDataPath;

        // Ensure the directory exists before trying to open it
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Debug.Log("Created persistent data path folder: " + path);
        }

        EditorUtility.RevealInFinder(path);
    }
}
#endif

public class GoogleSheetDownloader : MonoBehaviour
{
    private const string urlFormatWithTabGid = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv&gid={1}";
    public string sheetID = "1PSUTZGtU5XhHl7WjrSL9_fjz8Jt0b4lZsxWQrI96uB4";
    public string updateTabGid = "1061358908";
    public int MaxDownloads = 3;
    [HideInInspector]
    public bool HasData = false;
    [HideInInspector]
    public Dictionary<string, string> Data = new Dictionary<string, string> ();
    [HideInInspector]
    public Dictionary<string, string> CachedData = new Dictionary<string, string>();
    
    private Queue<UpdateDataItem> downloadDataItems = new Queue<UpdateDataItem>();
    private int activeDownloads = 0;

    public class UpdateDataItem
    {
        public string TabName;
        public string Timestamp;
        public string TabID;
    }

    IEnumerator Start()
    {
        Data.Clear();
        CachedData.Clear();

        // Load our cached data, if it exists
        string json = FileAccessManager.Load("cachedData.json");
        if (!string.IsNullOrEmpty(json))
        { 
            CachedData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        // Grab our Updates data, if it exists
        List<UpdateDataItem> CachedUpdatesDataItems = new List<UpdateDataItem>();
        if (CachedData.ContainsKey("Updates"))
        {
            CachedUpdatesDataItems = JsonConvert.DeserializeObject<List<UpdateDataItem>>(CachedData["Updates"]);
        }
        Dictionary<string, string> CachedUpdatesData = new Dictionary<string, string>();
        foreach (var dataItem in CachedUpdatesDataItems)
        {
            CachedUpdatesData.Add(dataItem.TabName, dataItem.Timestamp);
        }

        // Download the latest Updates data
        yield return DownloadSheet(sheetID, "Updates", updateTabGid);
        List<UpdateDataItem> LatestUpdatesDataItems = JsonConvert.DeserializeObject<List<UpdateDataItem>>(Data["Updates"]);
        Dictionary<string, UpdateDataItem> LatestUpdatesData = new Dictionary<string, UpdateDataItem>();
        foreach (var dataItem in LatestUpdatesDataItems)
        {
            LatestUpdatesData.Add(dataItem.TabName, dataItem);
        }

        // Build a list of each Tab to update, by checking if the timestamps differ
        foreach (var latestDataItem in LatestUpdatesDataItems)
        {
            // If we never downloaded it, download it
            if (!CachedUpdatesData.TryGetValue(latestDataItem.TabName, out string cachedTimestamp))
            {
#if DEBUG_LOGS
                Debug.LogError("GoogleSheets: Will download new tab: " + latestDataItem.TabName);
#endif
                downloadDataItems.Enqueue(latestDataItem);
                continue;
            }

            // If we have downloaded it, we check the cached data to see if
            // it matches the lastest timestamp
            if (latestDataItem.Timestamp != cachedTimestamp)
            {
#if DEBUG_LOGS
                Debug.LogError("GoogleSheets: Will download updated tab: " + latestDataItem.TabName);
#endif
                downloadDataItems.Enqueue(latestDataItem);
                continue;
            }

            // The data is the same, add the cached data to our updated data
#if DEBUG_LOGS
            Debug.Log("GoogleSheets: Has latest version of tab: " + latestDataItem.TabName);
#endif
            Data.Add(latestDataItem.TabName, CachedData[latestDataItem.TabName]);
        }

        // Download all the sheets that updated
        if (downloadDataItems.Count > 0)
        {
            for (int i = 0; i < MaxDownloads; ++i)
            {
                StartCoroutine(DownloadWorker());
            }
        }

        yield return new WaitUntil(() => activeDownloads == 0);

        // Save our data
        json = JsonConvert.SerializeObject(Data);
        FileAccessManager.Save("cachedData.json", json);

        HasData = true;
    }

    IEnumerator DownloadWorker()
    {
        while (downloadDataItems.Count > 0)
        {
            var dataItem = downloadDataItems.Dequeue();
            yield return StartCoroutine(DownloadSheet(sheetID, dataItem.TabName, dataItem.TabID));
        }
    }

    IEnumerator DownloadSheet(string docID, string tabName, string tabGid)
    {
        if (string.IsNullOrEmpty(docID) || string.IsNullOrEmpty(tabName) || string.IsNullOrEmpty(tabGid))
        {
            Debug.LogError("GoogleSheets: Invalid/empty docID, tabName, or tabGid");
            yield return null;
        }
        activeDownloads++;
        string uri = string.Format(urlFormatWithTabGid, docID, tabGid);
#if DEBUG_LOGS
        Debug.Log("GoogleSheets: getting " + uri);
#endif
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            // handle an unknown internet error
            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarningFormat("GoogleSheets: Couldn't retrieve sheet at [{0}], error message: {1}", webRequest.url, webRequest.error);
            }
            else
            {
                // make sure the fetched file isn't just a Google login page
                string requestText = webRequest.downloadHandler.text;
#if DEBUG_LOGS
                Debug.Log("GoogleSheets: Downloaded text: " + requestText);
#endif
                if (requestText.Contains("google-site-verification"))
                {
                    Debug.LogWarningFormat("GoogleSheets: Couldn't retrieve file at [{0}], likely due to the sheet not set for public access enabled", webRequest.url);
                }
                string json = ConvertCsvTextToJsonObject(requestText);
#if DEBUG_LOGS
                Debug.Log("GoogleSheets: Converted Json: " + json);
#endif
                Data.Add(tabName, json);
            }
        }
        activeDownloads--;
    }

    public static List<string> ParseCsvLine(string line)
    {
        // A CSV line is comma separated, but we might have
        // quotes in our text that include commas as well, and
        // we don't want to split in that situtaion.
        // For example,
        // one line has:
        // 1, 2, 3, 4
        // Another has:
        // 1, "2, 5, 6, 7, 8", 3, "4, 4, 4"
        // We want the commas in the quotes to stay with the field they are a part of.

        List<string> fields = new List<string>();
        bool inQuotes = false;
        int start = 0;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '\"')
            {
                inQuotes = !inQuotes; // Toggle quote state
            }
            else if (line[i] == ',' && !inQuotes)
            {
                fields.Add(CleanField(line.Substring(start, i - start)));
                start = i + 1;
            }
        }
        // Add the final field
        fields.Add(CleanField(line.Substring(start)));
        return fields;
    }

    private static string CleanField(string field)
    {
        field = field.Trim();
        if (field.StartsWith("\"") && field.EndsWith("\""))
        {
            // Remove surrounding quotes and handle escaped double quotes
            field = field.Substring(1, field.Length - 2).Replace("\"\"", "\"");
        }
        return field;
    }

    public string ConvertCsvTextToJsonObject(string text)
    {
        char[] delimiters = new char[] { '\r', '\n' };
        List<string> lines = new List<string>(text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries));
#if DEBUG_LOGS
        Debug.Log("GoogleSheets: CSV Lines: " + lines.Count);
#endif

        var properties = lines[0].Split(',');

        var listObjResult = new List<Dictionary<string, string>>();

        for (int i = 1; i < lines.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            List<string> fields = ParseCsvLine(lines[i]);
            var objResult = new Dictionary<string, string>();
            for (int j = 0; j < properties.Length; j++)
            {
                objResult.Add(properties[j], fields[j]);
            }
            listObjResult.Add(objResult);
        }
        return JsonConvert.SerializeObject(listObjResult);

    }

    public string ConvertCsvTextToJsonObject_old(string text)
    {
        char[] delimiters = new char[] { '\r', '\n' };
        List<string> lines = new List<string>(text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries));
#if DEBUG_LOGS
        Debug.Log("GoogleSheets: CSV Lines: " + lines.Count);
#endif
        var csv = new List<string[]>();
        foreach (string line in lines)
        {
            csv.Add(line.Split(','));
        }

        var properties = lines[0].Split(',');

        var listObjResult = new List<Dictionary<string, string>>();

        for (int i = 1; i < lines.Count; i++)
        {
            var objResult = new Dictionary<string, string>();
            for (int j = 0; j < properties.Length; j++)
            {
                objResult.Add(properties[j], csv[i][j]);
            }
            listObjResult.Add(objResult);
        }

        return JsonConvert.SerializeObject(listObjResult);
    }
}
