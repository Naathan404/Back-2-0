
using System.Collections.Generic;
using MeowgaByte.Data;
using UnityEngine;

namespace MeowgaByte.Core
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField] private LevelData _data;
        public float LevelTime => _data.LevelTime;
        public float SnapInterval => _data.SnapTime;
        public List<LevelCommand> Commands => _data.LevelCommands;
    }
}