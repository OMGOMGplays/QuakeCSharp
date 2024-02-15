namespace Quake;

public unsafe class spritegn_c
{
    public const int SPRITE_VERSION = 1;

    public enum synctype_t { ST_SYNC = 0, ST_RAND }

    public struct dsprite_t
    {
        public int ident;
        public int version;
        public int type;
        public float boundingradius;
        public int width;
        public int height;
        public int numframes;
        public float beamlength;
        public synctype_t synctype;
    }

    public const int SPR_VP_PARALLEL_UPRIGHT = 0;
    public const int SPR_FACING_UPRIGHT = 1;
    public const int SPR_VP_PARALLEL = 2;
    public const int SPR_ORIENTED = 3;
    public const int SPR_VP_PARALLEL_ORIENTED = 4;

    public struct dspriteframe_t
    {
        public int* origin;
        public int width;
        public int height;
    }

    public struct dspritegroup_t
    {
        public int numframes;
    }

    public struct dspriteinterval_t
    {
        public float interval;
    }

    public enum spriteframetype_t { SPR_SINGLE = 0, SPR_GROUP }

    public struct dspriteframetype_t
    {
        public spriteframetype_t type;
    }

    public const int IDSPRITEHEADER = (('P' << 24) + ('S' << 16) + ('D' << 8) + 'I');
}