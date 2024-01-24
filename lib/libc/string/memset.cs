namespace lib.libc;

public unsafe class memset_c
{
    public static void* memset(object* s, int c, IntPtr count)
    {
        char* xs = (char*)s;
        IntPtr len = (-(IntPtr)s) & (sizeof(IntPtr) - 1);
        IntPtr cc = c & 0xff;

        if (count > len)
        {
            count -= len;
            cc |= cc << 8;
            cc |= cc << 16;

            if (sizeof(IntPtr) == 8)
            {
                cc |= (int)cc << 32;
            }

            for (; len > 0; len--)
            {
                *xs++ = c;
            }

            for (len = count / sizeof(IntPtr); len > 0; len--)
            {
                *((IntPtr*)xs) = (IntPtr)cc;
                xs += sizeof(IntPtr);
            }

            count &= sizeof(IntPtr) - 1;
        }

        for (; count > 0; count--)
        {
            *xs++ = c;
        }

        return s;
    }
}