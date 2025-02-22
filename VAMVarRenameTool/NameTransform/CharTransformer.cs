using System.Collections.Generic;

public class CharTransformer
{
    private static readonly Dictionary<char, char> CharMap = new()
    {
        { ' ', '_' },
        { '*', '_' },
        { '|', '_' },
        { '\'', '_' },
        { ':', '_' },
        { '"', '_' },
        { '.', '_' },
        { '<', '_' },
        { '>', '_' },
        { '/', '_' },
        { '?', '_' },
    };

    public string Transform(string input)
    {
        var result = new char[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            if (CharMap.TryGetValue(input[i], out var newChar))
            {
                result[i] = newChar;
            }
            else
            {
                result[i] = input[i];
            }
        }
        return new string(result);
    }
}
