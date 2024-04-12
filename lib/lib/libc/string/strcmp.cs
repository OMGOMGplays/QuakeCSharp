namespace lib.libc;

public unsafe class strcmp_c
{
    public static int strcmp(char* cs, char* ct)
    {
        char __res;

        while (true)
        {
            if ((__res = (char)(*cs - *ct++)) != 0 || *cs++ == 0)
            {
                break;
            }
        }

        return __res;
    }
}