using MeowgaByte.Core;
using UnityEngine;

namespace MeowgaByte.Gameplay
{
    public abstract class BaseCommand : ScriptableObject
    {
        public Sprite CmdSprite;
        public abstract CommandType CmdType { get; }
        public abstract ActionType ActionType { get; set; }
    }
}
