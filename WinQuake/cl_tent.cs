namespace Quake;

public unsafe class cl_tent_c
{
    public static int MAX_BEAMS = 8;

    public struct beam_t
    {
        public int entity;
        public model_c.model_t* model;
        public float endtime;
        public Vector3 start, end;
    }

    public beam_t cl_beams;

    public static int MAX_EXPLOSIONS = 8;

    public struct explosion_t
    {
        public Vector3 origin;
        public float start;
        public model_c.model_t* model;
    }


}