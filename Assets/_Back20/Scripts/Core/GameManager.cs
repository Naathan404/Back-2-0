
using System;
using System.Collections.Generic;
using MeowgaByte.Data;
using MeowgaByte.Gameplay;
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

        public void Win()
        {
            _timelinePlaybackController.Stop();
        }

        public void Lose()
        {
            _timelinePlaybackController.Stop();
            _player.Die();
        }
    }
}