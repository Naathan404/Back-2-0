using UnityEngine;
using TMPro;
using DG.Tweening;
using MeowgaByte.Core;
using UnityEngine.UI;

namespace MeowgaByte.UI
{
    public class TooltipManager : MonoSingleton<TooltipManager>
    {
        [Header("References")]
        [SerializeField] private RectTransform _tooltipRect;
        [SerializeField] private TextMeshProUGUI _tooltipText;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Settings")]
        [SerializeField] private Vector2 _offset = new Vector2(15f, -15f);
        private void Start()
        {
            _canvasGroup.alpha = 0f;
            _tooltipRect.gameObject.SetActive(false);
        }

        private void Update()
        {
            // Nếu tooltip đang bật thì cho nó đi theo chuột
            if (_tooltipRect.gameObject.activeSelf)
            {
                _tooltipRect.position = Input.mousePosition + (Vector3)_offset;
            }
        }

        public void ShowTooltip(string content)
        {
            _tooltipText.text = content;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);

            _tooltipRect.gameObject.SetActive(true);
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(1f, 0.15f).SetEase(Ease.OutQuad);
        }

        public void HideTooltip()
        {
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(0f, 0.1f).SetEase(Ease.InQuad).OnComplete(() => 
            {
                _tooltipRect.gameObject.SetActive(false);
            });
        }
    }
}