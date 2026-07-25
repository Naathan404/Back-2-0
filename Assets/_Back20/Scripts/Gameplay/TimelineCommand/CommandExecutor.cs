using System;
using MeowgaByte.Core;
using UnityEngine;

namespace MeowgaByte.Gameplay
{
    public class CommandExecutor : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;

        public bool TryExecuteDurationCommand(ActionType actionType)
        {
            switch (actionType)
            {
                case ActionType.RunRight:
                    _player.SetMoveDirection(1);
                    return true;
                case ActionType.RunLeft:
                    _player.SetMoveDirection(-1);
                    return true;
                case ActionType.Wait:
                    _player.StopMovement();
                    return true;
                case ActionType.Jump:
                    DebugHandler.LogWarning(this.name, "Jump Command IS NOT a duration command");
                    return false;
                default:
                    _player.StopMovement();
                    break;
            }
            return false;
        }

        public bool TryExecuteInstantCommand(ActionType actionType)
        {   
            switch (actionType)
            {
                case ActionType.Jump:
                    return _player.TryJump();
                case ActionType.RunRight:
                case ActionType.RunLeft:
                case ActionType.Wait:
                    DebugHandler.LogWarning(this.name, $"{actionType} is a duration command and should not be executed as instant.");
                    return false;
                default:
                    break;
            }
            return false;
        }

        public void StopDurationCommand()
        {
            _player.StopMovement();
        }
    }
}