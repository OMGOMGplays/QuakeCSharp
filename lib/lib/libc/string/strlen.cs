namespace lib.libc;

public unsafe class strlen_c
{
    public static int strlen(char* s)
    {
        int i;

        i = 0;

        while (s[i] != 0)
        {
            i += 1;
        }

        return i;
    }
}