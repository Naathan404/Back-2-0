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

        private static (float begin, float end) Range(CommandNode n) => (n.StartTime - n.Duration, n.StartTime);

        private static bool IsFullyContained(float innerBegin, float innerEnd, float outerBegin, float outerEnd) 
            => innerBegin >= outerBegin && innerEnd <= outerEnd;

        private void Start()
        {
            _commandList = new List<CommandNode>();
        }

        public CommandNode TryAddCommandNode(float startTime, ActionType actionType, CommandType commandType, float duration = 0, string actionName = "")
        {
            if (startTime <= 0) return null;

            CommandNode node = new CommandNode
            {
                ActionName = actionName,
                StartTime = startTime,
                Duration = duration,
                ActType = actionType,
                CmdType = commandType
            };

            _commandList.Add(node);
            return node;
        }

        /// <summary>
        /// True nếu đặt 1 Duration command
        /// sẽ đè lên bất kỳ Duration command nào đã có trên track.
        /// </summary>
        public bool HasOverlap(CommandType cmdType, ActionType actionType, float candidateStart, float duration)
        {
            if (cmdType != CommandType.Duration || duration <= 0f) return false;

            float newBegin = candidateStart - duration;
            float newEnd = candidateStart;
            bool isWait = actionType == ActionType.Wait;

            foreach (CommandNode node in _commandList)
            {
                if (node.CmdType != CommandType.Duration) continue;

                var (otherBegin, otherEnd) = Range(node);
                bool intersects = newBegin < otherEnd && otherBegin < newEnd;
                if (!intersects) continue;

                bool otherIsWait = node.ActType == ActionType.Wait;
                if (isWait == otherIsWait) return true;

                bool valid = isWait
                    ? IsFullyContained(newBegin, newEnd, otherBegin, otherEnd)
                    : IsFullyContained(otherBegin, otherEnd, newBegin, newEnd);

                if (!valid) return true;
            }
            return false;
        }

        public bool IsNestedInsideRun(float candidateStart, float duration)
        {
            float begin = candidateStart - duration, end = candidateStart;
            return _commandList.Any(n => n.CmdType == CommandType.Duration && n.ActType != ActionType.Wait
                && begin >= (n.StartTime - n.Duration) && end <= n.StartTime);
        }

        /// <summary>
        /// Tìm khe hở chứa rawHoverTime, rồi ép candidateStart nằm gọn trong khe đó
        /// sao cho block không đè neighbor nào.
        /// Trả về float.NaN nếu khe hở hiện tại không đủ rộng để chứa "duration".
        /// </summary>
        // public float ClampToFreeGap(CommandType cmdType, float rawHoverTime, float duration, float levelTime)
        // {
        //     if (cmdType != CommandType.Duration || duration <= 0f)
        //         return Mathf.Clamp(rawHoverTime, 0f, levelTime);

        //     var occupied = _commandList
        //         .Where(n => n.CmdType == CommandType.Duration)
        //         .Select(n => (begin: n.StartTime - n.Duration, end: n.StartTime));

        //     foreach (var (begin, end) in occupied)
        //     {
        //         if (rawHoverTime > begin && rawHoverTime < end)
        //         {
        //             return float.NaN;
        //         }
        //     }

        //     float gapLeft = 0f;
        //     float gapRight = levelTime;

        //     foreach (var (begin, end) in occupied)
        //     {
        //         if (end <= rawHoverTime && end > gapLeft) gapLeft = end;
        //         if (begin >= rawHoverTime && begin < gapRight) gapRight = begin;
        //     }

        //     float minStart = gapLeft + duration;
        //     float maxStart = gapRight;

        //     if (minStart > maxStart) return float.NaN;

        //     return Mathf.Clamp(rawHoverTime, minStart, maxStart);
        // }

        public float ClampToFreeGap(CommandType cmdType, ActionType actionType, float rawHoverTime, float duration, float levelTime)
        {
            if (cmdType != CommandType.Duration || duration <= 0f)
                return Mathf.Clamp(rawHoverTime, 0f, levelTime);

            if (actionType == ActionType.Wait)
            {
                CommandNode hostRun = _commandList.FirstOrDefault(n =>
                    n.CmdType == CommandType.Duration && n.ActType != ActionType.Wait &&
                    rawHoverTime > (n.StartTime - n.Duration) && rawHoverTime < n.StartTime);

                if (hostRun != null)
                    return ClampInsideHostRun(hostRun, rawHoverTime, duration);
            }

            var occupied = _commandList
                .Where(n => n.CmdType == CommandType.Duration)
                .Select(Range);

            foreach (var (begin, end) in occupied)
                if (rawHoverTime > begin && rawHoverTime < end) return float.NaN;

            float gapLeft = 0f, gapRight = levelTime;
            foreach (var (begin, end) in occupied)
            {
                if (end <= rawHoverTime && end > gapLeft) gapLeft = end;
                if (begin >= rawHoverTime && begin < gapRight) gapRight = begin;
            }

            float minStart = gapLeft + duration;
            if (minStart > gapRight) return float.NaN;
            return Mathf.Clamp(rawHoverTime, minStart, gapRight);
        }

        private float ClampInsideHostRun(CommandNode hostRun, float rawHoverTime, float duration)
        {
            float runBegin = hostRun.StartTime - hostRun.Duration;
            float runEnd = hostRun.StartTime;

            // Các Wait đã lồng sẵn trong Run này cũng là chướng ngại (tránh Wait-Wait)
            var nestedWaits = _commandList
                .Where(n => n.CmdType == CommandType.Duration && n.ActType == ActionType.Wait
                            && n.StartTime <= runEnd && (n.StartTime - n.Duration) >= runBegin)
                .Select(Range);

            float gapLeft = runBegin, gapRight = runEnd;
            foreach (var (begin, end) in nestedWaits)
            {
                if (end <= rawHoverTime && end > gapLeft) gapLeft = end;
                if (begin >= rawHoverTime && begin < gapRight) gapRight = begin;
            }

            float minStart = gapLeft + duration;
            if (minStart > gapRight) return float.NaN;
            return Mathf.Clamp(rawHoverTime, minStart, gapRight);
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
