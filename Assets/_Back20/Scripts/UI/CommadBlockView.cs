using DG.Tweening;
using MeowgaByte.Core;
using MeowgaByte.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace MeowgaByte.UI
{
    public class CommandBlockView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage; 

        [SerializeField] private Sprite _instantGhostSprite;
        [SerializeField] private Sprite _durationGhostSprite;


        [Header("Nested Style (Wait lồng trong Run)")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _nestedColor = new Color(1f, 0.85f, 0.2f); 
        [SerializeField] private float _normalOutlineWidth = 0f;
        [SerializeField] private Outline _outline;

        private TimelinePlaybackController _playbackController;
        private CommandNode _node;

        private void Awake()
        {
            if (_backgroundImage == null)
            {
                _backgroundImage = GetComponent<Image>();
            }

            if (_backgroundImage != null)
            {
                _backgroundImage.type = Image.Type.Sliced;
            }

            if (_iconImage != null)
            {
                _iconImage.preserveAspect = true;
            }

            _playbackController = FindAnyObjectByType<TimelinePlaybackController>();
        }

        private void OnEnable()
        {
            _playbackController.OnCommandExecuted += PunchIcon;
        }
        private void OnDisable()
        {
            _playbackController.OnCommandExecuted -= PunchIcon;
        }

        public void SetIcon(Sprite icon, float x, float y)
        {
            if (_iconImage == null || icon == null) return;
            _iconImage.sprite = icon;
            _iconImage.rectTransform.sizeDelta = new Vector2(x, y);
        }

        public void SetData(CommandNode node)
        {
            _node = node;
        }

        public void SetGhostSprite(CommandType commandType)
        {
            if (_backgroundImage == null) return;
            
            switch(commandType)
            {
                case CommandType.Instant:
                    _backgroundImage.sprite = _instantGhostSprite;
                    break;
                case CommandType.Duration:
                    _backgroundImage.sprite = _durationGhostSprite;
                    break;
            }
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

        public void PunchIcon(CommandNode node)
        {
            if (_node != node) return;
            _iconImage.transform.DOKill();
            _iconImage.transform.DOPunchScale(0.25f * Vector2.one, 0.2f).SetEase(Ease.OutExpo);
        }
    }
}