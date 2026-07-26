using UnityEngine;
using UnityEngine.UI;

namespace MeowgaByte.UI
{
    public class CommandBlockView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage; 

        [Header("Nested Style (Wait lồng trong Run)")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _nestedColor = new Color(1f, 0.85f, 0.2f); 
        [SerializeField] private float _normalOutlineWidth = 0f;
        [SerializeField] private Outline _outline;

        private void Awake()
        {
            if (_backgroundImage == null)
            {
                _backgroundImage = GetComponent<Image>();
            }
        }

        public void SetIcon(Sprite icon, float x, float y)
        {
            if (_iconImage == null || icon == null) return;
            _iconImage.sprite = icon;
            _iconImage.rectTransform.sizeDelta = new Vector2(x, y);
        }

        public void SetNestedStyle(bool isNested)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = isNested ? _nestedColor : _normalColor;
            }

            if (_outline != null)
            {
                _outline.enabled = isNested;
            }
        }
    }
}