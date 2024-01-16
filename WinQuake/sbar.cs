namespace Quake;

public unsafe class sbar_c
{
    public const int SBAR_HEIGHT = 24;

    public static int sb_updates;

    public const int STAT_MINUS = 10;
    public static wad_c.qpic_t*[][] sb_nums;
    public static wad_c.qpic_t* sb_colon, sb_slash;
    public static wad_c.qpic_t* sb_ibar;
    public static wad_c.qpic_t* sb_sbar;
    public static wad_c.qpic_t* sb_scorebar;

    public static wad_c.qpic_t*[][] sb_weapons;
    public static wad_c.qpic_t*[] sb_ammo;
    public static wad_c.qpic_t*[] sb_sigil;
    public static wad_c.qpic_t*[] sb_armor;
    public static wad_c.qpic_t*[] sb_items;

    public static wad_c.qpic_t*[][] sb_faces;

    public static wad_c.qpic_t* sb_face_invis;
    public static wad_c.qpic_t* sb_face_quad;
    public static wad_c.qpic_t* sb_face_invuln;
    public static wad_c.qpic_t* sb_face_invis_invuln;

    public static bool sb_showscores;

    public static int sb_lines;

    public static wad_c.qpic_t*[] rsb_invbar;
    public static wad_c.qpic_t*[] rsb_weapons;
    public static wad_c.qpic_t*[] rsb_items;
    public static wad_c.qpic_t*[] rsb_ammo;
    public static wad_c.qpic_t* rsb_teambord;

    public static wad_c.qpic_t*[][] hsb_weapons;
    public static int[] hipweapons = { quakedef_c.HIT_LASER_CANNON_BIT, quakedef_c.HIT_MJOLNIR_BIT, 4, quakedef_c.HIT_PROXIMITY_GUN_BIT };
    public static wad_c.qpic_t*[] hsb_items;

    public static void Sbar_ShowScores()
    {
        if (sb_showscores)
        {
            return;
        }

        sb_showscores = true;
        sb_updates = 0;
    }

    public static void Sbar_DontShowScores()
    {
        sb_showscores = false;
        sb_updates = 0;
    }

    public static void Sbar_Changed()
    {
        sb_updates = 0;
    }

    public static void Sbar_Init()
    {
        int i;

        for (i = 0; i < 10; i++)
        {
            sb_nums[0][i] = draw_c.Draw_PicFromWad(common_c.va($"num_{i}"));
            sb_nums[1][i] = draw_c.Draw_PicFromWad(common_c.va($"anum_{i}"));
        }

        sb_nums[0][10] = draw_c.Draw_PicFromWad("num_minus");
        sb_nums[1][10] = draw_c.Draw_PicFromWad("anum_minus");

        sb_colon = draw_c.Draw_PicFromWad("num_colon");
        sb_slash = draw_c.Draw_PicFromWad("num_slash");

        sb_weapons[0][0] = draw_c.Draw_PicFromWad("inv_shotgun");
        sb_weapons[0][1] = draw_c.Draw_PicFromWad("inv_sshotgun");
        sb_weapons[0][2] = draw_c.Draw_PicFromWad("inv_nailgun");
        sb_weapons[0][3] = draw_c.Draw_PicFromWad("inv_snailgun");
        sb_weapons[0][4] = draw_c.Draw_PicFromWad("inv_rlaunch");
        sb_weapons[0][5] = draw_c.Draw_PicFromWad("inv_srlaunch");
        sb_weapons[0][6] = draw_c.Draw_PicFromWad("inv_lightng");

        sb_weapons[1][0] = draw_c.Draw_PicFromWad("inv2_shotgun");
        sb_weapons[1][1] = draw_c.Draw_PicFromWad("inv2_sshotgun");
        sb_weapons[1][2] = draw_c.Draw_PicFromWad("inv2_nailgun");
        sb_weapons[1][3] = draw_c.Draw_PicFromWad("inv2_snailgun");
        sb_weapons[1][4] = draw_c.Draw_PicFromWad("inv2_rlaunch");
        sb_weapons[1][5] = draw_c.Draw_PicFromWad("inv2_srlaunch");
        sb_weapons[1][6] = draw_c.Draw_PicFromWad("inv2_lightng");

        for (i = 0; i < 5; i++)
        {
            sb_weapons[2 + i][0] = draw_c.Draw_PicFromWad(common_c.va($"inva{i + 1}_shotgun"));
            sb_weapons[2 + i][1] = draw_c.Draw_PicFromWad(common_c.va($"inva{i + 1}_sshotgun"));
            sb_weapons[2 + i][2] = draw_c.Draw_PicFromWad(common_c.va($"inva{i + 1}_nailgun"));
            sb_weapons[2 + i][3] = draw_c.Draw_PicFromWad(common_c.va($"inva{i + 1}_snailgun"));
            sb_weapons[2 + i][4] = draw_c.Draw_PicFromWad(common_c.va($"inva{i + 1}_rlaunch"));
            sb_weapons[2 + i][5] = draw_c.Draw_PicFromWad(common_c.va($"inva{i + 1}_srlaunch"));
            sb_weapons[2 + i][6] = draw_c.Draw_PicFromWad(common_c.va($"inva{i + 1}_lightng"));
        }

        sb_ammo[0] = draw_c.Draw_PicFromWad("sb_shells");
        sb_ammo[1] = draw_c.Draw_PicFromWad("sb_nails");
        sb_ammo[2] = draw_c.Draw_PicFromWad("sb_rocket");
        sb_ammo[3] = draw_c.Draw_PicFromWad("sb_cells");

        sb_armor[0] = draw_c.Draw_PicFromWad("sb_armor1");
        sb_armor[1] = draw_c.Draw_PicFromWad("sb_armor2");
        sb_armor[2] = draw_c.Draw_PicFromWad("sb_armor3");

        sb_items[0] = draw_c.Draw_PicFromWad("sb_key1");
        sb_items[1] = draw_c.Draw_PicFromWad("sb_key2");
        sb_items[2] = draw_c.Draw_PicFromWad("sb_invis");
        sb_items[3] = draw_c.Draw_PicFromWad("sb_invuln");
        sb_items[4] = draw_c.Draw_PicFromWad("sb_suit");
        sb_items[5] = draw_c.Draw_PicFromWad("sb_quad");

        sb_sigil[0] = draw_c.Draw_PicFromWad("sb_sigil1");
        sb_sigil[1] = draw_c.Draw_PicFromWad("sb_sigil2");
        sb_sigil[2] = draw_c.Draw_PicFromWad("sb_sigil3");
        sb_sigil[3] = draw_c.Draw_PicFromWad("sb_sigil4");

        sb_faces[4][0] = draw_c.Draw_PicFromWad("face1");
        sb_faces[4][1] = draw_c.Draw_PicFromWad("face_p1");
        sb_faces[3][0] = draw_c.Draw_PicFromWad("face2");
        sb_faces[3][1] = draw_c.Draw_PicFromWad("face_p2");
        sb_faces[2][0] = draw_c.Draw_PicFromWad("face3");
        sb_faces[2][1] = draw_c.Draw_PicFromWad("face_p3");
        sb_faces[1][0] = draw_c.Draw_PicFromWad("face4");
        sb_faces[1][1] = draw_c.Draw_PicFromWad("face_p4");
        sb_faces[0][0] = draw_c.Draw_PicFromWad("face5");
        sb_faces[0][1] = draw_c.Draw_PicFromWad("face_p5");

        sb_face_invis = draw_c.Draw_PicFromWad("face_invis");
        sb_face_invuln = draw_c.Draw_PicFromWad("face_invul2");
        sb_face_invis_invuln = draw_c.Draw_PicFromWad("face_inv2");
        sb_face_quad = draw_c.Draw_PicFromWad("face_quad");

        cmd_c.Cmd_AddCommand("+showscores", Sbar_ShowScores);
        cmd_c.Cmd_AddCommand("-showscores", Sbar_DontShowScores);

        sb_sbar = draw_c.Draw_PicFromWad("sbar");
        sb_ibar = draw_c.Draw_PicFromWad("ibar");
        sb_scorebar = draw_c.Draw_PicFromWad("scorebar");

        if (common_c.hipnotic)
        {
            hsb_weapons[0][0] = draw_c.Draw_PicFromWad("inv_laser");
            hsb_weapons[0][1] = draw_c.Draw_PicFromWad("inv_mjolnir");
            hsb_weapons[0][2] = draw_c.Draw_PicFromWad("inv_gren_prox");
            hsb_weapons[0][3] = draw_c.Draw_PicFromWad("inv_prox_gren");
            hsb_weapons[0][4] = draw_c.Draw_PicFromWad("inv_prox");

            hsb_weapons[1][0] = draw_c.Draw_PicFromWad("inv2_laser");
            hsb_weapons[1][1] = draw_c.Draw_PicFromWad("inv2_mjolnir");
            hsb_weapons[1][2] = draw_c.Draw_PicFromWad("inv2_gren_prox");
            hsb_weapons[1][3] = draw_c.Draw_PicFromWad("inv2_prox_gren");
            hsb_weapons[1][4] = draw_c.Draw_PicFromWad("inv2_prox");

            for (i = 0; i < 5; i++)
            {
                hsb_weapons[2 + i][0] = draw_c.Draw_PicFromWad(common_c.va($"inva{i + 1}_laser"));
                hsb_weapons[2 + i][1] = draw_c.Draw_PicFromWad(common_c.va($"inva{i + 1}_mjolnir"));
                hsb_weapons[2 + i][2] = draw_c.Draw_PicFromWad(common_c.va($"inva{i + 1}_gren_prox"));
                hsb_weapons[2 + i][3] = draw_c.Draw_PicFromWad(common_c.va($"inva{i + 1}_prox_gren"));
                hsb_weapons[2 + i][4] = draw_c.Draw_PicFromWad(common_c.va($"inva{i + 1}_prox"));
            }

            hsb_items[0] = draw_c.Draw_PicFromWad("sb_wsuit");
            hsb_items[1] = draw_c.Draw_PicFromWad("sb_eshld");
        }

        if (common_c.rogue)
        {
            rsb_invbar[0] = draw_c.Draw_PicFromWad("r_invbar1");
            rsb_invbar[1] = draw_c.Draw_PicFromWad("r_invbar2");

            rsb_weapons[0] = draw_c.Draw_PicFromWad("r_lava");
            rsb_weapons[1] = draw_c.Draw_PicFromWad("r_superlava");
            rsb_weapons[2] = draw_c.Draw_PicFromWad("r_gren");
            rsb_weapons[3] = draw_c.Draw_PicFromWad("r_multirock");
            rsb_weapons[4] = draw_c.Draw_PicFromWad("r_plasma");

            rsb_items[0] = draw_c.Draw_PicFromWad("r_shield1");
            rsb_items[1] = draw_c.Draw_PicFromWad("r_agrav1");

            rsb_teambord = draw_c.Draw_PicFromWad("r_teambord");

            rsb_ammo[0] = draw_c.Draw_PicFromWad("r_ammolava");
            rsb_ammo[1] = draw_c.Draw_PicFromWad("r_ammomulti");
            rsb_ammo[2] = draw_c.Draw_PicFromWad("r_ammoplasma");
        }
    }

    public static void Sbar_DrawPic(int x, int y, wad_c.qpic_t* pic)
    {
        if (cl_main_c.cl.gametype == protocol_c.GAME_DEATHMATCH)
        {
            draw_c.Draw_Pic(x, y + (int)(vid_c.vid.height - SBAR_HEIGHT), pic);
        }
        else
        {
            draw_c.Draw_Pic(x + (int)((vid_c.vid.width - 320) >> 1), y + (int)(vid_c.vid.height - SBAR_HEIGHT), pic);
        }
    }

    public static void Sbar_DrawTransPic(int x, int y, wad_c.qpic_t* pic)
    {
        if (cl_main_c.cl.gametype == protocol_c.GAME_DEATHMATCH)
        {
            draw_c.Draw_TransPic(x, y + (int)(vid_c.vid.height - SBAR_HEIGHT), pic);
        }
        else
        {
            draw_c.Draw_TransPic(x + (int)((vid_c.vid.width - 320) >> 1), y + (int)(vid_c.vid.height - SBAR_HEIGHT), pic);
        }
    }

    public static void Sbar_DrawCharacter(int x, int y, int num)
    {
        if (cl_main_c.cl.gametype == protocol_c.GAME_DEATHMATCH)
        {
            draw_c.Draw_Character(x + 4, y + (int)(vid_c.vid.width - SBAR_HEIGHT), num);
        }
        else
        {
            draw_c.Draw_Character(x + (int)((vid_c.vid.width - 320) >> 1) + 4, y + (int)(vid_c.vid.height - SBAR_HEIGHT), num);
        }
    }

    public static void Sbar_DrawString(int x, int y, char* str)
    {
        if (cl_main_c.cl.gametype == protocol_c.GAME_DEATHMATCH)
        {
            draw_c.Draw_String(x, y + (int)(vid_c.vid.height - SBAR_HEIGHT), str);
        }
        else
        {
            draw_c.Draw_String(x + (int)((vid_c.vid.width - 320) >> 1), y + (int)(vid_c.vid.height - SBAR_HEIGHT), str);
        }
    }

    public static int Sbar_itao(int num, char* buf)
    {
        char* str;
        int pow10;
        int dig;

        str = buf;

        if (num < 0)
        {
            *str++ = '-';
            num = -num;
        }

        for (pow10 = 10; num >= pow10; pow10 *= 10);

        do
        {
            pow10 /= 10;
            dig = num / pow10;
            *str++ = (char)('0' + dig);
            num -= dig * pow10;
        } while (pow10 != 1);

        *str = (char)0;

        return (int)(str - buf);
    }

    public static void Sbar_DrawNum(int x, int y, int num, int digits, int color)
    {
        char* str;
        char* ptr;
        int l, frame;

        str = ptr = null;

        l = Sbar_itao(num, str);
        ptr = str;

        if (l > digits)
        {
            ptr += (l - digits);
        }

        if (l < digits)
        {
            x += (digits - l) * 24;
        }

        while (*ptr != 0)
        {
            if (*ptr == '-')
            {
                frame = STAT_MINUS;
            }
            else
            {
                frame = *ptr - '0';
            }

            Sbar_DrawTransPic(x, y, sb_nums[color][frame]);
            x += 24;
            ptr++;
        }
    }

    public static int[] fragsort = new int[quakedef_c.MAX_SCOREBOARD];

    public static char[][] scoreboardtext = new char[quakedef_c.MAX_SCOREBOARD][];
    public static int[] scoreboardtop = new int[quakedef_c.MAX_SCOREBOARD];
    public static int[] scoreboardbottom = new int[quakedef_c.MAX_SCOREBOARD];
    public static int[] scoreboardcount = new int[quakedef_c.MAX_SCOREBOARD];
    public static int scoreboardlines;

    public static void Sbar_SortFrags()
    {
        int i, j, k;

        for (i = 0; i < cl_main_c.cl.maxclients; i++)
        {
            if (cl_main_c.cl.scores[i].name[0] != 0)
            {
                fragsort[scoreboardlines] = i;
                scoreboardlines++;
            }
        }

        for (i = 0; i < scoreboardlines; i++)
        {
            for (j = 0; j < scoreboardlines - 1 - i; j++)
            {
                if (cl_main_c.cl.scores[fragsort[j]].frags < cl_main_c.cl.scores[fragsort[j + 1]].frags)
                {
                    k = fragsort[j];
                    fragsort[j] = fragsort[j + 1];
                    fragsort[j + 1] = k;
                }
            }
        }
    }

    public static int Sbar_ColorForMap(int m)
    {
        return m < 128 ? m + 8 : m + 8;
    }

    public static void Sbar_UpdateScoreboard()
    {
        int i, k;
        int top, bottom;
        client_c.scoreboard_t* s;

        Sbar_SortFrags();

        common_c.Q_memset(scoreboardtext, 0, scoreboardtext.Length);

        for (i = 0; i < scoreboardlines; i++)
        {
            k = fragsort[i];
            s = &cl_main_c.cl.scores[k];
            Console.WriteLine($"{s->frags} {*s->name}");

            top = s->colors & 0xf0;
            bottom = (s->colors & 15) << 4;
            scoreboardtop[i] = Sbar_ColorForMap(top);
            scoreboardbottom[i] = Sbar_ColorForMap(bottom);
        }
    }

    public static void Sbar_SoloScoreboard()
    {
        char* str = null;
        int minutes, seconds, tens, units;
        int l;

        Console.WriteLine(str->ToString(), $"Monsters: {cl_main_c.cl.stats[quakedef_c.STAT_MONSTERS]} /{cl_main_c.cl.stats[quakedef_c.STAT_TOTALMONSTERS]}");
        Sbar_DrawString(8, 4, str);

        Console.WriteLine(str->ToString(), $"Secrets: {cl_main_c.cl.stats[quakedef_c.STAT_SECRETS]} /{cl_main_c.cl.stats[quakedef_c.STAT_TOTALSECRETS]}");
        Sbar_DrawString(8, 12, str);

        minutes = (int)cl_main_c.cl.time / 60;
        seconds = (int)cl_main_c.cl.time - 60 * minutes;
        tens = seconds / 10;
        units = seconds - 10 * tens;
        Console.WriteLine(str->ToString(), $"Time: {minutes}:{tens}:{units}");
        Sbar_DrawString(184, 4, str);

        l = common_c.Q_strlen(cl_main_c.cl.levelname);
        Sbar_DrawString(232 - l * 4, 12, cl_main_c.cl.levelname);
    }

    public static void Sbar_DrawScoreboard()
    {
        Sbar_SoloScoreboard();

        if (cl_main_c.cl.gametype == protocol_c.GAME_DEATHMATCH)
        {
            Sbar_DeathmatchOverlay();
        }
    }

    public static void Sbar_DrawInventory()
    {
        int i;
        char* num = null;
        float time;
        int flashon;

        if (common_c.rogue)
        {
            if (cl_main_c.cl.stats[quakedef_c.STAT_ACTIVEWEAPON] >= quakedef_c.RIT_LAVA_NAILGUN)
            {
                Sbar_DrawPic(0, -24, rsb_invbar[0]);
            }
            else
            {
                Sbar_DrawPic(0, -24, rsb_invbar[1]);
            }
        }
        else
        {
            Sbar_DrawPic(0, -24, sb_ibar);
        }

        for (i = 0; i < 7; i++)
        {
            if ((cl_main_c.cl.items & (quakedef_c.IT_SHOTGUN << i)) != 0)
            {
                time = cl_main_c.cl.item_gettime[i];
                flashon = (int)((cl_main_c.cl.time - time) * 10);

                if (flashon >= 10)
                {
                    if (cl_main_c.cl.stats[quakedef_c.STAT_ACTIVEWEAPON] == (quakedef_c.IT_SHOTGUN << i))
                    {
                        flashon = 1;
                    }
                    else
                    {
                        flashon = 0;
                    }
                }
                else
                {
                    flashon = (flashon % 3) + 2;
                }

                Sbar_DrawPic(i * 24, -16, sb_weapons[flashon][i]);

                if (flashon > 1)
                {
                    sb_updates = 0;
                }
            }
        }

        if (common_c.hipnotic)
        {
            int grenadeflashing = 0;

            for (i = 0; i < 4; i++)
            {
                if ((cl_main_c.cl.items & (1 << hipweapons[i])) != 0)
                {
                    time = cl_main_c.cl.item_gettime[hipweapons[i]];
                    flashon = (int)((cl_main_c.cl.time - time) * 10);

                    if (flashon >= 10)
                    {
                        if (cl_main_c.cl.stats[quakedef_c.STAT_ACTIVEWEAPON] == (1 << hipweapons[i]))
                        {
                            flashon = 1;
                        }
                        else
                        {
                            flashon = 0;
                        }
                    }
                    else
                    {
                        flashon = (flashon % 5) + 2;
                    }

                    if (i == 2)
                    {
                        if ((cl_main_c.cl.items & quakedef_c.HIT_PROXIMITY_GUN) != 0)
                        {
                            if (flashon != 0)
                            {
                                grenadeflashing = 1;
                                Sbar_DrawPic(96, -16, hsb_weapons[flashon][4]);
                            }
                        }
                    }
                    else if (i == 3)
                    {
                        if ((cl_main_c.cl.items & (quakedef_c.IT_SHOTGUN << 4)) != 0)
                        {
                            if (flashon != 0 && grenadeflashing == 0)
                            {
                                Sbar_DrawPic(96, -16, hsb_weapons[flashon][3]);
                            }
                            else if (grenadeflashing == 0)
                            {
                                Sbar_DrawPic(96, -16, hsb_weapons[0][3]);
                            }
                        }
                        else
                        {
                            Sbar_DrawPic(96, -16, hsb_weapons[flashon][4]);
                        }
                    }
                    else
                    {
                        Sbar_DrawPic(176 + (i * 24), -16, hsb_weapons[flashon][i]);
                    }

                    if (flashon > 1)
                    {
                        sb_updates = 0;
                    }
                }
            }
        }

        if (common_c.rogue)
        {
            if (cl_main_c.cl.stats[quakedef_c.STAT_ACTIVEWEAPON] >= quakedef_c.RIT_LAVA_NAILGUN)
            {
                for (i = 0; i < 5; i++)
                {
                    if (cl_main_c.cl.stats[quakedef_c.STAT_ACTIVEWEAPON] == (quakedef_c.RIT_LAVA_NAILGUN << i))
                    {
                        Sbar_DrawPic((i + 2) * 24, -16, rsb_weapons[i]);
                    }
                }
            }
        }

        for (i = 0; i < 4; i++)
        {
            Console.WriteLine(num->ToString(), $"{cl_main_c.cl.stats[quakedef_c.STAT_SHELLS + i]}");

            if (num[0] != ' ')
            {
                Sbar_DrawCharacter((6 * i + 1) * 8 - 2, -24, 18 + num[0] - '0');
            }

            if (num[1] != ' ')
            {
                Sbar_DrawCharacter((6 * i + 2) * 8 - 2, -24, 18 + num[1] - '0');
            }

            if (num[2] != ' ')
            {
                Sbar_DrawCharacter((6 * i + 3) * 8 - 2, -24, 18 + num[2] - '0');
            }
        }

        flashon = 0;

        for (i = 0; i < 6; i++)
        {
            if ((cl_main_c.cl.items & (1 << (17 + i))) != 0)
            {
                time = cl_main_c.cl.item_gettime[17 + i];

                if (time != 0 && time > cl_main_c.cl.time - 2 && flashon != 0)
                {
                    sb_updates = 0;
                }
                else
                {
                    if (!common_c.hipnotic || (i > 1))
                    {
                        Sbar_DrawPic(192 + i * 16, -16, sb_items[i]);
                    }
                }

                if (time != 0 && time > cl_main_c.cl.time - 2)
                {
                    sb_updates = 0;
                }
            }
        }

        if (common_c.hipnotic)
        {
            for (i = 0; i < 2; i++)
            {
                if ((cl_main_c.cl.items & (1 << (17 + i))) != 0)
                {
                    time = cl_main_c.cl.item_gettime[17 + i];

                    if (time != 0 && time > cl_main_c.cl.time - 2 && flashon != 0)
                    {
                        sb_updates = 0;
                    }
                    else
                    {
                        Sbar_DrawPic(288 + i * 16, -16, hsb_items[i]);
                    }

                    if (time != 0 && time > cl_main_c.cl.time - 2)
                    {
                        sb_updates = 0;
                    }
                }
            }
        }

        if (common_c.rogue)
        {
            for (i = 0; i < 2; i++)
            {
                if ((cl_main_c.cl.items & (1 << (29 + i))) != 0)
                {
                    time = cl_main_c.cl.item_gettime[29 + i];

                    if (time != 0 && time > cl_main_c.cl.time - 2 && flashon != 0)
                    {
                        sb_updates = 0;
                    }
                    else
                    {
                        Sbar_DrawPic(288 + i * 16, -16, rsb_items[i]);
                    }

                    if (time != 0 && time > cl_main_c.cl.time - 2)
                    {
                        sb_updates = 0;
                    }
                }
            }
        }
        else
        {
            for (i = 0; i < 4; i++)
            {
                if ((cl_main_c.cl.items & (1 << (28 + i))) != 0)
                {
                    time = cl_main_c.cl.item_gettime[28 + i];

                    if (time != 0 && time > cl_main_c.cl.time - 2 && flashon != 0)
                    {
                        sb_updates = 0;
                    }
                    else
                    {
                        Sbar_DrawPic(320 - 32 + i * 8, -16, sb_sigil[i]);
                    }

                    if (time != 0 && time > cl_main_c.cl.time - 2)
                    {
                        sb_updates = 0;
                    }
                }
            }
        }
    }

    public static void Sbar_DrawFrags()
    {
        int i, k, l;
        int top, bottom;
        int x, y, f;
        int xofs;
        char* num = null;
        client_c.scoreboard_t* s;

        Sbar_SortFrags();

        l = scoreboardlines <= 4 ? scoreboardlines : 4;

        x = 23;

        if (cl_main_c.cl.gametype == protocol_c.GAME_DEATHMATCH)
        {
            xofs = 0;
        }
        else
        {
            xofs = (int)(vid_c.vid.width - 320) >> 1;
        }

        y = (int)vid_c.vid.height - SBAR_HEIGHT - 23;

        for (i = 0; i < l; i++)
        {
            k = fragsort[i];
            s = &cl_main_c.cl.scores[k];

            if (s->name[0] == 0)
            {
                continue;
            }

            top = s->colors & 0xf0;
            bottom = (s->colors & 15) << 4;
            top = Sbar_ColorForMap(top);
            bottom = Sbar_ColorForMap(bottom);

            draw_c.Draw_Fill(xofs + x * 8 + 10, y, 28, 4, top);
            draw_c.Draw_Fill(xofs + x * 8 + 10, y + 4, 28, 3, bottom);

            f = s->frags;
            Console.WriteLine(num->ToString(), $"{f}");

            Sbar_DrawCharacter((x + 1) * 8, -24, num[0]);
            Sbar_DrawCharacter((x + 2) * 8, -24, num[1]);
            Sbar_DrawCharacter((x + 3) * 8, -24, num[2]);

            if (k == cl_main_c.cl.viewentity - 1)
            {
                Sbar_DrawCharacter(x * 8 + 2, -24, 16);
                Sbar_DrawCharacter((x + 4) * 8 - 4, -24, 17);
            }

            x += 4;
        }
    }

    public static void Sbar_DrawFace()
    {
        int f, anim;

        if (common_c.rogue && (cl_main_c.cl.maxclients != 1) && (server_c.teamplay.value > 3) && (server_c.teamplay.value < 7))
        {
            int top, bottom;
            int xofs;
            char* num = null;
            client_c.scoreboard_t* s;

            s = &cl_main_c.cl.scores[cl_main_c.cl.viewentity - 1];
            top = s->colors & 0xf0;
            bottom = (s->colors & 15) << 4;
            top = Sbar_ColorForMap(top);
            bottom = Sbar_ColorForMap(bottom);

            if (cl_main_c.cl.gametype == protocol_c.GAME_DEATHMATCH)
            {
                xofs = 113;
            }
            else
            {
                xofs = (((int)vid_c.vid.width - 320) >> 1) + 113;
            }

            Sbar_DrawPic(112, 0, rsb_teambord);
            draw_c.Draw_Fill(xofs, (int)vid_c.vid.height - SBAR_HEIGHT + 3, 22, 9, bottom);
            draw_c.Draw_Fill(xofs, (int)vid_c.vid.height - SBAR_HEIGHT + 12, 22, 9, bottom);

            f = s->frags;
            Console.WriteLine(num->ToString(), $"{f}");

            if (top == 8)
            {
                if (num[0] != ' ')
                {
                    Sbar_DrawCharacter(109, 3, 18 + num[0] - '0');
                }

                if (num[1] != ' ')
                {
                    Sbar_DrawCharacter(116, 3, 18 + num[1] - '0');
                }

                if (num[2] != ' ')
                {
                    Sbar_DrawCharacter(123, 3, 18 + num[2] - '0');
                }
            }
            else
            {
                Sbar_DrawCharacter(109, 3, num[0]);
                Sbar_DrawCharacter(116, 3, num[1]);
                Sbar_DrawCharacter(123, 3, num[2]);
            }

            return;
        }

        if ((cl_main_c.cl.items & (quakedef_c.IT_INVISIBILITY | quakedef_c.IT_INVULNERABILITY)) == (quakedef_c.IT_INVISIBILITY | quakedef_c.IT_INVULNERABILITY))
        {
            Sbar_DrawPic(112, 0, sb_face_invis_invuln);
            return;
        }

        if ((cl_main_c.cl.items & quakedef_c.IT_QUAD) != 0)
        {
            Sbar_DrawPic(112, 0, sb_face_quad);
            return;
        }

        if ((cl_main_c.cl.items & quakedef_c.IT_INVISIBILITY) != 0)
        {
            Sbar_DrawPic(112, 0, sb_face_invis);
        }

        if ((cl_main_c.cl.items & quakedef_c.IT_INVULNERABILITY) != 0)
        {
            Sbar_DrawPic(112, 0, sb_face_invuln);
            return;
        }

        if (cl_main_c.cl.stats[quakedef_c.STAT_HEALTH] >= 100)
        {
            f = 4;
        }
        else
        {
            f = cl_main_c.cl.stats[quakedef_c.STAT_HEALTH] / 20;
        }

        if (cl_main_c.cl.time <= cl_main_c.cl.faceanimtime)
        {
            anim = 1;
            sb_updates = 0;
        }
        else
        {
            anim = 0;
        }

        Sbar_DrawPic(112, 0, sb_faces[f][anim]);
    }

    public static void Sbar_Draw()
    {
        if (screen_c.scr_con_current == vid_c.vid.height)
        {
            return;
        }

        if (sb_updates >= vid_c.vid.numpages)
        {
            return;
        }

        screen_c.scr_copyeverything = 1;

        sb_updates++;

        if (sb_lines != 0 && vid_c.vid.width > 320)
        {
            draw_c.Draw_TileClear(0, (int)vid_c.vid.height - sb_lines, (int)vid_c.vid.width, sb_lines);
        }

        if (sb_lines > 24)
        {
            Sbar_DrawInventory();

            if (cl_main_c.cl.maxclients != 1)
            {
                Sbar_DrawFrags();
            }
        }

        if (sb_showscores || cl_main_c.cl.stats[quakedef_c.STAT_HEALTH] <= 0)
        {
            Sbar_DrawPic(0, 0, sb_scorebar);
            Sbar_DrawScoreboard();
            sb_updates = 0;
        }
        else if (sb_lines != 0)
        {
            Sbar_DrawPic(0, 0, sb_sbar);

            if (common_c.hipnotic)
            {
                if ((cl_main_c.cl.items & quakedef_c.IT_KEY1) != 0)
                {
                    Sbar_DrawPic(209, 3, sb_items[0]);
                }

                if ((cl_main_c.cl.items & quakedef_c.IT_KEY2) != 0)
                {
                    Sbar_DrawPic(209, 12, sb_items[1]);
                }

                if ((cl_main_c.cl.items & quakedef_c.IT_INVULNERABILITY) != 0)
                {
                    Sbar_DrawNum(24, 0, 666, 3, 1);
                }
                else
                {
                    if (common_c.rogue)
                    {
                        Sbar_DrawNum(24, 0, cl_main_c.cl.stats[quakedef_c.STAT_ARMOR], 3, cl_main_c.cl.stats[quakedef_c.STAT_ARMOR] <= 25 ? 1 : 0);

                        if ((cl_main_c.cl.items & quakedef_c.RIT_ARMOR3) != 0)
                        {
                            Sbar_DrawPic(0, 0, sb_armor[2]);
                        }
                        else if ((cl_main_c.cl.items & quakedef_c.RIT_ARMOR2) != 0)
                        {
                            Sbar_DrawPic(0, 0, sb_armor[1]);
                        }
                        else if ((cl_main_c.cl.items & quakedef_c.RIT_ARMOR1) != 0)
                        {
                            Sbar_DrawPic(0, 0, sb_armor[0]);
                        }
                    }
                    else
                    {
                        Sbar_DrawNum(24, 0, cl_main_c.cl.stats[quakedef_c.STAT_ARMOR], 3, cl_main_c.cl.stats[quakedef_c.STAT_ARMOR] <= 25 ? 1 : 0);

                        if ((cl_main_c.cl.items & quakedef_c.IT_ARMOR3) != 0)
                        {
                            Sbar_DrawPic(0, 0, sb_armor[2]);
                        }
                        else if ((cl_main_c.cl.items & quakedef_c.IT_ARMOR2) != 0)
                        {
                            Sbar_DrawPic(0, 0, sb_armor[1]);
                        }
                        else if ((cl_main_c.cl.items & quakedef_c.IT_ARMOR1) != 0)
                        {
                            Sbar_DrawPic(0, 0, sb_armor[0]);
                        }
                    }
                }
            }

            Sbar_DrawFace();

            Sbar_DrawNum(136, 0, cl_main_c.cl.stats[quakedef_c.STAT_HEALTH], 3, cl_main_c.cl.stats[quakedef_c.STAT_HEALTH] <= 25 ? 1 : 0);

            if (common_c.rogue)
            {
                if ((cl_main_c.cl.items & quakedef_c.RIT_SHELLS) != 0)
                {
                    Sbar_DrawPic(224, 0, sb_ammo[0]);
                }
                else if ((cl_main_c.cl.items & quakedef_c.RIT_NAILS) != 0)
                {
                    Sbar_DrawPic(224, 0, sb_ammo[1]);
                }
                else if ((cl_main_c.cl.items & quakedef_c.RIT_ROCKETS) != 0)
                {
                    Sbar_DrawPic(224, 0, sb_ammo[2]);
                }
                else if ((cl_main_c.cl.items & quakedef_c.RIT_CELLS) != 0)
                {
                    Sbar_DrawPic(224, 0, sb_ammo[3]);
                }
                else if ((cl_main_c.cl.items & quakedef_c.RIT_LAVA_NAILS) != 0)
                {
                    Sbar_DrawPic(224, 0, rsb_ammo[0]);
                }
                else if ((cl_main_c.cl.items & quakedef_c.RIT_PLASMA_AMMO) != 0)
                {
                    Sbar_DrawPic(224, 0, rsb_ammo[1]);
                }
                else if ((cl_main_c.cl.items & quakedef_c.RIT_MULTI_ROCKETS) != 0)
                {
                    Sbar_DrawPic(224, 0, rsb_ammo[2]);
                }
            }
            else
            {
                if ((cl_main_c.cl.items & quakedef_c.IT_SHELLS) != 0)
                {
                    Sbar_DrawPic(224, 0, sb_ammo[0]);
                }
                else if ((cl_main_c.cl.items & quakedef_c.IT_NAILS) != 0)
                {
                    Sbar_DrawPic(224, 0, sb_ammo[1]);
                }
                else if ((cl_main_c.cl.items & quakedef_c.IT_ROCKETS) != 0)
                {
                    Sbar_DrawPic(224, 0, sb_ammo[2]);
                }
                else if ((cl_main_c.cl.items & quakedef_c.IT_CELLS) != 0)
                {
                    Sbar_DrawPic(224, 0, sb_ammo[3]);
                }
            }

            Sbar_DrawNum(248, 0, cl_main_c.cl.stats[quakedef_c.STAT_AMMO], 3, cl_main_c.cl.stats[quakedef_c.STAT_AMMO] <= 10 ? 1 : 0);
        }

        if (vid_c.vid.width > 320)
        {
            if (cl_main_c.cl.gametype == protocol_c.GAME_DEATHMATCH)
            {
                Sbar_MiniDeathmatchOverlay();
            }
        }
    }

    public static void Sbar_IntermissionNumber(int x, int y, int num, int digits, int color)
    {
        char* str = null;
        char* ptr;
        int l, frame;

        l = Sbar_itao(num, str);
        ptr = str;

        if (l > digits)
        {
            ptr += (l - digits);
        }

        if (l < digits)
        {
            x += (digits - l) * 24;
        }

        while (*ptr != 0)
        {
            if (*ptr == '-')
            {
                frame = STAT_MINUS;
            }
            else
            {
                frame = *ptr - '0';
            }

            draw_c.Draw_TransPic(x, y, sb_nums[color][frame]);
            x += 24;
            ptr++;
        }
    }

    public static void Sbar_DeathmatchOverlay()
    {
        wad_c.qpic_t* pic;
        int i, k, l;
        int top, bottom;
        int x, y, f;
        char* num = null;
        client_c.scoreboard_t* s;

        screen_c.scr_copyeverything = 1;
        screen_c.scr_fullupdate = 0;

        pic = draw_c.Draw_CachePic("gfx/ranking.lmp");
        menu_c.M_DrawPic((320 - pic->width) / 2, 8, pic);

        Sbar_SortFrags();

        l = scoreboardlines;

        x = 80 + (int)((vid_c.vid.width - 320) >> 1);
        y = 40;

        for (i = 0; i < l; i++)
        {
            k = fragsort[i];
            s = &cl_main_c.cl.scores[k];

            if (s->name[0] == 0)
            {
                continue;
            }

            top = s->colors & 0xf0;
            bottom = (s->colors & 15) << 4;
            top = Sbar_ColorForMap(top);
            bottom = Sbar_ColorForMap(bottom);

            f = s->frags;
            Console.WriteLine(num->ToString(), $"{f}");

            draw_c.Draw_Character(x + 8, y, num[0]);
            draw_c.Draw_Character(x + 16, y, num[1]);
            draw_c.Draw_Character(x + 24, y, num[2]);

            if (k == cl_main_c.cl.viewentity - 1)
            {
                draw_c.Draw_Character(x - 8, y, 12);
            }

            draw_c.Draw_String(x + 64, y, s->name);

            y += 10;
        }
    }

    public static void Sbar_MiniDeathmatchOverlay()
    {
        wad_c.qpic_t* pic;
        int i, k, l;
        int top, bottom;
        int x, y, f;
        char* num = null;
        client_c.scoreboard_t* s;
        int numlines;

        if (vid_c.vid.width < 512 || sb_lines == 0)
        {
            return;
        }

        screen_c.scr_copyeverything = 1;
        screen_c.scr_fullupdate = 0;

        Sbar_SortFrags();

        l = scoreboardlines;
        y = (int)vid_c.vid.height - sb_lines;
        numlines = sb_lines / 8;

        if (numlines < 3)
        {
            return;
        }

        for (i = 0; i < scoreboardlines; i++)
        {
            if (fragsort[i] == cl_main_c.cl.viewentity - 1)
            {
                break;
            }
        }

        if (i == scoreboardlines)
        {
            i = 0;
        }
        else
        {
            i = i - numlines / 2;
        }

        if (i > scoreboardlines - numlines)
        {
            i = scoreboardlines - numlines;
        }

        if (i < 0)
        {
            i = 0;
        }

        x = 324;

        for (; i < scoreboardlines && y < vid_c.vid.height - 8; i++)
        {
            k = fragsort[i];
            s = &cl_main_c.cl.scores[k];

            if (s->name[0] == 0)
            {
                continue;
            }

            top = s->colors & 0xf0;
            bottom = (s->colors & 15) << 4;
            top = Sbar_ColorForMap(top);
            bottom = Sbar_ColorForMap(bottom);

            draw_c.Draw_Fill(x, y + 1, 40, 3, top);
            draw_c.Draw_Fill(x, y + 4, 40, 4, bottom);

            f = s->frags;
            Console.WriteLine(num->ToString(), $"{f}");

            draw_c.Draw_Character(x + 8, y, num[0]);
            draw_c.Draw_Character(x + 16, y, num[1]);
            draw_c.Draw_Character(x + 24, y, num[2]);

            if (k == cl_main_c.cl.viewentity - 1)
            {
                draw_c.Draw_Character(x, y, 16);
                draw_c.Draw_Character(x + 32, y, 17);
            }

            draw_c.Draw_String(x + 48, y, s->name);

            y += 8;
        }
    }

    public static void Sbar_IntermissionOverlay()
    {
        wad_c.qpic_t* pic;
        int dig;
        int num;

        screen_c.scr_copyeverything = 1;
        screen_c.scr_fullupdate = 0;

        if (cl_main_c.cl.gametype == protocol_c.GAME_DEATHMATCH)
        {
            Sbar_DeathmatchOverlay();
            return;
        }

        pic = draw_c.Draw_CachePic("gfx/complete.lmp");
        draw_c.Draw_Pic(64, 24, pic);

        pic = draw_c.Draw_CachePic("gfx/inter.lmp");
        draw_c.Draw_Pic(0, 56, pic);

        dig = cl_main_c.cl.completed_time / 60;
        Sbar_IntermissionNumber(160, 64, dig, 3, 0);
        num = cl_main_c.cl.completed_time - dig * 60;
        draw_c.Draw_TransPic(234, 64, sb_colon);
        draw_c.Draw_TransPic(246, 64, sb_nums[0][num / 10]);
        draw_c.Draw_TransPic(266, 64, sb_nums[0][num % 10]);

        Sbar_IntermissionNumber(160, 104, cl_main_c.cl.stats[quakedef_c.STAT_SECRETS], 3, 0);
        draw_c.Draw_TransPic(232, 104, sb_slash);
        Sbar_IntermissionNumber(240, 104, cl_main_c.cl.stats[quakedef_c.STAT_TOTALSECRETS], 3, 0);

        Sbar_IntermissionNumber(160, 144, cl_main_c.cl.stats[quakedef_c.STAT_MONSTERS], 3, 0);
        draw_c.Draw_TransPic(232, 133, sb_slash);
        Sbar_IntermissionNumber(240, 144, cl_main_c.cl.stats[quakedef_c.STAT_TOTALMONSTERS], 3, 0);
    }

    public static void Sbar_FinaleOverlay()
    {
        wad_c.qpic_t* pic;

        screen_c.scr_copyeverything = 1;

        pic = draw_c.Draw_CachePic("gfx/finale.lmp");
        draw_c.Draw_TransPic((int)(vid_c.vid.width - pic->width) / 2, 16, pic);
    }
}