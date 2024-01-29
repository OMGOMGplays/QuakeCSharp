namespace Quake;

public unsafe class glquake_c
{
#pragma warning(disable : 4244)
#pragma warning(disable : 4136)
#pragma warning(disable : 4051)

    public static int texture_extension_number;
    public static int texture_mode;

    public static float gldepthmin, gldepthmax;

    public struct glvert_t
    {
        public float x, y, z;
        public float s, t;
        public float r, g, b;
    }

    public static glvert_t glv;

    public static int glx, gly, glwidth, glheight;

    public const double ALIAS_BASE_SIZE_RATIO = 1.0 / 11.0;

    public const int MAX_LBM_HEIGHT = 480;

    public const int TILE_SIZE = 128;

    public const int SKYSHIFT = 7;
    public const int SKYSIZE = 1 << SKYSHIFT;
    public const int SKYMASK = SKYSIZE - 1;

    public const double BACKFACE_ESPILON = 0.01;

    public struct surfcache_t
    {
        public surfcache_t* next;
        public surfcache_t** owner;
        public int* lightadj;
        public int dlight;
        public int size;
        public uint width;
        public uint height;
        public float mipscale;
        public model_c.texture_t* texture;
        public byte* data;
    }

    public struct drawsurf_t
    {
        public byte* surfdat;
        public int rowbytes;
        public model_c.msurface_t* surf;
        public int lightadj;

        public model_c.texture_t* texture;
        public int surfmip;
        public int surfwidth;
        public int surfheight;
    }

    public enum ptype_t { pt_static, pt_grav, pt_slowgrav, pt_fire, pt_explode, pt_explode2, pt_blob, pt_blob2 }

    public struct particle_t
    {
        public Vector3 org;
        public float color;

        public particle_t* next;
        public Vector3 vel;
        public float ramp;
        public float die;
        public ptype_t type;
    }
}