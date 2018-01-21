using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WorldActionSystem
{

    public class TreeCommandController : ICommandController
    {
        UnityAction<bool> onEndExecute { get; set; }
        List<IActionCommand> rootCommands;
        Dictionary<IActionCommand, List<IActionCommand>> commandDic;
        //Dictionary<IActionCommand, IActionCommand> parentDic;
        Stack<IActionCommand> executedCommands = new Stack<IActionCommand>();//正向执行过了记录
        Stack<IActionCommand> backupCommands = new Stack<IActionCommand>();//回退时记录
        List<IActionCommand> activeCommands = new List<IActionCommand>();
        public IActionCommand CurrCommand { get; private set; }

        public TreeCommandController(Dictionary<string, string[]> rule, List<IActionCommand> commandList)
        {
            this.commandDic = new Dictionary<IActionCommand, List<IActionCommand>>();
            //this.parentDic = new Dictionary<IActionCommand, IActionCommand>();
            foreach (var item in rule)
            {
                var key = commandList.Find(x => x.StepName == item.Key);
                var values = new List<IActionCommand>();
                foreach (var child in item.Value)
                {
                    var c = commandList.Find(x => x.StepName == child);
                    //parentDic.Add(c, key);
                    values.Add(c);
                }
                commandDic[key] = values;
            }
            rootCommands = SurchRootCommands(commandDic);
        }

        public TreeCommandController(Dictionary<IActionCommand, List<IActionCommand>> commandDic)
        {
            this.commandDic = new Dictionary<IActionCommand, List<IActionCommand>>();
            //this.parentDic = new Dictionary<IActionCommand, IActionCommand>();
            foreach (var item in commandDic)
            {
                commandDic[item.Key] = new List<IActionCommand>(item.Value);
                //foreach (var child in item.Value)
                //{
                //    parentDic.Add(child, item.Key);
                //}
            }
            rootCommands = SurchRootCommands(commandDic);
        }

        /// <summary>
        /// 开启一个命令,并返回正常执行与否
        /// </summary>
        public bool StartExecuteCommand(UnityAction<bool> onEndExecute, bool forceAuto)
        {
            Debug.Assert(rootCommands != null || rootCommands.Count == 0, "root is Empty");

            backupCommands.Clear();

            if (activeCommands.Count == 0)
            {
                this.onEndExecute = onEndExecute;
                if (CurrCommand == null)
                {
                    activeCommands.AddRange(rootCommands);
                }
                else
                {
                    if (commandDic.ContainsKey(CurrCommand))
                    {
                        activeCommands.AddRange(commandDic[CurrCommand]);
                    }
                }

                foreach (var cmd in activeCommands)
                {
                    if (!cmd.Startd)
                    {
                        cmd.StartExecute(forceAuto);
                    }
                }

                return true;
            }
            else
            {
                this.onEndExecute = onEndExecute;
                return false;
            }
        }


        /// <summary>
        /// 结束已经开始的命令
        /// </summary>
        public bool EndExecuteCommand()
        {
            if (backupCommands.Count > 0)
            {
                CurrCommand = backupCommands.Pop();
                executedCommands.Push(CurrCommand);
                CurrCommand.EndExecute();
                return true;
            }
            else
            {

                return false;
            }
        }

        /// <summary>
        /// 结束已经开始的命令
        /// </summary>
        public void OnEndExecuteCommand(string step)
        {
            bool haveNext = true;
            if (activeCommands.Count > 0)
            {
                foreach (var item in activeCommands)
                {
                    if (step == item.StepName)
                    {
                        CurrCommand = item;
                        executedCommands.Push(item);
                        haveNext = commandDic.ContainsKey(item);
                    }
                    else
                    {
                        item.UnDoExecute();
                    }
                }

                activeCommands.Clear();

                if (onEndExecute != null)
                {
                    onEndExecute(haveNext);
                }
            }

            
        }

        /// <summary>
        /// 撤消操作，并返回能否继续撤销
        /// </summary>
        /// <returns></returns>
        public bool UnDoCommand()
        {
            //清除已经开始的步骤
            if (activeCommands.Count > 0)
            {
                foreach (var cmd in activeCommands)
                {
                    cmd.UnDoExecute();
                }
                activeCommands.Clear();
            }

            //回退
            if (executedCommands.Count > 0)
            {
                var cmd = executedCommands.Pop();
                backupCommands.Push(cmd);
                cmd.UnDoExecute();

                if (executedCommands.Count > 0)
                {
                    CurrCommand = executedCommands.Peek();
                }
                else
                {
                    CurrCommand = null;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 执行多个步骤
        /// </summary>
        /// <param name="step"></param>
        public bool ExecuteMutliCommand(int step)
        {
            if (step < 0)
            {
                bool haveLast = false;
                while (step < 0)
                {
                    haveLast = UnDoCommand();
                    if (!haveLast)
                    {
                        break;
                    }
                    else
                    {
                        step++;
                    }
                }
                return haveLast;
            }
            else if (step > 0)
            {
                bool haveNext = true;

                while (step > 0)
                {
                    haveNext = EndExecuteCommand();
                    if (haveNext)
                    {
                        step--;
                    }
                    else
                    {
                        break;
                    }
                }
                return haveNext;
            }
            else
            {
                UnDoCommand();
                return true;
            }

        }

        /// <summary>
        /// 回到命令列表起始
        /// </summary>
        public void ToAllCommandStart()
        {
            while (executedCommands.Count > 0){
                UnDoCommand();
            }
            CurrCommand = null;
        }

        /// <summary>
        /// 完成所有命令
        /// </summary>
        public void ToAllCommandEnd()
        {
            while (backupCommands.Count > 0)
            {
                EndExecuteCommand();
            }
        }

        /// <summary>
        /// 执行到指定的步骤
        /// </summary>
        /// <param name="stepName"></param>
        public bool ToTargetCommand(string stepName)
        {
            if (executedCommands.Where(x => x.StepName == stepName).Count() > 0)//在已经执行过的步骤中
            {
                while (executedCommands.Peek().StepName != stepName)
                {
                    UnDoCommand();
                }
                UnDoCommand();
                return true;
            }
            else if (backupCommands.Where(x => x.StepName == stepName).Count() > 0)//在回退的步骤中
            {
                while (backupCommands.Peek().StepName != stepName)
                {
                    EndExecuteCommand();
                }
                return true;
            }
            return false;
        }

        private static List<IActionCommand> SurchRootCommands(Dictionary<IActionCommand, List<IActionCommand>> commandDic)
        {
            var parents = new List<IActionCommand>();
            foreach (var item in commandDic)
            {
                var maybe = item.Key;
                var haveparent = false;
                foreach (var item0 in commandDic)
                {
                    if (item0.Value.Contains(maybe))
                    {
                        haveparent = true;
                    }
                }
                if (!haveparent)
                {
                    parents.Add(maybe);
                }
            }
            return parents;
        }
    }

}