using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEditor;
public class InteractInfoLoader : ScriptableWizard
{
    [MenuItem("Tools/LoadInteractInfo")]
    static void GetWizard()
    {
        DisplayWizard<InteractInfoLoader>("反应信息导入", "关闭", "记录");
    }

    public TextAsset csvAsset;
    public FlowSystem.InOutItemBehaiver inoutItem;

    void OnWizardOtherButton()
    {
        InteractInfoTable table = LoadTable();
        InteractInfoTable.Row row;
        FlowSystem.Interact interact;
        inoutItem.interact.Clear();
        for (int i = 0; i < table.NumRows(); i++)
        {
            row = table.GetAt(i);
            interact = inoutItem.interact.Find(x => x.intype == row.inType);

            if (interact == null)
            {
                interact = new FlowSystem.Interact();
                inoutItem.interact.Add(interact);
            }

            interact.information = row.information;
            interact.intype = row.inType;
            interact.nodeID = row.nodeID;
            interact.outtype = row.outType;
            interact.outPutFiledinformation = row.outFailInformation;
        }
        UnityEditor.EditorUtility.SetDirty(inoutItem);
    }

    InteractInfoTable LoadTable()
    {
        InteractInfoTable table = new InteractInfoTable();
        table.Load(csvAsset.text);
        return table;
    }
}
