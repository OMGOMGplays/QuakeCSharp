namespace Quake;

public unsafe class chase_c
{
    public static cvar_c.cvar_t chase_back = new cvar_c.cvar_t { name = "chase_back", value = (char)100 };
    public static cvar_c.cvar_t chase_up = new cvar_c.cvar_t { name = "chase_up", value = (char)16 };
    public static cvar_c.cvar_t chase_right = new cvar_c.cvar_t { name = "chase_right", value = (char)0 };
    public static cvar_c.cvar_t chase_active = new cvar_c.cvar_t { name = "chase_active", value = (char)0 };

    public static Vector3 chase_pos;
    public static Vector3 chase_angles;

    public static Vector3 chase_dest;
    public static Vector3 chase_dest_angles;

    public static void Chase_Init()
    {
        cvar_c.Cvar_RegisterVariable(chase_back);
        cvar_c.Cvar_RegisterVariable(chase_up);
        cvar_c.Cvar_RegisterVariable(chase_right);
        cvar_c.Cvar_RegisterVariable(chase_active);
    }

    public static void Chase_Reset()
    {

    }

    public static void TraceLine(Vector3 start, Vector3 end, Vector3 impact)
    {
        world_c.trace_t trace = new();

        common_c.Q_memset(trace, 0, sizeof(world_c.trace_t));
        world_c.SV_RecursiveHullCheck(cl_main_c.cl.worldmodel->hulls, 0, 0, 1, start, end, &trace);

        mathlib_c.VectorCopy(trace.endpos, impact);
    }

    public static void Chase_Update()
    {
        int i;
        float dist;
        Vector3 forward, up, right;
        Vector3 dest, stop;

        forward = up = right = dest = stop = new();

        mathlib_c.AngleVectors(cl_main_c.cl.viewangles, forward, right, up);

        for (i = 0; i < 3; i++)
        {
            chase_dest[i] = r_main_c.r_refdef.vieworg[i] - forward[i]*chase_back.value - right[i]*chase_right.value; 
        }

        chase_dest[2] = r_main_c.r_refdef.vieworg[2] + chase_up.value;

        mathlib_c.VectorMA(r_main_c.r_refdef.vieworg, 4096, forward, dest);
        TraceLine(r_main_c.r_refdef.vieworg, dest, stop);

        mathlib_c.VectorSubtract(stop, r_main_c.r_refdef.vieworg, stop);
        dist = mathlib_c.DotProduct(stop, forward);

        if (dist < 1)
        {
            dist = 1;
        }

        r_main_c.r_refdef.viewangles[quakedef_c.PITCH] = -MathF.Atan(stop[2] / dist) / MathF.PI * 180;

        mathlib_c.VectorCopy(chase_dest, r_main_c.r_refdef.vieworg);
    }
}