using UnityEngine;

namespace DNDBeyond.Data
{
    public enum HarmonyState
    {
        Harmonious,
        DiscoveryQuirk
    }

    [CreateAssetMenu(fileName = "NewHarmonyRule", menuName = "DND/Harmony Rule")]
    public class HarmonyRuleSO : ScriptableObject
    {
        [Header("Rule Identification")]
        public string ruleId;
        public string ruleTitle;
        public HarmonyState harmonyState = HarmonyState.Harmonious;
        public int priority = 10;

        [Header("Class Condition")]
        public bool checkClass = false;
        public CharacterClass requiredClass = CharacterClass.None;

        [Header("Armor Condition")]
        public bool checkArmor = false;
        public ArmorType requiredArmor = ArmorType.None;

        [Header("Weapon Condition")]
        public bool checkWeapon = false;
        public WeaponType requiredWeapon = WeaponType.None;

        [Header("Race Condition")]
        public bool checkRace = false;
        public CharacterRace requiredRace = CharacterRace.None;

        [Header("Rule Insight Output")]
        public string shortStatus = "Harmonious Build";
        [TextArea(3, 6)]
        public string insightNoteText = "These choices synergize according to standard 5e rules.";

        public bool Matches(CharacterRace race, CharacterClass charClass, ArmorType armor, WeaponType weapon)
        {
            if (checkClass && charClass != requiredClass) return false;
            if (checkArmor && armor != requiredArmor) return false;
            if (checkWeapon && weapon != requiredWeapon) return false;
            if (checkRace && race != requiredRace) return false;
            return true;
        }
    }
}
