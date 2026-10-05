using UnityEngine;

namespace DNDBeyond.Data
{
    public enum OptionCategory
    {
        Race,
        Class,
        Armor,
        Weapon
    }

    public enum CharacterRace
    {
        None,
        Elf,
        Tiefling
    }

    public enum CharacterClass
    {
        None,
        Barbarian,
        Bard,
        Cleric,
        Druid,
        Fighter,
        Monk,
        Paladin,
        Ranger,
        Rogue,
        Sorcerer,
        Warlock,
        Wizard
    }

    [CreateAssetMenu(fileName = "NewCharacterOption", menuName = "DND/Character Option")]
    public class CharacterOptionSO : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        public OptionCategory category;

        [Header("Specific Type")]
        public CharacterRace raceType;
        public CharacterClass classType;

        [Header("Visuals (Greybox)")]
        public Sprite icon;
        public Sprite mannequinSprite;
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.white;

        [Header("Lore & Rules")]
        [TextArea(2, 4)]
        public string flavorTagline;
        [TextArea(2, 4)]
        public string description;
    }
}
