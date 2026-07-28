using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeowgaByte.Gameplay
{
    public class TimelineUndoManager : MonoSingleton<TimelineUndoManager>
    {
        [SerializeField] private TimelineManager _timelineManager;
        [SerializeField] private TimelinePlaybackController _playbackController;

        public static event Action<CommandNode> OnUndoPlacedCommand;

        public class PlacedRecord
        {
            public CommandNode Node;
            public GameObject BlockInstance;
        }

        private readonly Stack<PlacedRecord> _history = new Stack<PlacedRecord>();

        public bool CanUndo => _history.Count > 0;

        private void Update()
        {
            bool isCtrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool isZPressed = Input.GetKeyDown(KeyCode.Z);

            if (isCtrlHeld && isZPressed)
            {
                Undo();
            }
        }

        public void RecordPlacement(CommandNode node, GameObject blockInstance)
        {
            PlacedRecord placedRecord = new PlacedRecord
            {
                Node = node,
                BlockInstance = blockInstance
            };
            _history.Push(placedRecord);
        }

        public PlacedRecord Undo()
        {
            if (_history.Count <= 0) return null;
            if (_playbackController != null && _playbackController.IsPlaying) return null;

            PlacedRecord placedRecord = _history.Pop();
            _timelineManager.RemoveCommandNode(placedRecord.Node);

            if (placedRecord.BlockInstance != null)
            {
                Destroy(placedRecord.BlockInstance);
            }

            OnUndoPlacedCommand?.Invoke(placedRecord.Node);
            return placedRecord;
        }


    }
}