using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class UValueState : UParserState
{
    public UValueState(UParserStateMachine machine) : base(machine)
    {
    }

    public override void AnyChar(char ch)
    {
        machine.context.AddChar(ch);
    }

    public override void Comma()
    {
        machine.context.AddChar(UParserStateMachine.CommaCharacter);
        machine.context.SetQuote();//需要最外层的""
        machine.SetState(machine.uQuoteValueState);
    }

    public override void EndOfLine()
    {
        machine.context.AddLine();
        machine.SetState(machine.uLineStartState);
    }

    public override void EndValue()
    {
        machine.context.AddValue();
        machine.SetState(machine.uValueStartState);
    }

    public override void Quote()
    {
        machine.context.SetQuote();
        machine.context.AddChar(UParserStateMachine.QuoteCharacter);
        machine.context.AddChar(UParserStateMachine.QuoteCharacter);
        machine.SetState(machine.uQuoteValueState);
    }
}
