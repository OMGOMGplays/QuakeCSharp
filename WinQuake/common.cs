namespace Quake;

public unsafe class common_c
{
	public const int NUM_SAFE_ARGVS = 7;

	public static string largv = null;
	public static string argvdummy = " ";

	public static string[] safeargvs = { "-stdvid", "-nolan", "-nosound", "-nocdaudio", "-nojoy", "-nomouse", "-dibonly" };

	public static cvar_c.cvar_t registered = new cvar_c.cvar_t { name = "registered", value = (char)0 };
	public static cvar_c.cvar_t cmdline = new cvar_c.cvar_t { name = "cmdline", value = (char)0, server = true };

	public static bool com_modified; // Set true if using non-id files

	public static bool proghack;

	static int static_registered = 1; // Only for startup check, then set

	//public void COM_InitFileSystem() { }

	// If a packfile directory differs from this, it is assumed to be hacked
	public const int PAK0_COUNT = 339;
	public const int PAK0_CRC = 32981;

	public static string[] com_token = new string[1024];
	public static int com_argc;
	public static char* com_argv;

	public const int CMDLINE_LENGTH = 256;
	string[] cmd_cmdline = new string[CMDLINE_LENGTH];

	public static bool standard_quake = true, rogue, hipnotic;

	public static ushort[] pop =
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

	public struct sizebuf_t
	{
		public bool allowoverflow;
		public bool overflowed;
		public int* data;
		public int maxsize;
		public int cursize;
	}

	public struct link_t
	{
		public link_t* prev, next;
	}

	public const int Q_MAXCHAR = 0x7f;
	public const int Q_MAXSHORT = 0x7fff;
	public const int Q_MAXINT = 0x7fffffff;
	public const int Q_MAXLONG = 0x7fffffff;
	public const int Q_MAXFLOAT = 0x7fffffff;

	public const int Q_MINCHAR = 0x80;
	public const int Q_MINSHORT = 0x8000;
	public const int Q_MININT = 0x8000000;
	public const int Q_MINLONG = 0x8000000;
	public const int Q_MINFLOAT = 0x7fffffff;

	public static void ClearLink(link_t* l)
	{
		l->prev = l->next = l;
	}

	public static void RemoveLink(link_t l)
	{
		l.next->prev = l.prev;
		l.prev->next = l.next;
	}

	public static void InsertLinkBefore(link_t* l, link_t* before)
	{
		l->next = before;
		l->prev = before;
		l->prev->next = l;
		l->next->prev = l;
	}

	public void InsertLinkAfter(link_t* l, link_t* after)
	{
		l->next = after->next;
		l->prev = after;
		l->prev->next = l;
		l->next->prev = l;
	}

	public static void Q_memset(object dest, int fill, int count)
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

	public static void Q_memcpy(object dest, object src, int count)
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

	public static int Q_memcmp(object m1, object m2, int count)
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

	public static void Q_strcpy(string dest, string src)
	{
		int i = 0;

		while (src != null)
		{
			dest = src;
			i++;
		}

		dest = null;
	}

	public static void Q_strncpy(char[] dest, char[] src, int count)
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

	public static int Q_strlen(string str)
	{
		int count = 0;

		while (str[count] > 0)
		{
			count++;
		}

		return count;
	}

	public static int Q_strlen(char* str)
	{
		return Q_strlen(str->ToString());
	}

	public static string Q_strrchr(string s, char c)
	{
		int len = Q_strlen(s);
		s += len;
		while (len-- > 0)
		{
			if (s.Length == c)
			{
				return s;
			}
		}

		return null;
	}

	public static void Q_strcat(string dest, string src)
	{
		dest += Q_strlen(dest);
		Q_strcpy(dest, src);
	}

	public static void Q_strcat(char* dest, char* src)
	{
		Q_strcat(dest->ToString(), src->ToString());
	}

	public static bool Q_strcmp(string s1, string s2)
	{
		while (true)
		{
			if (s1 != s2)
			{
				return false; // Strings are not equal
			}

			if (s1 == s2)
			{
				return true; // Strings are equal
			}

			//s1++;
			//s2++;
		}

		return false; // Unreachable, still kept, cause why not!!
	}

	public static bool Q_strcmp(char* s1, char* s2)
	{
		return Q_strcmp(s1->ToString(), s2->ToString());
	}

	public static bool Q_strcmp(char s1, char* s2)
	{
		return Q_strcmp(s1.ToString(), s2->ToString());
	}

	public static int Q_strncmp(char* s1, char* s2, int count)
	{
		while (true)
		{
			if (count-- < 0)
			{
				return 0;
			}

			if (s1 != s2)
			{
				return -1; // Strings are not equal
			}

			if (s1 == s2)
			{
				return 0; // Strings are equal
			}

			s1++;
			s2++;
		}

		return -1; // Also unreachable
	}

	public static int Q_strncasecmp(char* s1, string s2, int n)
	{
		int c1, c2;

		while (true)
		{
			c1 = s1.Length + 1;
			c2 = s2.Length + 1;

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

	public static int Q_strcasecmp(string s1, string s2)
	{
		return Q_strncasecmp(s1, s2, 99999);
	}

	public static int Q_atoi(string str)
	{
		int val;
		int sign;
		int c;

		if (str == "-")
		{
			sign = -1;
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
				c = str.Length + 1;

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
			c = str.Length + 1;

			if (c < '0' || c > '9')
			{
				return val * sign;
			}

			val = val * 10 + c - '0';
		}

		return 0;
	}

	public static float Q_atof(char* str)
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

	static bool bigendien;

	public delegate short ShortConverter(short value);
	public delegate int IntConverter(int value);
	public delegate float FloatConverter(float value);

	public static ShortConverter BigShort = ShortSwap;
	public static ShortConverter LittleShort = ShortNoSwap;
	public static IntConverter BigLong = LongSwap;
	public static IntConverter LittleLong = LongNoSwap;
	public static FloatConverter BigFloat = FloatSwap;
	public static FloatConverter LittleFloat = FloatNoSwap;

	public static short ShortSwap(short l)
	{
		byte b1, b2;

		b1 = (byte)(l & 255);
		b2 = (byte)((l >> 8) & 255);

		return (short)((b1 << 8) + b2);
	}

	public static short ShortNoSwap(short l)
	{
		return l;
	}

	public static int LongSwap(int l)
	{
		byte b1, b2, b3, b4;

		b1 = (byte)(l & 255);
		b2 = (byte)((l >> 8) & 255);
		b3 = (byte)((l >> 16) & 255);
		b4 = (byte)((l >> 24) & 255);

		return (b1 << 24) + (b2 << 16) + (b3 << 8) + b4;
	}

	public static int LongNoSwap(int l)
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

	public static float FloatSwap(float f)
	{
		FloatSwap_Union dat1, dat2 = new();

		dat1.b1 = 0;
		dat1.b2 = 0;
		dat1.b3 = 0;
		dat1.b4 = 0;

		dat1.f = f;
		dat2.b1 = dat1.b4;
		dat2.b2 = dat1.b3;
		dat2.b3 = dat1.b2;
		dat2.b4 = dat1.b1;
		return dat2.f;
	}

	public static float FloatNoSwap(float f)
	{
		return f;
	}

	public static void MSG_WriteChar(sizebuf_t sb, int c)
	{
		int buf;
		byte* bufs;

#if PARANOID
        if (c < -128 || c > 127)
        {
            sys_win.Sys_Error("MSG_WriteChar: range error");
        }
#endif

		buf = SZ_GetSpace(sb, 1);
		bufs = (byte*)buf;
		bufs[0] = (byte)c;
	}

	public static void MSG_WriteByte(sizebuf_t sb, int c)
	{
		int buf;
		byte* bufs;

#if PARANOID
        if (c < 0 || c > 255) 
        {
            sys_win.Sys_Error("MSG_WriteByte: range error");
        }
#endif

		buf = SZ_GetSpace(sb, 1);
		bufs = (byte*)buf;
		bufs[0] = (byte)c;
	}

	public static void MSG_WriteShort(sizebuf_t sb, int c)
	{
		int buf;
		byte* bufs;

#if PARANOID
        if (c < ((short)0x8000) || c > (short)0x7fff) 
        {
            sys_win.Sys_Error("MSG_WriteShort: range error");
        }
#endif

		buf = SZ_GetSpace(sb, 2);
		bufs = (byte*)buf;
		bufs[0] = (byte)(c & 0xff);
		bufs[1] = (byte)(c >> 8);
	}

	public static void MSG_WriteLong(sizebuf_t sb, int c)
	{
		int buf;
		byte* bufs;

		buf = SZ_GetSpace(sb, 4);
		bufs = (byte*)buf;
		bufs[0] = (byte)(c & 0xff);
		bufs[1] = (byte)((c >> 8) & 0xff);
		bufs[2] = (byte)((c >> 16) & 0xff);
		bufs[3] = (byte)(c >> 24);
	}

	struct MSG_WriteFloat_Union
	{
		public float f;
		public int l;
	}

	public static void MSG_WriteFloat(sizebuf_t sb, float f)
	{
		MSG_WriteFloat_Union dat = new();

		dat.f = f;
		dat.l = LittleLong(dat.l);

		SZ_Write(sb, dat.l, 4);
	}

	public static void MSG_WriteString(sizebuf_t sb, char s)
	{
		if (s == null)
		{
			SZ_Write(sb, 0, 1);
		}
		else
		{
			string sstr = Marshal.PtrToStringAnsi((IntPtr)s);

			SZ_Write(sb, s, Q_strlen(sstr) + 1);
		}
	}

	public static void MSG_WriteCoord(sizebuf_t sb, float f)
	{
		MSG_WriteShort(sb, (int)(f * 8));
	}

	public static void MSG_WriteAngle(sizebuf_t sb, float f)
	{
		MSG_WriteByte(sb, ((int)f * 256 / 360) & 255);
	}

	//
	// Reading functions
	//
	public static int msg_readcount;
	public static bool msg_badread;

	public static void MSG_BeginReading()
	{
		msg_readcount = 0;
		msg_badread = false;
	}

	public static int MSG_ReadChar()
	{
		int c;

		if (msg_readcount + 1 > net_message.cursize)
		{
			msg_badread = true;
			return -1;
		}

		c = (sbyte)net_message.data[msg_readcount];
		msg_readcount++;

		return c;
	}

	public static int MSG_ReadByte()
	{
		int c;

		if (msg_readcount + 1 > net_message.cursize)
		{
			msg_badread = true;
			return -1;
		}

		c = (uint)net_message.data[msg_readcount];
		msg_readcount++;

		return c;
	}

	public static int MSG_ReadShort()
	{
		int c;

		if (msg_readcount + 1 > net_message.cursize)
		{
			msg_badread = true;
			return -1;
		}

		c = net_message.data[msg_readcount]
			+ (net_message.data[msg_readcount + 1] << 8)
			+ (net_message.data[msg_readcount + 2] << 16)
			+ (net_message.data[msg_readcount + 3] << 24);

		msg_readcount += 4;

		return c;
	}

	[StructLayout(LayoutKind.Explicit)]
	struct MSG_ReadFloat_Union
	{
		[FieldOffset(0)]
		public float f;

		[FieldOffset(0)]
		public int l;

		[FieldOffset(0)]
		public byte b1;

		[FieldOffset(1)]
		public byte b2;

		[FieldOffset(2)]
		public byte b3;

		[FieldOffset(3)]
		public byte b4;
	}

	public static float MSG_ReadFloat()
	{
		MSG_ReadFloat_Union dat = new();

		dat.b1 = net_message.data[msg_readcount];
		dat.b2 = net_message.data[msg_readcount + 1];
		dat.b3 = net_message.data[msg_readcount + 2];
		dat.b4 = net_message.data[msg_readcount + 3];
		msg_readcount += 4;

		dat.l = LittleLong(dat.l);

		return dat.f;
	}

	public static char* MSG_ReadString()
	{
		StringBuilder stringBuilder = new();
		int c;

		do
		{
			c = MSG_ReadChar();
			if (c == -1 || c == 0)
			{
				break;
			}

			stringBuilder.Append((char)c);
		} while (stringBuilder.Length < 2047);

		return stringBuilder.ToString();
	}

	public static float MSG_ReadCoord()
	{
		return MSG_ReadShort() * (1.0f / 8);
	}

	public static float MSG_ReadAngle()
	{
		return MSG_ReadChar() * (360.0f / 256);
	}

	public static void SZ_Alloc(sizebuf_t buf, int startsize)
	{
		if (startsize < 256)
		{
			startsize = 256;
		}

		buf.data = (int*)zone_c.Hunk_AllocName(startsize, "sizebuf");
		buf.maxsize = startsize;
		buf.cursize = 0;
	}

	public static void SZ_Free(sizebuf_t buf)
	{
		buf.cursize = 0;
	}

	public static void SZ_Clear(sizebuf_t buf)
	{
		buf.cursize = 0;
	}

	public static int SZ_GetSpace(sizebuf_t buf, int length)
	{
		int data;

		if (buf.cursize + length > buf.maxsize)
		{
			if (!buf.allowoverflow)
			{
				sys_win_c.Sys_Error(StringToChar("SZ_GetSpace: overflow without allowoverflow set"));
			}

			if (length > buf.maxsize)
			{
				sys_win_c.Sys_Error(StringToChar($"SZ_GetSpace: {length} is > full buffer size"));
			}

			buf.overflowed = true;
			console_c.Con_Printf("SZ_GetSpace: overflow");
			SZ_Clear(buf);
		}

		data = *buf.data + buf.cursize;
		buf.cursize += length;

		return data;
	}

	public static void SZ_Write(sizebuf_t buf, int data, int length)
	{
		Q_memcpy(SZ_GetSpace(buf, length), data, length);
	}

	public static void SZ_Print(sizebuf_t buf, string data)
	{
		int len;
		len = Q_strlen(data) + 1;

		if (*buf.data == buf.cursize - 1)
		{
			Q_memcpy((byte)SZ_GetSpace(buf, len), data.ToCharArray(), len);
		}
		else
		{
			Q_memcpy((byte)SZ_GetSpace(buf, len - 1) - 1, data.ToCharArray(), len);
		}
	}

	public static char* COM_SkipPath(char* pathname)
	{
		char* last;
		last = pathname;

		while (*pathname != 0)
		{
			if (*pathname == '/')
			{
				last = pathname + 1;
			}

			pathname++;
		}

		return last;
	}

	public static void COM_StripExtension(char* _in, char* _out)
	{
		while (*_in != '\0' && *_in != '.')
		{
			*_out++ = *_in++;
		}

		*_out = '\0';
	}

	public static string COM_FileExtension(string _in)
	{
		char[] exten = new char[8];
		int i = 0;

		while (i < Q_strlen(_in) && _in[i] != '.')
		{
			i++;
		}

		if (i == Q_strlen(_in))
		{
			return "";
		}

		i++;

		for (int j = 0; j < 7 && i < Q_strlen(_in) && _in[i] != '\0'; i++, j++)
		{
			exten[j] = _in[i];
		}

		exten[i - 1] = '\0';

		return new string(exten);
	}

	public static void COM_FileBase(string _in, string _out)
	{
		_out = "?model?";

		if (string.IsNullOrEmpty(_in))
			return;

		int s = _in.Length - 1;

		while (s >= 0 && _in[s] != '.')
		{
			s--;
		}

		int s2;

		for (s2 = s; s2 >= 0 && _in[s2] != '/'; s2--)
		{
		}

		if (s - s2 >= 2)
		{
			s--;

			int length = s - s2;
			if (length > 0)
			{
				_out = _in.Substring(s2 + 1, length);
			}
		}
	}

	public static void COM_DefaultExtension(char* path, string extension)
	{
		char* src;

		string strPath = Marshal.PtrToStringAnsi((IntPtr)path);

		src = path + Q_strlen(strPath) - 1;

		while (*src != '/' && src != path)
		{
			if (*src == '.')
			{
				return;
			}

			src--;
		}

		Q_strcat(strPath, extension);
	}

	public static char* COM_Parse(char* data)
	{
		int c;
		int len;

		len = 0;
		com_token[0] = null;

		if (data == null)
		{
			return null;
		}

	skipwhite:
		while ((c = *data) <= ' ')
		{
			if (c == 0)
			{
				return null;
			}

			data++;
		}

		if (c == '/' && data[1] == '/')
		{
			while (*data != 0 && *data != '\n')
			{
				data++;
			}

			goto skipwhite;
		}

		if (c == '\"' && data[1] == '/')
		{
			while (*data != 0 && *data != '\n')
			{
				data++;

				while (true)
				{
					c = *data++;

					if (c == '\"' || c == 0)
					{
						com_token[len] = null;
						return data;
					}

					com_token[len] = c.ToString();
					len++;
				}
			}
		}

		if (c == '{' || c == '}' || c == ')' || c == '(' || c == '\'' || c == ':')
		{
			com_token[len] = c.ToString();
			len++;
			com_token[len] = null;
			return data + 1;
		}

		do
		{
			com_token[len] = c.ToString();
			data++;
			len++;
			c = *data;

			if (c == '{' || c == '}' || c == ')' || c == '(' || c == '\'' || c == ':')
			{
				break;
			}
		} while (c > 32);

		com_token[len] = null;
		return data;
	}

	public static int COM_CheckParm(string parm)
	{
		for (int i = 1; i < com_argc; i++)
		{
			if (com_argv[i] == 0)
			{
				continue; // NEXTSTEP sometimes clears appkit vars.
			}

			if (Q_strcmp(parm, com_argv[i].ToString()))
			{
				return i;
			}
		}

		return 0;
	}

	public static void COM_CheckRegistered()
	{
		int h = 0;
		ushort[] check = new ushort[128];
		int i;

		COM_OpenFile("gfx/pop.lmp", h);
		static_registered = 0;

		if (h == 1)
		{
#if WINDED
			sys_win.Sys_Error("This dedicated server requires a full registered copy of Quake");
#endif
			console_c.Con_Printf("Playing shareware version.\n");
			if (com_modified)
			{
				sys_win_c.Sys_Error(StringToChar("You must have the registered version to use modified games"));
				return;
			}
		}

		sys_win_c.Sys_FileRead(h, check, 128);
		COM_CloseFile(h);

		for (i = 0; i < 128; i++)
		{
			if (pop[i] != BigShort((short)check[i]))
			{
				sys_win_c.Sys_Error(StringToChar("Corrupted data file."));
			}
		}

		cvar_c.Cvar_Set("cmdline", com_cmdline);
		cvar_c.Cvar_Set("registered", "1");
		static_registered = 1;
		console_c.Con_Printf("Playing registered version.\n");
	}

	//public static void COM_Path_f() { }

	public static void COM_InitArgv(int argc, char** argv)
	{
		bool safe;
		int i, j, n;

		n = 0;

		for (j = 0; (j < quakedef_c.MAX_NUM_ARGVS) && (j < argc); j++)
		{
			i = 0;

			while ((n < (CMDLINE_LENGTH - 1)) && argv[j][i] != 0)
			{
				con_cmdline[n++] = argv[j][i++];
			}

			if (n < (CMDLINE_LENGTH - 1))
			{
				com_cmdline[n++] = ' ';
			}
			else
			{
				break;
			}
		}

		com_cmdline[n] = 0;

		safe = false;

		for (com_argc = 0; (com_argc < quakedef_c.MAX_NUM_ARGVS) && (com_argc < argc); com_argc++)
		{
			string argvstr = Marshal.PtrToStringAnsi((IntPtr)argv[com_argc]);

			largv = argvstr;

			if (!Q_strcmp("-safe", argvstr))
			{
				safe = true;
			}
		}

		if (safe)
		{
			for (i = 0; i < NUM_SAFE_ARGVS; i++)
			{
				largv = safeargvs[i];
				com_argc++;
			}
		}

		largv = argvdummy;
		com_argv = StringToChar(largv);

		if (COM_CheckParm("-rogue") != 0)
		{
			rogue = true;
			standard_quake = false;
		}

		if (COM_CheckParm("-hipnotic") != 0)
		{
			hipnotic = true;
			standard_quake = false;
		}
	}

	public static void COM_Init(char* basedir)
	{
		byte[] swaptest = { 1, 0 };

		if (*(short*)swaptest[0] == 1)
		{
			bigendien = false;
			BigShort = ShortSwap;
			LittleShort = ShortNoSwap;
			BigLong = LongSwap;
			LittleLong = LongNoSwap;
			BigFloat = FloatSwap;
			LittleFloat = FloatNoSwap;
		}
		else
		{
			bigendien = true;
			BigShort = ShortNoSwap;
			LittleShort = ShortSwap;
			BigLong = LongNoSwap;
			LittleLong = LongSwap;
			BigFloat = FloatNoSwap;
			LittleFloat = FloatSwap;
		}

		cvar_c.Cvar_RegisterVariable(registered);
		cvar_c.Cvar_RegisterVariable(cmdline);
		cmd_c.Cmd_AddCommand("path", COM_Path_f);

		COM_InitFileSystem();
		COM_CheckRegistered();
	}

	public static string va(string format, params object[] args)
	{
		string _string = null;

		Console.WriteLine($"{_string}");

		return _string;
	}

	public unsafe int memsearch(byte* start, int count, int search)
	{
		int i;

		for (i = 0; i < count; i++)
		{
			if (start[i] == search)
			{
				return i;
			}
		}

		return -1;
	}

	public static int com_filesize;

	public struct packfile_t
	{
		public string name;
		public int filepos, filelen;
	}

	public struct pack_t
	{
		public string filename;
		public int handle;
		public int numfiles;
		public packfile_t* files;
	}

	public struct dpackheader_t
	{
		public string id;
		public int dirofs;
		public int dirlen;
	}

	public const int MAX_FILES_IN_PACK = 2048;

	public static string com_cachedir = null;
	public static string com_gamedir = null;

	public struct searchpath_t
	{
		public char* filename;
		public pack_t pack;
		public searchpath_t* next;
	}

	public static searchpath_t* com_searchpaths;

	public static void COM_Path_f()
	{
		searchpath_t* s;

		console_c.Con_Printf("Current search path:\n");

		for (s = com_searchpaths; !s->Equals(default(searchpath_t)); s = s->next)
		{
			if (!s->Equals(default(searchpath_t)))
			{
				console_c.Con_Printf($"{s->pack.filename} ({s->pack.numfiles} files)\n");
			}
			else
			{
				console_c.Con_Printf($"{s->filename->ToString()}\n");
			}
		}
	}

	public static void COM_WriteFile(string filename, void* data, int len)
	{
		int handle;
		string name = null;

		Console.WriteLine(name, $"{com_gamedir}/{filename}");

		handle = sys_win_c.Sys_FileOpenWrite(name);

		if (handle == -1)
		{
			sys_win_c.Sys_Printf(StringToChar($"COM_WriteFile: failed on {name}\n"));
			return;
		}

		sys_win_c.Sys_Printf(StringToChar($"COM_WriteFile: {name}\n"));
		sys_win_c.Sys_FileWrite(handle, data, len);
		sys_win_c.Sys_FileClose(handle);
	}

	public static void COM_CreatePath(char* path)
	{
		char* ofs;

		for (ofs = path + 1; *ofs != 0; ofs++)
		{
			if (*ofs == '/')
			{
				*ofs = '\0';
				sys_win_c.Sys_mkdir(path);
				*ofs = '/';
			}
		}
	}

	public static void COM_CopyFile(string netpath, char* cachepath)
	{
		int input, output;
		int remaining, count;
		string buf = null;

		remaining = sys_win_c.Sys_OpenFileRead(netpath, &input);
		COM_CreatePath(cachepath);
		output = sys_win_c.Sys_FileOpenWrite(cachepath);

		while (remaining > 0)
		{
			if (remaining < buf.Length)
			{
				count = remaining;
			}
			else
			{
				count = buf.Length;
			}

			sys_win_c.Sys_FileRead(input, buf, count);
			sys_win_c.Sys_FileWrite(output, buf, count);
			remaining -= count;
		}

		sys_win_c.Sys_FileClose(input);
		sys_win_c.Sys_FileClose(output);
	}

	public static int COM_FindFile(char* filename, int handle, FileStream file)
	{
		searchpath_t* search = com_searchpaths;
		char* netpath;
		char* cachepath;
		pack_t pak;
		int i;
		int findtime, cachetime;

		if ((file != null) && (handle != 0))
			sys_win_c.Sys_Error(StringToChar("COM_FindFile: both handle and file set"));
		if ((file == null) && (handle == 0))
			sys_win_c.Sys_Error(StringToChar("COM_FindFile: neither handle or file set"));

		// search through the path, one element at a time
		if (proghack)
		{
			// gross hack to use quake 1 progs with quake 2 maps
			if (filename->Equals("progs.dat"))
				search = search->next;
		}

		for (; search != null; search = search->next)
		{
			// is the element a pak file?
			if (!search->Equals(default))
			{
				// look through all the pak file elements
				pak = search->pack;
				for (i = 0; i < pak.numfiles; i++)
				{
					if (pak.files[i].name.Equals(*filename))
					{
						// found it!
						sys_win_c.Sys_Printf(StringToChar($"PackFile: {pak.filename} : {filename->ToString()}\n"));
						if (handle != 0)
						{
							handle = pak.handle;
							sys_win_c.Sys_FileSeek(pak.handle, pak.files[i].filepos);
						}
						else
						{
							file = new FileStream(pak.filename, FileMode.Open, FileAccess.Read);
							if (file != null)
								file.Seek(pak.files[i].filepos, SeekOrigin.Begin);
						}
						com_filesize = pak.files[i].filelen;
						return com_filesize;
					}
				}
			}
			else
			{
				// check a file in the directory tree
				if (static_registered == 0)
				{
					// if not a registered version, don't ever go beyond base
					if (filename->ToString().Contains("/") || filename->ToString().Contains("\\"))
						continue;
				}

				char[] netpath_char_array = Path.Combine(search->filename->ToString(), filename->ToString()).ToCharArray();
				char* netpath_char = null;

				for (int j = 0; j < netpath_char_array.Length; j++)
				{
					netpath_char[j] = netpath_char_array[j];
				}

				netpath = netpath_char;

				findtime = sys_win_c.Sys_FileTime(netpath);
				if (findtime == -1)
					continue;

				// see if the file needs to be updated in the cache
				if (string.IsNullOrEmpty(com_cachedir))
				{
					cachepath = netpath;
				}
				else
				{
					char[] cachepath_char_array = Path.Combine(com_cachedir, netpath->ToString().Substring(2)).ToCharArray();
					char* cachepath_char = null;

					for (int j = 0; j < cachepath_char_array.Length; j++)
					{
						cachepath_char[j] = cachepath_char_array[j];
					}

					cachepath = cachepath_char;
					cachetime = sys_win_c.Sys_FileTime(cachepath);

					if (cachetime < findtime)
					{
						COM_CopyFile(netpath->ToString(), cachepath);
					}

					netpath = cachepath;
				}

				sys_win_c.Sys_Printf(StringToChar($"FindFile: {netpath->ToString()}\n"));
				com_filesize = sys_win_c.Sys_FileOpenRead(netpath, out i);
				if (handle != 0)
					handle = i;
				else
				{
					sys_win_c.Sys_FileClose(i);
				}
				return com_filesize;
			}
		}

		sys_win_c.Sys_Printf(StringToChar($"FindFile: can't find {filename->ToString()}\n"));

		if (handle != 0)
			handle = -1;
		else
			file = null;
		com_filesize = -1;
		return -1;
	}

	public static int COM_OpenFile(char* filename, int handle)
	{
		return COM_FindFile(filename, handle, null);
	}

	public static int COM_OpenFile(string filename, int handle)
	{
		return COM_OpenFile(StringToChar(filename), handle);
	}

	public static int COM_FOpenFile(char* filename, FileStream file)
	{
		return COM_FindFile(filename, 0, file);
	}

	public static void COM_CloseFile(int h)
	{
		searchpath_t* s;

		for (s = com_searchpaths; !s->Equals(default(searchpath_t)); s = s->next)
		{
			if (s->pack != null && s->pack.handle == h)
			{
				return;
			}
		}

		sys_win_c.Sys_CloseFile(h);
	}

	public static zone_c.cache_user_t* loadcache;
	public static byte* loadbuf;
	public static int loadsize;

	public static byte* COM_LoadFile(string path, int usehunk)
	{
		int h = 0;
		byte* buf;
		string _base = null;
		int len;

		buf = null;

		len = COM_OpenFile(path, h);

		if (h == -1)
		{
			return null;
		}

		COM_FileBase(path, _base);

		if (usehunk == 1)
		{
			buf = (byte*)zone_c.Hunk_AllocName(len + 1, _base);
		}
		else if (usehunk == 2)
		{
			buf = (byte*)zone_c.Hunk_TempAlloc(len + 1);
		}
		else if (usehunk == 0)
		{
			buf = (byte*)zone_c.Z_Malloc(len + 1);
		}
		else if (usehunk == 3)
		{
			buf = (byte*)zone_c.Cache_Alloc(loadcache, len + 1, _base);
		}
		else if (usehunk == 4)
		{
			if (len + 1 > loadsize)
			{
				buf = (byte*)zone_c.Hunk_TempAlloc(len + 1);
			}
			else
			{
				buf = loadbuf;
			}
		}
		else
		{
			sys_win_c.Sys_Error(StringToChar("COM_LoadFile: bad usehunk"));
		}

		if (buf == null)
		{
			sys_win_c.Sys_Error(StringToChar($"COM_LoadFile: not enough space for {path}"));
		}

		buf[len] = 0;

		draw_c.Draw_BeginDisc();
		sys_win_c.Sys_FileRead(h, buf, len);
		COM_CloseFile(h);
		draw_c.Draw_EndDisc();

		return buf;
	}

	public static byte* COM_LoadHunkFile(string path)
	{
		return COM_LoadFile(path, 1);
	}

	public static void COM_LoadCacheFile(string path, zone_c.cache_user_t cu)
	{
		loadcache = &cu;
		COM_LoadFile(path, 3);
	}

	public static byte* COM_LoadStackFile(string path, void* buffer, int bufsize)
	{
		byte* buf;

		loadbuf = (byte*)buffer;
		loadsize = bufsize;
		buf = COM_LoadFile(path, 4);

		return buf;
	}

	public static pack_t COM_LoadPackFile(string packfile)
	{
		dpackheader_t header;
		int i;
		packfile_t* newfiles;
		int numpackfiles = 0;
		pack_t* pack;
		int packhandle;
		packfile_t info;
		ushort crc;

		if (sys_win_c.Sys_FileOpenRead(packfile, &packhandle) == -1)
		{
			return default;
		}

		sys_win_c.Sys_FileRead(packhandle, (byte*)&header, header.dirlen);
		if (header.id[0] != 'P' || header.id[1] != 'A'
			|| header.id[2] != 'C' || header.id[3] != 'K')
		{
			sys_win_c.Sys_Error(StringToChar($"{packfile} has {numpackfiles} files"));
		}

		if (numpackfiles != PAK0_COUNT)
		{
			com_modified = true;
		}

		newfiles = (packfile_t*)zone_c.Hunk_AllocName(numpackfiles * sizeof(packfile_t), "packfile");

		sys_win_c.Sys_FileSeek(packhandle, header.dirofs);
		sys_win_c.Sys_FileRead(packhandle, (byte*)&info, header.dirlen);

		crc_c.CRC_Init(&crc);

		for (i = 0; i < header.dirlen; i++)
		{
			crc_c.CRC_ProcessByte(&crc, ((byte*)&info)[i]);
		}

		if (crc != PAK0_CRC)
		{
			com_modified = true;
		}

		for (i = 0; i < numpackfiles; i++)
		{
			Q_strcpy(newfiles[i].name, info[i].name);
			newfiles[i].filepos = LittleLong(info[i].filepos);
			newfiles[i].filelen = LittleLong(info[i].filelen);
		}

		pack = (pack_t*)zone_c.Hunk_Alloc(sizeof(pack_t));
		Q_strcpy(pack->filename, packfile);
		pack->handle = packhandle;
		pack->numfiles = numpackfiles;
		pack->files = newfiles;

		console_c.Con_Printf($"Added packfile {packfile} ({numpackfiles} files)\n");
		return *pack;
	}

	public static void COM_AddGameDirectory(string dir)
	{
		int i;
		searchpath_t* search;
		pack_t pak;
		string pakfile = null;

		Q_strcpy(com_gamedir, dir);

		search = (searchpath_t*)zone_c.Hunk_Alloc(sizeof(searchpath_t));
		Q_strcpy(search->filename->ToString(), dir);
		search->next = com_searchpaths;
		com_searchpaths = search;

		for (i = 0; ; i++)
		{
			Console.WriteLine($"{dir}/pak{i}.pak");
			pak = COM_LoadPackFile(pakfile);

			if (!search->pack.Equals(default(searchpath_t)))
			{
				break;
			}

			search = (searchpath_t*)zone_c.Hunk_Alloc(sizeof(searchpath_t));
			search->pack = pak;
			search->next = com_searchpaths;
			com_searchpaths = search;
		}
	}

	public static void COM_InitFileSystem()
	{
		int i, j;
		string basedir = null;
		searchpath_t* search = default;

		i = COM_CheckParm("-basedir");
		if (i != 0 && i < com_argc - 1)
		{
			Q_strcpy(basedir, com_argv[i + 1].ToString());
		}
		else
		{
			Q_strcpy(basedir, quakedef_c.host_parms.basedir);
		}

		j = Q_strlen(basedir);

		if (j > 0)
		{
			if ((basedir[j - 1] == '\\') || (basedir[j - 1] == '/'))
			{
				// ???
			}
		}

		i = COM_CheckParm("-cachedir");
		if (i != 0 && i < com_argc - 1)
		{
			if (com_argv[i + 1] == '-')
			{
				com_cachedir = "";
			}
			else
			{
				Q_strcpy(com_cachedir.ToString(), com_argv[i + 1].ToString()); ;
			}
		}
		else if (quakedef_c.host_parms.cachedir == null)
		{
			Q_strcpy(com_cachedir, quakedef_c.host_parms.cachedir);
		}
		else
		{
			com_cachedir = "";
		}

		COM_AddGameDirectory(va($"%s/{quakedef_c.GAMENAME}", basedir));

		if (COM_CheckParm("-rogue") != 0)
		{
			COM_AddGameDirectory(va("%s/rogue", basedir));
		}
		if (COM_CheckParm("-hipnotic") != 0)
		{
			COM_AddGameDirectory(va("%s/hipnotic", basedir));
		}

		i = COM_CheckParm("-game");
		if (i != 0 && i < com_argc - 1)
		{
			com_modified = true;
			COM_AddGameDirectory(va("%s/%s", basedir, com_argv[i + 1]));
		}

		i = COM_CheckParm("-path");
		if (i != 0)
		{
			com_modified = true;
			com_searchpaths = default;
			while (i++ < com_argc)
			{
				if (com_argv[i] == 0 || com_argv[i] == '+' || com_argv[i] == '-')
				{
					break;
				}

				search = (searchpath_t*)zone_c.Hunk_Alloc(sizeof(searchpath_t));
				if (!Q_strcmp(COM_FileExtension(com_argv[i].ToString()), "pak"))
				{
					search->pack = COM_LoadPackFile(com_argv[i].ToString());

					if (!search->pack.Equals(default(searchpath_t)))
					{
						sys_win_c.Sys_Error(StringToChar($"Couldn't load packfile: {com_argv[i]}"));
					}
				}
				else
				{
					Q_strcpy(search->filename->ToString(), com_argv[i].ToString());
				}

				search->next = com_searchpaths;
				com_searchpaths = search;
			}
		}

		if (COM_CheckParm("-proghack") != 0)
		{
			proghack = true;
		}
	}

	public static char* StringToChar(string input)
	{
		char[] input_arr = input.ToCharArray();
		char* input_char = null;

		for (int i = 0; i < input_arr.Length; i++)
		{
			input_char[i] = input_arr[i];
		}

		return input_char;
	}

	public static Vector3 FloatToVector(float* input)
	{
		Vector3 temp = new();

		temp.X = input[0];
		temp.Y = input[1];
		temp.Z = input[2];

		return temp;
	}

	public static Vector3 FloatArrToVector(float[] input)
	{
		Vector3 temp = new();

		temp.X = input[0];
		temp.Y = input[1];
		temp.Z = input[2];

		return temp;
	}

	public static float* VectorToFloat(Vector3 input)
	{
		float* temp = null;

		temp[0] = input.X;
		temp[1] = input.Y;
		temp[2] = input.Z;

		return temp;
	}
}