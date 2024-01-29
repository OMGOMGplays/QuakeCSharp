namespace Quake;

public unsafe class world_c
{
    public struct plane_t
    {
        public Vector3 normal;
        public float dist;
    }

    public struct trace_t
    {
        public bool allsolid;
        public bool startsolid;
        public bool inopen, inwater;
        public float fraction;
        public Vector3 endpos;
        public plane_t plane;
        public progs_c.edict_t* ent;
    }

    public const int MOVE_NORMAL = 0;
    public const int MOVE_NOMONSTERS = 1;
    public const int MOVE_MISSILE = 2;

    public struct moveclip_t
    {
        public Vector3 boxmins, boxmaxs;
        public float* mins, maxs;
        public Vector3 mins2, maxs2;
        public float* start, end;
        public int type;
        public progs_c.edict_t* passedict;
    }

    public static model_c.hull_t box_hull;
    public static bspfile_c.dclipnode_t* box_clipnodes;
    public static model_c.mplane_t* box_planes;

    public static void SV_InitBoxHull()
    {
        int i;
        int side;

        box_hull.clipnodes = box_clipnodes;
        box_hull.planes = box_planes;
        box_hull.firstclipnode = 0;
        box_hull.lastclipnode = 5;
    }
}