using System.Collections.Generic;
using Loader;
using UI;
using UnityEngine;

namespace Manager
{
    public class TranslationManager : Singleton<TranslationManager>
    {
        private string currentLanguage = "en";
        private readonly Dictionary<string, string> translations = new();

        public string CurrentLanguage => currentLanguage;

        public void Awake()
        {
            var lang = SettingLoader.Instance.SettingData["language"].ToString();
            switch (lang)
            {
                case "English":
                    lang = "en";
                    break;
                case "Chinese":
                    lang = "zh";
                    break;
            }
            currentLanguage = lang;
            LoadLanguage(lang);
        }

        private void LoadLanguage(string lang)
        {
            translations.Clear();
            var textAsset = Resources.Load<TextAsset>($"Localization/{lang}");

            if (textAsset != null)
            {
                var data = JsonUtility.FromJson<TranslationData>("{\"entries\":" + textAsset.text + "}");
                foreach (var entry in data.entries)
                {
                    translations[entry.key] = entry.value;
                }
            }
            else
            {
                Debug.LogWarning("Translation file not found: " + lang);
            }
        }

        public string Get(string key)
        {
            return translations.TryGetValue(key, out string value) ? value : key;
        }
        public void SetLanguage(string lang)
        {
            lang = lang switch
            {
                "English" => "en",
                "Chinese" => "zh",
                _ => lang
            };
            currentLanguage = lang;
            LoadLanguage(lang);
            TranslationText.RefreshAll();
        }


        [System.Serializable]
        private class TranslationData
        {
            public List<TranslationEntry> entries;
        }

        [System.Serializable]
        private class TranslationEntry
        {
            public string key;
            public string value;
        }
    }
}