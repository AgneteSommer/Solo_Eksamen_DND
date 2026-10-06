using UnityEngine;

namespace DNDBeyond.Data
{
    public enum ArmorType
    {
        None,       // Unarmored / Robes
        Light,      // Leather
        Medium,     // Hide / Scale
        Heavy       // Plate
    }

    public enum WeaponType
    {
        None,
        Greataxe,
        ArcaneStaff,
        Dagger,
        GreatSword,
        LongBow,
        Staff
    }

    [CreateAssetMenu(fileName = "NewEquipment", menuName = "DND/Equipment")]
    public class EquipmentSO : CharacterOptionSO
    {
        [Header("Armor Specifics")]
        public ArmorType armorType = ArmorType.None;
        public int baseAC = 10;
        public bool stealthDisadvantage = false;
        public bool requiresProficiency = true;

        [Header("Weapon Specifics")]
        public WeaponType weaponType = WeaponType.None;
        public string damage = "1d4";
        public string damageType = "Bludgeoning";
        public string weaponProperties = "Simple";
        public bool isArcaneFocus = false;
    }
}
