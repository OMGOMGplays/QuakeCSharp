namespace Quake.Server;

public unsafe class sv_send_c
{
    public const int CHAN_AUTO = 0;
    public const int CHAN_WEAPON = 1;
    public const int CHAN_VOICE = 2;
    public const int CHAN_ITEM = 3;
    public const int CHAN_BODY = 4;

    public static char* outputbuf;

    public static server_c.redirect_t sv_redirected;

    public static cvar_c.cvar_t sv_phs;

    public static void SV_FlushRedirect()
    {
        char* send = null;

        if (sv_redirected == RD_PACKET)
        {
            send[0] = (char)0xff;
            send[1] = (char)0xff;
            send[2] = (char)0xff;
            send[3] = (char)0xff;
            send[4] = protocol_c.A2C_PRINT;
            common_c.Q_memcpy(*send + 5, *outputbuf, common_c.Q_strlen(outputbuf) + 1);

            net_udp_c.NET_SendPacket(common_c.Q_strlen(send) + 1, send, net_c.net_from);
        }
        else if (sv_redirected == RD_CLIENT)
        {
            sv_nchan_c.ClientReliableWrite_Begin(host_c.host_client, protocol_c.svc_print, common_c.Q_strlen(outputbuf) + 3);
            sv_nchan_c.ClientReliableWrite_Byte(host_c.host_client, protocol_c.PRINT_HIGH);
            sv_nchan_c.ClientReliableWrite_String(host_c.host_client, outputbuf);
        }

        outputbuf[0] = (char)0;
    }

    public static void SV_BeginRedirect(server_c.redirect_t rd)
    {
        sv_redirected = rd;
        outputbuf[0] = 0;
    }

    public static void SV_EndRedirect()
    {
        SV_FlushRedirect();
        sv_redirected = RD_NONE;
    }

    public const int MAXPRINTMSG = 4096;

    public static void Con_Printf(char* fmt, params object[] args)
    {
        char* msg;

        Console.WriteLine(msg->ToString());

        if (sv_redirected)
        {
            if (common_c.Q_strlen(msg) + common_c.Q_strlen(outputbuf) > sizeof(outputbuf) - 1)
            {
                SV_FlushRedirect();
            }

            common_c.Q_strcat(outputbuf, msg);
            return;
        }

        sys_win_c.Sys_Printf(msg);

        if (sv_main_c.sv_logfile != null)
        {

        }
    }
}