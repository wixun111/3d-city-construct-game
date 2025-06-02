using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BuildingToggle : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private Text label;
        [SerializeField] private Image icon;

        private int buildingId;

        public void Initialize(int id, string name, Sprite iconSprite, ToggleGroup group, System.Action<int,bool> onSelected)
        {
            buildingId = id;

            // 设置 UI
            label.text = name;
            if (icon != null && iconSprite != null) icon.sprite = iconSprite;
            if (toggle == null) return;
            toggle.group = group;
            toggle.onValueChanged.AddListener(isOn =>
            {
                onSelected?.Invoke(buildingId,isOn);
            });
        }
    }
}