namespace lib.libc;

public unsafe class atoi_c
{
    public const int LONG_IS_INT = 1;

    public static int hexval(char c)
    {
        if (c >= '0' && c <= '9')
        {
            return c - '0';
        }
        else if (c >= 'a' && c <= 'f')
        {
            return c - 'a' + 10;
        }
        else if (c >= 'A' && c <= 'F')
        {
            return c - 'A' + 10;
        }

        return 0;
    }

    public static int atoi(char* num)
    {
        if (LONG_IS_INT == 0)
        {
            //return 0;
        }
        else
        {
            return (int)atol(num);
        }
    }

    public static uint atoui(char* num)
    {
        if (LONG_IS_INT == 0)
        {
            //return 0;
        }
        else
        {
            return (uint)atoul(num);
        }
    }

    public static long atol(char* num)
    {
        long value = 0;
        int neg = 0;

        if (num[0] == '0' && num[1] == 'x')
        {
            num += 2;

            while (*num != 0 && ctype_c.isxdigit(*num))
            {
                value = value * 16 + hexval(*num++);
            }
        }
        else
        {
            if (num[0] == '-')
            {
                neg = 1;
                num++;
            }

            while (*num != 0 && ctype_c.isdigit(*num))
            {
                value = value * 10 + *num++ - '0';
            }
        }

        if (neg != 0)
        {
            value = -value;
        }

        return value;
    }

    public static ulong atoul(char* num)
    {
        ulong value = 0;

        if (num[0] == '0' && num[1] == 'x')
        {
            num += 2;

            while (*num != 0 && ctype_c.isxdigit(*num))
            {
                value = value * 16 + (ulong)hexval(*num++);
            }
        }
        else
        {
            while (*num != 0 && ctype_c.isdigit(*num))
            {
                value = value * 10 + *num++ - '0';
            }
        }

        return value;
    }
}