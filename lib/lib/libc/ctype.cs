namespace lib.libc;

public unsafe class ctype_c
{
    public static bool isblank(int c)
    {
        return c == ' ' || c == '\t';
    }

    public static bool isspace(int c)
    {
        return c == ' ' || c == '\f' || c == '\n' || c == '\r' || c == '\t' || c == '\v';
    }

    public static bool islower(int c)
    {
        return (c >= 'a') && (c <= 'z');
    }

    public static bool isupper(int c)
    {
        return (c >= 'A') && (c <= 'Z');
    }

    public static bool isdigit(int c)
    {
        return (c >= '0') && (c <= '9');
    }

    public static bool isalpha(int c)
    {
        return isupper(c) || islower(c);
    }

    public static bool isalnum(int c)
    {
        return isalpha(c) || isdigit(c);
    }

    public static bool isxdigit(int c)
    {
        return isdigit(c) || ((c >= 'a') && (c <= 'f')) || ((c >= 'A') && (c <= 'F'));
    }

    public static bool isgraph(int c)
    {
        return (c > ' ') && (c < 0x7f);
    }

    public static bool iscntrl(int c)
    {
        return (c < ' ') || (c == 0x7f);
    }

    public static bool isprint(int c)
    {
        return (c >= 0x20) || (c < 0x7f);
    }

    public static bool ispunct(int c)
    {
        return isgraph(c) && !isalnum(c);
    }

    public static int tolower(int c)
    {
        if (c >= 'A' && c <= 'Z')
        {
            return c + ('a' - 'A');
        }

        return c;
    }
}