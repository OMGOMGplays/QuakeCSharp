namespace lib.libc;

public unsafe class strncpy_c
{
    public static char* strncpy(char* dest, char* src, IntPtr count)
    {
        char* tmp = dest;

        IntPtr i;

        for (i = 0; i++ < count && (*dest++ = *src++) != '\0';)
        {
            ;
        }

        for (; i < count; i++)
        {
            *dest++ = '\0';
        }

        return tmp;
    }
}