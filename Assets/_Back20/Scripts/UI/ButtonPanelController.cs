using System;
using MeowgaByte.Core;
using MeowgaByte.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace MeowgaByte.UI
{
    public class ButtonPanelController : MonoBehaviour
    {
        [SerializeField] private TimelinePlaybackController _playbackConntroller;
        [SerializeField] private TimelineUndoManager _undoManager;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Button _clearButton;
        [SerializeField] private Button _undoButton;

        private void Awake()
        {
            if (_playbackConntroller == null)
                _playbackConntroller = FindAnyObjectByType<TimelinePlaybackController>();
            if (_undoManager == null)
                _undoManager = FindAnyObjectByType<TimelineUndoManager>();
        }

        private void Start()
        {
            _playButton.gameObject.SetActive(true);
            _clearButton.gameObject.SetActive(true);
            _undoButton.gameObject.SetActive(true);
            _replayButton.gameObject.SetActive(false);
        }

        public void Play()
        {
            _playbackConntroller.Play();
            _playButton.gameObject.SetActive(false);
            _clearButton.gameObject.SetActive(false);
            _undoButton.gameObject.SetActive(false);
            _replayButton.gameObject.SetActive(true);
        }

        public void Replay()
        {
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.ButtonClick, true);
            SceneController.Instance?.ReloadSceneWithTransition(false);
        }

        public void Undo()
        {
            _undoManager.Undo();
        }
    }
    
}