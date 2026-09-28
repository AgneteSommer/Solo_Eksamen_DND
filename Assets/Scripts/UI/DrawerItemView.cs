using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DNDBeyond.Core;
using DNDBeyond.Data;

namespace DNDBeyond.UI
{
    public class DrawerItemView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private Image selectionBorder;
        [SerializeField] private ItemDragHandler dragHandler;
        [SerializeField] private Button clickButton;

        [Header("Styling")]
        [SerializeField] private Color selectedColor = new Color(0.95f, 0.75f, 0.2f, 1f);
        [SerializeField] private Color normalBorderColor = new Color(0.3f, 0.35f, 0.4f, 0.5f);

        private CharacterOptionSO optionData;

        public CharacterOptionSO OptionData => optionData;

        public void Bind(CharacterOptionSO data, bool isSelected)
        {
            optionData = data;

            if (dragHandler != null)
            {
                dragHandler.Initialize(data);
            }

            if (titleText != null)
            {
                titleText.text = data != null ? data.displayName : "Item";
            }

            if (subtitleText != null)
            {
                subtitleText.text = GetSubtitleText(data);
            }

            if (iconImage != null && data != null && data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.color = data.primaryColor;
            }

            SetSelected(isSelected);

            if (clickButton != null)
            {
                clickButton.onClick.RemoveAllListeners();
                clickButton.onClick.AddListener(() =>
                {
                    if (CharacterCustomizerManager.Instance != null && optionData != null)
                    {
                        CharacterCustomizerManager.Instance.SelectOption(optionData);
                    }
                });
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (selectionBorder != null)
            {
                selectionBorder.color = isSelected ? selectedColor : normalBorderColor;
                selectionBorder.gameObject.SetActive(true);
            }
        }

        private string GetSubtitleText(CharacterOptionSO data)
        {
            if (data == null) return "";

            if (data is EquipmentSO equip)
            {
                if (equip.category == OptionCategory.Armor)
                {
                    string acStr = equip.armorType == ArmorType.None ? "Unarmored" : $"AC {equip.baseAC}";
                    string disadv = equip.stealthDisadvantage ? " (Stealth Disadv)" : "";
                    return $"{equip.armorType} Armor | {acStr}{disadv}";
                }
                else if (equip.category == OptionCategory.Weapon)
                {
                    string focus = equip.isArcaneFocus ? " [Arcane Focus]" : "";
                    return $"{equip.damage} {equip.damageType}{focus}";
                }
            }

            if (data.category == OptionCategory.Race)
            {
                return data.flavorTagline;
            }
            if (data.category == OptionCategory.Class)
            {
                return data.flavorTagline;
            }

            return data.flavorTagline;
        }
    }
}
