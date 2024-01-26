namespace Quake;

public unsafe class cl_parse_c
{
    public static char*[] svc_strings =
    {
        common_c.StringToChar("svc_bad"),
        common_c.StringToChar("svc_nop"),
        common_c.StringToChar("svc_disconnect"),
        common_c.StringToChar("svc_updatestat"),
        common_c.StringToChar("svc_version"),
        common_c.StringToChar("svc_setview"),
        common_c.StringToChar("svc_sound"),
        common_c.StringToChar("svc_time"),
        common_c.StringToChar("svc_print"),
        common_c.StringToChar("svc_stufftext"),
        common_c.StringToChar("svc_setangle"),
        common_c.StringToChar("svc_serverinfo"),
        common_c.StringToChar("svc_lightstyle"),
        common_c.StringToChar("svc_updatename"),
        common_c.StringToChar("svc_updatefrags"),
        common_c.StringToChar("svc_clientdata"),
        common_c.StringToChar("svc_stopsound"),
        common_c.StringToChar("svc_updatecolors"),
        common_c.StringToChar("svc_particle"),
        common_c.StringToChar("svc_damage"),
        common_c.StringToChar("svc_spawnstatic"),
        common_c.StringToChar("OBSOLETE svc_spawnbinary"),
        common_c.StringToChar("svc_spawnbaseline"),
        common_c.StringToChar("svc_temp_entity"),
        common_c.StringToChar("svc_setpause"),
        common_c.StringToChar("svc_signonnum"),
        common_c.StringToChar("svc_centerprint"),
        common_c.StringToChar("svc_killedmonster"),
        common_c.StringToChar("svc_foundsecret"),
        common_c.StringToChar("svc_spawnstaticsound"),
        common_c.StringToChar("svc_intermission"),
        common_c.StringToChar("svc_finale"),
        common_c.StringToChar("svc_cdtrack"),
        common_c.StringToChar("svc_sellscreen"),
        common_c.StringToChar("svc_cutscene")
    };

    public static render_c.entity_t* CL_EntityNum(int num)
    {
        if (num >= cl_main_c.cl.num_entities)
        {
            if (num >= quakedef_c.MAX_EDICTS)
            {
                host_c.Host_Error($"CL_EntityNum: {num} is an invalid number");
            }

            while (cl_main_c.cl.num_entities <= num)
            {
                cl_main_c.cl_entities[cl_main_c.cl.num_entities].colormap = vid_c.vid.colormap;
                cl_main_c.cl.num_entities++;
            }
        }

        return &cl_main_c.cl_entities[num];
    }

    public static void CL_ParseStartSoundPacket()
    {
        Vector3 pos = new();
        int channel, ent;
        int sound_num;
        int volume;
        int field_mask;
        float attenuation;
        int i;

        field_mask = common_c.MSG_ReadByte();

        if ((field_mask & protocol_c.SND_VOLUME) != 0)
        {
            volume = common_c.MSG_ReadByte();
        }
        else
        {
            volume = sound_c.DEFAULT_SOUND_PACKET_VOLUME;
        }

        if ((field_mask & protocol_c.SND_ATTENUATION) != 0)
        {
            attenuation = common_c.MSG_ReadByte() / 64.0f;
        }
        else
        {
            attenuation = sound_c.DEFAULT_SOUND_PACKET_ATTENUATION;
        }

        channel = common_c.MSG_ReadShort();
        sound_num = common_c.MSG_ReadByte();

        ent = channel >> 3;
        channel &= 7;

        if (ent > quakedef_c.MAX_EDICTS)
        {
            host_c.Host_Error($"CL_ParseStartSoundPacket: ent = {ent}");
        }

        for (i = 0; i < 3; i++)
        {
            pos[i] = common_c.MSG_ReadCoord();
        }

        snd_dma_c.S_StartSound(ent, channel, cl_main_c.cl.sound_precache[sound_num], pos, volume / 255.0, attenuation);
    }

    public static void CL_KeepaliveMessage()
    {
        float time;
        float lastmsg = 0.0f;
        int ret;
        common_c.sizebuf_t old;
        byte* olddata = null;

        if (server_c.sv.active)
        {
            return;
        }

        if (cl_main_c.cls.demoplayback)
        {
            return;
        }

        old = net_main_c.net_message;
        memcpy_c.memcpy((object*)olddata, (object*)net_main_c.net_message.data, net_main_c.net_message.cursize);

        do
        {
            ret = cl_demo_c.CL_GetMessage();

            switch (ret)
            {
                default:
                    host_c.Host_Error("CL_KeepaliveMessage: CL_GetMessage failed");
                    break;

                case 0:
                    break;

                case 1:
                    host_c.Host_Error("CL_KeepaliveMessage: received a message");
                    break;

                case 2:
                    if (common_c.MSG_ReadByte() != protocol_c.svc_nop)
                    {
                        host_c.Host_Error("CL_KeepaliveMessage: datagram wasn't a nop");
                    }
                    break;
            }
        } while (ret != null);

        net_c.net_message = old;
        memcpy_c.memcpy((object*)net_c.net_message.data, (object*)olddata, net_c.net_message.cursize);

        time = (float)sys_win_c.Sys_FloatTime();

        if (time - lastmsg < 5)
        {
            return;
        }

        lastmsg = time;

        console_c.Con_Printf("--> client to server keepalive\n");

        common_c.MSG_WriteByte(cl_main_c.cls.message, protocol_c.clc_nop);
        net_main_c.NET_SendMessage(cl_main_c.cls.netcon, cl_main_c.cls.message);
        common_c.SZ_Clear(cl_main_c.cls.message);
    }

    public static void CL_ParseServerInfo()
    {
        char* str;
        int i;
        int nummodels, numsounds;
        char* model_precache = null;
        char[][] sound_precache = new char[quakedef_c.MAX_SOUNDS][];

        console_c.Con_DPrintf("Serverinfo packet received.\n");

        cl_main_c.CL_ClearState();

        i = common_c.MSG_ReadLong();

        if (i != protocol_c.PROTOCOL_VERSION)
        {
            console_c.Con_Printf($"Server returned version {i}, not {protocol_c.PROTOCOL_VERSION}");
            return;
        }

        cl_main_c.cl.maxclients = common_c.MSG_ReadByte();

        if (cl_main_c.cl.maxclients < 1 || cl_main_c.cl.maxclients > quakedef_c.MAX_SCOREBOARD)
        {
            console_c.Con_Printf($"Bad maxclients ({cl_main_c.cl.maxclients}) from server\n");
            return;
        }

        cl_main_c.cl.scores = zone_c.Hunk_AllocName(cl_main_c.cl.maxclients * sizeof(cl_main_c.cl.scores), "scores");

        cl_main_c.cl.gametype = common_c.MSG_ReadByte();

        str = common_c.MSG_ReadString();
        strncpy_c.strncpy(cl_main_c.cl.levelname, str, sizeof(cl_main_c.cl.levelname) - 1);

        //console_c.Con_Printf($"\n\n\35\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\37\n\n");
        console_c.Con_Printf($"2{*str}\n");

        memset_c.memset((object*)model_precache, 0, sizeof(char*));

        for (nummodels = 1; ; nummodels++)
        {
            str = common_c.MSG_ReadString();

            if (str[0] == null)
            {
                return;
            }

            if (nummodels == quakedef_c.MAX_MODELS)
            {
                console_c.Con_Printf("Server sent too many model precaches\n");
                return;
            }

            strcpy_c.strcpy(&model_precache[nummodels], str++);
            model_c.Mod_TouchModel(str);
        }

        memset_c.memset(cl_main_c.cl.sound_precache, 0, sizeof(cl_main_c.cl.sound_precache));

        for (numsounds = 1; ; numsounds++)
        {
            str = common_c.MSG_ReadString();

            if (str[0] == 0)
            {
                break;
            }

            if (numsounds == quakedef_c.MAX_SOUNDS)
            {
                console_c.Con_Printf("Server sent too many sound precaches\n");
                return;
            }
        }

        for (i = 1; i < nummodels; i++)
        {
            cl_main_c.cl.model_precache[i] = *model_c.Mod_ForName(&model_precache[i], false);

            if (cl_main_c.cl.model_precache[i] == null)
            {
                console_c.Con_Printf($"Model {model_precache[i]} not found\n");
                return;
            }

            CL_KeepaliveMessage();
        }

        snd_dma_c.S_BeginPrecaching();

        for (i = 1; i < numsounds; i++)
        {
            cl_main_c.cl.sound_precache[i] = snd_dma_c.S_PrecacheSound(sound_precache[i]);
            CL_KeepaliveMessage();
        }

        snd_dma_c.S_EndPrecaching();

        cl_main_c.cl_entities[0].model = cl_main_c.cl.worldmodel = &cl_main_c.cl.model_precache[1];

        r_main_c.R_NewMap();

        zone_c.Hunk_Check();

        host_cmd_c.noclip_anglehack = false;
    }

    public static int* bitcounts;

    public static void CL_ParseUpdate(int bits)
    {
        int i;
        model_c.model_t* model;
        int modnum;
        bool forcelink;
        render_c.entity_t* ent;
        int num;
        int skin;

        if (cl_main_c.cls.signon == client_c.SIGNONS - 1)
        {
            cl_main_c.cls.signon = client_c.SIGNONS;
            cl_main_c.CL_SignonReply();
        }

        if ((bits & protocol_c.U_MOREBITS) != 0)
        {
            i = common_c.MSG_ReadByte();
            bits |= (i << 8);
        }

        if ((bits & protocol_c.U_LONGENTITY) != 0)
        {
            num = common_c.MSG_ReadShort();
        }
        else
        {
            num = common_c.MSG_ReadByte();
        }

        ent = CL_EntityNum(num);

        for (i = 0; i < 16; i++)
        {
            if ((bits & (1 << i)) != 0)
            {
                bitcounts[i]++;

                if (ent->msgtime != cl_main_c.cl.mtime[1])
                {
                    forcelink = true;
                }
                else
                {
                    forcelink = false;
                }

                ent->msgtime = cl_main_c.cl.mtime[0];

                if ((bits & protocol_c.U_MODEL) != 0)
                {
                    modnum = common_c.MSG_ReadByte();

                    if (modnum >= quakedef_c.MAX_MODELS)
                    {
                        host_c.Host_Error("CL_ParseModel: bad modnum");
                    }
                }
                else
                {
                    modnum = ent->baseline.modelindex;
                }

                model = &cl_main_c.cl.model_precache[modnum];

                if (model != ent->model)
                {
                    ent->model = model;

                    if (model != null)
                    {
                        if (model->synctype == modelgen_c.synctype_t.ST_RAND)
                        {
                            ent->syncbase = (float)(rand_c.rand() & 0x7fff) / 0x7fff;
                        }
                        else
                        {
                            ent->syncbase = 0.0f;
                        }
                    }
                    else
                    {
                        forcelink = true;
                    }

#if GLQUAKE
                    if (num > 0 && num <= cl_main_c.cl.maxclients)
                    {
                        R_TranslatePlayerSkin(num-1);
                    }
#endif
                }

                if ((bits & protocol_c.U_FRAME) != 0)
                {
                    ent->frame = common_c.MSG_ReadByte();
                }
                else
                {
                    ent->frame = ent->baseline.frame;
                }

                if ((bits & protocol_c.U_COLORMAP) != 0)
                {
                    i = common_c.MSG_ReadByte();
                }
                else
                {
                    i = ent->baseline.colormap;
                }

                if (i == 0)
                {
                    ent->colormap = vid_c.vid.colormap;
                }
                else
                {
                    if (i > cl_main_c.cl.maxclients)
                    {
                        sys_win_c.Sys_Error("i >= cl_main_c.cl.maxclients");
                    }

                    ent->colormap = cl_main_c.cl.scores[i - 1].translations;
                }

#if GLQUAKE
                if ((bits & protocol_c.U_SKIN) != 0)
                {
                    skin = common_c.MSG_ReadByte();
                }
                else
                {
                    skin = ent->baseline.skin;
                }

                if (skin != ent->skinnum)
                {
                    ent->skinnum = skin;

                    if (num > 0 && num <= cl_main_c.cl.maxclients)
                    {
                        R_TranslatePlayerSkin(num - 1);
                    }
                }


#else
                if ((bits & protocol_c.U_SKIN) != 0)
                {
                    ent->skinnum = common_c.MSG_ReadByte();
                }
                else
                {
                    ent->skinnum = ent->baseline.skin;
                }

#endif

                if ((bits & protocol_c.U_EFFECTS) != 0)
                {
                    ent->effects = common_c.MSG_ReadByte();
                }
                else
                {
                    ent->effects = ent->baseline.effects;
                }

                mathlib_c.VectorCopy(ent->msg_origins[0], ent->msg_origins[1]);
                mathlib_c.VectorCopy(ent->msg_angles[0], ent->msg_angles[1]);

                if ((bits & protocol_c.U_ORIGIN1) != 0)
                {
                    ent->msg_origins[0][0] = common_c.MSG_ReadCoord();
                }
                else
                {
                    ent->msg_origins[0][0] = ent->baseline.origin[0];
                }

                if ((bits & protocol_c.U_ANGLE1) != 0)
                {
                    ent->msg_angles[0][0] = common_c.MSG_ReadAngle();
                }
                else
                {
                    ent->msg_angles[0][0] = ent->baseline.angles[0];
                }

                if ((bits & protocol_c.U_ORIGIN2) != 0)
                {
                    ent->msg_origins[0][1] = common_c.MSG_ReadCoord();
                }
                else
                {
                    ent->msg_origins[0][1] = ent->baseline.origin[1];
                }

                if ((bits & protocol_c.U_ANGLE2) != 0)
                {
                    ent->msg_angles[0][1] = common_c.MSG_ReadAngle();
                }
                else
                {
                    ent->msg_angles[0][1] = ent->baseline.angles[1];
                }

                if ((bits & protocol_c.U_ORIGIN3) != 0)
                {
                    ent->msg_origins[0][2] = common_c.MSG_ReadCoord();
                }
                else
                {
                    ent->msg_origins[0][2] = ent->baseline.origin[2];
                }

                if ((bits & protocol_c.U_ANGLE3) != 0)
                {
                    ent->msg_angles[0][2] = common_c.MSG_ReadAngle();
                }
                else
                {
                    ent->msg_angles[0][2] = ent->baseline.angles[2];
                }

                if ((bits & protocol_c.U_NOLERP) != 0)
                {
                    ent->forcelink = true;
                }

                if (forcelink)
                {
                    mathlib_c.VectorCopy(ent->msg_origins[0], ent->msg_origins[1]);
                    mathlib_c.VectorCopy(ent->msg_origins[0], ent->origin);
                    mathlib_c.VectorCopy(ent->msg_angles[0], ent->msg_angles[1]);
                    mathlib_c.VectorCopy(ent->msg_angles[0], ent->angles);
                    ent->forcelink = true;
                }
            }
        }
    }

    public static void CL_ParseBaseLine(render_c.entity_t* ent)
    {
        int i;

        ent->baseline.modelindex = common_c.MSG_ReadByte();
        ent->baseline.frame = common_c.MSG_ReadByte();
        ent->baseline.colormap = common_c.MSG_ReadByte();
        ent->baseline.skin = common_c.MSG_ReadByte();

        for (i = 0; i < 3; i++)
        {
            ent->baseline.origin[i] = common_c.MSG_ReadCoord();
            ent->baseline.angles[i] = common_c.MSG_ReadAngle();
        }
    }

    public static void CL_ParseClientdata(int bits)
    {
        int i, j;

        if ((bits & protocol_c.SU_VIEWHEIGHT) != 0)
        {
            cl_main_c.cl.viewheight = common_c.MSG_ReadChar();
        }
        else
        {
            cl_main_c.cl.viewheight = protocol_c.DEFAULT_VIEWHEIGHT;
        }

        if ((bits & protocol_c.SU_IDEALPITCH) != 0)
        {
            cl_main_c.cl.idealpitch = common_c.MSG_ReadChar();
        }
        else
        {
            cl_main_c.cl.idealpitch = 0;
        }

        mathlib_c.VectorCopy(cl_main_c.cl.mvelocity[0], cl_main_c.cl.mvelocity[1]);

        for (i = 0; i < 3; i++)
        {
            if ((bits & (protocol_c.SU_PUNCH1 << i)) != 0)
            {
                cl_main_c.cl.punchangle[i] = common_c.MSG_ReadChar();
            }
            else
            {
                cl_main_c.cl.punchangle[i] = 0;
            }

            if ((bits & (protocol_c.SU_VELOCITY1 << i)) != 0)
            {
                cl_main_c.cl.mvelocity[0][i] = common_c.MSG_ReadChar() * 16;
            }
            else
            {
                cl_main_c.cl.mvelocity[0][i] = 0;
            }
        }

        i = common_c.MSG_ReadLong();

        if (cl_main_c.cl.items != i)
        {
            sbar_c.Sbar_Changed();

            for (j = 0; j < 32; j++)
            {
                if ((i & (1 << j)) != 0 && (cl_main_c.cl.items & (1 << j)) != 0)
                {
                    cl_main_c.cl.item_gettime[j] = cl_main_c.cl.time;
                }
            }

            cl_main_c.cl.items = i;
        }

        cl_main_c.cl.onground = (bits & protocol_c.SU_ONGROUND) != 0;
        cl_main_c.cl.inwater = (bits & protocol_c.SU_INWATER) != 0;

        if ((bits & protocol_c.SU_WEAPONFRAME) != 0)
        {
            cl_main_c.cl.stats[quakedef_c.STAT_ACTIVEWEAPON] = common_c.MSG_ReadByte();
        }
        else
        {
            cl_main_c.cl.stats[quakedef_c.STAT_ACTIVEWEAPON] = 0;
        }

        if ((bits & protocol_c.SU_ARMOR) != 0)
        {
            i = common_c.MSG_ReadByte();
        }
        else
        {
            i = 0;
        }

        if (cl_main_c.cl.stats[quakedef_c.STAT_ARMOR] != i)
        {
            cl_main_c.cl.stats[quakedef_c.STAT_ARMOR] = i;
            sbar_c.Sbar_Changed();
        }

        if ((bits & protocol_c.SU_WEAPON) != 0)
        {
            i = common_c.MSG_ReadByte();
        }
        else
        {
            i = 0;
        }

        if (cl_main_c.cl.stats[quakedef_c.STAT_WEAPON] != i)
        {
            cl_main_c.cl.stats[quakedef_c.STAT_WEAPON] = i;
            sbar_c.Sbar_Changed();
        }

        i = common_c.MSG_ReadShort();

        if (cl_main_c.cl.stats[quakedef_c.STAT_HEALTH] != i)
        {
            cl_main_c.cl.stats[quakedef_c.STAT_HEALTH] = i;
            sbar_c.Sbar_Changed();
        }

        i = common_c.MSG_ReadByte();

        if (cl_main_c.cl.stats[quakedef_c.STAT_AMMO] != i)
        {
            cl_main_c.cl.stats[quakedef_c.STAT_AMMO] = i;
            sbar_c.Sbar_Changed();
        }

        for (i = 0; i < 4; i++)
        {
            j = common_c.MSG_ReadByte();

            if (cl_main_c.cl.stats[quakedef_c.STAT_SHELLS + i] != j)
            {
                cl_main_c.cl.stats[quakedef_c.STAT_SHELLS + i] = j;
                sbar_c.Sbar_Changed();
            }
        }

        i = common_c.MSG_ReadByte();

        if (common_c.standard_quake)
        {
            if (cl_main_c.cl.stats[quakedef_c.STAT_ACTIVEWEAPON] != i)
            {
                cl_main_c.cl.stats[quakedef_c.STAT_ACTIVEWEAPON] = i;
                sbar_c.Sbar_Changed();
            }
        }
        else
        {
            if (cl_main_c.cl.stats[quakedef_c.STAT_ACTIVEWEAPON] != (1 << i))
            {
                cl_main_c.cl.stats[quakedef_c.STAT_ACTIVEWEAPON] = (1 << i);
                sbar_c.Sbar_Changed();
            }
        }
    }

    public static void CL_NewTranslation(int slot)
    {
        int i, j;
        int top, bottom;
        byte* dest, source;

        if (slot > cl_main_c.cl.maxclients)
        {
            sys_win_c.Sys_Error("CL_NewTranslation: slot > cl_main_c.cl.maxclients");
        }

        dest = cl_main_c.cl.scores[slot].translations;
        source = vid_c.vid.colormap;
        memcpy_c.memcpy(dest, vid_c.vid.colormap, sizeof(cl_main_c.cl.scores[slot].translations));
        top = cl_main_c.cl.scores[slot].colors & 0xf0;
        bottom = (cl_main_c.cl.scores[slot].colors & 15) << 4;
#if GLQUAKE
        R_TranslatePlayerSkin(slot);
#endif

        for (i = 0; i < vid_c.VID_GRADES; i++, dest += 256, source += 256)
        {
            if (top < 128)
            {
                memcpy_c.memcpy((object*)(dest + render_c.TOP_RANGE), (object*)(source + top), 16);
            }
            else
            {
                for (j = 0; j < 16; j++)
                {
                    dest[render_c.TOP_RANGE + j] = source[top + 15 - j];
                }
            }
        }
    }
}