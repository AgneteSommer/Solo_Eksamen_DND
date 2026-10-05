using System;
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
        [Header("Level 1: Main Categories View")]
        [SerializeField] private GameObject mainCategoriesPanel;
        [SerializeField] private Button raceCategoryButton;
        [SerializeField] private Button classCategoryButton;
        [SerializeField] private Button armorCategoryButton;
        [SerializeField] private Button weaponCategoryButton;

        [Header("Main Category Equipped Labels")]
        [SerializeField] private TextMeshProUGUI raceEquippedText;
        [SerializeField] private TextMeshProUGUI classEquippedText;
        [SerializeField] private TextMeshProUGUI armorEquippedText;
        [SerializeField] private TextMeshProUGUI weaponEquippedText;

        [Header("Level 2: Subcategory View")]
        [SerializeField] private GameObject subcategoryPanel;
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI subcategoryTitleText;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private GameObject drawerItemPrefab;

        private OptionCategory currentSubcategory = OptionCategory.Class;
        private List<DrawerItemView> activeItemViews = new List<DrawerItemView>();

        private void Start()
        {
            if (raceCategoryButton != null) raceCategoryButton.onClick.AddListener(() => OpenSubcategory(OptionCategory.Race));
            if (classCategoryButton != null) classCategoryButton.onClick.AddListener(() => OpenSubcategory(OptionCategory.Class));
            if (armorCategoryButton != null) armorCategoryButton.onClick.AddListener(() => OpenSubcategory(OptionCategory.Armor));
            if (weaponCategoryButton != null) weaponCategoryButton.onClick.AddListener(() => OpenSubcategory(OptionCategory.Weapon));

            if (backButton != null) backButton.onClick.AddListener(ShowMainMenu);

            if (CharacterCustomizerManager.Instance != null)
            {
                CharacterCustomizerManager.Instance.OnCharacterUpdated += HandleCharacterUpdated;
                UpdateEquippedLabels(CharacterCustomizerManager.Instance.CurrentBuild);
            }

            // Start on Main Menu
            ShowMainMenu();
        }

        private void OnDestroy()
        {
            if (CharacterCustomizerManager.Instance != null)
            {
                CharacterCustomizerManager.Instance.OnCharacterUpdated -= HandleCharacterUpdated;
            }
        }

        public void ShowMainMenu()
        {
            if (mainCategoriesPanel != null) mainCategoriesPanel.SetActive(true);
            if (subcategoryPanel != null) subcategoryPanel.SetActive(false);

            if (CharacterCustomizerManager.Instance != null)
            {
                UpdateEquippedLabels(CharacterCustomizerManager.Instance.CurrentBuild);
            }
        }

        public void OpenSubcategory(OptionCategory category)
        {
            currentSubcategory = category;

            if (mainCategoriesPanel != null) mainCategoriesPanel.SetActive(false);
            if (subcategoryPanel != null) subcategoryPanel.SetActive(true);

            if (subcategoryTitleText != null)
            {
                switch (category)
                {
                    case OptionCategory.Race:
                        subcategoryTitleText.text = "SELECT SPECIES";
                        break;
                    case OptionCategory.Class:
                        subcategoryTitleText.text = "SELECT CLASS";
                        break;
                    case OptionCategory.Armor:
                        subcategoryTitleText.text = "SELECT ARMOR";
                        break;
                    case OptionCategory.Weapon:
                        subcategoryTitleText.text = "SELECT WEAPON";
                        break;
                }
            }

            PopulateSubcategoryItems();
        }

        public void PopulateSubcategoryItems()
        {
            if (CharacterCustomizerManager.Instance == null || itemsContainer == null || drawerItemPrefab == null) return;

            // Clear old items cleanly
            for (int i = itemsContainer.childCount - 1; i >= 0; i--)
            {
                var child = itemsContainer.GetChild(i);
                child.SetParent(null);
                Destroy(child.gameObject);
            }
            activeItemViews.Clear();

            var options = CharacterCustomizerManager.Instance.GetOptionsForCategory(currentSubcategory);

            // If Class category: sort in strict alphabetical order
            if (currentSubcategory == OptionCategory.Class)
            {
                options.Sort((a, b) => string.Compare(a.displayName, b.displayName, StringComparison.OrdinalIgnoreCase));
            }

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
            UpdateEquippedLabels(build);

            foreach (var view in activeItemViews)
            {
                if (view != null && view.OptionData != null)
                {
                    view.SetSelected(IsOptionSelected(view.OptionData, build));
                }
            }
        }

        private void UpdateEquippedLabels(CharacterBuild build)
        {
            if (build == null) return;

            if (raceEquippedText != null)
                raceEquippedText.text = build.currentRace != null ? $"Current: {build.currentRace.displayName}" : "Current: (None chosen)";

            if (classEquippedText != null)
                classEquippedText.text = build.currentClass != null ? $"Current: {build.currentClass.displayName}" : "Current: (None chosen)";

            if (armorEquippedText != null)
                armorEquippedText.text = build.currentArmor != null ? $"Current: {build.currentArmor.displayName} (AC {build.CalculateAC()})" : "Current: (None chosen)";

            if (weaponEquippedText != null)
                weaponEquippedText.text = build.currentWeapon != null ? $"Current: {build.currentWeapon.displayName}" : "Current: (None chosen)";
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

        public void SetupDrillDownReferences(
            GameObject mainPanel,
            Button raceBtn, TextMeshProUGUI raceLabel,
            Button classBtn, TextMeshProUGUI classLabel,
            Button armorBtn, TextMeshProUGUI armorLabel,
            Button weaponBtn, TextMeshProUGUI weaponLabel,
            GameObject subPanel,
            Button backBtn,
            TextMeshProUGUI subTitle,
            Transform itemsParent,
            GameObject itemPrefab)
        {
            mainCategoriesPanel = mainPanel;
            raceCategoryButton = raceBtn;
            raceEquippedText = raceLabel;
            classCategoryButton = classBtn;
            classEquippedText = classLabel;
            armorCategoryButton = armorBtn;
            armorEquippedText = armorLabel;
            weaponCategoryButton = weaponBtn;
            weaponEquippedText = weaponLabel;

            subcategoryPanel = subPanel;
            backButton = backBtn;
            subcategoryTitleText = subTitle;
            itemsContainer = itemsParent;
            drawerItemPrefab = itemPrefab;
        }
    }
}
