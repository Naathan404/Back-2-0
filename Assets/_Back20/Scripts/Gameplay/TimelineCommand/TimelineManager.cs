using System;
using System.Collections.Generic;
using System.Linq;
using MeowgaByte.Core;
using UnityEngine;

namespace MeowgaByte.Gameplay
{
    public class TimelineManager : MonoBehaviour
    {
        [SerializeField] private List<CommandNode> _commandList;

        private void Start()
        {
            _commandList = new List<CommandNode>();
        }

        public bool TryAddCommandNode(float startTime, ActionType actionType, CommandType commandType, float duration = 0, string actionName = "")
        {
            if (startTime <= 0) return false;

            _commandList.Add(new CommandNode
            {
                ActionName = actionName,
                StartTime = startTime,
                Duration = duration,
                ActType = actionType,
                CmdType = commandType
            });
            return true;
        }

        /// <summary>
        /// True nếu đặt 1 Duration command
        /// sẽ đè lên bất kỳ Duration command nào đã có trên track.
        /// </summary>
        public bool HasOverlap(CommandType cmdType, float candidateStart, float duration)
        {
            if (cmdType != CommandType.Duration || duration <= 0f) return false;

            float newBegin = candidateStart - duration;
            float newEnd = candidateStart;

            foreach (CommandNode node in _commandList)
            {
                if (node.CmdType != CommandType.Duration) continue;

                float otherBegin = node.StartTime - node.Duration;
                float otherEnd = node.StartTime;

                if (newBegin < otherEnd && otherBegin < newEnd) return true;
            }
            return false;
        }

        /// <summary>
        /// Tìm khe hở chứa rawHoverTime, rồi ép candidateStart nằm gọn trong khe đó
        /// sao cho block không đè neighbor nào.
        /// Trả về float.NaN nếu khe hở hiện tại không đủ rộng để chứa "duration".
        /// </summary>
        public float ClampToFreeGap(CommandType cmdType, float rawHoverTime, float duration, float levelTime)
        {
            if (cmdType != CommandType.Duration || duration <= 0f)
                return Mathf.Clamp(rawHoverTime, 0f, levelTime);

            var occupied = _commandList
                .Where(n => n.CmdType == CommandType.Duration)
                .Select(n => (begin: n.StartTime - n.Duration, end: n.StartTime));

            foreach (var (begin, end) in occupied)
            {
                if (rawHoverTime > begin && rawHoverTime < end)
                {
                    return float.NaN;
                }
            }

            float gapLeft = 0f;
            float gapRight = levelTime;

            foreach (var (begin, end) in occupied)
            {
                if (end <= rawHoverTime && end > gapLeft) gapLeft = end;
                if (begin >= rawHoverTime && begin < gapRight) gapRight = begin;
            }

            float minStart = gapLeft + duration;
            float maxStart = gapRight;

            if (minStart > maxStart) return float.NaN;

            return Mathf.Clamp(rawHoverTime, minStart, maxStart);
        }

        public List<CommandNode> GetPlaybackSequence()
        {
            return _commandList.OrderByDescending(c => c.StartTime).ToList();
        }

    }

    [Serializable]
    public class CommandNode
    {
        public float StartTime = 0;
        public float Duration = 0;
        public string ActionName;
        public ActionType ActType = ActionType.Wait;
        public CommandType CmdType = CommandType.Instant;
    }
}
