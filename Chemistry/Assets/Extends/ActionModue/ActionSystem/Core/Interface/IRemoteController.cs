using UnityEngine.Events;
namespace WorldActionSystem
{

    public interface IRemoteController
    {
        ActionCommand CurrCommand { get; }
        bool StartExecuteCommand(UnityAction onEndExecute);//返回操作成功与否
        bool EndExecuteCommand();
        bool UnDoCommand();
        bool ToTargetCommand(string stap);
        bool ExecuteMutliCommand(int stap);
        void ToAllCommandStart();
        void ToAllCommandEnd();
    }
}