using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class UParserStateMachine {
    public UParserState uLineStartState;
    public UParserState uValueState;
    public UParserState uValueStartState;
    public UParserState uQuoteValueState;


    public UnParserContext context;
    private UParserState currState;
    public const char CommaCharacter = ',';
    public const char QuoteCharacter = '"';
    public UParserStateMachine()
    {
        context = new global::UnParserContext();
        uLineStartState = new ULineStartState(this);
        uValueState = new UValueState(this);
        uValueStartState = new UValueStartState(this);
        uQuoteValueState = new UQuoteValueState(this);
    }

    public void SetState(UParserState currState)
    {
        this.currState = currState;
    }


    public void AnyChar(char ch)
    {
        currState.AnyChar(ch);
    }
    public void Quote()
    {
        currState.Quote();
    }
    public void Comma()
    {
        currState.Comma();
    }
    public void EndValue()
    {
        currState.EndValue();
    }
    public void EndOfLine()
    {
        currState.EndOfLine();
    }
}
