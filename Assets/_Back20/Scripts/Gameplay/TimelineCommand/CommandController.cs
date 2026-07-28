using System.Collections.Generic;
using DG.Tweening;
using MeowgaByte.Core;
using MeowgaByte.Data;
using MeowgaByte.Gameplay;
using MeowgaByte.UI;
using UnityEngine;

public class CommandController : MonoBehaviour
{
    [Header("Command Settings")]
    [SerializeField] private DraggableCommand _commandUIPrefab; 
    [SerializeField] private Transform _commandContainer;

    private void Start()
    {
        InitCommandList();
    }

    private void OnEnable()
    {
        TimelineUndoManager.OnUndoPlacedCommand += AddCommand;
    }

    private void OnDisable()
    {
        TimelineUndoManager.OnUndoPlacedCommand -= AddCommand;
    }

    private void InitCommandList()
    {
        List<LevelCommand> commands = GameManager.Instance?.Commands;

        for (int i = 0; i < commands.Count; i++)
        {
            DraggableCommand cmd = Instantiate(_commandUIPrefab, _commandContainer);
            LevelCommand levelCommand = commands[i];
            cmd.Init(
                levelCommand.Command.CmdType, 
                levelCommand.Command.ActionType, 
                levelCommand.Command.CmdSprite, 
                levelCommand.Durtation, 
                levelCommand.Command.ActionName,
                levelCommand.Command.Description);
        }
    }

    public void AddCommand(CommandNode commandNode)
    {
        DraggableCommand cmd = Instantiate(_commandUIPrefab, _commandContainer);
        cmd.Init(
            commandNode.CmdType, 
            commandNode.ActType, 
            commandNode.IconSprite, 
            commandNode.Duration, 
            commandNode.ActionName,
            commandNode.Description);

        cmd.transform.DOPunchScale(0.25f * Vector2.one, 0.15f);
    }
}
