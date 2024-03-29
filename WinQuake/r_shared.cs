﻿namespace Quake;

public unsafe class r_shared_c
{
#if !GLQUAKE

    public const int MAXVERTS = 16;
    public const int MAXWORKINGVERTS = (MAXVERTS + 4);

    public const int MAXHEIGHT = 1024;
    public const int MAXWIDTH = 1280;
    public const int MAXDIMENSION = ((MAXHEIGHT > MAXWIDTH) ? MAXHEIGHT : MAXWIDTH);

    public const int SIN_BUFFER_SIZE = (MAXDIMENSION + CYCLE);

    public const int INFINITE_DISTANCE = 0x10000;

    public static int cachewidth;
    public static byte* cacheblock;
    public static int screenwidth;

    public static float pixelAspect;

    public static int r_drawnpolycount;

    public cvar_c.cvar_t r_clearcolor;

    public static int[] sintable = new int[SIN_BUFFER_SIZE];
    public static int[] intsintable = new int[SIN_BUFFER_SIZE];

    public static Vector3 vup, base_vup;
    public static Vector3 vpn, base_vpn;
    public static Vector3 vright, base_vright;
    public static render_c.entity_t* currententity;

    public const int NUMSTACKEDGES = 2400;
    public const int MINEDGES = NUMSTACKEDGES;
    public const int NUMSTACKSURFACES = 800;
    public const int MINSURFACES = NUMSTACKSURFACES;
    public const int MAXSPANS = 3000;

    public struct espan_t
    {
        public int u, v, count;
        public espan_t* pnext;
    }

    public struct surf_t
    {
        public surf_t* next;
        public surf_t* prev;
        public espan_t* spans;
        public int key;
        public int last_u;
        public int spanstate;

        public int flags;
        public void* data;
        public render_c.entity_t* entity;
        public float nearzi;
        public bool insubmodel;
        public float d_ziorigin, d_zistepu, d_zistepv;

        public int[] pad;
    }

    public static surf_t* surfaces, surface_p, surf_max;

    public static Vector3[] sxformaxis = new Vector3[4];
    public static Vector3[] txformaxis = new Vector3[4];

    public static Vector3 modelorg, base_modelorg;

    public static float xcenter, ycenter;
    public static float xscale, yscale;
    public static float xscaleinv, yscaleinv;
    public static float xscaleshrink, yscaleshrink;

    public static int[] d_lightstylevalue = new int[256];

    public static int r_skymade;

    public static int ubasestep, errorterm, erroradjustup, erroradjustdown;

    public const int ALIAS_LEFT_CLIP = 0x0001;
    public const int ALIAS_TOP_CLIP = 0x0002;
    public const int ALIAS_RIGHT_CLIP = 0x0004;
    public const int ALIAS_BOTTOM_CLIP = 0x0008;
    public const int ALIAS_Z_CLIP = 0x0010;
    public const int ALIAS_ONSEAM = 0x0020;

    public const int ALIAS_XY_CLIP_MASK = 0x000F;

    public struct edge_t
    {
        public int u;
        public int u_step;
        public edge_t* prev, next;
        public ushort[] surfs;
        public edge_t* nextremove;
        public float nearzi;
        public model_c.medge_t* owner;
    }

#endif
}