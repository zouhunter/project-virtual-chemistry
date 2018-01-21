using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{
    public class CommandController
    {
        public bool CommandRegisted { get; private set; }
        public List<IActionCommand> CommandList { get { return _commandList; }}
        public Dictionary<string, List<ActionCommand>> CommandDic { get { return actionDic; } }
        private List<IActionCommand> _commandList = new List<IActionCommand>();
        private Dictionary<string, List<ActionCommand>> actionDic = new Dictionary<string, List<ActionCommand>>();//触发器
        private Dictionary<string, SequencesCommand> seqDic = new Dictionary<string, SequencesCommand>();
        private int totalCommand;
        private int currentCommand;
        private StepComplete onStepComplete;
        private CommandExecute commandExecute;
        private RegistCommandList onAllCommandRegisted;
        private UserError onUserError;

        internal void InitCommand(int totalCommand, CommandExecute onCommandRegistComplete, StepComplete onStepComplete,UserError onUserError, RegistCommandList onAllCommandRegisted)
        {
            this.totalCommand = totalCommand;
            this.onStepComplete = onStepComplete;
            this.onUserError = onUserError;
            this.commandExecute = onCommandRegistComplete;
            this.onAllCommandRegisted = onAllCommandRegisted;
            TryComplelteRegist();
        }
        public void RegistCommand(ActionCommand command)
        {
            currentCommand++;
            if (actionDic.ContainsKey(command.StepName))
            {
                actionDic[command.StepName].Add(command);
            }
            else
            {
                actionDic[command.StepName] = new List<ActionCommand>() { command };
            }
            TryComplelteRegist();
        }

        private void TryComplelteRegist()
        {
            if (totalCommand == currentCommand)
            {
                RegistTriggerCommand();
                if(onAllCommandRegisted != null){
                    onAllCommandRegisted.Invoke(_commandList);
                }
                CommandRegisted = true;
            }
        }

        private void OnOneCommandComplete(string stepName)
        {
            if (seqDic.ContainsKey(stepName))
            {
                var cmd = seqDic[stepName];
                if (!cmd.ContinueExecute())
                {
                    onStepComplete(stepName);
                }
            }
            else
            {
                onStepComplete(stepName);
            }
        }


        private void RegistTriggerCommand()
        {
            if (actionDic != null)
            {
                foreach (var item in actionDic)
                {
                    var stepName = item.Key;
                    if (item.Value.Count > 1)//多命令
                    {
                        item.Value.Sort();
                        var list = new List<IActionCommand>();
                        var total = item.Value.Count;
                        for (int i = 0; i < item.Value.Count; i++)
                        {
                            int index = i;
                            int totalcmd = total;
                            item.Value[index].RegistComplete(OnOneCommandComplete);
                            item.Value[index].RegistAsOperate(OnUserError);
                            item.Value[index].onBeforeActive.AddListener((x) =>
                            {
                                OnCommandStartExecute(stepName, totalcmd, index);
                            });
                            list.Add(item.Value[index]);
                        }
                        var cmd = new SequencesCommand(stepName, list);
                        seqDic.Add(stepName, cmd);
                        _commandList.Add(cmd);
                    }
                    else//单命令
                    {
                        var cmd = item.Value[0];
                        cmd.RegistComplete(OnOneCommandComplete);
                        cmd.RegistAsOperate(OnUserError);
                        cmd.onBeforeActive.AddListener((x) =>
                        {
                            OnCommandStartExecute(stepName, 1, 1);
                        });
                        _commandList.Add(cmd);
                    }

                }
            }
        }

        private void OnUserError(string stepName,string errInfo)
        {
            if(this.onUserError != null)
            {
                this.onUserError.Invoke(stepName, errInfo);
            }
        }


        private void OnCommandStartExecute(string stepName, int totalCount, int currentID)
        {
            if (this.commandExecute != null)
            {
                commandExecute.Invoke(stepName, totalCount, currentID);
            }
        }
    }

}
