namespace Quake;

public unsafe class keys_c
{
	public const int K_TAB = 9;
	public const int K_ENTER = 13;
	public const int K_ESCAPE = 27;
	public const int K_SPACE = 32;

	public const int K_BACKSPACE = 127;
	public const int K_UPARROW = 128;
	public const int K_DOWNARROW = 129;
	public const int K_LEFTARROW = 130;
	public const int K_RIGHTARROW = 131;

	public const int K_ALT = 132;
	public const int K_CTRL = 133;
	public const int K_SHIFT = 134;
	public const int K_F1 = 135;
	public const int K_F2 = 136;
	public const int K_F3 = 137;
	public const int K_F4 = 138;
	public const int K_F5 = 139;
	public const int K_F6 = 140;
	public const int K_F7 = 141;
	public const int K_F8 = 142;
	public const int K_F9 = 143;
	public const int K_F10 = 144;
	public const int K_F11 = 145;
	public const int K_F12 = 146;
	public const int K_INS = 147;
	public const int K_DEL = 148;
	public const int K_PGDN = 149;
	public const int K_PGUP = 150;
	public const int K_HOME = 151;
	public const int K_END = 152;

	public const int K_PAUSE = 255;

	public const int K_MOUSE1 = 200;
	public const int K_MOUSE2 = 201;
	public const int K_MOUSE3 = 202;

	public const int K_JOY1 = 203;
	public const int K_JOY2 = 204;
	public const int K_JOY3 = 205;
	public const int K_JOY4 = 206;

	public const int K_AUX1 = 207;
	public const int K_AUX2 = 208;
	public const int K_AUX3 = 209;
	public const int K_AUX4 = 210;
	public const int K_AUX5 = 211;
	public const int K_AUX6 = 212;
	public const int K_AUX7 = 213;
	public const int K_AUX8 = 214;
	public const int K_AUX9 = 215;
	public const int K_AUX10 = 216;
	public const int K_AUX11 = 217;
	public const int K_AUX12 = 218;
	public const int K_AUX13 = 219;
	public const int K_AUX14 = 220;
	public const int K_AUX15 = 221;
	public const int K_AUX16 = 222;
	public const int K_AUX17 = 223;
	public const int K_AUX18 = 224;
	public const int K_AUX19 = 225;
	public const int K_AUX20 = 226;
	public const int K_AUX21 = 227;
	public const int K_AUX22 = 228;
	public const int K_AUX23 = 229;
	public const int K_AUX24 = 230;
	public const int K_AUX25 = 231;
	public const int K_AUX26 = 232;
	public const int K_AUX27 = 233;
	public const int K_AUX28 = 234;
	public const int K_AUX29 = 235;
	public const int K_AUX30 = 236;
	public const int K_AUX31 = 237;
	public const int K_AUX32 = 238;

	public const int K_MWHEELUP = 239;
	public const int K_MWHEELDOWN = 240;

	public enum keydest_t { key_game, key_console, key_message, key_menu };

	public const int MAXCMDLINE = 256;
	public char[,] key_lines = new char[MAXCMDLINE, 32];
	public int key_linepos;
	public int shift_down = 0;
	public int key_lastpress;

	public int edit_line = 0;
	public int history_line = 0;

	public static keydest_t key_dest;

	public static int key_count;

	public char* keybindings;
	public bool[] consolekeys = new bool[256];
	public bool[] menubound = new bool[256];
	public int[] keyshift = new int[256];
	public int[] key_repeats = new int[256];
	public bool[] keydown = new bool[256];

	public struct keyname_t
	{
		public string name;
		public int keynum;
	}

	public static Dictionary<string, int> keynames = new Dictionary<string, int>()
	{
		{"TAB", K_TAB },
		{"ENTER", K_ENTER },
		{"ESCAPE", K_ESCAPE },
		{"SPACE", K_SPACE },
		{"BACKSPACE", K_BACKSPACE },
		{"UPARROW", K_UPARROW },
		{"DOWNARROW", K_DOWNARROW },
		{"LEFTARROW", K_LEFTARROW },
		{"RIGHTARROW", K_RIGHTARROW },

		{"ALT", K_ALT },
		{"CTRL", K_CTRL },
		{"SHIFT", K_SHIFT},

		{"F1", K_F1},
		{"F2", K_F2},
		{"F3", K_F3},
		{"F4", K_F4},
		{"F5", K_F5},
		{"F6", K_F6},
		{"F7", K_F7},
		{"F8", K_F8},
		{"F9", K_F9},
		{"F10", K_F10},
		{"F11", K_F11},
		{"F12", K_F12},

		{"INS", K_INS},
		{"DEL", K_DEL},
		{"PGDN", K_PGDN},
		{"PGUP", K_PGUP},
		{"HOME", K_HOME},
		{"END", K_END},

		{"MOUSE1", K_MOUSE1},
		{"MOUSE2", K_MOUSE2},
		{"MOUSE3", K_MOUSE3},

		{"JOY1", K_JOY1},
		{"JOY2", K_JOY2},
		{"JOY3", K_JOY3},
		{"JOY4", K_JOY4},

		{"AUX1", K_AUX1},
		{"AUX2", K_AUX2},
		{"AUX3", K_AUX3},
		{"AUX4", K_AUX4},
		{"AUX5", K_AUX5},
		{"AUX6", K_AUX6},
		{"AUX7", K_AUX7},
		{"AUX8", K_AUX8},
		{"AUX9", K_AUX9},
		{"AUX10", K_AUX10},
		{"AUX11", K_AUX11},
		{"AUX12", K_AUX12},
		{"AUX13", K_AUX13},
		{"AUX14", K_AUX14},
		{"AUX15", K_AUX15},
		{"AUX16", K_AUX16},
		{"AUX17", K_AUX17},
		{"AUX18", K_AUX18},
		{"AUX19", K_AUX19},
		{"AUX20", K_AUX20},
		{"AUX21", K_AUX21},
		{"AUX22", K_AUX22},
		{"AUX23", K_AUX23},
		{"AUX24", K_AUX24},
		{"AUX25", K_AUX25},
		{"AUX26", K_AUX26},
		{"AUX27", K_AUX27},
		{"AUX28", K_AUX28},
		{"AUX29", K_AUX29},
		{"AUX30", K_AUX30},
		{"AUX31", K_AUX31},
		{"AUX32", K_AUX32},

		{"PAUSE", K_PAUSE},

		{"MWHEELUP", K_MWHEELUP},
		{"MWHEELDOWN", K_MWHEELDOWN},

		{"SEMICOLON", ';'},	// because a raw semicolon seperates commands

		{null, 0}
	};

	public void Key_Console(int key)
	{
		char* cmd;

		if (key == K_ENTER)
		{
			Cbuf_AddText(key_lines[edit_line] + 1);
			Cbuf_AddText("\n");
			console_c.Con_Printf($"{key_lines[edit_line, 0]}\n");
			edit_line = (edit_line + 1) & 31;
			history_line = edit_line;
			key_lines[edit_line, 0] = ']';
			key_linepos = 1;

			if (cls.state == ca_disconnected)
			{
				screen_c.SCR_UpdateScreen();
			}

			return;
		}

		if (key == K_TAB)
		{
			cmd = Cmd_CompleteCommand(key_lines[edit_line, 0] + 1);

			if (cmd == null)
			{
				cmd = Cvar_CompleteVariable(key_lines[edit_line, 0] + 1);
			}

			if (cmd != null)
			{
				common_c.Q_strcpy((key_lines[edit_line, 0] + 1).ToString(), cmd->ToString());
				key_linepos = common_c.Q_strlen(cmd->ToString()) + 1;
				key_lines[edit_line, key_linepos] = ' ';
				key_linepos++;
				key_lines[edit_line, key_linepos] = 0;
				return;
			}
		}

		if (key == K_BACKSPACE || key == K_LEFTARROW)
		{
			if (key_linepos > 1)
			{
				key_linepos--;
			}

			return;
		}

		if (key == K_UPARROW)
		{
			do
			{
				history_line = (history_line - 1) & 31;
			} while (history_line != edit_line && key_lines[history_line, 1] == 0);

			if (history_line == edit_line)
			{
				history_line = (edit_line + 1) & 31;
			}

			common_c.Q_strcpy(key_lines[edit_line, 0].ToString(), key_lines[history_line, 0].ToString());
			key_linepos = common_c.Q_strlen(key_lines[edit_line, 0].ToString());
			return;
		}

		if (key == K_DOWNARROW)
		{
			if (history_line == edit_line)
			{
				return;
			}

			do
			{
				history_line = (history_line + 1) & 31;
			} while (history_line != edit_line && key_lines[history_line, 1] == 0);

			if (history_line == edit_line)
			{
				key_lines[edit_line, 0] = ']';
				key_linepos = 1;
			}
			else
			{
				common_c.Q_strcpy(key_lines[edit_line, 0].ToString(), key_lines[history_line, 0].ToString());
				key_linepos = common_c.Q_strlen(key_lines[edit_line, 0].ToString());
			}

			return;
		}

		if (key == K_PGUP || key == K_MWHEELUP)
		{
			console_c.con_backscroll += 2;

			if (console_c.con_backscroll > console_c.con_totallines - (vid_win_c.vid.height >> 3) - 1)
			{
				console_c.con_backscroll = console_c.con_totallines - (vid_win_c.vid.height >> 3) - 1;
			}

			return;
		}

		if (key == K_PGDN || key == K_MWHEELDOWN)
		{
			console_c.con_backscroll -= 2;

			if (console_c.con_backscroll > console_c.con_totallines - (vid_win_c.vid.height >> 3) - 1)
			{
				console_c.con_backscroll = console_c.con_totallines - (vid_win_c.vid.height >> 3) - 1;
			}

			return;
		}

		if (key == K_HOME)
		{
			console_c.con_backscroll = console_c.con_totallines - (vid_win_c.vid.height >> 3) - 1;
			return;
		}

		if (key == K_END)
		{
			console_c.con_backscroll = 0;
			return;
		}

		if (key < 32 || key > 127)
		{
			return;
		}

		if (key_linepos < MAXCMDLINE - 1)
		{
			key_lines[edit_line, key_linepos] = (char)key;
			key_linepos++;
			key_lines[edit_line, key_linepos] = '\0';
		}
	}

	public char[] chat_buffer = new char[32];
	public bool team_message = false;

	public void Key_Message(int key)
	{
		int chat_bufferlen = 0;

		if (key == K_ENTER)
		{
			if (team_message)
			{
				Cbuf_AddText("say_team \"");
			}
			else
			{
				Cbuf_AddText("say \"");
			}

			Cbuf_AddText(chat_buffer);
			Cbuf_AddText("\"\n");

			key_dest = keydest_t.key_game;
			chat_bufferlen = 0;
			chat_buffer[0] = '\0';
			return;
		}

		if (key == K_ESCAPE)
		{
			key_dest = key_dest_t.key_game;
			chat_bufferlen = 0;
			chat_buffer[0] = '\0';
			return;
		}

		if (key < 32 || key > 127)
		{
			return;
		}

		if (key == K_BACKSPACE)
		{
			if (chat_bufferlen != 0)
			{
				chat_bufferlen--;
				chat_buffer[chat_bufferlen] = '\0';
			}
			return;
		}

		if (chat_bufferlen == 31)
		{
			return;
		}

		chat_buffer[chat_bufferlen++] = (char)key;
		chat_buffer[chat_bufferlen] = '\0';
	}

	public int Key_StringToKeyNum(char* str)
	{
		keyname_t* kn = default;

		if (str == null || str[0] == null)
		{
			return -1;
		}

		if (str[1] == null)
		{
			return str[0];
		}

		for (kn->keynum = keynames.Count; kn->name != null; kn++)
		{
			if (common_c.Q_strcasecmp(str->ToString(), kn->name) == 0)
			{
				return kn->keynum;
			}
		}

		return -1;
	}

	public string Key_KeynumToString(int keynum)
	{
		keyname_t* kn = default;
		char[] tinystr = new char[2];

		if (keynum == -1)
		{
			return "<KEY NOT FOUND>";
		}

		if (keynum > 32 && keynum < 127)
		{
			tinystr[0] = (char)keynum;
			tinystr[1] = '\0';
			return tinystr.ToString();
		}

		for (kn->keynum = keynames.Count; kn->name != null; kn++)
		{
			if (keynum == kn->keynum)
			{
				return kn->name;
			}
		}

		return "<UNKOWN KEYNUM>";
	}

	public void Key_SetBinding(int keynum, char* binding)
	{
		string* _new;
		int l;

		if (keynum == -1)
		{
			return;
		}

		if (keybindings[keynum] != 0)
		{
			zone_c.Z_Free(keybindings);
			keybindings[keynum] = '\0';
		}

		l = common_c.Q_strlen(binding->ToString());
		_new = (string*)zone_c.Z_Malloc(l + 1);
		common_c.Q_strcpy(_new->ToString(), binding->ToString());
		_new[l] = null;
		keybindings[keynum] = _new->ToCharArray().First();
	}

	public void Key_Unbind_f()
	{
		int b;

		if (Cmd_Argc() != 2)
		{
			console_c.Con_Printf("unbind <key> : remove commands from a key\n");
			return;
		}

		b = Key_StringToKeyNum(Cmd_Argv(1));

		if (b == -1)
		{
			console_c.Con_Printf($"\"{Cmd_Argv(1)}\" isn't a valid key\n");
			return;
		}

		Key_SetBinding(b, null);
	}

	public void Key_Unbindall_f()
	{
		int i;

		for (i = 0; i < 256; i++)
		{
			if (keybindings[i] != 0)
			{
				Key_SetBinding(i, null);
			}
		}
	}

	public void Key_Bind_f()
	{
		int i, c, b;
		char cmd;

		c = Cmd_Argc();

		if (c != 2 && c != 3)
		{
			console_c.Con_Printf("bind <key> [command] : attach a command to a key\n");
		}
	}
}