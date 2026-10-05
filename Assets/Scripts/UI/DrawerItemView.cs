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
        [SerializeField] private Image badgeBackgroundImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI chevronText;
        [SerializeField] private Image selectionBorder;
        [SerializeField] private ItemDragHandler dragHandler;
        [SerializeField] private Button clickButton;

        [Header("Styling")]
        [SerializeField] private Color selectedColor = new Color(0.85f, 0.18f, 0.18f, 1f); // D&D Beyond Red
        [SerializeField] private Color normalBorderColor = new Color(0.88f, 0.88f, 0.90f, 1f); // #E0E0E0

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
                titleText.text = data != null ? data.displayName.ToUpper() : "ITEM";
                titleText.color = new Color(0.14f, 0.15f, 0.16f); // #242527
            }

            if (subtitleText != null)
            {
                subtitleText.text = GetSubtitleText(data);
                subtitleText.color = new Color(0.40f, 0.40f, 0.40f); // #666666
            }

            if (data != null)
            {
                if (data.category == OptionCategory.Class)
                {
                    if (badgeBackgroundImage != null) badgeBackgroundImage.color = data.primaryColor;
                    if (iconImage != null && data.icon != null)
                    {
                        iconImage.sprite = data.icon;
                        iconImage.color = Color.white; // Official white vector class emblem
                    }
                }
                else
                {
                    if (badgeBackgroundImage != null) badgeBackgroundImage.color = new Color(0.93f, 0.94f, 0.96f);
                    if (iconImage != null && data.icon != null)
                    {
                        iconImage.sprite = data.icon;
                        iconImage.color = data.primaryColor;
                    }
                }
            }

            if (chevronText != null)
            {
                chevronText.text = ">";
                chevronText.color = new Color(0.50f, 0.57f, 0.65f); // #8091A5
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
