using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class ParserStateMachine
{
    public LineStartState LineStartState;
    public ValueStartState ValueStartState;
    public ValueState ValueState;
    public QuotedValueState QuotedValueState;
    public QuoteState QuoteState;

    private ParserState currState;
    public ParserContext context;
    public const char CommaCharacter = ',';
    public const char QuoteCharacter = '"';
    public bool TrimTrailingEmptyLines;
    public int MaxColumnsToRead;
    public ParserStateMachine(bool TrimTrailingEmptyLines = false,int MaxColumnsToRead = 0)
    {
        this.TrimTrailingEmptyLines = TrimTrailingEmptyLines;
        this.MaxColumnsToRead = MaxColumnsToRead;
        context = new global::ParserContext();
        LineStartState = new LineStartState(this);
        ValueStartState = new ValueStartState(this);
        ValueState = new ValueState(this);
        QuotedValueState = new QuotedValueState(this);
        QuoteState = new QuoteState(this);
    }

    public void SetState(ParserState currState)
    {
        this.currState = currState;
    }

    public void AnyChar(char ch)
    {
        currState.AnyChar(ch);
    }

    public void Comma()
    {
        currState.Comma();
    }

    public void EndOfLine()
    {
        currState.EndOfLine();
    }

    public void Quote()
    {
        currState.Quote();
    }
}
