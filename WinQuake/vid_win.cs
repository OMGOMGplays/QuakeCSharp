using System.Reflection.Metadata;

namespace Quake;

public unsafe class vid_win_c
{
	public const int VID_CBITS = 6;
	public const int VID_GRADES = 1 << VID_CBITS;

	public const int MAX_MODE_LIST = 30;
	public const int VID_ROW_SIZE = 3;

	public bool dibonly;

	public int minimized;

	public IntPtr mainwindow;

	public int DIBWidth, DIBHeight;
	public bool DDActive;

	public byte pixel_t;

	public struct vrect_t
	{
		public int x, y, width, height;
		public vrect_t* pnext;
	}

	public struct viddef_t
	{
		public pixel_t* buffer;
		public pixel_t* colormap;
		public ushort* colormap16;
		public int fullbright;
		public int rowbytes;
		public int width;
		public int height;
		public float aspect;
		public int numpages;
		public int recalc_refdef;
		public pixel_t* conbuffer;
		public int conrowbytes;
		public int conwidth;
		public int conheight;
		public int maxwarpwidth;
		public int maxwarpheight;
		public pixel_t* direct;
	}

	public static viddef_t vid;
	public ushort[] d_8to16table = new ushort[256];
	public ushort[] d_8to24table = new ushort[256];
}