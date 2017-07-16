using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

public class RemoteControl
{
    public Dictionary<string, IUndoableCommand> commandDic;
    private List<IUndoableCommand> commands = new List<IUndoableCommand>();
    private IUndoableCommand lastCommand;
    private IUndoableCommand icommand;
    private Stack<UnityAction> undoFunctions = new Stack<UnityAction>();

    public bool IswaitToExecute { get; private set; }
    private int commandIndex;

    public RemoteControl()
    {
        IswaitToExecute = true;
    }
    ~RemoteControl()
    {

    }
    void GetOperateList()
    {
        //IUndoableCommand cmd;
        //foreach (var item in OperationManager.Instance.experimentSteps)
        //{
        //    cmd = commandDic[item.procedureName];
        //    commands.Add(cmd);
        //}
    }

    public bool Execute()
    {
        if (!IswaitToExecute) return false;
        if (commandIndex >= commands.Count) return false;

        IswaitToExecute = false;
        icommand = commands[commandIndex];
        lastCommand = icommand;
        if (icommand != null)
        {
            icommand.Execute();
            undoFunctions.Push(icommand.UnDo);
            commandIndex++;
        }
        return true;
    }

    public bool EndExecute()
    {
        IswaitToExecute = true;
        if (lastCommand != null && !lastCommand.SecureEnd)
        {
            lastCommand.EndExecute();
            return true;
        }
        return false;
    }

    public bool UnDo()
    {
        if (commandIndex < 0) return false;
        if (undoFunctions.Count <= 0) return false;

        undoFunctions.Pop().Invoke();
        IswaitToExecute = true;
        commandIndex--;
        return true;
    }
}

