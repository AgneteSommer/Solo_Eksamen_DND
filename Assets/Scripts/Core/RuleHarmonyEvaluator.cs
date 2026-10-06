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
            if (build == null || build.ClassType == CharacterClass.None)
            {
                return new HarmonyEvaluationResult
                {
                    state = HarmonyState.Harmonious,
                    title = "Synergy",
                    shortStatus = "Balanced Synergy",
                    insightNote = "Balanced Synergy — As you select your Class, Armor, and Weapons, your proficiencies and quirks will appear here.",
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
                // Safety guard: Quirks can NEVER trigger if a class has not yet been selected
                if (bestMatch.IsQuirk && charClass == CharacterClass.None)
                {
                    return new HarmonyEvaluationResult
                    {
                        state = HarmonyState.Harmonious,
                        title = "Synergy",
                        shortStatus = "Balanced Synergy",
                        insightNote = "Balanced Synergy — As you select your Class, Armor, and Weapons, your proficiencies and quirks will appear here.",
                        matchedRule = null
                    };
                }

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
                title = "Synergy",
                shortStatus = "Balanced Synergy",
                insightNote = "This equipment setup functions naturally within standard 5e rules.",
                matchedRule = null
            };
        }
    }
}
