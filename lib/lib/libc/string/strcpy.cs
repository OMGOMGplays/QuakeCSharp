namespace lib.libc;

public unsafe class strcpy_c
{
    public static char* strcpy(char* dest, char* src)
    {
        char* tmp = dest;

        while ((*dest++ = *src++) != '\0')
        {

        }

        return tmp;
    }
}