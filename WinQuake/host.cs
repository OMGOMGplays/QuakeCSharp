namespace Quake;

public unsafe class host_c
{
    public static quakedef_c.quakeparms_t host_parms;

    public static bool host_initialized;

    public static double host_frametime;
    public static double host_time;
    public static double realtime;
    public static double oldrealtime;
    public static int host_framecount;

    public static int host_hunklevel;

    public static int minimum_memory;

    public static server_c.client_t* host_client;

    public static byte* host_basepal;
    public static byte* host_colormap;

    public static cvar_c.cvar_t host_framerate = new cvar_c.cvar_t { name = "host_frametime", value = (char)0 };
    public static cvar_c.cvar_t host_speeds = new cvar_c.cvar_t { name = "host_speeds", value = (char)0 };

    public static cvar_c.cvar_t sys_ticrate = new cvar_c.cvar_t { name = "sys_ticrate", value = (char)0.05 };
    public static cvar_c.cvar_t serverprofile = new cvar_c.cvar_t { name = "serverprofile", value = (char)0 };

    public static cvar_c.cvar_t fraglimit = new cvar_c.cvar_t { name = "fraglimit", value = (char)0, archive = true };
    public static cvar_c.cvar_t timelimit = new cvar_c.cvar_t { name = "timelimit", value = (char)0, archive = true };
    public static cvar_c.cvar_t teamplay = new cvar_c.cvar_t { name = "teamplay", value = (char)0, archive = true };

    public static cvar_c.cvar_t samelevel = new cvar_c.cvar_t { name = "samelevel", value = (char)0 };
    public static cvar_c.cvar_t noexit = new cvar_c.cvar_t { name = "noexit", value = (char)0, archive = true };

#if QUAKE2
    public cvar_c.cvar_t developer = new cvar_c.cvar_t {name = "developer", value = (char)1};
#else
    public static cvar_c.cvar_t developer = new cvar_c.cvar_t { name = "developer", value = (char)0 };
#endif

    public static cvar_c.cvar_t skill = new cvar_c.cvar_t { name = "skill", value = (char)1 };
    public static cvar_c.cvar_t deathmatch = new cvar_c.cvar_t { name = "deathmatch", value = (char)0 };
    public static cvar_c.cvar_t coop = new cvar_c.cvar_t { name = "coop", value = (char)0 };

    public static cvar_c.cvar_t pausable = new cvar_c.cvar_t { name = "pausable", value = (char)1 };

    public static cvar_c.cvar_t temp1 = new cvar_c.cvar_t { name = "temp1", value = (char)0 };

    public static void Host_EndGame(char* message, params object[] args)
    {
        char[] str = new char[1024];

        Console.WriteLine($"{str} {message->ToString()}");
        console_c.Con_DPrintf($"Host_EndGame: {message->ToString()}\n");

        if (server_c.sv.active)
        {
            Host_ShutdownServer(false);
        }

        if (client_c.cls.state == client_c.cactive_t.ca_dedicated)
        {
            sys_win_c.Sys_Error($"Host_EndGame: {str}\n");
        }

        if (client_c.cls.demonum != -1)
        {
            cl_demo_c.CL_NextDemo();
        }
        else
        {
            cl_main_c.CL_Disconnect();
        }
    }

    public static void Host_EndGame(string message, params object[] args)
    {
        Host_EndGame(common_c.StringToChar(message));
    }

    public static void Host_Error(char* error, params object[] args)
    {
        char[] str = new char[1024];
        bool inerror = false;

        if (inerror)
        {
            sys_win_c.Sys_Error("Host_Error: recursively entered");
        }

        inerror = true;

        screen_c.SCR_EndLoadingPlaque();

        Console.WriteLine($"{str} {error->ToString()}");
        console_c.Con_Printf($"Host_Error: {error->ToString()}\n");

        if (server_c.sv.active)
        {
            Host_ShutdownServer(false);
        }

        if (client_c.cls.state == client_c.cactive_t.ca_dedicated)
        {
            sys_win_c.Sys_Error($"Host_Error: {error->ToString()}");
        }

        cl_main_c.CL_Disconnect();
        client_c.cls.demonum = -1;

        inerror = false;
    }

    public static void Host_Error(string error, params object[] args)
    {
        Host_Error(common_c.StringToChar(error), args);
    }

    public static void Host_FindMaxClients()
    {
        int i;

        server_c.svs.maxclients = 1;

        i = common_c.COM_CheckParm("-dedicated");

        if (i != 0)
        {
            client_c.cls.state = client_c.cactive_t.ca_dedicated;

            if (i != (common_c.com_argc - 1))
            {
                server_c.svs.maxclients = common_c.Q_atoi(common_c.com_argv[i + 1].ToString());
            }
            else
            {
                server_c.svs.maxclients = 8;
            }
        }
        else
        {
            client_c.cls.state = client_c.cactive_t.ca_disconnected;
        }

        i = common_c.COM_CheckParm("-listen");

        if (i != 0)
        {
            if (client_c.cls.state == client_c.cactive_t.ca_dedicated)
            {
                sys_win_c.Sys_Error("Only one of -dedicated or -listen can be specified");
            }

            if (i != (common_c.com_argc - 1))
            {
                server_c.svs.maxclients = common_c.Q_atoi(common_c.com_argv[i + 1].ToString());
            }
            else
            {
                server_c.svs.maxclients = 8;
            }
        }

        if (server_c.svs.maxclients < 1)
        {
            server_c.svs.maxclients = 8;
        }
        else if (server_c.svs.maxclients > quakedef_c.MAX_SCOREBOARD)
        {
            server_c.svs.maxclients = quakedef_c.MAX_SCOREBOARD;
        }

        server_c.svs.maxclientslimit = server_c.svs.maxclients;

        if (server_c.svs.maxclientslimit < 4)
        {
            server_c.svs.maxclientslimit = 4;
        }

        server_c.svs.clients = (server_c.client_t*)zone_c.Hunk_AllocName(server_c.svs.maxclientslimit * sizeof(server_c.client_t), "clients");

        if (server_c.svs.maxclients > 1)
        {
            cvar_c.Cvar_SetValue("deathmatch", 1.0f);
        }
        else
        {
            cvar_c.Cvar_SetValue("deathmatch", 0.0f);
        }
    }

    public static void Host_InitLocal()
    {
        host_cmd_c.Host_InitCommands();

        cvar_c.Cvar_RegisterVariable(host_framerate);
        cvar_c.Cvar_RegisterVariable(host_speeds);

        cvar_c.Cvar_RegisterVariable(sys_ticrate);
        cvar_c.Cvar_RegisterVariable(serverprofile);

        cvar_c.Cvar_RegisterVariable(fraglimit);
        cvar_c.Cvar_RegisterVariable(timelimit);
        cvar_c.Cvar_RegisterVariable(teamplay);
        cvar_c.Cvar_RegisterVariable(samelevel);
        cvar_c.Cvar_RegisterVariable(noexit);
        cvar_c.Cvar_RegisterVariable(skill);
        cvar_c.Cvar_RegisterVariable(developer);
        cvar_c.Cvar_RegisterVariable(deathmatch);
        cvar_c.Cvar_RegisterVariable(coop);

        cvar_c.Cvar_RegisterVariable(pausable);

        cvar_c.Cvar_RegisterVariable(temp1);

        Host_FindMaxClients();

        host_time = 1.0;
    }

    public static void Host_WriteConfiguration()
    {
        if (host_initialized && !sys_win_c.isDedicated)
        {
            string configFilePath = Path.Combine(common_c.com_gamedir, "config.cfg");

            try
            {
                using (StreamWriter writer = new StreamWriter(configFilePath))
                {
                    keys_c.Key_WriteBindings(writer);
                    cvar_c.Cvar_WriteVariables(writer);
                }

                console_c.Con_Printf("Config.cfg written successfully");
            }
            catch (IOException ex)
            {
                console_c.Con_Printf($"Couldn't write config.cfg: {ex.Message}");
            }
        }
    }

    public static void SV_ClientPrintf(char* fmt, params object[] args)
    {
        Console.WriteLine(fmt->ToString());

        common_c.MSG_WriteByte(host_client->message, protocol_c.svc_print);
        common_c.MSG_WriteString(host_client->message, *fmt);
    }

    public static void SV_ClientPrintf(string fmt, params object[] args)
    {
        SV_ClientPrintf(common_c.StringToChar(fmt));
    }

    public static void SV_BroadcastPrintf(char* fmt, params object[] args)
    {
        int i;

        Console.WriteLine(fmt->ToString());

        for (i = 0; i < server_c.svs.maxclients; i++)
        {
            if (server_c.svs.clients[i].active && server_c.svs.clients[i].spawned)
            {
                common_c.MSG_WriteByte(server_c.svs.clients[i].message, protocol_c.svc_print);
                common_c.MSG_WriteString(server_c.svs.clients[i].message, *fmt);
            }
        }
    }

    public static void Host_ClientCommands(char* fmt, params object[] args)
    {
        Console.WriteLine(fmt->ToString());

        common_c.MSG_WriteByte(host_client->message, protocol_c.svc_stufftext);
        common_c.MSG_WriteString(host_client->message, *fmt);
    }

    public static void Host_ClientCommands(string fmt, params object[] args)
    {
        Host_ClientCommands(common_c.StringToChar(fmt));
    }

    public static void SV_DropClient(bool crash)
    {
        int saveSelf;
        int i;
        server_c.client_t* client;

        if (!crash)
        {
            if (net_main_c.NET_CanSendMessage(host_client->netconnection))
            {
                common_c.MSG_WriteByte(host_client->message, protocol_c.svc_disconnect);
                net_main_c.NET_SendMessage(host_client->netconnection, &host_client->message);
            }

            if (host_client->edict != null && host_client->spawned)
            {
                saveSelf = progs_c.pr_global_struct->self;
                progs_c.pr_global_struct->self = progs_c.EDICT_TO_PROG(host_client->edict);
                pr_exec_c.PR_ExecuteProgram(progs_c.pr_global_struct->ClientDisconnect);
                progs_c.pr_global_struct->self = saveSelf;
            }

            sys_win_c.Sys_Printf($"Client {host_client->name->ToString()} removed\n");
        }

        net_main_c.NET_Close(host_client->netconnection);
        host_client->netconnection = null;

        host_client->active = false;
        host_client->name[0] = (char)0;
        host_client->old_frags = -999999;
        net_main_c.net_activeconnections--;

        for (i = 0, client = server_c.svs.clients; i < server_c.svs.maxclients; i++, client++)
        {
            if (!client->active)
            {
                continue;
            }

            common_c.MSG_WriteByte(client->message, protocol_c.svc_updatename);
            common_c.MSG_WriteByte(client->message, (int)(host_client - server_c.svs.clients));
            common_c.MSG_WriteString(client->message, *common_c.StringToChar(""));
            common_c.MSG_WriteByte(client->message, protocol_c.svc_updatefrags);
            common_c.MSG_WriteByte(client->message, (int)(host_client - server_c.svs.clients));
            common_c.MSG_WriteShort(client->message, 0);
            common_c.MSG_WriteByte(client->message, protocol_c.svc_updatecolors);
            common_c.MSG_WriteByte(client->message, (int)(host_client - server_c.svs.clients));
            common_c.MSG_WriteByte(client->message, 0);
        }
    }

    public static void Host_ShutdownServer(bool crash)
    {
        int i;
        int count;
        common_c.sizebuf_t buf = null;
        char* message = null;
        double start;

        if (!server_c.sv.active)
        {
            return;
        }

        server_c.sv.active = false;

        if (client_c.cls.state == client_c.cactive_t.ca_connected)
        {
            cl_main_c.CL_Disconnect();
        }

        start = sys_win_c.Sys_FloatTime();

        do
        {
            count = 0;

            for (i = 0, host_client = server_c.svs.clients; i < server_c.svs.maxclients; i++, host_client++)
            {
                if (host_client->active && host_client->message.cursize != 0)
                {
                    if (net_main_c.NET_CanSendMessage(host_client->netconnection))
                    {
                        net_main_c.NET_SendMessage(host_client->netconnection, &host_client->message);
                        common_c.SZ_Clear(host_client->message);
                    }
                    else
                    {
                        net_main_c.NET_GetMessage(host_client->netconnection);
                        count++;
                    }
                }
            }

            if ((sys_win_c.Sys_FloatTime() - start) > 3.0)
            {
                break;
            }
        } while (count != 0);

        buf.data = (int*)message;
        buf.maxsize = 4;
        buf.cursize = 0;
        common_c.MSG_WriteByte(buf, protocol_c.svc_disconnect);
        count = net_main_c.NET_SendToAll(&buf, 5);

        if (count != 0)
        {
            console_c.Con_Printf($"Host_ShutdownServer: NET_SendToAll failed for {count} clients\n");
        }

        for (i = 0, host_client = server_c.svs.clients; i < server_c.svs.maxclients; i++, host_client++)
        {
            if (host_client->active)
            {
                SV_DropClient(crash);
            }
        }

        common_c.Q_memset(server_c.sv, 0, sizeof(server_c.server_t));
        common_c.Q_memset(*server_c.svs.clients, 0, server_c.svs.maxclientslimit * sizeof(server_c.client_t));
    }

    public static void Host_ClearMemory()
    {
        console_c.Con_DPrintf("Clearing memory\n");
        d_surf_c.D_FlushCaches();
        model_c.Mod_ClearAll();

        if (host_hunklevel != 0)
        {
            zone_c.Hunk_FreeToLowMark(host_hunklevel);
        }

        client_c.cls.signon = 0;
        common_c.Q_memset(server_c.sv, 0, sizeof(server_c.server_t));
        common_c.Q_memset(cl_main_c.cl, 0, sizeof(client_c.client_state_t));
    }

    public static bool Host_FilterTime(float time)
    {
        realtime += time;

        if (!client_c.cls.timedemo && realtime - oldrealtime < 1.0 / 72.0)
        {
            return false;
        }

        host_frametime = realtime - oldrealtime;
        oldrealtime = realtime;

        if (host_framerate.value > 0)
        {
            host_frametime = host_framerate.value;
        }
        else
        {
            if (host_frametime > 0.1)
            {
                host_frametime = 0.1;
            }

            if (host_frametime < 0.001)
            {
                host_frametime = 0.001;
            }
        }

        return true;
    }

    public static void Host_GetConsoleCommands()
    {
        char* cmd;

        while (true)
        {
            cmd = sys_win_c.Sys_ConsoleInput();

            if (cmd == null)
            {
                break;
            }

            cmd_c.Cbuf_AddText(cmd);
        }
    }

#if FPS_20
    public static void _Host_ServerFrame()
    {
        progs_c.pr_global_struct->frametime = host_frametime;

        sv_user_c.SV_RunClients();

        if (!server_c.sv.paused && (server_c.svs.maxclients > 1 || keys_c.key_dest == keys_c.keydest_t.key_game))
        {
            sv_phys_c.SV_Physics();
        }
    }
    
    public static void Host_ServerFrame()
    {
        float save_host_frametime;
        float temp_host_frametime;

        progs_c.pr_global_struct->frametime = (float)host_frametime;

        sv_main_c.SV_ClearDatagram();

        sv_main_c.SV_CheckForNewClients();

        temp_host_frametime = save_host_frametime = host_frametime;

        while (temp_host_frametime>(1.0/72.0))
        {
            if (temp_host_frametime > 0.05)
            {
                host_frametime = 0.05;
            }
            else
            {
                host_frametime = temp_host_frametime;
            }

            temp_host_frametime -= host_frametime;
            _Host_ServerFrame();
        }

        host_frametime = save_host_frametime;

        sv_main_c.SV_SendClientMessages();
    }

#else

    public static void Host_ServerFrame()
    {
        progs_c.pr_global_struct->frametime = (float)host_frametime;

        sv_main_c.SV_ClearDatagram();

        sv_main_c.SV_CheckForNewClients();

        sv_user_c.SV_RunClients();

        if (!sv_main_c.sv.paused && (server_c.svs.maxclients > -1 || keys_c.key_dest == keys_c.keydest_t.key_game))
        {
            sv_phys_c.SV_Physics();
        }

        sv_main_c.SV_SendClientMessages();
    }

#endif

    public static void _Host_Frame(float time)
    {
        double time1 = 0;
        double time2 = 0;
        double time3 = 0;
        int pass1, pass2, pass3;

        rand_c.rand();

        if (!Host_FilterTime(time))
        {
            return;
        }

        sys_win_c.Sys_SendKeyEvents();

        in_win_c.IN_Commands();

        cmd_c.Cbuf_Execute();

        net_main_c.NET_Poll();

        if (sv_main_c.sv.active)
        {
            cl_main_c.CL_SendCmd();
        }

        Host_GetConsoleCommands();

        if (sv_main_c.sv.active)
        {
            Host_ServerFrame();
        }

        if (!sv_main_c.sv.active)
        {
            cl_main_c.CL_SendCmd();
        }

        host_time += host_frametime;

        if (cl_main_c.cls.state == client_c.cactive_t.ca_connected)
        {
            cl_main_c.CL_ReadFromServer();
        }

        if (host_speeds.value != 0)
        {
            time1 = sys_win_c.Sys_FloatTime();
        }

        screen_c.SCR_UpdateScreen();

        if (host_speeds.value != 0)
        {
            time2 = sys_win_c.Sys_FloatTime();
        }

        if (cl_main_c.cls.signon == client_c.SIGNONS)
        {
            snd_null_c.S_Update(render_c.r_origin, render_c.vpn, render_c.vright, render_c.vup);
            cl_main_c.CL_DecayLights();
        }
        else
        {
            snd_null_c.S_Update(mathlib_c.vec3_origin, mathlib_c.vec3_origin, mathlib_c.vec3_origin, mathlib_c.vec3_origin);
        }

        cd_win_c.CDAudio_Update();

        if (host_speeds.value != 0)
        {
            pass1 = (int)(time1 - time3) * 1000;
            time3 = sys_win_c.Sys_FloatTime();
            pass2 = (int)(time2 - time1) * 1000;
            pass3 = (int)(time3 - time2) * 100;
            console_c.Con_Printf($"{pass1 + pass2 + pass3} tot {pass1} server {pass2} gfx {pass3} snd\n");
        }

        host_framecount++;
    }

    public static void Host_Frame(float time)
    {
        double time1, time2;
        double timetotal = 0;
        int timecount = 0;
        int i, c, m;

        if (serverprofile.value == 0)
        {
            _Host_Frame(time);
            return;
        }

        time1 = sys_win_c.Sys_FloatTime();
        _Host_Frame(time);
        time2 = sys_win_c.Sys_FloatTime();

        timetotal += time2 - time1;
        timecount++;

        if (timecount < 1000)
        {
            return;
        }

        m = (int)timetotal * 1000 / timecount;
        timecount = 0;
        timetotal = 0;
        c = 0;

        for (i = 0; i < server_c.svs.maxclients; i++)
        {
            if (server_c.svs.clients[i].active)
            {
                c++;
            }
        }

        console_c.Con_Printf($"serverprofile: {c} clients {m} msec\n");
    }

    public static int vcrFile;
    public static int VCR_SIGNATURE = 0x56435231;

    public static void Host_InitVCR(quakedef_c.quakeparms_t* parms)
    {
        int i, len, n;
        char* p;

        if (common_c.COM_CheckParm("-playback") != 0)
        {
            if (common_c.com_argc != 2)
            {
                sys_win_c.Sys_Error("No other parameters allowed with -playback\n");
            }

            sys_win_c.Sys_FileOpenRead(common_c.StringToChar("quake.vcr"), vcrFile);

            if (vcrFile == -1)
            {
                sys_win_c.Sys_Error("playback file not found\n");
            }

            sys_win_c.Sys_FileRead(vcrFile, (byte*)&i, sizeof(int));

            if (i != VCR_SIGNATURE)
            {
                sys_win_c.Sys_Error("Invalid signature in vcr file\n");
            }

            sys_win_c.Sys_FileRead(vcrFile, (byte*)&common_c.com_argc, sizeof(int));
            common_c.com_argv = (char*)zone_c.Z_Malloc(common_c.com_argc * sizeof(char*));
            common_c.com_argv[0] = parms->argv[0];

            for (i = 0; i < common_c.com_argc; i++)
            {
                sys_win_c.Sys_FileRead(vcrFile, (byte*)&len, sizeof(int));
                p = (char*)zone_c.Z_Malloc(len);
                sys_win_c.Sys_FileRead(vcrFile, (byte*)p, len);
                common_c.com_argv[i + 1] = *p;
            }

            common_c.com_argc++;
            parms->argc = common_c.com_argc;
            parms->argv = common_c.com_argv;
        }

        if ((n = common_c.COM_CheckParm("-record")) != 0)
        {
            vcrFile = sys_win_c.Sys_FileOpenRead(common_c.StringToChar("quake.vcr"));

            i = VCR_SIGNATURE;
            sys_win_c.Sys_FileWrite(vcrFile, &i, sizeof(int));
            i = common_c.com_argc - 1;
            sys_win_c.Sys_FileWrite(vcrFile, &i, sizeof(int));

            for (i = 1; i < common_c.com_argc; i++)
            {
                if (i == n)
                {
                    len = 10;
                    sys_win_c.Sys_FileWrite(vcrFile, &len, sizeof(int));
                    sys_win_c.Sys_FileWrite(vcrFile, &common_c.com_argv[i], len);
                    continue;
                }

                len = common_c.Q_strlen(common_c.com_argv[i].ToString()) + 1;
                sys_win_c.Sys_FileWrite(vcrFile, &len, sizeof(int));
                sys_win_c.Sys_FileWrite(vcrFile, &common_c.com_argv[i], len);
            }
        }
    }

    public static void Host_Init(quakedef_c.quakeparms_t* parms)
    {
        if (common_c.standard_quake)
        {
            minimum_memory = quakedef_c.MINIMUM_MEMORY;
        }
        else
        {
            minimum_memory = quakedef_c.MINIMUM_MEMORY_LEVELPAK;
        }

        if (common_c.COM_CheckParm("-minmemory") != 0)
        {
            parms->memsize = minimum_memory;
        }

        host_parms = *parms;

        if (parms->memsize < minimum_memory)
        {
            sys_win_c.Sys_Error($"Only {parms->memsize / (float)0x100000} megs of memory available, can't execute game");
        }

        common_c.com_argc = parms->argc;
        common_c.com_argv = parms->argv;

        zone_c.Memory_Init(parms->membase, parms->memsize);
        cmd_c.Cbuf_Init();
        cmd_c.Cmd_Init();
        view_c.V_Init();
        chase_c.Chase_Init();
        Host_InitVCR(parms);
        common_c.COM_Init(common_c.StringToChar(parms->basedir));
        Host_InitLocal();
        wad_c.W_LoadWadFile("gfx.wad");
        keys_c.Key_Init();
        console_c.Con_Init();
        menu_c.M_Init();
        pr_edict_c.PR_Init();
        model_c.Mod_Init();
        net_main_c.NET_Init();
        sv_main_c.SV_Init();

        console_c.Con_Printf("Exe: " + __TIME__ + " " + __DATE__ + "\n");
        console_c.Con_Printf($"{parms->memsize / (1024 * 1024)} megabyte heap\n");

        r_main_c.R_InitTextures();

        if (client_c.cls.state != client_c.cactive_t.ca_dedicated)
        {
            host_basepal = (byte*)common_c.COM_LoadHunkFile("gfx/palette.lmp");

            if (host_basepal == null)
            {
                sys_win_c.Sys_Error("Couldn't load gfx/palette.lmp");
            }

            host_colormap = (byte*)common_c.COM_LoadHunkFile("gfx/colormap.lmp");

            if (host_colormap == null)
            {
                sys_win_c.Sys_Error("Couldn't load gfx/colormap.lmp");
            }

#if !_WIN32
            in_null_c.IN_Init();
#endif

            vid_win_c.VID_Init(host_basepal);

            draw_c.Draw_Init();
            screen_c.SCR_Init();
            r_main_c.R_Init();

#if !_WIN32
            snd_null_c.S_Init();
#elif GLQUAKE
            snd_null_c.S_Init();
#endif

            cd_win_c.CDAudio_Init();
            sbar_c.Sbar_Init();
            cl_main_c.CL_Init();
#if _WIN32
            in_null_c.IN_Init();
#endif
        }

        cmd_c.Cbuf_InsertText(common_c.StringToChar("exec quake.rc\n"));

        zone_c.Hunk_AllocName(0, "-HOST_HUNKLEVEL-");
        host_hunklevel = zone_c.Hunk_LowMark();

        host_initialized = true;

        sys_win_c.Sys_Printf("========Quake Initialized=========\n");
    }

    public static void Host_Shutdown() 
    {
        bool isdown = false;

        if (isdown) 
        {
            Console.WriteLine("recursive shutdown");
            return;
        } 

        isdown = true;

        screen_c.scr_disabled_for_loading = true;

        Host_WriteConfiguration();

        cd_win_c.CDAudio_Shutdown();
        net_main_c.NET_Shutdown();
        snd_null_c.S_Shutdown();
        in_null_c.IN_Shutdown();

        if (cl_main_c.cls.state != client_c.cactive_t.ca_dedicated) 
        {
            vid_null_c.VID_Shutdown();
        }
    }
}