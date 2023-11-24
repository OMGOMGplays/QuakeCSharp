using quakedef;
using System.Runtime.InteropServices;

namespace common;

public class common_c
{
    public const int NUM_SAFE_ARGVS = 7;

    public static string[] largv = new string[quakedef_c.MAX_NUM_ARGVS + NUM_SAFE_ARGVS + 1];
    public static string argvdummy = " ";

    public static string[] safeargvs = { "-stdvid", "-nolan", "-nosound", "-nocdaudio", "-nojoy", "-nomouse", "-dibonly" };

    public cvar_s registered = new cvar_s { "registered", 0 };
    public cvar_s cmdline = new cvar_s { "cmdline", "0", false, true };

    bool com_modified; // Set true if using non-id files

    bool proghack;

    int static_registered = 1; // Only for startup check, then set

    public void COM_InitFileSystem() { }

    // If a packfile directory differs from this, it is assumed to be hacked
    public static int PAK0_COUNT = 339;
    public static int PAK0_CRC = 32981;

    string[] com_token = new string[1024];
    int com_argc;
    string com_argv;

    public static int CMDLINE_LENGTH = 256;
    string[] cmd_cmdline = new string[CMDLINE_LENGTH];

    bool standard_quake = true, rogue, hipnotic;

    static ushort[] pop =
    {
        0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
        0x0000, 0x0000, 0x6600, 0x0000, 0x0000, 0x0000, 0x6600, 0x0000,
        0x0000, 0x0066, 0x0000, 0x0000, 0x0000, 0x0000, 0x0067, 0x0000,
        0x0000, 0x6665, 0x0000, 0x0000, 0x0000, 0x0000, 0x0065, 0x6600,
        0x0063, 0x6561, 0x0000, 0x0000, 0x0000, 0x0000, 0x0061, 0x6563,
        0x0064, 0x6561, 0x0000, 0x0000, 0x0000, 0x0000, 0x0061, 0x6564,
        0x0064, 0x6564, 0x0000, 0x6469, 0x6969, 0x6400, 0x0064, 0x6564,
        0x0063, 0x6568, 0x6200, 0x0064, 0x6864, 0x0000, 0x6268, 0x6563,
        0x0000, 0x6567, 0x6963, 0x0064, 0x6764, 0x0063, 0x6967, 0x6500,
        0x0000, 0x6266, 0x6769, 0x6a68, 0x6768, 0x6a69, 0x6766, 0x6200,
        0x0000, 0x0062, 0x6566, 0x6666, 0x6666, 0x6666, 0x6562, 0x0000,
        0x0000, 0x0000, 0x0062, 0x6364, 0x6664, 0x6362, 0x0000, 0x0000,
        0x0000, 0x0000, 0x0000, 0x0062, 0x6662, 0x0000, 0x0000, 0x0000,
        0x0000, 0x0000, 0x0000, 0x0061, 0x6661, 0x0000, 0x0000, 0x0000,
        0x0000, 0x0000, 0x0000, 0x0000, 0x6500, 0x0000, 0x0000, 0x0000,
        0x0000, 0x0000, 0x0000, 0x0000, 0x6400, 0x0000, 0x0000, 0x0000,
    };

    public void ClearLink(link_t l)
    {
        l.prev = l.next = l;
    }

    public void RemoveLink(link_t l)
    {
        l.next.prev = l.prev;
        l.prev.next = l.next;
    }

    public void InsertLinkBefore(link_t l, link_t before)
    {
        l.next = before;
        l.prev = before;
        l.prev.next = l;
        l.next.prev = l;
    }

    public void InsertLinkAfter(link_t l, link_t after)
    {
        l.next = after.next;
        l.prev = after;
        l.prev.next = l;
        l.next.prev = l;
    }

    public void Q_memset(object dest, int fill, int count)
    {
        if ((((long)dest | count) & 3) == 0)
        {
            count >>= 2;
            fill = fill | (fill << 8) | (fill << 16) | (fill << 24);
            for (int i = 0; i < count; i++)
            {
                ((int[])dest)[i] = fill;
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                ((byte[])dest)[i] = (byte)fill;
            }
        }
    }

    public void Q_memcpy(object dest, object src, int count)
    {
        if ((((long)dest | (long)src | count) & 3) == 0)
        {
            count >>= 2;
            for (int i = 0; i < count; i++)
            {
                ((int[])dest)[i] = ((int[])src)[i];
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                ((byte[])dest)[i] = ((byte[])src)[i];
            }
        }
    }

    public int Q_memcmp(object m1, object m2, int count)
    {
        while (count != 0)
        {
            count--;
            if (((byte[])m1)[count] != ((byte[])m2)[count])
            {
                return -1;
            }
        }

        return 0;
    }

    public unsafe void Q_strcpy(char* dest, char* src)
    {
        int i = 0;

        while (src[i] != '\0')
        {
            dest[i] = src[i];
            i++;
        }

        dest[i] = '\0';
    }

    public void Q_strncpy(char[] dest, char[] src, int count)
    {
        int i = 0;

        while (src[i] != '\0' && count > 0)
        {
            dest[i] = src[i];
            i++;
            count--;
        }

        if (count != 0)
        {
            dest[i] = '\0';
        }
    }

    public unsafe int Q_strlen(char* str)
    {
        int count = 0;

        while (str[count] > 0)
        {
            count++;
        }

        return count;
    }

    public unsafe char* Q_strrchr(char* s, char c)
    {
        int len = Q_strlen(s);
        s += len;
        while (len-- > 0)
        {
            if (*--s == c)
            {
                return s;
            }
        }

        return null;
    }

    public unsafe void Q_strcat(char* dest, char* src)
    {
        dest += Q_strlen(dest);
        Q_strcpy(dest, src);
    }

    public unsafe int Q_strcmp(char* s1, char* s2)
    {
        while (true)
        {
            if (*s1 != *s2)
            {
                return -1; // Strings are not equal
            }

            if (*s1 == *s2)
            {
                return 0; // Strings are equal
            }

            s1++;
            s2++;
        }

        return -1; // Unreachable, still kept, cause why not!!
    }

    public unsafe int Q_strncmp(char* s1, char* s2, int count)
    {
        while (true)
        {
            if (count-- < 0)
            {
                return 0;
            }

            if (*s1 != *s2)
            {
                return -1; // Strings are not equal
            }

            if (*s1 == *s2)
            {
                return 0; // Strings are equal
            }

            s1++;
            s2++;
        }

        return -1; // Also unreachable
    }

    public unsafe int Q_strncasecmp(char* s1, char* s2, int n)
    {
        int c1, c2;

        while (true)
        {
            c1 = *s1++;
            c2 = *s2++;

            if (n-- < 0)
            {
                return 0; // Strings are equal until the end point
            }

            if (c1 != c2)
            {
                if (c1 >= 'a' && c1 <= 'z')
                {
                    c1 -= ('a' - 'A');
                }

                if (c2 >= 'a' && c2 <= 'z')
                {
                    c2 -= ('a' - 'A');
                }

                if (c1 != c2)
                {
                    return -1; // Strings are not equal
                }
            }

            if (c1 == c2)
            {
                return 0; // Strings are equal
            }
        }

        return -1;
    }

    public unsafe int Q_strcasecmp(char* s1, char* s2)
    {
        return Q_strncasecmp(s1, s2, 99999);
    }

    public unsafe int Q_atoi(char* str)
    {
        int val;
        int sign;
        int c;

        if (*str == '-')
        {
            sign = -1;
            str++;
        }
        else
        {
            sign = 1;
        }

        val = 0;

        //
        // Check for hex
        //
        if (str[0] == '0' && (str[1] == 'x' || str[1] == 'X'))
        {
            str += 2;

            while (true)
            {
                c = *str++;

                if (c >= '0' && c <= '9')
                {
                    val = (val << 4) + c - '0';
                }
                else if (c >= 'a' && c <= 'f')
                {
                    val = (val << 4) + c - 'a' + 10;
                }
                else if (c >= 'A' && c <= 'F')
                {
                    val = (val << 4) + c - 'A' + 10;
                }
                else
                {
                    return val * sign;
                }
            }
        }

        //
        // Check for character
        //
        if (str[0] == '\'')
        {
            return sign * str[1];
        }

        //
        // Assume decimal
        //
        while (true)
        {
            c = *str++;

            if (c < '0' || c > '9')
            {
                return val * sign;
            }

            val = val * 10 + c - '0';
        }

        return 0;
    }

    public unsafe float Q_atof(char* str)
    {
        double val;
        int sign;
        int c;
        int _decimal, total;

        if (*str == '-')
        {
            sign = -1;
            str++;
        }
        else
        {
            sign = 1;
        }

        val = 0;

        //
        // Check for hex
        //
        if (str[0] == '0' && (str[1] == 'x' || str[1] == 'X'))
        {
            str += 2;

            while (true)
            {
                c = *str++;

                if (c >= '0' && c <= '9')
                {
                    val = (val * 16) + c - '0';
                }
                else if (c >= 'a' && c <= 'f')
                {
                    val = (val * 16) + c - 'a' + 10;
                }
                else if (c >= 'A' && c <= 'F')
                {
                    val = (val * 16) + c - 'A' + 10;
                }
                else
                {
                    return (float)val * sign;
                }
            }
        }

        //
        // Check for character
        //
        if (str[0] == '\'')
        {
            return sign * str[1];
        }

        //
        // Assume decimal
        //
        _decimal = 1;
        total = 0;

        while (true)
        {
            c = *str++;

            if (c == '.')
            {
                _decimal = total;
                continue;
            }

            if (c < '0' || c > '9')
            {
                break;
            }

            val = val * 10 + c - '0';
            total++;
        }

        if (_decimal == -1)
        {
            return (float)val * sign;
        }

        while (total > _decimal)
        {
            val /= 10;
            total--;
        }

        return (float)val * sign;
    }

    bool bigendien;

    delegate short BigShort(short l);
    delegate short LittleShort(short l);
    delegate int BigLong(int l);
    delegate int LittleLong(int l);
    delegate float BigFloat(float l);
    delegate float LittleFloat(float l);

    public short ShortSwap(short l)
    {
        byte b1, b2;

        b1 = (byte)(l & 255);
        b2 = (byte)((l >> 8) & 255);

        return (short)((b1 << 8) + b2);
    }

    public short ShortNoSwap(short l)
    {
        return l;
    }

    public int LongSwap(int l)
    {
        byte b1, b2, b3, b4;

        b1 = (byte)(l & 255);
        b2 = (byte)((l >> 8) & 255);
        b3 = (byte)((l >> 16) & 255);
        b4 = (byte)((l >> 24) & 255);

        return (b1 << 24) + (b2 << 16) + (b3 << 8) + b4;
    }

    public int LongNoSwap(int l)
    {
        return l;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct FloatSwap_Union
    {
        [FieldOffset(0)]
        public float f;

        [FieldOffset(0)]
        public byte b1;

        [FieldOffset(1)]
        public byte b2;

        [FieldOffset(2)]
        public byte b3;

        [FieldOffset(3)]
        public byte b4;
    }

    public float FloatSwap(float f)
    {
        FloatSwap_Union dat1, dat2 = new();

        dat1.f = f;
        dat2.b1 = dat1.b4;
        dat2.b2 = dat1.b3;
        dat2.b3 = dat1.b2;
        dat2.b4 = dat1.b1;
        return dat2.f;
    }

    public float FloatNoSwap(float f)
    {
        return f;
    }

    public unsafe void MSG_WriteChar(sizebuf_t* sb, int c)
    {
        byte* buf;

#if PARANOID
        if (c < -128 || c > 127)
        {
            Sys_Error("MSG_WriteChar: range error");
        }
#endif

        buf = SZ_GetSpace(sb, 1);
        buf[0] = (byte)c;
    }

    public unsafe void MSG_WriteByte(sizebuf_t* sb, int c)
    {
        byte* buf;

#if PARANOID
        if (c < 0 || c > 255) 
        {
            Sys_Error("MSG_WriteByte: range error");
        }
#endif

        buf = SZ_GetSpace(sb, 1);
        buf[0] = (byte)c;
    }

    public unsafe void MSG_WriteShort(sizebuf_t* sb, int c)
    {
        byte* buf;

#if PARANOID
        if (c < ((short)0x8000) || c > (short)0x7fff) 
        {
            Sys_Error("MSG_WriteShort: range error");
        }
#endif

        buf = SZ_GetSpace(sb, 2);
        buf[0] = (byte)(c & 0xff);
        buf[1] = (byte)(c >> 8);
    }

    public unsafe void MSG_WriteLong(sizebuf_t* sb, int c)
    {
        byte* buf;

        buf = SZ_GetSpace(sb, 4);
        buf[0] = (byte)(c & 0xff);
        buf[1] = (byte)((c >> 8) & 0xff);
        buf[2] = (byte)((c >> 16) & 0xff);
        buf[3] = (byte)(c >> 24);
    }

    struct MSG_WriteFloat_Union
    {
        public float f;
        public int l;
    }

    public unsafe void MSG_WriteFloat(sizebuf_t* sb, float f)
    {
        MSG_WriteFloat_Union dat = new();

        dat.f = f;
        dat.l = LittleLong(dat.l);

        SZ_Write(sb, &dat.l, 4);
    }


}