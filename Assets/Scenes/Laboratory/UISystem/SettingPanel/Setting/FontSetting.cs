using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class FontSetting : SettingTemp
{
    [SerializeField]
    private Text _text;
    private int MaxSize;

    private void Awake()
    {
        if (_text == null)
            _text = GetComponent<Text>();
        MaxSize = _text.fontSize;
    }
    protected override void LoadSettingOnOpen(ExpSetting expSetting)
    {
        OnFontSizeChange(expSetting.fontSize);
    }

    protected override void RegistEventOnSettingChange(ExpSetting expSetting)
    {
        expSetting.cg_FontSize += OnFontSizeChange;
    }

    protected override void RemoveEventOnSettingChange(ExpSetting expSetting)
    {
        expSetting.cg_FontSize -= OnFontSizeChange;
    }

    private void OnFontSizeChange(float Size)
    {
       if(_text) _text.fontSize = Mathf.CeilToInt(MaxSize * Size);
    }

}
