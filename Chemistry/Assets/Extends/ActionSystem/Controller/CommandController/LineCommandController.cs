using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{

    public class LineCommandController : ICommandController
    {
        int index = 0;
        bool started = false;
        public UnityAction<bool> onEndExecute;
        List<IActionCommand> commandList;

        public IActionCommand CurrCommand
        {
            get { return HaveCommand(index) ? commandList[index] : null; }
        }
        public LineCommandController(IEnumerable<IActionCommand> commandList)
        {
            this.commandList = new List<IActionCommand>(commandList);
        }


        bool HaveCommand(int id)
        {
            if (id >= 0 && id < commandList.Count)
            {
                return true;
            }
            return false;
        }

        bool HaveNext()
        {
            if (index < commandList.Count - 1)
            {
                return true;
            }
            return false;
        }

        bool HaveLast()
        {
            if (index > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 开启一个命令,并返回正常执行与否
        /// </summary>
        public bool StartExecuteCommand(UnityAction<bool> onEndExecute, bool forceAuto)
        {
            if (!started && HaveCommand(index))
            {
                started = true;
                this.onEndExecute = onEndExecute;
                if (!CurrCommand.Startd)
                    CurrCommand.StartExecute(forceAuto);
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
            if (started)
            {
                started = false;
                CurrCommand.EndExecute();

                index++;

                if (onEndExecute != null)
                {
                    onEndExecute(HaveNext());
                }
                return true;
            }
            return false;

        }

        /// <summary>
        /// 结束已经开始的命令
        /// </summary>
        public void OnEndExecuteCommand(string stepName)
        {
            if (CurrCommand != null && CurrCommand.StepName == stepName)
            {
                started = false;
                index++;

                if (onEndExecute != null)
                {
                    onEndExecute(HaveNext());
                }
            }
            else
            {
               if(CurrCommand!=null) Debug.LogError("StepNotEqual:" + stepName + ":" + CurrCommand.StepName);
            }
        }

        /// <summary>
        /// 撤消操作，并返回能否继续撤销
        /// </summary>
        /// <returns></returns>
        public bool UnDoCommand()
        {
            if (started)
            {
                started = false;
                onEndExecute = null;
                CurrCommand.UnDoExecute();
                return true;
            }
            else
            {
                if (HaveLast())
                {
                    index--;
                    CurrCommand.UnDoExecute();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 执行多个步骤
        /// </summary>
        /// <param name="step"></param>
        public bool ExecuteMutliCommand(int step)
        {
            int newIndex = step + index;

            if (step > 0)
            {
                if (started)
                {
                    EndExecuteCommand();
                    if (HaveCommand(newIndex))
                    {
                        for (int i = 0; i < step - 1; i++)
                        {
                            StartExecuteCommand(null, false);
                            EndExecuteCommand();
                        }
                    }
                    return true;
                }
                else
                {
                    if (HaveCommand(newIndex - 1))
                    {
                        for (int i = 0; i < step; i++)
                        {
                            StartExecuteCommand(null, false);
                            EndExecuteCommand();
                        }
                        return true;
                    }
                }
            }

            else if (step < 0)
            {
                if (started)
                {
                    UnDoCommand();
                    if (HaveCommand(newIndex))
                    {
                        for (int i = 0; i < -step; i++)
                        {
                            UnDoCommand();
                        }
                    }
                    return true;
                }
                else
                {
                    if (HaveCommand(newIndex))
                    {
                        for (int i = 0; i < -step; i++)
                        {
                            UnDoCommand();
                        }
                        return true;
                    }
                }
            }
            else
            {
                if (started)
                {
                    UnDoCommand();
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// 回到命令列表起始
        /// </summary>
        public void ToAllCommandStart()
        {
            ExecuteMutliCommand(-index);
        }

        /// <summary>
        /// 完成所有命令
        /// </summary>
        public void ToAllCommandEnd()
        {
            ExecuteMutliCommand(commandList.Count - index);
        }

        /// <summary>
        /// 执行到指定的步骤
        /// </summary>
        /// <param name="stepName"></param>
        public bool ToTargetCommand(string stepName)
        {
            bool haveNext = true;
            IActionCommand cmd = commandList.Find((x) => stepName == x.StepName);
            if (cmd != null)
            {
                int indexofCmd = commandList.IndexOf(cmd);
                haveNext &= ExecuteMutliCommand(indexofCmd - index);
            }
            return haveNext;
        }
    }

}