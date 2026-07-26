using System;
using System.Collections.Generic;
using MeowgaByte.Core;
using MeowgaByte.UI;
using UnityEngine;

namespace MeowgaByte.Gameplay
{
    /// <summary>
    /// Điều khiển việc "Play" level: chạy Playhead từ LevelTime (bên phải)
    /// về 0 (bên trái) trên các Timeline, và thực thi Command tương ứng
    /// ngay khi Playhead cán mốc StartTime của lệnh đó.
    /// 
    /// </summary>
    public class TimelinePlaybackController : MonoBehaviour
    {
        [Serializable]
        public class PlayheadLane
        {
            [Tooltip("Timeline mà Playhead này chạy trên đó")]
            public TimelineDropZone Zone;
            [Tooltip("RectTransform hiển thị vạch Playhead trên Timeline này")]
            public RectTransform PlayheadVisual;
        }

        [Header("References")]
        [SerializeField] private TimelineManager _timelineManager;
        [SerializeField] private CommandExecutor _executor;
        [SerializeField] private PlayerController _player;
        [SerializeField] private InfoPanelController _info;

        [Header("Playhead Visuals")]
        [SerializeField] private List<PlayheadLane> _lanes = new List<PlayheadLane>();

        public event Action OnPlaybackStarted;
        public event Action OnPlaybackTimeout;
        public event Action<CommandNode> OnCommandExecuted;

        private List<CommandNode> _playbackSequence;
        private int _nextCommandIndex;

        private List<ScheduledEvent> _playbackEvents;
        private int _nextEventIndex;

        private int _activeDurationVersion;
        private float _elapsedTime;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;
        private GameManager _gameManager => GameManager.Instance;

        private class ScheduledEvent
        {
            public float Time;
            public CommandNode Node;
            public bool IsStop;
            public int Version;
        }

        private void Start()
        {
            _info.UpdateCounterText(_gameManager.LevelTime);
            _info.UpdateSnapIntervalText(_gameManager.SnapInterval);
        }

        private void Update()
        {
            if (!_isPlaying) return;

            _elapsedTime += Time.deltaTime;
            float levelTime = _gameManager.LevelTime;
            float clockValue = Mathf.Clamp(levelTime - _elapsedTime, 0f, levelTime);

            UpdatePlayheadVisuals(clockValue);
            ExecuteDueCommands();

            _info.UpdateCounterText(Mathf.Max(0, levelTime - _elapsedTime));

            if (_elapsedTime >= levelTime)
            {
                _isPlaying = false;
                GameManager.Instance?.Lose();
                _executor.StopDurationCommand();
                OnPlaybackTimeout?.Invoke();
            }
        }

        /// <summary>
        /// Gọi hàm này từ nút "Play". Reset nhân vật + Playhead rồi bắt đầu chạy.
        /// </summary>
        public void Play()
        {
            if (_timelineManager == null || _executor == null || _player == null)
            {
                DebugHandler.LogError(this.name, "Missing references (TimelineManager / CommandExecutor / PlayerController)");
                return;
            }
            
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.GameStartSFX, true, true);
            _player.Respawn();
            _executor.StopDurationCommand();

            _playbackSequence = _timelineManager.GetPlaybackSequence();
            _nextCommandIndex = 0;

            _playbackEvents = BuildPlaybackEvents();
            _nextEventIndex = 0;
            _activeDurationVersion = 0;
            _elapsedTime = 0f;
            _isPlaying = true;

            UpdatePlayheadVisuals(_gameManager.LevelTime);
            OnPlaybackStarted?.Invoke();
        }

        /// <summary>
        /// Gộp mỗi CommandNode thành 1 event Start (và thêm 1 event Stop nếu là
        /// Duration command có Duration > 0), rồi sắp xếp toàn bộ theo thời gian
        /// thực tăng dần để chạy tuần tự trong Update().
        /// </summary>
        private List<ScheduledEvent> BuildPlaybackEvents()
        {
            float levelTime = _gameManager.LevelTime;
            List<CommandNode> sequence = _timelineManager.GetPlaybackSequence();
            List<ScheduledEvent> events = new List<ScheduledEvent>(sequence.Count * 2);
            int version = 0;

            // Tìm các Wait bị lồng bên trong 1 Run để xử lý riêng (không phát Start/Stop độc lập)
            HashSet<CommandNode> nestedWaits = new HashSet<CommandNode>();
            foreach (CommandNode run in sequence)
            {
                if (run.CmdType != CommandType.Duration || run.ActType == ActionType.Wait) continue;
                float runBegin = run.StartTime - run.Duration, runEnd = run.StartTime;

                foreach (CommandNode other in sequence)
                {
                    if (other == run || other.ActType != ActionType.Wait) continue;
                    float wBegin = other.StartTime - other.Duration, wEnd = other.StartTime;
                    if (wBegin >= runBegin && wEnd <= runEnd) nestedWaits.Add(other);
                }
            }

            foreach (CommandNode node in sequence)
            {
                if (nestedWaits.Contains(node)) continue; // xử lý bên dưới, gắn theo version của Run chứa nó

                float startTrigger = levelTime - node.StartTime;

                if (node.CmdType == CommandType.Duration && node.Duration > 0f)
                {
                    version++;
                    events.Add(new ScheduledEvent { Time = startTrigger, Node = node, IsStop = false, Version = version });
                    events.Add(new ScheduledEvent { Time = startTrigger + node.Duration, Node = node, IsStop = true, Version = version });

                    if (node.ActType != ActionType.Wait)
                    {
                        float runBegin = node.StartTime - node.Duration, runEnd = node.StartTime;

                        foreach (CommandNode wait in nestedWaits)
                        {
                            float wBegin = wait.StartTime - wait.Duration, wEnd = wait.StartTime;
                            if (wBegin < runBegin || wEnd > runEnd) continue;

                            float pauseTrigger = levelTime - wait.StartTime;
                            float resumeTrigger = pauseTrigger + wait.Duration;

                            // Pause: coi như 1 Stop, dùng CHUNG version với Run đang chạy
                            events.Add(new ScheduledEvent { Time = pauseTrigger, Node = wait, IsStop = true, Version = version });
                            // Resume: Start lại đúng ActionType của Run, vẫn CHUNG version
                            events.Add(new ScheduledEvent { Time = resumeTrigger, Node = node, IsStop = false, Version = version });
                        }
                    }
                }
                else
                {
                    events.Add(new ScheduledEvent { Time = startTrigger, Node = node, IsStop = false });
                }
            }

            events.Sort((a, b) => a.Time.CompareTo(b.Time));
            return events;
        }

        /// <summary>
        /// Dừng thực thi giữa chừng (vd: khi PlayerController báo đã chạm đích,
        /// hoặc người chơi bấm nút Stop).
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
            _executor.StopDurationCommand();
        }

        private void ExecuteDueCommands()
        {
            while (_nextEventIndex < _playbackEvents.Count &&
                   _elapsedTime >= _playbackEvents[_nextEventIndex].Time)
            {
                ScheduledEvent evt = _playbackEvents[_nextEventIndex];
 
                if (evt.IsStop)
                {
                    if (evt.Version == _activeDurationVersion)
                    {
                        _executor.StopDurationCommand();
                        _info.UpdateNotifyText("STOP");
                    }
                }
                else if (evt.Node.CmdType == CommandType.Duration)
                {
                    _executor.TryExecuteDurationCommand(evt.Node.ActType);
                    _activeDurationVersion = evt.Version;
                    _info.UpdateNotifyText(evt.Node.ActionName);
                }
                else
                {
                    _executor.TryExecuteInstantCommand(evt.Node.ActType);
                    _info.UpdateNotifyText(evt.Node.ActionName);
                }
 
                OnCommandExecuted?.Invoke(evt.Node);
                _nextEventIndex++;
            }
        }
 

        private void UpdatePlayheadVisuals(float clockValue)
        {
            foreach (var lane in _lanes)
            {
                if (lane.Zone == null || lane.PlayheadVisual == null) continue;

                float x = lane.Zone.TimeToLocalX(clockValue);
                Vector2 pos = lane.PlayheadVisual.anchoredPosition;
                pos.x = x;
                lane.PlayheadVisual.anchoredPosition = pos;
            }
        }
    }
}