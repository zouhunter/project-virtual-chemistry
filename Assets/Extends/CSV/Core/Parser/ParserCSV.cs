using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


public static class ParserCSV {
    
    public static string[][] Parse(ParserStateMachine machine, TextReader reader)
    {
        if (machine.MaxColumnsToRead != 0)
            machine.context.MaxColumnsToRead = machine.MaxColumnsToRead;
        machine.SetState(machine.LineStartState);
        string next;
        while ((next = reader.ReadLine()) != null)
        {
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
            machine.EndOfLine();
        }
        List<string[]> allLines = machine.context.GetAllLines();
        if (machine.TrimTrailingEmptyLines && allLines.Count > 0)
        {
            bool isEmpty = true;
            for (int i = allLines.Count - 1; i >= 0; i--)
            {
                // ReSharper disable RedundantAssignment
                isEmpty = true;
                // ReSharper restore RedundantAssignment
                for (int j = 0; j < allLines[i].Length; j++)
                {
                    if (!String.IsNullOrEmpty(allLines[i][j]))
                    {
                        isEmpty = false;
                        break;
                    }
                }
                if (!isEmpty)
                {
                    if (i < allLines.Count - 1)
                        allLines.RemoveRange(i + 1, allLines.Count - i - 1);
                    break;
                }
            }
            if (isEmpty)
                allLines.RemoveRange(0, allLines.Count);
        }
        return allLines.ToArray();
    }

    public static string[][] Parse(string input)
    {
        ParserStateMachine machine = new ParserStateMachine();

        using (StringReader reader = new StringReader(input))
        {
            return Parse(machine, reader);
        }
    }
}
