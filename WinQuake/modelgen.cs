namespace Quake;

public unsafe class modelgen_c
{
    public static int ALIAS_VERSION = 6;

    public static int ALIAS_ONSEAM = 0x0020;

    public enum synctype_t { ST_SYNC = 0, ST_RAND }

    public enum aliasframetype_t { ALIAS_SINGLE = 0, ALIAS_GROUP }

    public enum aliasskintype_t { ALIAS_SKIN_SINGLE = 0, ALIAS_SKIN_GROUP }

    public struct mdl_t
    {
        public int ident;
        public int version;
        public Vector3 scale;
        public Vector3 scale_origin;
        public float boundingradius;
        public Vector3 eyeposition;
        public int numskins;
        public int skinwidth;
        public int skinheight;
        public int numverts;
        public int numtris;
        public int numframes;
        public synctype_t synctype;
        public int flags;
        public float size;
    }

    public struct stvert_t
    {
        public int onseam;
        public int s;
        public int t;
    }

    public struct dtriangle_t
    {
        public int facesfront;
        public int[] vertindex;
    }

    public static int FACES_FRONT = 0x0010;

    public struct trivertx_t
    {
        public byte[] v;
        public byte lightnormalindex;
    }

    public struct daliasframe_t
    {
        public trivertx_t bboxmin;
        public trivertx_t bboxmax;
        public char[] name;
    }

    public struct daliasgroup_t
    {
        public int numframes;
        public trivertx_t bboxmin;
        public trivertx_t bboxmax;
    }

    public struct daliasskingroup_t
    {
        public int numskins;
    }

    public struct daliasinterval_t
    {
        public float interval;
    }

    public struct daliasskininterval_t
    {
        public float interval;
    }

    public struct daliasframetype_t
    {
        public aliasframetype_t type;
    }

    public struct daliasskintype_t
    {
        public aliasskintype_t type;
    }
}