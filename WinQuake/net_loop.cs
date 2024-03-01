namespace Quake;

public unsafe class net_loop_c
{
    public static bool localconnectpending = false;
    public static net_c.qsocket_t* loop_client = null;
    public static net_c.qsocket_t* loop_server = null;

    public static int Loop_Init()
    {
        if (cl_main_c.cls.state == client_c.cactive_t.ca_dedicated)
        {
            return -1;
        }
        return 0;
    }

    public static void Loop_Shutdown()
    {
    }

    public static void Loop_Listen(bool state)
    {
    }

    public static void Loop_SearchForHosts(bool xmit)
    {
        if (!server_c.sv.active)
        {
            return;
        }

        net_c.hostCacheCount = 1;

        if (!common_c.Q_strcmp(net_main_c.hostname.str->ToString(), "UNNAMED"))
        {
            common_c.Q_strcpy(net_c.hostcache[0].name, "local");
        }
        else
        {
            common_c.Q_strcpy(net_c.hostcache[0].name, net_main_c.hostname.str);
        }

        common_c.Q_strcpy(net_c.hostcache[0].name, server_c.sv.name);
        net_c.hostcache[0].users = net_c.net_activeconnections;
        net_c.hostcache[0].maxusers = server_c.svs.maxclients;
        net_c.hostcache[0].driver = net_c.net_driverlevel;
        common_c.Q_strcpy(net_c.hostcache[0].cname, "local");
    }

    public static net_c.qsocket_t* Loop_Connect(char* host)
    {
        if (common_c.Q_strcmp(host->ToString(), "local"))
        {
            return null;
        }

        localconnectpending = true;

        if (loop_client == null)
        {
            if ((loop_client = net_main_c.NET_NewQSocket()) == null)
            {
                console_c.Con_Printf("Loop_Connect: no qsocket available\n");
                return null;
            }

            common_c.Q_strcpy(loop_client->address, "localhost");
        }

        loop_client->receiveMessageLength = 0;
        loop_client->sendMessageLength = 0;
        loop_client->canSend = true;

        if (loop_server == null)
        {
            if ((loop_server = net_main_c.NET_NewQSocket()) == null)
            {
                console_c.Con_Printf("Loop_Connect: no qsocket available\n");
                return null;
            }

            common_c.Q_strcpy(loop_server->address, "LOCAL");
        }

        loop_server->receiveMessageLength = 0;
        loop_server->sendMessageLength = 0;
        loop_server->canSend = true;

        loop_client->driverdata = (void*)loop_server;
        loop_server->driverdata = (void*)loop_client;

        return loop_client;
    }

    public static net_c.qsocket_t* Loop_CheckNewConnection()
    {
        if (!localconnectpending)
        {
            return null;
        }

        localconnectpending = false;
        loop_server->sendMessageLength = 0;
        loop_server->receiveMessageLength = 0;
        loop_server->canSend = true;
        loop_client->sendMessageLength = 0;
        loop_client->receiveMessageLength = 0;
        loop_client->canSend = true;
        return loop_server;
    }

    public static int IntAlign(int value)
    {
        return (value + (sizeof(int) - 1)) & (~(sizeof(int) - 1));
    }

    public static int Loop_GetMessage(net_c.qsocket_t* sock)
    {
        int ret;
        int length;

        if (sock->receiveMessageLength == 0)
        {
            return 0;
        }

        ret = sock->receiveMessage[0];
        length = sock->receiveMessage[1] + (sock->receiveMessage[2] << 8);

        common_c.SZ_Clear(net_c.net_message);
        common_c.SZ_Write(net_c.net_message, sock->receiveMessage[4], length);

        length = IntAlign(length + 4);
        sock->receiveMessageLength -= length;

        if (sock->driverdata != null && ret == 1)
        {
            ((net_c.qsocket_t*)sock->driverdata)->canSend = true;
        }

        return ret;
    }

    public static int Loop_SendMessage(net_c.qsocket_t* sock, common_c.sizebuf_t* data)
    {
        byte* buffer;
        int* bufferLength;

        if (sock->driverdata == null)
        {
            return -1;
        }

        bufferLength = &((net_c.qsocket_t*)sock->driverdata)->receiveMessageLength;

        if ((*bufferLength + data->cursize + 4) > net_c.NET_MAXMESSAGE)
        {
            sys_win_c.Sys_Error("Loop_SendMessage: overflow\n");
        }

        buffer = ((net_c.qsocket_t*)sock->driverdata)->receiveMessage + *bufferLength;

        *buffer++ = 1;

        *buffer++ = (byte)(data->cursize & 0xff);
        *buffer++ = (byte)(data->cursize >> 8);

        buffer++;

        common_c.Q_memcpy(*buffer, *data->data, data->cursize);
        *bufferLength = IntAlign(*bufferLength + data->cursize + 4);

        sock->canSend = true;
        return 1;
    }

    public static int Loop_SendUnreliableMessage(net_c.qsocket_t* sock, common_c.sizebuf_t* data)
    {
        byte* buffer;
        int* bufferLength;

        if (sock->driverdata == null)
        {
            return -1;
        }

        bufferLength = &((net_c.qsocket_t*)sock->driverdata)->receiveMessageLength;

        if ((*bufferLength + data->cursize + sizeof(byte) + sizeof(short)) > net_c.NET_MAXMESSAGE)
        {
            return 0;
        }

        buffer = ((net_c.qsocket_t*)sock->driverdata)->receiveMessage + *bufferLength;

        *buffer++ = 2;

        *buffer++ = (byte)(data->cursize & 0xff);
        *buffer++ = (byte)(data->cursize >> 8);

        buffer++;

        common_c.Q_memcpy(*buffer, *data->data, data->cursize);
        *bufferLength = IntAlign(*bufferLength + data->cursize + 4);
        return 1;
    }

    public static bool Loop_CanSendMessage(net_c.qsocket_t* sock)
    {
        if (sock->driverdata == null)
        {
            return false;
        }

        return sock->canSend;
    }

    public static bool Loop_CanSendUnreliableMessage(net_c.qsocket_t* sock)
    {
        return true;
    }

    public static void Loop_Close(net_c.qsocket_t* sock)
    {
        if (sock->driverdata != null)
        {
            ((net_c.qsocket_t*)sock->driverdata)->driverdata = null;
        }

        sock->receiveMessageLength = 0;
        sock->sendMessageLength = 0;
        sock->canSend = true;

        if (sock == loop_client)
        {
            loop_client = null;
        }
        else
        {
            loop_server = null;
        }
    }
}