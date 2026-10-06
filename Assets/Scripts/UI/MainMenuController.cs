using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace DNDBeyond.UI
{
    public class MainMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Scene Navigation")]
        [SerializeField] private string targetSceneName = "SampleScene";
        [SerializeField] private int targetSceneIndex = 1;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clickSound;

        [Header("Visual Feedback")]
        [SerializeField] private RectTransform buttonTransform;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Image overlayCardImage;
        [SerializeField] private float hoverScale = 1.018f;
        [SerializeField] private float clickScale = 0.985f;
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 0.08f);
        [SerializeField] private Color normalColor = new Color(0f, 0f, 0f, 0f);

        private Vector3 defaultScale = Vector3.one;
        private Coroutine scaleCoroutine;
        private bool isTransitioning = false;

        private void Awake()
        {
            if (buttonTransform == null) buttonTransform = GetComponent<RectTransform>();
            if (buttonImage == null) buttonImage = GetComponent<Image>();
            if (buttonTransform != null) defaultScale = buttonTransform.localScale;

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;

            var btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(OnButtonClicked);
            }
        }

        public void Setup(AudioClip clip, string sceneName = "SampleScene", int sceneIndex = 1, Image overlay = null)
        {
            clickSound = clip;
            targetSceneName = sceneName;
            targetSceneIndex = sceneIndex;
            if (overlay != null) overlayCardImage = overlay;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isTransitioning) return;
            AnimateScale(defaultScale * hoverScale, 0.12f);
            if (buttonImage != null) buttonImage.color = hoverColor;
            if (overlayCardImage != null) overlayCardImage.color = new Color(1.08f, 1.08f, 1.08f, 1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isTransitioning) return;
            AnimateScale(defaultScale, 0.12f);
            if (buttonImage != null) buttonImage.color = normalColor;
            if (overlayCardImage != null) overlayCardImage.color = Color.white;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isTransitioning) return;
            AnimateScale(defaultScale * clickScale, 0.08f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isTransitioning) return;
            AnimateScale(defaultScale * hoverScale, 0.08f);
        }

        public void OnButtonClicked()
        {
            if (isTransitioning) return;
            StartCoroutine(DoTransition());
        }

        private IEnumerator DoTransition()
        {
            isTransitioning = true;

            if (clickSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(clickSound);
            }

            // Snappy feedback before loading
            yield return new WaitForSecondsRealtime(0.12f);

            if (!string.IsNullOrEmpty(targetSceneName) && Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                SceneManager.LoadScene(targetSceneIndex);
            }
        }

        private void AnimateScale(Vector3 target, float duration)
        {
            if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
            scaleCoroutine = StartCoroutine(ScaleRoutine(target, duration));
        }

        private IEnumerator ScaleRoutine(Vector3 target, float duration)
        {
            if (buttonTransform == null) yield break;
            Vector3 start = buttonTransform.localScale;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                buttonTransform.localScale = Vector3.Lerp(start, target, elapsed / duration);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            buttonTransform.localScale = target;
        }
    }
}
