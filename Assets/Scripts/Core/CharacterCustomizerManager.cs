using System;
using System.Collections.Generic;
using UnityEngine;
using DNDBeyond.Data;

namespace DNDBeyond.Core
{
    public class CharacterCustomizerManager : MonoBehaviour
    {
        public static CharacterCustomizerManager Instance { get; private set; }

        [Header("Available Options")]
        [SerializeField] private List<CharacterOptionSO> availableOptions = new List<CharacterOptionSO>();

        [Header("State")]
        [SerializeField] private CharacterBuild currentBuild = new CharacterBuild();

        [Header("Rule Evaluator")]
        [SerializeField] private RuleHarmonyEvaluator evaluator;

        [Header("Center Stage Mannequin")]
        [SerializeField] private GameObject mannequinRoot;

        public event Action<CharacterBuild> OnCharacterUpdated;
        public event Action<HarmonyEvaluationResult> OnHarmonyEvaluated;

        public CharacterBuild CurrentBuild => currentBuild;
        public List<CharacterOptionSO> AvailableOptions => availableOptions;
        public GameObject MannequinRoot => mannequinRoot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (evaluator == null)
            {
                evaluator = GetComponent<RuleHarmonyEvaluator>();
                if (evaluator == null)
                {
                    evaluator = gameObject.AddComponent<RuleHarmonyEvaluator>();
                }
            }

            if (mannequinRoot == null)
            {
                var found = GameObject.Find("ModularMannequin");
                if (found != null) mannequinRoot = found;
            }
        }

        private void Start()
        {
            UpdateMannequinVisibility();
            // Initial broadcast
            NotifyChanges();
        }

        public void SetMannequinRoot(GameObject root)
        {
            mannequinRoot = root;
            UpdateMannequinVisibility();
        }

        public void UpdateMannequinVisibility()
        {
            if (mannequinRoot == null)
            {
                var found = GameObject.Find("ModularMannequin");
                if (found != null) mannequinRoot = found;
            }

            if (mannequinRoot == null) return;

            bool hasAnyChoice = currentBuild != null && (
                currentBuild.currentRace != null ||
                currentBuild.currentClass != null ||
                currentBuild.currentArmor != null ||
                currentBuild.currentWeapon != null ||
                currentBuild.currentHair != null ||
                currentBuild.currentHorns != null
            );

            if (mannequinRoot.activeSelf != hasAnyChoice)
            {
                mannequinRoot.SetActive(hasAnyChoice);
            }
        }

        public void SetAvailableOptions(List<CharacterOptionSO> options)
        {
            availableOptions = options;
        }

        public List<CharacterOptionSO> GetOptionsForCategory(OptionCategory category)
        {
            List<CharacterOptionSO> result = new List<CharacterOptionSO>();
            foreach (var opt in availableOptions)
            {
                if (opt != null && opt.category == category)
                {
                    result.Add(opt);
                }
            }
            return result;
        }

        public void SelectOption(CharacterOptionSO option)
        {
            if (option == null) return;

            switch (option.category)
            {
                case OptionCategory.Race:
                    currentBuild.currentRace = option;
                    if (option.raceType == CharacterRace.Tiefling && (currentBuild.currentHorns == null || currentBuild.currentHorns.id == "app_horns_none"))
                    {
                        var hornOpt = availableOptions.Find(o => o.id == "app_horns_1");
                        if (hornOpt != null) currentBuild.currentHorns = hornOpt;
                    }
                    else if (option.raceType == CharacterRace.Elf && currentBuild.currentHorns != null && currentBuild.currentHorns.id == "app_horns_1")
                    {
                        var noHornOpt = availableOptions.Find(o => o.id == "app_horns_none");
                        if (noHornOpt != null) currentBuild.currentHorns = noHornOpt;
                    }
                    if (currentBuild.currentHair == null)
                    {
                        var hairOpt = availableOptions.Find(o => o.id == "app_hair_1");
                        if (hairOpt != null) currentBuild.currentHair = hairOpt;
                    }
                    break;
                case OptionCategory.Class:
                    currentBuild.currentClass = option;
                    break;
                case OptionCategory.Armor:
                    currentBuild.currentArmor = option as EquipmentSO;
                    break;
                case OptionCategory.Weapon:
                    currentBuild.currentWeapon = option as EquipmentSO;
                    break;
                case OptionCategory.Appearance:
                    if (option.appearanceSlot == AppearanceSlot.Hair)
                    {
                        currentBuild.currentHair = option;
                    }
                    else if (option.appearanceSlot == AppearanceSlot.Horns)
                    {
                        currentBuild.currentHorns = option;
                    }
                    break;
            }

            NotifyChanges();
        }

        public void EquipArmor(EquipmentSO armor)
        {
            currentBuild.currentArmor = armor;
            NotifyChanges();
        }

        public void EquipWeapon(EquipmentSO weapon)
        {
            currentBuild.currentWeapon = weapon;
            NotifyChanges();
        }

        public void SelectRace(CharacterOptionSO race)
        {
            currentBuild.currentRace = race;
            NotifyChanges();
        }

        public void SelectClass(CharacterOptionSO charClass)
        {
            currentBuild.currentClass = charClass;
            NotifyChanges();
        }

        public void NotifyChanges()
        {
            UpdateMannequinVisibility();
            OnCharacterUpdated?.Invoke(currentBuild);

            if (evaluator != null)
            {
                var result = evaluator.Evaluate(currentBuild);
                OnHarmonyEvaluated?.Invoke(result);
            }
        }
    }
}
