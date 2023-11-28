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
		public msurface_t* pcurrentface;
		public polyvert_t* pverts;
	}

	public struct finalvert_t
	{
		public int[] v;
		public int flags;
		public float reserved;
	}

	public struct affinetridesc_t
	{
		public void* pskin;
		public maliasskindesc_t* pskindesc;
		public int skinwidth;
		public int skinheight;
		public mtriangle_t* ptriangles;
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
		public mspriteframe_t* pspriteframe;
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
	public int r_framecount;
}