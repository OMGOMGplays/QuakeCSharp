namespace Quake;

public unsafe class bspfile_c
{
	public static int MAX_MAP_HULLS = 4;

	public static int MAX_MAP_MODELS = 256;
	public static int MAX_MAP_BRUSHES = 4096;
	public static int MAX_MAP_ENTITIES = 1024;
	public static int MAX_MAP_ENTSTRING = 65536;

	public static int MAX_MAP_PLANES = 32767;
	public static int MAX_MAP_NODES = 32767;
	public static int MAX_MAP_CLIPNODES = 32767;
	public static int MAX_MAP_LEAFS = 8192;
	public static int MAX_MAP_VERTS = 65535;
	public static int MAX_MAP_FACES = 65535;
	public static int MAX_MAP_MARKSURFACES = 65535;
	public static int MAX_MAP_TEXINFO = 4096;
	public static int MAX_MAP_EDGES = 256000;
	public static int MAX_MAP_SURFEDGES = 512000;
	public static int MAX_MAP_TEXTURES = 512;
	public static int MAX_MAP_MIPTEX = 0x200000;
	public static int MAX_MAP_LIGHTING = 0x100000;
	public static int MAX_MAP_VISIBILITY = 0x100000;

	public static int MAX_MAP_PORTALS = 65536;

	public static int MAX_KEY = 32;
	public static int MAX_VALUE = 1024;

	public static int BSPVERSION = 29;
	public static int TOOLVERSION = 2;

	public struct lump_t
	{
		public int fileofs, filelen;
	}

	public static int LUMP_ENTITES = 0;
	public static int LUMP_PLANES = 1;
	public static int LUMP_TEXTURES = 2;
	public static int LUMP_VERTEXES = 3;
	public static int LUMP_VISIBILITY = 4;
	public static int LUMP_NODES = 5;
	public static int LUMP_TEXINFO = 6;
	public static int LUMP_FACES = 7;
	public static int LUMP_LIGHTING = 8;
	public static int LUMP_CLIPNODES = 9;
	public static int LUMP_LEAFS = 10;
	public static int LUMP_MARKSURFACES = 11;
	public static int LUMP_EDGES = 12;
	public static int LUMP_SURFEDGES = 13;
	public static int LUMP_MODELS = 14;

	public static int HEADER_LUMPS = 15;

	public struct dmodel_t
	{
		public float[] mins, maxs;
		public float[] origin;
		public int[] headnode;
		public int visleafs;
		public int firstface, numfaces;
	}

	public struct dheader_t
	{
		public int version;
		public lump_t[] lumps;
	}

	public struct dmiptexlump_t
	{
		public int nummiptex;
		public int[] dataofs;
	}

	public static int MIPLEVELS = 4;

	public struct miptex_t
	{
		public char* name;
		public uint width, height;
		public uint[] offsets;
	}

	public struct dvertex_t
	{
		public float[] point;
	}

	public static int PLANE_X = 0;
	public static int PLANE_Y = 1;
	public static int PLANE_Z = 2;

	public static int PLANE_ANYX = 3;
	public static int PLANE_ANYY = 4;
	public static int PLANE_ANYZ = 5;

	public struct dplane_t
	{
		public float[] normal;
		public float dist;
		public int type;
	}

	public static int CONTENTS_EMPTY = -1;
	public static int CONTENTS_SOLID = -2;
	public static int CONTENTS_WATER = -3;
	public static int CONTENTS_SLIME = -4;
	public static int CONTENTS_LAVA = -5;
	public static int CONTENTS_SKY = -6;
	public static int CONTENTS_ORIGIN = -7;
	public static int CONTENTS_CLIP = -8;

	public static int CONTENTS_CURRENT_0 = -9;
	public static int CONTENTS_CURRENT_90 = -10;
	public static int CONTENTS_CURRENT_180 = -11;
	public static int CONTENTS_CURRENT_270 = -12;
	public static int CONTENTS_CURRENT_UP = -13;
	public static int CONTENTS_CURRENT_DOWN = -14;

	public struct dnode_t
	{
		public int planenum;
		public short[] children;
		public short[] mins;
		public short[] maxs;
		public ushort firstface;
		public ushort numfaces;
	}

	public struct dclipnode_t
	{
		public int planenum;
		public short[] children;
	}

	public struct texinfo_t
	{
		public float[][] vecs;
		public int miptex;
		public int flags;
	}

	public static int TEX_SPECIAL = 1;

	public struct dedge_t
	{
		public ushort[] v;
	}

	public static int MAXLIGHTMAPS = 4;

	public struct dface_t
	{
		public short planenum;
		public short side;

		public int firstedge;
		public short numedges;
		public short texinfo;

		public byte[] styles;
		public int lightofs;
	}

	public static int AMBIENT_WATER = 0;
	public static int AMBIENT_SKY = 1;
	public static int AMBIENT_SLIME = 2;
	public static int AMBIENT_LAVA = 3;

	public static int NUM_AMBIENTS = 4;

	public struct dleaf_t
	{
		public int contents;
		public int visofs;

		public short[] mins;
		public short[] maxs;

		public ushort firstmarksurface;
		public ushort nummarksurfaces;

		public byte[] ambient_level;
	}

#if !QUAKE_GAME

	public static int ANGLE_UP = -1;
	public static int ANGLE_DOWN = -2;

	public static int nummodels;
	public static dmodel_t[] dmodels = new dmodel_t[MAX_MAP_MODELS];

	public static int visdatasize;
	public static byte[] dvisdata = new byte[MAX_MAP_VISIBILITY];

	public static int lightdatasize;
	public static byte[] dlightdata = new byte[MAX_MAP_LIGHTING];

	public static int texdatasize;
	public static byte[] dtexdata = new byte[MAX_MAP_MIPTEX];

	public static int entdatasize;
	public static char[] dentdata = new char[MAX_MAP_ENTSTRING];

	public static int numleafs;
	public static dleaf_t[] dleafs = new dleaf_t[MAX_MAP_LEAFS];

	public static int numplanes;
	public static dplane_t[] dplanes = new dplane_t[MAX_MAP_PLANES];

	public static int numvertexes;
	public static dvertex_t[] dvertexes = new dvertex_t[MAX_MAP_VERTS];

	public static int numnodes;
	public static dnode_t[] dnodes = new dnode_t[MAX_MAP_NODES];

	public static int numtexinfo;
	public static texinfo_t[] texinfo = new texinfo_t[MAX_MAP_TEXINFO];

	public static int numfaces;
	public static dface_t[] dfaces = new dface_t[MAX_MAP_FACES];

	public static int numclipnodes;
	public static dclipnode_t[] dclipnodes = new dclipnode_t[MAX_MAP_CLIPNODES];

	public static int numedges;
	public static dedge_t[] dedges = new dedge_t[MAX_MAP_EDGES];

	public static int nummarksurfaces;
	public static ushort[] dmarksurfaces = new ushort[MAX_MAP_MARKSURFACES];

	public static int numsurfedges;
	public static int[] dsurfedges = new int[MAX_MAP_SURFEDGES];

	public struct epair_t
	{
		public epair_t* next;
		public char* key;
		public char* value;
	}

	public struct entity_t
	{
		public Vector3 origin;
		public int firstbrush;
		public int numbrushes;
		public epair_t* epairs;
	}

	public static int num_entities;
	public static entity_t[] entities = new entity_t[MAX_MAP_ENTITIES];

#endif
}