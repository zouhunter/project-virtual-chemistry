using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using WorldActionSystem;
using BundleUISystem;
public class ActionPanel : UIPanelTemp, IMediator<float>
{
    public ActionCommand command;
    public bool autoExecute;
    [SerializeField]
    private Button m_Close;
    [SerializeField]
    private Slider m_slider;
    [SerializeField]
    private Text m_sliderText;
    [SerializeField]
    private Button m_Start;
    [SerializeField]
    private ActionCommand commandPrefab;
    public string Acceptor { get { return "progress"; } }
    protected override void OnEnable()
    {
        base.OnEnable();
        Facade.RegisterMediator(this);
    }
    private void Start()
    {
        command = GameObject.Instantiate(commandPrefab);
        command.RegistComplete((x) => { Debug.Log(x + ":Completed"); });
        m_Start.onClick.AddListener(() => { command.UnDoExecute(); command.StartExecute(autoExecute); });
        m_Close.onClick.AddListener(() => { Destroy(gameObject); ; });
        command.StartExecute(autoExecute);
    }
    protected void OnDisable()
    {
        Facade.RegisterMediator(this);
    }
    public override void HandleData(object data)
    {
        base.HandleData(data);
        if(data is ActionCommand)
        {
            commandPrefab = data as ActionCommand;
        }
    }

    public void HandleNotification(float notify)
    {
        var intValue = (int)(notify * 100);
        m_sliderText.text = string.Format("{0}/%", intValue);
        m_slider.value = notify;

        if (intValue > 95)
        {
            m_slider.gameObject.SetActive(false);
        }
        else
        {
            m_slider.gameObject.SetActive(true);
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if(command && command.gameObject)
        {
            Destroy(command.gameObject);
        }
    }
    //private void OnGUI()
    //{
    //    if (GUILayout.Button("StartCommand"))
    //    {
    //        command.StartExecute(autoExecute);
    //    }
    //    if (GUILayout.Button("EndCommand"))
    //    {
    //        command.EndExecute();
    //    }
    //    if (GUILayout.Button("EndStarted"))
    //    {
    //        command.ActionObjCtrl.CompleteOneStarted();
    //    }
    //    if (GUILayout.Button("UnDoCommand"))
    //    {
    //        command.UnDoExecute();
    //    }
    //}
}
