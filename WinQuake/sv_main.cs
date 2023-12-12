namespace Quake;

public unsafe class sv_main_c
{
    public static server_c.server_t sv;
    public static server_c.server_static_t svs;

    public static char[][] localmodels = new char[quakedef_c.MAX_MODELS][];

    public static void SV_Init()
    {
        int i;
        cvar_c.cvar_t sv_maxvelocity;
        cvar_c.cvar_t sv_gravity;
        cvar_c.cvar_t sv_nostep;
        cvar_c.cvar_t sv_friction;
        cvar_c.cvar_t sv_edgefriction;
        cvar_c.cvar_t sv_stopspeed;
        cvar_c.cvar_t sv_maxspeed;
        cvar_c.cvar_t sv_accelerate;
        cvar_c.cvar_t sv_idealpitchscale;
        cvar_c.cvar_t sv_aim;

        cvar_c.Cvar_RegisterVariable(&sv_maxvelocity);
        cvar_c.Cvar_RegisterVariable(&sv_gravity);
        cvar_c.Cvar_RegisterVariable(&sv_nostep);
        cvar_c.Cvar_RegisterVariable(&sv_friction);
        cvar_c.Cvar_RegisterVariable(&sv_edgefriction);
        cvar_c.Cvar_RegisterVariable(&sv_stopspeed);
        cvar_c.Cvar_RegisterVariable(&sv_maxspeed);
        cvar_c.Cvar_RegisterVariable(&sv_accelerate);
        cvar_c.Cvar_RegisterVariable(&sv_idealpitchscale);
        cvar_c.Cvar_RegisterVariable(&sv_aim);

        for (i = 0; i < quakedef_c.MAX_MODELS; i++)
        {
            Console.WriteLine($"{localmodels[i]} {i}");
        }
    }

    public static void SV_StartParticle(Vector3 org, Vector3 dir, int color, int count)
    {
        int i, v;

        if (sv.datagram.cursize > quakedef_c.MAX_DATAGRAM - 16)
        {
            return;
        }

        common_c.MSG_WriteByte(sv.datagram, protocol_c.svc_particle);
        common_c.MSG_WriteCoord(sv.datagram, org[0]);
        common_c.MSG_WriteCoord(sv.datagram, org[1]);
        common_c.MSG_WriteCoord(sv.datagram, org[2]);

        for (i = 0; i < 3; i++)
        {
            v = (int)(dir[i] * 16);

            if (v > 127)
            {
                v = 127;
            }
            else if (v < -128)
            {
                v = -128;
            }

            common_c.MSG_WriteChar(sv.datagram, v);
        }

        common_c.MSG_WriteByte(sv.datagram, count);
        common_c.MSG_WriteByte(sv.datagram, color);
    }

    public static void SV_StartSound(progs_c.edict_t* entity, int channel, char* sample, int volume, float attenuation)
    {
        int sound_num;
        int field_mask;
        int i;
        int ent;

        if (volume < 0 || volume > 255)
        {
            sys_win_c.Sys_Error(common_c.StringToChar($"SV_StartSound: volume = {volume}"));
        }

        if (attenuation < 0 || attenuation > 4)
        {
            sys_win_c.Sys_Error(common_c.StringToChar($"SV_StartSound: attenuation = {attenuation}"));
        }

        if (channel < 0 || channel > 7)
        {
            sys_win_c.Sys_Error(common_c.StringToChar($"SV_StartSound: channel = {channel}"));
        }

        if (sv.datagram.cursize > quakedef_c.MAX_DATAGRAM - 16)
        {
            return;
        }

        for (sound_num = 1; sound_num < quakedef_c.MAX_SOUNDS && sv.sound_precache[sound_num] != 0; sound_num++)
        {
            if (!common_c.Q_strcmp(sample->ToString(), sv.sound_precache[sound_num].ToString()))
            {
                break;
            }
        }

        if (sound_num == quakedef_c.MAX_SOUNDS || sv.sound_precache[sound_num] == 0)
        {
            console_c.Con_Printf($"SV_StartSound: {sample->ToString()} not precached\n");
            return;
        }

        ent = progs_c.NUM_FOR_EDICT(entity);

        channel = (ent << 3) | channel;

        field_mask = 0;

        if (volume != sound_c.DEFAULT_SOUND_PACKET_VOLUME)
        {
            field_mask |= protocol_c.SND_VOLUME;
        }

        if (attenuation != sound_c.DEFAULT_SOUND_PACKET_ATTENUATION)
        {
            field_mask |= protocol_c.SND_ATTENUATION;
        }

        common_c.MSG_WriteByte(sv.datagram, protocol_c.svc_sound);
        common_c.MSG_WriteByte(sv.datagram, field_mask);

        if ((field_mask & protocol_c.SND_VOLUME) != 0)
        {
            common_c.MSG_WriteByte(sv.datagram, volume);
        }

        if ((field_mask & protocol_c.SND_ATTENUATION) != 0)
        {
            common_c.MSG_WriteByte(sv.datagram, (int)(attenuation * 64));
        }

        common_c.MSG_WriteShort(sv.datagram, channel);
        common_c.MSG_WriteByte(sv.datagram, sound_num);

        for (i = 0; i < 3; i++)
        {
            common_c.MSG_WriteCoord(sv.datagram, entity->v.origin[i] + 0.5f * (entity->v.mins[i] + entity->v.maxs[i]));
        }
    }

    public static void SV_SendServerInfo(server_c.client_t* client)
    {
        char** s;
        char* message = null;

        common_c.MSG_WriteByte(client->message, protocol_c.svc_print);
        Console.WriteLine($"2\nVERSION {quakedef_c.VERSION} SERVER ({pr_edict_c.pr_crc} CRC)");
        common_c.MSG_WriteString(client->message, *message);

        common_c.MSG_WriteByte(client->message, protocol_c.svc_serverinfo);
        common_c.MSG_WriteLong(client->message, protocol_c.PROTOCOL_VERSION);
        common_c.MSG_WriteByte(client->message, svs.maxclients);

        if (host_c.coop.value == 0 && host_c.deathmatch.value != 0)
        {
            common_c.MSG_WriteByte(client->message, protocol_c.GAME_DEATHMATCH);
        }
        else
        {
            common_c.MSG_WriteByte(client->message, protocol_c.GAME_COOP);
        }

        Console.WriteLine(message, pr_edict_c.pr_strings + sv.edicts->v.message);

        common_c.MSG_WriteString(client->message, message);

        for (*s = sv.model_precache + 1; *s != null; s++)
        {
            common_c.MSG_WriteByte(client->message, **s);
        }

        common_c.MSG_WriteByte(client->message, 0);

        for (*s = sv.sound_precache + 1; *s != null; s++)
        {
            common_c.MSG_WriteString(client->message, **s);
        }

        common_c.MSG_WriteByte(client->message, 0);

        common_c.MSG_WriteByte(client->message, protocol_c.svc_cdtrack);
        common_c.MSG_WriteByte(client->message, (int)sv.edicts->v.sounds);
        common_c.MSG_WriteByte(client->message, (int)sv.edicts->v.sounds);

        common_c.MSG_WriteByte(client->message, protocol_c.svc_setview);
        common_c.MSG_WriteShort(client->message, progs_c.NUM_FOR_EDICT(client->edict));

        common_c.MSG_WriteByte(client->message, protocol_c.svc_signonnum);
        common_c.MSG_WriteByte(client->message, 1);

        client->sendsignon = true;
        client->spawned = false;
    }

    public static void SV_ConnectClient(int clientnum)
    {
        progs_c.edict_t* ent;
        server_c.client_t* client;
        int edictnum;
        net_c.qsocket_t* netconnection;
        int i;
        float* spawn_parms = null;

        client = svs.clients + clientnum;

        console_c.Con_DPrintf($"Client {client->netconnection->address} connected\n");

        edictnum = clientnum + 1;

        ent = progs_c.EDICT_NUM(edictnum);

        netconnection = client->netconnection;

        if (sv.loadgame)
        {
            common_c.Q_memcpy(*spawn_parms, *client->spawn_parms, sizeof(float));
        }

        common_c.Q_memset(*client, 0, sizeof(server_c.client_t));
        client->netconnection = netconnection;

        common_c.Q_strcpy(client->name->ToString(), "unconnected");
        client->active = true;
        client->spawned = false;
        client->edict = ent;
        client->message.data = (int*)client->msgbuf;
        client->message.maxsize = quakedef_c.MAX_MSGLEN;
        client->message.allowoverflow = true;

#if IDGODS
        client->privileged = IsID(client->netconnection->addr);
#else
        client->privileged = false;
#endif

        if (sv.loadgame)
        {
            common_c.Q_memcpy(*client->spawn_parms, *spawn_parms, sizeof(float*));
        }
        else
        {
            pr_exec_c.PR_ExecuteProgram(progs_c.pr_global_struct->SetNewParms);

            for (i = 0; i < server_c.NUM_SPAWN_PARMS; i++)
            {
                client->spawn_parms[i] = (&progs_c.pr_global_struct->parm1)[i];
            }
        }

        SV_SendServerInfo(client);
    }

    public static void SV_CheckForNewClients()
    {
        net_c.qsocket_t* ret;
        int i;

        while (true)
        {
            ret = net_main_c.NET_CheckNewConnections();

            if (ret == null)
            {
                break;
            }


        }
    }
}