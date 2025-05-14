public static class StringExtensions
{
    public static string Capitalize(this string input) =>
        string.Concat((char)(input[0] & 0xDF), input[1..]);

    public static string EnsureBoxed(this int input, int max = 8)
    {
        string str = input.ToString();

        if (str.Length > max)
        {
            return str.Substring(0, max - 1) + "+";
        }
        else
        {
            return str;
        }
    }

    public static string EnsureScored(this int input)
    {
        if (input >= 1000000000)
        {
            return (input / 1000000000).ToString() + "b";
        }
        else if (input >= 1000000)
        {
            return (input / 1000000).ToString() + "m";
        }
        else if (input >= 1000)
        {
            return (input / 1000).ToString() + "k";
        }
        else
        {
            return input.ToString();
        }
    }
}