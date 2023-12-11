using pixel_t = System.Byte;

namespace Quake;

public unsafe class vid_c
{
    public static int VID_CBITS = 6;
    public static int VID_GRADES = (1 << VID_CBITS);

    public struct vrect_t
    {
        public int x, y, width, height;
        public vrect_t* next;
    }

    public struct viddef_t
    {
        public pixel_t* buffer;
        public pixel_t* colormap;
        public ushort* colormap16;
        public int fullbright;
        public uint rowbytes;
        public uint width;
        public uint height;
        public float aspect;
        public int numpages;
        public int recalc_refdef;
        public pixel_t* conbuffer;
        public int conrowbytes;
        public uint conwidth;
        public uint conheight;
        public int maxwarpwidth;
        public int maxwarpheight;
        public pixel_t* direct;
    }

    public viddef_t vid;
    public ushort[] d_8to16table = new ushort[256];
    public ushort[] d_8to24table = new ushort[256];
    public delegate void* vid_menudrawfn();
    public delegate void* vid_menukeyfn(int key);
}