using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ParserContext
{
    private readonly StringBuilder _currentValue = new StringBuilder();
    private readonly List<string[]> _lines = new List<string[]>();
    private readonly List<string> _currentLine = new List<string>();

    public ParserContext()
    {
        MaxColumnsToRead = 1000;
    }

    public int MaxColumnsToRead { get; set; }

    public void AddChar(char ch)
    {
        _currentValue.Append(ch);
    }

    public void AddValue()
    {
        if (_currentLine.Count < MaxColumnsToRead)
            _currentLine.Add(_currentValue.ToString());
        _currentValue.Remove(0, _currentValue.Length);
    }

    public void AddLine()
    {
        _lines.Add(_currentLine.ToArray());
        _currentLine.Clear();
    }

    public List<string[]> GetAllLines()
    {
        if (_currentValue.Length > 0)
        {
            AddValue();
        }
        if (_currentLine.Count > 0)
        {
            AddLine();
        }
        return _lines;
    }
}
