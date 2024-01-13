namespace Quake;

public unsafe class sv_phys_c
{
    public static cvar_c.cvar_t sv_friction = new cvar_c.cvar_t { name = "sv_friction", value = (char)0, archive = true };
    public static cvar_c.cvar_t sv_stopspeed = new cvar_c.cvar_t { name = "sv_stopspeed", value = (char)100 };
    public static cvar_c.cvar_t sv_gravity = new cvar_c.cvar_t { name = "sv_gravity", value = (char)800, archive = false, server = true };
    public static cvar_c.cvar_t sv_maxvelocity = new cvar_c.cvar_t { name = "sv_maxvelocity", value = (char)2000 };
    public static cvar_c.cvar_t sv_nostep = new cvar_c.cvar_t { name = "sv_nostep", value = (char)0 };

#if QUAKE2
    public static Vector3 vec_origin = new Vector3 (0, 0, 0);
#endif

    public const double MOVE_EPSILON = 0.01;

    public static void SV_CheckAllEnts()
    {
        int e;
        progs_c.edict_t* check;

        check = progs_c.NEXT_EDICT(server_c.sv.edicts);

        for (e = 1; e < server_c.sv.num_edicts; e++, check = progs_c.NEXT_EDICT(check))
        {
            if (check->free)
            {
                continue;
            }

            if (check->v.movetype == server_c.MOVETYPE_PUSH || check->v.movetype == server_c.MOVETYPE_NONE
#if QUAKE2
                || check->v.movetype == server_c.MOVETYPE_FOLLOW
#endif
                    || check->v.movetype == server_c.MOVETYPE_NOCLIP)
            {
                continue;
            }

            if (world_c.SV_TestEntityPosition(check) != null)
            {
                console_c.Con_Printf("entity in invalid position\n");
            }
        }
    }

    public static void SV_CheckVelocity(progs_c.edict_t* ent)
    {
        int i;

        for (i = 0; i < 3; i++)
        {
            if (mathlib_c.IS_NAN(ent->v.velocity[i]))
            {
                console_c.Con_Printf($"Got a NaN velocity on {*pr_edict_c.pr_strings + ent->v.classname}\n");
                ent->v.velocity[i] = 0;
            }

            if (mathlib_c.IS_NAN(ent->v.origin[i]))
            {
                console_c.Con_Printf($"Got a NaN origin on {*pr_edict_c.pr_strings + ent->v.classname}\n");
                ent->v.origin[i] = 0;
            }

            if (ent->v.velocity[i] > sv_maxvelocity.value)
            {
                ent->v.velocity[i] = sv_maxvelocity.value;
            }
            else if (ent->v.velocity[i] < -sv_maxvelocity.value)
            {
                ent->v.velocity[i] = -sv_maxvelocity.value;
            }
        }
    }

    public static bool SV_RunThink(progs_c.edict_t* ent)
    {
        float thinktime;

        thinktime = ent->v.nextthink;

        if (thinktime <= 0 || thinktime > server_c.sv.time + host_c.host_frametime)
        {
            return true;
        }

        if (thinktime < server_c.sv.time)
        {
            thinktime = (float)server_c.sv.time;
        }

        ent->v.nextthink = 0;
        pr_edict_c.pr_global_struct->time = thinktime;
        pr_edict_c.pr_global_struct->self = progs_c.EDICT_TO_PROG(ent);
        pr_edict_c.pr_global_struct->other = progs_c.EDICT_TO_PROG(server_c.sv.edicts);
        pr_exec_c.PR_ExecuteProgram(ent->v.think);
        return !ent->free;
    }

    public static void SV_Impact(progs_c.edict_t* e1, progs_c.edict_t* e2)
    {
        int old_self, old_other;

        old_self = pr_edict_c.pr_global_struct->self;
        old_other = pr_edict_c.pr_global_struct->other;

        pr_edict_c.pr_global_struct->time = (float)server_c.sv.time;

        if (e1->v.touch != null && e1->v.solid != server_c.SOLID_NOT)
        {
            pr_edict_c.pr_global_struct->self = progs_c.EDICT_TO_PROG(e1);
            pr_edict_c.pr_global_struct->other = progs_c.EDICT_TO_PROG(e2);
            pr_exec_c.PR_ExecuteProgram(e1->v.touch);
        }

        if (e2->v.touch != null && e2->v.solid != server_c.SOLID_NOT)
        {
            pr_edict_c.pr_global_struct->self = progs_c.EDICT_TO_PROG(e2);
            pr_edict_c.pr_global_struct->other = progs_c.EDICT_TO_PROG(e1);
            pr_exec_c.PR_ExecuteProgram(e2->v.touch);
        }

        pr_edict_c.pr_global_struct->self = old_self;
        pr_edict_c.pr_global_struct->other = old_other;
    }

    public const double STOP_EPSILON = 0.1;

    public static int ClipVelocity(Vector3 input, Vector3 normal, Vector3 output, float overbounce)
    {
        float backoff;
        float change;
        int i, blocked;

        blocked = 0;

        if (normal[2] > 0)
        {
            blocked |= 1;
        }

        if (normal[2] == 0)
        {
            blocked |= 2;
        }

        backoff = mathlib_c.DotProduct(input, normal) * overbounce;

        for (i = 0; i < 3; i++)
        {
            change = normal[i] * backoff;
            output[i] = input[i] - change;

            if (output[i] > STOP_EPSILON && output[i] < STOP_EPSILON)
            {
                output[i] = 0;
            }
        }

        return blocked;
    }

    public const int MAX_CLIP_PLANES = 5;

    public static int SV_FlyMove(progs_c.edict_t* ent, float time, world_c.trace_t* steptrace)
    {
        int bumpcount, numbumps;
        Vector3 dir;
        float d;
        int numplanes;
        Vector3* planes;
        Vector3 primal_velocity, original_velocity, new_velocity;
        int i, j;
        world_c.trace_t trace;
        Vector3 end;
        float time_left;
        int blocked;

        primal_velocity = original_velocity = new_velocity = end = new();

        numbumps = 4;

        blocked = 0;
        mathlib_c.VectorCopy(ent->v.velocity, original_velocity);
        mathlib_c.VectorCopy(ent->v.velocity, primal_velocity);
        numplanes = 0;

        time_left = time;

        for (bumpcount = 0; bumpcount < numbumps; bumpcount++)
        {
            if (ent->v.velocity[0] == 0 && ent->v.velocity[1] == 0 && ent->v.velocity[2] == 0)
            {
                break;
            }

            for (i = 0; i < 3; i++)
            {
                end[i] = ent->v.origin[i] + time_left * ent->v.velocity[i];
            }

            trace = world_c.SV_Move(ent->v.origin, ent->v.mins, ent->v.maxs, end, 0, ent);

            if (trace.allsolid)
            {
                mathlib_c.VectorCopy(mathlib_c.vec3_origin, ent->v.velocity);
                return 3;
            }

            if (trace.fraction > 0)
            {
                mathlib_c.VectorCopy(trace.endpos, ent->v.origin);
                mathlib_c.VectorCopy(ent->v.velocity, original_velocity);
                numplanes = 0;
            }

            if (trace.fraction == 1)
            {
                break;
            }

            if (trace.ent == null)
            {
                sys_win_c.Sys_Error("SV_FlyMove: !trace.ent");
            }

            if (trace.plane.normal[2] > 0.7f)
            {
                blocked |= 1;

                if (trace.ent->v.solid == server_c.SOLID_BSP)
                {
                    ent->v.flags = (int)ent->v.flags | server_c.FL_ONGROUND;
                    ent->v.groundentity = progs_c.EDICT_TO_PROG(trace.ent);
                }
            }

            if (trace.plane.normal[2] == 0)
            {
                blocked |= 2;

                if (steptrace == null)
                {
                    *steptrace = trace;
                }
            }

            SV_Impact(ent, trace.ent);

            if (ent->free)
            {
                break;
            }

            time_left -= time_left * trace.fraction;

            if (numplanes >= MAX_CLIP_PLANES)
            {

            }
        }
    }
}