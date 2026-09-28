using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DNDBeyond.Core;
using DNDBeyond.Data;

namespace DNDBeyond.UI
{
    public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Data")]
        [SerializeField] private CharacterOptionSO optionData;

        [Header("Visual Feedback")]
        [SerializeField] private RectTransform cardTransform;
        [SerializeField] private Image iconImage;

        private static Canvas rootCanvas;
        private GameObject dragGhost;
        private RectTransform ghostRect;
        private bool wasDroppedSuccessfully = false;
        public bool WasDroppedSuccessfully => wasDroppedSuccessfully;
        private Vector3 originalScale = Vector3.one;

        public CharacterOptionSO OptionData => optionData;

        private void Awake()
        {
            if (cardTransform == null) cardTransform = GetComponent<RectTransform>();
            if (iconImage == null) iconImage = GetComponentInChildren<Image>();
            originalScale = cardTransform != null ? cardTransform.localScale : Vector3.one;

            if (rootCanvas == null)
            {
                rootCanvas = GetComponentInParent<Canvas>();
            }
        }

        public void Initialize(CharacterOptionSO data)
        {
            optionData = data;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (cardTransform != null && !eventData.dragging)
            {
                cardTransform.localScale = originalScale * 1.05f;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (cardTransform != null && !eventData.dragging)
            {
                cardTransform.localScale = originalScale;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Click to equip immediately!
            if (optionData != null && CharacterCustomizerManager.Instance != null)
            {
                CharacterCustomizerManager.Instance.SelectOption(optionData);
                StartCoroutine(DoClickPulse());
            }
        }

        private IEnumerator DoClickPulse()
        {
            if (cardTransform == null) yield break;
            cardTransform.localScale = originalScale * 1.15f;
            yield return new WaitForSecondsRealtime(0.08f);
            cardTransform.localScale = originalScale;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            wasDroppedSuccessfully = false;

            if (rootCanvas == null)
            {
                rootCanvas = GetComponentInParent<Canvas>();
            }

            // Create drag ghost
            dragGhost = new GameObject("DragGhost", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            dragGhost.transform.SetParent(rootCanvas.transform, false);
            dragGhost.transform.SetAsLastSibling();

            ghostRect = dragGhost.GetComponent<RectTransform>();
            ghostRect.sizeDelta = new Vector2(80, 80);

            var ghostImage = dragGhost.GetComponent<Image>();
            if (iconImage != null && iconImage.sprite != null)
            {
                ghostImage.sprite = iconImage.sprite;
                ghostImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0.85f);
            }
            else if (optionData != null && optionData.icon != null)
            {
                ghostImage.sprite = optionData.icon;
                ghostImage.color = new Color(1f, 1f, 1f, 0.85f);
            }
            else
            {
                ghostImage.color = new Color(0.9f, 0.7f, 0.2f, 0.8f);
            }

            var canvasGroup = dragGhost.GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;

            UpdateGhostPosition(eventData);

            if (cardTransform != null)
            {
                cardTransform.localScale = originalScale * 0.95f;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragGhost != null)
            {
                UpdateGhostPosition(eventData);
            }
        }

        private void UpdateGhostPosition(PointerEventData eventData)
        {
            if (ghostRect == null || rootCanvas == null) return;

            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.transform as RectTransform,
                eventData.position,
                rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
                out pos);

            ghostRect.anchoredPosition = pos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (CharacterDropZone.Instance != null)
            {
                CharacterDropZone.Instance.SetHighlight(false);
            }

            if (cardTransform != null)
            {
                cardTransform.localScale = originalScale;
            }

            if (dragGhost != null)
            {
                Destroy(dragGhost);
                dragGhost = null;
            }
        }

        public void MarkDroppedSuccess()
        {
            wasDroppedSuccessfully = true;
        }
    }
}
