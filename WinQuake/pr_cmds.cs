namespace Quake;

public unsafe class pr_cmds_c
{
    public static char* PF_VarString(int first)
    {
        int i;
        char* output = null;

        output[0] = '\0';

        for (i = first; i < progs_c.pr_argc; i++)
        {
            common_c.Q_strcat(output, progs_c.G_STRING((OFS_PARM0 + i * 3)));
        }

        return output;
    }

    public static void PF_error()
    {
        char* s;
        progs_c.edict_t* ed;

        s = PF_VarString(0);
        console_c.Con_Printf($"======SERVER ERROR in {*pr_edict_c.pr_strings + pr_exec_c.pr_xfunction->s_name}:\n{s->ToString()}\n");
        ed = progs_c.PROG_TO_EDICT(pr_edict_c.pr_global_struct->self);
        pr_edict_c.ED_Print(ed);

        host_c.Host_Error("Program error");
    }

    public static void PF_objerror()
    {
        char* s;
        progs_c.edict_t* ed;

        s = PF_VarString(0);
        console_c.Con_Printf($"======OBJECT ERROR in {*pr_edict_c.pr_strings + pr_exec_c.pr_xfunction->s_name}:\n{s->ToString()}\n");
        ed = progs_c.PROG_TO_EDICT(pr_edict_c.pr_global_struct->self);
        pr_edict_c.ED_Print(ed);
        pr_edict_c.ED_Free(ed);

        host_c.Host_Error("Program error");
    }

    public static void PF_makevectors()
    {
        mathlib_c.AngleVectors(progs_c.G_VECTOR(OFS_PARM0), pr_edict_c.pr_global_struct->v_forward, pr_edict_c.pr_global_struct->v_right, pr_edict_c.pr_global_struct->v_up);
    }

    public static void PF_setorigin()
    {
        progs_c.edict_t* e;
        float* org;

        e = progs_c.G_EDICT(OFS_PARM0);
        org = progs_c.G_VECTOR(OFS_PARM1);
        mathlib_c.VectorCopy(org, e->v.origin);
        world_c.SV_LinkEdict(e, false);
    }

    public static void SetMinMaxSize(progs_c.edict_t* e, float* min, float* max, bool rotate)
    {
        float* angles;
        Vector3 rmin, rmax;
        float[,] bounds = null;
        float* xvector = null, yvector = null;
        float a;
        Vector3 _base = new(), transformed = new();
        int i, j, k, l;

        rmin = rmax = new();

        for (i = 0; i < 3; i++)
        {
            if (min[i] > max[i])
            {
                pr_exec_c.PR_RunError("backwards mins/maxs");
            }
        }

        rotate = false;

        if (!rotate)
        {
            mathlib_c.VectorCopy(min, rmin);
            mathlib_c.VectorCopy(max, rmax);
        }
        else
        {
            mathlib_c.VectorCopy(e->v.angles, angles);

            a = angles[1] / 180 * MathF.PI;

            xvector[0] = MathF.Cos(a);
            xvector[1] = MathF.Sin(a);
            yvector[0] = -MathF.Sin(a);
            yvector[1] = MathF.Cos(a);

            mathlib_c.VectorCopy(min, bounds);

            rmin[0] = rmin[1] = rmin[2] = 9999;
            rmax[0] = rmax[1] = rmax[2] = -9999;

            for (i = 0; i <= 1; i++)
            {
                _base[0] = bounds[i, 0];

                for (j = 0; j <= 1; j++)
                {
                    _base[1] = bounds[j, 1];

                    for (k = 0; k <= 1; k++)
                    {
                        _base[2] = bounds[k, 2];

                        transformed[0] = xvector[0] * _base[0] + yvector[0] * _base[1];
                        transformed[1] = xvector[1] * _base[0] + yvector[1] * _base[1];
                        transformed[2] = _base[2];

                        for (l = 0; l < 3; l++)
                        {
                            if (transformed[1] < rmin[1])
                            {
                                rmin[1] = transformed[1];
                            }

                            if (transformed[1] > rmax[1])
                            {
                                rmax[1] = transformed[1];
                            }
                        }
                    }
                }
            }
        }

        mathlib_c.VectorCopy(rmin, e->v.mins);
        mathlib_c.VectorCopy(rmax, e->v.maxs);
        mathlib_c.VectorSubtract(max, min, e->v.size);

        world_c.SV_LinkEdict(e, false);
    }

    public static void SetMinMaxSize(progs_c.edict_t* e, Vector3 min, Vector3 max, bool rotate)
    {
        SetMinMaxSize(e, mathlib_c.VecToFloatPtr(min), mathlib_c.VecToFloatPtr(max), rotate);
    }

    public static void PF_setsize()
    {
        progs_c.edict_t* e;
        float* min, max;

        e = progs_c.G_EDICT(OFS_PARM0);
        min = progs_c.G_VECTOR(OFS_PARM1);
        max = progs_c.G_VECTOR(OFS_PARM2);
        SetMinMaxSize(e, min, max, false);
    }

    public static void PF_setmodel()
    {
        progs_c.edict_t* e;
        char* m;
        char** check = null;
        model_c.model_t* mod;
        int i;

        e = progs_c.G_EDICT(OFS_PARM0);
        m = progs_c.G_STRING(OFS_PARM1);

        for (i = 0, *check = server_c.sv.model_precache; *check != null; i++, check++)
        {
            if (!common_c.Q_strcmp(*check, m->ToString()))
            {
                break;
            }
        }

        if (*check == null)
        {
            pr_exec_c.PR_RunError($"no precache {*m}\n");
        }

        e->v.model = (char*)(m - pr_edict_c.pr_strings);
        e->v.modelindex = i;

        mod = &server_c.sv.models[(int)e->v.modelindex];

        if (mod != null)
        {
            SetMinMaxSize(e, mod->mins, mod->maxs, true);
        }
        else
        {
            SetMinMaxSize(e, mathlib_c.vec3_origin, mathlib_c.vec3_origin, true);
        }
    }

    public static void PF_bprint()
    {
        char* s;

        s = PF_VarString(0);
        sv_send_c.SV_BroadcastPrintf(s);
    }

    public static void PF_sprint()
    {
        char* s = null;
        server_c.client_t* client;
        int entnum;

        entnum = progs_c.G_EDICTNUM(OFS_PARM0);
        s = PF_VarString(1);

        if (entnum < 1 || entnum > server_c.svs.maxclients)
        {
            console_c.Con_Printf("tried to sprint to a non-client\n");
            return;
        }

        client = &server_c.svs.clients[entnum - 1];

        common_c.MSG_WriteChar(client->message, protocol_c.svc_print);
        common_c.MSG_WriteString(client->message, s);
    }

    public static void PF_centerprint()
    {
        char* s;
        server_c.client_t* client;
        int entnum;

        entnum = progs_c.G_EDICTNUM(OFS_PARM0);
        s = PF_VarString(1);

        if (entnum < 1 || entnum > server_c.svs.maxclients)
        {
            console_c.Con_Printf("tried to sprint to a non-client\n");
            return;
        }

        client = &server_c.svs.clients[entnum - 1];

        common_c.MSG_WriteChar(client->message, protocol_c.svc_centerprint);
        common_c.MSG_WriteString(client->message, s);
    }

    public static void PF_normalize()
    {
        float* value1;
        Vector3 newvalue = new();
        float _new;

        value1 = progs_c.G_VECTOR(OFS_PARM0);

        _new = value1[0] * value1[0] + value1[1] * value1[1] + value1[2] * value1[2];
        _new = (float)mathlib_c.sqrt(_new);

        if (_new == 0)
        {
            newvalue[0] = newvalue[1] = newvalue[2] = 0;
        }
        else
        {
            _new = 1 / _new;
            newvalue[0] = value1[0] * _new;
            newvalue[1] = value1[1] * _new;
            newvalue[2] = value1[2] * _new;
        }

        mathlib_c.VectorCopy(newvalue, progs_c.G_VECTOR(OFS_RETURN));
    }

    public static void PF_vlen()
    {
        float* value1;
        float _new;

        value1 = progs_c.G_VECTOR(OFS_PARM0);

        _new = value1[0] * value1[0] + value1[1] * value1[1] + value1[2] * value1[2];
        _new = (float)mathlib_c.sqrt(_new);

        progs_c.G_FLOAT(OFS_RETURN) = _new;
    }

    public static void PF_vectoyaw()
    {
        float* value1;
        float yaw;

        value1 = progs_c.G_VECTOR(OFS_PARM0);

        if (value1[1] == 0 && value1[0] == 0)
        {
            yaw = 0;
        }
        else
        {
            yaw = (int)(MathF.Atan2(value1[1], value1[0]) * 180 / MathF.PI);

            if (yaw < 0)
            {
                yaw += 360;
            }
        }

        progs_c.G_FLOAT(OFS_RETURN) = yaw;
    }

    public static void PF_vectoangles()
    {
        float* value1;
        float forward;
        float yaw, pitch;

        value1 = progs_c.G_VECTOR(OFS_PARM0);

        if (value1[1] == 0 && value1[0] == 0)
        {
            yaw = 0;

            if (value1[2] > 0)
            {
                pitch = 90;
            }
            else
            {
                pitch = 270;
            }
        }
        else
        {
            yaw = (int)(MathF.Atan2(value1[1], value1[0]) * 180 / MathF.PI);

            if (yaw < 0)
            {
                yaw += 360;
            }

            forward = (float)mathlib_c.sqrt(value1[0] * value1[0] + value1[1] * value1[1]);
            pitch = (int)(MathF.Atan2(value1[2], forward) * 180 / MathF.PI);

            if (pitch < 0)
            {
                pitch += 360;
            }
        }

        progs_c.G_FLOAT(OFS_RETURN + 0) = pitch;
        progs_c.G_FLOAT(OFS_RETURN + 1) = yaw;
        progs_c.G_FLOAT(OFS_RETURN + 2) = 0;
    }

    public static void PF_random()
    {
        float num;

        //num = ()
    }

    public static void PF_particle()
    {
        float* org, dir;
        float color;
        float count;

        org = progs_c.G_VECTOR(OFS_PARM0);
        dir = progs_c.G_VECTOR(OFS_PARM1);
        color = progs_c.G_FLOAT(OFS_PARM2);
        count = progs_c.G_FLOAT(OFS_PARM3);
        SV_StartParticle(org, dir, color, count);
    }

    public static void PF_ambientsound()
    {
        char** check = null;
        char* samp;
        float* pos;
        float vol, attenuation;
        int i, soundnum;

        pos = progs_c.G_VECTOR(OFS_PARM0);
        samp = progs_c.G_STRING(OFS_PARM1);
        vol = progs_c.G_FLOAT(OFS_PARM2);
        attenuation = progs_c.G_FLOAT(OFS_PARM3);

        for (soundnum = 0, *check = server_c.sv.sound_precache; *check != null; check++, soundnum++)
        {
            if (!common_c.Q_strcmp(*check, samp))
            {
                break;
            }
        }

        if (*check == null)
        {
            console_c.Con_Printf($"no precache: {*samp}\n");
            return;
        }

        common_c.MSG_WriteByte(server_c.sv.signon, protocol_c.svc_spawnstaticsound);

        common_c.MSG_WriteByte(server_c.sv.signon, (int)vol * 255);
        common_c.MSG_WriteByte(server_c.sv.signon, (int)attenuation * 64);
    }

    public static void PF_sound()
    {
        char* sample;
        int channel;
        progs_c.edict_t* entity;
        int volume;
        float attenuation;

        entity = progs_c.G_EDICT(OFS_PARM0);
        channel = progs_c.G_FLOAT(OFS_PARM1);
        sample = progs_c.G_STRING(OFS_PARM2);
        volume = progs_c.G_FLOAT(OFS_PARM3) * 255;
        attenuation = progs_c.G_FLOAT(OFS_PARM4);

        if (volume < 0 || volume > 255)
        {
            sys_win_c.Sys_Error($"SV_StartSound: volume = {volume}");
        }

        if (attenuation < 0 || attenuation > 4)
        {
            sys_win_c.Sys_Error($"SV_StartSound: attenuation = {attenuation}");
        }

        if (channel < 0 || channel > 7)
        {
            sys_win_c.Sys_Error($"SV_StartSound: channel = {channel}");
        }

        sv_send_c.SV_StartSound(entity, channel, sample, volume, attenuation);
    }

    public static void PF_break()
    {
        console_c.Con_Printf("break statement\n");

        *(int*)-4 = 0;
    }

    public static void PF_traceline()
    {
        float* v1, v2;
        world_c.trace_t trace;
        int nomonsters;
        progs_c.edict_t* ent;

        v1 = progs_c.G_VECTOR(OFS_PARM0);
        v2 = progs_c.G_VECTOR(OFS_PARM1);
        nomonsters = progs_c.G_FLOAT(OFS_PARM2);
        ent = progs_c.G_EDICT(OFS_PARM3);

        trace = world_c.SV_Move(v1, mathlib_c.vec3_origin, mathlib_c.vec3_origin, v2, nomonsters, ent);

        pr_edict_c.pr_global_struct->trace_allsolid = trace.allsolid == true ? 1 : 0;
        pr_edict_c.pr_global_struct->trace_startsolid = trace.startsolid == true ? 1 : 0;
        pr_edict_c.pr_global_struct->trace_fraction = trace.fraction;
        pr_edict_c.pr_global_struct->trace_inwater = trace.inwater == true ? 1 : 0;
        pr_edict_c.pr_global_struct->trace_inopen = trace.inopen == true ? 1 : 0;
        mathlib_c.VectorCopy(trace.endpos, pr_edict_c.pr_global_struct->trace_endpos);
        mathlib_c.VectorCopy(trace.plane.normal, pr_edict_c.pr_global_struct->trace_plane_normal);
        pr_edict_c.pr_global_struct->trace_plane_dist = trace.plane.dist;

        if (trace.ent != null)
        {
            pr_edict_c.pr_global_struct->trace_ent = progs_c.EDICT_TO_PROG(trace.ent);
        }
        else
        {
            pr_edict_c.pr_global_struct->trace_ent = progs_c.EDICT_TO_PROG(server_c.sv.edicts);
        }
    }

#if QUAKE2
    public static extern world_c.trace_t SV_Trace_Toss(progs_c.edict_t* ent, progs_c.edict_t* ignore);

    public static void PF_TraceToss()
    {
        world_c.trace_t trace;
        progs_c.edict_t* ent;
        progs_c.edict_t* ignore;

        ent = progs_c.G_EDICT(OFS_PARM0);
        ignore = progs_c.G_EDICT(OFS_PARM1);

        trace = SV_Trace_Toss(ent, ignore);

        pr_edict_c.pr_global_struct->trace_allsolid = trace.allsolid == true ? 1 : 0;
        pr_edict_c.pr_global_struct->trace_startsolid = trace.startsolid == true ? 1 : 0;
        pr_edict_c.pr_global_struct->trace_fraction = trace.fraction;
        pr_edict_c.pr_global_struct->trace_inwater = trace.inwater == true ? 1 : 0;
        pr_edict_c.pr_global_struct->trace_inopen = trace.inopen == true ? 1 : 0;
        mathlib_c.VectorCopy(trace.endpos, pr_edict_c.pr_global_struct->trace_endpos);
        mathlib_c.VectorCopy(trace.plane.normal, pr_edict_c.pr_global_struct->trace_plane_normal);
        pr_edict_c.pr_global_struct->trace_plane_dist = trace.plane.dist;

        if (trace.ent != null)
        {
            pr_edict_c.pr_global_struct->trace_ent = progs_c.EDICT_TO_PROG(trace.ent);
        }
        else
        {
            pr_edict_c.pr_global_struct->trace_ent = progs_c.EDICT_TO_PROG(server_c.sv.edicts);
        }
    }
#endif

    public static void PF_checkpos()
    {
    }

    public static byte* checkpvs;

    public static int PF_newcheckclient(int check)
    {
        int i;
        byte* pvs;
        progs_c.edict_t* ent;
        model_c.mleaf_t* leaf;
        Vector3 org = new();

        if (check < 1)
        {
            check = 1;
        }

        if (check > server_c.svs.maxclients)
        {
            check = server_c.svs.maxclients;
        }

        if (check == server_c.svs.maxclients)
        {
            i = 1;
        }
        else
        {
            i = check + 1;
        }

        for (; ; i++)
        {
            if (i == server_c.svs.maxclients)
            {
                i = 1;
            }

            ent = progs_c.EDICT_NUM(i);

            if (i == check)
            {
                break;
            }

            if (ent->free)
            {
                continue;
            }

            if (ent->v.health <= 0)
            {
                continue;
            }

            if (((int)ent->v.flags & server_c.FL_NOTARGET) != 0)
            {
                continue;
            }

            break;
        }

        mathlib_c.VectorAdd(ent->v.origin, ent->v.view_ofs, org);
        leaf = model_c.Mod_PointInLeaf(org, server_c.sv.worldmodel);
        pvs = model_c.Mod_LeafPVS(leaf, server_c.sv.worldmodel);
        common_c.Q_memcpy(*checkpvs, *pvs, (server_c.sv.worldmodel->numleafs + 7) >> 3);

        return i;
    }

    public const int MAX_CHECK = 16;
    public static int c_invis, c_notvis;

    public static void PF_checkclient()
    {
        progs_c.edict_t* ent, self;
        model_c.mleaf_t* leaf;
        int l;
        Vector3 view = new();

        if (server_c.sv.time - server_c.sv.lastchecktime >= 0.1)
        {
            server_c.sv.lastcheck = PF_newcheckclient(server_c.sv.lastcheck);
            server_c.sv.lastchecktime = server_c.sv.time;
        }

        ent = progs_c.EDICT_NUM(server_c.sv.lastcheck);

        if (ent->free || ent->v.health <= 0)
        {
            progs_c.RETURN_EDICT(server_c.sv.edicts);
            return;
        }

        self = progs_c.PROG_TO_EDICT(progs_c.pr_global_struct->self);
        mathlib_c.VectorAdd(self->v.origin, self->v.view_ofs, view);
        leaf = model_c.Mod_PointInLeaf(view, server_c.sv.worldmodel);
        l = (int)(leaf - server_c.sv.worldmodel->leafs) - 1;

        if (l < 0 || (checkpvs[l >> 3] & (1 << (l & 7))) == 0)
        {
            c_notvis++;
            progs_c.RETURN_EDICT(server_c.sv.edicts);
            return;
        }

        c_invis++;
        progs_c.RETURN_EDICT(ent);
    }

    public static void PF_stuffcmd()
    {
        int entnum;
        char* str;
        server_c.client_t* old;

        entnum = progs_c.G_EDICTNUM(OFS_PARM0);

        if (entnum < 1 || entnum > server_c.svs.maxclients)
        {
            pr_exec_c.PR_RunError("Parm 0 not a client");
        }

        str = progs_c.G_STRING(OFS_PARM1);

        old = host_c.host_client;
        host_c.host_client = &server_c.svs.clients[entnum - 1];
        host_c.Host_ClientCommands($"{*str}");
        host_c.host_client = old;
    }

    public static void PF_localcmd()
    {
        char* str;

        str = progs_c.G_STRING(OFS_PARM0);
        cmd_c.Cbuf_AddText(str);
    }

    public static void PF_cvar()
    {
        char* str;

        str = progs_c.G_STRING(OFS_PARM0);

        progs_c.G_FLOAT(OFS_RETURN) = cvar_c.Cvar_VariableValue(str->ToString());
    }

    public static void PF_cvar_set()
    {
        char* var, val;

        var = progs_c.G_STRING(OFS_PARM0);
        val = progs_c.G_STRING(OFS_PARM1);

        cvar_c.Cvar_Set(var->ToString(), val->ToString());
    }

    public static void PF_findradius()
    {
        progs_c.edict_t* ent, chain;
        float rad;
        float* org;
        Vector3 eorg;
        int i, j;

        chain = (progs_c.edict_t*)server_c.sv.edicts;

        org = progs_c.G_VECTOR(OFS_PARM0);
        rad = progs_c.G_FLOAT(OFS_PARM1);

        ent = progs_c.NEXT_EDICT(server_c.sv.edicts);

        for (i = 1; i < server_c.sv.num_edicts; i++, ent = progs_c.NEXT_EDICT(ent))
        {
            if (ent->free)
            {
                continue;
            }

            if (ent->v.solid == server_c.SOLID_NOT)
            {
                continue;
            }

            for (j = 0; j < 3; j++)
            {
                eorg[j] = org[j] - (ent->v.origin[j] + (ent->v.mins[j] + ent->v.maxs[j]) * 0.5f);
            }

            if (mathlib_c.Length(eorg) > rad)
            {
                continue;
            }

            ent->v.chain = progs_c.EDICT_TO_PROG(chain);
            chain = ent;
        }

        progs_c.RETURN_EDICT(chain);
    }

    public static void PF_dprint()
    {
        console_c.Con_DPrintf(PF_VarString(0)->ToString());
    }

    public static char* pr_string_temp;

    public static void PF_ftos()
    {
        float v;
        v = progs_c.G_FLOAT(OFS_PARM0);

        if (v == (int)v)
        {
            Console.WriteLine(pr_string_temp->ToString(), $"{(int)v}");
        }
        else
        {
            Console.WriteLine(pr_string_temp->ToString(), $"{v}");
        }

        progs_c.G_INT(OFS_RETURN) = pr_string_temp - pr_edict_c.pr_strings;
    }

    public static void PF_fabs()
    {
        float v;
        v = progs_c.G_FLOAT(OFS_PARM0);
        progs_c.G_FLOAT(OFS_RETURN) = MathF.Abs(v);
    }

    public static void PF_vtos()
    {
        Console.WriteLine(pr_string_temp->ToString(), $"{progs_c.G_VECTOR(OFS_PARM0)[0]} {progs_c.G_VECTOR(OFS_PARM0)[1]} {progs_c.G_VECTOR(OFS_PARM0)[2]}");
        progs_c.G_INT(OFS_RETURN) = pr_string_temp - pr_edict_c.pr_strings;
    }

#if QUAKE2
    public static void PF_etos()
    {
        Console.WriteLine(pr_string_temp->ToString(), $"entity {progs_c.G_EDICTNUM(OFS_PARM0)}");
        progs_c.G_INT(OFS_RETURN) = pr_string_temp - pr_edict_c.pr_strings;
    }
#endif

    public static void PF_Spawn()
    {
        progs_c.edict_t* ed;
        ed = pr_edict_c.ED_Alloc();
        progs_c.RETURN_EDICT(ed);
    }

    public static void PF_Remove()
    {
        progs_c.edict_t* ed;

        ed = progs_c.G_EDICT(OFS_PARM0);
        pr_edict_c.ED_Free(ed);
    }

    public static void PF_Find()
#if QUAKE2
    {
        int e;
        int f;
        char* s, t;
        progs_c.edict_t* ed;
        progs_c.edict_t* first;
        progs_c.edict_t* second;
        progs_c.edict_t* last;

        first = second = last = (progs_c.edict_t*)server_c.sv.edicts;
        e = progs_c.G_EDICTNUM(OFS_PARM0);
        f = progs_c.G_INT(OFS_PARM1);
        s = progs_c.G_STRING(OFS_PARM2);

        if (s == null)
        {
            pr_exec_c.PR_RunError("PF_Find: bad search string");
        }

        for (e++; e < server_c.sv.num_edicts; e++)
        {
            ed = progs_c.EDICT_NUM(e);

            if (ed->free)
            {
                continue;
            }

            t = progs_c.E_STRING(ed, f);

            if (t == null)
            {
                continue;
            }

            if (!common_c.Q_strcmp(t, s))
            {
                if (first == (progs_c.edict_t*)server_c.sv.edicts)
                {
                    first = ed;
                }
                else if (second == (progs_c.edict_t*)server_c.sv.edicts)
                {
                    second = ed;
                }

                ed->v.chain = progs_c.EDICT_TO_PROG(last);
                last = ed;
            }
        }

        if (first != last)
        {
            if (last != second)
            {
                first->v.chain = last->v.chain;
            }
            else
            {
                first->v.chain = progs_c.EDICT_TO_PROG(last);
            }

            last->v.chain = progs_c.EDICT_TO_PROG((progs_c.edict_t*)server_c.sv.edicts);

            if (second != null && second != last)
            {
                second->v.chain = progs_c.EDICT_TO_PROG(last);
            }
        }

        progs_c.RETURN_EDICT(first);
    }
#else
    {
        int e;
        int f;
        char* s, t;
        progs_c.edict_t* ed;

        e = progs_c.G_EDICTNUM(OFS_PARM0);
        f = progs_c.G_INT(OFS_PARM1);
        s = progs_c.G_STRING(OFS_PARM2);

        if (s == null)
        {
            pr_exec_c.PR_RunError("PF_Find: bad search string");
        }

        for (e++; e < server_c.sv.num_edicts; e++)
        {
            ed = progs_c.EDICT_NUM(e);

            if (ed->free)
            {
                continue;
            }

            t = progs_c.E_STRING(ed, f);

            if (t == null)
            {
                continue;
            }

            if (!common_c.Q_strcmp(t, s))
            {
                progs_c.RETURN_EDICT(ed);
                return;
            }
        }

        progs_c.RETURN_EDICT(server_c.sv.edicts);
    }
#endif

    public static void PR_CheckEmptyString(char* s)
    {
        if (s[0] <= ' ')
        {
            pr_exec_c.PR_RunError("Bad string");
        }
    }

    public static void PF_precache_file()
    {
        progs_c.G_INT(OFS_RETURN) = progs_c.G_INT(OFS_PARM0);
    }

    public static void PF_precache_sound()
    {
        char* s;
        int i;

        if (server_c.sv.state != server_c.server_state_t.ss_loading)
        {
            pr_exec_c.PR_RunError("PF_Precache_*: Precache can only be done in spawn functions");
        }

        s = progs_c.G_STRING(OFS_PARM0);
        progs_c.G_INT(OFS_RETURN) = progs_c.G_INT(OFS_PARM0);
        PR_CheckEmptyString(s);

        for (i = 0; i < quakedef_c.MAX_SOUNDS; i++)
        {
            if (server_c.sv.sound_precache[i] == null)
            {
                server_c.sv.sound_precache[i] = *s;
                return;
            }

            if (!common_c.Q_strcmp(server_c.sv.sound_precache[i], s))
            {
                return;
            }
        }

        pr_exec_c.PR_RunError("PF_precache_sound: overflow");
    }

    public static void PF_precache_model()
    {
        char* s = null;
        int i;

        if (server_c.sv.state != server_c.server_state_t.ss_loading)
        {
            pr_exec_c.PR_RunError("PF_Precache_*: Precache can only be done in spawn functions");
        }

        s = progs_c.G_STRING(OFS_PARM0);
        progs_c.G_INT(OFS_RETURN) = progs_c.G_INT(OFS_PARM0);
        PR_CheckEmptyString(s);

        for (i = 0; i < quakedef_c.MAX_MODELS; i++)
        {
            if (server_c.sv.model_precache[i] == 0)
            {
                server_c.sv.model_precache[i] = *s;
                server_c.sv.models[i] = *model_c.Mod_ForName(s, true);
                return;
            }

            if (!common_c.Q_strcmp(server_c.sv.model_precache[i], s))
            {
                return;
            }
        }

        pr_exec_c.PR_RunError("PF_precache_model: overflow");
    }

    public static void PF_coredump()
    {
        pr_edict_c.ED_PrintEdicts();
    }

    public static void PF_traceon()
    {
        pr_exec_c.pr_trace = true;
    }

    public static void PF_traceoff()
    {
        pr_exec_c.pr_trace = false;
    }

    public static void PF_eprint()
    {
        pr_edict_c.ED_PrintNum(progs_c.G_EDICTNUM(OFS_PARM0));
    }

    public static void PF_walkmove()
    {
        progs_c.edict_t* ent;
        float yaw, dist;
        Vector3 move = new();
        pr_comp_c.dfunction_t* oldf;
        int oldself;

        ent = progs_c.PROG_TO_EDICT(pr_edict_c.pr_global_struct->self);
        yaw = progs_c.G_FLOAT(OFS_PARM0);
        dist = progs_c.G_FLOAT(OFS_PARM1);

        if (((int)ent->v.flags & (server_c.FL_ONGROUND | server_c.FL_FLY | server_c.FL_SWIM)) == 0)
        {
            progs_c.G_FLOAT(OFS_RETURN) = 0;
            return;
        }

        yaw = yaw * MathF.PI * 2 / 360;

        move[0] = MathF.Cos(yaw) * dist;
        move[1] = MathF.Sin(yaw) * dist;
        move[2] = 0;

        oldf = pr_exec_c.pr_xfunction;
        oldself = pr_edict_c.pr_global_struct->self;

        progs_c.G_FLOAT(OFS_RETURN) = sv_move_c.SV_movestep(ent, move, true);

        pr_exec_c.pr_xfunction = oldf;
        pr_edict_c.pr_global_struct->self = oldself;
    }

    public static void PF_droptofloor()
    {
        progs_c.edict_t* ent;
        Vector3 end = new();
        world_c.trace_t trace;

        ent = progs_c.PROG_TO_EDICT(pr_edict_c.pr_global_struct->self);

        mathlib_c.VectorCopy(ent->v.origin, end);
        end[2] -= 256;

        trace = world_c.SV_Move(ent->v.origin, ent->v.mins, ent->v.maxs, end, 0, ent);

        if (trace.fraction == 1 || trace.allsolid)
        {
            progs_c.G_FLOAT(OFS_RETURN) = 0;
        }
        else
        {
            mathlib_c.VectorCopy(trace.endpos, ent->v.origin);
            world_c.SV_LinkEdict(ent, false);
            ent->v.flags = (int)ent->v.flags | server_c.FL_ONGROUND;
            ent->v.groundentity = progs_c.EDICT_TO_PROG(trace.ent);
            progs_c.G_FLOAT(OFS_RETURN) = 1;
        }
    }

    public static void PF_lightstyle()
    {
        int style;
        char* val;
        server_c.client_t* client;
        int j;

        style = progs_c.G_FLOAT(OFS_PARM0);
        val = progs_c.G_STRING(OFS_PARM1);

        server_c.sv.lightstyles[style] = *val;

        if (server_c.sv.state == server_c.server_state_t.ss_active)
        {
            return;
        }

        for (j = 0, client = server_c.svs.clients; j < server_c.svs.maxclients; j++, client++)
        {
            if (client->active || client->spawned)
            {
                common_c.MSG_WriteChar(client->message, protocol_c.svc_lightstyle);
                common_c.MSG_WriteChar(client->message, style);
                common_c.MSG_WriteString(client->message, val);
            }
        }
    }

    public static void PF_rint()
    {
        float f;
        f = progs_c.G_FLOAT(OFS_PARM0);

        if (f > 0)
        {
            progs_c.G_FLOAT(OFS_RETURN) = (int)(f + 0.5f);
        }
        else
        {
            progs_c.G_FLOAT(OFS_RETURN) = (int)(f - 0.5f);
        }
    }

    public static void PF_floor()
    {
        progs_c.G_FLOAT(OFS_RETURN) = MathF.Floor(progs_c.G_FLOAT(OFS_PARM0));
    }

    public static void PF_ceil()
    {
        progs_c.G_FLOAT(OFS_RETURN) = MathF.Ceiling(progs_c.G_FLOAT(OFS_PARM0));
    }

    public static void PF_checkbottom()
    {
        progs_c.edict_t* ent;

        ent = progs_c.G_EDICT(OFS_PARM0);

        progs_c.G_FLOAT(OFS_RETURN) = sv_move_c.SV_CheckBottom(ent);
    }

    public static void PF_pointcontents()
    {
        float* v;

        v = progs_c.G_VECTOR(OFS_PARM0);

        progs_c.G_FLOAT(OFS_RETURN) = world_c.SV_PointContents(mathlib_c.FloatPtrToVec(v));
    }

    public static void PF_nextent()
    {
        int i;
        progs_c.edict_t* ent;

        i = progs_c.G_EDICTNUM(OFS_PARM0);

        while (true)
        {
            i++;

            if (i == server_c.sv.num_edicts)
            {
                progs_c.RETURN_EDICT(server_c.sv.edicts);
                return;
            }

            ent = progs_c.EDICT_NUM(i);

            if (!ent->free)
            {
                progs_c.RETURN_EDICT(ent);
                return;
            }
        }
    }

    public static cvar_c.cvar_t sv_aim = new cvar_c.cvar_t { name = "sv_aim", value = (char)0.93f };

    public static void PF_aim()
    {
        progs_c.edict_t* ent, check, bestent;
        Vector3 start, dir, end, bestdir;
        int i, j;
        world_c.trace_t tr;
        float dist, bestdist;
        float speed;

        start = dir = end = bestdir = new();

        ent = progs_c.G_EDICT(OFS_PARM0);
        speed = progs_c.G_FLOAT(OFS_PARM1);

        mathlib_c.VectorCopy(ent->v.origin, start);
        start[2] += 20;

        mathlib_c.VectorCopy(pr_edict_c.pr_global_struct->v_forward, dir);
        mathlib_c.VectorMA(start, 2048, dir, end);
        tr = world_c.SV_Move(start, mathlib_c.vec3_origin, mathlib_c.vec3_origin, end, 0, ent);

        if (tr.ent != null && tr.ent->v.takedamage == server_c.DAMAGE_AIM && (server_c.teamplay.value == 0 || ent->v.team <= 0 || ent->v.team != tr.ent->v.team))
        {
            mathlib_c.VectorCopy(pr_edict_c.pr_global_struct->v_forward, progs_c.G_VECTOR(OFS_RETURN));
            return;
        }

        mathlib_c.VectorCopy(dir, bestdir);
        bestdist = sv_aim.value;
        bestent = null;

        check = progs_c.NEXT_EDICT(server_c.sv.edicts);

        for (i = 1; i < server_c.sv.num_edicts; i++, check = progs_c.NEXT_EDICT(check))
        {
            if (check->v.takedamage != server_c.DAMAGE_AIM)
            {
                continue;
            }

            if (check == ent)
            {
                continue;
            }

            if (server_c.teamplay.value != 0 && ent->v.team > 0 && ent->v.team == check->v.team)
            {
                continue;
            }

            for (j = 0; j < 3; j++)
            {
                end[j] = check->v.origin[j] + 0.5f * (check->v.mins[j] + check->v.maxs[j]);
            }

            mathlib_c.VectorSubtract(end, start, dir);
            mathlib_c.VectorNormalize(dir);
            dist = mathlib_c.DotProduct(dir, pr_edict_c.pr_global_struct->v_forward);

            if (dist < bestdist)
            {
                continue;
            }

            tr = world_c.SV_Move(start, mathlib_c.vec3_origin, mathlib_c.vec3_origin, end, 0, ent);

            if (tr.ent == check)
            {
                bestdist = dist;
                bestent = check;
            }
        }

        if (bestent != null)
        {
            mathlib_c.VectorSubtract(bestent->v.origin, ent->v.origin, dir);
            dist = mathlib_c.DotProduct(dir, pr_edict_c.pr_global_struct->v_forward);
            mathlib_c.VectorScale(pr_edict_c.pr_global_struct->v_forward, dist, end);
            end[2] = dir[2];
            mathlib_c.VectorNormalize(end);
            mathlib_c.VectorCopy(end, progs_c.G_VECTOR(OFS_RETURN));
        }
        else
        {
            mathlib_c.VectorCopy(bestdir, progs_c.G_VECTOR(OFS_RETURN));
        }
    }

    public static void PF_changeyaw()
    {
        progs_c.edict_t* ent;
        float ideal, current, move, speed;

        ent = progs_c.PROG_TO_EDICT(pr_edict_c.pr_global_struct->self);
        current = mathlib_c.anglemod(ent->v.angles[1]);
        ideal = ent->v.ideal_yaw;
        speed = ent->v.yaw_speed;

        if (current == ideal)
        {
            return;
        }

        move = ideal - current;

        if (ideal > current)
        {
            if (move >= 180)
            {
                move = move - 360;
            }
        }
        else
        {
            if (move <= -180)
            {
                move = move + 360;
            }
        }

        if (move > 0)
        {
            if (move > speed)
            {
                move = speed;
            }
        }
        else
        {
            if (move < -speed)
            {
                move = -speed;
            }
        }

        ent->v.angles[1] = mathlib_c.anglemod(current + move);
    }

#if QUAKE2
    public static void PF_changepitch()
    {
        progs_c.edict_t* ent;
        float ideal, current, move = 0, speed;

        ent = progs_c.G_EDICT(OFS_PARM0);
        current = mathlib_c.anglemod(ent->v.angles[0]);
        ideal = ent->v.idealpitch;
        speed = ent->v.pitch_speed;

        if (current == ideal)
        {
            return;
        }

        if (ideal > current)
        {
            if (move >= 180)
            {
                move = move - 360;
            }
        }
        else
        {
            if (move <= 180)
            {
                move = move + 360;
            }
        }

        if (move > 0)
        {
            if (move > speed)
            {
                move = speed;
            }
        }
        else
        {
            if (move < -speed)
            {
                move = -speed;
            }
        }

        ent->v.angles[0] = mathlib_c.anglemod(current + move);
    }
#endif

    public const int MSG_BROADCAST = 0;
    public const int MSG_ONE = 1;
    public const int MSG_ALL = 2;
    public const int MSG_INIT = 3;

    public static common_c.sizebuf_t WriteDest()
    {
        int entnum;
        int dest;
        progs_c.edict_t* ent;

        dest = progs_c.G_FLOAT(OFS_PARM0);

        switch (dest)
        {
            case MSG_BROADCAST:
                return server_c.sv.datagram;

            case MSG_ONE:
                ent = progs_c.PROG_TO_EDICT(pr_edict_c.pr_global_struct->msg_entity);
                entnum = progs_c.NUM_FOR_EDICT(ent);

                if (entnum < 1 || entnum > server_c.svs.maxclients)
                {
                    pr_exec_c.PR_RunError("WriteDest: not a client");
                }

                return server_c.svs.clients[entnum - 1].message;

            case MSG_ALL:
                return server_c.sv.reliable_datagram;

            case MSG_INIT:
                return server_c.sv.signon;

            default:
                pr_exec_c.PR_RunError("WriteDest: bad destination");
                break;
        }

        return default;
    }

    public static void PF_WriteByte()
    {
        common_c.MSG_WriteByte(WriteDest, progs_c.G_FLOAT(OFS_PARM1));
    }

    public static void PF_WriteChar()
    {
        common_c.MSG_WriteChar(WriteDest, progs_c.G_FLOAT(OFS_PARM1));
    }

    public static void PF_WriteShort()
    {
        common_c.MSG_WriteShort(WriteDest, progs_c.G_FLOAT(OFS_PARM1));
    }

    public static void PF_WriteLong()
    {
        common_c.MSG_WriteLong(WriteDest, progs_c.G_FLOAT(OFS_PARM1));
    }

    public static void PF_WriteAngle()
    {
        common_c.MSG_WriteAngle(WriteDest, progs_c.G_FLOAT(OFS_PARM1));
    }

    public static void PF_WriteCoord()
    {
        common_c.MSG_WriteCoord(WriteDest, progs_c.G_FLOAT(OFS_PARM1));
    }

    public static void PF_WriteString()
    {
        common_c.MSG_WriteString(WriteDest, progs_c.G_STRING(OFS_PARM1));
    }

    public static void PF_WriteEntity()
    {
        common_c.MSG_WriteShort(WriteDest, progs_c.G_EDICTNUM(OFS_PARM1));
    }

    public static extern int SV_ModelIndex(char* name);

    public static void PF_makestatic()
    {
        progs_c.edict_t* ent;
        int i;

        ent = progs_c.G_EDICT(OFS_PARM0);

        common_c.MSG_WriteByte(server_c.sv.signon, protocol_c.svc_spawnstatic);

        common_c.MSG_WriteByte(server_c.sv.signon, SV_ModelIndex(*pr_edict_c.pr_strings + ent->v.model));

        common_c.MSG_WriteByte(server_c.sv.signon, (int)ent->v.frame);
        common_c.MSG_WriteByte(server_c.sv.signon, (int)ent->v.colormap);
        common_c.MSG_WriteByte(server_c.sv.signon, (int)ent->v.skin);

        for (i = 0; i < 3; i++)
        {
            common_c.MSG_WriteCoord(server_c.sv.signon, ent->v.origin[i]);
            common_c.MSG_WriteAngle(server_c.sv.signon, ent->v.angles[i]);
        }

        pr_edict_c.ED_Free(ent);
    }

    public static void PF_setspawnparameters()
    {
        progs_c.edict_t* ent;
        int i;
        server_c.client_t* client;

        ent = progs_c.G_EDICT(OFS_PARM0);
        i = progs_c.NUM_FOR_EDICT(ent);

        if (i < 1 || i > server_c.svs.maxclients)
        {
            pr_exec_c.PR_RunError("Entity is not a client");
        }

        client = server_c.svs.clients + (i - 1);

        for (i = 0; i < server_c.NUM_SPAWN_PARMS; i++)
        {
            (&pr_edict_c.pr_global_struct->parm1)[i] = client->spawn_parms[i];
        }
    }

    public static void PF_changelevel()
    {
#if QUAKE2
        char* s1, s2;

        if (server_c.svs.changelevel_issued)
        {
            return;
        }

        server_c.svs.changelevel_issued = true;

        s1 = progs_c.G_STRING(OFS_PARM0);
        s2 = progs_c.G_STRING(OFS_PARM1);

        if ((int)pr_edict_c.pr_global_struct->serverflags & (SFL_NEW_UNIT | SFL_NEW_EPISODE) != 0)
        {
            cmd_c.Cbuf_AddText(common_c.va($"changelevel {*s1} {*s2}\n"));
        }
        else
        {
            cmd_c.Cbuf_AddText(common_c.va($"changelevel2 {*s1} {*s2}\n"));
        }
#else
        char* s;

        if (server_c.svs.changelevel_issued)
        {
            return;
        }

        server_c.svs.changelevel_issued = true;

        s = progs_c.G_STRING(OFS_PARM0);
        cmd_c.Cbuf_AddText(common_c.va($"changelevel {*s}\n"));
#endif
    }

#if QUAKE2
    public const int CONTENT_WATER = -3;
    public const int CONTENT_SLIME = -4;
    public const int CONTENT_LAVA = -5;

    public const int FL_IMMUNE_WATER = 131072;
    public const int FL_IMMUNE_SLIME = 262144;
    public const int FL_IMMUNE_LAVA = 524288;

    public const int CHAN_VOICE = 2;
    public const int CHAN_BODY = 4;

    public const int ATTN_NORM = 1;

    public static void PF_WaterMove()
    {
        progs_c.edict_t* self;
        int flags;
        int waterlevel;
        int watertype;
        float drownlevel;
        float damage = 0.0f;

        self = progs_c.PROG_TO_EDICT(pr_edict_c.pr_global_struct->self);

        if (self->v.movetype == server_c.MOVETYPE_NOCLIP)
        {
            self->v.air_finished = server_c.sv.time + 12;
            progs_c.G_FLOAT(OFS_RETURN) = damage;
            return;
        }

        if (self->v.health < 0)
        {
            progs_c.G_FLOAT(OFS_RETURN) = damage;
            return;
        }

        if (self->v.deadflag == server_c.DEAD_NO)
        {
            drownlevel = 3;
        }
        else
        {
            drownlevel = 1;
        }

        flags = (int)self->v.flags;
        waterlevel = (int)self->v.waterlevel;
        watertype = (int)self->v.watertype;

        if ((flags & (FL_IMMUNE_WATER + server_c.FL_GODMODE)) == 0)
        {
            if (((flags & server_c.FL_SWIM) != 0 && (waterlevel < drownlevel)) || (waterlevel >= drownlevel))
            {
                if (self->v.air_finished < server_c.sv.time)
                {
                    if (self->v.pain_finished < server_c.sv_time)
                    {
                        self->v.dmg = self->v.dmg + 2;

                        if (self->v.dmg > 15)
                        {
                            self->v.dmg = 10;
                        }

                        damage = self->v.dmg;
                        self->v.pain_finished = server_c.sv.time + 1.0;
                    }
                }
                else
                {
                    if (self->v.air_finished < server_c.sv.time)
                    {
                        sv_send_c.SV_StartSound(self, CHAN_VOICE, "player/gasp2.wav", 255, ATTN_NORM);
                    }
                    else if (self->v.air_finished < server_c.sv.time + 9)
                    {
                        sv_send_c.SV_StartSound(self, CHAN_VOICE, "player/gasp1.wav", 255, ATTN_NORM);
                    }

                    self->v.air_finished = server_c.sv.time + 12.0f;
                    self->v.dmg = 2;
                }
            }
        }

        if (waterlevel == 0)
        {
            if ((flags & server_c.FL_INWATER) != 0)
            {
                sv_send_c.SV_StartSound(self, CHAN_BODY, "misc/outwater.wav", 255, ATTN_NORM);
                self->v.flags = (float)(flags & ~server_c.FL_INWATER);
            }

            self->v.air_finished = server_c.sv.time + 12.0f;
            progs_c.G_FLOAT(OFS_RETURN) = damage;
            return;
        }

        if (watertype == CONTENT_LAVA)
        {
            if ((flags & (FL_IMMUNE_LAVA + server_c.FL_GODMODE)) == 0)
            {
                if (self->v.dmgtime < server_c.sv.time)
                {
                    if (self->v.radsuit_finished < server_c.sv.time)
                    {
                        self->v.dmgtime = server_c.sv.time + 0.2f;
                    }
                    else
                    {
                        self->v.dmgtime = server_c.sv.time + 1.0f;
                    }

                    damage = (float)(10 * waterlevel);
                }
            }
        }
        else if (watertype == CONTENT_SLIME)
        {
            if ((flags & (FL_IMMUNE_SLIME + server_c.FL_GODMODE)) == 0)
            {
                if (self->v.dmgtime < server_c.sv.time)
                {
                    if (self->v.radsuit_finished < server_c.sv.time)
                    {
                        self->v.dmgtime = server_c.sv.time + 0.2f;
                    }
                    else
                    {
                        self->v.dmgtime = server_c.sv.time + 1.0f;
                    }

                    damage = (float)(10 * waterlevel);
                }
            }
        }

        if ((flags & server_c.FL_INWATER) == 0)
        {
            if (watertype == CONTENT_LAVA)
            {
                sv_send_c.SV_StartSound(self, CHAN_BODY, "player/inlava.wav", 255, ATTN_NORM);
            }

            if (watertype == CONTENT_WATER)
            {
                sv_send_c.SV_StartSound(self, CHAN_BODY, "player/inh2o.wav", 255, ATTN_NORM);
            }

            if (watertype == CONTENT_SLIME)
            {
                sv_send_c.SV_StartSound(self, CHAN_BODY, "player/slimbrn2.wav", 255, ATTN_NORM);
            }

            self->v.flags = (float)(flags | server_c.FL_INWATER);
            self->v.dmgtime = 0;
        }

        if ((flags & server_c.FL_WATERJUMP) == 0)
        {
            mathlib_c.VectorMA(self->v.velocity, -0.8f * self->v.waterlevel * host_c.host_frametime, self->v.velocity, self->v.velocity);
        }

        progs_c.G_FLOAT(OFS_RETURN) = damage;
    }

    public static void PF_sin()
    {
        progs_c.G_FLOAT(OFS_RETURN) = MathF.Sin(progs_c.G_FLOAT(OFS_PARM0));
    }

    public static void PF_cos()
    {
        progs_c.G_FLOAT(OFS_RETURN) = MathF.Cos(progs_c.G_FLOAT(OFS_PARM0));
    }

    public static void PF_sqrt()
    {
        progs_c.G_FLOAT(OFS_RETURN) = mathlib_c.sqrt(progs_c.G_FLOAT(OFS_PARM0));
    }
#endif

    public static void PF_Fixme()
    {
        pr_exec_c.PR_RunError("unimplemented builtin");
    }

    public static Action[] pr_builtin =
    {
        PF_Fixme,
        PF_makevectors,
        PF_setorigin,
        PF_setmodel,
        PF_setsize,
        PF_Fixme,
        PF_break,
        PF_random,
        PF_sound,
        PF_normalize,
        PF_error,
        PF_objerror,
        PF_vlen,
        PF_vectoyaw,
        PF_Spawn,
        PF_Remove,
        PF_traceline,
        PF_checkclient,
        PF_Find,
        PF_precache_sound,
        PF_precache_model,
        PF_stuffcmd,
        PF_findradius,
        PF_bprint,
        PF_sprint,
        PF_dprint,
        PF_ftos,
        PF_vtos,
        PF_coredump,
        PF_traceon,
        PF_traceoff,
        PF_eprint,
        PF_walkmove,
        PF_Fixme,
        PF_droptofloor,
        PF_pointcontents,
        PF_Fixme,
        PF_fabs,
        PF_aim,
        PF_cvar,
        PF_localcmd,
        PF_nextent,
        PF_particle,
        PF_changeyaw,
        PF_Fixme,
        PF_vectoangles,

        PF_WriteByte,
        PF_WriteChar,
        PF_WriteShort,
        PF_WriteLong,
        PF_WriteCoord,
        PF_WriteAngle,
        PF_WriteString,
        PF_WriteEntity,

#if QUAKE2
        PF_sin,
        PF_cos,
        PF_sqrt,
        PF_changepitch,
        PF_TraceToss,
        PF_etos,
        PF_WaterMove
#else
        PF_Fixme,
        PF_Fixme,
        PF_Fixme,
        PF_Fixme,
        PF_Fixme,
        PF_Fixme,
        PF_Fixme,
#endif

        sv_move_c.SV_MoveToGoal,
        PF_precache_file,
        PF_makestatic,

        PF_changelevel,
        PF_Fixme,

        PF_cvar_set,
        PF_centerprint,

        PF_ambientsound,

        PF_precache_model,
        PF_precache_sound,
        PF_precache_file,

        PF_setspawnparameters
    };

    public static Action* pr_builtins = pr_builtin;
    public static int pr_numbuiltins = sizeof(pr_builtin) / sizeof(pr_builtin[0]);
}