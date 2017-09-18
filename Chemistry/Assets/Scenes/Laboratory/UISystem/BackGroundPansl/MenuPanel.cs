using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using BundleUISystem;

public class MenuPanel :UIPanelTemp  {
    
	[SerializeField] private Button m_Setting; 
	[SerializeField] private Button m_Help; 
	[SerializeField] private Button m_Quit; 
	[SerializeField] private Button m_present; 
	[SerializeField] private Button m_medicine; 
	[SerializeField] private Button m_element; 
	[SerializeField] private Button m_prictice;
    [SerializeField] private Toggle warehouse;
	private void Awake()
	{
		m_Setting.onClick.AddListener(OnSettingClicked); 
		m_Help.onClick.AddListener(OnHelpClicked); 
		m_Quit.onClick.AddListener(OnQuitClicked); 
		m_present.onClick.AddListener(OnPresentClicked); 
		m_medicine.onClick.AddListener(OnMedicineClicked); 
		m_element.onClick.AddListener(OnElementClicked); 
		m_prictice.onClick.AddListener(OnPricticeClicked);
	}
    private void Start()
    {
        SceneMain.Current.RegisterEvent(AppConfig.EventKey.ClickEmpty, () => { warehouse.isOn = false; });
    }
    private void OnSettingClicked()
	{
        UIGroup.Open<SettingPanel>();
	}
	private void OnHelpClicked()
	{
        UIGroup.Open<PlayerHelpPanel>();
	}
	private void OnQuitClicked()
	{
        Application.Quit();
	}
	private void OnPresentClicked()
	{
        UIGroup.Open<ExperimentPanel>();
        SceneMain.Current.InvokeEvents(AppConfig.EventKey.ClickEmpty);
    }
    private void OnMedicineClicked()
	{
        UIGroup.Open("MedicinePanel");
        SceneMain.Current.InvokeEvents(AppConfig.EventKey.ClickEmpty);
    }
    private void OnElementClicked()
	{
        UIGroup.Open("ElementPanel");
        SceneMain.Current.InvokeEvents(AppConfig.EventKey.ClickEmpty);
    }
    private void OnPricticeClicked()
	{
        UIGroup.Open("ExperimentChoisePanel");
        SceneMain.Current.InvokeEvents(AppConfig.EventKey.ClickEmpty);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        SceneMain.Current.RemoveEvent(AppConfig.EventKey.ClickEmpty, () => { warehouse.isOn = false; });
    }
}