using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DNDBeyond.Core;
using DNDBeyond.Data;

namespace DNDBeyond.UI
{
    public class CharacterDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public static CharacterDropZone Instance { get; private set; }

        [Header("Drop Feedback Visuals")]
        [SerializeField] private Image highlightBorder;
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.05f);
        [SerializeField] private Color hoverValidColor = new Color(0.3f, 0.85f, 1f, 0.35f);
        private bool isPointerOver = false;
        public bool IsPointerOver => isPointerOver;

        private void Awake()
        {
            Instance = this;
            if (highlightBorder != null)
            {
                highlightBorder.color = normalColor;
            }
        }

        public void SetHighlight(bool active)
        {
            if (highlightBorder != null)
            {
                highlightBorder.color = active ? hoverValidColor : normalColor;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerOver = true;
            if (eventData.dragging)
            {
                SetHighlight(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;
            SetHighlight(false);
        }

        public void OnDrop(PointerEventData eventData)
        {
            SetHighlight(false);
            if (eventData.pointerDrag != null)
            {
                var dragHandler = eventData.pointerDrag.GetComponent<ItemDragHandler>();
                if (dragHandler != null && dragHandler.OptionData != null)
                {
                    dragHandler.MarkDroppedSuccess();
                    CharacterCustomizerManager.Instance.SelectOption(dragHandler.OptionData);
                }
            }
        }
    }
}
