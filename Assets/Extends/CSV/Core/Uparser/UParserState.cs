using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class UParserState {
    protected UParserStateMachine machine;
    public UParserState(UParserStateMachine machine){
        this.machine = machine;
    }
    public abstract void AnyChar(char ch);
    public abstract void Quote();
    public abstract void Comma();
    public abstract void EndValue();
    public abstract void EndOfLine();
}
