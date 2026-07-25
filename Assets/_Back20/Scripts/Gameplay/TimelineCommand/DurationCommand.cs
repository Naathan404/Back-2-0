

using MeowgaByte.Core;
using UnityEngine;

namespace MeowgaByte.Gameplay
{
    [CreateAssetMenu(fileName = "New Duration Command", menuName = "MeowgaByte/Command/Duration")]
    public class DurationCommand : BaseCommand
    {
        [SerializeField] private ActionType _actionType;
        public override CommandType CmdType => CommandType.Duration;

        public override ActionType ActionType { get => _actionType; set => _actionType = value; }
    }
}