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

        public event Action<CharacterBuild> OnCharacterUpdated;
        public event Action<HarmonyEvaluationResult> OnHarmonyEvaluated;

        public CharacterBuild CurrentBuild => currentBuild;
        public List<CharacterOptionSO> AvailableOptions => availableOptions;

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
        }

        private void Start()
        {
            // Initial broadcast
            NotifyChanges();
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
            OnCharacterUpdated?.Invoke(currentBuild);

            if (evaluator != null)
            {
                var result = evaluator.Evaluate(currentBuild);
                OnHarmonyEvaluated?.Invoke(result);
            }
        }
    }
}
