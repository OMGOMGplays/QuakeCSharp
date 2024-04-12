namespace Quake;

public unsafe class sv_move_c
{
    public const int STEPSIZE = 18;

    public static int c_yes, c_no;

    public static bool SV_CheckBottom(progs_c.edict_t* ent)
    {
        Vector3 mins, maxs, start, stop;
        world_c.trace_t trace;
        int x, y;
        float mid, bottom;

        mins = maxs = start = stop = new();

        mathlib_c.VectorAdd(ent->v.origin, ent->v.mins, mins);
        mathlib_c.VectorAdd(ent->v.origin, ent->v.maxs, maxs);

        start[2] = mins[2] - 1;

        for (x = 0; x <= 1; x++)
        {
            for (y = 0; y <= 1; y++)
            {
                start[0] = x == 1 ? maxs[0] : mins[0];
                start[1] = y == 1 ? maxs[1] : mins[1];

                if (world_c.SV_PointContents(start) != bspfile_c.CONTENTS_SOLID)
                {
                    goto realcheck;
                }
            }
        }

        c_yes++;
        return true;


    realcheck:
        c_no++;

        start[2] = mins[2];

        start[0] = stop[0] = (mins[0] + maxs[0]) * 0.5f;
        start[1] = stop[1] = (mins[1] + maxs[1]) * 0.5f;
        stop[2] = start[2] - 2 * STEPSIZE;
        trace = world_c.SV_Move(start, mathlib_c.vec3_origin, mathlib_c.vec3_origin, stop, 1, ent);

        if (trace.fraction == 1.0f)
        {
            return false;
        }

        mid = bottom = trace.endpos[2];

        for (x = 0; x <= 1; x++)
        {
            for (y = 0; y <= 1; y++)
            {
                start[0] = stop[0] = x == 1 ? maxs[0] : mins[0];
                start[1] = stop[1] = y == 1 ? maxs[1] : mins[1];

                trace = world_c.SV_Move(start, mathlib_c.vec3_origin, mathlib_c.vec3_origin, stop, 1, ent);

                if (trace.fraction != 1.0f && trace.endpos[2] > bottom)
                {
                    bottom = trace.endpos[2];
                }

                if (trace.fraction == 1.0f || mid - trace.endpos[2] > STEPSIZE)
                {
                    return false;
                }
            }

        }

        c_yes++;
        return true;
    }

    public static bool SV_movestep(progs_c.edict_t* ent, Vector3 move, bool relink)
    {
        float dz;
        Vector3 oldorg, neworg, end;
        world_c.trace_t trace;
        int i;
        progs_c.edict_t* enemy;

        oldorg = neworg = end = new();

        mathlib_c.VectorCopy(ent->v.origin, oldorg);
        mathlib_c.VectorAdd(ent->v.origin, move, neworg);

        if (((int)ent->v.flags & (server_c.FL_SWIM | server_c.FL_FLY)) != 0)
        {
            for (i = 0; i < 2; i++)
            {
                mathlib_c.VectorAdd(ent->v.origin, move, neworg);
                enemy = progs_c.PROG_TO_EDICT(ent->v.enemy);

                if (i == 0 && enemy != server_c.sv.edicts)
                {
                    dz = ent->v.origin[2] - progs_c.PROG_TO_EDICT(ent->v.enemy)->v.origin[2];

                    if (dz > 40)
                    {
                        neworg[2] -= 8;
                    }

                    if (dz < 30)
                    {
                        neworg[2] += 8;
                    }
                }

                trace = world_c.SV_Move(ent->v.origin, ent->v.mins, ent->v.maxs, neworg, 0, ent);

                if (trace.fraction == 1)
                {
                    if (((int)ent->v.flags & server_c.FL_SWIM) != 0 && world_c.SV_PointContents(trace.endpos) == bspfile_c.CONTENTS_EMPTY)
                    {
                        return false;
                    }

                    mathlib_c.VectorCopy(trace.endpos, ent->v.origin);

                    if (relink)
                    {
                        world_c.SV_LinkEdict(ent, true);
                    }

                    return true;
                }

                if (enemy == server_c.sv.edicts)
                {
                    break;
                }
            }

            return false;
        }

        neworg[2] += STEPSIZE;
        mathlib_c.VectorCopy(neworg, end);
        end[2] -= STEPSIZE * 2;

        trace = world_c.SV_Move(neworg, ent->v.mins, ent->v.maxs, end, 0, ent);

        if (trace.allsolid)
        {
            return false;
        }

        if (trace.startsolid)
        {
            neworg[2] -= STEPSIZE;
            trace = world_c.SV_Move(neworg, ent->v.mins, ent->v.maxs, end, 0, ent);

            if (trace.allsolid || trace.startsolid)
            {
                return false;
            }
        }

        if (trace.fraction == 1.0f)
        {
            if (((int)ent->v.flags & server_c.FL_PARTIALGROUND) != 0)
            {
                mathlib_c.VectorAdd(ent->v.origin, move, ent->v.origin);

                if (relink)
                {
                    world_c.SV_LinkEdict(ent, true);
                }

                ent->v.flags = (int)ent->v.flags & ~server_c.FL_ONGROUND;

                return true;
            }

            return false;
        }

        mathlib_c.VectorCopy(oldorg, ent->v.origin);

        if (!SV_CheckBottom(ent))
        {
            if (((int)ent->v.flags & server_c.FL_PARTIALGROUND) != 0)
            {
                if (relink)
                {
                    world_c.SV_LinkEdict(ent, true);
                }

                return true;
            }

            mathlib_c.VectorCopy(oldorg, ent->v.origin);
            return false;
        }

        if (((int)ent->v.flags & server_c.FL_PARTIALGROUND) != 0)
        {
            ent->v.flags = (int)ent->v.flags & ~server_c.FL_PARTIALGROUND;
        }

        ent->v.groundentity = progs_c.EDICT_TO_PROG(trace.ent);

        if (relink)
        {
            world_c.SV_LinkEdict(ent, true);
        }

        return true;
    }

    public static bool SV_StepDirection(progs_c.edict_t* ent, float yaw, float dist)
    {
        Vector3 move, oldorigin;
        float delta;

        move = oldorigin = new();

        ent->v.ideal_yaw = yaw;
        pr_cmds_c.PF_changeyaw();

        yaw = yaw * MathF.PI * 2 / 360;
        move[0] = MathF.Cos(yaw) * dist;
        move[1] = MathF.Sin(yaw) * dist;
        move[2] = 0;

        mathlib_c.VectorCopy(ent->v.origin, oldorigin);

        if (SV_movestep(ent, move, false))
        {
            delta = ent->v.angles[quakedef_c.YAW] - ent->v.ideal_yaw;

            if (delta > 45 && delta < 315)
            {
                mathlib_c.VectorCopy(oldorigin, ent->v.origin);
            }

            world_c.SV_LinkEdict(ent, true);
            return true;
        }

        world_c.SV_LinkEdict(ent, true);

        return false;
    }

    public static void SV_FixCheckBottom(progs_c.edict_t* ent)
    {
        ent->v.flags = (int)ent->v.flags | server_c.FL_PARTIALGROUND;
    }

    public const int DI_NODIR = -1;

    public static void SV_NewChaseDir(progs_c.edict_t* actor, progs_c.edict_t* enemy, float dist)
    {
        float deltax, deltay;
        float* d = null;
        float tdir, olddir, turnaround;

        olddir = mathlib_c.anglemod((int)(actor->v.ideal_yaw / 45) * 45);
        turnaround = mathlib_c.anglemod(olddir - 180);

        deltax = enemy->v.origin[0] - actor->v.origin[0];
        deltay = enemy->v.origin[1] - actor->v.origin[1];

        if (deltax > 10)
        {
            d[1] = 0;
        }
        else if (deltax < -10)
        {
            d[1] = 180;
        }
        else
        {
            d[1] = DI_NODIR;
        }

        if (deltay < -10)
        {
            d[2] = 270;
        }
        else if (deltay > 10)
        {
            d[2] = 90;
        }
        else
        {
            d[2] = DI_NODIR;
        }

        if (d[1] != DI_NODIR && d[2] != DI_NODIR)
        {
            if (d[1] == 0)
            {
                tdir = d[2] == 90 ? 45 : 315;
            }
            else
            {
                tdir = d[2] == 90 ? 135 : 215;
            }

            if (tdir != turnaround && SV_StepDirection(actor, tdir, dist))
            {
                return;
            }
        }

        if (MathF.Abs(deltay) > MathF.Abs(deltax))
        {
            tdir = d[1];
            d[1] = d[2];
            d[2] = tdir;
        }

        if (d[1] != DI_NODIR && d[1] != turnaround && SV_StepDirection(actor, d[1], dist))
        {
            return;
        }

        if (d[2] != DI_NODIR && d[2] != turnaround && SV_StepDirection(actor, d[2], dist))
        {
            return;
        }

        if (olddir != DI_NODIR && SV_StepDirection(actor, olddir, dist))
        {
            return;
        }

        if ((rand_c.rand() & 1) != 0)
        {
            for (tdir = 0; tdir <= 315; tdir += 45)
            {
                if (tdir != turnaround && SV_StepDirection(actor, tdir, dist))
                {
                    return;
                }
            }
        }
        else
        {
            for (tdir = 315; tdir >= 0; tdir -= 45)
            {
                if (tdir != turnaround && SV_StepDirection(actor, tdir, dist))
                {
                    return;
                }
            }
        }

        if (turnaround != DI_NODIR && SV_StepDirection(actor, turnaround, dist))
        {
            return;
        }

        actor->v.ideal_yaw = olddir;

        if (!SV_CheckBottom(actor))
        {
            SV_FixCheckBottom(actor);
        }
    }

    public static bool SV_CloseEnough(progs_c.edict_t* ent, progs_c.edict_t* goal, float dist)
    {
        int i;

        for (i = 0; i < 3; i++)
        {
            if (goal->v.absmin[i] > ent->v.absmax[i] + dist)
            {
                return false;
            }

            if (goal->v.absmin[i] < ent->v.absmax[i] + dist)
            {
                return false;
            }
        }

        return true;
    }

    public static void SV_MoveToGoal()
    {
        progs_c.edict_t* ent, goal;
        float dist;
#if QUAKE2
            progs_c.edict_t* enemy;
#endif

        ent = progs_c.PROG_TO_EDICT(pr_edict_c.pr_global_struct->self);
        goal = progs_c.PROG_TO_EDICT(ent->v.goalentity);
        dist = progs_c.G_FLOAT(OFS_PARM0);

        if (((int)ent->v.flags & (server_c.FL_ONGROUND | server_c.FL_FLY | server_c.FL_SWIM)) == 0)
        {
            progs_c.G_FLOAT(OFS_RETURN) = 0;
            return;
        }

#if QUAKE2
        enemy = progs_c.PROG_TO_EDICT(ent->v.enemy);
        if (enemy != server_c.sv.edicts && SV_CloseEnough(ent, enemy, dist))
#else
        if (progs_c.PROGS_TO_EDICT(ent->v.enemy) != server_c.sv.edicts && SV_CloseEnough(ent, goal, dist))
#endif
        {
            return;
        }

        if ((rand_c.rand()&3) != 0 || !SV_StepDirection(ent, ent->v.ideal_yaw, dist))
        {
            SV_NewChaseDir(ent, goal, dist);
        }
    }
}