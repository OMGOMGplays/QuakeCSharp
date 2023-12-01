namespace Quake;

public unsafe class cl_main_c
{
    public cvar_c.cvar_t cl_name = new cvar_c.cvar_t { name = "_cl_name", value = "player".ToCharArray()[0], archive = true }
    public cvar_c.cvar_t cl_color = new cvar_c.cvar_t { name = "cl_color", value = (char)0, archive = true };

    public cvar_c.cvar_t cl_shownet = new cvar_c.cvar_t { name = "cl_shownet", value = (char)0 };
    public cvar_c.cvar_t cl_nolerp = new cvar_c.cvar_t { name = "cl_nolerp", value = (char)0 };

    public static cvar_c.cvar_t lookspring = new cvar_c.cvar_t { name = "lookspring", value = (char)0, archive = true };
    public cvar_c.cvar_t lookstrafe = new cvar_c.cvar_t { name = "lookstrafe", value = (char)0, archive = true };
    public cvar_c.cvar_t sensitivity = new cvar_c.cvar_t { name = "sensitivity", value = (char)3, archive = true };

    public cvar_c.cvar_t m_pitch = new cvar_c.cvar_t { name = "m_pitch", value = (char)0.022, archive = true };
    public cvar_c.cvar_t m_yaw = new cvar_c.cvar_t { name = "m_yaw", value = (char)0.022, archive = true };
    public cvar_c.cvar_t m_forward = new cvar_c.cvar_t { name = "m_forward", value = (char)1, archive = true };
    public cvar_c.cvar_t m_side = new cvar_c.cvar_t { name = "m_side", value = (char)0.8, archive = true };

    public static client_c.client_static_t cls;
    public static sv_main_c.client_state_t cl;

    public render_c.efrag_t[] cl_efrags = new render_c.efrag_t[client_c.MAX_EFRAGS];
    public render_c.entity_t cl_entities;
    public render_c.entity_t[] cl_static_entites = new render_c.entity_t[client_c.MAX_STATIC_ENTITIES];
    public client_c.lightstyle_t[] cl_lightstyle = new client_c.lightstyle_t[bothdefs_c.MAX_LIGHTSTYLES];
    public static client_c.dlight_t* cl_dlights;

    public static int cl_numvisedicts;
    public static render_c.entity_t* cl_visedicts;

    public void CL_ClearState()
    {
        int i;

        if (!sv.active)
        {
            Host_ClearMemory();
        }

        common_c.Q_memset(cl, 0, sizeof(sv_main_c.client_state_t));

        common_c.SZ_Clear(cls.message);

        common_c.Q_memset(cl_efrags, 0, client_c.MAX_EFRAGS);
        common_c.Q_memset(cl_entities, 0, quakedef_c.MAX_EDICTS);
        common_c.Q_memset(cl_dlights, 0, client_c.MAX_DLIGHTS);
        common_c.Q_memset(cl_lightstyle, 0, bothdefs_c.MAX_LIGHTSTYLES);
        common_c.Q_memset(cl_tent_c.cl_temp_entities, 0, client_c.MAX_TEMP_ENTITIES);
        common_c.Q_memset(cl_tent_c.cl_beams, 0, client_c.MAX_BEAMS);

        cl.free_efrags = cl_efrags;

        for (i = 0; i < client_c.MAX_EFRAGS - 1; i++)
        {
            cl.free_efrags[i].entnext = cl.free_efrags[i + 1];
        }

        cl.free_efrags[i].entnext = null;
    }

    public void CL_Disconnect()
    {
        S_StopAllSounds(true);

        if (cls.demoplayback)
        {
            CL_StopPlayback();
        }
        else if (cls.state == client_c.cactive_t.ca_connected)
        {
            if (cls.demorecording)
            {
                CL_Stop_f();
            }

            console_c.Con_DPrintf("Sending clc_disconnect\n");
            common_c.SZ_Clear(cls.message);
            common_c.MSG_WriteByte(cls.message, clc_disconnect);
            NET_SendUnreliableMessage(cls.netcon, cls.message);
            common_c.SZ_Clear(cls.message);
            NET_Close(cls.netcon);

            cls.state = client_c.cactive_t.ca_disconnected;

            if (sv.active)
            {
                Host_ShutdownServer(false);
            }
        }

        cls.demoplayback = cls.timedemo = false;
        cls.signon = 0;
    }

    public void CL_Disconnect_f()
    {
        CL_Disconnect();

        if (sv.active)
        {
            Host_ShutdownServer(false);
        }
    }

    public void CL_EstablishConnection(string host)
    {
        if (cls.state == client_c.cactive_t.ca_dedicated)
        {
            return;
        }

        if (cls.demoplayback)
        {
            return;
        }

        CL_Disconnect();

        cls.netcon = NET_Connect(host);

        if (cls.netcon == null)
        {
            Host_Error("CL_Connect: connect failed\n");
        }

        console_c.Con_DPrintf($"CL_EstablishConnection: connected to {host}\n");

        cls.demonum = -1;
        cls.state = client_c.cactive_t.ca_connected;
        cls.signon = 0;
    }

    public void CL_SignonReply()
    {
        char[] str = new char[8192];

        console_c.Con_DPrintf($"CL_SignonReply: {cls.signon}\n");

        switch (cls.signon)
        {
            case 1:
                common_c.MSG_WriteByte(cls.message, clc_stringcmd);
                common_c.MSG_WriteString(cls.message, *common_c.StringToChar("prespawn"));
                break;

            case 2:
                common_c.MSG_WriteByte(cls.message, clc_stringcmd);
                common_c.MSG_WriteString(cls.message, *common_c.StringToChar(common_c.va($"name \"{cl_name.str->ToString()}\"\n")));

                common_c.MSG_WriteByte(cls.message, clc_stringcmd);
                common_c.MSG_WriteString(cls.message, *common_c.StringToChar(common_c.va($"color {(cl_color.value) >> 4} {(cl_color.value) & 15}\n")));

                common_c.MSG_WriteByte(cls.message, clc_stringcmd);
                Console.WriteLine($"spawn {cls.spawnparms->ToString()}");
                common_c.MSG_WriteString(cls.message, *common_c.StringToChar(str.ToString()));
                break;

            case 3:
                common_c.MSG_WriteByte(cls.message, clc_stringcmd);
                common_c.MSG_WriteString(cls.message, *common_c.StringToChar("begin"));
                Cache_Report();
                break;

            case 4:
                screen_c.SCR_EndLoadingPlaque();
                break;
        }
    }

    public void CL_NextDemo()
    {
        char[] str = new char[1024];

        if (cls.demonum == -1)
        {
            return;
        }

        screen_c.SCR_BeginLoadingPlaque();

        if (cls.demos[cls.demonum] != 0 || cls.demonum == client_c.MAX_DEMOS)
        {
            cls.demonum = 0;

            if (cls.demos[cls.demonum] == 0)
            {
                console_c.Con_Printf("No demos listed with startdemos\n");
                cls.demonum = -1;
                return;
            }
        }

        Console.WriteLine($"playdemo {cls.demos[cls.demonum]}\n");
        Cbuf_InsertText(str);
        cls.demonum++;
    }

    public void CL_PrintEntities_f()
    {
        render_c.entity_t* ent;
        int i;

        for (i = 0, ent = cl_entities; i < cl.num_entities; i++, ent++)
        {
            console_c.Con_Printf($"{i}");

            if (ent->model == null)
            {
                console_c.Con_Printf("EMPTY\n");
                continue;
            }

            console_c.Con_Printf($"{ent->model->name}:{ent->frame} ({ent->origin[0]}, {ent->origin[1]}, {ent->origin[2]}) [{ent->angles[0]}, {ent->angles[1]}, {ent->angles[2]}]\n");
        }
    }

    public void SetPal(int i)
    {
#if 0
        int old;
        byte[] pal = new byte[768];
        int c;

        if (i == old) 
        {
            return;
        }

        old = i;

        if (i == 0) 
        {
            VID_SetPalette(host_basepal);
        }
        else if (i == 1) 
        {
            for (c = 0; c < 768; c+=3) 
            {
                pal[c] = 0;
                pal[c+1] = 0;
                pal[c+2] = 255;
            }

            VID_SetPalette(pal);
        }

#endif
    }

    public static client_c.dlight_t* CL_AllocDlight(int key)
    {
        int i;
        client_c.dlight_t* dl;

        if (key != 0)
        {
            dl = cl_dlights;

            for (i = 0; i < client_c.MAX_DLIGHTS; i++, dl++)
            {
                if (dl->key == key)
                {
                    common_c.Q_memset(*dl, 0, sizeof(client_c.dlight_t));
                    dl->key = key;
                    return dl;
                }
            }
        }

        dl = cl_dlights;

        for (i = 0; i < client_c.MAX_DLIGHTS; i++, dl++)
        {
            if (dl->die < cl.time)
            {
                common_c.Q_memset(*dl, 0, sizeof(client_c.dlight_t));
                dl->key = key;
                return dl;
            }
        }

        dl = &cl_dlights[0];
        common_c.Q_memset(dl, 0, sizeof(client_c.dlight_t));
        dl->key = key;
        return dl;
    }

    public void CL_DecayLights()
    {
        int i;
        client_c.dlight_t* dl;
        float time;

        time = cl.time - cl.oldtime;

        dl = cl_dlights;

        for (i = 0; i < client_c.MAX_DLIGHTS; i++, dl++)
        {
            if (dl->die < cl.time || dl->radius == 0)
            {
                continue;
            }

            dl->radius -= time * dl->decay;

            if (dl->radius < 0)
            {
                dl->radius = 0;
            }
        }
    }

    public float CL_LerpPoint()
    {
        float f, frac;

        f = cl.mtime[0] - cl.mtime[1];

        if (f == 0 || cl_nolerp.value != 0 || cls.timedemo || sv.active)
        {
            cl.time = cl.mtime[0];
            return 1;
        }

        if (f > 0.1f)
        {
            cl.mtime[1] = cl.mtime[0] - 0.1f;
            f = 0.1f;
        }

        frac = (cl.time - cl.mtime[1]) / f;

        if (frac < 0)
        {
            if (frac < -0.01f)
            {
                SetPal(1);
                cl.time = cl.mtime[1];
            }

            frac = 0.0f;
        }
        else if (frac > 1)
        {
            if (frac > 1.01f)
            {
                SetPal(2);
                cl.time = cl.mtime[0];
            }

            frac = 1.0f;
        }
        else
        {
            SetPal(0);
        }

        return frac;
    }

    public void CL_RelinkEntities()
    {
        render_c.entity_t* ent;
        int i, j;
        float frac, f, d;
        Vector3 delta = null;
        float bobjrotate;
        Vector3 oldorg;
        client_c.dlight_t* dl;

        frac = CL_LerpPoint();

        cl_numvisedicts = 0;

        for (i = 0; i < 3; i++)
        {
            cl.velocity[i] = cl.mvelocity[1, i] + frac * (cl.mvelocity[0, i] - cl.mvelocity[1, i]);
        }

        if (cls.demoplayback)
        {
            for (j = 0; j < 3; j++)
            {
                d = cl.mviewangles[0, j] - cl.mviewangles[1, j];

                if (d > 180)
                {
                    d -= 360;
                }
                else if (d < -180)
                {
                    d += 360;
                }

                cl.viewangles[j] = cl.mviewangles[1, j] + frac * d;
            }
        }

        bobjrotate = mathlib_c.anglemod(100 * cl.time);

        for (i = 1, ent = cl_entities + 1; i < cl.num_entites; i++, ent++)
        {
            if (ent->model == null)
            {
                if (ent->forcelink)
                {
                    R_RemoveEfrags(ent);
                }

                continue;
            }

            if (ent->msgtime != cl.mtime[0])
            {
                ent->model = null;
                continue;
            }

            mathlib_c.VectorCopy(ent->origin, oldorg);

            if (ent->forcelink)
            {
                mathlib_c.VectorCopy(ent->msg_origins[0], ent->origin);
                mathlib_c.VectorCopy(ent->msg_angles[0], ent->angles);
            }
            else
            {
                f = frac;

                for (j = 0; j < 3; j++)
                {
                    delta[j] = ent->msg_origins[0][j] - ent->msg_origins[1][j];

                    if (delta[j] > 100 || delta[j] < -100)
                    {
                        f = 1;
                    }
                }

                for (j = 0; j < 3; j++)
                {
                    ent->origin[j] = ent->msg_origins[1][j] + f * delta[j];

                    d = ent->msg_angles[0][j] - ent->msg_angles[1][j];

                    if (d > 180)
                    {
                        d -= 360;
                    }
                    else if (d < -180)
                    {
                        d += 360;
                    }

                    ent->angles[j] = ent->msg_angles[1][j] + f * d;
                }
            }

            if (ent->model->flags & EF_ROTATE)
            {
                ent->angles[1] = bobjrotate;
            }

            if (ent->effects & EF_BRIGHTFIELD)
            {
                R_EntityParticles(ent);
            }

#if QUAKE2
            if (ent->effects & EF_DARKFIELD) 
            {
                R_DarkFieldParticles(ent);
            }
#endif

            if (ent->effects & EF_MUZZLEFLASH)
            {
                Vector3 fv, rv, uv;

                dl = CL_AllocDlight(i);
                mathlib_c.VectorCopy(ent->origin, dl->origin);
                dl->origin[2] += 16;
                mathlib_c.AngleVectors(ent->angles, fv, rv, uv);

                mathlib_c.VectorMA(dl->origin, 18, fv, dl->origin);
                dl->radius = 200 + (rand() & 31);
                dl->minlight = 32;
                dl->die = cl.time + 0.1f;
            }

            if (ent->effects & EF_BRIGHTLIGHT)
            {
                dl = CL_AllocDlight(i);
                mathlib_c.VectorCopy(ent->origin, dl->origin);
                dl->radius = 200 + (rand() & 31);
                dl->die = cl.time + 0.001f;
            }

#if QUAKE2
            if (ent->effects & EF_DARKLIGHT) 
            {
                dl = CL_AllocDlight(i);
                mathlib_c.VectorCopy(ent->origin, dl->origin);
                dl->radius = 200;
                dl->die = cl.time + 0.001f;
                dl->dark = true;
            }

            if (ent->effects & EF_LIGHT) 
            {
                dl = CL_AllocDlight(i);
                mathlib_c.VectorCopy(ent->origin, dl->origin);
                dl->radius = 200;
                dl->die = cl.time + 0.001f;
            }
#endif

            if (ent->model->flags & EF_GIB != 0)
            {
                R_RocketTrail(oldorg, ent->origin, 2);
            }
            else if (ent->model->flags & EF_ZOMGIB != 0)
            {
                R_RocketTrail(oldorg, ent->origin, 4);
            }
            else if (ent->model->flags & EF_TRACER != 0)
            {
                R_RocketTrail(oldorg, ent->origin, 3);
            }
            else if (ent->model->flags & EF_TRACER2 != 0)
            {
                R_RocketTrail(oldorg, ent->origin, 5);
            }
            else if (ent->model->flags & EF_ROCKET != 0)
            {
                R_RocketTrail(oldorg, ent->origin, 0);
                dl = CL_AllocDlight(i);
                mathlib_c.VectorCopy(ent->origin, dl->origin);
                dl->radius = 200;
                dl->die = cl.time + 0.01f;
            }
            else if (ent->model->flags & EF_GRENADE != 0)
            {
                R_RocketTrail(oldorg, ent->origin, 1);
            }
            else if (ent->model->flags & EF_TRACER3 != 0)
            {
                R_RocketTrail(oldorg, ent->origin, 6);
            }

            ent->forcelink = false;

            if (i == cl.viewentity && chase_active.value == 0)
            {
                continue;
            }

#if QUAKE2
            if (ent->effects & EF_NODRAW != 0) 
            {
                continue;
            }
#endif

            if (cl_numvisedicts < quakedef_c.MAX_EDICTS)
            {
                cl_visedicts[cl_numvisedicts] = ent;
                cl_numvisedicts++;
            }
        }
    }

    public int CL_ReadFromServer()
    {
        int ret;

        cl.oldtime = cl.time;
        cl.time += host_frametime;

        do
        {
            ret = CL_GetMessage();

            if (ret == -1)
            {
                Host_Error("CL_ReadFromServer: lost server connection");
            }

            if (ret == 0)
            {
                break;
            }

            cl.last_received_message = quakedef_c.realtime;
            CL_ParseServerMessage();
        } while (ret != 0 && cls.state == client_c.cactive_t.ca_connected);

        if (cl_shownet.value != 0)
        {
            console_c.Con_Printf("\n");
        }

        CL_RelinkEntities();
        cl_tent_c.CL_UpdateTEnts();

        return 0;
    }

    public void CL_SendCmd()
    {
        client_c.usercmd_t cmd;

        if (cls.state != client_c.cactive_t.ca_connected)
        {
            return;
        }

        if (cls.signon == client_c.SIGNONS)
        {
            CL_BaseMove(cmd);

            IN_Move(cmd);

            CL_SendMove(cmd);
        }

        if (cls.demoplayback)
        {
            common_c.SZ_Clear(cls.message);
            return;
        }

        if (cls.message.cursize == 0)
        {
            return;
        }

        if (NET_CanSendMessage(cls.netcon) == 0)
        {
            console_c.Con_DPrintf("CL_WriteToServer: can't send\n");
            return;
        }

        if (NET_SendMessage(cls.netcon, cls.message) == -1)
        {
            Host_Error("CL_WriteToServer: lost server connection");
        }

        common_c.SZ_Clear(cls.message);
    }

    public void CL_Init()
    {
        common_c.SZ_Alloc(cls.message, 1024);

        CL_InitInput();
        CL_InitTEnts();

        cvar_c.Cvar_RegisterVariable(cl_name);
        cvar_c.Cvar_RegisterVariable(cl_color);
        cvar_c.Cvar_RegisterVariable(cl_upspeed);
        cvar_c.Cvar_RegisterVariable(cl_forwardspeed);
        cvar_c.Cvar_RegisterVariable(cl_backspeed);
        cvar_c.Cvar_RegisterVariable(cl_sidespeed);
        cvar_c.Cvar_RegisterVariable(cl_movespeedkey);
        cvar_c.Cvar_RegisterVariable(cl_yawspeed);
        cvar_c.Cvar_RegisterVariable(cl_pitchspeed);
        cvar_c.Cvar_RegisterVariable(cl_anglespeedkey);
        cvar_c.Cvar_RegisterVariable(cl_shownet);
        cvar_c.Cvar_RegisterVariable(cl_nolerp);
        cvar_c.Cvar_RegisterVariable(lookspring);
        cvar_c.Cvar_RegisterVariable(lookstrafe);
        cvar_c.Cvar_RegisterVariable(sensitivity);

        cvar_c.Cvar_RegisterVariable(m_pitch);
        cvar_c.Cvar_RegisterVariable(m_yaw);
        cvar_c.Cvar_RegisterVariable(m_forward);
        cvar_c.Cvar_RegisterVariable(m_side);

        cmd_c.Cmd_AddCommand("entities", CL_PrintEntities_f);
        cmd_c.Cmd_AddCommand("disconnect", CL_Disconnect_f);
        cmd_c.Cmd_AddCommand("record", CL_Record_f);
        cmd_c.Cmd_AddCommand("stop", CL_Stop_f);
        cmd_c.Cmd_AddCommand("playdemo", CL_PlayDemo_f);
        cmd_c.Cmd_AddCommand("timedemo", CL_TimeDemo_f);
    }
}