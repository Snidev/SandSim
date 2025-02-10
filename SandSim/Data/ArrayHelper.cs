namespace SandSim.Data;

internal static class ArrayHelper
{
    public static void Expand<T>(ref T[] array, int to)
    {
        to++;
        if (array.Length > to)
            return;
        
        Array.Resize(ref array, Math.Max(to, array.Length * 2));
    }
}