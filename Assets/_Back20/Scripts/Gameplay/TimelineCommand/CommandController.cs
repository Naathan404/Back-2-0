using System.Collections.Generic;
using MeowgaByte.Core;
using MeowgaByte.Data;
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

    private void InitCommandList()
    {
        List<LevelCommand> commands = GameManager.Instance?.Commands;

        for (int i = 0; i < commands.Count; i++)
        {
            DraggableCommand cmd = Instantiate(_commandUIPrefab, _commandContainer);
            LevelCommand levelCommand = commands[i];
            cmd.Init(levelCommand.Command.CmdType, levelCommand.Command.ActionType, levelCommand.Command.CmdSprite, levelCommand.Durtation);
        }
    }
}
