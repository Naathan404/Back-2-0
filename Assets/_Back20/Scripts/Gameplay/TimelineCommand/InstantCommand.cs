using System;
using MeowgaByte.Core;
using UnityEngine;

namespace MeowgaByte.Gameplay
{
    [CreateAssetMenu(fileName = "New Instant Command", menuName = "MeowgaByte/Command/Instant")]
    public class InstantCommand : BaseCommand
    {
        [SerializeField] private ActionType _actionType;
        public override CommandType CmdType => CommandType.Instant;

        public override ActionType ActionType { get => _actionType; set => _actionType = value; }
    }
}