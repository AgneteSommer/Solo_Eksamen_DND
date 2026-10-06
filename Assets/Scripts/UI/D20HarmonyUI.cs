using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DNDBeyond.Core;
using DNDBeyond.Data;

namespace DNDBeyond.UI
{
    public class D20HarmonyUI : MonoBehaviour
    {
        [Header("D20 Indicator (Top-Left)")]
        [SerializeField] private Button d20Button;
        [SerializeField] private Image d20GlowImage;
        [SerializeField] private Image d20IconImage;
        [SerializeField] private TextMeshProUGUI d20StatusText;
        [SerializeField] private Image quirkBadgeImage;

        [Header("Note / Rule Insight Card")]
        [SerializeField] private RectTransform noteCardPanel;
        [SerializeField] private CanvasGroup noteCardCanvasGroup;
        [SerializeField] private TextMeshProUGUI noteTitleText;
        [SerializeField] private TextMeshProUGUI noteBodyText;
        [SerializeField] private TextMeshProUGUI noteTagText;
        [SerializeField] private Button noteCloseButton;

        [Header("Color Schemes")]
        [SerializeField] private Color harmoniousGlow = new Color(0.20f, 0.65f, 0.30f, 0.85f);
        [SerializeField] private Color harmoniousBg = Color.white;
        [SerializeField] private Color quirkGlow = new Color(0.85f, 0.22f, 0.22f, 0.95f);
        [SerializeField] private Color quirkBg = Color.white;

        private bool isNoteCardOpen = false;
        private Coroutine pulseCoroutine;
        private Coroutine slideCoroutine;
        private HarmonyEvaluationResult lastResult;

        private void Awake()
        {
            // Start closed and inactive on clean slate before any events fire
            SetNoteCardVisible(false, immediate: true);
        }

        private void Start()
        {
            if (d20Button != null)
            {
                d20Button.onClick.AddListener(ToggleNoteCard);
            }

            if (noteCloseButton != null)
            {
                noteCloseButton.onClick.AddListener(() => SetNoteCardVisible(false));
            }

            if (CharacterCustomizerManager.Instance != null)
            {
                CharacterCustomizerManager.Instance.OnHarmonyEvaluated += HandleHarmonyEvaluated;
            }

            // Confirm closed on clean slate
            SetNoteCardVisible(false, immediate: true);
        }

        private void OnDestroy()
        {
            if (CharacterCustomizerManager.Instance != null)
            {
                CharacterCustomizerManager.Instance.OnHarmonyEvaluated -= HandleHarmonyEvaluated;
            }
        }

        public void HandleHarmonyEvaluated(HarmonyEvaluationResult result)
        {
            if (result == null) return;
            lastResult = result;

            bool isQuirk = result.IsQuirk;

            // Update D20 colors & labels
            if (d20GlowImage != null)
            {
                d20GlowImage.color = isQuirk ? quirkGlow : harmoniousGlow;
            }

            if (d20StatusText != null)
            {
                if (isQuirk)
                {
                    d20StatusText.text = "<b><color=#B71C1C>SYNERGY</color></b>\n<size=12><color=#666666>Rule Quirk Active (Click)</color></size>";
                }
                else
                {
                    string status = !string.IsNullOrEmpty(result.shortStatus) ? result.shortStatus : "Balanced Synergy";
                    d20StatusText.text = $"<b><color=#181818>SYNERGY</color></b>\n<size=12><color=#2E7D32>{status}</color></size>";
                }
            }

            if (quirkBadgeImage != null)
            {
                quirkBadgeImage.gameObject.SetActive(isQuirk);
            }

            // Update Note Card content
            if (noteTitleText != null)
            {
                noteTitleText.text = result.title;
            }

            if (noteBodyText != null)
            {
                noteBodyText.text = result.insightNote;
            }

            if (noteTagText != null)
            {
                noteTagText.text = isQuirk ? "[ RULE QUIRK ]" : "[ SYNERGY INSIGHT ]";
                noteTagText.color = isQuirk ? new Color(0.77f, 0.12f, 0.12f) : new Color(0.18f, 0.55f, 0.25f);
            }

            // Animate D20 pulse
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(DoD20Pulse(isQuirk));

            // Automatically slide open note card when a quirk is discovered!
            if (isQuirk)
            {
                SetNoteCardVisible(true);
            }
        }

        public void ToggleNoteCard()
        {
            SetNoteCardVisible(!isNoteCardOpen);
        }

        public void SetNoteCardVisible(bool visible, bool immediate = false)
        {
            isNoteCardOpen = visible;

            if (slideCoroutine != null) StopCoroutine(slideCoroutine);

            if (noteCardPanel != null && visible)
            {
                noteCardPanel.gameObject.SetActive(true);
            }

            if (immediate)
            {
                if (noteCardCanvasGroup != null)
                {
                    noteCardCanvasGroup.alpha = visible ? 1f : 0f;
                    noteCardCanvasGroup.interactable = visible;
                    noteCardCanvasGroup.blocksRaycasts = visible;
                }
                if (noteCardPanel != null)
                {
                    noteCardPanel.anchoredPosition = visible ? new Vector2(0, -95) : new Vector2(-430, -95);
                    if (!visible)
                    {
                        noteCardPanel.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                slideCoroutine = StartCoroutine(DoSlideAnimation(visible));
            }
        }

        private IEnumerator DoSlideAnimation(bool visible)
        {
            if (noteCardPanel == null || noteCardCanvasGroup == null) yield break;

            if (visible)
            {
                noteCardPanel.gameObject.SetActive(true);
            }

            Vector2 startPos = noteCardPanel.anchoredPosition;
            Vector2 targetPos = visible ? new Vector2(0, -95) : new Vector2(-430, -95);

            float startAlpha = noteCardCanvasGroup.alpha;
            float targetAlpha = visible ? 1f : 0f;

            float elapsed = 0f;
            float duration = 0.22f;

            noteCardCanvasGroup.interactable = visible;
            noteCardCanvasGroup.blocksRaycasts = visible;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                t = Mathf.SmoothStep(0f, 1f, t);
                noteCardPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                noteCardCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            noteCardPanel.anchoredPosition = targetPos;
            noteCardCanvasGroup.alpha = targetAlpha;

            if (!visible && noteCardPanel != null)
            {
                noteCardPanel.gameObject.SetActive(false);
            }
        }

        private IEnumerator DoD20Pulse(bool isQuirk)
        {
            if (d20Button == null) yield break;
            Transform tr = d20Button.transform;
            Vector3 originalScale = Vector3.one;
            Vector3 peakScale = isQuirk ? Vector3.one * 1.25f : Vector3.one * 1.12f;

            float elapsed = 0f;
            float duration = 0.12f;
            while (elapsed < duration)
            {
                tr.localScale = Vector3.Lerp(originalScale, peakScale, elapsed / duration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            elapsed = 0f;
            duration = 0.15f;
            while (elapsed < duration)
            {
                tr.localScale = Vector3.Lerp(peakScale, originalScale, elapsed / duration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            tr.localScale = originalScale;
        }
    }
}
