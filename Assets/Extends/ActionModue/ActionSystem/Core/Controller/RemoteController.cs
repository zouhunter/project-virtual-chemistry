using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{

    public class RemoteController : IRemoteController
    {
        int index = 0;
        bool started = false;
        public UnityAction onEndExecute;
        List<ActionCommand> commandList;
        public ActionCommand CurrCommand
        {
            get { return HaveCommand(index) ? commandList[index] : null; }
        }
        public RemoteController(IEnumerable<ActionCommand> commandList)
        {
            this.commandList = new List<ActionCommand>(commandList);
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
        public bool StartExecuteCommand(UnityAction onEndExecute)
        {
            if (!started && HaveCommand(index))
            {
                started = true;
                this.onEndExecute = onEndExecute;
                CurrCommand.StartExecute();
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
                    onEndExecute();
                }
                return true;
            }
            return false;

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
                CurrCommand.UnDoCommand();
                return true;
            }
            else
            {
                if (HaveLast())
                {
                    index--;
                    CurrCommand.UnDoCommand();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 执行多个步骤
        /// </summary>
        /// <param name="stap"></param>
        public bool ExecuteMutliCommand(int stap)
        {
            int newIndex = stap + index;

            if (stap > 0)
            {
                if (started)
                {
                    EndExecuteCommand();
                    if (HaveCommand(newIndex))
                    {
                        for (int i = 0; i < stap - 1; i++)
                        {
                            StartExecuteCommand(null);
                            EndExecuteCommand();
                        }
                    }
                    return true;
                }
                else
                {
                    if (HaveCommand(newIndex - 1))
                    {
                        for (int i = 0; i < stap; i++)
                        {
                            StartExecuteCommand(null);
                            EndExecuteCommand();
                        }
                        return true;
                    }
                }
            }

            else if (stap < 0)
            {
                if (started)
                {
                    UnDoCommand();
                    if (HaveCommand(newIndex))
                    {
                        for (int i = 0; i < -stap; i++)
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
                        for (int i = 0; i < -stap; i++)
                        {
                            UnDoCommand();
                        }
                        return true;
                    }
                }
            }
            return false;
            /*
            int newIndex = stap + index;

            if (started)
            {
                if (stap > 0)
                {
                    EndExecuteCommand();

                }
                else
                {
                    UnDoCommand();
                }

                if (stap != 0)
                {
                    if (HaveCommand(newIndex))
                    {
                        if (stap > 0)
                        {
                            for (int i = 0; i < stap - 1; i++)
                            {
                                StartExecuteCommand(null);
                                EndExecuteCommand();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < - stap; i++)
                            {
                                UnDoCommand();
                            }
                        }
                    }
                    return true;
                }
            }
            else
            {
                if (stap != 0)
                {
                    if (stap > 0 && HaveCommand(newIndex - 1))
                    {
                        for (int i = 0; i < stap; i++)
                        {
                            StartExecuteCommand(null);
                            EndExecuteCommand();
                        }
                    }
                    else if(stap < 0 && HaveCommand(newIndex))
                    {
                        for (int i = 0; i < -stap; i++)
                        {
                            UnDoCommand();
                        }
                    }
                    return true;
                }
            }

            return false;*/
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
        /// <param name="stapName"></param>
        public bool ToTargetCommand(string stapName)
        {
            bool haveNext = true;
            ActionCommand cmd = commandList.Find((x) => stapName == x.StapName);
            if (cmd != null)
            {
                int indexofCmd = commandList.IndexOf(cmd);
                haveNext &= ExecuteMutliCommand(indexofCmd - index);
            }
            return haveNext;
        }
    }

}