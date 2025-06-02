using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Text))]
    public class TranslationText : MonoBehaviour
    {
        [SerializeField] private string translationKey;
        private static List<TranslationText> all = new();
        private Text uiText;
        

        private void Awake()
        {
            uiText = GetComponent<Text>();
            translationKey = uiText.text;
            all.Add(this);
        }

        private void Start()
        {
            ApplyTranslation();
        }
        private void OnDestroy()
        {
            all.Remove(this);
        }
        public static void RefreshAll()
        {
            foreach (var t in all)
            {
                t.ApplyTranslation();
            }
        }

        public void ApplyTranslation()
        {
            if (string.IsNullOrEmpty(translationKey)) return;
            uiText.text = TranslationManager.Instance.Get(translationKey);
        }
    }
}