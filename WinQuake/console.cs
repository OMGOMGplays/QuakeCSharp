namespace Quake;

public unsafe class console_c
{
    public static int con_totallines;
    public static int con_backscroll;
    public static bool con_forcedup;
    public static bool con_initialized;
    public static byte* con_chars;
    public static int con_notifylines;
    public static int con_x;

    public static int con_linewidth;

    public static float con_cursorspeed = 4;

    public static int CON_TEXTSIZE = 16384;

    public static int con_current;
    public static char* con_text = null;

    public static cvar_c.cvar_t con_notifytime = new cvar_c.cvar_t { name = "con_notifytime", value = '3' };

    public static int NUM_CON_TIMES = 4;

    public static float* con_times;

    public static int con_vislines;

    public static bool con_debuglog;

    public static int MAXCMDLINE = 256;
    public static char[][] key_lines = new char[32][];
    public static int edit_line;
    public static int key_linepos;

    public static extern void M_Main_Menu_f();

    public static void Con_ToggleConsole_f()
    {
        if (keys_c.key_dest == keys_c.keydest_t.key_console)
        {
            if (client_c.cls.state == client_c.cactive_t.ca_connected)
            {
                keys_c.key_dest = keys_c.keydest_t.key_game;
                key_lines[edit_line][1] = 0;
                key_linepos = 1;
            }
            else
            {
                M_Menu_Main_f();
            }
        }
        else
        {
            keys_c.key_dest = keys_c.keydest_t.key_console;
        }

        screen_c.SCR_EndLoadingPlaque();
        common_c.Q_memset(con_times, 0, NUM_CON_TIMES);
    }

    public static void Con_Clear_f()
    {
        if (con_text != null)
        {
            common_c.Q_memset(*con_text, ' ', CON_TEXTSIZE);
        }
    }

    public static void Con_ClearNotify()
    {
        int i;

        for (i = 0; i < NUM_CON_TIMES; i++)
        {
            con_times[i] = 0;
        }
    }

    public static bool teammessage;

    public static void Con_MessageMode_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_message;
        keys_c.team_message = false;
    }

    public static void Con_MessageMode2_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_message;
        keys_c.team_message = true;
    }

    public static void Con_CheckResize()
    {
        int i, j, width, oldwidth, oldtotallines, numlines, numchars;
        char[] tbuf = new char[CON_TEXTSIZE];

        width = (int)(vid_c.vid.width >> 3) - 2;

        if (width == con_linewidth)
        {
            return;
        }

        if (width < 1)
        {
            width = 38;
            con_linewidth = width;
            con_totallines = CON_TEXTSIZE / con_linewidth;
            common_c.Q_memset(*con_text, ' ', CON_TEXTSIZE);
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
                numchars = con_linewidth;
            }

            common_c.Q_memcpy(tbuf, *con_text, CON_TEXTSIZE);
            common_c.Q_memset(*con_text, ' ', CON_TEXTSIZE);

            for (i = 0; i < numlines; i++)
            {
                for (j = 0; j < numchars; j++)
                {
                    con_text[(con_totallines - 1 - i) * con_linewidth + j] = tbuf[((con_current - i + oldtotallines) % oldtotallines) * oldwidth + j];
                }
            }

            Con_ClearNotify();
        }

        con_backscroll = 0;
        con_current = con_totallines - 1;
    }

    public static int MAXGAMEDIRLEN = 1000;

    public static void Con_Init()
    {
        char[] temp = new char[MAXGAMEDIRLEN + 1];
        char* t2 = common_c.StringToChar("/qconsole.log");

        con_debuglog = common_c.COM_CheckParm("-condebug") != 0 ? true : false;

        if (con_debuglog)
        {
            if (common_c.Q_strlen(common_c.com_gamedir) < (MAXGAMEDIRLEN - common_c.Q_strlen(t2->ToString())))
            {
                Console.WriteLine(temp.ToString(), $"{common_c.com_gamedir}{t2->ToString()}");
                File.Delete(temp.ToString());
            }
        }

        con_text = (char*)zone_c.Hunk_AllocName(CON_TEXTSIZE, "context");
        common_c.Q_memset(*con_text, ' ', CON_TEXTSIZE);
        con_linewidth = -1;
        Con_CheckResize();

        Con_Printf(common_c.StringToChar("Console initialized\n"));

        cvar_c.Cvar_RegisterVariable(&con_notifytime);

        cmd_c.Cmd_AddCommand("toggleconsole", Con_ToggleConsole_f);
        cmd_c.Cmd_AddCommand("messagemode", Con_MessageMode_f);
        cmd_c.Cmd_AddCommand("messagemode2", Con_MessageMode2_f);
        cmd_c.Cmd_AddCommand("clear", Con_Clear_f);
        con_initialized = true;
    }

    public static void Con_Linefeed()
    {
        con_x = 0;
        con_current++;
        common_c.Q_memset(con_text[(con_current % con_totallines) * con_linewidth], ' ', con_linewidth);
    }

    public static void Con_Print(char* txt)
    {
        int y;
        int c, l;
        int cr = 0;
        int mask;

        con_backscroll = 0;

        if (txt[0] == 1)
        {
            mask = 128;
            snd_null_c.S_LocalSound("misc/talk.wav");
            txt++;
        }
        else if (txt[0] == 2)
        {
            mask = 128;
            txt++;
        }
        else
        {
            mask = 0;
        }

        while ((c == *txt))
        {
            for (l = 0; l < con_linewidth; l++)
            {
                if (txt[l] <= ' ')
                {
                    break;
                }
            }

            if (l != con_linewidth && (con_x + l > con_linewidth))
            {
                con_x = 0;
            }

            txt++;

            if (cr != 0)
            {
                con_current--;
                cr = 0;
            }

            if (con_x == 0)
            {
                Con_Linefeed();

                if (con_current >= 0)
                {
                    con_times[con_current % NUM_CON_TIMES] = (float)quakedef_c.realtime;
                }
            }

            switch (c)
            {
                case '\n':
                    con_x = 0;
                    break;

                case '\r':
                    con_x = 0;
                    cr = 1;
                    break;

                default:
                    y = con_current % con_totallines;
                    con_text[y * con_totallines + con_x] = (char)(c | mask);
                    con_x++;

                    if (con_x >= con_linewidth)
                    {
                        con_x = 0;
                    }
                    break;
            }
        }
    }

    public static void Con_DebugLog(string file, string fmt, params object[] args)
    {
        string data;

        data = string.Format(fmt, args);

        using (StreamWriter writer = new StreamWriter(file, true))
        {
            writer.Write(data);
        }
    }

    public static int MAXPRINTMSG = 4096;

    public static void Con_Printf(char* fmt, params object[] args)
    {
        string msg;
        bool inupdate = false;

        msg = string.Format(fmt->ToString(), args);

        using (StreamWriter writer = new StreamWriter(fmt->ToString(), true))
        {
            writer.Write(msg);
        }

        sys_win_c.Sys_Printf(common_c.StringToChar(msg));

        if (con_debuglog)
        {
            Con_DebugLog(common_c.va($"{common_c.com_gamedir}/qconsole.log"), msg);
        }

        if (!con_initialized)
        {
            return;
        }

        if (client_c.cls.state == client_c.cactive_t.ca_dedicated)
        {
            return;
        }

        Con_Print(common_c.StringToChar(msg));

        if (client_c.cls.signon != client_c.SIGNONS && !screen_c.scr_disabled_for_loading)
        {
            if (!inupdate)
            {
                inupdate = true;
                screen_c.SCR_UpdateScreen();
                inupdate = false;
            }
        }
    }

    public static void Con_DPrintf(string fmt, params object[] args)
    {
        string msg = "";

        if (host_c.developer.value == 0)
        {
            return;
        }

        msg = string.Format(fmt, args);

        using (StreamWriter writer = new StreamWriter(fmt, true))
        {
            writer.Write(msg);
        }

        Con_Printf(common_c.StringToChar(msg));
    }

    public static void Con_SafePrintf(string fmt, params object[] args)
    {
        string msg = "";
        int temp;

        msg = string.Format(fmt, args);

        using (StreamWriter writer = new StreamWriter(fmt, true))
        {
            writer.Write(msg);
        }

        temp = screen_c.scr_disabled_for_loading == true ? 1 : 0;
        screen_c.scr_disabled_for_loading = true;
        Con_Printf(common_c.StringToChar(msg));
        screen_c.scr_disabled_for_loading = temp == 1 ? true : false;
    }

    public static void Con_DrawInput()
    {
        int y;
        int i;
        char* text;

        if (keys_c.key_dest != keys_c.keydest_t.key_console && !con_forcedup)
        {
            return;
        }

        text = &key_lines[edit_line][0];

        text[key_linepos] = (char)(10 + ((int)(quakedef_c.realtime * con_cursorspeed) & 1));

        for (i = key_linepos + 1; i < con_linewidth; i++)
        {
            text[i] = ' ';
        }

        if (key_linepos >= con_linewidth)
        {
            text += 1 + key_linepos - con_linewidth;
        }

        y = con_linewidth - 16;

        for (i = 0; i < con_linewidth; i++)
        {
            draw_c.Draw_Character((i + 1) << 3, con_vislines - 16, text[i]);
        }

        key_lines[edit_line][key_linepos] = '\0';
    }

    public static void Con_DrawNotify()
    {
        int x, v;
        char* text;
        int i;
        float time;
        char[] chat_buffer = null;

        v = 0;

        for (i = con_current - NUM_CON_TIMES + 1; i <= con_current; i++)
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

            text = con_text + (i % con_totallines) * con_linewidth;

            screen_c.clearnotify = 0;
            screen_c.scr_copytop = 1;

            for (x = 0; x < con_linewidth; x++)
            {
                draw_c.Draw_Character((x + 1) << 3, v, text[x]);
            }

            v += 8;
        }

        if (keys_c.key_dest == keys_c.keydest_t.key_message)
        {
            screen_c.clearnotify = 0;
            screen_c.scr_copytop = 1;

            x = 0;

            draw_c.Draw_String(8, v, new string('say: '));

            while (chat_buffer[x] != 0)
            {
                draw_c.Draw_Character((x + 5) << 3, v, chat_buffer[x]);
                x++;
            }

            draw_c.Draw_Character((x + 5) << 3, v, 10 + ((int)(quakedef_c.realtime * con_cursorspeed) & 1));
            v += 8;
        }

        if (v > con_notifylines)
        {
            con_notifylines = v;
        }
    }

    public static void Con_DrawConsole(int lines, bool drawinput)
    {
        int i, x, y;
        int rows;
        char* text;
        int j;

        if (lines <= 0)
        {
            return;
        }

        draw_c.Draw_ConsoleBackground(lines);

        con_vislines = lines;

        rows = (lines - 16) >> 3;
        y = lines - 16 - (rows << 3);

        for (i = con_current - rows + 1; i <= con_current; i++, y += 8)
        {
            j = i - con_backscroll;

            if (j < 0)
            {
                j = 0;
            }

            text = con_text + (j % con_totallines) * con_linewidth;

            for (x = 0; x < con_linewidth; x++)
            {
                draw_c.Draw_Character((x + 1) << 3, y, text[x]);
            }
        }

        if (drawinput)
        {
            Con_DrawInput();
        }
    }

    public static void Con_NotifyBox(char* text)
    {
        double t1, t2;

        Con_Printf(common_c.StringToChar('\n\n\35\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\37\n'));

        Con_Printf(text);

        Con_Printf(common_c.StringToChar("Press a key.\n"));
        Con_Printf('\35\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\37\n');

        keys_c.key_count = -2;
        keys_c.key_dest = keys_c.keydest_t.key_console;

        do
        {
            t1 = sys_win_c.Sys_FloatTime();
            screen_c.SCR_UpdateScreen();
            sys_win_c.Sys_SendKeyEvents();
            t2 = sys_win_c.Sys_FloatTime();
            quakedef_c.realtime += t2 - t1;
        } while (keys_c.key_count < 0);

        Con_Printf(common_c.StringToChar("\n"));
        keys_c.key_dest = keys_c.keydest_t.key_game;
        quakedef_c.realtime = 0;
    }
}