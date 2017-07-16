using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class UParserCSV {

    public static string UParser(UParserStateMachine machine, string[][] classValue)
    {
        machine.SetState(machine.uLineStartState);
        string next;
        for (int i = 0; i < classValue.Length; i++)
        {
            for (int j = 0; j < classValue[i].Length; j++)
            {
                next = classValue[i][j];
                foreach (char ch in next)
                {
                    switch (ch)
                    {
                        case ParserStateMachine.CommaCharacter:
                            machine.Comma();
                            break;
                        case ParserStateMachine.QuoteCharacter:
                            machine.Quote();
                            break;
                        default:
                            machine.AnyChar(ch);
                            break;
                    }

                }
                machine.EndValue();
            }
            machine.EndOfLine();
        }
        return machine.context.GetText();
    }

    public static string UParser(string[][] classValue)
    {
        UParserStateMachine mechine = new global::UParserStateMachine();
        return UParser(mechine, classValue);
    }
}
