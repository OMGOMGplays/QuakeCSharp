global using System;
global using System.IO;
global using System.Linq;

public unsafe class lib_c
{
    public static void Main()
    {
        return;
    }

    public static char* StringToChar(string input)
    {
        char[] input_arr = input.ToCharArray();
        char* input_char = null;

        for (int i = 0; i < input_arr.Length; i++)
        {
            input_char[i] = input_arr[i];
        }

        return input_char;
    }
}