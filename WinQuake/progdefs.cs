﻿namespace Quake;

public unsafe class progdefs_c
{
    // THIS IS QUAKE 1!!!
    // If I am to implement Quake 2, fuck with this
    public struct globalvars_t
    {
        public int[] pad;
        public int self;
        public int other;
        public int world;
        public float time;
        public float frametime;
        public float force_retouch;
        public char* mapname;
        public float deathmatch;
        public float coop;
        public float teamplay;
        public float serverflags;
        public float total_secrets;
        public float total_monsters;
        public float found_secrets;
        public float killed_monsters;
        public float parm1;
        public float parm2;
        public float parm3;
        public float parm4;
        public float parm5;
        public float parm6;
        public float parm7;
        public float parm8;
        public float parm9;
        public float parm10;
        public float parm11;
        public float parm12;
        public float parm13;
        public float parm14;
        public float parm15;
        public float parm16;
        public Vector3 v_forward;
        public Vector3 v_up;
        public Vector3 v_right;
        public float trace_allsolid;
        public float trace_startsolid;
        public float trace_fraction;
        public Vector3 trace_endpos;
        public Vector3 trace_plane_normal;
        public float trace_plane_dist;
        public int trace_ent;
        public float trace_inopen;
        public float trace_inwater;
        public int msg_entity;
        public Action main;
        public Action StartFrame;
        public Action PlayerPreThink;
        public Action PlayerPostThink;
        public Action ClientKill;
        public Action ClientConnect;
        public Action PutClientInServer;
        public Action ClientDisconnect;
        public Action SetNewParms;
        public Action SetChangeParms;
    }

    public struct entvars_t
    {
        public float modelindex;
        public Vector3 absmin;
        public Vector3 absmax;
        public float ltime;
        public float movetype;
        public float solid;
        public Vector3 origin;
        public Vector3 oldorigin;
        public Vector3 velocity;
        public Vector3 angles;
        public Vector3 avelocity;
        public Vector3 punchangle;
        public string classname;
        public char* model;
        public float frame;
        public float skin;
        public float effects;
        public Vector3 mins;
        public Vector3 maxs;
        public Vector3 size;
        public Action touch;
        public Action use;
        public Action think;
        public Action blocked;
        public float nextthink;
        public int groundentity;
        public float health;
        public float frags;
        public float weapon;
        public string weaponmodel;
        public float weaponframe;
        public float currentammo;
        public float ammo_shells;
        public float ammo_nails;
        public float ammo_rockets;
        public float ammo_cells;
        public float items;
        public float takedamage;
        public int chain;
        public float deadflag;
        public Vector3 view_ofs;
        public float button0;
        public float button1;
        public float button2;
        public float impulse;
        public float fixangle;
        public Vector3 v_angle;
        public float idealpitch;
        public string netname;
        public int enemy;
        public float flags;
        public float colormap;
        public float team;
        public float max_health;
        public float teleport_time;
        public float armortype;
        public float armorvalue;
        public float waterlevel;
        public float watertype;
        public float ideal_yaw;
        public float yaw_speed;
        public int aiment;
        public int goalentity;
        public float spawnflags;
        public string target;
        public string targetname;
        public float dmg_take;
        public float dmg_save;
        public int dmg_inflictor;
        public int owner;
        public Vector3 movedir;
        public string message;
        public float sounds;
        public string noise;
        public string noise1;
        public string noise2;
        public string noise3;
    }
}