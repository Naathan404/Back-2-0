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
        [SerializeField] private string _actionName = "";
        [SerializeField] private string _description = "this is the default description";
        [SerializeField] private float _duration = 0;
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _durationText;
        public TimelineDropZone CurrentHoveredZone;

        [Header("Settings")]
        private Transform _originalParent;
        private int _originalSiblingIndex; 
        private RectTransform _rect;
        private CanvasGroup _canvasGroup;
        private Vector2 _originalScale;
        private float _scaleAmount = 1.3f;
        private float _scaleDuration = 0.2f;
        private float _punchDurationn = 0.3f;
        private float _returnDurationn = 0.3f;

        private float _draggingAlpha = 0.8f;
        
        private GameObject _placeholder; 
        
        private bool _isDraggingOrReturning = false;
        private bool _isReturning = false;

        public string ActionName => _actionName;
        public string CmdDescription => _description;
        public CommandType CmdType => _commandType;
        public ActionType Action => _actionType;
        public Sprite Icon => _image != null ? _image.sprite : null;
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
            _image.sprite = sprite;
            _durationText.text = duration != 0 ? duration.ToString("N2") : "";

            _originalScale = _rect.transform.localScale;
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _isDraggingOrReturning = true; 
            TooltipManager.Instance?.HideTooltip();
            
            _rect.DOKill();
            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();
            _rect.DOScale(_originalScale * _scaleAmount, _scaleDuration);

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
            _rect.position = eventData.position;

            if (CurrentHoveredZone != null)
            {
                CurrentHoveredZone.UpdateGhostEffect(eventData.position, this);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            _rect.DOKill();
            _isReturning = true;
            bool placed = CurrentHoveredZone != null
                && CurrentHoveredZone.AcceptsCommandType == _commandType
                && CurrentHoveredZone.AcceptCommand(this);

            if (placed)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true; 

                _rect.DOScale(Vector2.zero, _scaleDuration).SetEase(Ease.InOutExpo).OnComplete(() =>
                {
                    if (_placeholder != null) Destroy(_placeholder);
                    Destroy(gameObject);
                });
            }
            else
            {
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
            if (_isDraggingOrReturning || _isReturning) return; 

            _rect.DOKill();
            _rect.DOScale(_originalScale * 1.25f, 0.2f);

            string tooltipContent = _description;
            TooltipManager.Instance?.ShowTooltip(tooltipContent);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isDraggingOrReturning || _isReturning) return; 
            
            _rect.DOKill();
            _rect.DOScale(_originalScale, 0.2f);

            TooltipManager.Instance?.HideTooltip();
        }
    }
}