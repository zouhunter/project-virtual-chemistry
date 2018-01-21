using System.Collections.Generic;

namespace WorldActionSystem
{
    public delegate void CommandExecute(string stepName,int totalCount,int currentID);
    public delegate void StepComplete(string stepName);
    public delegate void UserError(string step, string info);
    public delegate void RegistCommandList(List<IActionCommand> commandList);

    public delegate ElementController GetElementController();
}
