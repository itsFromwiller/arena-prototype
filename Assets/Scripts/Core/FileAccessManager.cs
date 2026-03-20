using System.IO;
using UnityEngine;

namespace Arena.Core
{
    public static class FileAccessManager
    {
        public static void Save(string key, string json)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLLocalStorage.LocalStorageManager.Save(key, json);
#else
            string filePath = $"{Application.persistentDataPath}/{key}";
            File.WriteAllText(filePath, json);
#endif
        }

        public static string Load(string key)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return WebGLLocalStorage.LocalStorageManager.Load(key);
#else
            string filePath = $"{Application.persistentDataPath}/{key}";
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return null;
#endif
        }

        public static void Remove(string key)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLLocalStorage.LocalStorageManager.Remove(key, json);
#else
            string filePath = $"{Application.persistentDataPath}/{key}";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
#endif
        }
    }
}
