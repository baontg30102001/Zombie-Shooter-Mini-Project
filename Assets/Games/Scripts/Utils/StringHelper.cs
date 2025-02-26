public static class StringHelper
{
    public static string[] SplitStrings(string str)
    {
        if (string.IsNullOrEmpty(str)) return null;

        string[] collection = str.Split("_");
        return collection;
    }
}