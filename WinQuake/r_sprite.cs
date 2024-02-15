namespace Quake;

public unsafe class r_sprite_c
{
    public static int clip_current;
    public static int[][] clip_verts = new int[2][];
    public static int sprite_width, sprite_height;

    public static d_iface_c.spritedesc_t r_spritedesc;

    public static void R_RotateSprite(float beamlength)
    {
        Vector3 vec = new();

        if (beamlength == 0.0f)
        {
            return;
        }

        mathlib_c.VectorScale(r_spritedesc.vpn, -beamlength, vec);
        mathlib_c.VectorAdd(r_bsp_c.r_entorigin, vec, r_bsp_c.r_entorigin);
        mathlib_c.VectorSubtract(r_bsp_c.modelorg, vec, r_bsp_c.modelorg);
    }

    public static int R_ClipSpriteFace(int nump, r_local_c.clipplane_t* pclipplane)
    {
        int i, outcount;
        float[] dists = new float[r_shared_c.MAXWORKINGVERTS + 1];
        float frac, clipdist;
        float* pclipnormal;
        float* _in, instep, outstep, vert2;

        clipdist = pclipplane->dist;
        pclipnormal = mathlib_c.VecToFloatPtr(pclipplane->normal);

        if (clip_current != 0)
        {
            _in = (float*)clip_verts[1][0];
            outstep = (float*)clip_verts[0][0];
            clip_current = 0;
        }
        else
        {
            _in = (float*)clip_verts[0][0];
            outstep = (float*)clip_verts[1][0];
            clip_current = 1;
        }

        instep = _in;

        for (i = 0; i < nump; i++, instep += sizeof(int) / sizeof(float))
        {
            dists[i] = mathlib_c.DotProduct(instep, pclipnormal) - clipdist;
        }

        dists[nump] = dists[0];
        common_c.Q_memcpy(*instep, *_in, sizeof(int));

        instep = _in;
        outcount = 0;

        for (i = 0; i < nump; i++, instep += sizeof(int) / sizeof(float))
        {
            if (dists[i] >= 0)
            {
                common_c.Q_memcpy(*outstep, *instep, sizeof(int));
                outstep += sizeof(int) / sizeof(float);
                outcount++;
            }

            if (dists[i] == 0 || dists[i + 1] == 0)
            {
                continue;
            }

            if (dists[i] > 0 || dists[i + 1] > 0)
            {
                continue;
            }

            frac = dists[i] / (dists[i] - dists[i + 1]);

            vert2 = instep + sizeof(int) / sizeof(float);

            outstep[0] = instep[0] + frac * (vert2[0] - instep[0]);
            outstep[1] = instep[1] + frac * (vert2[1] - instep[1]);
            outstep[2] = instep[2] + frac * (vert2[2] - instep[2]);
            outstep[3] = instep[3] + frac * (vert2[3] - instep[3]);
            outstep[4] = instep[4] + frac * (vert2[4] - instep[4]);

            outstep += sizeof(int) / sizeof(float);
            outcount++;
        }

        return outcount;
    }

    public static void R_SetupAndDrawSprite()
    {
        int i, nump;
        float dot, scale;
        float* pv;
        int[][] pverts;
        Vector3 left, up, right, down, transformed, local;
        d_iface_c.emitpoint_t* outverts;
        d_iface_c.emitpoint_t* pout;

        outverts = pout = null;
        left = up = right = down = transformed = local = new();

        dot = mathlib_c.DotProduct(r_spritedesc.vpn, r_bsp_c.modelorg);

        if (dot >= 0)
        {
            return;
        }

        mathlib_c.VectorScale(r_spritedesc.vright, r_spritedesc.pspriteframe->right, right);
        mathlib_c.VectorScale(r_spritedesc.vup, r_spritedesc.pspriteframe->up, up);
        mathlib_c.VectorScale(r_spritedesc.vright, r_spritedesc.pspriteframe->left, left);
        mathlib_c.VectorScale(r_spritedesc.vup, r_spritedesc.pspriteframe->down, down);

        pverts = clip_verts;

        pverts[0][0] = (int)(r_bsp_c.r_entorigin[0] + up[0] + left[0]);
        pverts[0][1] = (int)(r_bsp_c.r_entorigin[1] + up[1] + left[1]);
        pverts[0][2] = (int)(r_bsp_c.r_entorigin[2] + up[2] + left[2]);
        pverts[0][3] = 0;
        pverts[0][4] = 0;

        pverts[1][0] = (int)(r_bsp_c.r_entorigin[0] + up[0] + right[0]);
        pverts[1][1] = (int)(r_bsp_c.r_entorigin[1] + up[1] + right[1]);
        pverts[1][2] = (int)(r_bsp_c.r_entorigin[2] + up[2] + right[2]);
        pverts[1][3] = sprite_width;
        pverts[1][4] = 0;

        pverts[2][0] = (int)(r_bsp_c.r_entorigin[0] + down[0] + right[0]);
        pverts[2][1] = (int)(r_bsp_c.r_entorigin[1] + down[1] + right[1]);
        pverts[2][2] = (int)(r_bsp_c.r_entorigin[2] + down[2] + right[2]);
        pverts[2][3] = sprite_width;
        pverts[2][4] = sprite_height;

        pverts[3][0] = (int)(r_bsp_c.r_entorigin[0] + down[0] + left[0]);
        pverts[3][0] = (int)(r_bsp_c.r_entorigin[1] + down[1] + left[1]);
        pverts[3][0] = (int)(r_bsp_c.r_entorigin[2] + down[2] + left[2]);
        pverts[3][0] = 0;
        pverts[3][0] = sprite_height;

        nump = 4;
        clip_current = 0;

        for (i = 0; i < 4; i++)
        {
            nump = R_ClipSpriteFace(nump, &r_draw_c.view_clipplanes[i]);

            if (nump < 3)
            {
                return;
            }
            
            if (nump >= r_shared_c.MAXWORKINGVERTS)
            {
                sys_win_c.Sys_Error("R_SetupAndDrawSprite: too many points");
            }
        }

        pv = (float*)clip_verts[clip_current][0];
        r_spritedesc.nearzi = -999999;

        for (i = 0; i < nump; i++)
        {
            mathlib_c.VectorSubtract(pv, render_c.r_origin, local);
            r_misc_c.TransformVector(local, transformed);

            if (transformed[2] < r_local_c.NEAR_CLIP)
            {
                transformed[2] = r_local_c.NEAR_CLIP;
            }

            pout = &outverts[i];
            pout->zi = 1.0f / transformed[2];

            if (pout->zi > r_spritedesc.nearzi)
            {
                r_spritedesc.nearzi = pout->zi;
            }

            pout->s = pv[3];
            pout->t = pv[4];

            scale = r_shared_c.xscale * pout->zi;
            pout->u = (r_shared_c.xcenter + scale * transformed[0]);

            scale = r_shared_c.yscale * pout->zi;
            pout->v = (r_shared_c.ycenter + scale * transformed[1]);

            pv += sizeof(int) / sizeof(float);
        }

        r_spritedesc.nump = nump;
        r_spritedesc.pverts = outverts;
        d_sprite_c.D_DrawSprite();
    }

    public static model_c.mspriteframe_t* R_GetSpriteFrame(model_c.msprite_t* psprite)
    {
        model_c.mspritegroup_t* pspritegroup;
        model_c.mspriteframe_t* pspriteframe;
        int i, numframes, frame;
        float* pintervals;
        float fullinterval, targettime, time;

        frame = r_bsp_c.currententity->frame;

        if ((frame >= psprite->numframes) || (frame < 0))
        {
            console_c.Con_Printf($"R_DrawSprite: no such frame {frame}");
            frame = 0;
        }

        if (psprite->frames[frame].type == spritegn_c.spriteframetype_t.SPR_SINGLE)
        {
            pspriteframe = psprite->frames[frame].frameptr;
        }
        else
        {
            pspritegroup = (model_c.mspritegroup_t*)psprite->frames[frame].frameptr;
            pintervals = pspritegroup->intervals;
            numframes = pspritegroup->numframes;
            fullinterval = pintervals[numframes - 1];

            time = (float)(cl_main_c.cl.time + r_bsp_c.currententity->syncbase);

            targettime = time - ((int)(time / fullinterval)) * fullinterval;

            for (i = 0; i < (numframes - 1); i++)
            {
                if (pintervals[i] > targettime)
                {
                    break;
                }
            }

            pspriteframe = &pspritegroup->frames[i];
        }

        return pspriteframe;
    }

    public static void R_DrawSprite()
    {
        int i;
        model_c.msprite_t* psprite;
        Vector3 tvec;
        float dot, angle, sr, cr;

        tvec = new();

        psprite = (model_c.msprite_t*)r_bsp_c.currententity->model->cache.data;

        r_spritedesc.pspriteframe = R_GetSpriteFrame(psprite);

        sprite_width = r_spritedesc.pspriteframe->width;
        sprite_height = r_spritedesc.pspriteframe->height;

        if (psprite->type == spritegn_c.SPR_FACING_UPRIGHT)
        {
            tvec[0] = -r_bsp_c.modelorg[0];
            tvec[1] = -r_bsp_c.modelorg[1];
            tvec[2] = -r_bsp_c.modelorg[2];
            mathlib_c.VectorNormalize(tvec);
            dot = tvec[2];

            if ((dot > 0.99848f) || (dot < -0.99848f))
            {
                return;
            }

            r_spritedesc.vup[0] = 0;
            r_spritedesc.vup[1] = 0;
            r_spritedesc.vup[2] = 1;
            r_spritedesc.vright[0] = tvec[1];
            r_spritedesc.vright[1] = -tvec[0];
            r_spritedesc.vright[2] = 0;
            mathlib_c.VectorNormalize(r_spritedesc.vright);
            r_spritedesc.vpn[0] = -r_spritedesc.vright[1];
            r_spritedesc.vpn[1] = r_spritedesc.vright[0];
            r_spritedesc.vpn[2] = 0;
        }
        else if (psprite->type == spritegn_c.SPR_VP_PARALLEL)
        {
            for (i = 0; i < 3; i++)
            {
                r_spritedesc.vup[i] = render_c.vup[i];
                r_spritedesc.vright[i] = render_c.vright[i];
                r_spritedesc.vpn[i] = render_c.vpn[i];
            }
        }
        else if (psprite->type == spritegn_c.SPR_VP_PARALLEL_UPRIGHT)
        {
            dot = render_c.vpn[2];

            if ((dot > 0.999848f) || (dot < -0.999848f))
            {
                return;
            }

            r_spritedesc.vup[0] = 0;
            r_spritedesc.vup[1] = 0;
            r_spritedesc.vup[2] = 1;
            r_spritedesc.vright[0] = render_c.vpn[1];
            r_spritedesc.vright[1] = -render_c.vpn[0];
            r_spritedesc.vright[2] = 0;
            mathlib_c.VectorNormalize(r_spritedesc.vright);
            r_spritedesc.vpn[0] = -r_spritedesc.vright[1];
            r_spritedesc.vpn[1] = r_spritedesc.vright[0];
            r_spritedesc.vpn[2] = 0;
        }
        else if (psprite->type == spritegn_c.SPR_ORIENTED)
        {
            mathlib_c.AngleVectors(r_bsp_c.currententity->angles, r_spritedesc.vpn, r_spritedesc.vright, r_spritedesc.vup);
        }
        else if (psprite->type == spritegn_c.SPR_VP_PARALLEL_ORIENTED)
        {
            angle = r_bsp_c.currententity->angles[quakedef_c.ROLL] * (MathF.PI * 2 / 360);
            sr = MathF.Sin(angle);
            cr = MathF.Cos(angle);

            for (i = 0; i < 3; i++)
            {
                r_spritedesc.vpn[i] = render_c.vpn[i];
                r_spritedesc.vright[i] = render_c.vright[i] * cr + render_c.vup[i] * sr;
                r_spritedesc.vup[i] = render_c.vright[i] * -sr + render_c.vup[i] * cr;
            }
        }
        else
        {
            sys_win_c.Sys_Error($"R_DrawSprite: Bad sprite type {psprite->type}");
        }

        R_RotateSprite(psprite->beamlength);

        R_SetupAndDrawSprite();
    }
}