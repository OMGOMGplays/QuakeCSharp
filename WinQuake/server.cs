namespace Quake;

public unsafe class server_c
{
    public struct server_static_t
    {
        public int maxclients;
        public int maxclientslimit;
        public client_t* clients;
        public int serverflags;
        public bool changelevel_issued;
    }

    public enum server_state_t { ss_loading, ss_active }

    public struct server_t
    {
        public bool active;

        public bool paused;
        public bool loadgame;

        public double time;

        public int lastcheck;
        public double lastchecktime;

        public char* name;

#if QUAKE2
        public char* startspot;
#endif
        public char* modelname;
        public model_c.model_t* worldmodel;
        public char* model_precache;
        public model_c.model_t* models;
        public char* sound_precache;
        public char* lightstyles;
        public int num_edicts;
        public int max_edicts;
        public progs_c.edict_t* edicts;

        public server_state_t state;

        public common_c.sizebuf_t datagram;
        public byte* datagram_buf;

        public common_c.sizebuf_t reliable_datagram;
        public byte* reliable_datagram_buf;

        public common_c.sizebuf_t signon;
        public byte* signon_buf;
    }

    public static int NUM_PING_TIMES = 16;
    public static int NUM_SPAWN_PARMS = 16;

    public struct client_t
    {
        public bool active;
        public bool spawned;
        public bool dropasap;
        public bool privileged;
        public bool sendsignon;

        public double last_message;

        public net_c.qsocket_t* netconnection;

        public client_c.usercmd_t cmd;
        public Vector3 wishdir;

        public common_c.sizebuf_t message;

        public byte* msgbuf;
        public progs_c.edict_t* edict;
        public char* name;
        public int colors;

        public float* ping_times;
        public int num_pings;

        public float* spawn_parms;

        public int old_frags;
    }

    public const int MOVETYPE_NONE = 0;
    public const int MOVETYPE_ANGLENOCLIP = 1;
    public const int MOVETYPE_ANGLECLIP = 2;
    public const int MOVETYPE_WALK = 3;
    public const int MOVETYPE_STEP = 4;
    public const int MOVETYPE_FLY = 5;
    public const int MOVETYPE_TOSS = 6;
    public const int MOVETYPE_PUSH = 7;
    public const int MOVETYPE_NOCLIP = 8;
    public const int MOVETYPE_FLYMISSILE = 9;
    public const int MOVETYPE_BOUNCE = 10;
#if QUAKE2
    public const int MOVETYPE_BOUNCEMISSILE = 11;
    public const int MOVETYPE_FOLLOW = 12;
#endif

    public const int SOLID_NOT = 0;
    public const int SOLID_TRIGGER = 1;
    public const int SOLID_BBOX = 2;
    public const int SOLID_SLIDEBOX = 3;
    public const int SOLID_BSP = 4;

    public const int DEAD_NO = 0;
    public const int DEAD_DYING = 1;
    public const int DEAD_DEAD = 2;

    public const int DAMAGE_NO = 0;
    public const int DAMAGE_YES = 1;
    public const int DAMAGE_AIM = 2;

    public const int FL_FLY = 1;
    public const int FL_SWIM = 2;
    //public const int FL_GLIMPSE = 4;
    public const int FL_CONVEYOR = 4;
    public const int FL_CLIENT = 8;
    public const int FL_INWATER = 16;
    public const int FL_MONSTER = 32;
    public const int FL_GODMODE = 64;
    public const int FL_NOTARGET = 128;
    public const int FL_ITEM = 256;
    public const int FL_ONGROUND = 512;
    public const int FL_PARTIALGROUND = 1024;
    public const int FL_WATERJUMP = 2048;
    public const int FL_JUMPRELEASED = 4096;
#if QUAKE2
    public const int FL_FLASHLIGHT = 8192;
    public const int FL_ARCHIVE_OVERRIDE = 1048576;
#endif

    public const int EF_BRIGHTFIELD = 1;
    public const int EF_MUZZLEFLASH = 2;
    public const int EF_BRIGHTLIGHT = 4;
    public const int EF_DIMLIGHT = 8;
#if QUAKE2
    public const int EF_DARKLIGHT = 16;
    public const int EF_DARKFIELD = 32;
    public const int EF_LIGHT = 64;
    public const int EF_NODRAW = 128;
#endif

    public const int SPAWNFLAG_NOT_EASY = 256;
    public const int SPAWNFLAG_NOT_MEDIUM = 512;
    public const int SPAWNFLAG_NOT_HARD = 1024;
    public const int SPAWNFLAG_NOT_DEATHMATCH = 2048;

#if QUAKE2
    public const int SFL_EPISODE_1 = 1;
    public const int SFL_EPISODE_2 = 2;
    public const int SFL_EPISODE_3 = 4;
    public const int SFL_EPISODE_4 = 8;
    public const int SFL_NEW_UNIT = 16;
    public const int SFL_NEW_EPISODE = 32;
    public const int SFL_CROSS_TRIGGERS = 65280;
#endif

    public static cvar_c.cvar_t teamplay;
    public static cvar_c.cvar_t skill;
    public static cvar_c.cvar_t deathmatch;
    public static cvar_c.cvar_t coop;
    public static cvar_c.cvar_t fraglimit;
    public static cvar_c.cvar_t timelimit;

    public static server_static_t svs;
    public static server_t sv;

    public static client_t* host_client;

    //public static jmp_buf host_abortserver; // Fix somehow

    public static double host_time;

    public static progs_c.edict_t* sv_player;
}