using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class UnParserContext
{
    private readonly StringBuilder _currentValue = new StringBuilder();
    private readonly StringBuilder _lines = new StringBuilder();
    private readonly StringBuilder _currentLine = new StringBuilder();
    private bool haveQuote = false;


    public void SetQuote()
    {
        haveQuote = true;
    }

    public void AddChar(char ch)
    {
        _currentValue.Append(ch);
    }

    public void AddValue()
    {
        if (!haveQuote)
        {
            _currentLine.Append(_currentValue.ToString() + ",");
        }
        else
        {
            _currentLine.Append(ParserStateMachine.QuoteCharacter + _currentValue.ToString() + ParserStateMachine.QuoteCharacter + ",");
        }
        _currentValue.Remove(0, _currentValue.Length);
        haveQuote = false;
    }

    public void AddLine()
    {
        _lines.Append(_currentLine.ToString() +"\n");
        _currentLine.Remove(0, _currentLine.Length);
    }

    public string GetText()
    {
        if (_currentValue.Length > 0)
        {
            AddValue();
        }
        if (_currentLine.Length > 0)
        {
            AddLine();
        }
        return _lines.ToString();
    }
}
