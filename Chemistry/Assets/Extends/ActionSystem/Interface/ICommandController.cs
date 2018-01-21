using UnityEngine.Events;
namespace WorldActionSystem
{
    /// <summary>
    /// 状态控制器
    /// </summary>
    public interface ICommandController
    {
        IActionCommand CurrCommand { get; }
        bool StartExecuteCommand(UnityAction<bool> onEndExecute,bool forceAuto);//返回操作成功与否
        bool EndExecuteCommand();
        void OnEndExecuteCommand(string step);//外部触发结束
        bool UnDoCommand();
        bool ToTargetCommand(string step);
        bool ExecuteMutliCommand(int step);
        void ToAllCommandStart();
        void ToAllCommandEnd();
    }
}