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
        private Text uiText;
        

        private void Awake()
        {
            uiText = GetComponent<Text>();
            translationKey = uiText.text;
        }

        private void Start()
        {
            ApplyTranslation();
        }
        public void ApplyTranslation()
        {
            if (string.IsNullOrEmpty(translationKey)) return;
            uiText.text = TranslationManager.Instance.Get(translationKey);
        }
    }
}