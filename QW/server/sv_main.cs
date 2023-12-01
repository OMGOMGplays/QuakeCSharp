namespace Quake;

public unsafe class sv_main_c
{
    public static int MAX_MASTERS = 8;

    public static int MAX_SIGNON_BUFFERS = 8;

    public enum server_state_t
    {
        ss_dead,
        ss_loading,
        ss_active
    }

    public struct server_t
    {
        public bool active;
        public server_state_t state;

        public double time;

        public int lastcheck;
        public double lastchecktime;

        public bool paused;

        public uint model_player_checksum;
        public uint eyes_player_checksum;

        public char[] name;
        public char[] modelname;
        public model_t* worldmodel;
        public char* model_precache;
        public char* sound_precache;
        public char* lightstyles;
        public model_t* models;

        public int num_edicts;
        public edict_t* edicts;

        public byte* pvs, phs;

        public common_c.sizebuf_t datagram;
        public byte datagram_buf;

        public common_c.sizebuf_t reliable_datagram;
        public byte reliable_datagram_buf;

        public common_c.sizebuf_t multicast;
        public byte multicast_buf;

        public common_c.sizebuf_t master;
        public byte master_buf;

        public common_c.sizebuf_t signon;
        public int num_signon_buffers;
        public int signon_buffer_size;
        public byte signon_buffers;
    }

    public static int NUM_SPAWN_PARMS = 16;

    public enum client_state_t
    {
        cs_free,
        cs_zombie,
        cs_connected,
        cs_spawned
    }

    public struct client_frame_t
    {
        public double senttime;
        public float ping_time;
        public packet_entites_t entities;
    }

    public static int MAX_BACK_BUFFERS = 4;

    public struct client_t
    {
        public client_state_t state;

        public int spectator;

        public bool sendinfo;

        public float lastnametime;
        public int lastnamecount;
        public uint checksum;
        public bool drop;
        public int lossage;

        public int userid;
        public char userinfo;

        public usercmd_t lastcmd;
        public double localtime;
        public int oldbuttons;

        public float maxspeed;
        public float entgravity;

        public edict_t* edict;
        public char name;

        public int messagelevel;

        public common_c.sizebuf_t datagram;
        public byte datagram_buf;

        public common_c.sizebuf_t backbuf;
        public int num_backbuf;
        public int backbuf_size;
        public byte backbuf_data;

        public double connection_started;
        public bool send_message;

        public float spawn_parms;

        public int old_frags;

        public int stats;

        public client_frame_t frames;

        public FileStream* download;
        public int downloadsize;
        public int downloadcount;

        public int spec_track;

        public double whensaid;
        public int whensaidhead;
        public double lockedtill;

        public bool upgradewarn;

        public FileStream* upload;
        public char uploadfn;
        public netadr_t snap_from;
        public bool remote_snap;

        public int chokecount;
        public int delta_sequence;
        public netchan_t netchan;
    }

    public static int STATFRAMES = 100;

    public struct svstats_t
    {
        public double active;
        public double idle;
        public int count;
        public int packets;

        public double latched_active;
        public double latched_idle;
        public int latched_packets;
    }

    public static int MAX_CHALLENGES = 1024;

    public struct challenge_t
    {
        public netadr_t adr;
        public int challenge;
        public int time;
    }

    public struct server_static_t
    {
        public int spawncount;

        public client_t clients;
        public int serverflags;

        public double last_heartbeat;
        public int heartbeat_sequence;
        public svstats_t stats;

        public char info;

        public int logsequence;
        public double logtime;
        public common_c.sizebuf_t log;
        public byte log_buf;

        challenge_t challenges;
    }

    public static int MOVETYPE_NONE = 0;
    public static int MOVETYPE_ANGLENOCLIP = 1;
    public static int MOVETYPE_ANGLECLIP = 2;
    public static int MOVETYPE_WALK = 3;
    public static int MOVETYPE_STEP = 4;
    public static int MOVETYPE_FLY = 5;
    public static int MOVETYPE_TOSS = 6;
    public static int MOVETYPE_PUSH = 7;
    public static int MOVETYPE_NOCLIP = 8;
    public static int MOVETYPE_FLYMISSILE = 9;
    public static int MOVETYPE_BOUNCE = 10;

    public static int SOLID_NOT = 0;
    public static int SOLID_TRIGGER = 1;
    public static int SOLID_BBOX = 2;
    public static int SOLID_SLIDEBOX = 3;
    public static int SOLID_BSP = 4;

    public static int DEAD_NO = 0;
    public static int DEAD_DYING = 1;
    public static int DEAD_DEAD = 2;

    public static int DAMAGE_NO = 0;
    public static int DAMAGE_YES = 1;
    public static int DAMAGE_AIM = 2;

    public static int FL_FLY = 1;
    public static int FL_SWIM = 2;
    public static int FL_GLIMPSE = 4;
    public static int FL_CLIENT = 8;
    public static int FL_INWATER = 16;
    public static int FL_MONSTER = 32;
    public static int FL_GODMODE = 64;
    public static int FL_NOTARGET = 128;
    public static int FL_ITEM = 256;
    public static int FL_ONGROUND = 512;
    public static int FL_PARTIALGROUND = 1024;
    public static int FL_WATERJUMP = 2048;

    public static int EF_BRIGHTLIGHT = 4;
    public static int EF_DIMLIGHT = 8;

    public static int SPAWNFLAG_NOT_EASY = 256;
    public static int SPAWNFLAG_NOT_MEDIUM = 512;
    public static int SPAWNFLAG_NOT_HARD = 1024;
    public static int SPAWNFLAG_NOT_DEATHMATCH = 2048;

    public static int MULTICAST_ALL = 0;
    public static int MULTICAST_PHS = 1;
    public static int MULTICAST_PVS = 2;

    public static int MULTICAST_ALL_R = 3;
    public static int MULTICAST_PHS_R = 4;
    public static int MULTICAST_PVS_R = 5;

    public quakedef_c.quakeparms_t host_parms;

    public static bool host_initialized;

    public double host_frametime;
    public double realtime;

    public int host_hunklevel;

    public net_c.netadr_t[] master_adr = new net_c.netadr_t[server_c.MAX_MASTERS];

    public server_c.client_t* host_client;

    public cvar_c.cvar_t sv_mintic = new cvar_c.cvar_t { name = "sv_mintic", value = (char)0.03f };
    public cvar_c.cvar_t sv_maxtic = new cvar_c.cvar_t { name = "sv_maxtic", value = (char)0.1f };

    public cvar_c.cvar_t developer = new cvar_c.cvar_t { name = "developer", value = (char)0 };

    public cvar_c.cvar_t timeout = new cvar_c.cvar_t { name = "timeout", value = (char)65 };
    public cvar_c.cvar_t zombietime = new cvar_c.cvar_t { name = "zombietime", value = (char)2 };

    public cvar_c.cvar_t rcon_password = new cvar_c.cvar_t { name = "rcon_password", value = ' ' };
    public cvar_c.cvar_t password = new cvar_c.cvar_t { name = "password", value = ' ' };
    public cvar_c.cvar_t spectator_password = new cvar_c.cvar_t { name = "spectator_password", value = ' ' };

    public cvar_c.cvar_t allow_download = new cvar_c.cvar_t { name = "allow_download", value = (char)1 };
    public cvar_c.cvar_t allow_download_skins = new cvar_c.cvar_t { name = "allow_download_skin", value = (char)1 };
    public cvar_c.cvar_t allow_download_models = new cvar_c.cvar_t { name = "allow_download_models", value = (char)1 };
    public cvar_c.cvar_t allow_download_sounds = new cvar_c.cvar_t { name = "allow_download_sounds", value = (char)1 };
    public cvar_c.cvar_t allow_download_maps = new cvar_c.cvar_t { name = "allow_download_maps", value = (char)1 };

    public cvar_c.cvar_t sv_highchars = new cvar_c.cvar_t { name = "sv_highchars", value = (char)1 };

    public cvar_c.cvar_t sv_phs = new cvar_c.cvar_t { name = "sv_phs", value = (char)1 };

    public cvar_c.cvar_t pausable = new cvar_c.cvar_t { name = "pausable", value = (char)1 };

    public cvar_c.cvar_t fraglimit = new cvar_c.cvar_t { name = "fraglimit", value = (char)0, server = true };
    public cvar_c.cvar_t timelimit = new cvar_c.cvar_t { name = "timelimit", value = (char)0, server = true };
    public cvar_c.cvar_t teamplay = new cvar_c.cvar_t { name = "teamplay", value = (char)0, server = true };
    public cvar_c.cvar_t samelevel = new cvar_c.cvar_t { name = "samelevel", value = (char)0, server = true };
    public cvar_c.cvar_t maxclients = new cvar_c.cvar_t { name = "maxclients", value = (char)8, server = true };
    public cvar_c.cvar_t maxspectators = new cvar_c.cvar_t { name = "maxspectators", value = (char)8, server = true };
    public cvar_c.cvar_t deathmatch = new cvar_c.cvar_t { name = "deathmatch", value = (char)1, server = true };
    public cvar_c.cvar_t spawn = new cvar_c.cvar_t { name = "spawn", value = (char)0, server = true };
    public cvar_c.cvar_t watervis = new cvar_c.cvar_t { name = "watervis", value = (char)0, server = true };

    public cvar_c.cvar_t hostname = new cvar_c.cvar_t { name = "hostname", value = ' ', server = true };

    public FileStream* sv_logfile;
    public FileStream* sv_fraglogfile;

    public bool ServerPaused()
    {
        return sv.paused;
    }

    public void SV_Shutdown()
    {
        Master_Shutdown();

        if (sv_logfile != null)
        {
            sv_logfile->Close();
            sv_logfile = null;
        }

        if (sv_fraglogfile != null)
        {
            sv_fraglogfile->Close();
            sv_fraglogfile = null;
        }

        NET_Shutdown();
    }

    public void SV_Error(char* error, params object[] args)
    {
        char[] str = new char[1024];
        bool inerror = false;

        if (inerror)
        {
            sys_win_c.Sys_Error($"SV_Error: recursively entered ({str})");
        }

        inerror = true;

        Console.WriteLine($"{str} {error->ToString()}");

        console_c.Con_Printf($"SV_Error: {str}\n");

        SV_FinalMessage(common_c.va($"Server crashed: {str}\n"));

        SV_Shutdown();

        sys_win_c.Sys_Error($"SV_Error: {str}\n");
    }

    public void SV_FinalMessage(string message)
    {
        int i;
        client_t* cl;

        common_c.SZ_Clear(&net_message);
        common_c.MSG_WriteByte(&net_message, svc_print);
        common_c.MSG_WriteByte(&net_message, PRINT_HIGH);
        common_c.MSG_WriteString(&net_message, message);
        common_c.MSG_WriteByte(&net_message, svc_disconnect);

        for (i = 0; i < svs.clients; i < MAX_CLIENTS; i++, cl++) {
            if (cl->state >= cs_spawned)
            {
                Netchan_Transit(&cl->netchan, net_message.cursize, net_message.data);
            }
        }
    }

    public void SV_DropClient(client_t* drop)
    {
        common_c.MSG_WriteByte(&drop->netchan.message, svc_disconnect);

        if (drop->state == cs_spawned)
        {
            if (!drop->spectator)
            {
                pr_global_struct->self = EDICT_TO_PROG(drop->edict);
                PR_ExecuteProgram(pr_global_struct->ClientDisconnect);
            }
            else if (SpectatorDisconnect)
            {
                pr_global_struct->self = EDICT_TO_PROG(drop->edict);
                PR_ExecuteProgram(SpectatorDisconnect);
            }
        }

        if (drop->spectator)
        {
            console_c.Con_Printf($"Spectator {drop->name} removed\n");
        }
        else
        {
            console_c.Con_Printf($"Client {drop->name} removed\n");
        }

        if (drop->download != null)
        {
            drop->download.Close();
            drop->download = null;
        }

        if (drop->upload != null)
        {
            drop->upload.Close();
            drop->upload = null;
        }

        *drop->uploadfn = 0;

        drop->state = cs_zombie;
        drop->connection_started = realtime;

        drop->old_frags = 0;
        drop->edict->v.frags = 0;
        drop->name[0] = '\0';
        common_c.Q_memset(drop->userinfo, 0, sizeof(drop->userinfo));

        SV_FullClientUpdate(drop, &sv.reliable_datagram);
    }

    public int SV_CalcPing(client_t* cl)
    {
        float ping;
        int i;
        int count;
        client_frame_t* frame;

        ping = 0;
        count = 0;

        for (frame = cl->frames, i = 0; i < UPDATE_BACKUP; i++, frame++)
        {
            if (frame->ping_time > 0)
            {
                ping += frame->ping_time;
                count++;
            }
        }

        if (count == 0)
        {
            return 9999;
        }

        ping /= count;

        return (int)ping * 1000;
    }

    public void SV_FullClientUpdate(client_t* client, common_c.sizebuf_t buf)
    {
        int i;
        char info;

        i = client - svs.clients;

        common_c.MSG_WriteByte(buf, svc_updatefrags);
        common_c.MSG_WriteByte(buf, i);
        common_c.MSG_WriteShort(buf, client->old_frags);

        common_c.MSG_WriteByte(buf, svc_updateping);
        common_c.MSG_WriteByte(buf, i);
        common_c.MSG_WriteShort(buf, SV_CalcPing(client));

        common_c.MSG_WriteByte(buf, svc_updatepl);
        common_c.MSG_WriteByte(buf, i);
        common_c.MSG_WriteByte(buf, client->lossage);

        common_c.MSG_WriteByte(buf, svc_updateentertime);
        common_c.MSG_WriteByte(buf, i);
        common_c.MSG_WriteFloat(buf, realtime - client->connection_started);

        common_c.Q_strcpy(info, client->userinfo);
        Info_RemovePrefixedKeys(info, '_');

        common_c.MSG_WriteByte(buf, svc_updateuserinfo);
        common_c.MSG_WriteByte(buf, i);
        common_c.MSG_WriteLong(buf, client->userid);
        common_c.MSG_WriteString(buf, info);
    }

    public void SV_FullUpdateToClient(client_t* client, client_t* cl)
    {
        ClientReliableCheckBlock(cl, 24 + common_c.Q_strlen(client->userinfo));

        if (cl->numbackbuf != 0)
        {
            SV_FullClientUpdate(client, &cl->backbuf);
            ClientReliable_FinishWrite(cl);
        }
        else
        {
            SV_FullClientUpdate(client, &cl->netchan.message);
        }
    }

    public void SVC_Status()
    {
        int i;
        client_t* cl;
        int ping;
        int top, bottom;

        string status = "status";
        char[] status_char_array = status.ToCharArray();
        char* status_char = null;

        for (int j = 0; j < status_char_array.Length; j++)
        {
            status_char[j] = status_char_array[j];
        }

        cmd_c.Cmd_TokenizeString(status_char);
    }
}