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

    public static int CSHIFT_CONTENTS = 0;
    public static int CSHIFT_DAMAGE = 1;
    public static int CSHIFT_BONUS = 2;
    public static int CSHIFT_POWERUP = 3;
    public static int NUM_CSHIFTS = 4;

    public static int NAME_LENGTH = 64;

    public static int SIGNONS = 4;

    public static int MAX_DLIGHTS = 32;

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

    public static int MAX_BEAMS = 24;

    public struct beam_t
    {
        public int entity;
        public model_t* model;
        public float endtime;
        public Vector3 start, end;
    }

    public static int MAX_EFRAGS = 640;

    public static int MAX_MAPSTRING = 2048;
    public static int MAX_DEMOS = 8;
    public static int MAX_DEMONAME = 16;

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
        public qsocket_t* netcon;
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

        public model_t* model_precache;
        public sfx_t* sound_precache;

        public char[] levelname;
        public int viewentity;
        public int maxclients;
        public int gametype;

        public model_t* worldmodel;
        public efrag_t* free_efrags;
        public int num_entities;
        public int num_statics;
        public entity_t viewent;

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

    public static int MAX_TEMP_ENTITIES = 64;
    public static int MAX_STATIC_ENTITIES = 128;

    public client_state_t cl;

    public efrag_t[] cl_efrags = new efrag_t[MAX_EFRAGS];
    public static entity_t[] cl_entities = new entity_t[quakedef_c.MAX_EDICTS];
    public entity_t[] cl_static_entities = new entity_t[MAX_STATIC_ENTITIES];
    public lightstyle_t[] cl_lightstyle = new lightstyle_t[quakedef_c.MAX_LIGHTSTYLES];
    public dlight_t[] cl_dlights[quakedef_c.MAX_DLIGHTS];
    public entity_t[] cl_temp_entities = new entity_t[MAX_TEMP_ENTITIES];
    public beam_t[] cl_beams = new beam_t[MAX_BEAMS];
    public dlight_t* CL_AllocDlight(int key)
    {
        
    }

    public static int MAX_VISEDICTS = 256;
    public static int cl_numvisedicts;
    public static entity_t* cl_visedicts;

    public struct kbutton_t
    {
        public int[] down;
        public int state;
    }

    public kbutton_t in_mlook, in_klook;
    public kbutton_t in_strafe;
    public kbutton_t in_speed;

}