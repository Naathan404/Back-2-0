using UnityEngine;

namespace MeowgaByte.Core
{
    public enum CommandType
    {
        Instant = 0,
        Duration = 1
    }

    public enum ActionType
    {
        RunRight = 0,
        RunLeft = 1,
        Wait = 2,
        Jump = 3
    }
}