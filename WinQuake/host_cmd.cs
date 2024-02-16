namespace Quake;

public unsafe class host_cmd_c
{
    public static cvar_c.cvar_t pausable;

    public static int current_skill;

    public static void Host_Quit_f()
    {
        if (keys_c.key_dest != keys_c.keydest_t.key_console && client_c.cls.state != client_c.cactive_t.ca_dedicated)
        {
            menu_c.M_Menu_Quit_f();
            return;
        }

        cl_main_c.CL_Disconnect();
        host_c.Host_ShutdownServer(false);

        sys_win_c.Sys_Quit();
    }

    public static void Host_Status_f()
    {
        server_c.client_t* client;
        int seconds;
        int minutes;
        int hours = 0;
        int j;
        Action<string, object[]> print;

        if (cmd_c.cmd_source == cmd_c.cmd_source_t.src_command)
        {
            if (!server_c.sv.active)
            {
                cmd_c.Cmd_ForwardToServer();
                return;
            }

            print = console_c.Con_Printf;
        }
        else
        {
            print = host_c.SV_ClientPrintf;
        }

        print($"host:    {*cvar_c.Cvar_VariableString("hostname")}\n", null);
        print($"version: {quakedef_c.VERSION}\n", null);

        if (net_main_c.tcpipAvailable)
        {
            print($"tcp/ip: {*net_main_c.my_tcpip_address}\n", null);
        }

        if (net_main_c.ipxAvailable)
        {
            print($"ipx: {*net_main_c.my_ipx_address}\n", null);
        }

        print($"map: {*server_c.sv.name}\n", null);
        print($"players: {net_main_c.net_activeconnections} active ({server_c.svs.maxclients} max)\n\n", null);

        for (j = 0, client = server_c.svs.clients; j < server_c.svs.maxclients; j++, client++)
        {
            if (!client->active)
            {
                continue;
            }

            seconds = (int)(net_main_c.net_time - client->netconnection->connecttime);
            minutes = seconds / 60;

            if (minutes != 0)
            {
                seconds -= (minutes * 60);
                hours = minutes / 60;

                if (hours != 0)
                {
                    minutes -= (hours * 60);
                }
            }
            else
            {
                hours = 0;
            }

            print($"#{j + 1} {*client->name} {(int)client->edict->v.frags} {hours}:{minutes}:{seconds}\n", null);
            print($"    {*client->netconnection->address}\n", null);
        }
    }

    public static void Host_God_f()
    {
        if (cmd_c.cmd_source == cmd_c.cmd_source_t.src_command)
        {
            cmd_c.Cmd_ForwardToServer();
            return;
        }

        if (pr_edict_c.pr_global_struct->deathmatch != 0 && !host_c.host_client->privileged)
        {
            return;
        }

        sv_user_c.sv_player->v.flags = (int)sv_user_c.sv_player->v.flags ^ server_c.FL_GODMODE;

        if (((int)sv_user_c.sv_player->v.flags & server_c.FL_GODMODE) == 0)
        {
            host_c.SV_ClientPrintf("godmode OFF\n");
        }
        else
        {
            host_c.SV_ClientPrintf("godmode ON\n");
        }
    }

    public static void Host_Notarget_f()
    {

    }
}