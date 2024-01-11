namespace Quake;

public unsafe class net_main_c
{
    public static net_c.qsocket_t* net_activeSockets = null;
    public static net_c.qsocket_t* net_freeSockets = null;
    public static int net_numsockets = 0;

    public static bool serialAvailable = false;
    public static bool ipxAvailable = false;
    public static bool tcpipAvailable = false;

    public static int net_hostport;
    public static int DEFAULTnet_hostport = 26000;

    public static char* my_ipx_address;
    public static char* my_tcpip_address;

    public static Action<int, int, int, int, bool> GetComPortConfig { get; set; }
    public static Action<int, int, int, int, bool> SetComPortConfig { get; set; }
    public static Action<int, char, char, char, char> GetModemConfig { get; set; }
    public static Action<int, char, char, char, char> SetModemConfig { get; set; }

    public static bool listening = false;

    public static bool slistInProgress = false;
    public static bool slistSilent = false;
    public static bool slistLocal = true;
    public static double slistStartTime;
    public static int slistLastShown;

    public static net_c.PollProcedure slistSendProcedure = new net_c.PollProcedure { next = null, nextTime = 0.0, procedure = Slist_Send };
    public static net_c.PollProcedure slistPollProcedure = new net_c.PollProcedure { next = null, nextTime = 0.0, procedure = Slist_Poll };

    public static common_c.sizebuf_t net_message;
    public static int net_activeconnections = 0;

    public static int messagesSent = 0;
    public static int messagesReceived = 0;
    public static int unreliableMessagesSent = 0;
    public static int unreliableMessagesReceived = 0;

    public static cvar_c.cvar_t net_messagetimeout = new cvar_c.cvar_t { name = "net_messagetimeout", value = (char)300 };
    public static cvar_c.cvar_t hostname = new cvar_c.cvar_t { name = "hostname", value = *common_c.StringToChar("UNNAMED") };

    public static bool configRestored = false;
    public static cvar_c.cvar_t config_com_port = new cvar_c.cvar_t { name = "_config_com_port", value = (char)0x3f8, server = true };
    public static cvar_c.cvar_t config_com_irq = new cvar_c.cvar_t { name = "_config_com_irq", value = (char)4, server = true };
    public static cvar_c.cvar_t config_com_baud = new cvar_c.cvar_t { name = "_config_com_baud", value = (char)57600, server = true };
    public static cvar_c.cvar_t config_com_modem = new cvar_c.cvar_t { name = "_config_com_modem", value = (char)1, server = true };
    public static cvar_c.cvar_t config_modem_dialtype = new cvar_c.cvar_t { name = "_config_modem_dialtype", value = *common_c.StringToChar("T"), server = true };
    public static cvar_c.cvar_t config_modem_clear = new cvar_c.cvar_t { name = "_config_modem_clear", value = *common_c.StringToChar("ATZ"), server = true };
    public static cvar_c.cvar_t config_modem_init = new cvar_c.cvar_t { name = "_config_modem_init", value = *common_c.StringToChar(""), server = true };
    public static cvar_c.cvar_t config_modem_hangup = new cvar_c.cvar_t { name = "_config_modem_hangup", value = *common_c.StringToChar("AT H"), server = true };

#if IDGODS
    public static cvar_c.cvar_t idgods = new cvar_c.cvar_t {name = "idgods", value = (char)0};
#endif

    public static int vcrFile = -1;
    public static bool recording = false;

    public static net_c.net_driver_t sfunc
    {
        get { return net_c.net_drivers[net_c.sock->driver]; }
    }

    public static net_c.net_driver_t dfunc
    {
        get { return net_c.net_drivers[net_driverlevel]; }
    }

    public static int net_driverlevel;

    public static double net_time;

    public static double SetNetTime()
    {
        net_time = sys_win_c.Sys_FloatTime();
        return net_time;
    }

    public static net_c.qsocket_t* NET_NewQSocket()
    {
        net_c.qsocket_t* sock;

        if (net_freeSockets == null)
        {
            return null;
        }

        if (net_activeconnections >= server_c.svs.maxclients)
        {
            return null;
        }

        sock = net_freeSockets;
        net_freeSockets = sock->next;

        sock->next = net_activeSockets;
        net_activeSockets = sock;

        sock->disconnected = false;
        sock->connecttime = net_time;
        common_c.Q_strcpy(sock->address->ToString(), "UNSET ADDRESS");
        sock->driver = net_driverlevel;
        sock->socket = 0;
        sock->driverdata = null;
        sock->canSend = true;
        sock->sendNext = false;
        sock->lastMessageTime = net_time;
        sock->ackSequence = 0;
        sock->sendSequence = 0;
        sock->unreliableSendSequence = 0;
        sock->sendMessageLength = 0;
        sock->receiveSequence = 0;
        sock->unreliableReceiveSequence = 0;
        sock->receiveMessageLength = 0;

        return sock;
    }

    public static void NET_FreeQSocket(net_c.qsocket_t* sock)
    {
        net_c.qsocket_t* s;

        if (sock == net_activeSockets)
        {
            net_activeSockets = net_activeSockets->next;
        }
        else
        {
            for (s = net_activeSockets; s != null; s = s->next)
            {
                if (s->next == sock)
                {
                    s->next = sock->next;
                    break;
                }
            }

            if (s == null)
            {
                sys_win_c.Sys_Error(common_c.StringToChar("NET_FreeQSocket: not active\n"));
            }
        }

        sock->next = net_freeSockets;
        net_freeSockets = sock;
        sock->disconnected = true;
    }

    public static void NET_Listen_f()
    {
        if (cmd_c.Cmd_Argc() != 2)
        {
            console_c.Con_Printf(common_c.StringToChar($"\"listen\" is \"{(listening == true ? 1 : 0)}\"\n"));
            return;
        }

        listening = common_c.Q_atoi(cmd_c.Cmd_Argv(1)->ToString()) == 1 ? true : false;

        for (net_driverlevel = 0; net_driverlevel < net_c.net_numdrivers; net_driverlevel++)
        {
            if (net_c.net_drivers[net_driverlevel].initialized == false)
            {
                continue;
            }

            dfunc.Listen(listening);
        }
    }

    public static void MaxPlayers_f()
    {
        int n;

        if (cmd_c.Cmd_Argc() != 2)
        {
            console_c.Con_Printf(common_c.StringToChar($"\"maxplayers\" is \"{server_c.svs.maxclients}\"\n"));
            return;
        }

        if (server_c.sv.active)
        {
            console_c.Con_Printf(common_c.StringToChar("maxplayers can not be changed while a server is running.\n"));
            return;
        }

        n = common_c.Q_atoi(cmd_c.Cmd_Argv(1)->ToString());

        if (n < 1)
        {
            n = 1;
        }

        if (n > server_c.svs.maxclientslimit)
        {
            n = server_c.svs.maxclientslimit;
            console_c.Con_Printf(common_c.StringToChar($"\"maxplayers\" set to \"{n}\"\n"));
        }

        if ((n == 1) && listening)
        {
            cmd_c.Cbuf_AddText(common_c.StringToChar("listen 0\n"));
        }

        if ((n > 1) && (!listening))
        {
            cmd_c.Cbuf_AddText(common_c.StringToChar("listen 1\n"));
        }

        server_c.svs.maxclients = n;

        if (n == 1)
        {
            cvar_c.Cvar_Set("deatmatch", "0");
        }
        else
        {
            cvar_c.Cvar_Set("deathmatch", "1");
        }
    }

    public static void NET_Port_f()
    {
        int n;

        if (cmd_c.Cmd_Argc() != 2)
        {
            console_c.Con_Printf(common_c.StringToChar($"\"port\" is \"{net_hostport}\"\n"));
            return;
        }

        n = common_c.Q_atoi(cmd_c.Cmd_Argv(1)->ToString());

        if (n < 1 || n > 65534)
        {
            console_c.Con_Printf(common_c.StringToChar("Bad value, must be between 1 and 65534\n"));
            return;
        }

        DEFAULTnet_hostport = n;
        net_hostport = n;

        if (listening)
        {
            cmd_c.Cbuf_AddText(common_c.StringToChar("listen 0\n"));
            cmd_c.Cbuf_AddText(common_c.StringToChar("listen 1\n"));
        }
    }

    public static void PrintSlistHeader()
    {
        console_c.Con_Printf(common_c.StringToChar("Server          Map             Users\n"));
        console_c.Con_Printf(common_c.StringToChar("--------------- --------------- -----\n"));
        slistLastShown = 0;
    }

    public static void PrintSlist()
    {
        int n;

        for (n = slistLastShown; n < net_c.hostCacheCount; n++)
        {
            if (net_c.hostcache[n].maxusers != 0)
            {
                console_c.Con_Printf(common_c.StringToChar($"{net_c.hostcache[n].name->ToString()} {net_c.hostcache[n].map->ToString()} {net_c.hostcache[n].users}/{net_c.hostcache[n].maxusers}\n"));
            }
            else
            {
                console_c.Con_Printf(common_c.StringToChar($"{net_c.hostcache[n].name->ToString()} {net_c.hostcache[n].map->ToString()}"));
            }
        }

        slistLastShown = n;
    }

    public static void PrintSlistTrailer()
    {
        if (net_c.hostCacheCount != 0)
        {
            console_c.Con_Printf(common_c.StringToChar("== end list ==\n\n"));
        }
        else
        {
            console_c.Con_Printf(common_c.StringToChar("No Quake servers found.\n\n"));
        }
    }

    public static void NET_Slist_f()
    {
        if (slistInProgress)
        {
            return;
        }

        if (!slistSilent)
        {
            console_c.Con_Printf("Looking for Quake servers...\n");
            PrintSlistHeader();
        }

        slistInProgress = true;
        slistStartTime = sys_win_c.Sys_FloatTime();

        SchedulePollProcedure(slistSendProcedure, 0.0);
        SchedulePollProcedure(slistPollProcedure, 0.1);

        net_c.hostCacheCount = 0;
    }

    public static void Slist_Send()
    {
        for (net_driverlevel = 0; net_driverlevel < net_c.net_numdrivers; net_driverlevel++)
        {
            if (!slistLocal && net_driverlevel == 0)
            {
                continue;
            }

            if (net_c.net_drivers[net_driverlevel].initialized == false)
            {
                continue;
            }

            dfunc.SearchForHosts(true);
        }

        if ((sys_win_c.Sys_FloatTime() - slistStartTime) < 0.5)
        {
            SchedulePollProcedure(slistSendProcedure, 0.75);
        }
    }

    public static void Slist_Poll()
    {
        for (net_driverlevel = 0; net_driverlevel < net_c.net_numdrivers; net_driverlevel++)
        {
            if (!slistLocal && net_driverlevel == 0)
            {
                continue;
            }

            if (net_c.net_drivers[net_driverlevel].initialized == false)
            {
                continue;
            }

            dfunc.SearchForHosts(false);
        }

        if (!slistSilent)
        {
            PrintSlist();
        }

        if ((sys_win_c.Sys_FloatTime() - slistStartTime) < 1.5)
        {
            SchedulePollProcedure(slistPollProcedure, 0.1);
            return;
        }

        if (!slistSilent)
        {
            PrintSlistTrailer();
        }

        slistInProgress = false;
        slistSilent = false;
        slistLocal = true;
    }

    public static int hostCacheCount = 0;
    public static net_c.hostcache_t* hostcache;

    public static net_c.qsocket_t* NET_Connect(char* host)
    {
        net_c.qsocket_t* ret = null;
        int n;
        int numdrivers = net_c.net_numdrivers;

        SetNetTime();

        if (host != null && *host == 0)
        {
            host = null;
        }

        if (host != null)
        {
            if (common_c.Q_strcasecmp(host->ToString(), "local") == 0)
            {
                numdrivers = 1;
                goto JustDoIt;
            }

            if (hostCacheCount != 0)
            {
                for (n = 0; n < hostCacheCount; n++)
                {
                    if (common_c.Q_strcasecmp(host->ToString(), hostcache[n].name->ToString()) == 0)
                    {
                        host = hostcache[n].cname;
                        break;
                    }
                }

                if (n < hostCacheCount)
                {
                    goto JustDoIt;
                }
            }
        }

        slistSilent = host != null ? true : false;
        NET_Slist_f();

        while (slistInProgress)
        {
            NET_Poll();
        }

        if (host == null)
        {
            if (hostCacheCount != 1)
            {
                return null;
            }

            host = hostcache[0].cname;
            console_c.Con_Printf($"Connecting to...{hostcache[0].name->ToString()}\n @ {host->ToString()}\n\n");
        }

        if (hostCacheCount != 0)
        {
            for (n = 0; n < hostCacheCount; n++)
            {
                if (common_c.Q_strcasecmp(host->ToString(), hostcache[n].name->ToString()) == 0)
                {
                    host = hostcache[n].cname;
                    break;
                }
            }
        }

    JustDoIt:
        for (net_driverlevel = 0; net_driverlevel < numdrivers; net_driverlevel++)
        {
            if (net_c.net_drivers[net_driverlevel].initialized == false)
            {
                continue;
            }

            *ret = dfunc.Connect(*host);

            if (ret != null)
            {
                return ret;
            }
        }

        if (host != null)
        {
            console_c.Con_Printf("\n");
            PrintSlistHeader();
            PrintSlist();
            PrintSlistTrailer();
        }

        return null;
    }

    public struct vcrConnect
    {
        public double time;
        public int op;
        public long session;
    }

    public static net_c.qsocket_t* NET_CheckNewConnections()
    {
        net_c.qsocket_t* ret = null;
        vcrConnect connect = new vcrConnect();

        SetNetTime();

        for (net_driverlevel = 0; net_driverlevel < net_c.net_numdrivers; net_driverlevel++)
        {
            if (net_c.net_drivers[net_driverlevel].initialized == false)
            {
                continue;
            }

            if (net_driverlevel != 0 && !listening)
            {
                continue;
            }

            if (ret != null)
            {
                if (recording)
                {
                    connect.time = host_c.host_time;
                    connect.op = net_vcr_c.VCR_OP_CONNECT;
                    connect.session = (long)ret;
                    sys_win_c.Sys_FileWrite(vcrFile, &connect, sizeof(vcrConnect));
                    sys_win_c.Sys_FileWrite(vcrFile, ret->address, net_c.NET_NAMELEN);
                }

                return ret;
            }
        }

        if (recording)
        {
            connect.time = host_c.host_time;
            connect.op = net_vcr_c.VCR_OP_CONNECT;
            connect.session = 0;
            sys_win_c.Sys_FileWrite(vcrFile, &connect, sizeof(vcrConnect));
        }

        return null;
    }

    public static void NET_Close(net_c.qsocket_t* sock)
    {
        if (sock == null)
        {
            return;
        }

        if (sock->disconnected)
        {
            return;
        }

        SetNetTime();

        sfunc.Close(*sock);

        NET_FreeQSocket(sock);
    }

    public struct vcrGetMessage
    {
        public double time;
        public int op;
        public long session;
        public int ret;
        public int len;
    }

    public static Action<net_c.qsocket_t> PrintStats { get; set; }

    public static int NET_GetMessage(net_c.qsocket_t* sock)
    {
        int ret;
        vcrGetMessage getMsg = new vcrGetMessage();

        if (sock == null)
        {
            return -1;
        }

        if (sock->disconnected)
        {
            console_c.Con_Printf("NET_GetMessage: disconnected socket\n");
            return -1;
        }

        SetNetTime();

        ret = sfunc.QGetMessage(*sock);

        if (ret == 0 && sock->driver != 0)
        {
            if (net_time - sock->lastMessageTime > net_messagetimeout.value)
            {
                NET_Close(sock);
                return -1;
            }
        }

        if (ret > 0)
        {
            if (sock->driver != 0)
            {
                sock->lastMessageTime = net_time;

                if (ret == 1)
                {
                    messagesReceived++;
                }
                else if (ret == 2)
                {
                    unreliableMessagesReceived++;
                }
            }

            if (recording)
            {
                getMsg.time = host_c.host_time;
                getMsg.op = net_vcr_c.VCR_OP_GETMESSAGE;
                getMsg.session = (long)sock;
                getMsg.ret = ret;
                sys_win_c.Sys_FileWrite(vcrFile, &getMsg, 20);
            }
        }
        else
        {
            if (recording)
            {
                getMsg.time = host_c.host_time;
                getMsg.op = net_vcr_c.VCR_OP_GETMESSAGE;
                getMsg.session = (long)sock;
                getMsg.ret = ret;
                sys_win_c.Sys_FileWrite(vcrFile, &getMsg, 20);
            }
        }

        return ret;
    }

    public struct vcrSendMessage
    {
        public double time;
        public int op;
        public long session;
        public int r;
    }

    public static int NET_SendMessage(net_c.qsocket_t* sock, common_c.sizebuf_t* data)
    {
        int r;
        vcrSendMessage sendMsg = new vcrSendMessage();

        if (sock == null)
        {
            return -1;
        }

        if (sock->disconnected)
        {
            console_c.Con_Printf("NET_SendMessage: disconnected socket\n");
            return -1;
        }

        SetNetTime();
        r = sfunc.QSendMessage(*sock, *data);

        if (r == 1 && sock->driver != 0)
        {
            messagesSent++;
        }

        if (recording)
        {
            sendMsg.time = host_c.host_time;
            sendMsg.op = net_vcr_c.VCR_OP_SENDMESSAGE;
            sendMsg.session = (long)sock;
            sendMsg.r = r;
            sys_win_c.Sys_FileWrite(vcrFile, &sendMsg, 20);
        }

        return r;
    }

    public static int NET_SendUnreliableMessage(net_c.qsocket_t* sock, common_c.sizebuf_t* data)
    {
        int r;
        vcrSendMessage sendMsg = new vcrSendMessage();

        if (sock == null)
        {
            return -1;
        }

        if (sock->disconnected)
        {
            console_c.Con_Printf("NET_SendMessage: disconnected socket\n");
            return -1;
        }

        SetNetTime();
        r = sfunc.SendUnreliableMessage(*sock, *data);

        if (r == 1 && sock->driver != 0)
        {
            unreliableMessagesSent++;
        }

        if (recording)
        {
            sendMsg.time = host_c.host_time;
            sendMsg.op = net_vcr_c.VCR_OP_SENDMESSAGE;
            sendMsg.session = (long)sock;
            sendMsg.r = r;
            sys_win_c.Sys_FileWrite(vcrFile, &sendMsg, 20);
        }

        return r;
    }

    public static bool NET_CanSendMessage(net_c.qsocket_t* sock)
    {
        int r;
        vcrSendMessage sendMsg = new vcrSendMessage();

        if (sock == null)
        {
            return false;
        }

        if (sock->disconnected)
        {
            return false;
        }

        SetNetTime();

        r = sfunc.CanSendMessage(*sock) == true ? 1 : 0;

        if (recording)
        {
            sendMsg.time = host_c.host_time;
            sendMsg.op = net_vcr_c.VCR_OP_CANSENDMESSAGE;
            sendMsg.session = (long)sock;
            sendMsg.r = r;
            sys_win_c.Sys_FileWrite(vcrFile, &sendMsg, 20);
        }

        return r;
    }

    public static int NET_SendToAll(common_c.sizebuf_t* data, int blocktime)
    {
        double start;
        int i;
        int count = 0;
        bool[] state1 = new bool[quakedef_c.MAX_SCOREBOARD];
        bool[] state2 = new bool[quakedef_c.MAX_SCOREBOARD];

        for (i = 0, host_c.host_client = server_c.svs.clients; i < server_c.svs.maxclients; i++, host_c.host_client++)
        {
            if (!host_c.host_client->netconnection)
            {
                continue;
            }

            if (host_c.host_client->active)
            {
                if (host_c.host_client->netconnection->driver == 0)
                {
                    NET_SendMessage(host_c.host_client->netconnection, data);
                    state1[i] = true;
                    state2[i] = true;
                    continue;
                }

                count++;
                state1[i] = false;
                state2[i] = false;
            }
            else
            {
                state1[i] = true;
                state2[i] = true;
            }
        }

        start = sys_win_c.Sys_FloatTime();

        while (count != 0)
        {
            count = 0;

            for (i = 0, host_c.host_client = server_c.svs.clients; i < server_c.svs.maxclients; i++, host_c.host_client++)
            {
                if (!state1[i])
                {
                    if (NET_CanSendMessage(host_c.host_client->netconnection))
                    {
                        state1[i] = true;
                        NET_SendMessage(host_c.host_client->netconnection, data);
                    }
                    else
                    {
                        NET_GetMessage(host_c.host_client->netconnection);
                    }

                    count++;
                    continue;
                }

                if (!state2[i])
                {
                    if (NET_CanSendMessage(host_c.host_client->netconnection))
                    {
                        state2[i] = true;
                    }
                    else
                    {
                        NET_GetMessage(host_c.host_client->netconnection);
                    }

                    count++;
                    continue;
                }
            }

            if ((sys_win_c.Sys_FloatTime() - start) > blocktime)
            {
                break;
            }
        }

        return count;
    }

    public static void NET_Init()
    {
        int i;
        int controlSocket;
        net_c.qsocket_t* s;

        if (common_c.COM_CheckParm("-playback") != 0)
        {
            net_c.net_numdrivers = 1;
            net_c.net_drivers[0].Init = net_vcr_c.VCR_Init;
        }

        if (common_c.COM_CheckParm("-record") != 0)
        {
            recording = true;
        }

        i = common_c.COM_CheckParm("-port");

        if (i == 0)
        {
            i = common_c.COM_CheckParm("-uddport");
        }

        if (i == 0)
        {
            i = common_c.COM_CheckParm("-ipxport");
        }

        if (i != 0)
        {
            if (i < common_c.com_argc - 1)
            {
                DEFAULTnet_hostport = common_c.Q_atoi(common_c.com_argv[i + 1].ToString());
            }
            else
            {
                sys_win_c.Sys_Error("NET_Init: you must specify a number after -port");
            }
        }

        net_c.net_hostport = DEFAULTnet_hostport;

        if (common_c.COM_CheckParm("-listen") != 0 || client_c.cls.state == client_c.cactive_t.ca_dedicated)
        {
            listening = true;
        }

        net_numsockets = server_c.svs.maxclientslimit;

        if (client_c.cls.state != client_c.cactive_t.ca_dedicated)
        {
            net_numsockets++;
        }

        SetNetTime();

        for (i = 0; i < net_numsockets; i++)
        {
            s = (net_c.qsocket_t*)zone_c.Hunk_AllocName(sizeof(net_c.qsocket_t), "qsocket");
            s->next = net_freeSockets;
            net_freeSockets = s;
            s->disconnected = true;
        }

        common_c.SZ_Alloc(net_c.net_message, net_c.NET_MAXMESSAGE);

        cvar_c.Cvar_RegisterVariable(net_messagetimeout);
        cvar_c.Cvar_RegisterVariable(hostname);
        cvar_c.Cvar_RegisterVariable(config_com_port);
        cvar_c.Cvar_RegisterVariable(config_com_irq);
        cvar_c.Cvar_RegisterVariable(config_com_baud);
        cvar_c.Cvar_RegisterVariable(config_com_modem);
        cvar_c.Cvar_RegisterVariable(config_modem_dialtype);
        cvar_c.Cvar_RegisterVariable(config_modem_clear);
        cvar_c.Cvar_RegisterVariable(config_modem_init);
        cvar_c.Cvar_RegisterVariable(config_modem_hangup);
#if IDGODS
        cvar_c.Cvar_RegisterVariable(idgods);
#endif

        cmd_c.Cmd_AddCommand("slist", NET_Slist_f);
        cmd_c.Cmd_AddCommand("listen", NET_Listen_f);
        cmd_c.Cmd_AddCommand("maxplayers", MaxPlayers_f);
        cmd_c.Cmd_AddCommand("port", NET_Port_f);

        for (net_driverlevel = 0; net_driverlevel < net_c.net_numdrivers; net_driverlevel++)
        {
            controlSocket = net_c.net_drivers[net_driverlevel].Init();

            if (controlSocket == -1)
            {
                continue;
            }

            net_c.net_drivers[net_driverlevel].initialized = true;
            net_c.net_drivers[net_driverlevel].controlSock = controlSocket;

            if (listening)
            {
                net_c.net_drivers[net_driverlevel].Listen(true);
            }

            if (*my_ipx_address != 0)
            {
                console_c.Con_DPrintf($"IPX address {my_ipx_address->ToString()}\n");
            }

            if (*my_tcpip_address != 0)
            {
                console_c.Con_DPrintf($"TCP/IP address {my_tcpip_address->ToString()}\n");
            }
        }
    }

    public static void NET_Shutdown()
    {
        net_c.qsocket_t* sock;

        SetNetTime();

        for (sock = net_activeSockets; sock != null; sock = sock->next)
        {
            NET_Close(sock);
        }

        for (net_driverlevel = 0; net_driverlevel < net_c.net_numdrivers; net_driverlevel++)
        {
            if (net_c.net_drivers[net_driverlevel].initialized == true)
            {
                net_c.net_drivers[net_driverlevel].Shutdown();
                net_c.net_drivers[net_driverlevel].initialized = false;
            }
        }

        if (vcrFile != -1)
        {
            console_c.Con_Printf("Closing vcrfile.\n");
            sys_win_c.Sys_FileClose(vcrFile);
        }
    }

    public static net_c.PollProcedure* pollProcedureList = null;

    public static void NET_Poll()
    {
        net_c.PollProcedure* pp;
        bool useModem;

        if (!configRestored)
        {
            if (serialAvailable)
            {
                if (config_com_modem.value == 1.0)
                {
                    useModem = true;
                }
                else
                {
                    useModem = false;
                }

                SetComPortConfig(0, (int)config_com_port.value, (int)config_com_irq.value, (int)config_com_baud.value, useModem);
                SetModemConfig(0, *config_modem_dialtype.str, *config_modem_clear.str, *config_modem_init.str, *config_modem_hangup.str);
            }

            configRestored = true;
        }

        SetNetTime();

        for (pp = pollProcedureList; pp != null; pp = pp->next)
        {
            if (pp->nextTime > net_time)
            {
                break;
            }

            pollProcedureList = pp->next;
            pp->procedure(pp->arg);
        }
    }

    public static void SchedulePollProcedure(net_c.PollProcedure* proc, double timeOffset)
    {
        net_c.PollProcedure* pp, prev;

        proc->nextTime = sys_win_c.Sys_FloatTime() + timeOffset;

        for (pp = pollProcedureList, prev = null; pp != null; pp = pp->next)
        {
            if (pp->nextTime >= proc->nextTime)
            {
                break;
            }

            prev = pp;
        }

        if (prev == null)
        {
            proc->next = pollProcedureList;
            pollProcedureList = proc;
            return;
        }

        proc->next = pp;
        prev->next = proc;
    }

#if IDGODS
    public static int IDNET = 0xc0f62800;

    public static bool IsID(net_c.qsockaddr* addr) 
    {
        if (idgods.value == 0.0) 
        {
            return false;
        }

        if (addr->sa_family != 2) 
        {
            return false;
        }

        if ((common_c.BigLong(*(int*)&addr->sa_data[2]) & 0xffffff00) == IDNET) 
        {
            return true;
        }

        return false;
    }
#endif
}