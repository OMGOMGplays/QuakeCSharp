namespace Quake;

public unsafe class vid_c
{
    public const int VID_CBITS = 6;
    public const int VID_GRADES = (1 << VID_CBITS);

    public struct vrect_t
    {
        public int x, y, width, height;
        public vrect_t* pnext;
    }

    public struct viddef_t
    {
        public byte* buffer;
        public byte* colormap;
        public ushort* colormap16;
        public int fullbright;
        public uint rowbytes;
        public uint width;
        public uint height;
        public float aspect;
        public int numpages;
        public int recalc_refdef;
        public byte* conbuffer;
        public int conrowbytes;
        public uint conwidth;
        public uint conheight;
        public int maxwarpwidth;
        public int maxwarpheight;
        public byte* direct;
    }

    public static viddef_t vid;
    public static ushort[] d_8to16table = new ushort[256];
    public static ushort[] d_8to24table = new ushort[256];
}