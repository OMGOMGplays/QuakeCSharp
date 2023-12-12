namespace Quake;

public unsafe class protocol_c
{
    public static int PROTOCOL_VERSION = 15;

    public static int U_MOREBITS = (1 << 0);
    public static int U_ORIGIN1 = (1 << 1);
    public static int U_ORIGIN2 = (1 << 2);
    public static int U_ORIGIN3 = (1 << 3);
    public static int U_ANGLE2 = (1 << 4);
    public static int U_NOLERP = (1 << 5);
    public static int U_FRAME = (1 << 6);
    public static int U_SIGNAL = (1 << 7);

    public static int U_ANGLE1 = (1 << 8);
    public static int U_ANGLE3 = (1 << 9);
    public static int U_MODEL = (1 << 10);
    public static int U_COLORMAP = (1 << 11);
    public static int U_SKIN = (1 << 12);
    public static int U_EFFECTS = (1 << 13);
    public static int U_LONGENTITY = (1 << 14);

    public static int SU_VIEWHEIGHT = (1 << 0);
    public static int SU_IDEALPITCH = (1 << 1);
    public static int SU_PUNCH1 = (1 << 2);
    public static int SU_PUNCH2 = (1 << 3);
    public static int SU_PUNCH3 = (1 << 4);
    public static int SU_VELOCITY1 = (1 << 5);
    public static int SU_VELOCITY2 = (1 << 6);
    public static int SU_VELOCITY3 = (1 << 7);
    //public static int SU_AIMENT = (1 << 8);
    public static int SU_ITEMS = (1 << 9);
    public static int SU_ONGROUND = (1 << 10);
    public static int SU_INWATER = (1 << 11);
    public static int SU_WEAPONFRAME = (1 << 12);
    public static int SU_ARMOR = (1 << 13);
    public static int SU_WEAPON = (1 << 14);

    public static int SND_VOLUME = (1 << 0);
    public static int SND_ATTENUATION = (1 << 1);
    public static int SND_LOOPING = (1 << 2);

    public static int DEFAULT_VIEWHEIGHT = 22;

    public static int GAME_COOP = 0;
    public static int GAME_DEATHMATCH = 1;

    public static int svc_bad = 0;
    public static int svc_nop = 1;
    public static int svc_disconnect = 2;
    public static int svc_updatestat = 3;
    public static int svc_version = 4;
    public static int svc_setview = 5;
    public static int svc_sound = 6;
    public static int svc_time = 7;
    public static int svc_print = 8;
    public static int svc_stufftext = 9;
    public static int svc_setangle = 10;
    public static int svc_serverinfo = 11;
    public static int svc_lightstyle = 12;
    public static int svc_updatename = 13;
    public static int svc_updatefrags = 14;
    public static int svc_clientdata = 15;
    public static int svc_stopsound = 16;
    public static int svc_updatecolors = 17;
    public static int svc_particle = 18;
    public static int svc_damage = 19;

    public static int svc_spawnstatic = 20;
    //public static int svc_spawnbinary = 21;
    public static int svc_spawnbaseline = 22;

    public static int svc_temp_entity = 23;

    public static int svc_setpause = 24;
    public static int svc_signonnum = 25;

    public static int svc_centerprint = 26;

    public static int svc_killedmonster = 27;
    public static int svc_foundsecret = 28;

    public static int svc_spawnstaticsound = 29;

    public static int svc_intermission = 30;
    public static int svc_finale = 31;

    public static int svc_cdtrack = 32;
    public static int svc_sellscreen = 33;

    public static int svc_cutscene = 34;

    public static int clc_bad = 0;
    public static int clc_nop = 1;
    public static int clc_disconnect = 2;
    public static int clc_move = 3;
    public static int clc_stringcmd = 4;

    public static int TE_SPIKE = 0;
    public static int TE_SUPERSPIKE = 1;
    public static int TE_GUNSHOT = 2;
    public static int TE_EXPLOSION = 3;
    public static int TE_TAREXPLOSION = 4;
    public static int TE_LIGHTNING1 = 5;
    public static int TE_LIGHTNING2 = 6;
    public static int TE_WIZSPIKE = 7;
    public static int TE_KNIGHTSPIKE = 8;
    public static int TE_LIGHTNING3 = 9;
    public static int TE_LAVASPLASH = 10;
    public static int TE_TELEPORT = 11;
    public static int TE_EXPLOSION2 = 12;

    public static int TE_BEAM = 13;

#if QUAKE2
    public static int TE_IMPLOSION = 14;
    public static int TE_RAILTRAIL = 15;
#endif
}