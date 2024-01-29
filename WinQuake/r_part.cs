namespace Quake;

public unsafe class r_part_c
{
    public const int MAX_PARTICLES = 2048;
    public const int ABSOLUTE_MIN_PARTICLES = 512;

    public static int[] ramp1 = { 0x6f, 0x6d, 0x6b, 0x69, 0x67, 0x65, 0x63, 0x61 };
    public static int[] ramp2 = { 0x6f, 0x6e, 0x6d, 0x6c, 0x6b, 0x6a, 0x68, 0x66 };
    public static int[] ramp3 = { 0x6d, 0x6b, 6, 5, 4, 3 };

    public static glquake_c.particle_t* active_particles, free_particles;

    public static glquake_c.particle_t* particles;
    public static int r_numparticles;

    public static Vector3 r_pright, r_pup, r_ppn;

    public static void R_InitParticles()
    {
        int i;

        i = common_c.COM_CheckParm("-particles");

        if (i != 0)
        {
            r_numparticles = common_c.Q_atoi(common_c.com_argv[i + 1].ToString());

            if (r_numparticles < ABSOLUTE_MIN_PARTICLES)
            {
                r_numparticles = ABSOLUTE_MIN_PARTICLES;
            }
        }
        else
        {
            r_numparticles = MAX_PARTICLES;
        }

        particles = (glquake_c.particle_t*)zone_c.Hunk_AllocName(r_numparticles * sizeof(glquake_c.particle_t), "particles");
    }

#if QUAKE2
    public static void R_DarkFieldParticles(render_c.entity_t* ent)
    {
        int i, j, k;
        glquake_c.particle_t* p;
        float vel;
        Vector3 dir;
        Vector3 org;

        dir = org = new();

        org[0] = ent->origin[0];
        org[1] = ent->origin[1];
        org[2] = ent->origin[2];

        for (i = -16; i < 16; i += 8)
        {
            for (j = -16; j < 16; j += 8)
            {
                for (k = 0; k < 32; k += 8)
                {
                    if (free_particles == null)
                    {
                        return;
                    }

                    p = free_particles;
                    free_particles = p->next;
                    p->next = active_particles;
                    active_particles = p;

                    p->die = cl_main_c.cl.time + 0.2f + (rand_c.rand() & 7) * 0.02f;
                    p->color = 150 + rand_c.rand() % 6;
                    p->type = glquake_c.ptype_t.pt_slowgrav;

                    dir[0] = j * 8;
                    dir[1] = i * 8;
                    dir[2] = k * 8;

                    p->org[0] = org[0] + i + (rand_c.rand() & 3);
                    p->org[1] = org[1] + j + (rand_c.rand() & 3);
                    p->org[2] = org[2] + k + (rand_c.rand() & 3);

                    mathlib_c.VectorNormalize(dir);
                    vel = 50 + (rand_c.rand() & 63);
                    mathlib_c.VectorScale(dir, vel, p->vel);
                }
            }
        }
    }
#endif

    public const int NUMVERTEXNORMALS = 256;
    public static float[][] r_avertexnormals = new float[NUMVERTEXNORMALS][];
    public static Vector3* avelocities;
    public static float beamlength = 16;
    public static Vector3 avelocity = new Vector3(23, 7, 3);
    public static float partstep = 0.01f;
    public static float timescale = 0.01f;

    public static void R_EntityParticles(render_c.entity_t* ent)
    {
        int count;
        int i;
        glquake_c.particle_t* p;
        float angle;
        float sr, sp, sy, cr, cp, cy;
        Vector3 forward = new();
        float dist;

        dist = 64;
        count = 50;

        if (avelocities[0][0] == 0)
        {
            for (i = 0; i < NUMVERTEXNORMALS * 3; i++)
            {
                angle = (float)cl_main_c.cl.time * avelocities[i][0];
                sy = MathF.Sin(angle);
                cy = MathF.Cos(angle);
                angle = (float)cl_main_c.cl.time * avelocities[i][1];
                sp = MathF.Sin(angle);
                cp = MathF.Cos(angle);
                angle = (float)cl_main_c.cl.time * avelocities[i][2];
                sr = MathF.Sin(angle);
                cr = MathF.Cos(angle);

                forward[0] = cp * cy;
                forward[1] = cp * sy;
                forward[2] = -sp;

                if (free_particles == null)
                {
                    return;
                }

                p = free_particles;
                free_particles = p->next;
                p->next = active_particles;
                active_particles = p;

                p->die = (float)cl_main_c.cl.time + 0.01f;
                p->color = 0x6f;
                p->type = glquake_c.ptype_t.pt_explode;

                p->org[0] = ent->origin[0] + r_avertexnormals[i][0] * dist + forward[0] * beamlength;
                p->org[1] = ent->origin[1] + r_avertexnormals[i][1] * dist + forward[1] * beamlength;
                p->org[2] = ent->origin[2] + r_avertexnormals[i][2] * dist + forward[2] * beamlength;
            }
        }
    }

    public static void R_ClearParticles()
    {
        int i;

        free_particles = &particles[0];
        active_particles = null;

        for (i = 0; i < r_numparticles; i++)
        {
            particles[i].next = &particles[i + 1];
        }

        particles[r_numparticles - 1].next = null;
    }

    // R_ReadPointFile_f

    public static void R_ParseParticleEffect()
    {
        Vector3 org, dir;
        int i, count, msgcount, color;

        org = dir = new();

        for (i = 0; i < 3; i++)
        {
            org[i] = common_c.MSG_ReadCoord();
        }

        for (i = 0; i < 3; i++)
        {
            dir[i] = common_c.MSG_ReadChar() * (1.0f / 16);
        }

        msgcount = common_c.MSG_ReadByte();
        color = common_c.MSG_ReadByte();

        if (msgcount == 255)
        {
            count = 1024;
        }
        else
        {
            count = msgcount;
        }

        R_RunParticleEffect(org, dir, color, count);
    }

    public static void R_ParticleExplosion(Vector3 org)
    {
        int i, j;
        glquake_c.particle_t* p;

        for (i = 0; i < 1024; i++)
        {
            if (free_particles == null)
            {
                return;
            }

            p = free_particles;
            free_particles = p->next;
            p->next = active_particles;
            active_particles = p;

            p->die = (float)cl_main_c.cl.time + 5;
            p->color = ramp1[0];
            p->ramp = rand_c.rand() & 3;

            if ((i & 1) != 0)
            {
                p->type = glquake_c.ptype_t.pt_explode;

                for (j = 0; j < 3; j++)
                {
                    p->org[j] = org[j] + ((rand_c.rand() % 32) - 16);
                    p->vel[j] = (rand_c.rand() % 512) - 256;
                }
            }
            else
            {
                p->type = glquake_c.ptype_t.pt_explode2;

                for (j = 0; j < 3; j++)
                {
                    p->org[j] = org[j] + ((rand_c.rand() % 32) - 16);
                    p->vel[j] = (rand_c.rand() % 512) - 256;
                }
            }
        }
    }

    public static void R_ParticleExplosion2(Vector3 org, int colorStart, int colorLength)
    {
        int i, j;
        glquake_c.particle_t* p;
        int colorMod = 0;

        for (i = 0; i < 512; i++)
        {
            if (free_particles == null)
            {
                return;
            }

            p = free_particles;
            free_particles = p->next;
            p->next = active_particles;
            active_particles = p;

            p->die = (float)cl_main_c.cl.time + 0.3f;
            p->color = colorStart + (colorMod % colorLength);
            colorMod++;

            p->type = glquake_c.ptype_t.pt_blob;

            for (j = 0; j < 3; j++)
            {
                p->org[j] = org[j] + ((rand_c.rand() % 32) - 16);
                p->vel[j] = (rand_c.rand() % 512) - 256;
            }
        }
    }

    public static void R_BlobExplosion(Vector3 org)
    {
        int i, j;
        glquake_c.particle_t* p;

        for (i = 0; i < 1024; i++)
        {
            if (free_particles == null)
            {
                return;
            }

            p = free_particles;
            free_particles = p->next;
            p->next = active_particles;
            active_particles = p;

            p->die = (float)cl_main_c.cl.time + 1 + (rand_c.rand() & 8) * 0.05f;

            if ((i & 1) != 0)
            {
                p->type = glquake_c.ptype_t.pt_blob;
                p->color = 66 + rand_c.rand() % 6;

                for (j = 0; j < 3; j++)
                {
                    p->org[j] = org[j] + ((rand_c.rand() % 32) - 16);
                    p->vel[j] = (rand_c.rand() % 512) - 256;
                }
            }
            else
            {
                p->type = glquake_c.ptype_t.pt_blob2;
                p->color = 150 + rand_c.rand() % 6;

                for (j = 0; j < 3; j++)
                {
                    p->org[j] = org[j] + ((rand_c.rand() % 32) - 16);
                    p->vel[j] = (rand_c.rand() % 512) - 256;
                }
            }
        }
    }

    public static void R_RunParticleEffect(Vector3 org, Vector3 dir, int color, int count)
    {
        int i, j;
        glquake_c.particle_t* p;

        for (i = 0; i < count; i++)
        {
            if (free_particles == null)
            {
                return;
            }

            p = free_particles;
            free_particles = p->next;
            p->next = active_particles;
            active_particles = p;

            if (count == 1024)
            {
                p->die = (float)cl_main_c.cl.time + 5;
                p->color = ramp1[0];
                p->ramp = rand_c.rand() & 3;

                if ((i & 1) != 0)
                {
                    p->type = glquake_c.ptype_t.pt_explode;

                    for (j = 0; j < 3; j++)
                    {
                        p->org[j] = org[j] + ((rand_c.rand() % 32) - 16);
                        p->vel[j] = (rand_c.rand() % 512) - 256;
                    }
                }
                else
                {
                    p->type = glquake_c.ptype_t.pt_explode2;

                    for (j = 0; j < 3; j++)
                    {
                        p->org[j] = org[j] + ((rand_c.rand() % 32) - 16);
                        p->vel[j] = (rand_c.rand() % 512) - 256;
                    }
                }
            }
            else
            {
                p->die = (float)cl_main_c.cl.time + 0.1f * (rand_c.rand() % 5);
                p->color = (color & ~7) + (rand_c.rand() & 7);
                p->type = glquake_c.ptype_t.pt_slowgrav;

                for (j = 0; j < 3; j++)
                {
                    p->org[j] = org[j] + ((rand_c.rand() & 15) - 8);
                    p->vel[j] = dir[j] * 15;
                }
            }
        }
    }

    public static void R_LavaSplash(Vector3 org)
    {
        int i, j, k;
        glquake_c.particle_t* p;
        float vel;
        Vector3 dir;

        dir = new();

        for (i = -16; i < 16; i++)
        {
            for (j = -16; j < 16; j++)
            {
                for (k = 0; k < 1; k++)
                {
                    if (free_particles == null)
                    {
                        return;
                    }

                    p = free_particles;
                    free_particles = p->next;
                    p->next = active_particles;
                    active_particles = p;

                    p->die = (float)cl_main_c.cl.time + 2 + (rand_c.rand() & 31) * 0.02f;
                    p->color = 224 + (rand_c.rand() & 7);
                    p->type = glquake_c.ptype_t.pt_slowgrav;

                    dir[0] = j * 8 + (rand_c.rand() & 7);
                    dir[1] = i * 8 + (rand_c.rand() & 7);
                    dir[2] = 256;

                    p->org[0] = org[0] + dir[0];
                    p->org[1] = org[1] + dir[1];
                    p->org[2] = org[2] + (rand_c.rand() & 63);

                    mathlib_c.VectorNormalize(dir);
                    vel = 50 + (rand_c.rand() & 63);
                    mathlib_c.VectorScale(dir, vel, p->vel);
                }
            }
        }
    }

    public static void R_TeleportSplash(Vector3 org)
    {
        int i, j, k;
        glquake_c.particle_t* p;
        float vel;
        Vector3 dir;

        dir = new();

        for (i = -16; i < 16; i += 4)
        {
            for (j = -16; j < 16; j += 4)
            {
                for (k = -24; k < 32; k += 4)
                {
                    if (free_particles == null)
                    {
                        return;
                    }

                    p = free_particles;
                    free_particles = p->next;
                    p->next = active_particles;
                    active_particles = p;

                    p->die = (float)cl_main_c.cl.time + 0.2f + (rand_c.rand() & 7) * 0.02f;
                    p->color = 7 + (rand_c.rand() & 7);
                    p->type = glquake_c.ptype_t.pt_slowgrav;

                    dir[0] = j * 8;
                    dir[1] = i * 8;
                    dir[2] = k * 8;

                    p->org[0] = org[0] + i + (rand_c.rand() & 3);
                    p->org[1] = org[1] + j + (rand_c.rand() & 3);
                    p->org[2] = org[2] + k + (rand_c.rand() & 3);

                    mathlib_c.VectorNormalize(dir);
                    vel = 50 + (rand_c.rand() & 63);
                    mathlib_c.VectorScale(dir, vel, p->vel);
                }
            }
        }
    }

    public static void R_RocketTrail(Vector3 start, Vector3 end, int type)
    {
        Vector3 vec;
        float len;
        int j;
        glquake_c.particle_t* p;
        int dec;
        int tracercount = 0;

        vec = new();

        mathlib_c.VectorSubtract(end, start, vec);
        len = mathlib_c.VectorNormalize(vec);

        if (type < 128)
        {
            dec = 3;
        }
        else
        {
            dec = 1;
            type -= 128;
        }

        while (len > 0)
        {
            len -= dec;

            if (free_particles == null)
            {
                return;
            }

            p = free_particles;
            free_particles = p->next;
            p->next = active_particles;
            active_particles = p;

            mathlib_c.VectorCopy(mathlib_c.vec3_origin, p->vel);
            p->die = (float)cl_main_c.cl.time + 2;

            switch (type)
            {
                case 0:
                    p->ramp = (rand_c.rand() & 3);
                    p->color = ramp3[(int)p->ramp];
                    p->type = glquake_c.ptype_t.pt_fire;

                    for (j = 0; j < 3; j++)
                    {
                        p->org[j] = start[j] + ((rand_c.rand() % 6) - 3);
                    }
                    break;

                case 1:
                    p->ramp = (rand_c.rand() & 3) + 2;
                    p->color = ramp3[(int)p->ramp];
                    p->type = glquake_c.ptype_t.pt_fire;

                    for (j = 0; j < 3; j++)
                    {
                        p->org[j] = start[j] + ((rand_c.rand() % 6) - 3);
                    }
                    break;

                case 2:
                    p->type = glquake_c.ptype_t.pt_grav;
                    p->color = 67 + (rand_c.rand() & 3);

                    for (j = 0; j < 3; j++)
                    {
                        p->org[j] = start[j] + ((rand_c.rand() % 6) - 3);
                    }
                    break;

                case 3:
                case 5:
                    p->die = (float)cl_main_c.cl.time + 0.5f;
                    p->type = glquake_c.ptype_t.pt_static;

                    if (type == 3)
                    {
                        p->color = 52 + ((tracercount & 4) << 1);
                    }
                    else
                    {
                        p->color = 230 + ((tracercount & 4) << 1);
                    }

                    tracercount++;

                    mathlib_c.VectorCopy(start, p->org);

                    if ((tracercount & 1) != 0)
                    {
                        p->vel[0] = 30 * vec[1];
                        p->vel[1] = 30 * -vec[0];
                    }
                    else
                    {
                        p->vel[0] = 30 * -vec[1];
                        p->vel[1] = 30 * vec[0];
                    }
                    break;

                case 4:
                    p->type = glquake_c.ptype_t.pt_grav;
                    p->color = 67 + (rand_c.rand() & 3);

                    for (j = 0; j < 3; j++)
                    {
                        p->org[j] = start[j] + ((rand_c.rand() % 6) - 3);
                    }

                    len -= 3;
                    break;

                case 6:
                    p->color = 9 * 16 + 8 + (rand_c.rand() & 3);
                    p->type = glquake_c.ptype_t.pt_static;
                    p->die = (float)cl_main_c.cl.time + 0.3f;

                    for (j = 0; j < 3; j++)
                    {
                        p->org[j] = start[j] + ((rand_c.rand() & 15) - 8);
                    }
                    break;
            }

            mathlib_c.VectorAdd(start, vec, start);
        }
    }

    public static void R_DrawParticles()
    {
        glquake_c.particle_t* p, kill;
        float grav;
        int i;
        float time2, time3;
        float time1;
        float dvel;
        float frametime;

#if GLQUAKE
        Vector3 up, right;
        float scale;

        GL_Bind(particletexture);
        glEnable(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_MODULATE);
        glBegin(GL_TRIANGLES);

        mathlib_c.VectorScale(vup, 1.5f, up);
        mathlib_c.VectorScale(vright, 1.5f, right);
#else
        d_part_c.D_StartParticles();

        mathlib_c.VectorScale(r_shared_c.vright, r_main_c.xscaleshrink, r_pright);
        mathlib_c.VectorScale(r_shared_c.vup, r_main_c.yscaleshrink, r_pup);
        mathlib_c.VectorCopy(r_shared_c.vpn, r_ppn);
#endif
        frametime = (float)(cl_main_c.cl.time + cl_main_c.cl.oldtime);
        time3 = frametime * 15;
        time2 = frametime * 10;
        time1 = frametime * 5;
        grav = frametime * sv_phys_c.sv_gravity.value * 0.05f;
        dvel = 4 * frametime;

        for (; ; )
        {
            kill = active_particles;

            if (kill != null && kill->die < cl_main_c.cl.time)
            {
                active_particles = kill->next;
                kill->next = free_particles;
                free_particles = kill;
                continue;
            }

            break;
        }

        for (p = active_particles; p != null; p = p->next)
        {
            for (; ; )
            {
                kill = p->next;

                if (kill != null && kill->die < (float)cl_main_c.cl.time)
                {
                    p->next = kill->next;
                    kill->next = free_particles;
                    free_particles = kill;
                    continue;
                }

                break;
            }
        }

#if GLQUAKE
        scale = (p->org[0] - render_c.r_origin[0]) * r_shared_c.vpn[0] + (p->org[1] - render_c.r_origin[1]) * r_shared_c.vpn[1] + (p->org[2] - render_c.r_origin[2]) * r_shared_c.vpn[2];

        if (scale < 20)
        {
            scale = 1;
        }
        else
        {
            scale = 1 + scale * 0.004f;
        }

        glColor3ubv((byte*)vid_c.d_8to24table[(int)p->color]);
        glTexCoord2f(0, 0);
        glVertex3fv(p->org);
        glTexCoord2f(1, 0);
        glVertex3f(p->org[0] + up[0] * scale, p->org[1] + up[1] * scale, p->org[2] + up[2] * scale);
        glTexCoord2f(0, 1);
        glVertex3f(p->org[0] + right[0] * scale, p->org[1] + right[1] * scale, p->org[2] + right[2] * scale);
#else
        d_part_c.D_DrawParticle(p);
#endif
        p->org[0] += p->vel[0] * frametime;
        p->org[1] += p->vel[1] * frametime;
        p->org[2] += p->vel[2] * frametime;

        switch (p->type)
        {
            case glquake_c.ptype_t.pt_static:
                break;

            case glquake_c.ptype_t.pt_fire:
                p->ramp += time1;

                if (p->ramp >= 6)
                {
                    p->die = -1;
                }
                else
                {
                    p->color = ramp3[(int)p->ramp];
                }

                p->vel[2] += grav;
                break;

            case glquake_c.ptype_t.pt_explode:
                p->ramp += time2;

                if (p->ramp >= 8)
                {
                    p->die = -1;
                }
                else
                {
                    p->color = ramp1[(int)p->ramp];
                }

                for (i = 0; i < 3; i++)
                {
                    p->vel[i] += p->vel[i] * dvel;
                }

                p->vel[2] -= grav;
                break;

            case glquake_c.ptype_t.pt_explode2:
                p->ramp += time3;

                if (p->ramp >= 8)
                {
                    p->die = -1;
                }
                else
                {
                    p->color = ramp2[(int)p->ramp];
                }

                p->vel[2] -= grav;
                break;

            case glquake_c.ptype_t.pt_blob:
                for (i = 0; i < 3; i++)
                {
                    p->vel[i] += p->vel[i] * dvel;
                }

                p->vel[2] -= grav;
                break;

            case glquake_c.ptype_t.pt_blob2:
                for (i = 0; i < 3; i++)
                {
                    p->vel[i] -= p->vel[i] * dvel;
                }

                p->vel[2] -= grav;
                break;

            case glquake_c.ptype_t.pt_grav:
#if QUAKE2
                p->vel[2] -= grav * 20;
                break;
#endif
            case glquake_c.ptype_t.pt_slowgrav:
                p->vel[2] -= grav;
                break;
        }

#if GLQUAKE
        glEnd();
        glDisable(GL_BLEND);
        glTexEnvf(GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_REPLACE);
#else
        d_part_c.D_EndParticles();
#endif
    }
}