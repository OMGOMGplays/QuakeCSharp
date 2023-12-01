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

    public beam_t* cl_beams;

    public static int MAX_EXPLOSIONS = 8;

    public struct explosion_t
    {
        public Vector3 origin;
        public float start;
        public model_c.model_t* model;
    }

    public explosion_t* cl_explosions;

    public sfx_t* cl_sfx_wizhit;
    public sfx_t* cl_sfx_knighthit;
    public sfx_t* cl_sfx_tink1;
    public sfx_t* cl_sfx_ric1;
    public sfx_t* cl_sfx_ric2;
    public sfx_t* cl_sfx_ric3;
    public sfx_t* cl_sfx_r_exp3;

    public void CL_InitTEnts()
    {
        cl_sfx_wizhit = S_PrecacheSound("wizard/hit.wav");
        cl_sfx_knighthit = S_PrecacheSound("hknight/hit.wav");
        cl_sfx_tink1 = S_PrecacheSound("weapons/tink1.wav");
        cl_sfx_ric1 = S_PrecacheSound("weapons/ric1.wav");
        cl_sfx_ric2 = S_PrecacheSound("weapons/ric2.wav");
        cl_sfx_ric3 = S_PrecacheSound("weapons/ric3.wav");
        cl_sfx_r_exp3 = S_Precache("weapons/r_exp3.wav");
    }

    public void CL_ClearTEnts()
    {
        common_c.Q_memset(cl_beams, 0, sizeof(beam_t));
        common_c.Q_memset(cl_explosions, 0, sizeof(explosion_t));
    }

    public explosion_t* CL_AllocExplosion()
    {
        int i;
        float time;
        int index;

        for (i = 0; i < MAX_EXPLOSIONS; i++)
        {
            if (cl_explosions[i].model == null)
            {
                return cl_explosions[i];
            }
        }

        time = cl_main_c.cl.time;
        index = 0;

        for (i = 0; i < MAX_EXPLOSIONS; i++)
        {
            if (cl_explosions[i].start < time)
            {
                time = cl_explosions[i].start;
                index = i;
            }
        }

        return &cl_explosions[index];
    }

    public void CL_ParseBeam(model_c.model_t* m)
    {
        int ent;
        Vector3 start = new Vector3(0, 0, 0), end = new Vector3(0, 0, 0);
        beam_t* b;
        int i;

        ent = common_c.MSG_ReadShort();

        start[0] = common_c.MSG_ReadCoord();
        start[1] = common_c.MSG_ReadCoord();
        start[2] = common_c.MSG_ReadCoord();

        end[0] = common_c.MSG_ReadCoord();
        end[1] = common_c.MSG_ReadCoord();
        end[2] = common_c.MSG_ReadCoord();

        for (i = 0, b = cl_beams; i < MAX_BEAMS; i++, b++)
        {
            if (b->model == null || b->endtime < cl.time)
            {
                b->entity = ent;
                b->model = m;
                b->endtime = cl_main_c.cl.time + 0.2f;
                //VectorCopy(start, b->start);
                // VectorCopy(end, b->end);
                return;
            }
        }

        console_c.Con_Printf("beam list overflow!\n");
    }

    public void CL_ParseTEnt()
    {
        int type;
        Vector3 pos = new Vector3(0, 0, 0);
        client_c.dlight_t* dl;
        int rnd;
        explosion_t* ex;
        int cnt;

        type = common_c.MSG_ReadByte();

        switch (type)
        {
            case TE_WIZSPIKE:
                pos[0] = common_c.MSG_ReadCoord();
                pos[1] = common_c.MSG_ReadCoord();
                pos[2] = common_c.MSG_ReadCoord();
                R_RunParticleEffect(pos, mathlib_c.vec3_origin, 20, 30);
                S_StartSound(-1, 0, cl_sfx_wizhit, pos, 1, 1);
                break;

            case TE_KNIGHTSPIKE:
                pos[0] = common_c.MSG_ReadCoord();
                pos[1] = common_c.MSG_ReadCoord();
                pos[2] = common_c.MSG_ReadCoord();
                R_RunParticleEffects(pos, mathlib_c.vec3_origin, 226, 30);
                S_StartSound(-1, 0, cl_sfx_knighthit, pos, 1, 1);
                break;

            case TE_SPIKE:
                pos[0] = common_c.MSG_ReadCoord();
                pos[1] = common_c.MSG_ReadCoord();
                pos[2] = common_c.MSG_ReadCoord();
                R_RunParticleEffect(pos, mathlib_c.vec3_origin, 0, 10);

                if ((rand() % 5) != 0)
                {
                    S_StartSound(-1, 0, cl_sfx_tink1, pos, 1, 1);
                }
                else
                {
                    rnd = rand() & 3;

                    if (rnd == 1)
                    {
                        S_StartSound(-1, 0, cl_sfx_ric1, pos, 1, 1);
                    }
                    else if (rnd == 2)
                    {
                        S_StartSound(-1, 0, cl_sfx_ric2, pos, 1, 1);
                    }
                    else
                    {
                        S_StartSound(-1, 0, cl_sfx_ric3, pos, 1, 1);
                    }
                }
                break;

            case TE_SUPERSPIKE:
                pos[0] = common_c.MSG_ReadCoord();
                pos[1] = common_c.MSG_ReadCoord();
                pos[2] = common_c.MSG_ReadCoord();
                R_RunParticleEffect(pos, mathlib_c.vec3_origin, 0, 20);

                if ((rand() % 5) != 0)
                {
                    S_StartSound(-1, 0, cl_sfx_tink1, pos, 1, 1);
                }
                else
                {
                    rnd = rand() & 3;

                    if (rnd == 1)
                    {
                        S_StartSound(-1, 0, cl_sfx_ric1, pos, 1, 1);
                    }
                    else if (rnd == 2)
                    {
                        S_StartSound(-1, 0, cl_sfx_ric2, pos, 1, 1);
                    }
                    else
                    {
                        S_StartSound(-1, 0, cl_sfx_ric3, pos, 1, 1);
                    }
                }
                break;

            case TE_EXPLOSION:
                pos[0] = common_c.MSG_ReadCoord();
                pos[1] = common_c.MSG_ReadCoord();
                pos[2] = common_c.MSG_ReadCoord();
                R_ParticleExplosion(pos);

                dl = cl_main_c.CL_AllocDlight(0);
                //VectorCopy(pos, dl->origin);
                dl->radius = 350;
                dl->die = cl_main_c.cl.time + 0.5f;
                dl->decay = 300;
                dl->color[0] = 0.2f;
                dl->color[1] = 0.1f;
                dl->color[2] = 0.05f;
                dl->color[3] = 0.7f;

                S_StartSound(-1, 0, cl_sfx_r_exp3, pos, 1, 1);

                ex = CL_AllocExplosion();
                //VectorCopy(pos, ex->origin);
                ex->start = cl_main_c.cl.time;
                ex->model = Mod_ForName("progs/s_explod.spr", true);
                break;

            case TE_TAREXPLOSION:
                pos[0] = common_c.MSG_ReadCoord();
                pos[1] = common_c.MSG_ReadCoord();
                pos[2] = common_c.MSG_ReadCoord();
                R_BlobExplosion(pos);

                S_StartSound(-1, 0, cl_sfx_r_exp3, pos, 1, 1);
                break;

            case TE_LIGHTNING1:
                CL_ParseBeam(Mod_ForName("progs/bolt.mdl", true));
                break;

            case TE_LIGHTNING2:
                CL_ParseBeam(Mod_ForName("progs/bolt2.mdl", true));
                break;

            case TE_LIGHTNING3:
                CL_ParseBeam(Mod_ForName("progs/bolt3.mdl", true));
                break;

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

            case TE_GUNSHOT:
                cnt = common_c.MSG_ReadByte();
                pos[0] = common_c.MSG_ReadCoord();
                pos[1] = common_c.MSG_ReadCoord();
                pos[2] = common_c.MSG_ReadCoord();
                R_RunParticleEffect(pos, mathlib_c.vec3_origin, 0, 20 * cnt);
                break;

            case TE_BLOOD:
                cnt = common_c.MSG_ReadByte();
                pos[0] = common_c.MSG_ReadCoord();
                pos[1] = common_c.MSG_ReadCoord();
                pos[2] = common_c.MSG_ReadCoord();
                R_RunParticleEffect(pos, mathlib_c.vec3_origin, 73, 20 * cnt);
                break;

            case TE_LIGHTNINGBLOOD:
                pos[0] = common_c.MSG_ReadCoord();
                pos[1] = common_c.MSG_ReadCoord();
                pos[2] = common_c.MSG_ReadCoord();
                R_RunParticleEffect(pos, mathlib_c.vec3_origin, 225, 50);
                break;

            default:
                sys_win_c.Sys_Error("CL_ParseTEnt: bad type");
        }
    }

    public render_c.entity_t* CL_NewTempEntity()
    {
        render_c.entity_t* ent;

        if (cl_main_c.cl_numvisedicts == client_c.MAX_VISEDICTS)
        {
            return null;
        }

        ent = cl_main_c.cl_visedicts[cl_main_c.cl_numvisedicts];
        cl_main_c.cl_numvisedicts++;
        ent->keynum = 0;

        common_c.Q_memset(ent, 0, sizeof(render_c.entity_t));

        ent->colormap = vid_win_c.vid.colormap;
        return ent;
    }
}