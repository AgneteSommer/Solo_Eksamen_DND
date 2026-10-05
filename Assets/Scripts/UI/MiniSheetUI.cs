using UnityEngine;
using TMPro;
using DNDBeyond.Core;

namespace DNDBeyond.UI
{
    public class MiniSheetUI : MonoBehaviour
    {
        [Header("Mini Sheet Chips")]
        [SerializeField] private TextMeshProUGUI speciesChipText;
        [SerializeField] private TextMeshProUGUI classChipText;
        [SerializeField] private TextMeshProUGUI armorChipText;
        [SerializeField] private TextMeshProUGUI weaponChipText;
        [SerializeField] private TextMeshProUGUI totalAcText;
        [SerializeField] private TextMeshProUGUI adventureReadyText;

        private void Start()
        {
            if (CharacterCustomizerManager.Instance != null)
            {
                CharacterCustomizerManager.Instance.OnCharacterUpdated += UpdateChips;
                UpdateChips(CharacterCustomizerManager.Instance.CurrentBuild);
            }
        }

        private void OnDestroy()
        {
            if (CharacterCustomizerManager.Instance != null)
            {
                CharacterCustomizerManager.Instance.OnCharacterUpdated -= UpdateChips;
            }
        }

        public void UpdateChips(CharacterBuild build)
        {
            if (build == null) return;

            bool hasSpecies = build.currentRace != null;
            bool hasClass = build.currentClass != null;
            bool hasArmor = build.currentArmor != null;
            bool hasWeapon = build.currentWeapon != null;

            int readyCount = (hasSpecies ? 1 : 0) + (hasClass ? 1 : 0) + (hasArmor ? 1 : 0) + (hasWeapon ? 1 : 0);

            if (speciesChipText != null)
            {
                speciesChipText.text = hasSpecies 
                    ? $"<b><color=#1E7E34>[✓]</color> <color=#242527>Species:</color></b> {build.currentRace.displayName}" 
                    : "<color=#718096>[ ] Species: (None chosen)</color>";
            }

            if (classChipText != null)
            {
                classChipText.text = hasClass 
                    ? $"<b><color=#1E7E34>[✓]</color> <color=#242527>Class:</color></b> {build.currentClass.displayName}" 
                    : "<color=#718096>[ ] Class: (None chosen)</color>";
            }

            if (armorChipText != null)
            {
                int ac = build.CalculateAC();
                if (hasArmor)
                {
                    armorChipText.text = $"<b><color=#1E7E34>[✓]</color> <color=#242527>Armor:</color></b> {build.currentArmor.displayName} (AC {ac})";
                }
                else
                {
                    armorChipText.text = "<color=#718096>[ ] Armor: (None chosen)</color>";
                }
            }

            if (weaponChipText != null)
            {
                if (hasWeapon)
                {
                    weaponChipText.text = $"<b><color=#1E7E34>[✓]</color> <color=#242527>Weapon:</color></b> {build.currentWeapon.displayName} ({build.GetWeaponDamageSummary()})";
                }
                else
                {
                    weaponChipText.text = "<color=#718096>[ ] Weapon: (None chosen)</color>";
                }
            }

            if (totalAcText != null)
            {
                int ac = build.CalculateAC();
                string acSuffix = (!hasArmor && !hasClass) ? "10 (Base)" : ac.ToString();
                totalAcText.text = $"<b><color=#B7791F>ARMOR CLASS: {acSuffix}</color></b>";
            }

            if (adventureReadyText != null)
            {
                if (readyCount == 4)
                {
                    adventureReadyText.text = "<b><color=#B45309>⭐ Adventure Ready! (4/4)</color></b>";
                }
                else
                {
                    adventureReadyText.text = $"<color=#718096>Quest Checklist: ({readyCount}/4 Complete)</color>";
                }
            }
        }
    }
}
