namespace lib.libc;

public unsafe class strncmp_c
{
    public static int strncmp(char* cs, char* ct, IntPtr count)
    {
        char __res = '\0';

        while (count > 0)
        {
            if ((__res = (char)(*cs - *ct++)) != 0 || *cs++ == 0)
            {
                break;
            }

            count--;
        }

        return __res;
    }

    public static int strncmp(char* cs, string ct, IntPtr count)
    {
        return strncmp(cs, Quake.common_c.StringToChar(ct), count);
    }
}