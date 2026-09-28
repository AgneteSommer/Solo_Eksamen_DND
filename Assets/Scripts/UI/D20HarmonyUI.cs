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
        [SerializeField] private Color harmoniousGlow = new Color(0.95f, 0.78f, 0.2f, 0.85f);
        [SerializeField] private Color harmoniousBg = new Color(0.25f, 0.22f, 0.08f, 0.95f);
        [SerializeField] private Color quirkGlow = new Color(1.0f, 0.45f, 0.15f, 0.95f);
        [SerializeField] private Color quirkBg = new Color(0.35f, 0.12f, 0.08f, 0.95f);

        private bool isNoteCardOpen = true;
        private Coroutine pulseCoroutine;
        private Coroutine slideCoroutine;
        private HarmonyEvaluationResult lastResult;

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

            // Start open
            SetNoteCardVisible(true, immediate: true);
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
                d20StatusText.text = isQuirk ? "RULE QUIRK" : "HARMONIOUS";
                d20StatusText.color = isQuirk ? new Color(1f, 0.6f, 0.2f) : new Color(1f, 0.9f, 0.4f);
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
                noteTagText.text = isQuirk ? "[ 5e RULE RESTRICTION ]" : "[ 5e SYNERGY ]";
                noteTagText.color = isQuirk ? new Color(1f, 0.5f, 0.2f) : new Color(0.4f, 0.9f, 0.4f);
            }

            // Animate D20 pulse
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(DoD20Pulse(isQuirk));

            // Automatically open note card to draw attention to insights/quirks!
            SetNoteCardVisible(true);
        }

        public void ToggleNoteCard()
        {
            SetNoteCardVisible(!isNoteCardOpen);
        }

        public void SetNoteCardVisible(bool visible, bool immediate = false)
        {
            isNoteCardOpen = visible;

            if (slideCoroutine != null) StopCoroutine(slideCoroutine);

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
                    noteCardPanel.anchoredPosition = visible ? new Vector2(0, -90) : new Vector2(-400, -90);
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

            Vector2 startPos = noteCardPanel.anchoredPosition;
            Vector2 targetPos = visible ? new Vector2(0, -90) : new Vector2(-400, -90);

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
