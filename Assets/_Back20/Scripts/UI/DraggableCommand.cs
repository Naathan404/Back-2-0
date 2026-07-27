using System;
using System.Collections.Generic;
using DG.Tweening;
using MeowgaByte.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MeowgaByte.UI
{
    public class DraggableCommand : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] private CommandType _commandType = CommandType.Instant;
        [SerializeField] private ActionType _actionType = ActionType.RunRight;

        [Header("Information")]
        [SerializeField] private string _actionName = "";
        [SerializeField] private string _description = "this is the default description";
        [SerializeField] private float _duration = 0;

        [Header("UI References")]
        [SerializeField] private List<Sprite> _backgroundList;
        [SerializeField] private List<Sprite> _hoverBackgroundList;
        [SerializeField] private Image _background;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _typeIconImage;
        [SerializeField] private List<IconTypeUI> _iconTypeIcons;
        [SerializeField] private TextMeshProUGUI _durationText;

        [Header("Debug")]
        public TimelineDropZone CurrentHoveredZone;

        [Header("Settings")]
        private Transform _originalParent;
        private int _originalSiblingIndex; 
        private RectTransform _rect;
        private CanvasGroup _canvasGroup;
        private Vector2 _originalScale;
        [SerializeField] private float _scaleAmount = 1.3f;
        [SerializeField] private float _scaleDuration = 0.2f;
        [SerializeField] private float _punchDurationn = 0.3f;
        [SerializeField] private float _returnDurationn = 0.3f;

        [SerializeField] private float _draggingAlpha = 0.6f;
        
        private GameObject _placeholder; 
        
        private bool _isDraggingOrReturning = false;
        private bool _isReturning = false;

        public string ActionName => _actionName;
        public string CmdDescription => _description;
        public CommandType CmdType => _commandType;
        public ActionType Action => _actionType;
        public Sprite Icon => _iconImage != null ? _iconImage.sprite : null;
        public float Duration => _duration;

        private void Awake()
        {
            if (!TryGetComponent(out _rect))
            {
                DebugHandler.LogError(this.name, "Missing RectTransform");
            }
            if (!TryGetComponent(out _canvasGroup))
            {
                DebugHandler.LogError(this.name, "Missing CanvasGroup");
            }            
        }

        public void Init(CommandType commandType, ActionType actionType, Sprite sprite, float duration = 0, string actionName = "", string description = "")
        {
            _commandType = commandType;
            _actionType = actionType;
            _actionName = actionName;
            _description = description;
            _duration = duration;
            
            UpdateUI(commandType, sprite, duration);

            _originalScale = _rect.transform.localScale;
        }

        private void UpdateUI(CommandType commandType, Sprite icon, float duration)
        {
            _iconImage.sprite = icon;
            _durationText.text = duration != 0 ? duration.ToString("N2") : "";
            _background.sprite = _backgroundList[UnityEngine.Random.Range(0, _backgroundList.Count)];

            for(int i = 0; i < _iconTypeIcons.Count; i++)
            {
                if (_iconTypeIcons[i].CmdType == commandType)
                {
                    _typeIconImage.sprite = _iconTypeIcons[i].Icon;
                    break;
                }
            }
        }

        private void UpdateBackgroundUI(bool active)
        {
            if (active)
            {
                _background.sprite = _hoverBackgroundList[UnityEngine.Random.Range(0, _hoverBackgroundList.Count)];
            }
            else
            {
                _background.sprite = _backgroundList[UnityEngine.Random.Range(0, _backgroundList.Count)];
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (GameManager.Instance?.State != GameState.Prepare) return;
            _isDraggingOrReturning = true; 
            TooltipManager.Instance?.HideTooltip();
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.SelectCommandSFX, true);
            
            _rect.DOKill();
            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();
            _rect.DOScale(_originalScale * _scaleAmount, _scaleDuration);
            UpdateBackgroundUI(false);

            _placeholder = new GameObject("Placeholder");
            RectTransform placeholderRect = _placeholder.AddComponent<RectTransform>();
            placeholderRect.SetParent(_originalParent);
            placeholderRect.SetSiblingIndex(_originalSiblingIndex);
            
            LayoutElement le = _placeholder.AddComponent<LayoutElement>();
            le.preferredWidth = _rect.rect.width;
            le.preferredHeight = _rect.rect.height;

            transform.SetParent(transform.root);
            transform.SetAsLastSibling();

            _canvasGroup.alpha = _draggingAlpha;
            _durationText.gameObject.SetActive(false);
            _canvasGroup.blocksRaycasts = false;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (GameManager.Instance?.State != GameState.Prepare) return;
            _rect.position = eventData.position;

            if (CurrentHoveredZone != null)
            {
                CurrentHoveredZone.UpdateGhostEffect(eventData.position, this);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (GameManager.Instance?.State != GameState.Prepare) return;
            _rect.DOKill();
            _isReturning = true;
            bool placed = CurrentHoveredZone != null
                && CurrentHoveredZone.AcceptsCommandType == _commandType
                && CurrentHoveredZone.AcceptCommand(this);

            if (placed)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true; 

                AudioManager.Instance?.PlaySFX(AudioManager.Instance.DropSuccessCommandSFX, true);
                _rect.DOScale(Vector2.zero, _scaleDuration).SetEase(Ease.InOutExpo).OnComplete(() =>
                {
                    if (_placeholder != null) Destroy(_placeholder);
                    Destroy(gameObject);
                });
            }
            else
            {
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.DropFailCommandSFX, true);
                _rect.DOPunchRotation(new Vector3(0, 0, 15), _punchDurationn, 10, 1).OnComplete(() =>
                {
                    _rect.DOScale(_originalScale, _returnDurationn);
                    _rect.DOMove(_placeholder.transform.position, _returnDurationn)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => 
                        {
                            transform.SetParent(_originalParent);
                            transform.SetSiblingIndex(_placeholder.transform.GetSiblingIndex());
                            _rect.DOPunchScale(0.2f * Vector2.one, _punchDurationn);
                            _rect.DOPunchRotation(new Vector3(0, 0, 15), _punchDurationn, 10, 1).OnComplete(() => _isReturning = false );

                            if (_placeholder != null) Destroy(_placeholder);
                            _canvasGroup.alpha = 1f;
                            _durationText.gameObject.SetActive(true);
                            
                            _canvasGroup.blocksRaycasts = true;
                            _isDraggingOrReturning = false; 
                        });
                });
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (GameManager.Instance?.State != GameState.Prepare) return;
            if (_isDraggingOrReturning || _isReturning) return; 

            _rect.DOKill();
            _rect.DOScale(_originalScale * 1.25f, 0.2f);

            string tooltipContent = _description;
            UpdateBackgroundUI(true);
            TooltipManager.Instance?.ShowTooltip(tooltipContent);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (GameManager.Instance?.State != GameState.Prepare) return;
            if (_isDraggingOrReturning || _isReturning) return; 
            
            _rect.DOKill();
            _rect.DOScale(_originalScale, 0.2f);
            UpdateBackgroundUI(false);
            TooltipManager.Instance?.HideTooltip();
        }
    }

    [Serializable]
    public class IconTypeUI
    {
        public CommandType CmdType;
        public Sprite Icon;
    }
}