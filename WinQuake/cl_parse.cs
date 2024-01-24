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
        char[][] model_precache = new char[quakedef_c.MAX_MODELS][];
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

        console_c.Con_Printf($"\n\n\35\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\36\37\n\n");
        console_c.Con_Printf($"2{*str}\n");
    }
}