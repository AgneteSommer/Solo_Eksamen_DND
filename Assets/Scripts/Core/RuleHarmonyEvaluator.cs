using System;
using System.Collections.Generic;
using UnityEngine;
using DNDBeyond.Data;

namespace DNDBeyond.Core
{
    [Serializable]
    public class HarmonyEvaluationResult
    {
        public HarmonyState state;
        public string title;
        public string shortStatus;
        public string insightNote;
        public HarmonyRuleSO matchedRule;

        public bool IsQuirk => state == HarmonyState.DiscoveryQuirk;
    }

    public class RuleHarmonyEvaluator : MonoBehaviour
    {
        [Header("Rule Set")]
        [SerializeField] private List<HarmonyRuleSO> rules = new List<HarmonyRuleSO>();

        public void SetRules(List<HarmonyRuleSO> newRules)
        {
            rules = newRules;
        }

        public HarmonyEvaluationResult Evaluate(CharacterBuild build)
        {
            if (build == null)
            {
                return new HarmonyEvaluationResult
                {
                    state = HarmonyState.Harmonious,
                    title = "Character Creation",
                    shortStatus = "Select Options",
                    insightNote = "Choose your Race, Class, Armor, and Weapon from the drawer below.",
                    matchedRule = null
                };
            }

            CharacterRace race = build.RaceType;
            CharacterClass charClass = build.ClassType;
            ArmorType armor = build.CurrentArmorType;
            WeaponType weapon = build.CurrentWeaponType;

            // Sort rules by priority descending
            HarmonyRuleSO bestMatch = null;
            int highestPriority = int.MinValue;

            foreach (var rule in rules)
            {
                if (rule == null) continue;
                if (rule.Matches(race, charClass, armor, weapon))
                {
                    if (rule.priority > highestPriority)
                    {
                        highestPriority = rule.priority;
                        bestMatch = rule;
                    }
                }
            }

            if (bestMatch != null)
            {
                return new HarmonyEvaluationResult
                {
                    state = bestMatch.harmonyState,
                    title = bestMatch.ruleTitle,
                    shortStatus = bestMatch.shortStatus,
                    insightNote = bestMatch.insightNoteText,
                    matchedRule = bestMatch
                };
            }

            // Fallback default
            return new HarmonyEvaluationResult
            {
                state = HarmonyState.Harmonious,
                title = "Balanced Synergy",
                shortStatus = "Harmonious Build",
                insightNote = "This equipment setup functions naturally within standard 5e rules.",
                matchedRule = null
            };
        }
    }
}
