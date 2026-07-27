using DG.Tweening;
using MeowgaByte.Core;
using MeowgaByte.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MeowgaByte.UI
{
    public class TimelineDropZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        [SerializeField] private TimelineManager _timeline;
        [SerializeField] private CommandType _acceptsCommandType;

        [Header("Snapping Settings")]
        [SerializeField] private float _snapInterval = 0.5f;

        [Header("Timeline Visual")]
        [SerializeField] private GameObject _tickMarkPrefab;
        [SerializeField] private Transform _tickMarkContainer;

        [Header("Ghost Effect")]
        [SerializeField] private RectTransform _ghostImage;
        [SerializeField] private int _sizeDeltaX = 100;
        [SerializeField] private int _sizeDeltaY = 100;
        [SerializeField] private float _iconScaleAmount = 1.25f;

        [Header("Overlap Feedback")]
        [SerializeField] private Color _validColor = Color.white;
        [SerializeField] private Color _nestedValidColor = new Color(1f, 0.85f, 0.2f); // vàng
        [SerializeField] private Color _invalidColor = new Color(1f, 0.3f, 0.3f);


        [Header("Instantiation")]
        [SerializeField] private GameObject _commandBlockPrefab;

        [Header("Timeline Settings")]
        [SerializeField] private float _pixelPerSecond = 100f;


        public CommandType AcceptsCommandType => _acceptsCommandType;

        private GameManager _gameManager => GameManager.Instance;

        private RectTransform _rectTransform;
        private Image _ghostRenderer;
        private bool _isCurrentPlacementValid = true;
        private bool _isGhostExpanded = false;
        private float _lastSnappedTime;
        private Vector2 _lastTargetSize;

        private void Awake()
        {
            if (!TryGetComponent(out _rectTransform))
            {
                DebugHandler.LogError(this.name, "Missing RectTransform");
            }
            _ghostImage.gameObject.SetActive(false);
            _ghostRenderer = _ghostImage.GetComponent<Image>();
        }

        private void Start()
        {
            _pixelPerSecond = _rectTransform.rect.width / _gameManager.LevelTime;
            _snapInterval = _gameManager.SnapInterval;

            DrawTimelineTick();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var draggable = eventData.pointerDrag?.GetComponent<DraggableCommand>();
            if (draggable != null)
            {
                draggable.CurrentHoveredZone = this;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var draggable = eventData.pointerDrag?.GetComponent<DraggableCommand>();
            if (draggable != null && draggable.CurrentHoveredZone == this)
            {
                draggable.CurrentHoveredZone = null;
                _ghostImage.gameObject.SetActive(false);
            }
        }


        public void UpdateGhostEffect(Vector2 mousePosition, DraggableCommand command)
        {
            if (command.CmdType != AcceptsCommandType) 
            {
                _ghostImage.gameObject.SetActive(false);
                return;
            }

            if (!_ghostImage.gameObject.activeSelf)
            {
                _ghostImage.gameObject.SetActive(true);
                _ghostImage.sizeDelta = new Vector2(_sizeDeltaX, _sizeDeltaY); 
                _isGhostExpanded = false;

                _lastTargetSize = new Vector2(_sizeDeltaX, _sizeDeltaY); 
            }

            _ghostImage.SetAsLastSibling();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, mousePosition, null, out Vector2 localPoint);
            float distanceFromLeft = localPoint.x - _rectTransform.rect.xMin;

            float timeHovered = distanceFromLeft / _pixelPerSecond;
            float snappedTime = Mathf.Round(timeHovered / _snapInterval) * _snapInterval;
            snappedTime = Mathf.Clamp(snappedTime, 0f, _gameManager.LevelTime);

            // ============ chặn biên ===========
            // float maxTime = _gameManager.LevelTime; 
            // float minTime = 0f;

            // if (command.CmdType == CommandType.Duration)
            // {
            //     minTime = command.Duration; 
            // }

            // snappedTime = Mathf.Clamp(snappedTime, minTime, maxTime);
            _isCurrentPlacementValid = true;
            bool isNested = false;
            if (command.CmdType == CommandType.Duration)
            {
                float clamped = _timeline.ClampToFreeGap(
                    command.CmdType, command.Action, snappedTime, command.Duration, _gameManager.LevelTime);

                if (float.IsNaN(clamped)) _isCurrentPlacementValid = false;
                else
                {
                    snappedTime = clamped;
                    isNested = command.Action == ActionType.Wait && _timeline.IsNestedInsideRun(snappedTime, command.Duration);
                }
            }
            _ghostRenderer.color = !_isCurrentPlacementValid ? _invalidColor
                : isNested ? _nestedValidColor
                : _validColor;
            // =====================

            _lastSnappedTime = snappedTime;
            float snappedLocalX = TimeToLocalX(snappedTime);

            if (command.CmdType == CommandType.Duration)
            {
                _ghostImage.pivot = new Vector2(1f, 0.5f); 
            }
            else
            {
                _ghostImage.pivot = new Vector2(0.5f, 0.5f);
            }

            _ghostImage.DOAnchorPosX(snappedLocalX, 0.05f).SetEase(Ease.OutCubic);

            if (command.CmdType == CommandType.Duration && !_isGhostExpanded)
            {
                _isGhostExpanded = true;
                // Sử dụng Width động thay vì Width cứng
                float widthPixels = command.Duration * _pixelPerSecond;
                _lastTargetSize = new Vector2(widthPixels, _sizeDeltaY); 
                _ghostImage.DOSizeDelta(new Vector2(widthPixels, _ghostImage.sizeDelta.y), 0.2f)
                    .SetEase(Ease.OutBack);
            }
        }

        public bool AcceptCommand(DraggableCommand command)
        {
            float startTime = _lastSnappedTime;

            if (!_isCurrentPlacementValid ||
                _timeline.HasOverlap(command.CmdType, command.Action, startTime, command.Duration))
            {
                _ghostImage.gameObject.SetActive(false);
                return false;
            }

            float droppedX = TimeToLocalX(startTime);
            Debug.Log($"Đã gắn lệnh {command.Action} vào mốc {startTime}s trên timeline {AcceptsCommandType}");

            GameObject newBlock = Instantiate(_commandBlockPrefab, this.transform);
            RectTransform blockRect = newBlock.GetComponent<RectTransform>();

            blockRect.pivot = _ghostImage.pivot;
            blockRect.anchoredPosition = new Vector2(droppedX, 0); 
            blockRect.sizeDelta = _lastTargetSize;

            if (newBlock.TryGetComponent(out CommandBlockView blockView))
            {
                blockView.SetIcon(command.Icon, _sizeDeltaX * _iconScaleAmount, _sizeDeltaY * _iconScaleAmount);
                blockView.SetData(startTime, command.CmdType);
                blockView.SetGhostSprite(command.CmdType);
                bool isNested = command.Action == ActionType.Wait
                    && _timeline.IsNestedInsideRun(startTime, command.Duration);
                blockView.SetNestedStyle(isNested);
            }

            _timeline.TryAddCommandNode(startTime, command.Action, command.CmdType, command.Duration, command.ActionName);
            
            _ghostImage.gameObject.SetActive(false);
            return true;
        }

        private void DrawTimelineTick()
        {
            if (_tickMarkPrefab == null) return;

            float levelTime = _gameManager.LevelTime;

            for (float t = 0; t <= levelTime; t += _snapInterval)
            {
                GameObject tick = Instantiate(_tickMarkPrefab, _tickMarkContainer);
                if (tick.TryGetComponent<RectTransform>(out RectTransform tickRect))
                {
                    tickRect.anchorMin = new Vector2(0.5f, 0.5f);
                    tickRect.anchorMax = new Vector2(0.5f, 0.5f);
                    tickRect.pivot = new Vector2(0.5f, 0.5f);

                    tickRect.anchoredPosition = new Vector2(TimeToLocalX(t), 0);
                }
            }
        }

        /// <summary>
        /// Chuyển đổi một mốc thời gian (giây, tính từ mép trái = 0) thành
        /// vị trí X cục bộ (anchoredPosition.x) trên Timeline này.
        /// Dùng chung cho tick mark, ghost preview và Playhead lúc Play.
        /// </summary>
        public float TimeToLocalX(float timeValue)
        {
            return _rectTransform.rect.xMin + timeValue * _pixelPerSecond;
        }

        public float TimeToLocalXWithOffset(float timeValue)
        {
            float offset = 0;
            if (timeValue <= 0) offset = 25;

            return _rectTransform.rect.xMin + offset + timeValue * _pixelPerSecond;
        }
    }
}