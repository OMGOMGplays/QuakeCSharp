namespace Quake;

public unsafe class d_local_c
{
    public static int SCANBUFFERPAD = 0x1000;

    public static int R_SKY_SMASK = 0x007F0000;
    public static int R_SKY_TMASK = 0x007F0000;

    public static int DS_SPAN_LIST_END = -128;

    public static int SURFCACHE_SIZE_AT_320X200 = 600 * 1024;

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

    public cvar_c.cvar_t d_subdiv16;

    public float scale_for_mip;

    public bool d_roverwrapped;
    public surfcache_t* sc_rover;
    public surfcache_t* d_initial_rover;

    public float d_sdivzstepu, d_tdivzstepu, d_zistepu;
    public float d_sdivzstepv, d_tdivzstepv, d_zistepv;
    public float d_sdivzorigin, d_tdivzorigin, d_ziorigin;

    public int sadjust, tadjust;
    public int bbextents, bbextentt;

    public delegate void* prealspandrawer();

    public short* d_pzbuffer;
    public uint d_zrowbytes, d_zwidth;

    public int* d_pscantable;
    public int[] d_scantable = new int[r_shared_c.MAXHEIGHT];

    public int d_vrectx, d_vrecty, d_vrectright_particle, d_vrectbottom_particle;

    public int d_y_aspect_shift, d_pix_min, d_pix_max, d_pix_shift;

    public vid_c.pixel_t* d_viewbuffer;

    public short* zspantable;

    public int d_minmip;
    public float[] d_scalemip = new float[3];

    public delegate void* d_drawspans(r_shared_c.espan_t* pspan);
}