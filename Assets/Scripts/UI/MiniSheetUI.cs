using UnityEngine;
using TMPro;
using DNDBeyond.Core;

namespace DNDBeyond.UI
{
    public class MiniSheetUI : MonoBehaviour
    {
        [Header("Mini Sheet Chips (Top-Right)")]
        [SerializeField] private TextMeshProUGUI speciesChipText;
        [SerializeField] private TextMeshProUGUI classChipText;
        [SerializeField] private TextMeshProUGUI armorChipText;
        [SerializeField] private TextMeshProUGUI weaponChipText;
        [SerializeField] private TextMeshProUGUI totalAcText;

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

            if (speciesChipText != null)
            {
                speciesChipText.text = build.currentRace != null 
                    ? $"[ Species: {build.currentRace.displayName} ]" 
                    : "[ Species: Select ]";
            }

            if (classChipText != null)
            {
                classChipText.text = build.currentClass != null 
                    ? $"[ Class: {build.currentClass.displayName} ]" 
                    : "[ Class: Select ]";
            }

            if (armorChipText != null)
            {
                int ac = build.CalculateAC();
                if (build.currentArmor != null)
                {
                    armorChipText.text = $"[ Armor: {build.currentArmor.displayName} (AC {ac}) ]";
                }
                else
                {
                    armorChipText.text = $"[ Armor: Unarmored (AC {ac}) ]";
                }
            }

            if (weaponChipText != null)
            {
                if (build.currentWeapon != null)
                {
                    weaponChipText.text = $"[ Weapon: {build.currentWeapon.displayName} ({build.GetWeaponDamageSummary()}) ]";
                }
                else
                {
                    weaponChipText.text = "[ Weapon: Unarmed (1 Bludgeoning) ]";
                }
            }

            if (totalAcText != null)
            {
                totalAcText.text = $"ARMOR CLASS: {build.CalculateAC()}";
            }
        }
    }
}
