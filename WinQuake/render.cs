namespace Quake;

public unsafe class render_c
{
    public const int MAXCLIPPLANES = 11;

    public const int TOP_RANGE = 16;
    public const int BOTTOM_RANGE = 96;

    public struct efrag_t
    {
        public model_c.mleaf_t* leaf;
        public efrag_t* leafnext;
        public entity_t* entity;
        public efrag_t* entnext;
    }

    public struct entity_t
    {
        public bool forcelink;

        public int update_type;

        public entity_state_t baseline;

        public double msgtime;
        public Vector3[] msg_origins;
        public Vector3 origin;
        public Vector3[] msg_angles;
        public Vector3 angles;
        public model_c.model_t* model;
        public efrag_t* efrag;
        public int frame;
        public float syncbase;
        public byte* colormap;
        public int effects;
        public int skinnum;
        public int visframe;

        public int dlightframe;
        public int dlightbits;

        public int trivial_accept;
        public model_c.mnode_t* topnode;
    }

    public struct refdef_t
    {
        public vid_c.vrect_t vrect;

        public vid_c.vrect_t aliasvrect;
        public int vrectright, vrectbottom;
        public int aliasvrectright, aliasvrectbottom;
        public float vrectrightedge;

        public float fvrectx, fvrecty;
        public float fvrectx_adj, fvrecty_adj;
        public int vrect_x_adj_shift20;
        public int vrectright_adj_shift20;
        public float fvrectright_adj, fvrectbottom_adj;

        public float fvrectright;
        public float fvrectbottom;
        public float horizontalFieldOfView;

        public float xOrigin;
        public float yOrigin;

        public Vector3 vieworg;
        public Vector3 viewangles;

        public float fov_x, fov_y;

        public int ambientlight;
    }

    public static int reinit_surfcache;

    public static refdef_t r_refdef;
    public static Vector3 r_origin, vpn, vright, vup;

    public static model_c.texture_t* r_notexture_mip;

    public static bool r_cache_trash;
}