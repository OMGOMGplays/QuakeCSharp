using lib.libc;
using System.Security.Cryptography;

namespace Quake;

public unsafe class menu_c
{
    public const int MNET_IPX = 1;
    public const int MNET_TCP = 2;

    public static int m_activenet;

    public enum m_state { m_none, m_main, m_singleplayer, m_load, m_save, m_multiplayer, m_setup, m_net, m_options, m_video, m_keys, m_help, m_quit, m_serialconfig, m_modeconfig, m_lanconfig, m_gameoptions, m_search, m_slist }

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

    public static void M_Print(int cx, int cy, char* str)
    {
        while (*str != 0)
        {
            M_DrawCharacter(cx, cy, (*str) + 128);
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
            M_Print(16, 32 + 8 * i, &m_filenames[i][0]);
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
            M_Print(16, 32 + 8 * i, &m_filenames[i][0]);
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
        r = (1.0 - view_c.v_gamma.value) / 0.5;
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
                    state = m_quit_prevstate;
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
            state = m_quit_prevstate;
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
        m_return_reason[0] = 0;
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
            M_DrawCharacter(168 + 8 * common_c.Q_strlen(serialConfig_phone->ToString()), serialConfig_cursor_table[serialConfig_cursor], 10 + ((int)(realtime * 4) & 1));
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

                m_return_state = m_state.m_net;
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
                    if (common_c.Q_strlen(serialConfig_phone))
                    {
                        serialConfig_phone[common_c.Q_strlen(serialConfig_phone) - 1] = (char)0;
                    }
                }

                break;

            
        }
    }
}