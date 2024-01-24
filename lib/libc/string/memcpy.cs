namespace lib.libc;

public unsafe class memcpy_c
{
    public static long word;

    public const int lsize = sizeof(long);
    public const int lmask = lsize - 1;

    public static void* memcpy(object* dest, object* src, IntPtr count)
    {
        char* d = (char*)dest;
        char* s = (char*)src;
        int len;

        if (count == 0 || dest == src)
        {
            return dest;
        }

        if ((((long)d | (long)s) & lmask) != 0 || (count < lsize))
        {
            if ((((long)d ^ (long)s) & lmask) != 0 || (count < lsize))
            {
                len = (int)count;
            }
            else
            {
                len = lsize - (int)((long)d & lmask);
            }

            count -= len;

            for (; len > 0; len--)
            {
                *d++ = *s++;
            }
        }

        for (len = (int)(count / lsize); len > 0; len--)
        {
            *(long*)d = *(long*)s;
            d += lsize;
            s += lsize;
        }

        for (len = (int)(count & lmask); len > 0; len--)
        {
            *d++ = *s++;
        }

        return dest;
    }
}