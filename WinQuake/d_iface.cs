namespace Quake;

public unsafe class d_iface_c
{
	public const int WARP_WIDTH = 320;
	public const int WARP_HEIGHT = 200;

	public const int MAX_LBM_HEIGHT = 480;

	public struct emitpoint_t
	{
		public float u, v;
		public float s, t;
		public float zi;
	}

	public enum ptype_t { pt_static, pt_grav, pt_slowgrav, pt_fire, pt_explode, pt_explode2, pt_blob, pt_blob2 };

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

	public const double PARTICLE_Z_CLIP = 8.0;

	public struct polyvert_t
	{
		public float u, v, zi, s, t;
	}

	public struct polydesc_t
	{
		public int numverts;
		public float nearzi;
		public model_c.msurface_t* pcurrentface;
		public polyvert_t* pverts;
	}

	public struct finalvert_t
	{
		public int* v;
		public int flags;
		public float reserved;
	}

	public struct affinetridesc_t
	{
		public void* pskin;
		public model_c.maliasskindesc_t* pskindesc;
		public int skinwidth;
		public int skinheight;
		public model_c.mtriangle_t* ptriangles;
		public finalvert_t* pfinalverts;
		public int numtriangles;
		public int drawtype;
		public int seamfixupX16;
	}

	public struct screenpart_t
	{
		public float u, v, zi, color;
	}

	public struct spritedesc_t
	{
		public int nump;
		public emitpoint_t* pverts;
		public model_c.mspriteframe_t* pspriteframe;
		public Vector3 vup, vright, vpn;
		public float nearzi;
	}

	public struct zpointdesc_t
	{
		public int u, v;
		public float zi;
		public int color;
	}

	public cvar_c.cvar_t r_drawflat;
	public int d_spanpixcount;
	public static int r_framecount;

	public static bool r_drawpolys;
	public static bool r_drawculledpolys;
	public static bool r_worldpolysbacktofront;
	public static bool r_recursiveaffinetriangles;

	public static float r_aliasuvscale;

	public static int r_pixbytes;
	public static bool r_dowarp;

	public static affinetridesc_t r_affinetridesc;
	public static spritedesc_t r_spritedesc;
	public static zpointdesc_t r_zpointdesc;
	public static polydesc_t r_polydesc;

	public static int d_con_indirect;

	public static Vector3 r_pright, r_pup, r_ppn;

	public static int r_skydirect;
	public static byte* r_skysource;

	public const int DR_SOLID = 0;
	public const int DR_TRANSPARENT = 1;

	public const int TRANSPARENT_COLOR = 0xFF;

	public static void* acolormap;

	public struct drawsurf_t
	{
		public byte* surfdat;
		public int rowbytes;
		public model_c.msurface_t* surf;
		public int* lightadj;

		public model_c.texture_t* texture;
		public int surfmip;
		public int surfwidth;
		public int surfheight;
	}

	public static drawsurf_t r_drawsurf;

	public const int TURB_TEX_SIZE = 64;

	public const int CYCLE = 128;

	public const int TILE_SIZE = 128;

	public const int SKYSHIFT = 7;
	public const int SKYSIZE = 1 << SKYSHIFT;
	public const int SKYMASK = SKYSIZE - 1;

	public static float skyspeed, skyspeed2;
	public static float skytime;

	public static int c_surf;
	public static vid_c.vrect_t scr_vrect;

	public static byte* r_warpbuffer;
}