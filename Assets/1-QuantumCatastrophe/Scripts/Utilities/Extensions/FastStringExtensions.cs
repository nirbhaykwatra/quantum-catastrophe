using System;
using System.Text;

public static class FastStringExtensions
{
    public static string SplitPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        StringBuilder sb = new StringBuilder(input.Length * 2);
        sb.Append(input[0]); // Add the first character as-is

        for (int i = 1; i < input.Length; i++)
        {
            char current = input[i];
            
            // If current is uppercase and the previous character wasn't a space or uppercase
            if (char.IsUpper(current) && !char.IsWhiteSpace(input[i - 1]) && !char.IsUpper(input[i - 1]))
            {
                sb.Append(' ');
            }
            sb.Append(current);
        }

        return sb.ToString();
    }
}
