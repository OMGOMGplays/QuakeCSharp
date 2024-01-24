namespace Quake;

public unsafe class client_c
{
    public struct usercmd_t
    {
        public Vector3 viewangles;

        public float forwardmove;
        public float sidemove;
        public float upmove;

#if QUAKE2
        public byte lightlevel;
#endif
    }

    public struct lightstyle_t
    {
        public int length;
        public char* map;
    }

    public struct scoreboard_t
    {
        public char* name;
        public float entertime;
        public int frags;
        public int colors;
        public byte* translations;
    }

    public struct cshift_t
    {
        public int* destcolor;
        public int percent;
    }

    public const int CSHIFT_CONTENTS = 0;
    public const int CSHIFT_DAMAGE = 1;
    public const int CSHIFT_BONUS = 2;
    public const int CSHIFT_POWERUP = 3;
    public const int NUM_CSHIFTS = 4;

    public const int NAME_LENGTH = 64;

    public const int SIGNONS = 4;

    public const int MAX_DLIGHTS = 32;

    public struct dlight_t
    {
        public Vector3 origin;
        public float radius;
        public float die;
        public float decay;
        public float minlight;
        public int key;
#if QUAKE2
        public bool dark;
#endif
    }

    public const int MAX_BEAMS = 24;

    public struct beam_t
    {
        public int entity;
        public model_c.model_t* model;
        public float endtime;
        public Vector3 start, end;
    }

    public const int MAX_EFRAGS = 640;

    public const int MAX_MAPSTRING = 2048;
    public const int MAX_DEMOS = 8;
    public const int MAX_DEMONAME = 16;

    public enum cactive_t
    {
        ca_dedicated,
        ca_disconnected,
        ca_connected
    }

    public struct client_static_t
    {
        public cactive_t state;

        public char* mapstring;
        public char* spawnparms;

        public int demonum;
        public char* demos;

        public bool demorecording;
        public bool demoplayback;
        public bool timedemo;
        public int forcetrack;
        public FileStream* demofile;
        public int td_lastframe;
        public int td_startframe;
        public float td_starttime;

        public int signon;
        public net_c.qsocket_t* netcon;
        public common_c.sizebuf_t message;
    }

    public static client_static_t cls;

    public struct client_state_t
    {
        public int movemessages;

        public usercmd_t cmd;

        public int[] stats;
        public int items;
        public float[] item_gettime;
        public float faceanimtime;

        public cshift_t[] cshifts;
        public cshift_t[] prev_cshifts;

        public Vector3[] mviewangles;

        public Vector3 viewangles;

        public Vector3[] mvelocity;

        public Vector3 velocity;

        public Vector3 punchangle;

        public float idealpitch;
        public float pitchvel;
        public bool nodrift;
        public float driftmove;
        public double laststop;

        public float viewheight;
        public float crouch;

        public bool paused;
        public bool onground;
        public bool inwater;

        public int intermission;
        public int completed_time;

        public double[] mtime;
        public double time;

        public double oldtime;

        public float last_received_message;

        public model_c.model_t* model_precache;
        public sound_c.sfx_t* sound_precache;

        public char* levelname;
        public int viewentity;
        public int maxclients;
        public int gametype;

        public model_c.model_t* worldmodel;
        public render_c.efrag_t* free_efrags;
        public int num_entities;
        public int num_statics;
        public int viewent;

        public int cdtrack, looptrack;

        public scoreboard_t* scores;

#if QUAKE2
        public int light_level;
#endif
    }

    public cvar_c.cvar_t cl_name;
    public cvar_c.cvar_t cl_color;

    public cvar_c.cvar_t cl_upspeed;
    public cvar_c.cvar_t cl_forwardspeed;
    public cvar_c.cvar_t cl_backspeed;
    public cvar_c.cvar_t cl_sidespeed;

    public cvar_c.cvar_t cl_movespeedkey;

    public cvar_c.cvar_t cl_yawspeed;
    public cvar_c.cvar_t cl_pitchspeed;

    public cvar_c.cvar_t cl_anglespeedkey;

    public cvar_c.cvar_t cl_autofire;

    public cvar_c.cvar_t cl_shownet;
    public cvar_c.cvar_t cl_nolerp;

    public cvar_c.cvar_t cl_pitchdriftspeed;
    public cvar_c.cvar_t lookspring;
    public cvar_c.cvar_t lookstrafe;
    public cvar_c.cvar_t sensitivity;

    public cvar_c.cvar_t m_pitch;
    public cvar_c.cvar_t m_yaw;
    public cvar_c.cvar_t m_forward;
    public cvar_c.cvar_t m_side;

    public const int MAX_TEMP_ENTITIES = 64;
    public const int MAX_STATIC_ENTITIES = 128;

    public client_state_t cl;

    public render_c.efrag_t[] cl_efrags = new render_c.efrag_t[MAX_EFRAGS];
    public static render_c.entity_t[] cl_entities = new render_c.entity_t[quakedef_c.MAX_EDICTS];
    public render_c.entity_t[] cl_static_entities = new render_c.entity_t[MAX_STATIC_ENTITIES];
    public lightstyle_t[] cl_lightstyle = new lightstyle_t[quakedef_c.MAX_LIGHTSTYLES];
    public dlight_t[] cl_dlights = new dlight_t[MAX_DLIGHTS];
    public render_c.entity_t[] cl_temp_entities = new render_c.entity_t[MAX_TEMP_ENTITIES];
    public beam_t[] cl_beams = new beam_t[MAX_BEAMS];

    public const int MAX_VISEDICTS = 256;
    public static int cl_numvisedicts;
    public static render_c.entity_t* cl_visedicts;

    public struct kbutton_t
    {
        public int[] down;
        public int state;
    }

    public kbutton_t in_mlook, in_klook;
    public kbutton_t in_strafe;
    public kbutton_t in_speed;
}