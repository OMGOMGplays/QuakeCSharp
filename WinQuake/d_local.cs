namespace Quake;

public unsafe class d_local_c
{
    public const int SCANBUFFERPAD = 0x1000;

    public const int R_SKY_SMASK = 0x007F0000;
    public const int R_SKY_TMASK = 0x007F0000;

    public const int DS_SPAN_LIST_END = -128;

    public const int SURFCACHE_SIZE_AT_320X200 = 600 * 1024;

    public struct surfcache_t
    {
        public surfcache_t* next;
        public surfcache_t** owner;
        public int[] lightadj;
        public int dlight;
        public int size;
        public uint width;
        public uint height;
        public float mipscale;
        public model_c.texture_t* texture;
        public byte[] data;
    }

    public struct sspan_t
    {
        public int u, v, count;
    }

    public static cvar_c.cvar_t d_subdiv16;

    public static float scale_for_mip;

    public static bool d_roverwrapped;
    public static surfcache_t* sc_rover;
    public static surfcache_t* d_initial_rover;

    public static float d_sdivzstepu, d_tdivzstepu, d_zistepu;
    public static float d_sdivzstepv, d_tdivzstepv, d_zistepv;
    public static float d_sdivzorigin, d_tdivzorigin, d_ziorigin;

    public static int sadjust, tadjust;
    public static int bbextents, bbextentt;

    public static short* d_pzbuffer;
    public static uint d_zrowbytes, d_zwidth;

    public static int* d_pscantable;
    public static int[] d_scantable = new int[r_shared_c.MAXHEIGHT];

    public static int d_vrectx, d_vrecty, d_vrectright_particle, d_vrectbottom_particle;

    public static int d_y_aspect_shift, d_pix_min, d_pix_max, d_pix_shift;

    public static byte* d_viewbuffer;

    public static short* zspantable;

    public static int d_minmip;
    public float[] d_scalemip = new float[3];
}