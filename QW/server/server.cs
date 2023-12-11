namespace Quake.Server;

public unsafe class server_c
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

        public progs_c.edict_t* edict;
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

        public challenge_t challenges;
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
}