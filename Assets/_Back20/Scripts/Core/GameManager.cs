
using System;
using System.Collections.Generic;
using MeowgaByte.Data;
using MeowgaByte.Gameplay;
using Unity.VisualScripting;
using UnityEngine;

namespace MeowgaByte.Core
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField] private LevelData _data;
        public float LevelTime => _data.LevelTime;
        public float SnapInterval => _data.SnapTime;
        public List<LevelCommand> Commands => _data.LevelCommands;

        [SerializeField] private TimelinePlaybackController _timelinePlaybackController;
        [SerializeField] private PlayerController _player;

        private GameState _state = GameState.Prepare;
        public GameState State
        {
            get => _state;
            set => _state = value;
        }

        public void Win()
        {
            _state = GameState.Win;
            _timelinePlaybackController.Stop();
        }

        [Obsolete]
        public void Lose()
        {
            _state = GameState.Lose;
            _timelinePlaybackController.Stop();
            _player.Die();
        }
    }

    public enum GameState
    {
        Prepare,
        Playing,
        Pause,
        Win,
        Lose
    }
}