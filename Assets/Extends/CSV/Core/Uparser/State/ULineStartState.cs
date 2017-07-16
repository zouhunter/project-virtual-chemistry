using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class ULineStartState : UParserState
{
    public ULineStartState(UParserStateMachine machine) : base(machine)
    {
    }

    public override void AnyChar(char ch)
    {
        machine.context.AddChar(ch);
        machine.SetState(machine.uValueState);
    }

    public override void Comma()
    {
        machine.context.SetQuote();
        machine.context.AddChar(UParserStateMachine.CommaCharacter);
        machine.SetState(machine.uQuoteValueState);
    }

    public override void EndOfLine()
    {
        machine.context.AddLine();
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
