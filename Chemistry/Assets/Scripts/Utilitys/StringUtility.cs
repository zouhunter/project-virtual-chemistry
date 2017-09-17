using UnityEngine;
using System;
using System.IO;

public class StringUtility
{
    public static bool IsChineseString(string input)
    {
        int chfrom = 0x4e00;
        int chend = 0x9fff;

        if (!string.IsNullOrEmpty(input))
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] < chfrom || input[i] > chend)
                    return false;
            }
            return true;
        }

        return false;
    }

    public static bool IsNumberString(string input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (!char.IsDigit(input[i]))
                    return false;
            }
            return true;
        }

        return false;
    }

    public static string[] SplitNewLine(string input)
    {
        Debug.Assert(!string.IsNullOrEmpty(input));
        var splitArray = input.Split(new char[] {'\n', '\r', '\t'}, StringSplitOptions.RemoveEmptyEntries);
        return splitArray;
    }

    public static bool IsPathString(string input)
    {
        if (!string.IsNullOrEmpty(input))
            return false;

        var invalidPathChars = Path.GetInvalidPathChars();
        foreach (var c in input)
        {
            foreach (var i in invalidPathChars)
            {
                if (c == i) return false;
            }
        }

        return true;
    }

    public static string RemoveSubString(string source, string remove)
    {
        if (source == null)
            throw new ArgumentNullException();

        if (string.IsNullOrEmpty(remove))
            return source;

        int index = source.IndexOf(remove);
        if (index == -1)
            return source;

        return source.Remove(index);
    }

    public static int ExtractIntFromBracketString(string input, char leftBracket, char rightBracket)
    {
        int leftBracketIndex = input.IndexOf(leftBracket);
        
        if (leftBracketIndex != 0)
            throw new FormatException();

        input = input.Remove(leftBracketIndex, 1);

        int rightBrackIndex = input.IndexOf(rightBracket);
        if (rightBrackIndex >= input.Length)
            throw new FormatException();

        input = input.Remove(rightBrackIndex, 1);
        int value = int.Parse(input);
        return value;
    }
}
