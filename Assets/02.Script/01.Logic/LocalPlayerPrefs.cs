namespace TF
{
    //!<=================================================================================
    //None Encryption
    public static class LocalPlayerPrefs
    {
        public static void DoForceCleanup()
        {
            UnityEngine.PlayerPrefs.DeleteAll();
            UnityEngine.PlayerPrefs.Save();
        }
        public static int GetInt(string key)
        {
            return UnityEngine.PlayerPrefs.GetInt(key);
        }
        public static int GetInt(string key, int defaultValue)
        {
            return UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
        }
        public static void SetInt(string key, int value)
        {
            UnityEngine.PlayerPrefs.SetInt(key, value);
        }
        public static float GetFloat(string key)
        {
            return UnityEngine.PlayerPrefs.GetFloat(key);
        }
        public static float GetFloat(string key, float defaultValue)
        {
            return UnityEngine.PlayerPrefs.GetFloat(key, defaultValue);
        }
        public static void SetFloat(string key, float value)
        {
            UnityEngine.PlayerPrefs.SetFloat(key, value);
        }
        public static string GetString(string key)
        {
            return UnityEngine.PlayerPrefs.GetString(key);
        }
        public static string GetString(string key, string defaultValue)
        {
            return UnityEngine.PlayerPrefs.GetString(key, defaultValue);
        }
        public static void SetString(string key, string value)
        {
            UnityEngine.PlayerPrefs.SetString(key, value);
        }
        public static bool GetBoolean(string key)
        {
            return UnityEngine.PlayerPrefs.GetInt(key, 0) == 1;
        }
        public static bool GetBoolean(string key, bool defaultValue)
        {
            return UnityEngine.PlayerPrefs.GetInt(key, defaultValue == true ? 1 : 0) == 1;
        }
        public static void SetBoolean(string key, bool value)
        {
            UnityEngine.PlayerPrefs.SetInt(key, value == true ? 1 : 0);
        }
        public static bool HasKey(string key)
        {
            return UnityEngine.PlayerPrefs.HasKey(key);
        }
        public static void DeleteKey(string key)
        {
            UnityEngine.PlayerPrefs.DeleteKey(key);
        }
        public static void SavePrefs()
        {
            UnityEngine.PlayerPrefs.Save();
        }
    }

    //!<=================================================================================
}
