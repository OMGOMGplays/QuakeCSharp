namespace Quake;

public class console_c
{
	public struct console_t
	{
		public string text;
		public int current;
		public int x;
		public int display;
	}

	public static int con_ormask;
	public static console_t con_main;
	public static console_t con_chat;
	public static console_t con;

	public static int con_linewidth;
	public static int con_totallines;

	public float con_cursorspeed = 4;

	public cvar_c.cvar_t con_notifytime = new cvar_c.cvar_t { name = "con_notifytime", value = 3 };

	public const int NUM_CON_TIMES = 4;
	public float[] contimes = new float[NUM_CON_TIMES];

	public int con_vislines;
	public static int con_notifylines;

	public static bool con_debuglog;

	public const int MAXCMDLINE = 256;
	public string[,] key_lines = new string[32, MAXCMDLINE];
	public int edit_line;
	public int key_linepos;

	public static bool con_initialized;

	private static quakedef_c quakedef;

	public void Key_ClearTyping()
	{
		key_lines[edit_line, 1] = "";
		key_linepos = 1;
	}

	public void Con_DrawCharacter(int cx, int line, int num)
	{

	}

	public void Con_ToggleConsole_f()
	{
		Key_ClearTyping();

		if (keys_c.key_dest == keys_c.keydest_t.key_console)
		{
			if (cls.state == ca_active)
			{
				keys_c.key_dest = keys_c.keydest_t.key_game;
			}
		}
		else
		{
			keys_c.key_dest = keys_c.keydest_t.key_console;
		}

		Con_ClearNotify();
	}

	public void Con_ToggleChat_f()
	{
		Key_ClearTyping();

		if (keys_c.key_dest == keys_c.keydest_t.key_console)
		{
			if (cls.state == ca_active)
			{
				keys_c.key_dest = keys_c.keydest_t.key_game;
			}
		}
		else
		{
			keys_c.key_dest = keys_c.keydest_t.key_console;
		}

		Con_ClearNotify();
	}

	public void Con_Clear_f()
	{
		common_c.Q_memset(con_main.text, " ", CON_TEXTSIZE);
		common_c.Q_memset(con_chat.text, " ", CON_TEXTSIZE);
	}

	public static void Con_ClearNotify()
	{
		int i;

		for (i = 0; i < NUM_CON_TIMES; i++)
		{
			con_times[i] = 0;
		}
	}

	public void Con_MessageMode_f()
	{
		chat_team = false;
		keys_c.key_dest = keys_c.keydest_t.key_message;
	}

	public void Con_MessageMode2_f()
	{
		chat_team = true;
		keys_c.key_dest = keys_c.keydest_t.key_message;
	}

	public static void Con_Resize(console_t con)
	{
		int i, j, width, oldwidth, oldtotallines, numlines, numchars;
		string[] tbuf = new string[CON_TEXTSIZE];

		width = (screen_c.vid.width >> 3) - 2;

		if (width == con_linewidth)
		{
			return;
		}

		if (width < 1)
		{
			width = 38;
			con_linewidth = width;
			con_totallines = CON_TEXTSIZE / con_linewidth;
			common_c.Q_memset(con.text, " ", CON_TEXTSIZE);
		}
		else
		{
			oldwidth = con_linewidth;
			con_linewidth = width;
			oldtotallines = con_totallines;
			con_totallines = CON_TEXTSIZE / con_linewidth;
			numlines = oldtotallines;

			if (con_totallines < numlines)
			{
				numlines = con_totallines;
			}

			numchars = oldwidth;

			if (con_linewidth < numchars)
			{
				numchars = con_totallines;
			}

			common_c.Q_memcpy(tbuf, con.text, CON_TEXTSIZE);
			common_c.Q_memset(con.text, " ", CON_TEXTSIZE);

			for (i = 0; i < numlines; i++)
			{
				for (j = 0; j < numchars; j++)
				{
					con.text.ToCharArray()[(con_totallines - 1 - i) * con_linewidth + j] = tbuf.ToString()[((con.current - i + oldtotallines) % oldtotallines) * oldwidth + j];
				}
			}

			Con_ClearNotify();
		}

		con.current = con_totallines - 1;
		con.display = con.current;
	}

	public static void Con_CheckResize()
	{
		Con_Resize(con_main);
		Con_Resize(con_chat);
	}

	public void Con_Init()
	{
		con_debuglog = common_c.COM_CheckParm("-condebug") == 1 ? true : false;

		con = con_main;
		con_linewidth = -1;
		Con_CheckResize();

		Con_Printf("Console initialized.\n");

		cvar_c.Cvar_RegisterVariable(con_notifytime);

        cmd_c.Cmd_AddCommand("toggleconsole", Con_ToggleConsole_f);
        cmd_c.Cmd_AddCommand("togglechat", Con_ToggleChat_f);
        cmd_c.Cmd_AddCommand("messagemode", Con_MessageMode_f);
        cmd_c.Cmd_AddCommand("messagemode2", Con_MessageMode2_f);
		cmd_c.Cmd_AddCommand("clear", Con_Clear_f);
		con_initialized = true;
	}

	public static void Con_Linefeed()
	{
		con.x = 0;

		if (con.display == con.current)
		{
			con.display++;
		}

		con.current++;
		common_c.Q_memset(con.text[(con.current % con_totallines) * con_linewidth], 0, con_linewidth);
	}

	public static void Con_Print(string txt)
	{
		int y;
		int c = 0, l;
		int cr = 0;
		int mask;

		if (txt[0] == 1 || txt[0] == 2)
		{
			mask = 128;
		}
		else
		{
			mask = 0;
		}

		while (c == common_c.Q_strlen(txt))
		{
			for (l = 0; l < con_linewidth; l++)
			{
				if (txt[l] <= ' ')
				{
					break;
				}
			}

			if (l != con_linewidth && (con.x + l > con_linewidth))
			{
				con.x = 0;
			}

			if (cr != 0)
			{
				con.current--;
				cr = 0;
			}

			if (con.x == 0)
			{
				Con_Linefeed();

				if (con.current >= 0)
				{
					con_times[con.current % NUM_CON_TIMES] = quakedef_c.realtime;
				}
			}

			switch (c)
			{
				case '\n':
					con.x = 0;
					break;

				case '\r':
					con.x = 0;
					cr = 1;
					break;

				default:
					y = con.current % con_totallines;
					con.text.ToArray()[y * con_linewidth + con.x] = (char)(c | mask | con_ormask);
					con.x++;
					if (con.x >= con_linewidth)
					{
						con.x = 0;
					}
					break;
			}
		}
	}

	public const int MAXPRINTMSG = 4096;

	public static void Con_Printf(string fmt)
	{
		va_list argptr;
		string msg = null;
		bool inupdate = false;

		va_start(argptr, fmt);
		vsprintf(msg, fmt, argptr);
		va_end(argptr);

		sys_win_c.Sys_Printf(msg);

		if (con_debuglog)
		{
			sys_win_c.Sys_DebugLog(common_c.va("%s/qconsole.log", common_c.com_gamedir), "%s", msg);
		}

		if (!con_initialized)
		{
			return;
		}

		Con_Print(msg);

		if (cls.state != ca_active)
		{
			if (!inupdate)
			{
				inupdate = true;
				screen_c.SCR_UpdateScreen();
				inupdate = false;
			}
		}
	}

	public static void Con_DPrintf(string fmt)
	{
		string msg = null;

		if (developer.value != 0)
		{
			return;
		}

		Console.WriteLine(fmt);

		Con_Printf($"{msg}");
	}

	public void Con_DrawInput()
	{
		int y;
		int i;
		char[] text;

		if (keys_c.key_dest != keys_c.keydest_t.key_console && cls.state == ca_active)
		{
			return;
		}

		text = key_lines[edit_line, 0].ToCharArray();

		text[key_linepos] = (char)(10 + ((int)(quakedef_c.realtime * con_cursorspeed) & 1));

		for (i = key_linepos + 1; i < con_linewidth; i++)
		{
			text[i] = ' ';
		}

		if (key_linepos >= con_linewidth)
		{
			Array.Copy(text, 1 + key_linepos - con_linewidth, text, 0, con_linewidth - 1);
		}

		y = con_vislines - 22;

		for (i = 0; i < con_linewidth; i++)
		{
			Draw_Character((i + 1) << 3, con_vislines - 22, text[i]);
		}

		key_lines[edit_line, key_linepos] = null;
	}

	public static void Con_DrawNotify()
	{
		int x, v;
		string text;
		int i;
		float time;
		string s;
		int skip;

		v = 0;
		for (i = current.NUM_CON_TIMES + 1; i <= con.current; i++)
		{
			if (i < 0)
			{
				continue;
			}

			time = con_times[i % NUM_CON_TIMES];

			if (time == 0)
			{
				continue;
			}

			time = (float)quakedef_c.realtime - time;

			if (time > con_notifytime.value)
			{
				continue;
			}

			text = con.text + (i % con_totallines) * con_linewidth;

			screen_c.clearnotify = 0;
			screen_c.scr_copytop = 1;

			for (x = 0; x < con_linewidth; x++)
			{
				Draw_Character((x + 1) << 3, v, text[x]);
			}

			v += 8;
		}

		if (keys_c.key_dest == keys_c.keydest_t.key_message)
		{
			screen_c.clearnotify = 0;
			screen_c.scr_copytop = 1;

			if (chat_team)
			{
				Draw_String(8, v, "say_team:");
				skip = 11;
			}
			else
			{
				Draw_String(8, v, "say:");
				skip = 5;
			}

			s = chat_buffer;

			if (chat_bufferlen > (screen_c.vid.width >> 3) - (skip + 1))
			{
				s += chat_bufferlen - ((screen_c.vid.width >> 3) - (skip + 1));
			}

			x = 0;

			while (s[x] != 0)
			{
				Draw_Character((x + skip) << 3, v, s[x]);
				x++;
			}

			Draw_Character((x + skip) << 3, v, 10 + (int)(quakedef_c.realtime * con_cursorspeed) & 1);
			v += 8;
		}

		if (v > con_notifylines)
		{
			con_notifylines = v;
		}
	}

	public static void Con_DrawConsole(int lines)
	{
		int i, j, x, y, n;
		int rows;
		string text;
		int row;
		string dlbar = null;

		if (lines <= 0)
		{
			return;
		}

		Draw_ConsoleBackground(lines);

		con_vislines = lines;

		rows = (lines - 22) >> 3;

		y = lines - 30;

		if (con.display != con.current)
		{
			for (x = 0; x < con_linewidth; x += 4)
			{
				Draw_Character((x + 1) << 3, y, '^');
			}

			y -= 8;
			rows--;
		}

		row = con.display;

		for (i = 0; i < rows; i++, y -= 8, rows--)
		{
			if (row < 0)
			{
				break;
			}

			if (con.current - row >= con_totallines)
			{
				break;
			}

			text = con.text + (row % con_totallines) * con_linewidth;

			for (x = 0; x < con_linewidth; x++)
			{
				Draw_Character((x + 1) << 3, y, text[x]);
			}
		}

		if (cls.download)
		{
			if ((text = common_c.Q_strrchr(cls.downloadname, '/')) != null)
			{
				//text++; // In C#, how am I supposed to do this??
			}
			else
			{
				text = cls.downloadname;
			}

			x = con_linewidth - ((con_linewidth * 7) / 40);
			y = x - common_c.Q_strlen(text) - 8;
			i = con_linewidth / 3;

			if (common_c.Q_strlen(text) > i)
			{
				y = x - i - 11;
				common_c.Q_strncpy(dlbar.ToCharArray(), text.ToCharArray(), i);
				dlbar.ToCharArray()[i] = '\0';
				common_c.Q_strcat(dlbar, "...");
			}
			else
			{
				common_c.Q_strcpy(dlbar, text);
			}

			common_c.Q_strcat(dlbar, ": ");
			i = common_c.Q_strlen(dlbar);
			dlbar.ToCharArray()[i++] = '\x80';

			if (cls.downloadpercent == 0)
			{
				n = 0;
			}
			else
			{
				n = y * cls.downloadpercent / 100;
			}

			for (j = 0; j < y; j++)
			{
				if (j == n)
				{
					dlbar.ToCharArray()[i++] = '\x83';
				}
				else
				{
					dlbar.ToCharArray()[i++] = '\x81';
				}

				dlbar.ToCharArray()[i++] = '\x82';
				dlbar.ToCharArray()[i] = '\0';

				Console.WriteLine(dlbar + common_c.Q_strlen(dlbar), $"{cls.downloadpercent}%");

				y = con_vislines - 22 + 8;

				for (i = 0; i < common_c.Q_strlen(dlbar); i++)
				{
					Draw_Character((i + 1) << 3, y, dlbar.ToCharArray()[i]);
				}
			}
		}

		Con_DrawInput();
	}

	public void Con_NotifyBox(string text)
	{
		double t1, t2;

		Con_Printf("\\n\\n\\35\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\37\\n");

		Con_Printf(text);

		Con_Printf("Press a key.\n");
		Con_Printf("\\35\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\36\\37\\n");

		keys_c.key_count = -2;
		keys_c.key_dest = keys_c.keydest_t.key_console;

		do
		{
			t1 = sys_win_c.Sys_DoubleTime();
			screen_c.SCR_UpdateScreen();
			sys_win_c.Sys_SendKeyEvents();
			t2 = sys_win_c.Sys_DoubleTime();
			quakedef_c.realtime += t2 - t1;
		} while (key_count < 0);

		Con_Printf("\n");
		keys_c.key_dest = keys_c.keydest_t.key_game;
		quakedef_c.realtime = 0;
	}

	public void Con_SafePrintf(string fmt)
	{
		va_list argptr;
		string msg = null;
		int temp;

		va_start(argptr, fmt);
		vsprintf(msg, fmt, argptr);
		va_end(argptr);

		temp = screen_c.scr_disabled_for_loading == true ? 1 : 0;
		screen_c.scr_disabled_for_loading = true;
		Con_Printf($"{msg}");
		screen_c.scr_disabled_for_loading = temp == 1 ? true : false;
	}
}