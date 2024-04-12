namespace Quake;

public unsafe class menu_c
{
    public const int MNET_IPX = 1;
    public const int MNET_TCP = 2;

    public static int m_activenet;

    public enum m_state { m_none, m_main, m_singleplayer, m_load, m_save, m_multiplayer, m_setup, m_net, m_options, m_video, m_keys, m_help, m_quit, m_serialconfig, m_modemconfig, m_lanconfig, m_gameoptions, m_search, m_slist }

    public static bool m_entersound;

    public static bool m_recursiveDraw;

    public static int m_return_state;
    public static bool m_return_onerror;
    public static char* m_return_reason;

    public static bool StartingGame = m_multiplayer_cursor == 1 ? true : false;
    public static bool JoiningGame = m_multiplayer_cursor == 0 ? true : false;
    public static bool SerialConfig = m_net_cursor == 0 ? true : false;
    public static bool DirectConfig = m_net_cursor == 1 ? true : false;
    public static bool IPXConfig = m_net_cursor == 2 ? true : false;
    public static bool TCPIPConfig = m_net_cursor == 3 ? true : false;

    public static m_state state;

    public static void M_DrawCharacter(int cx, int line, int num)
    {
        draw_c.Draw_Character(cx + (int)((vid_c.vid.width - 320) >> 1), line, num);
    }

    public static void M_Print(int cx, int cy, string str)
    {
        M_Print(cx, cy, common_c.StringToChar(str));
    }

    public static void M_Print(int cx, int cy, char[] str)
    {
        for (int i = 0; i < str.Length;)
        {
            while (str[i] != 0)
            {
                M_DrawCharacter(cx, cy, str[i] + 128);
                i++;
                cx += 8;
            }
        }
    }

    public static void M_Print(int cx, int cy, char* str)
    {
        while (*str != 0)
        {
            M_DrawCharacter(cx, cy, (*str) + 128);
            str++;
            cx += 8;
        }
    }

    public static void M_Print(int cx, int cy, char str)
    {
        while (str != 0)
        {
            M_DrawCharacter(cx, cy, (str) + 128);
            str++;
            cx += 8;
        }
    }

    public static void M_PrintWhite(int cx, int cy, char* str)
    {
        while (*str != 0)
        {
            M_DrawCharacter(cx, cy, *str);
            str++;
            cx += 8;
        }
    }

    public static void M_DrawTransPic(int x, int y, wad_c.qpic_t* pic)
    {
        draw_c.Draw_TransPic(x + (int)((vid_c.vid.width - 320) >> 1), y, pic);
    }

    public static void M_DrawPic(int x, int y, wad_c.qpic_t* pic)
    {
        draw_c.Draw_Pic(x + (int)((vid_c.vid.width - 320) >> 1), y, pic);
    }

    public static byte* identityTable;
    public static byte* translationTable;

    public static void M_BuildTranslationTable(int top, int bottom)
    {
        int j;
        byte* dest, source;

        for (j = 0; j < 256; j++)
        {
            identityTable[j] = (byte)j;
        }

        dest = translationTable;
        source = identityTable;
        common_c.Q_memcpy(*dest, *source, 256);

        if (top < 128)
        {
            common_c.Q_memcpy(*dest + render_c.TOP_RANGE, *source + top, 16);
        }
        else
        {
            for (j = 0; j < 16; j++)
            {
                dest[render_c.TOP_RANGE + j] = source[top + 15 - j];
            }
        }

        if (bottom < 128)
        {
            common_c.Q_memcpy(*dest + render_c.BOTTOM_RANGE, *source + bottom, 16);
        }
        else
        {
            for (j = 0; j < 16; j++)
            {
                dest[render_c.BOTTOM_RANGE + j] = source[bottom + 15 - j];
            }
        }
    }

    public static void M_DrawTransPicTranslate(int x, int y, wad_c.qpic_t* pic)
    {
        draw_c.Draw_TransPicTranslate(x + (int)((vid_c.vid.width - 320) >> 1), y, pic, translationTable);
    }

    public static void M_DrawTextBox(int x, int y, int width, int lines)
    {
        wad_c.qpic_t* p;
        int cx, cy;
        int n;

        cx = x;
        cy = y;
        p = draw_c.Draw_CachePic("gfx/box_tl.lmp");
        M_DrawTransPic(cx, cy, p);
        p = draw_c.Draw_CachePic("gfx/box_ml.lmp");

        for (n = 0; n < lines; n++)
        {
            cy += 8;
            M_DrawTransPic(cx, cy, p);
        }

        p = draw_c.Draw_CachePic("gfx/box_bl.lmp");
        M_DrawTransPic(cx, cy + 8, p);

        cx += 8;

        while (width > 0)
        {
            cy = y;
            p = draw_c.Draw_CachePic("gfx/box_tm.lmp");
            M_DrawTransPic(cx, cy, p);
            p = draw_c.Draw_CachePic("gfx/box_mm.lmp");

            for (n = 0; n < lines; n++)
            {
                cy += 8;

                if (n == 1)
                {
                    p = draw_c.Draw_CachePic("gfx/box_mm2.lmp");
                }

                M_DrawTransPic(cx, cy, p);
            }

            p = draw_c.Draw_CachePic("gfx/box_bm.lmp");
            M_DrawTransPic(cx, cy + 8, p);
            width -= 2;
            cx += 16;
        }

        cy = y;
        p = draw_c.Draw_CachePic("gfx/box_tr.lmp");
        M_DrawTransPic(cx, cy, p);
        p = draw_c.Draw_CachePic("gfx/box_mr.lmp");

        for (n = 0; n < lines; n++)
        {
            cy += 8;
            M_DrawTransPic(cx, cy, p);
        }

        p = draw_c.Draw_CachePic("gfx/box_br.lmp");
        M_DrawTransPic(cx, cy + 8, p);
    }

    public static int m_save_demonum;

    public static void M_ToggleMenu_f()
    {
        m_entersound = true;

        if (keys_c.key_dest == keys_c.keydest_t.key_menu)
        {
            if (state != m_state.m_main)
            {
                M_Menu_Main_f();
                return;
            }

            keys_c.key_dest = keys_c.keydest_t.key_game;
            state = m_state.m_none;
            return;
        }

        if (keys_c.key_dest == keys_c.keydest_t.key_console)
        {
            console_c.Con_ToggleConsole_f();
        }
        else
        {
            M_Menu_Main_f();
        }
    }

    public static int m_main_cursor;
    public const int MAIN_ITEMS = 5;

    public static void M_Menu_Main_f()
    {
        if (keys_c.key_dest != keys_c.keydest_t.key_menu)
        {
            m_save_demonum = client_c.cls.demonum;
            client_c.cls.demonum = -1;
        }

        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_main;
        m_entersound = true;
    }

    public static void M_Main_Draw()
    {
        int f;
        wad_c.qpic_t* p;

        M_DrawTransPic(16, 4, draw_c.Draw_CachePic("gfx/qplaque.lmp"));
        p = draw_c.Draw_CachePic("gfx/ttl_main.lmp");
        M_DrawPic((320 - p->width) / 2, 4, p);
        M_DrawTransPic(72, 32, draw_c.Draw_CachePic("gfx/mainmenu.lmp"));

        f = (int)(host_c.host_time * 10) % 6;

        M_DrawTransPic(54, 32 + m_main_cursor * 20, draw_c.Draw_CachePic(common_c.va($"gfx/menudot{f + 1}.lmp")));
    }

    public static void M_Main_Key(int key)
    {
        switch (key)
        {
            case keys_c.K_ESCAPE:
                keys_c.key_dest = keys_c.keydest_t.key_game;
                state = m_state.m_none;
                client_c.cls.demonum = m_save_demonum;

                if (client_c.cls.demonum != -1 && !client_c.cls.demoplayback && client_c.cls.state != client_c.cactive_t.ca_connected)
                {
                    cl_main_c.CL_NextDemo();
                }
                break;

            case keys_c.K_DOWNARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");

                if (++m_main_cursor >= MAIN_ITEMS)
                {
                    m_main_cursor = 0;
                }
                break;

            case keys_c.K_UPARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");

                if (--m_main_cursor < 0)
                {
                    m_main_cursor = MAIN_ITEMS - 1;
                }
                break;

            case keys_c.K_ENTER:
                m_entersound = true;

                switch (m_main_cursor)
                {
                    case 0:
                        M_Menu_SinglePlayer_f();
                        break;

                    case 1:
                        M_Menu_MultiPlayer_f();
                        break;

                    case 2:
                        M_Menu_Options_f();
                        break;

                    case 3:
                        M_Menu_Help_f();
                        break;

                    case 4:
                        M_Menu_Quit_f();
                        break;
                }
                break;
        }
    }

    public static int m_singleplayer_cursor;
    public const int SINGLEPLAYER_ITEMS = 3;

    public static void M_Menu_SinglePlayer_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_singleplayer;
        m_entersound = true;
    }

    public static void M_SinglePlayer_Draw()
    {
        int f;
        wad_c.qpic_t* p;

        M_DrawTransPic(16, 4, draw_c.Draw_CachePic("gfx/qplaque.lmp"));
        p = draw_c.Draw_CachePic("gfx/ttl_sgl.lmp");
        M_DrawPic((320 - p->width) / 2, 4, p);
        M_DrawTransPic(72, 32, draw_c.Draw_CachePic("gfx/sp_menu.lmp"));

        f = (int)(host_c.host_time * 10) % 6;

        M_DrawTransPic(54, 32 + m_singleplayer_cursor * 20, draw_c.Draw_CachePic(common_c.va($"gfx/menudot{f + 1}.lmp")));
    }

    public static void M_SinglePlayer_Key(int key)
    {
        switch (key)
        {
            case keys_c.K_ESCAPE:
                M_Menu_Main_f();
                break;

            case keys_c.K_DOWNARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");

                if (++m_singleplayer_cursor >= SINGLEPLAYER_ITEMS)
                {
                    m_singleplayer_cursor = 0;
                }
                break;

            case keys_c.K_UPARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");

                if (--m_singleplayer_cursor < 0)
                {
                    m_singleplayer_cursor = SINGLEPLAYER_ITEMS - 1;
                }
                break;

            case keys_c.K_ENTER:
                m_entersound = true;

                switch (m_singleplayer_cursor)
                {
                    case 0:
                        if (server_c.sv.active)
                        {
                            if (screen_c.SCR_ModalMessage(common_c.StringToChar("Are you sure you want to\nstart a new game?\n")) == 0)
                            {
                                break;
                            }
                        }

                        keys_c.key_dest = keys_c.keydest_t.key_game;

                        if (server_c.sv.active)
                        {
                            cmd_c.Cbuf_AddText(common_c.StringToChar("disconnect\n"));
                        }

                        cmd_c.Cbuf_AddText(common_c.StringToChar("maxplayers 1\n"));
                        cmd_c.Cbuf_AddText(common_c.StringToChar("map start\n"));
                        break;

                    case 1:
                        M_Menu_Load_f();
                        break;

                    case 2:
                        M_Menu_Save_f();
                        break;
                }
                break;
        }
    }

    public static int load_cursor;

    public const int MAX_SAVEGAMES = 12;
    public static char[][] m_filenames = new char[MAX_SAVEGAMES][];
    public static int[] loadable = new int[MAX_SAVEGAMES];

    public static void M_ScanSaves()
    {
        int i, j;
        char[] name = new char[quakedef_c.MAX_OSPATH];
        FileStream f;
        int version;

        for (i = 0; i < MAX_SAVEGAMES; i++)
        {
            common_c.Q_strcpy(m_filenames[i].ToString(), "--- UNUSED SLOT ---");
            loadable[i] = 0;
            Console.WriteLine(name.ToString(), $"{common_c.com_gamedir}/{i}.sav");
            f = File.Open(name.ToString(), FileMode.Open);

            if (f == null)
            {
                continue;
            }

            using (StreamReader reader = new StreamReader($"{common_c.com_gamedir}/{i}.sav"))
            {
                if (int.TryParse(reader.ReadLine(), out version))
                {
                    common_c.Q_strncpy(m_filenames[i], name, (MAX_SAVEGAMES + quakedef_c.SAVEGAME_COMMENT_LENGTH) - 1);
                }
            }

            for (j = 0; j < quakedef_c.SAVEGAME_COMMENT_LENGTH; j++)
            {
                if (m_filenames[i][j] == '_')
                {
                    m_filenames[i][j] = ' ';
                }
            }

            loadable[i] = 1;
            f.Close();
        }
    }

    public static void M_Menu_Load_f()
    {
        m_entersound = true;
        state = m_state.m_load;
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        M_ScanSaves();
    }

    public static void M_Menu_Save_f()
    {
        if (!server_c.sv.active)
        {
            return;
        }

        if (cl_main_c.cl.intermission != 0)
        {
            return;
        }

        if (sv_main_c.svs.maxclients != 1)
        {
            return;
        }

        m_entersound = true;
        state = m_state.m_save;
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        M_ScanSaves();
    }

    public static void M_Load_Draw()
    {
        int i;
        wad_c.qpic_t* p;

        p = draw_c.Draw_CachePic("gfx/p_load.lmp");
        M_DrawPic((320 - p->width) / 2, 4, p);

        for (i = 0; i < MAX_SAVEGAMES; i++)
        {
            M_Print(16, 32 + 8 * i, m_filenames[i][0]);
        }

        M_DrawCharacter(8, 32 + load_cursor * 8, 12 + ((int)(quakedef_c.realtime * 4) & 1));
    }

    public static void M_Save_Draw()
    {
        int i;
        wad_c.qpic_t* p;

        p = draw_c.Draw_CachePic("gfx/p_save.lmp");
        M_DrawPic((320 - p->width) / 2, 4, p);

        for (i = 0; i < MAX_SAVEGAMES; i++)
        {
            M_Print(16, 32 + 8 * i, m_filenames[i][0]);
        }

        M_DrawCharacter(8, 32 + load_cursor * 8, 12 + ((int)(quakedef_c.realtime * 4) & 1));
    }

    public static void M_Load_Key(int k)
    {
        switch (k)
        {
            case keys_c.K_ESCAPE:
                M_Menu_SinglePlayer_f();
                break;

            case keys_c.K_ENTER:
                snd_null_c.S_LocalSound("misc/menu2.wav");

                if (loadable[load_cursor] == 0)
                {
                    return;
                }

                state = m_state.m_none;
                keys_c.key_dest = keys_c.keydest_t.key_game;

                screen_c.SCR_BeginLoadingPlaque();

                cmd_c.Cbuf_AddText(common_c.StringToChar(common_c.va($"load {load_cursor}")));
                return;

            case keys_c.K_UPARROW:
            case keys_c.K_LEFTARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");
                load_cursor--;

                if (load_cursor < 0)
                {
                    load_cursor = MAX_SAVEGAMES - 1;
                }
                break;

            case keys_c.K_DOWNARROW:
            case keys_c.K_RIGHTARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");
                load_cursor++;

                if (load_cursor >= MAX_SAVEGAMES)
                {
                    load_cursor = 0;
                }
                break;
        }
    }

    public static void M_Save_Key(int k)
    {
        switch (k)
        {
            case keys_c.K_ESCAPE:
                M_Menu_SinglePlayer_f();
                break;

            case keys_c.K_ENTER:
                state = m_state.m_none;
                keys_c.key_dest = keys_c.keydest_t.key_game;
                cmd_c.Cbuf_AddText(common_c.StringToChar(common_c.va($"save {load_cursor}\n")));
                return;

            case keys_c.K_UPARROW:
            case keys_c.K_LEFTARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");
                load_cursor--;

                if (load_cursor < 0)
                {
                    load_cursor = MAX_SAVEGAMES - 1;
                }
                break;

            case keys_c.K_DOWNARROW:
            case keys_c.K_RIGHTARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");
                load_cursor++;

                if (load_cursor >= MAX_SAVEGAMES)
                {
                    load_cursor = 0;
                }
                break;
        }
    }

    public static int m_multiplayer_cursor;
    public const int MULTIPLAYER_ITEMS = 3;

    public static void M_Menu_MultiPlayer_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_multiplayer;
        m_entersound = true;
    }

    public static void M_MultiPlayer_Draw()
    {
        int f;
        wad_c.qpic_t* p;

        M_DrawTransPic(16, 4, draw_c.Draw_CachePic("gfx/qplaque.lmp"));
        p = draw_c.Draw_CachePic("gfx/p_multi.lmp");
        M_DrawPic((320 - p->width) / 2, 4, p);
        M_DrawTransPic(72, 32, draw_c.Draw_CachePic("gfx/mp_menu.lmp"));

        f = (int)(host_c.host_time * 10) % 6;

        M_DrawTransPic(54, 32 + m_multiplayer_cursor * 20, draw_c.Draw_CachePic(common_c.va($"gfx/menudot{f + 1}.lmp")));

        if (net_c.serialAvailable || net_c.ipxAvailable || net_c.tcpipAvailable)
        {
            return;
        }

        M_PrintWhite((320 / 2) - ((27 * 8) / 2), 148, common_c.StringToChar("No Communications Available"));
    }

    public static void M_MultiPlayer_Key(int key)
    {
        switch (key)
        {
            case keys_c.K_ESCAPE:
                M_Menu_Main_f();
                break;

            case keys_c.K_DOWNARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");

                if (++m_multiplayer_cursor >= MULTIPLAYER_ITEMS)
                {
                    m_multiplayer_cursor = 0;
                }
                break;

            case keys_c.K_UPARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");

                if (--m_multiplayer_cursor < 0)
                {
                    m_multiplayer_cursor = MULTIPLAYER_ITEMS - 1;
                }
                break;

            case keys_c.K_ENTER:
                m_entersound = true;

                switch (m_multiplayer_cursor)
                {
                    case 0:
                        if (net_c.serialAvailable || net_c.ipxAvailable || net_c.tcpipAvailable)
                        {
                            M_Menu_Net_f();
                        }
                        break;

                    case 1:
                        if (net_c.serialAvailable || net_c.ipxAvailable || net_c.tcpipAvailable)
                        {
                            M_Menu_Net_f();
                        }
                        break;

                    case 2:
                        M_Menu_Setup_f();
                        break;
                }
                break;
        }
    }

    public static int setup_cursor = 4;
    public static int[] setup_cursor_table = new int[] { 40, 56, 80, 104, 140 };

    public static char* setup_hostname;
    public static char* setup_myname;
    public static int setup_oldtop;
    public static int setup_oldbottom;
    public static int setup_top;
    public static int setup_bottom;

    public const int NUM_SETUP_CMDS = 5;

    public static void M_Menu_Setup_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_setup;
        m_entersound = true;
        common_c.Q_strcpy(setup_myname->ToString(), cl_main_c.cl_name.str->ToString());
        common_c.Q_strcpy(setup_hostname->ToString(), net_main_c.hostname.str->ToString());
        setup_top = setup_oldtop = ((int)cl_main_c.cl_color.value) >> 4;
        setup_bottom = setup_oldbottom = ((int)cl_main_c.cl_color.value) & 15;
    }

    public static void M_Setup_Draw()
    {
        wad_c.qpic_t* p;

        M_DrawTransPic(16, 4, draw_c.Draw_CachePic("gfx/qplaque.lmp"));
        p = draw_c.Draw_CachePic("gfx/p_multi.lmp");
        M_DrawPic((320 - p->width) / 2, 4, p);

        M_Print(64, 40, common_c.StringToChar("Hostname"));
        M_DrawTextBox(160, 32, 16, 1);
        M_Print(168, 40, setup_hostname);

        M_Print(64, 40, common_c.StringToChar("Your name"));
        M_DrawTextBox(160, 48, 16, 1);
        M_Print(168, 56, setup_myname);

        M_Print(64, 80, common_c.StringToChar("Shirt color"));
        M_Print(64, 104, common_c.StringToChar("Pants color"));

        M_DrawTextBox(64, 140 - 8, 14, 1);
        M_Print(72, 140, common_c.StringToChar("Accept Changes"));

        p = draw_c.Draw_CachePic("gfx/bigbox.lmp");
        M_DrawTransPic(160, 64, p);
        p = draw_c.Draw_CachePic("gfx/menuplyr.lmp");
        M_BuildTranslationTable(setup_top * 16, setup_bottom * 16);
        M_DrawTransPicTranslate(172, 72, p);

        M_DrawCharacter(56, setup_cursor_table[setup_cursor], 12 + ((int)(quakedef_c.realtime * 4) & 1));

        if (setup_cursor == 0)
        {
            M_DrawCharacter(168 + 8 * common_c.Q_strlen(setup_hostname->ToString()), setup_cursor_table[setup_cursor], 10 + ((int)(quakedef_c.realtime * 4) & 1));
        }

        if (setup_cursor == 1)
        {
            M_DrawCharacter(168 + 8 * common_c.Q_strlen(setup_myname->ToString()), setup_cursor_table[setup_cursor], 10 + ((int)(quakedef_c.realtime * 4) & 1));
        }
    }

    public static void M_Setup_Key(int k)
    {
        int l;

        switch (k)
        {
            case keys_c.K_ESCAPE:
                M_Menu_MultiPlayer_f();
                break;

            case keys_c.K_UPARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");
                setup_cursor--;

                if (setup_cursor < 0)
                {
                    setup_cursor = NUM_SETUP_CMDS - 1;
                }
                break;

            case keys_c.K_DOWNARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");
                setup_cursor++;

                if (setup_cursor >= NUM_SETUP_CMDS)
                {
                    setup_cursor = 0;
                }
                break;

            case keys_c.K_LEFTARROW:
                if (setup_cursor < 2)
                {
                    return;
                }

                snd_null_c.S_LocalSound("misc/menu3.wav");

                if (setup_cursor == 2)
                {
                    setup_top = setup_top - 1;
                }

                if (setup_cursor == 3)
                {
                    setup_bottom = setup_bottom - 1;
                }
                break;

            case keys_c.K_RIGHTARROW:
                if (setup_cursor < 2)
                {
                    return;
                }

            forward:
                snd_null_c.S_LocalSound("misc/menu3.wav");

                if (setup_cursor == 2)
                {
                    setup_top = setup_top + 1;
                }

                if (setup_cursor == 3)
                {
                    setup_bottom = setup_bottom + 1;
                }
                break;

            case keys_c.K_ENTER:
                if (setup_cursor == 0 || setup_cursor == 1)
                {
                    return;
                }

                if (setup_cursor == 2 || setup_cursor == 3)
                {
                    goto forward;
                }

                if (common_c.Q_strcmp(cl_main_c.cl_name.str->ToString(), setup_myname->ToString()))
                {
                    cmd_c.Cbuf_AddText(common_c.StringToChar(common_c.va($"name \"{setup_myname->ToString()}\"\n")));
                }

                if (common_c.Q_strcmp(net_main_c.hostname.str->ToString(), setup_hostname->ToString()))
                {
                    cvar_c.Cvar_Set("hostname", setup_hostname->ToString());
                }

                if (setup_top != setup_oldtop || setup_bottom != setup_oldbottom)
                {
                    cmd_c.Cbuf_AddText(common_c.StringToChar(common_c.va($"color {setup_top} {setup_bottom}\n")));
                }

                m_entersound = true;
                M_Menu_MultiPlayer_f();
                break;

            case keys_c.K_BACKSPACE:
                if (setup_cursor == 0)
                {
                    if (common_c.Q_strlen(setup_hostname->ToString()) != 0)
                    {
                        setup_hostname[common_c.Q_strlen(setup_hostname->ToString()) - 1] = (char)0;
                    }
                }

                if (setup_cursor == 1)
                {
                    if (common_c.Q_strlen(setup_myname->ToString()) != 0)
                    {
                        setup_myname[common_c.Q_strlen(setup_myname->ToString()) - 1] = (char)0;
                    }
                }
                break;

            default:
                if (k < 32 || k > 127)
                {
                    break;
                }

                if (setup_cursor == 0)
                {
                    l = common_c.Q_strlen(setup_hostname->ToString());

                    if (l < 15)
                    {
                        setup_myname[l + 1] = (char)0;
                        setup_hostname[l] = (char)k;
                    }
                }

                if (setup_cursor == 1)
                {
                    l = common_c.Q_strlen(setup_myname->ToString());

                    if (l < 15)
                    {
                        setup_myname[l + 1] = (char)0;
                        setup_hostname[l] = (char)k;
                    }
                }

                break;
        }

        if (setup_top > 13)
        {
            setup_top = 0;
        }

        if (setup_top < 0)
        {
            setup_top = 13;
        }

        if (setup_bottom > 13)
        {
            setup_bottom = 0;
        }

        if (setup_bottom < 0)
        {
            setup_bottom = 13;
        }
    }

    public static int m_net_cursor;
    public static int m_net_items;
    public static int m_net_saveHeight;

    public static char*[] net_helpMessage = new char*[]
    {
        common_c.StringToChar("                        "),
        common_c.StringToChar(" Two computers connected"),
        common_c.StringToChar("   through two modems.  "),
        common_c.StringToChar("                        "),

        common_c.StringToChar("                        "),
        common_c.StringToChar(" Two computers connected"),
        common_c.StringToChar(" by a null-modem cable. "),
        common_c.StringToChar("                        "),

        common_c.StringToChar(" Novell network LANs    "),
        common_c.StringToChar(" or Windows 95 DOS-box. "),
        common_c.StringToChar("                        "),
        common_c.StringToChar("(LAN=Local Area Network)"),

        common_c.StringToChar(" Commonly used to play  "),
        common_c.StringToChar(" over the Internet, but "),
        common_c.StringToChar(" also used on a Local   "),
        common_c.StringToChar(" Area Network.          "),
    };

    public static void M_Menu_Net_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_net;
        m_entersound = true;
        m_net_items = 4;

        if (m_net_cursor >= m_net_items)
        {
            m_net_cursor = 0;
        }

        m_net_cursor--;
        M_Net_Key(keys_c.K_DOWNARROW);
    }

    public static void M_Net_Draw()
    {
        int f;
        wad_c.qpic_t* p;

        M_DrawTransPic(16, 4, draw_c.Draw_CachePic("gfx/qplaque.lmp"));
        p = draw_c.Draw_CachePic("gfx/p_multi.lmp");
        M_DrawPic((320 - p->width) / 2, 4, p);

        f = 32;

        if (net_main_c.serialAvailable)
        {
            p = draw_c.Draw_CachePic("gfx/netmen1.lmp");
        }
        else
        {
#if _WIN32
            p = null;
#else
            p = draw_c.Draw_CachePic("gfx/dim_modm.lmp");
#endif
        }

        if (p != null)
        {
            M_DrawTransPic(72, f, p);
        }

        f += 19;

        if (net_main_c.serialAvailable)
        {
            p = draw_c.Draw_CachePic("gfx/netmen2.lmp");
        }
        else
        {
#if _WIN32
            p = null;
#else
            p = draw_c.Draw_CachePic("gfx/dim_drct.lmp");
#endif
        }

        if (p != null)
        {
            M_DrawTransPic(72, f, p);
        }

        f += 19;

        if (net_main_c.ipxAvailable)
        {
            p = draw_c.Draw_CachePic("gfx/netmen3.lmp");
        }
        else
        {
            p = draw_c.Draw_CachePic("gfx/dim_ipx.lmp");
        }

        M_DrawTransPic(72, f, p);

        f += 19;

        if (net_main_c.tcpipAvailable)
        {
            p = draw_c.Draw_CachePic("gfx/netmen4.lmp");
        }
        else
        {
            p = draw_c.Draw_CachePic("gfx/dim_tcp.lmp");
        }

        M_DrawTransPic(72, f, p);

        if (m_net_items == 5)
        {
            f += 19;
            p = draw_c.Draw_CachePic("gfx/netmen5.lmp");
            M_DrawTransPic(72, f, p);
        }

        f = (320 - 26 * 8) / 2;
        M_DrawTextBox(f, 134, 24, 4);
        f += 8;
        M_Print(f, 142, net_helpMessage[m_net_cursor * 4 + 0]);
        M_Print(f, 150, net_helpMessage[m_net_cursor * 4 + 1]);
        M_Print(f, 148, net_helpMessage[m_net_cursor * 4 + 2]);
        M_Print(f, 166, net_helpMessage[m_net_cursor * 4 + 3]);

        f = (int)(server_c.host_time * 10) % 6;
        M_DrawTransPic(54, 32 + m_net_cursor * 20, draw_c.Draw_CachePic(common_c.va($"gfx/menudot{f + 1}.lmp")));
    }

    public static void M_Net_Key(int k)
    {
    again:
        switch (k)
        {
            case keys_c.K_ESCAPE:
                M_Menu_MultiPlayer_f();
                break;

            case keys_c.K_DOWNARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");

                if (++m_net_cursor >= m_net_items)
                {
                    m_net_cursor = 0;
                }
                break;

            case keys_c.K_UPARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");

                if (--m_net_cursor < 0)
                {
                    m_net_cursor = m_net_items - 1;
                }
                break;

            case keys_c.K_ENTER:
                m_entersound = true;

                switch (m_net_cursor)
                {
                    case 0:
                        M_Menu_SerialConfig_f();
                        break;

                    case 1:
                        M_Menu_SerialConfig_f();
                        break;

                    case 2:
                        M_Menu_LanConfig_f();
                        break;

                    case 3:
                        M_Menu_LanConfig_f();
                        break;

                    case 4:
                        break;
                }
                break;
        }

        if (m_net_cursor == 0 && !net_main_c.serialAvailable)
        {
            goto again;
        }

        if (m_net_cursor == 1 && !net_main_c.serialAvailable)
        {
            goto again;
        }

        if (m_net_cursor == 2 && !net_main_c.ipxAvailable)
        {
            goto again;
        }

        if (m_net_cursor == 3 && !net_main_c.tcpipAvailable)
        {
            goto again;
        }
    }

#if _WIN32
    public const int OPTIONS_ITEMS = 14;
#else
    public const int OPTIONS_ITEMS = 13;
#endif

    public const int SLIDER_RANGE = 10;

    public static int options_cursor;

    public static void M_Menu_Options_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_options;
        m_entersound = true;

#if _WIN32
        if ((options_cursor == 13) && (vid_win_c.modestate != winquake_c.MS_WINDOWED)) 
        {
            options_cursor = 0;
        }
#endif
    }

    public static void M_AdjustSliders(int dir)
    {
        snd_null_c.S_LocalSound("misc/menu3.wav");

        switch (options_cursor)
        {
            case 3:
                screen_c.scr_viewsize.value += (char)(dir * 10);

                if (screen_c.scr_viewsize.value < 30)
                {
                    screen_c.scr_viewsize.value = (char)30;
                }

                if (screen_c.scr_viewsize.value > 120)
                {
                    screen_c.scr_viewsize.value = (char)120;
                }

                cvar_c.Cvar_SetValue("viewsize", screen_c.scr_viewsize.value);
                break;

            case 4:
                view_c.v_gamma.value -= (char)(dir * 0.05);

                if (view_c.v_gamma.value < 0.5)
                {
                    view_c.v_gamma.value = (char)0.5;
                }

                if (view_c.v_gamma.value > 1)
                {
                    view_c.v_gamma.value = (char)1;
                }

                cvar_c.Cvar_SetValue("gamma", view_c.v_gamma.value);
                break;

            case 5:
                cl_main_c.sensitivity.value += (char)(dir * 0.5);

                if (cl_main_c.sensitivity.value < 1)
                {
                    cl_main_c.sensitivity.value = (char)1;
                }

                if (cl_main_c.sensitivity.value > 11)
                {
                    cl_main_c.sensitivity.value = (char)11;
                }

                cvar_c.Cvar_SetValue("sensitivity", cl_main_c.sensitivity.value);
                break;

            case 6:
#if _WIN32
                snd_null_c.bgmvolume.value += (char)(dir * 1.0);
#else
                snd_null_c.bgmvolume.value += (char)(dir * 0.1);
#endif
                if (snd_null_c.bgmvolume.value < 0)
                {
                    snd_null_c.bgmvolume.value = (char)0;
                }

                if (snd_null_c.bgmvolume.value > 1)
                {
                    snd_null_c.bgmvolume.value = (char)1;
                }

                cvar_c.Cvar_SetValue("bgmvolume", (float)snd_null_c.bgmvolume.value);
                break;

            case 7:
                snd_null_c.volume.value += (char)(dir * 0.1);

                if (snd_null_c.volume.value < 0)
                {
                    snd_null_c.volume.value = (char)0;
                }

                if (snd_null_c.volume.value > 1)
                {
                    snd_null_c.volume.value = (char)1;
                }

                cvar_c.Cvar_SetValue("volume", (float)snd_null_c.volume.value);
                break;

            case 8:
                if (cl_input_c.cl_forwardspeed.value > 200)
                {
                    cvar_c.Cvar_SetValue("cl_forwardspeed", 200);
                    cvar_c.Cvar_SetValue("cl_backspeed", 200);
                }
                else
                {
                    cvar_c.Cvar_SetValue("cl_forwardspeed", 400);
                    cvar_c.Cvar_SetValue("cl_backspeed", 400);
                }
                break;

            case 9:
                cvar_c.Cvar_SetValue("m_pitch", -cl_main_c.m_pitch.value);
                break;

            case 10:
                cvar_c.Cvar_SetValue("lookspring", -cl_main_c.lookspring.value);
                break;

            case 11:
                cvar_c.Cvar_SetValue("lookstrafe", -cl_main_c.lookstrafe.value);
                break;

#if _WIN32
			case 13:
				cvar_c.Cvar_SetValue("_windowed_mouse", -vid_win_c._windowed_mouse.value);
				break;
#endif
        }
    }

    public static void M_DrawSlider(int x, int y, float range)
    {
        int i;

        if (range < 0)
        {
            range = 0;
        }

        if (range > 1)
        {
            range = 1;
        }

        M_DrawCharacter(x - 8, y, 128);

        for (i = 0; i < SLIDER_RANGE; i++)
        {
            M_DrawCharacter(x + i * 8, y, 129);
        }

        M_DrawCharacter(x + i * 8, y, 130);
        M_DrawCharacter((int)(x + (SLIDER_RANGE - 1) * 8 * range), y, 131);
    }

    public static void M_DrawCheckbox(int x, int y, int on)
    {
        if (on != 0)
        {
            M_Print(x, y, common_c.StringToChar("on"));
        }
        else
        {
            M_Print(x, y, common_c.StringToChar("off"));
        }
    }

    public static void M_Options_Draw()
    {
        float r;
        wad_c.qpic_t* p;

        M_DrawTransPic(16, 4, draw_c.Draw_CachePic("gfx/qplaque.lmp"));
        p = draw_c.Draw_CachePic("gfx/p_option.lmp");
        M_DrawPic((320 - p->width) / 2, 4, p);

        M_Print(16, 32, common_c.StringToChar("    Customize controls"));
        M_Print(16, 40, common_c.StringToChar("         Go to console"));
        M_Print(16, 48, common_c.StringToChar("     Reset to defaults"));

        M_Print(16, 56, common_c.StringToChar("           Screen size"));
        r = (screen_c.scr_viewsize.value - 30) / (120 - 30);
        M_DrawSlider(220, 56, r);

        M_Print(16, 64, common_c.StringToChar("            Brightness"));
        r = (1.0f - view_c.v_gamma.value) / 0.5f;
        M_DrawSlider(220, 64, r);

        M_Print(16, 72, common_c.StringToChar("           Mouse Speed"));
        r = (cl_main_c.sensitivity.value - 1) / 10;
        M_DrawSlider(220, 72, r);

        M_Print(16, 80, common_c.StringToChar("       CD Music Volume"));
        r = snd_null_c.bgmvolume.value;
        M_DrawSlider(220, 80, r);

        M_Print(16, 88, common_c.StringToChar("          Sound Volume"));
        r = snd_null_c.volume.value;
        M_DrawSlider(220, 88, r);

        M_Print(16, 96, common_c.StringToChar("            Always Run"));
        M_DrawCheckbox(220, 96, cl_input_c.cl_forwardspeed.value > 200 ? 1 : 0);

        M_Print(16, 104, common_c.StringToChar("          Invert Mouse"));
        M_DrawCheckbox(220, 104, cl_main_c.m_pitch.value < 0 ? 1 : 0);

        M_Print(16, 112, common_c.StringToChar("            Lookspring"));
        M_DrawCheckbox(220, 112, cl_main_c.lookspring.value != 0 ? 1 : 0);

        M_Print(16, 120, common_c.StringToChar("            Lookstrafe"));
        M_DrawCheckbox(220, 120, cl_main_c.lookstrafe.value != 0 ? 1 : 0);

        M_Print(16, 128, common_c.StringToChar("         Video Options"));

#if _WIN32
		if (vid_win_c.modestate == MS_WINDOWED) 
		{
			M_Print(16, 136, common_c.StringToChar("             Use Mouse"));
			M_DrawCheckbox(220, 136, _windowed_mouse.value);
		}
#endif

        M_DrawCharacter(200, 32 + options_cursor * 8, 12 + ((int)(quakedef_c.realtime * 4) & 1));
    }

    public static void M_Options_Key(int k)
    {
        switch (k)
        {
            case keys_c.K_ESCAPE:
                M_Menu_Main_f();
                break;

            case keys_c.K_ENTER:
                m_entersound = true;

                switch (options_cursor)
                {
                    case 0:
                        M_Menu_Keys_f();
                        break;

                    case 1:
                        state = m_state.m_none;
                        console_c.Con_ToggleConsole_f();
                        break;

                    case 2:
                        cmd_c.Cbuf_AddText(common_c.StringToChar("exec default.cfg\n"));
                        break;

                    case 12:
                        M_Menu_Video_f();
                        break;

                    default:
                        M_AdjustSliders(1);
                        break;
                }
                return;

            case keys_c.K_UPARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");
                options_cursor--;

                if (options_cursor < 0)
                {
                    options_cursor = OPTIONS_ITEMS - 1;
                }
                break;

            case keys_c.K_DOWNARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");
                options_cursor++;

                if (options_cursor >= OPTIONS_ITEMS)
                {
                    options_cursor = 0;
                }
                break;

            case keys_c.K_LEFTARROW:
                M_AdjustSliders(-1);
                break;

            case keys_c.K_RIGHTARROW:
                M_AdjustSliders(1);
                break;
        }

        vid_c.vid_menudrawfn menudrawn = null;

        if (options_cursor == 12 && menudrawn == null)
        {
            if (k == keys_c.K_UPARROW)
            {
                options_cursor = 11;
            }
            else
            {
                options_cursor = 0;
            }
        }

#if _WIN32
		if ((options_cursor == 13) && (vid_win_c.modestate != MS_WINDOWED)) 
		{
			if (k == keys_c.K_UPARROW) 
			{
				options_cursor = 12;
			}
			else 
			{
				options_cursor = 0;
			}
		}
#endif
    }

    public static string[,] bindnames = new string[,]
    {
        { "+attack", "attack" },
        { "impulse 10", "change weapon"},
        { "+jump", "jump / swim up"},
        { "+forward", "walk forward"},
        {"+back", "backpedal"},
        {"+left", "turn left"},
        {"+right", "turn right"},
        {"+speed", "run"},
        {"+moveleft", "step left" },
        {"+moveright", "step right"},
        {"+strafe", "sidestep"},
        {"+lookup", "look up"},
        {"+lookdown","look down"},
        {"centerview", "center view"},
        {"+mlook", "mouse look"},
        {"+klook", "keyboard look"},
        {"+moveup", "swim up"},
        {"+movedown", "swim down" },
    };

    public const int NUMCOMMANDS = 18;

    public static int keys_cursor;
    public static int bind_grab;

    public static void M_Menu_Keys_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_keys;
        m_entersound = true;
    }

    public static void M_FindKeysForCommand(char* command, int* twokeys)
    {
        int count;
        int j;
        int l;
        char* b;

        twokeys[0] = twokeys[1] = -1;
        l = common_c.Q_strlen(command->ToString());
        count = 0;

        for (j = 0; j < 256; j++)
        {
            b = &keys_c.keybindings[j];

            if (b == null)
            {
                continue;
            }

            if (common_c.Q_strncmp(b, command, l) == 0)
            {
                twokeys[count] = j;
                count++;

                if (count == 2)
                {
                    break;
                }
            }
        }
    }

    public static void M_UnbindCommand(char* command)
    {
        int j;
        int l;
        char* b;

        l = common_c.Q_strlen(command->ToString());

        for (j = 0; j < 256; j++)
        {
            b = &keys_c.keybindings[j];

            if (b == null)
            {
                continue;
            }

            if (common_c.Q_strncmp(b, command, l) == 0)
            {
                keys_c.Key_SetBinding(j, common_c.StringToChar(""));
            }
        }
    }

    public static void M_Keys_Draw()
    {
        int i, l;
        int* keys = null;
        char* name;
        int x, y;
        wad_c.qpic_t* p;

        p = draw_c.Draw_CachePic("gfx/ttl_cstm.lmp");
        M_DrawPic((320 - p->width) / 2, 4, p);

        if (bind_grab != 0)
        {
            M_Print(12, 32, common_c.StringToChar("Press a key or button for this action"));
        }
        else
        {
            M_Print(18, 32, common_c.StringToChar("Enter to change, backspace to clear"));
        }

        for (i = 0; i < NUMCOMMANDS; i++)
        {
            y = 48 + 8 * i;

            M_Print(16, y, common_c.StringToChar(bindnames[i, 1]));

            l = common_c.Q_strlen(bindnames[i, 0]);

            M_FindKeysForCommand(common_c.StringToChar(bindnames[i, 0]), keys);

            if (keys[0] == -1)
            {
                M_Print(140, y, common_c.StringToChar("???"));
            }
            else
            {
                name = common_c.StringToChar(keys_c.Key_KeynumToString(keys[0]));
                M_Print(140, y, name);
                x = common_c.Q_strlen(name->ToString()) * 8;

                if (keys[1] != -1)
                {
                    M_Print(140 + x + 8, y, common_c.StringToChar("or"));
                    M_Print(140 + x + 32, y, common_c.StringToChar(keys_c.Key_KeynumToString(keys[1])));
                }
            }
        }

        if (bind_grab != 0)
        {
            M_DrawCharacter(130, 48 + keys_cursor * 8, '=');
        }
        else
        {
            M_DrawCharacter(130, 48 + keys_cursor * 8, 12 + ((int)(quakedef_c.realtime * 4) & 1));
        }
    }

    public static void M_Keys_Key(int k)
    {
        char* cmd = null;
        int* keys = null;

        if (bind_grab != 0)
        {
            snd_null_c.S_LocalSound("misc/menu1.wav");

            if (k == keys_c.K_ESCAPE)
            {
                bind_grab = 0;
            }
            else if (k != '`')
            {
                Console.WriteLine(cmd->ToString(), $"bind \"{keys_c.Key_KeynumToString(k)}\" \"{bindnames[keys_cursor, 0]}\"\n");
                cmd_c.Cbuf_AddText(cmd);
            }

            bind_grab = 0;
            return;
        }

        switch (k)
        {
            case keys_c.K_ESCAPE:
                M_Menu_Options_f();
                break;

            case keys_c.K_LEFTARROW:
            case keys_c.K_UPARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");
                keys_cursor--;

                if (keys_cursor < 0)
                {
                    keys_cursor = NUMCOMMANDS - 1;
                }
                break;

            case keys_c.K_DOWNARROW:
            case keys_c.K_RIGHTARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");
                keys_cursor++;

                if (keys_cursor >= NUMCOMMANDS)
                {
                    keys_cursor = 0;
                }
                break;

            case keys_c.K_ENTER:
                M_FindKeysForCommand(common_c.StringToChar(bindnames[keys_cursor, 0]), keys);
                snd_null_c.S_LocalSound("misc/menu2.wav");

                if (keys[1] != -1)
                {
                    M_UnbindCommand(common_c.StringToChar(bindnames[keys_cursor, 0]));
                }

                bind_grab = 1;
                break;

            case keys_c.K_BACKSPACE:
            case keys_c.K_DEL:
                snd_null_c.S_LocalSound("misc/menu2.wav");
                M_UnbindCommand(common_c.StringToChar(bindnames[keys_cursor, 0]));
                break;
        }
    }

    public static void M_Menu_Video_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_video;
        m_entersound = true;
    }

    public static void M_Video_Draw()
    {
        //(*vid_c.vid_menudrawfn)();
    }

    public static void M_Video_Key(int key)
    {
        //(*vid_c.vid_menukeyfn)(key);
    }

    public static int help_page;
    public const int NUM_HELP_PAGES = 6;

    public static void M_Menu_Help_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_help;
        m_entersound = true;
        help_page = 0;
    }

    public static void M_Help_Draw()
    {
        M_DrawPic(0, 0, draw_c.Draw_CachePic(common_c.va("gfx/help%i.lmp", help_page)));
    }

    public static void M_Help_Key(int key)
    {
        switch (key)
        {
            case keys_c.K_ESCAPE:
                M_Menu_Main_f();
                break;

            case keys_c.K_UPARROW:
            case keys_c.K_RIGHTARROW:
                m_entersound = true;

                if (++help_page > NUM_HELP_PAGES)
                {
                    help_page = 0;
                }
                break;

            case keys_c.K_DOWNARROW:
            case keys_c.K_LEFTARROW:
                m_entersound = true;

                if (--help_page < 0)
                {
                    help_page = NUM_HELP_PAGES - 1;
                }
                break;
        }
    }

    public static int msgNumber;
    public static int m_quit_prevstate;
    public static bool wasInMenus;

#if !_WIN32
    public static string[] quitMessage = new string[]
    {
          "  Are you gonna quit    ",
  "  this game just like   ",
  "   everything else?     ",
  "                        ",

  " Milord, methinks that  ",
  "   thou art a lowly     ",
  " quitter. Is this true? ",
  "                        ",

  " Do I need to bust your ",
  "  face open for trying  ",
  "        to quit?        ",
  "                        ",

  " Man, I oughta smack you",
  "   for trying to quit!  ",
  "     Press Y to get     ",
  "      smacked out.      ",

  " Press Y to quit like a ",
  "   big loser in life.   ",
  "  Press N to stay proud ",
  "    and successful!     ",

  "   If you press Y to    ",
  "  quit, I will summon   ",
  "  Satan all over your   ",
  "      hard drive!       ",

  "  Um, Asmodeus dislikes ",
  " his children trying to ",
  " quit. Press Y to return",
  "   to your Tinkertoys.  ",

  "  If you quit now, I'll ",
  "  throw a blanket-party ",
  "   for you next time!   ",
  "                        "
    };
#endif

    public static void M_Menu_Quit_f()
    {
        if (state == m_state.m_quit)
        {
            return;
        }

        wasInMenus = (keys_c.key_dest == keys_c.keydest_t.key_menu) ? true : false;
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        m_quit_prevstate = (int)state;
        state = m_state.m_quit;
        m_entersound = true;
        msgNumber = rand_c.rand() % 8;
    }

    public static void M_Quit_Key(int key)
    {
        switch (key)
        {
            case keys_c.K_ESCAPE:
            case 'n':
            case 'N':
                if (wasInMenus)
                {
                    state = (m_state)m_quit_prevstate;
                    m_entersound = true;
                }
                else
                {
                    keys_c.key_dest = keys_c.keydest_t.key_game;
                    state = m_state.m_none;
                }
                break;

            case 'Y':
            case 'y':
                keys_c.key_dest = keys_c.keydest_t.key_console;
                host_cmd_c.Host_Quit_f();
                break;

            default:
                break;
        }
    }

    public static void M_Quit_Draw()
    {
        if (wasInMenus)
        {
            state = (m_state)m_quit_prevstate;
            m_recursiveDraw = true;
            M_Draw();
            state = m_state.m_quit;
        }

        M_DrawTextBox(56, 76, 24, 4);
        M_Print(64, 84, common_c.StringToChar(quitMessage[msgNumber * 4 + 0]));
        M_Print(64, 92, common_c.StringToChar(quitMessage[msgNumber * 4 + 1]));
        M_Print(64, 100, common_c.StringToChar(quitMessage[msgNumber * 4 + 2]));
        M_Print(64, 108, common_c.StringToChar(quitMessage[msgNumber * 4 + 3]));
    }

    public static int serialConfig_cursor;
    public static int[] serialConfig_cursor_table = new int[] { 48, 64, 80, 96, 112, 132 };
    public const int NUM_SERIALCONFIG_CMDS = 6;

    public static int[] ISA_uarts = new int[] { 0x3f8, 0x2f8, 0x3e8, 0x2e8 };
    public static int[] ISA_IRQs = new int[] { 4, 3, 4, 3 };
    public static int[] serialConfig_baudrate = new int[] { 9600, 14400, 19200, 28800, 38400, 57600 };

    public static int serialConfig_comport;
    public static int serialConfig_irq;
    public static int serialConfig_baud;
    public static char* serialConfig_phone;

    public static void M_Menu_SerialConfig_f()
    {
        int n;
        int port;
        int baudrate;
        bool useModem;

        port = baudrate = 0;
        useModem = false;

        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_serialconfig;
        m_entersound = true;

        if (JoiningGame && SerialConfig)
        {
            serialConfig_cursor = 4;
        }
        else
        {
            serialConfig_cursor = 5;
        }

        net_main_c.GetComPortConfig(0, port, serialConfig_irq, baudrate, useModem);

        for (n = 0; n < 4; n++)
        {
            if (ISA_uarts[n] == port)
            {
                break;
            }
        }

        if (n == 4)
        {
            n = 0;
            serialConfig_irq = n + 1;
        }

        serialConfig_comport = n + 1;

        for (n = 0; n < 6; n++)
        {
            if (serialConfig_baudrate[n] == baudrate)
            {
                break;
            }
        }

        if (n == 6)
        {
            n = 5;
        }

        serialConfig_baud = n;

        m_return_onerror = false;
        m_return_reason[0] = '0';
    }

    public static void M_SerialConfig_Draw()
    {
        wad_c.qpic_t* p;
        int basex;
        char* startJoin;
        char* directModem;

        M_DrawTransPic(16, 4, draw_c.Draw_CachePic("gfx/qplaque.lmp"));
        p = draw_c.Draw_CachePic("gfx/p_multi.lmp");
        basex = (320 - p->width) / 2;
        M_DrawPic(basex, 4, p);

        if (StartingGame)
        {
            startJoin = common_c.StringToChar("New Game");
        }
        else
        {
            startJoin = common_c.StringToChar("Join Game");
        }

        if (SerialConfig)
        {
            directModem = common_c.StringToChar("Modem");
        }
        else
        {
            directModem = common_c.StringToChar("Direct Connect");
        }

        M_Print(basex, 32, common_c.StringToChar(common_c.va($"{*startJoin} - {*directModem}")));
        basex += 8;

        M_Print(basex, serialConfig_cursor_table[0], common_c.StringToChar("Port"));
        M_DrawTextBox(160, 40, 4, 1);
        M_Print(168, serialConfig_cursor_table[1], common_c.StringToChar(common_c.va($"COM{serialConfig_comport}")));

        M_Print(basex, serialConfig_cursor_table[1], common_c.StringToChar("IRQ"));
        M_DrawTextBox(160, serialConfig_cursor_table[1] - 8, 1, 1);
        M_Print(168, serialConfig_cursor_table[1], common_c.StringToChar(common_c.va($"{serialConfig_irq}")));

        M_Print(basex, serialConfig_cursor_table[2], common_c.StringToChar("Baud"));
        M_DrawTextBox(160, serialConfig_cursor_table[2] - 8, 5, 1);
        M_Print(168, serialConfig_cursor_table[2], common_c.StringToChar(common_c.va($"{serialConfig_baudrate[serialConfig_baud]}")));

        if (SerialConfig)
        {
            M_Print(basex, serialConfig_cursor_table[3], common_c.StringToChar("Modem Setup..."));

            if (JoiningGame)
            {
                M_Print(basex, serialConfig_cursor_table[4], common_c.StringToChar("Phone number"));
                M_DrawTextBox(160, serialConfig_cursor_table[4] - 8, 16, 1);
                M_Print(168, serialConfig_cursor_table[4], serialConfig_phone);
            }
        }

        if (JoiningGame)
        {
            M_DrawTextBox(basex, serialConfig_cursor_table[5] - 8, 7, 1);
            M_Print(basex + 8, serialConfig_cursor_table[5], common_c.StringToChar("Connect"));
        }
        else
        {
            M_DrawTextBox(basex, serialConfig_cursor_table[5] - 8, 2, 1);
            M_Print(basex + 8, serialConfig_cursor_table[5], common_c.StringToChar("OK"));
        }

        M_DrawCharacter(basex - 8, serialConfig_cursor_table[serialConfig_cursor], 12 + ((int)(host_c.realtime * 4) & 1));

        if (serialConfig_cursor == 4)
        {
            M_DrawCharacter(168 + 8 * common_c.Q_strlen(serialConfig_phone->ToString()), serialConfig_cursor_table[serialConfig_cursor], 10 + ((int)(host_c.realtime * 4) & 1));
        }

        if (*m_return_reason != 0)
        {
            M_PrintWhite(basex, 148, m_return_reason);
        }
    }

    public static void M_SerialConfig_Key(int key)
    {
        int l;

        switch (key)
        {
            case keys_c.K_ESCAPE:
                M_Menu_Net_f();
                break;

            case keys_c.K_UPARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");
                serialConfig_cursor--;

                if (serialConfig_cursor < 0)
                {
                    serialConfig_cursor = NUM_SERIALCONFIG_CMDS - 1;
                }
                break;

            case keys_c.K_DOWNARROW:
                snd_null_c.S_LocalSound("misc/menu1.wav");
                serialConfig_cursor++;

                if (serialConfig_cursor >= NUM_SERIALCONFIG_CMDS)
                {
                    serialConfig_cursor = 0;
                }
                break;

            case keys_c.K_LEFTARROW:
                if (serialConfig_cursor > 2)
                {
                    break;
                }

                snd_null_c.S_LocalSound("misc/menu3.wav");

                if (serialConfig_cursor == 0)
                {
                    serialConfig_comport--;

                    if (serialConfig_comport == 0)
                    {
                        serialConfig_comport = 4;
                    }

                    serialConfig_comport = ISA_IRQs[serialConfig_comport - 1];
                }

                if (serialConfig_cursor == 1)
                {
                    serialConfig_irq--;

                    if (serialConfig_irq == 6)
                    {
                        serialConfig_irq = 5;
                    }

                    if (serialConfig_irq == 1)
                    {
                        serialConfig_irq = 7;
                    }
                }

                if (serialConfig_cursor == 2)
                {
                    serialConfig_baud--;

                    if (serialConfig_baud < 0)
                    {
                        serialConfig_baud = 5;
                    }
                }

                break;

            case keys_c.K_RIGHTARROW:
                if (serialConfig_cursor > 2)
                {
                    break;
                }

            forward:
                snd_null_c.S_LocalSound("misc/menu3.wav");

                if (serialConfig_cursor == 0)
                {
                    serialConfig_comport++;

                    if (serialConfig_comport > 4)
                    {
                        serialConfig_comport = 1;
                    }

                    serialConfig_irq = ISA_IRQs[serialConfig_comport - 1];
                }

                if (serialConfig_cursor == 1)
                {
                    serialConfig_irq++;

                    if (serialConfig_irq == 6)
                    {
                        serialConfig_irq = 7;
                    }

                    if (serialConfig_irq == 8)
                    {
                        serialConfig_irq = 2;
                    }
                }

                if (serialConfig_irq == 2)
                {
                    serialConfig_baud++;

                    if (serialConfig_baud > 5)
                    {
                        serialConfig_baud = 0;
                    }
                }

                break;

            case keys_c.K_ENTER:
                if (serialConfig_cursor < 3)
                {
                    goto forward;
                }

                m_entersound = true;

                if (serialConfig_cursor == 3)
                {
                    net_main_c.SetComPortConfig(0, ISA_uarts[serialConfig_comport - 1], serialConfig_irq, serialConfig_baudrate[serialConfig_baud], SerialConfig);

                    M_Menu_SerialConfig_f();
                    break;
                }

                if (serialConfig_cursor == 4)
                {
                    serialConfig_cursor = 5;
                    break;
                }

                net_main_c.SetComPortConfig(0, ISA_uarts[serialConfig_comport - 1], serialConfig_irq, serialConfig_baudrate[serialConfig_baud], SerialConfig);

                M_ConfigureNetSubsystem();

                if (StartingGame)
                {
                    M_Menu_GameOptions_f();
                    break;
                }

                m_return_state = (int)m_state.m_net;
                m_return_onerror = true;
                keys_c.key_dest = keys_c.keydest_t.key_game;
                //m_state = m_state.m_none;

                if (SerialConfig)
                {
                    cmd_c.Cbuf_AddText(common_c.va($"connect \"{*serialConfig_phone}\"\n"));
                }
                else
                {
                    cmd_c.Cbuf_AddText("connect\n");
                }
                break;

            case keys_c.K_BACKSPACE:
                if (serialConfig_cursor == 4)
                {
                    if (common_c.Q_strlen(serialConfig_phone) != 0)
                    {
                        serialConfig_phone[common_c.Q_strlen(serialConfig_phone) - 1] = (char)0;
                    }
                }

                break;

            default:
                if (key < 32 || key > 127)
                {
                    break;
                }

                if (serialConfig_cursor == 4)
                {
                    l = strlen_c.strlen(serialConfig_phone);

                    if (l < 15)
                    {
                        serialConfig_phone[l + 1] = (char)0;
                        serialConfig_phone[l] = (char)key;
                    }
                }

                break;
        }

        if (DirectConfig && (serialConfig_cursor == 3 || serialConfig_cursor == 4))
        {
            if (key == keys_c.K_UPARROW)
            {
                serialConfig_cursor = 2;
            }
            else
            {
                serialConfig_cursor = 5;
            }
        }

        if (SerialConfig && StartingGame && serialConfig_cursor == 4)
        {
            if (key == keys_c.K_UPARROW)
            {
                serialConfig_cursor = 3;
            }
            else
            {
                serialConfig_cursor = 5;
            }
        }
    }

    public static int modemConfig_cursor;
    public static int[] modemConfig_cursor_table = { 40, 56, 88, 120, 156 };
    public const int NUM_MODEMCONFIG_CMDS = 5;

    public static char modemConfig_dialing;
    public static char* modemConfig_clear;
    public static char* modemConfig_init;
    public static char* modemConfig_hangup;

    public static void M_Menu_ModemConfig_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_modemconfig;
        m_entersound = true;
        net_main_c.GetModemConfig(0, modemConfig_dialing, *modemConfig_clear, *modemConfig_init, *modemConfig_hangup);
    }

    public static void M_ModemConfig_Draw()
    {
        wad_c.qpic_t* p;
        int basex;

        M_DrawTransPic(16, 4, draw_c.Draw_CachePic("gfx/qplaque.lmp"));
        p = draw_c.Draw_CachePic("gfx/p_multi.lmp");
        basex = (320 - p->width) / 2;
        M_DrawPic(basex, 4, p);
        basex += 8;

        if (modemConfig_dialing == 'P')
        {
            M_Print(basex, modemConfig_cursor_table[0], "Pulse Dialing");
        }
        else
        {
            M_Print(basex, modemConfig_cursor_table[0], "Touch Tone Dialing");
        }

        M_Print(basex, modemConfig_cursor_table[1], "Clear");
        M_DrawTextBox(basex, modemConfig_cursor_table[1] + 4, 16, 1);
        M_Print(basex + 8, modemConfig_cursor_table[1] + 12, modemConfig_clear);

        if (modemConfig_cursor == 1)
        {
            M_DrawCharacter(basex + 8 + 8 * strlen_c.strlen(modemConfig_clear), modemConfig_cursor_table[1] + 12, 10 + ((int)(host_c.realtime * 4) & 1));
        }

        M_Print(basex, modemConfig_cursor_table[2], "Init");
        M_DrawTextBox(basex, modemConfig_cursor_table[2] + 4, 30, 1);
        M_Print(basex + 8, modemConfig_cursor_table[2] + 12, modemConfig_init);

        if (modemConfig_cursor == 2)
        {
            M_DrawCharacter(basex + 8 + 8 * strlen_c.strlen(modemConfig_init), modemConfig_cursor_table[2] + 12, 10 + ((int)(host_c.realtime * 4) & 1));
        }

        M_Print(basex, modemConfig_cursor_table[3], "Hangup");
        M_DrawTextBox(basex, modemConfig_cursor_table[3] + 4, 16, 1);
        M_Print(basex + 8, modemConfig_cursor_table[3] + 12, modemConfig_hangup);

        if (modemConfig_cursor == 3)
        {
            M_DrawCharacter(basex + 8 + 8 * strlen_c.strlen(modemConfig_hangup), modemConfig_cursor_table[3] + 12, 10 + ((int)(host_c.realtime * 4) & 1));
        }

        M_DrawTextBox(basex, modemConfig_cursor_table[4] - 8, 2, 1);
        M_Print(basex + 8, modemConfig_cursor_table[4], "OK");

        M_DrawCharacter(basex - 8, modemConfig_cursor_table[modemConfig_cursor], 12 + ((int)(host_c.realtime * 4) & 1));
    }

    public static void M_ModemConfig_Key(int key)
    {
        int l;

        switch (key)
        {
            case keys_c.K_ESCAPE:
                M_Menu_SerialConfig_f();
                break;

            case keys_c.K_UPARROW:
                snd_dma_c.S_LocalSound("misc/menu1.wav");
                modemConfig_cursor--;

                if (modemConfig_cursor < 0)
                {
                    modemConfig_cursor = NUM_MODEMCONFIG_CMDS - 1;
                }

                break;

            case keys_c.K_LEFTARROW:
            case keys_c.K_RIGHTARROW:
                if (modemConfig_cursor == 0)
                {
                    if (modemConfig_dialing == 'P')
                    {
                        modemConfig_dialing = 'T';
                    }
                    else
                    {
                        modemConfig_dialing = 'P';
                    }

                    snd_dma_c.S_LocalSound("misc/menu1.wav");
                }

                break;

            case keys_c.K_ENTER:
                if (modemConfig_cursor == 0)
                {
                    if (modemConfig_dialing == 'P')
                    {
                        modemConfig_dialing = 'T';
                    }
                    else
                    {
                        modemConfig_dialing = 'P';
                    }

                    m_entersound = true;
                }

                if (modemConfig_cursor == 4)
                {
                    net_main_c.SetModemConfig(0, common_c.va(modemConfig_dialing), *modemConfig_clear, *modemConfig_init, *modemConfig_hangup);
                    m_entersound = true;
                    M_Menu_SerialConfig_f();
                }

                break;

            case keys_c.K_BACKSPACE:
                if (modemConfig_cursor == 1)
                {
                    if (strlen_c.strlen(modemConfig_clear) != 0)
                    {
                        modemConfig_clear[strlen_c.strlen(modemConfig_clear) - 1] = (char)0;
                    }
                }

                if (modemConfig_cursor == 2)
                {
                    if (strlen_c.strlen(modemConfig_init) != 0)
                    {
                        modemConfig_init[strlen_c.strlen(modemConfig_init) - 1] = (char)0;
                    }
                }

                if (modemConfig_cursor == 3)
                {
                    if (strlen_c.strlen(modemConfig_hangup) != 0)
                    {
                        modemConfig_hangup[strlen_c.strlen(modemConfig_hangup) - 1] = (char)0;
                    }
                }

                break;

            default:
                if (key < 32 || key > 127)
                {
                    break;
                }

                if (modemConfig_cursor == 1)
                {
                    l = strlen_c.strlen(modemConfig_clear);

                    if (l < 15)
                    {
                        modemConfig_clear[l + 1] = '0';
                        modemConfig_clear[l] = (char)key;
                    }
                }

                if (modemConfig_cursor == 2)
                {
                    l = strlen_c.strlen(modemConfig_init);

                    if (l < 29)
                    {
                        modemConfig_init[l + 1] = '0';
                        modemConfig_init[l] = (char)key;
                    }
                }

                if (modemConfig_cursor == 3)
                {
                    l = strlen_c.strlen(modemConfig_hangup);

                    if (l < 15)
                    {
                        modemConfig_hangup[l + 1] = '0';
                        modemConfig_hangup[l] = (char)key;
                    }
                }

                break;
        }
    }

    public static int lanConfig_cursor = -1;
    public static int[] lanConfig_cursor_table = { 72, 92, 124 };
    public const int NUM_LANCONFIG_CMDS = 3;

    public static int lanConfig_port;
    public static char[] lanConfig_portname = new char[6];
    public static char[] lanConfig_joinname = new char[22];

    public static void M_Menu_LanConfig_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_lanconfig;
        m_entersound = true;

        if (lanConfig_cursor == -1)
        {
            if (JoiningGame && TCPIPConfig)
            {
                lanConfig_cursor = 2;
            }
            else
            {
                lanConfig_cursor = 1;
            }
        }

        if (StartingGame && lanConfig_cursor == 2)
        {
            lanConfig_cursor = 1;
        }

        lanConfig_port = net_main_c.DEFAULTnet_hostport;
        Console.WriteLine($"{lanConfig_port}");

        m_return_onerror = false;
        m_return_reason[0] = (char)0;
    }

    public static void M_LanConfig_Draw()
    {
        wad_c.qpic_t* p;
        int basex;
        char* startJoin;
        char* protocol;

        M_DrawTransPic(16, 4, draw_c.Draw_CachePic("gfx/qplaque.lmp"));
        p = draw_c.Draw_CachePic("gfx/p_multi.lmp");
        basex = (320 - p->width) / 2;
        M_DrawPic(basex, 4, p);

        if (StartingGame)
        {
            startJoin = common_c.StringToChar("New Game");
        }
        else
        {
            startJoin = common_c.StringToChar("Join Game");
        }

        if (IPXConfig)
        {
            protocol = common_c.StringToChar("IPX");
        }
        else
        {
            protocol = common_c.StringToChar("TCP/IP");
        }

        M_Print(basex, 32, common_c.va($"{*startJoin} - {*protocol}"));
        basex += 8;

        M_Print(basex, 52, common_c.StringToChar("Address:"));

        if (IPXConfig)
        {
            M_Print(basex + 9 * 8, 52, net_main_c.my_ipx_address);
        }
        else
        {
            M_Print(basex + 9 * 8, 52, net_main_c.my_tcpip_address);
        }

        M_Print(basex, lanConfig_cursor_table[0], "Port");
        M_DrawTextBox(basex + 8 * 8, lanConfig_cursor_table[0] - 8, 6, 1);
        M_Print(basex + 9 * 8, lanConfig_cursor_table[0], lanConfig_portname);

        if (JoiningGame)
        {
            M_Print(basex, lanConfig_cursor_table[1], "Search for local games...");
            M_Print(basex, 108, "Join game at:");
            M_DrawTextBox(basex + 8, lanConfig_cursor_table[2] - 8, 22, 1);
            M_Print(basex + 16, lanConfig_cursor_table[2], lanConfig_joinname);
        }
        else
        {
            M_DrawTextBox(basex, lanConfig_cursor_table[1] - 8, 2, 1);
            M_Print(basex + 8, lanConfig_cursor_table[1], "OK");
        }

        M_DrawCharacter(basex - 8, lanConfig_cursor_table[lanConfig_cursor], 12 + ((int)(host_c.realtime * 4) & 1));

        if (lanConfig_cursor == 0)
        {
            M_DrawCharacter(basex + 9 * 8 + 8 * strlen_c.strlen(lanConfig_portname), lanConfig_cursor_table[0], 10 + ((int)(host_c.realtime * 4) & 1));
            M_Print(basex + 8, lanConfig_cursor_table[1], "OK");
        }

        M_DrawCharacter(basex - 8, lanConfig_cursor_table[lanConfig_cursor], 12 + ((int)(host_c.realtime * 4) & 1));

        if (lanConfig_cursor == 0)
        {
            M_DrawCharacter(basex + 9 * 8 + 8 * strlen_c.strlen(lanConfig_portname), lanConfig_cursor_table[0], 10 + ((int)(host_c.realtime * 4) & 1));
        }

        if (lanConfig_cursor == 2)
        {
            M_DrawCharacter(basex + 16 + 8 * strlen_c.strlen(lanConfig_joinname), lanConfig_cursor_table[2], 10 + ((int)(host_c.realtime * 4) & 1));
        }

        if (*m_return_reason != null)
        {
            M_PrintWhite(basex, 148, m_return_reason);
        }
    }

    public static void M_LanConfig_Key(int key)
    {
        int l;

        switch (key)
        {
            case keys_c.K_ESCAPE:
                M_Menu_Net_f();
                break;

            case keys_c.K_UPARROW:
                snd_dma_c.S_LocalSound("misc/menu1.wav");
                lanConfig_cursor--;

                if (lanConfig_cursor < 0)
                {
                    lanConfig_cursor = NUM_LANCONFIG_CMDS - 1;
                }

                break;

            case keys_c.K_DOWNARROW:
                snd_dma_c.S_LocalSound("misc/menu1.wav");
                lanConfig_cursor++;

                if (lanConfig_cursor >= NUM_LANCONFIG_CMDS)
                {
                    lanConfig_cursor = 0;
                }

                break;

            case keys_c.K_ENTER:
                if (lanConfig_cursor == 0)
                {
                    break;
                }

                m_entersound = true;

                if (lanConfig_cursor == 1)
                {
                    if (StartingGame)
                    {
                        M_Menu_GameOptions_f();
                        break;
                    }

                    M_Menu_Search_f();
                    break;
                }

                if (lanConfig_cursor == 2)
                {
                    m_return_state = (int)state;
                    m_return_onerror = true;
                    keys_c.key_dest = keys_c.keydest_t.key_game;
                    state = m_state.m_none;
                    cmd_c.Cbuf_AddText(common_c.va($"connect \"{lanConfig_joinname}\"\n"));
                    break;
                }

                break;

            case keys_c.K_BACKSPACE:
                if (lanConfig_cursor == 0)
                {
                    if (strlen_c.strlen(lanConfig_portname) != 0)
                    {
                        lanConfig_portname[strlen_c.strlen(lanConfig_portname) - 1] = (char)0;
                    }
                }

                if (lanConfig_cursor == 2)
                {
                    if (strlen_c.strlen(lanConfig_joinname) != 0)
                    {
                        lanConfig_joinname[strlen_c.strlen(lanConfig_joinname) - 1] = (char)0;
                    }
                }

                break;

            default:
                if (key < 32 || key > 127)
                {
                    break;
                }

                if (lanConfig_cursor == 2)
                {
                    l = strlen_c.strlen(lanConfig_joinname);

                    if (l < 21)
                    {
                        lanConfig_joinname[l + 1] = '0';
                        lanConfig_joinname[l] = (char)key;
                    }
                }

                if (key < '0' || key > '9')
                {
                    break;
                }

                if (lanConfig_cursor == 0)
                {
                    l = strlen_c.strlen(lanConfig_portname);

                    if (l < 5)
                    {
                        lanConfig_portname[l + 1] = '0';
                        lanConfig_portname[l] = (char)key;
                    }
                }

                break;
        }

        if (StartingGame && lanConfig_cursor == 2)
        {
            if (key == keys_c.K_UPARROW)
            {
                lanConfig_cursor = 1;
            }
            else
            {
                lanConfig_cursor = 0;
            }
        }

        l = common_c.Q_atoi(lanConfig_portname);

        if (l > 65535)
        {
            l = lanConfig_port;
        }
        else
        {
            lanConfig_port = l;
        }

        Console.WriteLine($"{lanConfig_port}");
    }

    public struct level_t
    {
        public char* name;
        public char* description;
    }

    public static level_t[] levels =
    {
        new level_t {name = common_c.StringToChar("start"), description = common_c.StringToChar("Entrance")},

        new level_t {name = common_c.StringToChar("e1m1"), description = common_c.StringToChar("Slipgate Complex")},
        new level_t {name = common_c.StringToChar("e1m2"), description = common_c.StringToChar("Castle of the Damned")},
        new level_t {name = common_c.StringToChar("e1m3"), description = common_c.StringToChar("The Necropolis")},
        new level_t {name = common_c.StringToChar("e1m4"), description = common_c.StringToChar("The Grisly Grotto") },
        new level_t {name = common_c.StringToChar("e1m5"), description = common_c.StringToChar("Gloom Keep")},
        new level_t {name = common_c.StringToChar("e1m6"), description = common_c.StringToChar("The Door To Chthon")},
        new level_t {name = common_c.StringToChar("e1m7"), description = common_c.StringToChar("The House of Chthon")},
        new level_t {name = common_c.StringToChar("e1m8"), description = common_c.StringToChar("Ziggurat Vertigo")},

        new level_t {name = common_c.StringToChar("e2m1"), description = common_c.StringToChar("The Installation")},
        new level_t {name = common_c.StringToChar("e2m2"), description = common_c.StringToChar("Ogre Citadel")},
        new level_t {name = common_c.StringToChar("e2m3"), description = common_c.StringToChar("Crypt of Decay")},
        new level_t {name = common_c.StringToChar("e2m4"), description = common_c.StringToChar("The Ebon Fortress")},
        new level_t {name = common_c.StringToChar("e2m5"), description = common_c.StringToChar("The Wizard's Manse")},
        new level_t {name = common_c.StringToChar("e2m6"), description = common_c.StringToChar("The Dismal Oubliette")},
        new level_t {name = common_c.StringToChar("e2m7"), description = common_c.StringToChar("Underearth")},

        new level_t {name = common_c.StringToChar("e3m1"), description = common_c.StringToChar("Termination Central")},
        new level_t {name = common_c.StringToChar("e3m2"), description = common_c.StringToChar("The Vaults of Zin")},
        new level_t {name = common_c.StringToChar("e3m3"), description = common_c.StringToChar("The Tomb of Terror")},
        new level_t {name = common_c.StringToChar("e3m4"), description = common_c.StringToChar("Satan's Dark Delight")},
        new level_t {name = common_c.StringToChar("e3m5"), description = common_c.StringToChar("Wind Tunnels")},
        new level_t {name = common_c.StringToChar("e3m6"), description = common_c.StringToChar("Chambers of Torment")},
        new level_t {name = common_c.StringToChar("e3m7"), description = common_c.StringToChar("The Haunted Halls")},

        new level_t {name = common_c.StringToChar("e4m1"), description = common_c.StringToChar("The Sewage System")},
        new level_t {name = common_c.StringToChar("e4m2"), description = common_c.StringToChar("The Tower of Despair")},
        new level_t {name = common_c.StringToChar("e4m3"), description = common_c.StringToChar("The Elder God Shrine")},
        new level_t {name = common_c.StringToChar("e4m4"), description = common_c.StringToChar("The Palace of Hate")},
        new level_t {name = common_c.StringToChar("e4m5"), description = common_c.StringToChar("Hell's Atrium")},
        new level_t {name = common_c.StringToChar("e4m6"), description = common_c.StringToChar("The Pain Maze")},
        new level_t {name = common_c.StringToChar("e4m7"), description = common_c.StringToChar("Azure Agony")},
        new level_t {name = common_c.StringToChar("e4m8"), description = common_c.StringToChar("The Nameless City")},

        new level_t {name = common_c.StringToChar("end"), description = common_c.StringToChar("Shub-Niggurath's Pit")},

        new level_t {name = common_c.StringToChar("dm1"), description = common_c.StringToChar("Place of Two Deaths")},
        new level_t {name = common_c.StringToChar("dm2"), description = common_c.StringToChar("Claustrophobopolis")},
        new level_t {name = common_c.StringToChar("dm3"), description = common_c.StringToChar("The Abandoned Base")},
        new level_t {name = common_c.StringToChar("dm4"), description = common_c.StringToChar("The Bad Place")},
        new level_t {name = common_c.StringToChar("dm5"), description = common_c.StringToChar("The Cistern")},
        new level_t {name = common_c.StringToChar("dm6"), description = common_c.StringToChar("The Dark Zone")}
    };

    public static level_t[] hipnoticlevels =
    {
        new level_t {name = common_c.StringToChar("start"), description = common_c.StringToChar("Command HQ")},

        new level_t {name = common_c.StringToChar("hip1m1"), description = common_c.StringToChar("The Pumping Station")},
        new level_t {name = common_c.StringToChar("hip1m2"), description = common_c.StringToChar("Storage Facility")},
        new level_t {name = common_c.StringToChar("hip1m3"), description = common_c.StringToChar("The Lost Mine")},
        new level_t {name = common_c.StringToChar("hip1m4"), description = common_c.StringToChar("Research Facility")},
        new level_t {name = common_c.StringToChar("hip1m5"), description = common_c.StringToChar("Military Complex")},

        new level_t {name = common_c.StringToChar("hip2m1"), description = common_c.StringToChar("Ancient Realms")},
        new level_t {name = common_c.StringToChar("hip2m2"), description = common_c.StringToChar("The Black Cathedral")},
        new level_t {name = common_c.StringToChar("hip2m3"), description = common_c.StringToChar("The Catacombs")},
        new level_t {name = common_c.StringToChar("hip2m4"), description = common_c.StringToChar("The Crypt")},
        new level_t {name = common_c.StringToChar("hip2m5"), description = common_c.StringToChar("Mortum's Keep")},
        new level_t {name = common_c.StringToChar("hip2m6"), description = common_c.StringToChar("The Gremlin's Domain")},

        new level_t {name = common_c.StringToChar("hip3m1"), description = common_c.StringToChar("Tur Torment")},
        new level_t {name = common_c.StringToChar("hip3m2"), description = common_c.StringToChar("Pandemonium")},
        new level_t {name = common_c.StringToChar("hip3m3"), description = common_c.StringToChar("Limbo")},
        new level_t {name = common_c.StringToChar("hip3m4"), description = common_c.StringToChar("The Gauntlet")},

        new level_t {name = common_c.StringToChar("hipend"), description = common_c.StringToChar("Armagon's Lair")},

        new level_t {name = common_c.StringToChar("hipdm1"), description = common_c.StringToChar("The Edge of Oblivion")}
    };

    public static level_t[] roguelevels =
    {
        new level_t {name = common_c.StringToChar("start"), description = common_c.StringToChar("Split Decision")},
        new level_t {name = common_c.StringToChar("r1m1"), description = common_c.StringToChar("Deviant's Domain")},
        new level_t {name = common_c.StringToChar("r1m2"), description = common_c.StringToChar("Dread Portal")},
        new level_t {name = common_c.StringToChar("r1m3"), description = common_c.StringToChar("Judgement Call")},
        new level_t {name = common_c.StringToChar("r1m4"), description = common_c.StringToChar("Cave of Death")},
        new level_t {name = common_c.StringToChar("r1m5"), description = common_c.StringToChar("Towers of Wrath")},
        new level_t {name = common_c.StringToChar("r1m6"), description = common_c.StringToChar("Temple of Pain")},
        new level_t {name = common_c.StringToChar("r1m7"), description = common_c.StringToChar("Tomb of the Overlord")},
        new level_t {name = common_c.StringToChar("r2m1"), description = common_c.StringToChar("Tempus Fugit")},
        new level_t {name = common_c.StringToChar("r2m2"), description = common_c.StringToChar("Elemental Fury I")},
        new level_t {name = common_c.StringToChar("r2m3"), description = common_c.StringToChar("Elemental Fury II")},
        new level_t {name = common_c.StringToChar("r2m4"), description = common_c.StringToChar("Curse of Osiris")},
        new level_t {name = common_c.StringToChar("r2m5"), description = common_c.StringToChar("Wizard's Keep")},
        new level_t {name = common_c.StringToChar("r2m6"), description = common_c.StringToChar("Blood Sacrifice")},
        new level_t {name = common_c.StringToChar("r2m7"), description = common_c.StringToChar("Last Bastion")},
        new level_t {name = common_c.StringToChar("r2m8"), description = common_c.StringToChar("Source of Evil")},
        new level_t {name = common_c.StringToChar("ctf1"), description = common_c.StringToChar("Division of Change")}
    };

    public struct episode_t
    {
        public char* description;
        public int firstLevel;
        public int levels;
    }

    public static episode_t[] episodes =
    {
        new episode_t {description = common_c.StringToChar("Welcome to Quake"), firstLevel = 0, levels = 1},
        new episode_t {description = common_c.StringToChar("Doomed Dimension"), firstLevel = 1, levels = 8},
        new episode_t {description = common_c.StringToChar("Realm of Black Magic"), firstLevel = 9, levels = 7},
        new episode_t {description = common_c.StringToChar("Netherworld"), firstLevel = 16, levels = 7},
        new episode_t {description = common_c.StringToChar("The Elder World"), firstLevel = 23, levels = 8},
        new episode_t {description = common_c.StringToChar("Final Level"), firstLevel = 31, levels = 1},
        new episode_t {description = common_c.StringToChar("Deathmatch Arena"), firstLevel = 32, levels = 6}
    };

    public static episode_t[] hipnoticepisodes =
    {
        new episode_t {description = common_c.StringToChar("Scourge of Armagon"), firstLevel = 0, levels = 1 },
        new episode_t {description = common_c.StringToChar("Fortress of the Dead"), firstLevel = 1, levels = 5},
        new episode_t {description = common_c.StringToChar("Dominion of Darkness"), firstLevel = 6, levels = 6},
        new episode_t {description = common_c.StringToChar("The Rift"), firstLevel = 12, levels = 4},
        new episode_t {description = common_c.StringToChar("Final Level"), firstLevel = 16, levels = 1},
        new episode_t {description = common_c.StringToChar("Deathmatch Arena"), firstLevel = 17, levels = 1}
    };

    public static episode_t[] rogueepisodes =
    {
        new episode_t {description = common_c.StringToChar("Introduction"), firstLevel = 0, levels = 1},
        new episode_t {description = common_c.StringToChar("Hell's Fortress"), firstLevel = 1, levels = 7},
        new episode_t {description = common_c.StringToChar("Corridors of Time"), firstLevel = 8, levels = 8},
        new episode_t {description = common_c.StringToChar("Deathmatch Arena"), firstLevel = 16, levels = 1}
    };

    public static int startepisode;
    public static int startlevel;
    public static int maxplayers;
    public static bool m_serverInfoMessage = false;
    public static double m_serverInfoMessageTime;

    public static void M_Menu_GameOptions_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_gameoptions;
        m_entersound = true;

        if (maxplayers == 0)
        {
            maxplayers = server_c.svs.maxclients;
        }

        if (maxplayers < 2)
        {
            maxplayers = server_c.svs.maxclientslimit;
        }
    }

    public static int[] gameoptions_cursor_table = { 40, 56, 64, 72, 80, 88, 96, 112, 120 };
    public const int NUM_GAMEOPTIONS = 9;
    public static int gameoptions_cursor;

    public static void M_GameOptions_Draw()
    {
        wad_c.qpic_t* p;
        int x;

        M_DrawTransPic(16, 4, draw_c.Draw_CachePic("gfx/qplaque.lmp"));
        p = draw_c.Draw_CachePic("gfx/p_multi.lmp");
        M_DrawPic((320 - p->width) / 2, 4, p);

        M_DrawTextBox(152, 32, 10, 1);
        M_Print(160, 40, "begin game");

        M_Print(0, 56, "    Max players");
        M_Print(160, 56, common_c.va($"{maxplayers}"));

        M_Print(0, 64, "    Game Type");

        if (host_c.coop.value != null)
        {
            M_Print(160, 64, "Cooperative");
        }
        else
        {
            M_Print(160, 64, "Deathmatch");
        }

        M_Print(0, 72, "    Teamplay");

        if (common_c.rogue)
        {
            char* msg;

            switch ((int)host_c.teamplay.value)
            {
                case 1: msg = common_c.StringToChar("No Friendly Fire"); break;
                case 2: msg = common_c.StringToChar("Friendly Fire"); break;
                case 3: msg = common_c.StringToChar("Tag"); break;
                case 4: msg = common_c.StringToChar("Capture The Flag"); break;
                case 5: msg = common_c.StringToChar("One Flag CTF"); break;
                case 6: msg = common_c.StringToChar("Three Team CTF"); break;
                default: msg = common_c.StringToChar("Off"); break;
            }

            M_Print(160, 72, msg);
        }
        else
        {
            char* msg;

            switch ((int)host_c.teamplay.value)
            {
                case 1: msg = common_c.StringToChar("No Friendly Fire"); break;
                case 2: msg = common_c.StringToChar("Friendly Fire"); break;
                default: msg = common_c.StringToChar("Off"); break;
            }

            M_Print(160, 72, msg);
        }

        M_Print(0, 80, "        Skill");

        if (host_c.skill.value == '0')
        {
            M_Print(160, 80, "Easy Difficulty");
        }
        else if (host_c.skill.value == '1')
        {
            M_Print(160, 80, "Normal Difficulty");
        }
        else if (host_c.skill.value == '2')
        {
            M_Print(160, 80, "Hard Difficulty");
        }
        else
        {
            M_Print(160, 80, "Nightmare Difficulty");
        }

        M_Print(0, 88, "    Frag Limit");

        if (host_c.fraglimit.value == '0')
        {
            M_Print(160, 88, "none");
        }
        else
        {
            M_Print(160, 88, common_c.va($"{(int)host_c.fraglimit.value} frags"));
        }

        M_Print(0, 96, "    Time Limit");

        if (host_c.timelimit.value == '0')
        {
            M_Print(160, 96, "none");
        }
        else
        {
            M_Print(160, 96, common_c.va($"{(int)host_c.timelimit.value} minutes"));
        }

        M_Print(0, 112, "       Episode");

        if (common_c.hipnotic)
        {
            M_Print(160, 112, hipnoticepisodes[startepisode].description);
        }
        else if (common_c.rogue)
        {
            M_Print(160, 112, rogueepisodes[startepisode].description);
        }
        else
        {
            M_Print(160, 112, episodes[startepisode].description);
        }

        M_Print(0, 120, "       Level");

        if (common_c.hipnotic)
        {
            M_Print(160, 120, hipnoticlevels[hipnoticepisodes[startepisode].firstLevel + startlevel].description);
            M_Print(160, 128, hipnoticlevels[hipnoticepisodes[startepisode].firstLevel + startlevel].name);
        }
        else if (common_c.rogue)
        {
            M_Print(160, 120, roguelevels[rogueepisodes[startepisode].firstLevel + startlevel].description);
            M_Print(160, 128, roguelevels[rogueepisodes[startepisode].firstLevel + startlevel].name);
        }
        else
        {
            M_Print(160, 120, levels[episodes[startepisode].firstLevel + startlevel].description);
            M_Print(160, 128, levels[episodes[startepisode].firstLevel + startlevel].name);
        }

        M_DrawCharacter(144, gameoptions_cursor_table[gameoptions_cursor], 12 + ((int)(host_c.realtime * 4) & 1));

        if (m_serverInfoMessage)
        {
            if ((host_c.realtime - m_serverInfoMessageTime) < 5.0)
            {
                x = (320 - 26 * 8) / 2;
                M_DrawTextBox(x, 138, 24, 4);
                x += 8;
                M_Print(x, 146, "  More than 4 players   ");
                M_Print(x, 154, " requires using command ");
                M_Print(x, 162, "line parameters; please ");
                M_Print(x, 170, "   see techinfo.txt.    ");
            }
            else
            {
                m_serverInfoMessage = false;
            }
        }
    }

    public static void M_NetStart_Change(int dir)
    {
        int count;

        switch (gameoptions_cursor)
        {
            case 1:
                maxplayers += dir;

                if (maxplayers > server_c.svs.maxclientslimit)
                {
                    maxplayers = server_c.svs.maxclientslimit;
                    m_serverInfoMessage = true;
                    m_serverInfoMessageTime = host_c.realtime;
                }

                if (maxplayers < 2)
                {
                    maxplayers = 2;
                }

                break;

            case 2:
                cvar_c.Cvar_SetValue("coop", host_c.coop.value == 0 ? 0 : 1);
                break;

            case 3:
                if (common_c.rogue)
                {
                    count = 6;
                }
                else
                {
                    count = 2;
                }

                cvar_c.Cvar_SetValue("teamplay", host_c.teamplay.value + dir);

                if (host_c.teamplay.value > count)
                {
                    cvar_c.Cvar_SetValue("teamplay", 0);
                }
                else if (host_c.teamplay.value < 0)
                {
                    cvar_c.Cvar_SetValue("teamplay", count);
                }

                break;

            case 4:
                cvar_c.Cvar_SetValue("skill", host_c.skill.value + dir);

                if (host_c.skill.value > 3)
                {
                    cvar_c.Cvar_SetValue("skill", 0);
                }

                if (host_c.skill.value < 0)
                {
                    cvar_c.Cvar_SetValue("skill", 3);
                }

                break;

            case 5:
                cvar_c.Cvar_SetValue("fraglimit", host_c.fraglimit.value + dir * 10);

                if (host_c.fraglimit.value > 100)
                {
                    cvar_c.Cvar_SetValue("fraglimit", 0);
                }

                if (host_c.fraglimit.value < 0)
                {
                    cvar_c.Cvar_SetValue("fraglimit", 100);
                }

                break;

            case 6:
                cvar_c.Cvar_SetValue("timelimit", host_c.timelimit.value + dir * 5);

                if (host_c.timelimit.value > 60)
                {
                    cvar_c.Cvar_SetValue("timelimit", 0);
                }

                if (host_c.timelimit.value < 0)
                {
                    cvar_c.Cvar_SetValue("timelimit", 60);
                }

                break;

            case 7:
                startepisode += dir;

                if (common_c.hipnotic)
                {
                    count = 6;
                }
                else if (common_c.rogue)
                {
                    count = 4;
                }
                else if (common_c.registered.value != 0)
                {
                    count = 7;
                }
                else
                {
                    count = 2;
                }

                if (startepisode < 0)
                {
                    startepisode = count - 1;
                }

                if (startepisode >= count)
                {
                    startepisode = 0;
                }

                startlevel = 0;
                break;

            case 8:
                startlevel += dir;

                if (common_c.hipnotic)
                {
                    count = hipnoticepisodes[startepisode].levels;
                }
                else if (common_c.rogue)
                {
                    count = rogueepisodes[startepisode].levels;
                }
                else
                {
                    count = episodes[startepisode].levels;
                }

                if (startlevel < 0)
                {
                    startlevel = count - 1;
                }

                if (startlevel >= count)
                {
                    startlevel = 0;
                }

                break;
        }
    }

    public static void M_GameOptions_Key(int key)
    {
        switch (key)
        {
            case keys_c.K_ESCAPE:
                M_Menu_Net_f();
                break;

            case keys_c.K_UPARROW:
                snd_dma_c.S_LocalSound("misc/menu1.wav");
                gameoptions_cursor--;

                if (gameoptions_cursor < 0)
                {
                    gameoptions_cursor = NUM_GAMEOPTIONS - 1;
                }

                break;

            case keys_c.K_DOWNARROW:
                snd_dma_c.S_LocalSound("misc/menu1.wav");
                gameoptions_cursor++;

                if (gameoptions_cursor >= NUM_GAMEOPTIONS)
                {
                    gameoptions_cursor = 0;
                }

                break;

            case keys_c.K_LEFTARROW:
                if (gameoptions_cursor == 0)
                {
                    break;
                }

                snd_dma_c.S_LocalSound("misc/menu3.wav");
                M_NetStart_Change(-1);
                break;

            case keys_c.K_RIGHTARROW:
                if (gameoptions_cursor == 0)
                {
                    break;
                }

                snd_dma_c.S_LocalSound("misc/menu3.wav");
                M_NetStart_Change(1);
                break;

            case keys_c.K_ENTER:
                snd_dma_c.S_LocalSound("misc/menu2.wav");

                if (gameoptions_cursor == 0)
                {
                    if (server_c.sv.active)
                    {
                        cmd_c.Cbuf_AddText("disconnect\n");
                    }

                    cmd_c.Cbuf_AddText("listen 0\n");
                    cmd_c.Cbuf_AddText(common_c.va($"maxplayers {maxplayers}\n"));
                    screen_c.SCR_BeginLoadingPlaque();

                    if (common_c.hipnotic)
                    {
                        cmd_c.Cbuf_AddText(common_c.va($"map {*hipnoticlevels[hipnoticepisodes[startepisode].firstLevel + startlevel].name}"));
                    }
                    else if (common_c.rogue)
                    {
                        cmd_c.Cbuf_AddText(common_c.va($"map {*roguelevels[rogueepisodes[startepisode].firstLevel + startlevel].name}"));
                    }
                    else
                    {
                        cmd_c.Cbuf_AddText(common_c.va($"map {*levels[episodes[startepisode].firstLevel + startlevel].name}"));
                    }

                    return;
                }

                M_NetStart_Change(1);
                break;
        }
    }

    public static bool searchComplete = false;
    public static double searchCompleteTime;

    public static void M_Menu_Search_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_search;
        m_entersound = false;
        net_main_c.slistSilent = true;
        net_main_c.slistLocal = false;
        searchComplete = false;
        net_main_c.NET_Slist_f();
    }

    public static void M_Search_Draw()
    {
        wad_c.qpic_t* p;
        int x;

        p = draw_c.Draw_CachePic("gfx/p_multi.lmp");
        M_DrawPic((320 - p->width) / 2, 4, p);
        x = (320 / 2) - ((12 * 8) / 2) + 4;
        M_DrawTextBox(x - 8, 32, 12, 1);
        M_Print(x, 40, "Searching...");

        if (net_main_c.slistInProgress)
        {
            net_main_c.NET_Poll();
            return;
        }

        if (!searchComplete)
        {
            searchComplete = true;
            searchCompleteTime = host_c.realtime;
        }

        if (net_c.hostCacheCount != 0)
        {
            M_Menu_ServerList_f();
            return;
        }

        M_PrintWhite((320 / 2) - ((22 * 8) / 2), 64, common_c.StringToChar("No Quake servers found"));

        if ((host_c.realtime - searchCompleteTime) < 3.0)
        {
            return;
        }

        M_Menu_LanConfig_f();
    }

    public static void M_Search_Key(int key)
    {
    }

    public static int slist_cursor;
    public static bool slist_sorted;

    public static void M_Menu_ServerList_f()
    {
        keys_c.key_dest = keys_c.keydest_t.key_menu;
        state = m_state.m_slist;
        m_entersound = true;
        slist_cursor = 0;
        m_return_onerror = false;
        m_return_reason[0] = 0;
        slist_sorted = false;
    }

    public static void M_ServerList_Draw()
    {
        int n;
        char* str = null;
        wad_c.qpic_t* p;

        if (!slist_sorted)
        {
            if (net_c.hostCacheCount > 1)
            {
                int i, j;
                net_c.hostcache_t temp = default;

                for (i = 0; i < net_c.hostCacheCount; i++)
                {
                    for (j = i + 1; j < net_c.hostCacheCount; j++)
                    {
                        if (strcmp_c.strcmp(net_c.hostcache[j].name, net_c.hostcache[i].name) < 0)
                        {
                            common_c.Q_memcpy(temp, net_c.hostcache[j], sizeof(net_c.hostcache_t));
                            common_c.Q_memcpy(net_c.hostcache[j], net_c.hostcache[i], sizeof(net_c.hostcache_t));
                            common_c.Q_memcpy(net_c.hostcache[i], temp, sizeof(net_c.hostcache_t));
                        }
                    }
                }

            }
            slist_sorted = true;
        }

        p = draw_c.Draw_CachePic("gfx/p_multi.lmp");
        M_DrawPic((320 - p->width) / 2, 4, p);

        for (n = 0; n < net_c.hostCacheCount; n++)
        {
            if (net_c.hostcache[n].maxusers != 0)
            {
                Console.WriteLine($"{*net_c.hostcache[n].name} {*net_c.hostcache[n].map} {net_c.hostcache[n].users}/{net_c.hostcache[n].maxusers}\n");
            }
            else
            {
                Console.WriteLine($"{*net_c.hostcache[n].name} {*net_c.hostcache[n].map}");
            }

            M_Print(16, 32 + 8 * n, str);
        }

        M_DrawCharacter(0, 32 + slist_cursor * 8, 12 + ((int)(host_c.realtime * 4) & 1));

        if (*m_return_reason != 0)
        {
            M_PrintWhite(16, 148, m_return_reason);
        }
    }

    public static void M_ServerList_Key(int k)
    {
        switch (k)
        {
            case keys_c.K_ESCAPE:
                M_Menu_LanConfig_f();
                break;

            case keys_c.K_SPACE:
                M_Menu_Search_f();
                break;

            case keys_c.K_UPARROW:
            case keys_c.K_LEFTARROW:
                snd_dma_c.S_LocalSound("misc/menu1.wav");
                slist_cursor--;

                if (slist_cursor < 0)
                {
                    slist_cursor = net_c.hostCacheCount - 1;
                }

                break;

            case keys_c.K_DOWNARROW:
            case keys_c.K_RIGHTARROW:
                snd_dma_c.S_LocalSound("misc/menu1.wav");
                slist_cursor++;
                
                if (slist_cursor >= net_c.hostCacheCount)
                {
                    slist_cursor = 0;
                }

                break;

            case keys_c.K_ENTER:
                snd_dma_c.S_LocalSound("misc/menu2.wav");
                m_return_state = (int)state;
                m_return_onerror = true;
                slist_sorted = false;
                keys_c.key_dest = keys_c.keydest_t.key_game;
                state = m_state.m_none;
                cmd_c.Cbuf_AddText(common_c.va($"connect \"{*net_c.hostcache[slist_cursor].cname}\"\n"));
                break;

            default:
                break;
        }
    }

    public static void M_Init()
    {
        cmd_c.Cmd_AddCommand("togglemenu", M_ToggleMenu_f);

        cmd_c.Cmd_AddCommand("menu_main", M_Menu_Main_f);
        cmd_c.Cmd_AddCommand("menu_singleplayer", M_Menu_SinglePlayer_f);
        cmd_c.Cmd_AddCommand("menu_load", M_Menu_Load_f);
        cmd_c.Cmd_AddCommand("menu_save", M_Menu_Save_f);
        cmd_c.Cmd_AddCommand("menu_multiplayer", M_Menu_MultiPlayer_f);
        cmd_c.Cmd_AddCommand("menu_setup", M_Menu_Setup_f);
        cmd_c.Cmd_AddCommand("menu_options", M_Menu_Options_f);
        cmd_c.Cmd_AddCommand("menu_keys", M_Menu_Keys_f);
        cmd_c.Cmd_AddCommand("menu_video", M_Menu_Video_f);
        cmd_c.Cmd_AddCommand("help", M_Menu_Help_f);
        cmd_c.Cmd_AddCommand("menu_quit", M_Menu_Quit_f);
    }

    public static void M_Draw()
    {
        if (state == m_state.m_none || keys_c.key_dest != keys_c.keydest_t.key_menu)
        {
            return;
        }

        if (!m_recursiveDraw)
        {
            screen_c.scr_copyeverything = 1;

            if (screen_c.scr_con_current != 0)
            {
                draw_c.Draw_ConsoleBackground((int)vid_c.vid.height);
                vid_win_c.VID_UnlockBuffer();
                snd_dma_c.S_ExtraUpdate();
                vid_win_c.VID_LockBuffer();
            }
            else
            {
                draw_c.Draw_FadeScreen();
            }

            screen_c.scr_fullupdate = 0;
        }
        else
        {
            m_recursiveDraw = false;
        }

        switch (state)
        {
            case m_state.m_none:
                break;

            case m_state.m_main:
                M_Main_Draw();
                break;

            case m_state.m_singleplayer:
                M_SinglePlayer_Draw();
                break;

            case m_state.m_load:
                M_Load_Draw();
                break;

            case m_state.m_save:
                M_Save_Draw();
                break;

            case m_state.m_multiplayer:
                M_MultiPlayer_Draw();
                break;

            case m_state.m_setup:
                M_Setup_Draw();
                break;

            case m_state.m_net:
                M_Net_Draw();
                break;

            case m_state.m_options:
                M_Options_Draw();
                break;

            case m_state.m_keys:
                M_Keys_Draw();
                break;

            case m_state.m_video:
                M_Video_Draw();
                break;

            case m_state.m_help:
                M_Help_Draw();
                break;

            case m_state.m_quit:
                M_Quit_Draw();
                break;

            case m_state.m_serialconfig:
                M_SerialConfig_Draw();
                break;

            case m_state.m_modemconfig:
                M_ModemConfig_Draw();
                break;

            case m_state.m_lanconfig:
                M_LanConfig_Draw();
                break;

            case m_state.m_gameoptions:
                M_GameOptions_Draw();
                break;

            case m_state.m_search:
                M_Search_Draw();
                break;

            case m_state.m_slist:
                M_ServerList_Draw();
                break;
        }

        if (m_entersound)
        {
            snd_dma_c.S_LocalSound("misc/menu2.wav");
            m_entersound = false;
        }

        vid_win_c.VID_UnlockBuffer();
        snd_dma_c.S_ExtraUpdate();
        vid_win_c.VID_LockBuffer();
    }

    public static void M_Keydown(int key)
    {
        switch (state)
        {
            case m_state.m_none:
                return;

            case m_state.m_main:
                M_Main_Key(key);
                return;

            case m_state.m_singleplayer:
                M_SinglePlayer_Key(key);
                return;

            case m_state.m_load:
                M_Load_Key(key);
                return;

            case m_state.m_save:
                M_Save_Key(key);
                return;

            case m_state.m_multiplayer:
                M_MultiPlayer_Key(key);
                return;

            case m_state.m_setup:
                M_Setup_Key(key);
                return;

            case m_state.m_net:
                M_Net_Key(key);
                return;

            case m_state.m_options:
                M_Options_Key(key);
                return;

            case m_state.m_keys:
                M_Keys_Key(key);
                return;

            case m_state.m_video:
                M_Video_Key(key);
                return;

            case m_state.m_help:
                M_Help_Key(key);
                return;

            case m_state.m_quit:
                M_Quit_Key(key);
                return;

            case m_state.m_serialconfig:
                M_SerialConfig_Key(key);
                return;

            case m_state.m_modemconfig:
                M_ModemConfig_Key(key);
                return;

            case m_state.m_lanconfig:
                M_LanConfig_Key(key);
                return;

            case m_state.m_gameoptions:
                M_GameOptions_Key(key);
                return;

            case m_state.m_search:
                M_Search_Key(key);
                break;

            case m_state.m_slist:
                M_ServerList_Key(key);
                return;
        }
    }

    public static void M_ConfigureNetSubsystem()
    {
        cmd_c.Cbuf_AddText("stopdemo\n");

        if (SerialConfig || DirectConfig)
        {
            cmd_c.Cbuf_AddText("com1 enable\n");
        }

        if (IPXConfig || TCPIPConfig)
        {
            net_c.net_hostport = lanConfig_port;
        }
    }
}