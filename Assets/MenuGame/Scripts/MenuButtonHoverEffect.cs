using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace MenuGame
{
    [RequireComponent(typeof(RectTransform))]
    public class MenuButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Couleurs")]
        public Color normalColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        public Color hoverColor = new Color(0.82f, 0.06f, 0.06f, 1f);
        public Color pressedColor = new Color(0.55f, 0.02f, 0.02f, 1f);

        [Header("Animation")]
        [Range(0f, 20f)] public float offsetOnHover = 8f;
        [Range(1f, 1.3f)] public float scaleOnHover = 1.05f;
        public float animSpeed = 10f;

        private TMP_Text label;
        private RectTransform rect;
        private Vector2 originalPos;
        private Vector3 targetScale;
        private Vector2 targetPos;
        private Color targetColor;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            label = GetComponentInChildren<TMP_Text>();
            originalPos = rect.anchoredPosition;
            targetPos = originalPos;
            targetScale = Vector3.one;
            targetColor = normalColor;

            if (label != null)
                label.color = normalColor;
        }

        void Update()
        {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.unscaledDeltaTime * animSpeed);
            rect.localScale = Vector3.Lerp(rect.localScale, targetScale, Time.unscaledDeltaTime * animSpeed);

            if (label != null)
                label.color = Color.Lerp(label.color, targetColor, Time.unscaledDeltaTime * animSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetPos = originalPos + new Vector2(offsetOnHover, 0f);
            targetScale = Vector3.one * scaleOnHover;
            targetColor = hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetPos = originalPos;
            targetScale = Vector3.one;
            targetColor = normalColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            targetScale = Vector3.one * 0.97f;
            targetColor = pressedColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            targetScale = Vector3.one * scaleOnHover;
            targetColor = hoverColor;
        }
    }
}
