namespace Quake;

public unsafe class r_local_c
{
    public const float ALIAS_BASE_SIZE_RATIO = 1.0f / 11.0f;

    public const int BMODEL_FULLY_CLIPPED = 0x10;

    public struct alight_t
    {
        public int ambientlight;
        public int shadelight;
        public float* plightvec;
    }

    public struct bedge_t
    {
        public model_c.mvertex_t* v;
        public bedge_t* pnext;
    }

    public struct auxvert_t
    {
        public float* fv;
    }

    public const float XCENTERING = 1.0f / 2.0f;
    public const float YCENTERING = 1.0f / 2.0f;

    public const float CLIP_EPSILON = 0.001f;

    public const float BACKFACE_EPSILON = 0.01f;

    public const int DIST_NOT_SET = 98765;

    public struct clipplane_t
    {
        public Vector3 normal;
        public float dist;
        public clipplane_t* next;
        public byte leftedge;
        public byte rightedge;
        public byte* reserved;
    }

    public static clipplane_t* view_clipplanes;

    public static model_c.mplane_t* screenedge;

    public static Vector3 r_origin;

    public static Vector3 r_entorigin;

    public static float screenAspect;
    public static float verticalFieldOfView;
    public static float xOrigin, yOrigin;

    public static int r_visframecount;

    public static int vstartscan;

    public static bool insubmodel;
    public static Vector3 r_worldmodelorg;

    public static int c_faceclip;
    public static int r_polycount;
    public static int r_wholepolycount;

    public static model_c.model_t* cl_worldmodel;

    public static int* pfrustrum_indexes;

    public const float NEAR_CLIP = 0.01f;

    public static int ubasestep, errorterm, erroradjustup, erroradjustdown;

    public static int sadjust, tadjust;
    public static int bbextents, bbextentt;

    public const int MAXBVERTINDICES = 1000;

    public static model_c.mvertex_t* r_ptverts, r_ptvertsmax;

    public static Vector3* sbaseaxis, tbaseaxis;
    public static float[][] entity_rotation;

    public static int r_currentkey;
    public static int r_currentbkey;

    public struct btofpoly_t
    {
        int clipflags;
        model_c.msurface_t* psurf;
    }

    public const int MAX_BTOFPOLYS = 5000;

    public static int numbtofpolys;
    public static btofpoly_t* pbtofpolys;

    public const int MAXALIASVERTS = 2000;
    public const int ALIAS_Z_CLIP_PLANE = 5;

    public static int numverts;
    public static int a_skinwidth;
    public static model_c.mtriangle_t* ptriangles;
    public static int numtriangles;
    public static model_c.aliashdr_t* paliashdr;
    public static modelgen_c.mdl_t* pmdl;
    public static float leftclip, topclip, rightclip, bottomclip;
    public static int r_acliptype;
    public static d_iface_c.finalvert_t* pfinalverts;
    public static auxvert_t* pauxverts;

    public const int AMP = 8 * 0x10000;
    public const int AMP2 = 3;
    public const int SPEED = 20;

    public static int r_amodels_drawn;
    public static r_shared_c.edge_t* auxedges;
    public static int r_numallocatededges;
    public static r_shared_c.edge_t* r_edges, edge_p, edge_max;

    public static r_shared_c.edge_t* newedges;
    public static r_shared_c.edge_t* removeedges;

    public static int screenwidth;

    public static r_shared_c.edge_t* edge_head;
    public static r_shared_c.edge_t* edge_tail;
    public static r_shared_c.edge_t* edge_aftertail;
    public static int r_bmodelactive;
    public static vid_c.vrect_t* pconupdate;

    public static float aliasxscale, aliasyscale, aliasxcenter, aliasycenter;
    public static float r_aliastransition, r_resfudge;

    public static int r_outofsurfaces;
    public static int r_outofedges;

    public static model_c.mvertex_t* r_pcurrentvertbase;
    public static int r_maxvalidedgeoffset;

    public static float r_time1;
    public static float dp_time1, dp_time2, db_time1, db_time2, rw_time1, rw_time2;
    public static float se_time1, se_time2, de_time1, de_time2, dv_time1, dv_time2;
    public static int* r_frustrum_indexes;
    public static int r_maxsurfseen, r_maxedgeseen, r_cnumsurfs;
    public static bool r_surfsonstack;
    public static client_c.cshift_t cshift_water;
    public static bool r_dowarpold, r_viewchanged;

    public static model_c.mleaf_t* r_viewleaf, r_oldviewleaf;

    public static Vector3 r_emins, r_emaxs;
    public static model_c.mnode_t* r_pefragtopnode;
    public static int r_clipflags;
    public static int r_dlightframecount;
    public static bool r_fov_greater_than_90;
}