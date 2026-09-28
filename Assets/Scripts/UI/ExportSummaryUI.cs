using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DNDBeyond.Core;

namespace DNDBeyond.UI
{
    public class ExportSummaryUI : MonoBehaviour
    {
        [Header("Export Trigger")]
        [SerializeField] private Button openExportButton;

        [Header("Modal Panel")]
        [SerializeField] private GameObject modalOverlay;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI characterDetailsText;
        [SerializeField] private TextMeshProUGUI ruleHarmonyText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button confirmExportButton;
        [SerializeField] private TextMeshProUGUI feedbackText;

        private void Start()
        {
            if (openExportButton != null)
            {
                openExportButton.onClick.AddListener(OpenModal);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseModal);
            }

            if (confirmExportButton != null)
            {
                confirmExportButton.onClick.AddListener(HandleConfirmExport);
            }

            if (modalOverlay != null)
            {
                modalOverlay.SetActive(false);
            }
        }

        public void OpenModal()
        {
            if (modalOverlay == null || CharacterCustomizerManager.Instance == null) return;

            var build = CharacterCustomizerManager.Instance.CurrentBuild;
            int ac = build.CalculateAC();

            if (headerText != null)
            {
                headerText.text = $"D&D BEYOND EXPORT: {build.RaceName} {build.ClassName}";
            }

            if (characterDetailsText != null)
            {
                characterDetailsText.text = 
                    $"<b>Species:</b> {build.RaceName}\n" +
                    $"<b>Class:</b> {build.ClassName}\n" +
                    $"<b>Armor:</b> {build.ArmorName} (AC {ac})\n" +
                    $"<b>Weapon:</b> {build.WeaponName} ({build.GetWeaponDamageSummary()})\n" +
                    $"<b>Stealth:</b> {(build.currentArmor != null && build.currentArmor.stealthDisadvantage ? "Disadvantage" : "Normal")}";
            }

            if (ruleHarmonyText != null)
            {
                var evaluator = FindAnyObjectByType<RuleHarmonyEvaluator>();
                if (evaluator != null)
                {
                    var result = evaluator.Evaluate(build);
                    string statusColor = result.IsQuirk ? "#FF8844" : "#FFD700";
                    ruleHarmonyText.text = $"<color={statusColor}><b>[{result.shortStatus}]</b></color>\n{result.insightNote}";
                }
            }

            if (feedbackText != null)
            {
                feedbackText.text = "";
            }

            modalOverlay.SetActive(true);
        }

        public void CloseModal()
        {
            if (modalOverlay != null)
            {
                modalOverlay.SetActive(false);
            }
        }

        private void HandleConfirmExport()
        {
            if (feedbackText != null)
            {
                feedbackText.text = "<color=#44FF88>✓ Character build exported to D&D Beyond profile (Greybox Mock)!</color>";
            }
        }
    }
}
