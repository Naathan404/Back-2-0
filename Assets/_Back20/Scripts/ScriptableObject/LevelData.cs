using System;
using System.Collections.Generic;
using MeowgaByte.Gameplay;
using UnityEngine;

namespace MeowgaByte.Data
{
    [CreateAssetMenu(fileName = "LevelDataConfig", menuName = "MeowgaByte/Level Data")]
    public class LevelData : ScriptableObject
    {
        public float LevelTime = 5f;
        public float SnapTime = 0.5f;

        public List<LevelCommand> LevelCommands;
    }

    [Serializable]
    public class LevelCommand
    {
        public BaseCommand Command;
        [Tooltip("Nếu là Instant Command thì là 0 nha")]
        public float Durtation = 0;
    }
}