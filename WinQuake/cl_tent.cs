namespace Quake;

public unsafe class cl_tent_c
{
	public int num_temp_entities;
	public render_c.entity_t* cl_temp_entities;
	public client_c.beam_t* cl_beams;

	sound_c.sfx_t* cl_sfx_wizhit;
	sound_c.sfx_t* cl_sfx_knighthit;
	sound_c.sfx_t* cl_sfx_tink1;
	sound_c.sfx_t* cl_sfx_ric1;
	sound_c.sfx_t* cl_sfx_ric2;
	sound_c.sfx_t* cl_sfx_ric3;
	sound_c.sfx_t* cl_sfx_r_exp3;
#if QUAKE2
    sound_c.sfx_t* cl_sfx_imp;
	sound_c.sfx_t* cl_sfx_rail;
#endif

	public void CL_InitTEnts()
	{
		cl_sfx_wizhit = S_PrecacheSound("wizard/hit.wav");
		cl_sfx_knighthit = S_PrecacheSound("knight/hit.wav");
		cl_sfx_tink1 = S_PrecacheSound("weapons/tink1.wav");
		cl_sfx_ric1 = S_PrecacheSound("weapons/ric1.wav");
		cl_sfx_ric2 = S_PrecacheSound("weapons/ric2.wav");
		cl_sfx_ric3 = S_PrecacheSound("weapons/ric3.wav");
		cl_sfx_r_exp3 = S_PrecacheSound("weapons/r_exp3.wav");
#if QUAKE2
        cl_sfx_imp = S_PrecacheSound("shambler/sattck1.wav");
		cl_sfx_rail = S_PrecacheSound("weapons/1start.wav");
#endif
	}

	public void CL_ParseBeam(model_c.model_t* m)
	{
		int ent;
		Vector3 start = new Vector3(0, 0, 0), end = new Vector3(0, 0, 0);
		client_c.beam_t* b;
		int i;

		ent = common_c.MSG_ReadShort();

		start.X = common_c.MSG_ReadCoord();
		start.Y = common_c.MSG_ReadCoord();
		start.Z = common_c.MSG_ReadCoord();

		end.X = common_c.MSG_ReadCoord();
		end.Y = common_c.MSG_ReadCoord();
		end.Z = common_c.MSG_ReadCoord();

		for (i = 0, b = cl_beams; i < client_c.MAX_BEAMS; i++, b++)
		{
			if (b->entity == ent)
			{
				b->entity = ent;
				b->model = *m;
				b->endtime = cl_main_c.cl.time + 0.2f;
				mathlib_c.VectorCopy(start, b->start);
				mathlib_c.VectorCopy(end, b->end);
				return;
			}
		}

		for (i = 0, b = cl_beams; i < client_c.MAX_BEAMS; i++, b++)
		{
			if (b->model == null || b->endtime < cl_main_c.cl.time)
			{
				b->entity = ent;
				b->model = m;
				b->endtime = cl_main_c.cl.time + 0.2f;
				mathlib_c.VectorCopy(start, b->start);
				mathlib_c.VectorCopy(end, b->end);
				return;
			}
		}

		console_c.Con_Printf("beam list overflow!\n");
	}

	public void CL_ParseTEnt()
	{
		int type;
		Vector3 pos = new Vector3(0);
#if QUAKE2
		Vector3 endpos;
#endif
		client_c.dlight_t* dl;
		int rnd;
		int colorStart, colorLength;

		type = common_c.MSG_ReadByte();

		switch (type)
		{
			case TE_WIZSPIKE:           // spike hitting wall
				pos[0] = common_c.MSG_ReadCoord();
				pos[1] = common_c.MSG_ReadCoord();
				pos[2] = common_c.MSG_ReadCoord();
				R_RunParticleEffect(pos, mathlib_c.vec3_origin, 20, 30);
				S_StartSound(-1, 0, cl_sfx_wizhit, pos, 1, 1);
				break;

			case TE_KNIGHTSPIKE:            // spike hitting wall
				pos[0] = common_c.MSG_ReadCoord();
				pos[1] = common_c.MSG_ReadCoord();
				pos[2] = common_c.MSG_ReadCoord();
				R_RunParticleEffect(pos, mathlib_c.vec3_origin, 226, 20);
				S_StartSound(-1, 0, cl_sfx_knighthit, pos, 1, 1);
				break;

			case TE_SPIKE:          // spike hitting wall
				pos[0] = common_c.MSG_ReadCoord();
				pos[1] = common_c.MSG_ReadCoord();
				pos[2] = common_c.MSG_ReadCoord();
#if GLTEST
				Test_Spawn(pos);
#else
				R_RunParticleEffect(pos, mathlib_c.vec3_origin, 0, 10);
#endif
				if ((rand() % 5) != 0)
					S_StartSound(-1, 0, cl_sfx_tink1, pos, 1, 1);
				else
				{
					rnd = rand() & 3;
					if (rnd == 1)
						S_StartSound(-1, 0, cl_sfx_ric1, pos, 1, 1);
					else if (rnd == 2)
						S_StartSound(-1, 0, cl_sfx_ric2, pos, 1, 1);
					else
						S_StartSound(-1, 0, cl_sfx_ric3, pos, 1, 1);
				}
				break;
			case TE_SUPERSPIKE:         // super spike hitting wall
				pos[0] = common_c.MSG_ReadCoord();
				pos[1] = common_c.MSG_ReadCoord();
				pos[2] = common_c.MSG_ReadCoord();
				R_RunParticleEffect(pos, mathlib_c.vec3_origin, 0, 20);

				if ((rand() % 5) != 0)
					S_StartSound(-1, 0, cl_sfx_tink1, pos, 1, 1);
				else
				{
					rnd = rand() & 3;
					if (rnd == 1)
						S_StartSound(-1, 0, cl_sfx_ric1, pos, 1, 1);
					else if (rnd == 2)
						S_StartSound(-1, 0, cl_sfx_ric2, pos, 1, 1);
					else
						S_StartSound(-1, 0, cl_sfx_ric3, pos, 1, 1);
				}
				break;

			case TE_GUNSHOT:            // bullet hitting wall
				pos[0] = common_c.MSG_ReadCoord();
				pos[1] = common_c.MSG_ReadCoord();
				pos[2] = common_c.MSG_ReadCoord();
				R_RunParticleEffect(pos, mathlib_c.vec3_origin, 0, 20);
				break;

			case TE_EXPLOSION:          // rocket explosion
				pos[0] = common_c.MSG_ReadCoord();
				pos[1] = common_c.MSG_ReadCoord();
				pos[2] = common_c.MSG_ReadCoord();
				R_ParticleExplosion(pos);
				dl = CL_AllocDlight(0);
				VectorCopy(pos, dl->origin);
				dl->radius = 350;
				dl->die = cl_main_c.cl.time + 0.5;
				dl->decay = 300;
				S_StartSound(-1, 0, cl_sfx_r_exp3, pos, 1, 1);
				break;

			case TE_TAREXPLOSION:           // tarbaby explosion
				pos[0] = common_c.MSG_ReadCoord();
				pos[1] = common_c.MSG_ReadCoord();
				pos[2] = common_c.MSG_ReadCoord();
				R_BlobExplosion(pos);

				S_StartSound(-1, 0, cl_sfx_r_exp3, pos, 1, 1);
				break;

			case TE_LIGHTNING1:             // lightning bolts
				CL_ParseBeam(Mod_ForName("progs/bolt.mdl", true));
				break;

			case TE_LIGHTNING2:             // lightning bolts
				CL_ParseBeam(Mod_ForName("progs/bolt2.mdl", true));
				break;

			case TE_LIGHTNING3:             // lightning bolts
				CL_ParseBeam(Mod_ForName("progs/bolt3.mdl", true));
				break;

			// PGM 01/21/97 
			case TE_BEAM:               // grappling hook beam
				CL_ParseBeam(Mod_ForName("progs/beam.mdl", true));
				break;
			// PGM 01/21/97

			case TE_LAVASPLASH:
				pos[0] = common_c.MSG_ReadCoord();
				pos[1] = common_c.MSG_ReadCoord();
				pos[2] = common_c.MSG_ReadCoord();
				R_LavaSplash(pos);
				break;

			case TE_TELEPORT:
				pos[0] = common_c.MSG_ReadCoord();
				pos[1] = common_c.MSG_ReadCoord();
				pos[2] = common_c.MSG_ReadCoord();
				R_TeleportSplash(pos);
				break;

			case TE_EXPLOSION2:             // color mapped explosion
				pos[0] = common_c.MSG_ReadCoord();
				pos[1] = common_c.MSG_ReadCoord();
				pos[2] = common_c.MSG_ReadCoord();
				colorStart = common_c.MSG_ReadByte();
				colorLength = common_c.MSG_ReadByte();
				R_ParticleExplosion2(pos, colorStart, colorLength);
				dl = CL_AllocDlight(0);
				mathlib_c.VectorCopy(pos, dl->origin);
				dl->radius = 350;
				dl->die = cl_main_c.cl.time + 0.5;
				dl->decay = 300;
				S_StartSound(-1, 0, cl_sfx_r_exp3, pos, 1, 1);
				break;

#if QUAKE2
			case TE_IMPLOSION:
				pos[0] = common_c.MSG_ReadCoord();
				pos[1] = common_c.MSG_ReadCoord();
				pos[2] = common_c.MSG_ReadCoord();
				S_StartSound(-1, 0, cl_sfx_imp, pos, 1, 1);
				break;

			case TE_RAILTRAIL:
				pos[0] = common_c.MSG_ReadCoord();
				pos[1] = common_c.MSG_ReadCoord();
				pos[2] = common_c.MSG_ReadCoord();
				endpos[0] = common_c.MSG_ReadCoord();
				endpos[1] = common_c.MSG_ReadCoord();
				endpos[2] = common_c.MSG_ReadCoord();
				S_StartSound(-1, 0, cl_sfx_rail, pos, 1, 1);
				S_StartSound(-1, 1, cl_sfx_r_exp3, endpos, 1, 1);
				R_RocketTrail(pos, endpos, 0 + 128);
				R_ParticleExplosion(endpos);
				dl = CL_AllocDlight(-1);
				mathlib_c.VectorCopy(endpos, dl->origin);
				dl->radius = 350;
				dl->die = cl.time + 0.5;
				dl->decay = 300;
				break;
#endif

			default:
				sys_win_c.Sys_Error("CL_ParseTEnt: bad type");
		}
	}
}
}