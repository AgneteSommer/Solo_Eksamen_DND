using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DNDBeyond.Core;
using DNDBeyond.Data;

namespace DNDBeyond.UI
{
    public class CategoryDrawerUI : MonoBehaviour
    {
        [Header("Category Tabs")]
        [SerializeField] private Button raceTabButton;
        [SerializeField] private Button classTabButton;
        [SerializeField] private Button armorTabButton;
        [SerializeField] private Button weaponTabButton;

        [Header("Tab Visuals")]
        [SerializeField] private Color tabActiveColor = new Color(0.25f, 0.45f, 0.65f, 1f);
        [SerializeField] private Color tabInactiveColor = new Color(0.15f, 0.18f, 0.22f, 1f);

        [Header("Drawer Content Container")]
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private GameObject drawerItemPrefab;

        private OptionCategory activeCategory = OptionCategory.Race;
        private List<DrawerItemView> activeItemViews = new List<DrawerItemView>();

        private void Start()
        {
            if (raceTabButton != null) raceTabButton.onClick.AddListener(() => SwitchCategory(OptionCategory.Race));
            if (classTabButton != null) classTabButton.onClick.AddListener(() => SwitchCategory(OptionCategory.Class));
            if (armorTabButton != null) armorTabButton.onClick.AddListener(() => SwitchCategory(OptionCategory.Armor));
            if (weaponTabButton != null) weaponTabButton.onClick.AddListener(() => SwitchCategory(OptionCategory.Weapon));

            if (CharacterCustomizerManager.Instance != null)
            {
                CharacterCustomizerManager.Instance.OnCharacterUpdated += HandleCharacterUpdated;
            }

            // Default to Race or Armor
            SwitchCategory(OptionCategory.Armor);
        }

        private void OnDestroy()
        {
            if (CharacterCustomizerManager.Instance != null)
            {
                CharacterCustomizerManager.Instance.OnCharacterUpdated -= HandleCharacterUpdated;
            }
        }

        public void SwitchCategory(OptionCategory newCategory)
        {
            activeCategory = newCategory;
            UpdateTabVisuals();
            PopulateDrawerItems();
        }

        private void UpdateTabVisuals()
        {
            SetTabButtonState(raceTabButton, activeCategory == OptionCategory.Race);
            SetTabButtonState(classTabButton, activeCategory == OptionCategory.Class);
            SetTabButtonState(armorTabButton, activeCategory == OptionCategory.Armor);
            SetTabButtonState(weaponTabButton, activeCategory == OptionCategory.Weapon);
        }

        private void SetTabButtonState(Button btn, bool isActive)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isActive ? tabActiveColor : tabInactiveColor;
            }
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = isActive ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.9f);
            }
        }

        public void PopulateDrawerItems()
        {
            if (CharacterCustomizerManager.Instance == null || itemsContainer == null || drawerItemPrefab == null) return;

            // Clear old items
            for (int i = itemsContainer.childCount - 1; i >= 0; i--)
            {
                var child = itemsContainer.GetChild(i);
                child.SetParent(null);
                Destroy(child.gameObject);
            }
            activeItemViews.Clear();

            var options = CharacterCustomizerManager.Instance.GetOptionsForCategory(activeCategory);
            var build = CharacterCustomizerManager.Instance.CurrentBuild;

            foreach (var opt in options)
            {
                if (opt == null) continue;

                var itemGO = Instantiate(drawerItemPrefab, itemsContainer);
                var view = itemGO.GetComponent<DrawerItemView>();
                if (view != null)
                {
                    bool isSelected = IsOptionSelected(opt, build);
                    view.Bind(opt, isSelected);
                    activeItemViews.Add(view);
                }
            }
        }

        private void HandleCharacterUpdated(CharacterBuild build)
        {
            foreach (var view in activeItemViews)
            {
                if (view != null && view.OptionData != null)
                {
                    view.SetSelected(IsOptionSelected(view.OptionData, build));
                }
            }
        }

        private bool IsOptionSelected(CharacterOptionSO opt, CharacterBuild build)
        {
            if (build == null || opt == null) return false;

            switch (opt.category)
            {
                case OptionCategory.Race:
                    return build.currentRace == opt;
                case OptionCategory.Class:
                    return build.currentClass == opt;
                case OptionCategory.Armor:
                    return build.currentArmor == opt;
                case OptionCategory.Weapon:
                    return build.currentWeapon == opt;
                default:
                    return false;
            }
        }
    }
}
