using System;
using UnityEngine;
using DNDBeyond.Data;

namespace DNDBeyond.Core
{
    [Serializable]
    public class CharacterBuild
    {
        public CharacterOptionSO currentRace;
        public CharacterOptionSO currentClass;
        public EquipmentSO currentArmor;
        public EquipmentSO currentWeapon;

        public CharacterRace RaceType => currentRace != null ? currentRace.raceType : CharacterRace.None;
        public CharacterClass ClassType => currentClass != null ? currentClass.classType : CharacterClass.None;
        public ArmorType CurrentArmorType => currentArmor != null ? currentArmor.armorType : ArmorType.None;
        public WeaponType CurrentWeaponType => currentWeapon != null ? currentWeapon.weaponType : WeaponType.None;

        public string RaceName => currentRace != null ? currentRace.displayName : "None";
        public string ClassName => currentClass != null ? currentClass.displayName : "None";
        public string ArmorName => currentArmor != null ? currentArmor.displayName : "Unarmored";
        public string WeaponName => currentWeapon != null ? currentWeapon.displayName : "Unarmed";

        public int CalculateAC()
        {
            int dexMod = 2; // Representative +2 DEX
            int conMod = 3; // Representative +3 CON (for Barbarian)

            if (currentArmor == null || currentArmor.armorType == ArmorType.None)
            {
                if (currentClass == null && currentRace == null)
                {
                    return 10; // Base AC before class/race selection
                }

                // Unarmored Defense for Barbarian: 10 + DEX + CON
                if (ClassType == CharacterClass.Barbarian)
                {
                    return 10 + dexMod + conMod; // 15
                }
                return 10 + dexMod; // 12
            }

            switch (currentArmor.armorType)
            {
                case ArmorType.Light:
                    return currentArmor.baseAC + dexMod; // 11 + 2 = 13 or 12 + 2 = 14
                case ArmorType.Medium:
                    return currentArmor.baseAC + Mathf.Min(dexMod, 2); // 14 + 2 = 16
                case ArmorType.Heavy:
                    return currentArmor.baseAC; // 16 or 18
                default:
                    return 10;
            }
        }

        public string GetWeaponDamageSummary()
        {
            if (currentWeapon == null) return "1 Bludgeoning";
            return $"{currentWeapon.damage} {currentWeapon.damageType}";
        }
    }
}
