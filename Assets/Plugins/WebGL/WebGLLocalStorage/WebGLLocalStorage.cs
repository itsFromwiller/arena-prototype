using System.Runtime.InteropServices;

namespace WebGLLocalStorage
{
    public static class LocalStorageManager
    {
        [DllImport("__Internal")]
        private static extern void SaveToLocalStorage(string key, string value);

        [DllImport("__Internal")]
        private static extern string LoadFromLocalStorage(string key);

        [DllImport("__Internal")]
        private static extern void RemoveFromLocalStorage(string key);

        public static void Save(string key, string json)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SaveToLocalStorage(key, json);
#else
            UnityEngine.Debug.LogWarning("WebGLLocalStorage Save() is not available on this platform");
#endif
        }

        public static string Load(string key)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return LoadFromLocalStorage(key);
#else
            UnityEngine.Debug.LogWarning("WebGLLocalStorage Load() is not available on this platform");
            return null;
#endif
        }

        public static void Remove(string key)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            RemoveFromLocalStorage(key);
#else
            UnityEngine.Debug.LogWarning("WebGLLocalStorage Remove() is not available on this platform");
#endif
        }
    }
}