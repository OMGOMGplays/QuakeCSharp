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
            console_c.Con_Printf(common_c.StringToChar($"SV_StartSound: {sample->ToString()} not precached\n"));
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

        common_c.MSG_WriteString(client->message, *message);

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

        console_c.Con_DPrintf($"Client {client->netconnection->address->ToString()} connected\n");

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

            for (i = 0; i < svs.maxclients; i++)
            {
                if (!svs.clients[i].active)
                {
                    break;
                }
            }

            if (i == svs.maxclients)
            {
                sys_win_c.Sys_Error(common_c.StringToChar("Host_CheckForNewClients: no free clients"));
            }

            svs.clients[i].netconnection = ret;
            SV_ConnectClient(i);

            net_main_c.net_activeconnections++;
        }
    }

    public static void SV_ClearDatagram()
    {
        common_c.SZ_Clear(sv.datagram);
    }

    private static int fatbytes;
    private static byte* fatpvs;

    public static void SV_AddToFatPVS(Vector3 org, model_c.mnode_t* node)
    {
        int i;
        byte* pvs;
        model_c.mplane_t* plane;
        float d;

        while (true)
        {
            if (node->contents < 0)
            {
                if (node->contents != bspfile_c.CONTENTS_SOLID)
                {
                    pvs = model_c.Mod_LeafPVS((model_c.mleaf_t*)node, sv.worldmodel);

                    for (i = 0; i < fatbytes; i++)
                    {
                        fatpvs[i] |= pvs[i];
                    }
                }

                return;
            }

            plane = node->plane;

            d = mathlib_c.DotProduct_V(org, plane->normal) - plane->dist;

            if (d > 8)
            {
                node = &node->children[0];
            }
            else if (d < -8)
            {
                node = &node->children[1];
            }
            else
            {
                SV_AddToFatPVS(org, &node->children[0]);
                node = &node->children[1];
            }
        }
    }

    public static byte* SV_FatPVS(Vector3 org)
    {
        fatbytes = (sv.worldmodel->numleafs + 31) >> 3;
        common_c.Q_memset(*fatpvs, 0, fatbytes);
        SV_AddToFatPVS(org, sv.worldmodel->nodes);
        return fatpvs;
    }

    public static void SV_WriteEntitiesToClient(progs_c.edict_t* clent, common_c.sizebuf_t* msg)
    {
        int e, i;
        int bits;
        byte* pvs;
        Vector3 org;
        float miss;
        progs_c.edict_t* ent;

        mathlib_c.VectorAdd(clent->v.origin, clent->v.view_ofs, org);
        pvs = SV_FatPVS(org);

        ent = progs_c.NEXT_EDICT(sv.edicts);

        for (e = 1; e < sv.num_edicts; e++, ent = progs_c.NEXT_EDICT(ent))
        {
#if QUAKE2
            if (ent->v.effects == server_c.EF_NODRAW) 
            {
                continue;
            }
#endif

            if (ent != clent)
            {
                if (ent->v.modelindex == 0 || pr_edict_c.pr_strings[ent->v.model] == null)
                {
                    continue;
                }

                for (i = 0; i < ent->num_leafs; i++)
                {
                    if ((pvs[ent->leafnums[i] >> 3] & (1 << (ent->leafnums[i] & 7))) != 0)
                    {
                        break;
                    }
                }

                if (i == ent->num_leafs)
                {
                    continue;
                }
            }

            if (msg->maxsize - msg->cursize < 16)
            {
                console_c.Con_Printf(common_c.StringToChar("packet overflow\n"));
                return;
            }

            bits = 0;

            for (i = 0; i < 3; i++)
            {
                miss = ent->v.origin[i] - ent->baseline.origin[i];

                if (miss < -0.1f || miss > 0.1f)
                {
                    bits |= protocol_c.U_ORIGIN1 << i;
                }
            }

            if (ent->v.angles[0] != ent->baseline.angles[0])
            {
                bits |= protocol_c.U_ANGLE1;
            }

            if (ent->v.angles[1] != ent->baseline.angles[1])
            {
                bits |= protocol_c.U_ANGLE2;
            }

            if (ent->v.angles[2] != ent->baseline.angles[2])
            {
                bits |= protocol_c.U_ANGLE3;
            }

            if (ent->v.movetype == server_c.MOVETYPE_STEP)
            {
                bits |= protocol_c.U_NOLERP;
            }

            if (ent->baseline.colormap != ent->v.colormap)
            {
                bits |= protocol_c.U_COLORMAP;
            }

            if (ent->baseline.skin != ent->v.skin)
            {
                bits |= protocol_c.U_SKIN;
            }

            if (ent->baseline.frame != ent->v.frame)
            {
                bits |= protocol_c.U_FRAME;
            }

            if (ent->baseline.effects != ent->v.effects)
            {
                bits |= protocol_c.U_EFFECTS;
            }

            if (ent->baseline.modelindex != ent->v.modelindex)
            {
                bits |= protocol_c.U_MODEL;
            }

            if (e >= 256)
            {
                bits |= protocol_c.U_LONGENTITY;
            }

            if (bits >= 256)
            {
                bits |= protocol_c.U_MOREBITS;
            }

            common_c.MSG_WriteByte(*msg, bits | protocol_c.U_SIGNAL);

            if ((bits & protocol_c.U_MOREBITS) != 0)
            {
                common_c.MSG_WriteByte(*msg, bits >> 8);
            }

            if ((bits & protocol_c.U_LONGENTITY) != 0)
            {
                common_c.MSG_WriteShort(*msg, e);
            }
            else
            {
                common_c.MSG_WriteByte(*msg, e);
            }

            if ((bits & protocol_c.U_MODEL) != 0)
            {
                common_c.MSG_WriteByte(*msg, (int)ent->v.modelindex);
            }

            if ((bits & protocol_c.U_FRAME) != 0)
            {
                common_c.MSG_WriteByte(*msg, (int)ent->v.colormap);
            }

            if ((bits & protocol_c.U_COLORMAP) != 0)
            {
                common_c.MSG_WriteByte(*msg, (int)ent->v.colormap);
            }

            if ((bits & protocol_c.U_SKIN) != 0)
            {
                common_c.MSG_WriteByte(*msg, (int)ent->v.skin);
            }

            if ((bits & protocol_c.U_EFFECTS) != 0)
            {
                common_c.MSG_WriteByte(*msg, (int)ent->v.effects);
            }

            if ((bits & protocol_c.U_ORIGIN1) != 0)
            {
                common_c.MSG_WriteCoord(*msg, ent->v.origin[0]);
            }

            if ((bits & protocol_c.U_ANGLE1) != 0)
            {
                common_c.MSG_WriteAngle(*msg, ent->v.angles[0]);
            }

            if ((bits & protocol_c.U_ORIGIN2) != 0)
            {
                common_c.MSG_WriteCoord(*msg, ent->v.origin[1]);
            }

            if ((bits & protocol_c.U_ANGLE2) != 0)
            {
                common_c.MSG_WriteAngle(*msg, ent->v.angles[1]);
            }

            if ((bits & protocol_c.U_ORIGIN3) != 0)
            {
                common_c.MSG_WriteCoord(*msg, ent->v.origin[2]);
            }

            if ((bits & protocol_c.U_ANGLE3) != 0)
            {
                common_c.MSG_WriteAngle(*msg, ent->v.angles[2]);
            }
        }
    }

    public static void SV_CleanupEnts()
    {
        int e;
        progs_c.edict_t* ent;

        ent = progs_c.NEXT_EDICT(sv.edicts);

        for (e = 1; e < sv.num_edicts; e++, ent = progs_c.NEXT_EDICT(ent))
        {
            ent->v.effects = (int)ent->v.effects & ~server_c.EF_MUZZLEFLASH;
        }
    }

    public static void SV_WriteClientdataToMessage(progs_c.edict_t* ent, common_c.sizebuf_t* msg)
    {
        int bits;
        int i;
        progs_c.edict_t* other;
        int items;
#if !QUAKE2
        progs_c.eval_t* val;
#endif

        if (ent->v.dmg_take != 0 || ent->v.dmg_save != 0)
        {
            other = progs_c.PROG_TO_EDICT(ent->v.dmg_inflictor);
            common_c.MSG_WriteByte(*msg, protocol_c.svc_damage);
            common_c.MSG_WriteByte(*msg, (int)ent->v.dmg_save);
            common_c.MSG_WriteByte(*msg, (int)ent->v.dmg_take);

            for (i = 0; i < 3; i++)
            {
                common_c.MSG_WriteCoord(*msg, other->v.origin[i] + 0.5f * (other->v.mins[i] + other->v.maxs[i]));
            }

            ent->v.dmg_take = 0;
            ent->v.dmg_save = 0;
        }

        sv_user_c.SV_SetIdealPitch();

        if (ent->v.fixangle != 0)
        {
            common_c.MSG_WriteByte(*msg, protocol_c.svc_setangle);

            for (i = 0; i < 3; i++)
            {
                common_c.MSG_WriteAngle(*msg, ent->v.angles[i]);
            }

            ent->v.fixangle = 0;
        }

        bits = 0;

        if (ent->v.view_ofs[2] != protocol_c.DEFAULT_VIEWHEIGHT)
        {
            bits |= protocol_c.SU_VIEWHEIGHT;
        }

        if (ent->v.idealpitch != 0)
        {
            bits |= protocol_c.SU_IDEALPITCH;
        }

#if QUAKE2
        items = (int)ent->v.items | ((int)ent->v.items2 << 23);
#else
        val = pr_edict_c.GetEdictFieldValue(ent, common_c.StringToChar("items2"));

        if (val != null)
        {
            items = (int)ent->v.items | ((int)val->_float << 23);
        }
        else
        {
            items = (int)ent->v.items | ((int)progs_c.pr_global_struct->serverflags << 28);
        }
#endif

        if (ent->v.dmg_take != 0 || ent->v.dmg_save != 0)
        {
            other = progs_c.PROG_TO_EDICT(ent->v.dmg_inflictor);
            common_c.MSG_WriteByte(*msg, protocol_c.svc_damage);
            common_c.MSG_WriteByte(*msg, (int)ent->v.dmg_save);
            common_c.MSG_WriteByte(*msg, (int)ent->v.dmg_take);

            for (i = 0; i < 3; i++)
            {
                common_c.MSG_WriteCoord(*msg, other->v.origin[i] + 0.5f * (other->v.mins[i] + other->v.maxs[i]));
            }

            ent->v.dmg_take = 0;
            ent->v.dmg_save = 0;
        }

        sv_user_c.SV_SetIdealPitch();

        if (ent->v.fixangle != 0)
        {
            common_c.MSG_WriteByte(*msg, protocol_c.svc_setangle);

            for (i = 0; i < 3; i++)
            {
                common_c.MSG_WriteAngle(*msg, ent->v.angles[i]);
            }

            ent->v.fixangle = 0;
        }

        bits = 0;

        if (ent->v.view_ofs[2] != protocol_c.DEFAULT_VIEWHEIGHT)
        {
            bits |= protocol_c.SU_VIEWHEIGHT;
        }

        if (ent->v.idealpitch != 0)
        {
            bits |= protocol_c.SU_IDEALPITCH;
        }

#if QUAKE2
        items = (int)ent->v.items | ((int)ent->v.items2 << 23);
#else
        val = pr_edict_c.GetEdictFieldValue(ent, common_c.StringToChar("items2"));

        if (val != null)
        {
            items = (int)ent->v.items | ((int)val->_float << 23);
        }
        else
        {
            items = (int)ent->v.items | ((int)progs_c.pr_global_struct->serverflags << 28);
        }
#endif

        bits |= protocol_c.SU_ITEMS;

        if (((int)ent->v.flags & server_c.FL_ONGROUND) != 0)
        {
            bits |= protocol_c.SU_ONGROUND;
        }

        if (ent->v.waterlevel >= 2)
        {
            bits |= protocol_c.SU_INWATER;
        }

        for (i = 0; i < 3; i++)
        {
            if (ent->v.punchangle[i] != 0)
            {
                bits |= (protocol_c.SU_PUNCH1 << i);
            }

            if (ent->v.velocity[i] != 0)
            {
                bits |= (protocol_c.SU_VELOCITY1 << i);
            }
        }

        if (ent->v.weaponframe != 0)
        {
            bits |= protocol_c.SU_WEAPONFRAME;
        }

        if (ent->v.armorvalue != 0)
        {
            bits |= protocol_c.SU_ARMOR;
        }

        bits |= protocol_c.SU_WEAPON;

        common_c.MSG_WriteByte(*msg, protocol_c.svc_clientdata);
        common_c.MSG_WriteShort(*msg, bits);

        if ((bits & protocol_c.SU_VIEWHEIGHT) != 0)
        {
            common_c.MSG_WriteChar(*msg, (int)ent->v.view_ofs[2]);
        }

        if ((bits & protocol_c.SU_IDEALPITCH) != 0)
        {
            common_c.MSG_WriteChar(*msg, (int)ent->v.idealpitch);
        }

        for (i = 0; i < 3; i++)
        {
            if ((bits & (protocol_c.SU_PUNCH1 << i)) != 0)
            {
                common_c.MSG_WriteChar(*msg, (int)ent->v.punchangle[i]);
            }

            if ((bits & (protocol_c.SU_VELOCITY1 << i)) != 0)
            {
                common_c.MSG_WriteChar(*msg, (int)ent->v.velocity[i] / 16);
            }
        }

        common_c.MSG_WriteLong(*msg, items);

        if ((bits & protocol_c.SU_WEAPONFRAME) != 0)
        {
            common_c.MSG_WriteByte(*msg, (int)ent->v.weaponframe);
        }

        if ((bits & protocol_c.SU_ARMOR) != 0)
        {
            common_c.MSG_WriteByte(*msg, (int)ent->v.armorvalue);
        }

        if ((bits & protocol_c.SU_WEAPON) != 0)
        {
            common_c.MSG_WriteByte(*msg, SV_ModelIndex(*progs_c.pr_strings + common_c.StringToChar(ent->v.weaponmodel)));
        }

        common_c.MSG_WriteShort(*msg, (int)ent->v.health);
        common_c.MSG_WriteByte(*msg, (int)ent->v.currentammo);
        common_c.MSG_WriteByte(*msg, (int)ent->v.ammo_shells);
        common_c.MSG_WriteByte(*msg, (int)ent->v.ammo_nails);
        common_c.MSG_WriteByte(*msg, (int)ent->v.ammo_rockets);
        common_c.MSG_WriteByte(*msg, (int)ent->v.ammo_cells);

        if (common_c.standard_quake)
        {
            common_c.MSG_WriteByte(*msg, (int)ent->v.weapon);
        }
        else
        {
            for (i = 0; i < 32; i++)
            {
                if (((int)ent->v.weapon & (1 << i)) != 0)
                {
                    common_c.MSG_WriteByte(*msg, i);
                    break;
                }
            }
        }
    }

    public static bool SV_SendClientDatagram(server_c.client_t* client)
    {
        byte* buf = 0;
        common_c.sizebuf_t msg = new();

        msg.data = (int*)buf;
        msg.maxsize = quakedef_c.MAX_DATAGRAM;
        msg.cursize = 0;

        common_c.MSG_WriteByte(msg, protocol_c.svc_time);
        common_c.MSG_WriteFloat(msg, (float)sv.time);

        SV_WriteClientdataToMessage(client->edict, &msg);

        SV_WriteEntitiesToClient(client->edict, &msg);

        if (msg.cursize + sv.datagram.cursize < msg.maxsize)
        {
            common_c.SZ_Write(msg, *sv.datagram.data, sv.datagram.cursize);
        }

        if (net_main_c.NET_SendUnreliableMessage(client->netconnection, &msg) == -1)
        {
            Server.sv_main_c.SV_DropClient((Server.server_c.client_t*)client);
            return false;
        }

        return true;
    }

    public static void SV_UpdateToReliableMessages()
    {
        int i, j;
        server_c.client_t* client;

        for (i = 0, server_c.host_client = svs.clients; i < svs.maxclients; i++, server_c.host_client++)
        {
            if (server_c.host_client->old_frags != server_c.host_client->edict->v.frags)
            {
                for (j = 0, client = svs.clients; j < svs.maxclients; j++, client++)
                {
                    if (!client->active)
                    {
                        continue;
                    }

                    common_c.MSG_WriteByte(client->message, protocol_c.svc_updatefrags);
                    common_c.MSG_WriteByte(client->message, i);
                    common_c.MSG_WriteShort(client->message, (int)server_c.host_client->edict->v.frags);
                }

                server_c.host_client->old_frags = (int)server_c.host_client->edict->v.frags;
            }
        }

        for (j = 0, client = svs.clients; j < svs.maxclients; j++, client++)
        {
            if (!client->active)
            {
                continue;
            }

            common_c.SZ_Write(client->message, *sv.reliable_datagram.data, sv.reliable_datagram.cursize);
        }

        common_c.SZ_Clear(sv.reliable_datagram);
    }

    public static void SV_SendNop(server_c.client_t* client)
    {
        common_c.sizebuf_t msg = new();
        byte* buf = null;

        msg.data = (int*)buf;
        msg.maxsize = 4;
        msg.cursize = 0;

        common_c.MSG_WriteChar(msg, protocol_c.svc_nop);

        if (net_main_c.NET_SendUnreliableMessage(client->netconnection, &msg) == -1)
        {
            Server.sv_main_c.SV_DropClient((Server.server_c.client_t*)client);
        }

        client->last_message = quakedef_c.realtime;
    }

    public static void SV_SendClientMessages()
    {
        int i;

        SV_UpdateToReliableMessages();

        for (i = 0, server_c.host_client = svs.clients; i < svs.maxclients; i++, server_c.host_client++)
        {
            if (!server_c.host_client->active)
            {
                continue;
            }

            if (server_c.host_client->spawned)
            {
                if (!SV_SendClientDatagram(server_c.host_client))
                {
                    continue;
                }
            }
            else
            {
                if (!server_c.host_client->sendsignon)
                {
                    if (quakedef_c.realtime - server_c.host_client->last_message > 5)
                    {
                        SV_SendNop(server_c.host_client);
                    }

                    continue;
                }
            }

            if (server_c.host_client->message.overflowed)
            {
                Server.sv_main_c.SV_DropClient((Server.server_c.client_t*)server_c.host_client);
                server_c.host_client->message.overflowed = false;
                continue;
            }

            if (server_c.host_client->message.cursize != 0 || server_c.host_client->dropasap)
            {
                if (!net_main_c.NET_CanSendMessage(server_c.host_client->netconnection))
                {
                    continue;
                }

                if (server_c.host_client->dropasap)
                {
                    Server.sv_main_c.SV_DropClient(null);
                }
                else
                {
                    if (net_main_c.NET_SendMessage(server_c.host_client->netconnection, &server_c.host_client->message) == -1)
                    {
                        Server.sv_main_c.SV_DropClient((Server.server_c.client_t*)server_c.host_client);
                    }

                    common_c.SZ_Clear(server_c.host_client->message);
                    server_c.host_client->last_message = quakedef_c.realtime;
                    server_c.host_client->sendsignon = false;
                }
            }
        }

        SV_CleanupEnts();
    }

    public static int SV_ModelIndex(char* name)
    {
        int i;

        if (name == null || name[0] == 0)
        {
            return 0;
        }

        for (i = 0; i < quakedef_c.MAX_MODELS && sv.model_precache[i] != 0; i++)
        {
            if (!common_c.Q_strcmp(sv.model_precache[i].ToString(), name->ToString()))
            {
                return i;
            }
        }

        if (i == quakedef_c.MAX_MODELS || sv.model_precache[i] == 0)
        {
            sys_win_c.Sys_Error(common_c.StringToChar("SV_ModelIndex: model {name} not precached"));
        }

        return i;
    }

    public static void SV_CreateBaseline()
    {
        int i;
        progs_c.edict_t* svent;
        int entnum;

        for (entnum = 0; entnum < sv.num_edicts; entnum++)
        {
            svent = progs_c.EDICT_NUM(entnum);

            if (svent->free)
            {
                continue;
            }

            if (entnum > svs.maxclients && svent->v.modelindex == 0)
            {
                continue;
            }

            mathlib_c.VectorCopy(svent->v.origin, svent->baseline.origin);
            mathlib_c.VectorCopy(svent->v.angles, svent->baseline.angles);
            svent->baseline.frame = svent->v.frame;
            svent->baseline.skin = svent->v.skin;

            if (entnum > 0 && entnum <= svs.maxclients)
            {
                svent->baseline.colormap = entnum;
                svent->baseline.modelindex = SV_ModelIndex(common_c.StringToChar("progs/player.mdl"));
            }
            else
            {
                svent->baseline.colormap = 0;
                svent->baseline.modelindex = SV_ModelIndex(common_c.StringToChar(progs_c.pr_strings->ToString() + svent->v.model));
            }

            common_c.MSG_WriteByte(sv.signon, protocol_c.svc_spawnbaseline);
            common_c.MSG_WriteShort(sv.signon, entnum);

            common_c.MSG_WriteByte(sv.signon, svent->baseline.modelindex);
            common_c.MSG_WriteByte(sv.signon, svent->baseline.frame);
            common_c.MSG_WriteByte(sv.signon, svent->baseline.colormap);
            common_c.MSG_WriteByte(sv.signon, svent->baseline.skin);

            for (i = 0; i < 3; i++)
            {
                common_c.MSG_WriteCoord(sv.signon, svent->baseline.origin[i]);
                common_c.MSG_WriteAngle(sv.signon, svent->baseline.angles[i]);
            }
        }
    }

    public static void SV_SendReconnect()
    {
        char* data = null;
        common_c.sizebuf_t msg = new();

        msg.data = (int*)data;
        msg.cursize = 0;
        msg.maxsize = 128;

        common_c.MSG_WriteChar(msg, protocol_c.svc_stufftext);
        common_c.MSG_WriteString(msg, *common_c.StringToChar("reconnect\n"));
        net_main_c.NET_SendToAll(msg, 5);

        if (client_c.cls.state != client_c.cactive_t.ca_dedicated)
        {
#if QUAKE2
            cmd_c.Cbuf_InsertText("reconnect\n");
#else
            cmd_c.Cmd_ExecuteString("reconnect\n", cmd_c.src_command);
        }
    }

    public static void SV_SaveSpawnParms()
    {
        int i, j;

        svs.serverflags = (int)progs_c.pr_global_struct->serverflags;

        for (i = 0, server_c.host_client = svs.clients; i < svs.maxclients; i++, server_c.host_client++)
        {
            if (!server_c.host_client->active)
            {
                continue;
            }

            progs_c.pr_global_struct->self = progs_c.EDICT_TO_PROG(server_c.host_client->edict);
            pr_exec_c.PR_ExecuteProgram(progs_c.pr_global_struct->SetChangeParms);

            for (j = 0; j < server_c.NUM_SPAWN_PARMS; j++)
            {
                server_c.host_client->spawn_parms[j] = (&progs_c.pr_global_struct->parm1)[j];
            }
        }
    }

    public static float scr_centertime_off;

#if QUAKE2
    public static void SV_SpawnServer(char* server, char* startspot)
#else
    public static void SV_SpawnServer(char* server)
#endif
    {
        progs_c.edict_t* ent;
        int i;

        if (net_c.hostname.str[0] == 0)
        {
            cvar_c.Cvar_Set("hostname", "UNNAMED");
        }

        scr_centertime_off = 0;

        console_c.Con_DPrintf($"SpawnServer: {server->ToString()}\n");
        svs.changelevel_issued = false;

        if (sv.active)
        {
            SV_SendReconnect();
        }

        if (host_c.coop.value != 0)
        {
            cvar_c.Cvar_SetValue("deathmatch", 0);
        }

        host_cmd_c.current_skill = (int)(host_c.skill.value + 0.5f);

        if (host_cmd_c.current_skill < 0)
        {
            host_cmd_c.current_skill = 0;
        }

        if (host_cmd_c.current_skill > 3)
        {
            host_cmd_c.current_skill = 3;
        }

        cvar_c.Cvar_SetValue("skill", (float)host_cmd_c.current_skill);

        host_c.Host_ClearMemory();

        common_c.Q_memset(sv, 0, sizeof(server_c.server_t));

        common_c.Q_strcpy(sv.name->ToString(), server->ToString());

#if QUAKE2
        if (startspot != null) 
        {
            common_c.Q_strcpy(sv.startspot, startspot);
        }
#endif

        pr_edict_c.PR_LoadProgs();

        sv.max_edicts = quakedef_c.MAX_EDICTS;

        sv.edicts = (progs_c.edict_t*)zone_c.Hunk_AllocName(sv.max_edicts * progs_c.pr_edict_size, "edicts");

        sv.datagram.maxsize = sizeof(byte*);
        sv.datagram.cursize = 0;
        sv.datagram.data = (int*)sv.datagram_buf;

        sv.reliable_datagram.maxsize = sizeof(byte*);
        sv.reliable_datagram.cursize = 0;
        sv.reliable_datagram.data = (int*)sv.reliable_datagram_buf;

        sv.signon.maxsize = sizeof(byte*);
        sv.signon.cursize = 0;
        sv.signon.data = (int*)sv.signon_buf;

        sv.num_edicts = svs.maxclients + 1;

        for (i = 0; i < svs.maxclients; i++)
        {
            ent = progs_c.EDICT_NUM(i + 1);
            svs.clients[i].edict = ent;
        }

        sv.state = server_c.server_state_t.ss_loading;
        sv.paused = false;

        sv.time = 1.0;

        common_c.Q_strcpy(sv.name->ToString(), server->ToString());
        Console.WriteLine(sv.modelname->ToString(), $"maps/{server->ToString()}.bsp");
        sv.worldmodel = model_c.Mod_ForName(sv.modelname, false);

        if (sv.worldmodel == null)
        {
            console_c.Con_Printf(common_c.StringToChar($"Couldn't spawn server {sv.modelname->ToString()}\n"));
            sv.active = false;
            return;
        }

        sv.models[1] = *sv.worldmodel;

        world_c.SV_ClearWorld();

        sv.sound_precache[0] = *progs_c.pr_strings;

        sv.model_precache[0] = *progs_c.pr_strings;
        sv.model_precache[1] = *sv.modelname;

        for (i = 1; i < sv.worldmodel->numsubmodels; i++)
        {
            sv.model_precache[i + 1] = localmodels[i][0];
            sv.models[i + 1] = *model_c.Mod_ForName(common_c.StringToChar(localmodels[i][0].ToString()), false);
        }

        ent = progs_c.EDICT_NUM(0);
        common_c.Q_memset(ent->v, 0, progs_c.progs->entityfields * 4);
        ent->free = false;
        ent->v.model = (char*)(sv.worldmodel->name - progs_c.pr_strings);
        ent->v.modelindex = 1;
        ent->v.solid = server_c.SOLID_BSP;
        ent->v.movetype = server_c.MOVETYPE_PUSH;

        if (host_c.coop.value != 0)
        {
            progs_c.pr_global_struct->coop = host_c.coop.value;
        }
        else
        {
            progs_c.pr_global_struct->deathmatch = host_c.deathmatch.value;
        }

        progs_c.pr_global_struct->mapname = (char*)(sv.name - progs_c.pr_strings);
#if QUAKE2
        progs_c.pr_global_struct->startspot = sv.startspot - progs_c.pr_strings;
#endif

        progs_c.pr_global_struct->serverflags = svs.serverflags;

        pr_edict_c.ED_LoadFromFile(sv.worldmodel->entities);

        sv.active = true;

        sv.state = server_c.server_state_t.ss_active;

        host_c.host_frametime;
        sv_phys_c.SV_Physics();
        sv_phys_c.SV_Physics();

        SV_CreateBaseline();

        for (i = 0, server_c.host_client = svs.clients; i < svs.maxclients; i++, server_c.host_client++)
        {
            if (server_c.host_client->active)
            {
                SV_SendServerInfo(server_c.host_client);
            }
        }

        console_c.Con_DPrintf("Server spawned.\n");
    }
#endif
}