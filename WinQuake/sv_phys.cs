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
		Vector3* planes = null;
		Vector3 primal_velocity, original_velocity, new_velocity;
		int i, j;
		world_c.trace_t trace;
		Vector3 end;
		float time_left;
		int blocked;

		primal_velocity = original_velocity = new_velocity = end = dir = new();

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
				mathlib_c.VectorCopy(mathlib_c.vec3_origin, ent->v.velocity);
				return 3;
			}

			mathlib_c.VectorCopy(trace.plane.normal, planes[numplanes]);
			numplanes++;

			for (i = 0; i < numplanes; i++)
			{
				ClipVelocity(original_velocity, planes[i], new_velocity, 1);

				for (j = 0; j < numplanes; j++)
				{
					if (j != i)
					{
						if (mathlib_c.DotProduct(new_velocity, planes[j]) < 0)
						{
							break;
						}
					}

					if (j == numplanes)
					{
						break;
					}
				}
			}

			if (i != numplanes)
			{
				mathlib_c.VectorCopy(new_velocity, ent->v.velocity);
			}
			else
			{
				if (numplanes != 2)
				{
					mathlib_c.VectorCopy(mathlib_c.vec3_origin, ent->v.velocity);
					return 7;
				}

				mathlib_c.CrossProduct(planes[0], planes[1], dir);
				d = mathlib_c.DotProduct(dir, ent->v.velocity);
				mathlib_c.VectorScale(dir, d, ent->v.velocity);
			}

			if (mathlib_c.DotProduct(ent->v.velocity, primal_velocity) <= 0)
			{
				mathlib_c.VectorCopy(mathlib_c.vec3_origin, ent->v.velocity);
				return blocked;
			}
		}

		return blocked;
	}

	public static void SV_AddGravity(progs_c.edict_t* ent)
	{
		float ent_gravity;

#if QUAKE2
		if (ent->v.gravity != 0)
		{
			ent_gravity = ent->v.gravity;
		}
		else
		{
			ent_gravity = 1.0f;
		}
#else
		progs_c.eval_t* val;

		val = pr_edict_c.GetEdictFieldValue(ent, "gravity");

		if (val != null && val->_float != 0)
		{
			ent_gravity = val->_float;
		}
		else
		{
			ent_gravity = 1.0f;
		}
#endif
		ent->v.velocity[2] -= ent_gravity * sv_gravity.value * (float)host_c.host_frametime;
	}

	public static world_c.trace_t SV_PushEntity(progs_c.edict_t* ent, Vector3 push)
	{
		world_c.trace_t trace;
		Vector3 end = new();

		mathlib_c.VectorAdd(ent->v.origin, push, end);

		if (ent->v.movetype == server_c.MOVETYPE_FLYMISSILE)
		{
			trace = world_c.SV_Move(ent->v.origin, ent->v.mins, ent->v.maxs, end, world_c.MOVE_MISSILE, ent);
		}
		else if (ent->v.solid == server_c.SOLID_TRIGGER || ent->v.solid == server_c.SOLID_NOT)
		{
			trace = world_c.SV_Move(ent->v.origin, ent->v.mins, ent->v.maxs, end, world_c.MOVE_NOMONSTERS, ent);
		}
		else
		{
			trace = world_c.SV_Move(ent->v.origin, ent->v.mins, ent->v.maxs, end, world_c.MOVE_NORMAL, ent);
		}

		mathlib_c.VectorCopy(trace.endpos, ent->v.origin);
		world_c.SV_LinkEdict(ent, true);

		if (trace.ent != null)
		{
			SV_Impact(ent, trace.ent);
		}

		return trace;
	}

	public static void SV_PushMove(progs_c.edict_t* pusher, float movetime)
	{
		int i, e;
		progs_c.edict_t* check, block;
		Vector3 mins, maxs, move;
		Vector3 entorig, pushorig;
		int num_moved;
		progs_c.edict_t* moved_edict;
		Vector3* moved_from;

		mins = maxs = move = entorig = pushorig = new();
		moved_from = null;
		moved_edict = default;

		if (pusher->v.velocity[0] == 0 && pusher->v.velocity[1] == 0 && pusher->v.velocity[2] == 0)
		{
			pusher->v.ltime += movetime;
			return;
		}

		for (i = 0; i < 3; i++)
		{
			move[i] = pusher->v.velocity[i] * movetime;
			mins[i] = pusher->v.absmin[i] * move[i];
			maxs[i] = pusher->v.absmax[i] * move[i];
		}

		mathlib_c.VectorCopy(pusher->v.origin, pushorig);

		mathlib_c.VectorAdd(pusher->v.origin, move, pusher->v.origin);
		pusher->v.ltime += movetime;
		world_c.SV_LinkEdict(pusher, false);

		num_moved = 0;
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

			if (((int)check->v.flags & server_c.FL_ONGROUND) == 0 && progs_c.PROG_TO_EDICT(check->v.groundentity) == pusher)
			{
				if (check->v.absmin[0] >= maxs[0] || check->v.absmin[1] >= maxs[1] || check->v.absmin[2] >= maxs[2] || check->v.absmax[0] <= mins[0] || check->v.absmax[1] <= mins[1] || check->v.absmax[2] <= mins[2])
				{
					continue;
				}

				if (world_c.SV_TestEntityPosition(check) == null)
				{
					continue;
				}
			}

			if (check->v.movetype != server_c.MOVETYPE_WALK)
			{
				check->v.flags = (int)check->v.flags & ~server_c.FL_ONGROUND;
			}

			mathlib_c.VectorCopy(check->v.origin, entorig);
			mathlib_c.VectorCopy(check->v.origin, moved_from[num_moved]);
			moved_edict[num_moved] = *check;
			num_moved++;

			pusher->v.solid = server_c.SOLID_NOT;
			SV_PushEntity(check, move);
			pusher->v.solid = server_c.SOLID_BSP;

			block = world_c.SV_TestEntityPosition(check);

			if (block != null)
			{
				if (check->v.mins[0] == check->v.maxs[0])
				{
					continue;
				}

				if (check->v.solid == server_c.SOLID_NOT || check->v.solid == server_c.SOLID_TRIGGER)
				{
					check->v.mins[0] = check->v.mins[1] = 0;
					mathlib_c.VectorCopy(check->v.mins, check->v.maxs);
					continue;
				}

				mathlib_c.VectorCopy(entorig, check->v.origin);
				world_c.SV_LinkEdict(check, true);

				mathlib_c.VectorCopy(pushorig, pusher->v.origin);
				world_c.SV_LinkEdict(pusher, false);
				pusher->v.ltime -= movetime;

				if (pusher->v.blocked != null)
				{
					pr_edict_c.pr_global_struct->self = progs_c.EDICT_TO_PROG(pusher);
					pr_edict_c.pr_global_struct->other = progs_c.EDICT_TO_PROG(check);
					pr_exec_c.PR_ExecuteProgram(pusher->v.blocked);
				}

				for (i = 0; i < num_moved; i++)
				{
					mathlib_c.VectorCopy(moved_from[i], moved_edict[i].v.origin);
					world_c.SV_LinkEdict(moved_edict[i], false);
				}
				return;
			}
		}
	}

#if QUAKE2
	public static void SV_PushRotate(progs_c.edict_t* pusher, float movetime)
	{
		int i, e;
		progs_c.edict_t* check, block;
		Vector3 move, a, amove;
		Vector3 entorig, pushorig;
		int num_moved;
		progs_c.edict_t* moved_edict = null;
		Vector3* moved_from = null;
		Vector3 org, org2;
		Vector3 forward, right, up;

		move = a = amove = entorig = pushorig = org = org2 = forward = right = up = new();

		if (pusher->v.avelocity[0] == 0 && pusher->v.avelocity[1] == 0 && pusher->v.avelocity[2] == 0)
		{
			pusher->v.ltime += movetime;
			return;
		}

		for (i = 0; i < 3; i++)
		{
			amove[i] = pusher->v.avelocity[i] * movetime;
		}

		mathlib_c.VectorSubtract(mathlib_c.vec3_origin, amove, a);
		mathlib_c.AngleVectors(a, forward, right, up);

		mathlib_c.VectorCopy(pusher->v.angles, pushorig);

		mathlib_c.VectorAdd(pusher->v.angles, amove, pusher->v.angles);
		pusher->v.ltime += movetime;
		world_c.SV_LinkEdict(pusher, false);

		num_moved = 0;
		check = progs_c.NEXT_EDICT(server_c.sv.edicts);

		for (e = 1; e < server_c.sv.num_edicts; e++, check = progs_c.NEXT_EDICT(check))
		{
			if (check->free)
			{
				continue;
			}

			if (check->v.movetype == server_c.MOVETYPE_PUSH || check->v.movetype == server_c.MOVETYPE_NONE || check->v.movetype == server_c.MOVETYPE_FOLLOW || check->v.movetype == server_c.MOVETYPE_NOCLIP)
			{
				continue;
			}

			if (((int)check->v.flags & server_c.FL_ONGROUND) == 0 && progs_c.PROG_TO_EDICT(check->v.groundentity) == pusher)
			{
				if (check->v.absmin[0] >= pusher->v.absmax[0] || check->v.absmin[1] >= pusher->v.absmax[1] || check->v.absmin[2] >= check->v.absmax[2] || check->v.absmax[0] <= check->v.absmin[0] || check->v.absmax[0] <= check->v.absmin[0] || check->v.absmax[1] <= check->v.absmin[1] || check->v.absmax[2] <= check->v.absmin[2])
				{
					continue;
				}
			}

			if (world_c.SV_TestEntityPosition(check) == null)
			{
				continue;
			}

			if (check->v.movetype != server_c.MOVETYPE_WALK)
			{
				check->v.flags = (int)check->v.flags & ~server_c.FL_ONGROUND;
			}

			mathlib_c.VectorCopy(check->v.origin, entorig);
			mathlib_c.VectorCopy(check->v.origin, moved_from[num_moved]);
			moved_edict[num_moved] = *check;
			num_moved++;

			mathlib_c.VectorSubtract(check->v.origin, pusher->v.origin, org);
			org2[0] = mathlib_c.DotProduct(org, forward);
			org2[1] = -mathlib_c.DotProduct(org, right);
			org2[2] = mathlib_c.DotProduct(org, up);
			mathlib_c.VectorSubtract(org2, org, move);

			pusher->v.solid = server_c.SOLID_NOT;
			SV_PushEntity(check, move);
			pusher->v.solid = server_c.SOLID_BSP;

			block = world_c.SV_TestEntityPosition(check);

			if (block != null)
			{
				if (check->v.mins[0] == check->v.maxs[0])
				{
					continue;
				}

				if (check->v.solid == server_c.SOLID_NOT || check->v.solid == server_c.SOLID_TRIGGER)
				{
					check->v.mins[0] = check->v.mins[1] = 0;
					mathlib_c.VectorCopy(check->v.mins, check->v.maxs);
					continue;
				}

				mathlib_c.VectorCopy(entorig, check->v.origin);
				world_c.SV_LinkEdict(check, true);

				mathlib_c.VectorCopy(pushorig, pusher->v.angles);
				world_c.SV_LinkEdict(pusher, false);
				pusher->v.ltime -= movetime;

				if (pusher->v.blocked != null)
				{
					pr_edict_c.pr_global_struct->self = progs_c.EDICT_TO_PROG(pusher);
					pr_edict_c.pr_global_struct->other = progs_c.EDICT_TO_PROG(check);
					pr_exec_c.PR_ExecuteProgram(pusher->v.blocked);
				}

				for (i = 0; i < num_moved; i++)
				{
					mathlib_c.VectorCopy(moved_from[i], moved_edict[i].v.origin);
					mathlib_c.VectorSubtract(moved_edict[i].v.angles, amove, moved_edict[i].v.angles);
					world_c.SV_LinkEdict(&moved_edict[i], false);
				}
				return;
			}
			else
			{
				mathlib_c.VectorAdd(check->v.angles, amove, check->v.angles);
			}
		}
	}
#endif

	public static void SV_Physics_Pusher(progs_c.edict_t* ent)
	{
		float thinktime;
		float oldltime;
		float movetime;

		oldltime = ent->v.ltime;

		thinktime = ent->v.nextthink;

		if (thinktime < ent->v.ltime + host_c.host_frametime)
		{
			movetime = thinktime - ent->v.ltime;

			if (movetime < 0)
			{
				movetime = 0;
			}
		}
		else
		{
			movetime = (float)host_c.host_frametime;
		}

		if (movetime != 0)
		{
#if QUAKE2
			if (ent->v.avelocity[0] != 0 || ent->v.avelocity[1] != 0 || ent->v.avelocity[2] != 0)
			{
				SV_PushRotate(ent, movetime);
			}
			else
			{
				SV_PushMove(ent, movetime);
			}
#endif
		}

		if (thinktime > oldltime && thinktime <= ent->v.ltime)
		{
			ent->v.nextthink = 0;
			pr_edict_c.pr_global_struct->time = (float)server_c.sv.time;
			pr_edict_c.pr_global_struct->self = progs_c.EDICT_TO_PROG(ent);
			pr_edict_c.pr_global_struct->other = progs_c.EDICT_TO_PROG(server_c.sv.edicts);
			pr_exec_c.PR_ExecuteProgram(ent->v.think);

			if (ent->free)
			{
				return;
			}
		}
	}

	public static void SV_CheckStuck(progs_c.edict_t* ent)
	{
		int i, j;
		int z;
		Vector3 org = new();

		if (world_c.SV_TestEntityPosition(ent) == null)
		{
			mathlib_c.VectorCopy(ent->v.origin, org);
			return;
		}

		mathlib_c.VectorCopy(ent->v.origin, org);
		mathlib_c.VectorCopy(ent->v.origin, ent->v.origin);

		if (world_c.SV_TestEntityPosition(ent) == null)
		{
			console_c.Con_DPrintf("Unstuck.\n");
			world_c.SV_LinkEdict(ent, true);
			return;
		}

		for (z = 0; z < 18; z++)
		{
			for (i = -1; i <= 1; i++)
			{
				for (j = -1; j <= 1; j++)
				{
					ent->v.origin[0] = org[0] + i;
					ent->v.origin[1] = org[1] + j;
					ent->v.origin[2] = org[2] + z;

					if (world_c.SV_TestEntityPosition(ent) == null)
					{
						console_c.Con_DPrintf("Unstuck.\n");
						world_c.SV_LinkEdict(ent, true);
						return;
					}
				}
			}
		}

		mathlib_c.VectorCopy(org, ent->v.origin);
		console_c.Con_DPrintf("player is stuck.\n");
	}

	public static bool SV_CheckWater(progs_c.edict_t* ent)
	{
		Vector3 point = new();
		int cont;
#if QUAKE2
		int truecont;
#endif

		point[0] = ent->v.origin[0];
		point[1] = ent->v.origin[1];
		point[2] = ent->v.origin[2] + ent->v.mins[2] + 1;

		ent->v.waterlevel = 0;
		ent->v.waterlevel = bspfile_c.CONTENTS_EMPTY;
		cont = world_c.SV_PointContents(point);

		if (cont <= bspfile_c.CONTENTS_WATER)
		{
#if QUAKE2
			truecont = world_c.SV_TruePointContents(point);
#endif
			ent->v.waterlevel = cont;
			ent->v.waterlevel = 1;
			point[2] = ent->v.origin[2] + (ent->v.mins[2] + ent->v.maxs[2]) * 0.5f;
			cont = world_c.SV_PointContents(point);

			if (cont <= bspfile_c.CONTENTS_WATER)
			{
				ent->v.waterlevel = 2;
				point[2] = ent->v.origin[2] + ent->v.view_ofs[2];
				cont = world_c.SV_PointContents(point);

				if (cont <= bspfile_c.CONTENTS_WATER)
				{
					ent->v.waterlevel = 3;
				}
			}

#if QUAKE2
			if (truecont <= bspfile_c.CONTENTS_CURRENT_0 && truecont >= bspfile_c.CONTENTS_CURRENT_DOWN)
			{
				Vector3[] current_table =
				{
					new Vector3( 1,  0 , 0),
					new Vector3( 0,  1,  0),
					new Vector3(-1,  0,  0),
					new Vector3( 0, -1,  0),
					new Vector3( 0,  0,  1),
					new Vector3( 0,  0, -1),
				};

				mathlib_c.VectorMA(ent->v.basevelocity, 150.0f * ent->v.waterlevel / 3.0f, current_table[bspfile_c.CONTENTS_CURRENT_0 - truecont], ent->v.basevelocity);
			}
#endif
		}

		return ent->v.waterlevel > 1;
	}

	public static void SV_WallFriction(progs_c.edict_t* ent, world_c.trace_t* trace)
	{
		Vector3 forward, right, up;
		float d, i;
		Vector3 into, side;

		forward = right = up = into = side = new();

		mathlib_c.AngleVectors(ent->v.v_angle, forward, right, up);
		d = mathlib_c.DotProduct(trace->plane.normal, forward);

		d += 0.5f;

		if (d >= 0)
		{
			return;
		}

		i = mathlib_c.DotProduct(trace->plane.normal, ent->v.velocity);
		mathlib_c.VectorScale(trace->plane.normal, i, into);
		mathlib_c.VectorSubtract(ent->v.velocity, into, side);

		ent->v.velocity[0] = side[0] * (1 + d);
		ent->v.velocity[1] = side[1] * (1 + d);
	}

	public static int SV_TryUnstick(progs_c.edict_t* ent, Vector3 oldvel)
	{
		int i;
		Vector3 oldorg;
		Vector3 dir;
		int clip;
		world_c.trace_t steptrace;

		oldorg = dir = new();

		mathlib_c.VectorCopy(ent->v.origin, oldorg);
		mathlib_c.VectorCopy(mathlib_c.vec3_origin, dir);

		for (i = 0; i < 8; i++)
		{
			switch (i)
			{
				case 0:
					dir[0] = 2; dir[1] = 0; break;
				case 1:
					dir[0] = 0; dir[1] = 2; break;
				case 2:
					dir[0] = -2; dir[1] = 0; break;
				case 3:
					dir[0] = 0; dir[1] = -2; break;
				case 4:
					dir[0] = 2; dir[1] = 2; break;
				case 5:
					dir[0] = -2; dir[1] = 2; break;
				case 6:
					dir[0] = 2; dir[1] = -2; break;
				case 7:
					dir[0] = -2; dir[1] = -2; break;
			}

			SV_PushEntity(ent, dir);

			ent->v.velocity[0] = oldvel[0];
			ent->v.velocity[1] = oldvel[1];
			ent->v.velocity[2] = 0;
			clip = SV_FlyMove(ent, 0.1f, &steptrace);

			if (MathF.Abs(oldorg[1] - ent->v.origin[1]) > 4 || MathF.Abs(oldorg[0] - ent->v.origin[0]) > 4)
			{
				return clip;
			}

			mathlib_c.VectorCopy(oldorg, ent->v.origin);
		}

		mathlib_c.VectorCopy(mathlib_c.vec3_origin, ent->v.velocity);
		return 7;
	}

	public const int STEPSIZE = 18;

	public static void SV_WalkMove(progs_c.edict_t* ent)
	{
		Vector3 upmove, downmove;
		Vector3 oldorg, oldvel;
		Vector3 nosteporg, nostepvel;
		int clip;
		int oldonground;
		world_c.trace_t steptrace, downtrace;

		upmove = downmove = oldorg = oldvel = nosteporg = nostepvel = new();

		oldonground = (int)ent->v.flags & server_c.FL_ONGROUND;
		ent->v.flags = (int)ent->v.flags & ~server_c.FL_ONGROUND;

		mathlib_c.VectorCopy(ent->v.origin, oldorg);
		mathlib_c.VectorCopy(ent->v.velocity, oldvel);

		clip = SV_FlyMove(ent, (float)host_c.host_frametime, &steptrace);

		if ((clip & 2) == 0)
		{
			return;
		}

		if (oldonground == 0 && ent->v.waterlevel == 0)
		{
			return;
		}

		if (ent->v.movetype != server_c.MOVETYPE_WALK)
		{
			return;
		}

		if (sv_nostep.value != 0)
		{
			return;
		}

		if (((int)server_c.sv_player->v.flags & server_c.FL_WATERJUMP) != 0)
		{
			return;
		}

		mathlib_c.VectorCopy(ent->v.origin, nosteporg);
		mathlib_c.VectorCopy(ent->v.velocity, nostepvel);

		mathlib_c.VectorCopy(oldorg, ent->v.origin);

		mathlib_c.VectorCopy(mathlib_c.vec3_origin, upmove);
		mathlib_c.VectorCopy(mathlib_c.vec3_origin, downmove);
		upmove[2] = STEPSIZE;
		downmove[2] = -STEPSIZE + oldvel[2] * (float)host_c.host_frametime;

		SV_PushEntity(ent, upmove);

		ent->v.velocity[0] = oldvel[0];
		ent->v.velocity[1] = oldvel[1];
		ent->v.velocity[2] = 0;
		clip = SV_FlyMove(ent, (float)host_c.host_frametime, &steptrace);

		if (clip != 0)
		{
			if (MathF.Abs(oldorg[1] - ent->v.origin[1]) < 0.03125f && MathF.Abs(oldorg[0] - ent->v.origin[0]) < 0.03125f)
			{
				clip = SV_TryUnstick(ent, oldvel);
			}
		}

		if ((clip & 2) != 0)
		{
			SV_WallFriction(ent, &steptrace);
		}

		downtrace = SV_PushEntity(ent, downmove);

		if (downtrace.plane.normal[2] > 0.7f)
		{
			if (ent->v.solid == server_c.SOLID_BSP)
			{
				ent->v.flags = (int)ent->v.flags | server_c.FL_ONGROUND;
				ent->v.groundentity = progs_c.EDICT_TO_PROG(downtrace.ent);
			}
		}
		else
		{
			mathlib_c.VectorCopy(nosteporg, ent->v.origin);
			mathlib_c.VectorCopy(nostepvel, ent->v.velocity);
		}
	}

	public static void SV_Physics_Client(progs_c.edict_t* ent, int num)
	{
		if (!server_c.svs.clients[num - 1].active)
		{
			return;
		}

		pr_edict_c.pr_global_struct->time = (float)server_c.sv.time;
		pr_edict_c.pr_global_struct->self = progs_c.EDICT_TO_PROG;
		pr_exec_c.PR_ExecuteProgram(pr_edict_c.pr_global_struct->PlayerPreThink);

		SV_CheckVelocity(ent);

		switch ((int)ent->v.movetype)
		{
			case server_c.MOVETYPE_NONE:
				if (!SV_RunThink(ent))
				{
					return;
				}
				break;

			case server_c.MOVETYPE_WALK:
				if (!SV_RunThink(ent))
				{
					return;
				}

				if (!SV_CheckWater(ent) && ((int)ent->v.flags & server_c.FL_WATERJUMP) == 0)
				{
					SV_AddGravity(ent);
				}

				SV_CheckStuck(ent);

#if QUAKE2
				mathlib_c.VectorAdd(ent->v.velocity, ent->v.basevelocity, ent->v.velocity);
#endif
				SV_WalkMove(ent);

#if QUAKE2
				mathlib_c.VectorSubtract(ent->v.velocity, ent->v.basevelocity, ent->v.velocity);
#endif
				break;

			case server_c.MOVETYPE_TOSS:
			case server_c.MOVETYPE_BOUNCE:
				SV_Physics_Toss(ent);
				break;

			case server_c.MOVETYPE_FLY:
				if (!SV_RunThink(ent))
				{
					return;
				}

				SV_FlyMove(ent, (float)host_c.host_frametime, null);
				break;

			case server_c.MOVETYPE_NOCLIP:
				if (!SV_RunThink(ent))
				{
					return;
				}

				mathlib_c.VectorMA(ent->v.origin, (float)host_c.host_frametime, ent->v.velocity, ent->v.origin);
				break;

			default:
				sys_win_c.Sys_Error($"SV_Physics_Client: bad movetype {(int)ent->v.movetype}");
				break;
		}

		world_c.SV_LinkEdict(ent, true);

		pr_edict_c.pr_global_struct->time = (float)server_c.sv.time;
		pr_edict_c.pr_global_struct->self = progs_c.EDICT_TO_PROG(ent);
		pr_exec_c.PR_ExecuteProgram(pr_edict_c.pr_global_struct->PlayerPostThink);
	}

	public static void SV_Physics_None(progs_c.edict_t* ent)
	{
		SV_RunThink(ent);
	}

#if QUAKE2
	public static void SV_Physics_Follow(progs_c.edict_t* ent)
	{
		SV_RunThink(ent);
		mathlib_c.VectorAdd(progs_c.PROG_TO_EDICT(ent->v.aiment)->v.origin, ent->v.v_angle, ent->v.origin);
		world_c.SV_LinkEdict(ent, true);
	}
#endif

	public static void SV_Physics_Noclip(progs_c.edict_t* ent)
	{
		if (!SV_RunThink(ent))
		{
			return;
		}

		mathlib_c.VectorMA(ent->v.angles, (float)host_c.host_frametime, ent->v.avelocity, ent->v.angles);
		mathlib_c.VectorMA(ent->v.origin, (float)host_c.host_frametime, ent->v.avelocity, ent->v.origin);

		world_c.SV_LinkEdict(ent, false);
	}

	public static void SV_CheckWaterTransition(progs_c.edict_t* ent)
	{
		int cont;
#if QUAKE2
		Vector3 point = new();

		point[0] = ent->v.origin[0];
		point[1] = ent->v.origin[1];
		point[2] = ent->v.origin[2] + ent->v.mins[2] + 1;
		cont = world_c.SV_PointContents(point);
#else
		cont = world_c.SV_PointContents(ent->v.origin);
#endif

		if (ent->v.watertype == 0)
		{
			ent->v.watertype = cont;
			ent->v.waterlevel = 1;
			return;
		}

		if (cont <= bspfile_c.CONTENTS_WATER)
		{
			if (ent->v.watertype == bspfile_c.CONTENTS_EMPTY)
			{
				sv_send_c.SV_StartSound(ent, 0, "misc/h2ohit1.wav", 255, 1);
			}

			ent->v.watertype = cont;
			ent->v.waterlevel = 1;
		}
		else
		{
			if (ent->v.watertype != bspfile_c.CONTENTS_EMPTY)
			{
				sv_send_c.SV_StartSound(ent, 0, "misc/h2ohit1.wav", 255, 1);
			}

			ent->v.watertype = bspfile_c.CONTENTS_EMPTY;
			ent->v.waterlevel = cont;
		}
	}

	public static void SV_Physics_Toss(progs_c.edict_t* ent)
	{
		world_c.trace_t trace;
		Vector3 move = new();
		float backoff;
#if QUAKE2
		progs_c.edict_t* groundentity;

		groundentity = progs_c.PROG_TO_EDICT(ent->v.groundentity);

		if (((int)groundentity->v.flags & server_c.FL_CONVEYOR) != 0)
		{
			mathlib_c.VectorScale(groundentity->v.movedir, groundentity->v.speed, ent->v.basevelocity);
		}
		else
		{
			mathlib_c.VectorCopy(vec_origin, ent->v.basevelocity);
		}

		SV_CheckWater(ent);
#endif

		if (!SV_RunThink(ent))
		{
			return;
		}

#if QUAKE2
		if (ent->v.velocity[2] > 0)
		{
			ent->v.flags = (int)ent->v.flags & ~server_c.FL_ONGROUND;
		}

		if (((int)ent->v.flags & server_c.FL_ONGROUND) != 0)
		{
			if (mathlib_c.VectorCompare(ent->v.basevelocity, vec_origin))
			{
				return;
			}
		}

		SV_CheckVelocity(ent);

		if (((int)ent->v.flags & server_c.FL_ONGROUND) == 0 && ent->v.movetype != server_c.MOVETYPE_FLY && ent->v.movetype != server_c.MOVETYPE_BOUNCEMISSILE && ent->v.movetype != server_c.MOVETYPE_FLYMISSILE)
		{
			SV_AddGravity(ent);
		}
#else
		if (((int)ent->v.flags & server_c.FL_ONGROUND) != 0)
		{
			return;
		}

		SV_CheckVelocity(ent);

		if (ent->v.movetype != server_c.MOVETYPE_FLY && ent->v.movetype != server_c.MOVETYPE_FLYMISSILE)
		{
			SV_AddGravity(ent);
		}
#endif

		mathlib_c.VectorMA(ent->v.angles, (float)host_c.host_frametime, ent->v.velocity, ent->v.angles);

#if QUAKE2
		mathlib_c.VectorAdd(ent->v.velocity, ent->v.basevelocity, ent->v.velocity);
#endif
		mathlib_c.VectorScale(ent->v.velocity, (float)host_c.host_frametime, move);
		trace = SV_PushEntity(ent, move);
#if QUAKE2
		mathlib_c.VectorSubtract(ent->v.velocity, ent->v.basevelocity, ent->v.velocity);
#endif

		if (trace.fraction == 1.0f)
		{
			return;
		}

		if (ent->free)
		{
			return;
		}

		if (ent->v.movetype== server_c.MOVETYPE_BOUNCE)
		{
			backoff = 1.5f;
		}
#if QUAKE2
		else if (ent->v.movetype == server_c.MOVETYPE_BOUNCEMISSILE)
		{
			backoff = 2.0f;
		}
#endif
		else
		{
			backoff = 1.0f;
		}

		ClipVelocity(ent->v.velocity, trace.plane.normal, ent->v.velocity, backoff);

		if (trace.plane.normal[2] > 0.7f)
		{
#if QUAKE2
			if (ent->v.velocity[2] < 60 || (ent->v.movetype != server_c.MOVETYPE_BOUNCE && ent->v.movetype != server_c.MOVETYPE_BOUNCEMISSILE))
#else
			if (ent->v.velocity[2] < 60 || ent->v.movetype != server_c.MOVETYPE_BOUNCE)
#endif
			{
				ent->v.flags = (int)ent->v.flags | server_c.FL_ONGROUND;
				ent->v.groundentity = progs_c.EDICT_TO_PROG(trace.ent);
				mathlib_c.VectorCopy(mathlib_c.vec3_origin, ent->v.velocity);
				mathlib_c.VectorCopy(mathlib_c.vec3_origin, ent->v.avelocity);
			}
		}

		SV_CheckWaterTransition(ent);
	}

#if !QUAKE2
	public static void SV_Physics_Step(progs_c.edict_t* ent)
	{
		bool wasonground;
		bool inwater;
		bool hitsound = false;
		float* vel;
		float speed, newspeed, control;
		float friction;
		progs_c.edict_t* groundentity;

		groundentity = progs_c.PROG_TO_EDICT(ent->v.groundentity);

		if (((int)groundentity->v.flags & server_c.FL_CONVEYOR) != 0)
		{
			mathlib_c.VectorScale(groundentity->v.movedir, groundentity->v.speed, ent->v.basevelocity);
		}
		else
		{
			mathlib_c.VectorCopy(vec_origin, ent->v.basevelocity);
		}

		pr_edict_c.pr_global_struct->time = (float)server_c.sv.time;
		pr_edict_c.pr_global_struct->self = progs_c.EDICT_TO_PROG(ent);
		PF_WaterMove();

		SV_CheckVelocity(ent);

		wasonground = (int)ent->v.flags & server_c.FL_ONGROUND;

		inwater = SV_CheckWater(ent);

		if (!wasonground)
		{
			if (((int)ent->v.flags & server_c.FL_FLY) == 0)
			{

			}
		}
	}
#else
#endif
}