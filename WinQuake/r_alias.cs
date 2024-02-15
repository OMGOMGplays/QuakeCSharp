namespace Quake;

public unsafe class r_alias_c
{
    public const int LIGHT_MIN = 5;

    public static model_c.mtriangle_t* ptriangles;
    public static d_iface_c.affinetridesc_t r_affinetridesc;

    public static void* acolormap;

    public static modelgen_c.trivertx_t* r_apverts;

    public static modelgen_c.mdl_t* pmdl;
    public static Vector3 r_plightvec;
    public static int r_ambientlight;
    public static float r_shadelight;
    public static model_c.aliashdr_t* paliashdr;
    public static d_iface_c.finalvert_t* pfinalverts;
    public static r_local_c.auxvert_t* pauxverts;
    public static float ziscale;
    public static model_c.model_t* pmodel;

    public static Vector3 alias_forward, alias_right, alias_up;

    public static model_c.maliasskindesc_t* pskindesc;

    public static int r_amodels_drawn;
    public static int a_skinwidth;
    public static int r_anumverts;

    public static float** aliastransform;

    public struct aedge_t
    {
        public int index0;
        public int index1;
    }

    public static aedge_t[] aedges =
    {
        new aedge_t { index0 = 0, index1 = 1 },
        new aedge_t { index0 = 1, index1 = 2 },
        new aedge_t { index0 = 2, index1 = 3 },
        new aedge_t { index0 = 3, index1 = 0 },
        new aedge_t { index0 = 4, index1 = 5 },
        new aedge_t { index0 = 5, index1 = 6 },
        new aedge_t { index0 = 6, index1 = 7 },
        new aedge_t { index0 = 7, index1 = 4 },
        new aedge_t { index0 = 0, index1 = 5 },
        new aedge_t { index0 = 1, index1 = 4 },
        new aedge_t { index0 = 2, index1 = 7 },
        new aedge_t { index0 = 3, index1 = 6 },
    };

    public const int NUMVERTEXNORMALS = 162;

    public static float** r_avertexnormals;

    public static bool R_AliasCheckBox()
    {
        int i, flags, frame, numv;
        model_c.aliashdr_t* pahdr;
        float zi, v0, v1, frac;
        float** basepts = null;
        d_iface_c.finalvert_t* pv0, pv1, viewpts = null;
        r_local_c.auxvert_t* pa0, pa1, viewaux = null;
        model_c.maliasframedesc_t* pframedesc;
        bool zclipped, zfullyclipped;
        uint anyclip, allclip;
        int minz;

        r_bsp_c.currententity->trivial_accept = 0;
        pmodel = r_bsp_c.currententity->model;
        pahdr = (model_c.aliashdr_t*)model_c.Mod_Extradata(pmodel);
        pmdl = (modelgen_c.mdl_t*)((byte*)pahdr + pahdr->model);

        R_AliasSetUpTransform(0);

        frame = r_bsp_c.currententity->frame;

        if ((frame >= pmdl->numframes) || (frame < 0))
        {
            console_c.Con_DPrintf($"No such frame {frame} {*pmodel->name}\n");
            frame = 0;
        }

        pframedesc = &pahdr->frames[frame];

        basepts[0][0] = basepts[1][0] = basepts[2][0] = basepts[3][0] = (float)pframedesc->bboxmin.v[0];
        basepts[4][0] = basepts[5][0] = basepts[6][0] = basepts[7][0] = (float)pframedesc->bboxmax.v[0];

        basepts[0][1] = basepts[3][1] = basepts[5][1] = basepts[6][1] = (float)pframedesc->bboxmin.v[1];
        basepts[1][1] = basepts[2][1] = basepts[4][1] = basepts[7][1] = (float)pframedesc->bboxmax.v[1];

        basepts[0][2] = basepts[1][2] = basepts[4][2] = basepts[5][2] = (float)pframedesc->bboxmin.v[2];
        basepts[2][2] = basepts[3][2] = basepts[6][2] = basepts[7][2] = (float)pframedesc->bboxmax.v[2];

        zclipped = false;
        zfullyclipped = true;

        minz = 9999;

        for (i = 0; i < 8; i++)
        {
            R_AliasTransformVector(&basepts[i][0], &viewaux[i].fv[0]);

            if (viewaux[i].fv[2] < r_local_c.ALIAS_Z_CLIP_PLANE)
            {
                viewpts[i].flags = r_shared_c.ALIAS_Z_CLIP;
                zclipped = true;
            }
            else
            {
                if (viewaux[i].fv[2] < minz)
                {
                    minz = (int)viewaux[i].fv[2];
                }

                viewpts[i].flags = 0;
                zfullyclipped = false;
            }
        }

        if (zfullyclipped)
        {
            return false;
        }

        numv = 8;

        if (zclipped)
        {
            for (i = 0; i < 12; i++)
            {
                pv0 = &viewpts[aedges[i].index0];
                pv1 = &viewpts[aedges[i].index1];
                pa0 = &viewaux[aedges[i].index0];
                pa1 = &viewaux[aedges[i].index1];

                if ((pv0->flags ^ pv1->flags) != 0)
                {
                    frac = (r_local_c.ALIAS_Z_CLIP_PLANE - pa0->fv[2]) / (pa1->fv[2] - pa0->fv[2]);
                    viewaux[numv].fv[0] = pa0->fv[0] + (pa1->fv[0] - pa0->fv[0]) * frac;
                    viewaux[numv].fv[1] = pa0->fv[1] + (pa1->fv[1] - pa0->fv[1]) * frac;
                    viewaux[numv].fv[2] = r_local_c.ALIAS_Z_CLIP_PLANE;
                    viewpts[numv].flags = 0;
                    numv++;
                }
            }
        }

        anyclip = 0;
        allclip = r_shared_c.ALIAS_XY_CLIP_MASK;

        for (i = 0; i < numv; i++)
        {
            if ((viewpts[i].flags & r_shared_c.ALIAS_Z_CLIP) != 0)
            {
                continue;
            }

            zi = 1.0f / viewaux[i].fv[2];

            v0 = (viewaux[i].fv[0] * r_shared_c.xscale * zi) + r_shared_c.xcenter;
            v1 = (viewaux[i].fv[1] * r_shared_c.yscale * zi) + r_shared_c.ycenter;

            flags = 0;

            if (v0 < render_c.r_refdef.fvrectx)
            {
                flags |= r_shared_c.ALIAS_LEFT_CLIP;
            }

            if (v1 < render_c.r_refdef.fvrecty)
            {
                flags |= r_shared_c.ALIAS_TOP_CLIP;
            }

            if (v0 > render_c.r_refdef.fvrectright)
            {
                flags |= r_shared_c.ALIAS_RIGHT_CLIP;
            }

            if (v1 > render_c.r_refdef.fvrectbottom)
            {
                flags |= r_shared_c.ALIAS_BOTTOM_CLIP;
            }

            anyclip |= (uint)flags;
            allclip &= (uint)flags;
        }

        if (allclip != 0)
        {
            return false;
        }

        r_bsp_c.currententity->trivial_accept = ((anyclip == 0) && !zclipped) ? 1 : 0;

        if (r_bsp_c.currententity->trivial_accept != 0)
        {
            if (minz > (r_main_c.r_aliastransition + (pmdl->size * r_main_c.r_resfudge)))
            {
                r_bsp_c.currententity->trivial_accept |= 2;
            }
        }

        return true;
    }

    public static void R_AliasTransformVector(Vector3 input, Vector3 output)
    {
        output[0] = mathlib_c.DotProduct(input, aliastransform[0]) + aliastransform[0][3];
        output[1] = mathlib_c.DotProduct(input, aliastransform[1]) + aliastransform[1][3];
        output[2] = mathlib_c.DotProduct(input, aliastransform[2]) + aliastransform[2][3];
    }

    public static void R_AliasPreparePoints()
    {
        int i;
        modelgen_c.stvert_t* pstverts;
        d_iface_c.finalvert_t* fv;
        r_local_c.auxvert_t* av;
        model_c.mtriangle_t* ptri;
        d_iface_c.finalvert_t* pfv = null;

        pstverts = (modelgen_c.stvert_t*)((byte*)paliashdr + paliashdr->stverts);
        r_anumverts = pmdl->numverts;
        fv = pfinalverts;
        av = pauxverts;

        for (i = 0; i < r_anumverts; i++, fv++, av++, r_apverts++, pstverts++)
        {
            R_AliasTransformFinalVert(fv, av, r_apverts, pstverts);

            if (av->fv[2] < r_local_c.ALIAS_Z_CLIP_PLANE)
            {
                fv->flags |= r_shared_c.ALIAS_Z_CLIP;
            }
            else
            {
                R_AliasProjectFinalVert(fv, av);

                if (fv->v[0] < r_main_c.r_refdef.aliasvrect.x)
                {
                    fv->flags |= r_shared_c.ALIAS_LEFT_CLIP;
                }

                if (fv->v[1] < r_main_c.r_refdef.aliasvrect.y)
                {
                    fv->flags |= r_shared_c.ALIAS_TOP_CLIP;
                }

                if (fv->v[0] > r_main_c.r_refdef.aliasvrectright)
                {
                    fv->flags |= r_shared_c.ALIAS_RIGHT_CLIP;
                }

                if (fv->v[1] > r_main_c.r_refdef.aliasvrectbottom)
                {
                    fv->flags |= r_shared_c.ALIAS_BOTTOM_CLIP;
                }
            }
        }

        r_affinetridesc.numtriangles = 1;

        ptri = (model_c.mtriangle_t*)((byte*)paliashdr + paliashdr->triangles);

        for (i = 0; i < pmdl->numtris; i++, ptri++)
        {
            pfv[0] = pfinalverts[ptri->vertindex[0]];
            pfv[1] = pfinalverts[ptri->vertindex[1]];
            pfv[2] = pfinalverts[ptri->vertindex[2]];

            if ((pfv[0].flags & pfv[1].flags & pfv[2].flags & (r_shared_c.ALIAS_XY_CLIP_MASK | r_shared_c.ALIAS_Z_CLIP)) != 0)
            {
                continue;
            }

            if (((pfv[0].flags | pfv[1].flags | pfv[2].flags) & (r_shared_c.ALIAS_XY_CLIP_MASK | r_shared_c.ALIAS_Z_CLIP)) != 0)
            {
                r_affinetridesc.pfinalverts = pfinalverts;
                r_affinetridesc.ptriangles = ptri;
                d_polyse_c.D_PolysetDraw();
            }
            else
            {
                R_AliasClipTriangle(ptri);
            }
        }
    }

    public static void R_AliasSetUpTransform(int trivial_accept)
    {
        int i;
        float** rotationmatrix, t2matrix;
        float** tmatrix;
        float** viewmatrix;
        Vector3 angles = new();

        rotationmatrix = t2matrix = tmatrix = viewmatrix = null;

        angles[quakedef_c.ROLL] = r_bsp_c.currententity->angles[quakedef_c.ROLL];
        angles[quakedef_c.PITCH] = r_bsp_c.currententity->angles[quakedef_c.PITCH];
        angles[quakedef_c.YAW] = r_bsp_c.currententity->angles[quakedef_c.YAW];
        mathlib_c.AngleVectors(angles, alias_forward, alias_right, alias_up);

        tmatrix[0][0] = pmdl->scale[0];
        tmatrix[1][1] = pmdl->scale[1];
        tmatrix[2][2] = pmdl->scale[2];

        tmatrix[0][3] = pmdl->scale_origin[0];
        tmatrix[1][3] = pmdl->scale_origin[1];
        tmatrix[2][3] = pmdl->scale_origin[2];

        for (i = 0; i < 3; i++)
        {
            t2matrix[i][0] = alias_forward[i];
            t2matrix[i][1] = -alias_right[i];
            t2matrix[i][2] = alias_up[i];
        }

        t2matrix[0][3] = -r_bsp_c.modelorg[0];
        t2matrix[1][3] = -r_bsp_c.modelorg[1];
        t2matrix[2][3] = -r_bsp_c.modelorg[2];

        mathlib_c.R_ConcatTransforms(t2matrix, tmatrix, rotationmatrix);

        mathlib_c.VectorCopy(r_shared_c.vright, viewmatrix[0]);
        mathlib_c.VectorCopy(r_shared_c.vup, viewmatrix[1]);
        mathlib_c.VectorInverse(viewmatrix[1]);
        mathlib_c.VectorCopy(r_shared_c.vpn, viewmatrix[2]);

        mathlib_c.R_ConcatTransforms(viewmatrix, rotationmatrix, aliastransform);

        if (trivial_accept != 0)
        {
            for (i = 0; i < 4; i++)
            {
                aliastransform[0][i] *= r_main_c.aliasxscale * (1.0f / ((float)0x8000 * 0x10000));
                aliastransform[1][i] *= r_main_c.aliasyscale * (1.0f / ((float)0x8000 * 0x10000));
                aliastransform[2][i] *= 1.0f / ((float)0x8000 * 0x10000);
            }
        }
    }

    public static void R_AliasTransformFinalVert(d_iface_c.finalvert_t* fv, r_local_c.auxvert_t* av, modelgen_c.trivertx_t* pverts, modelgen_c.stvert_t* pstverts)
    {
        int temp;
        float lightcos;
        float* plightnormal;

        av->fv[0] = mathlib_c.DotProduct(pverts->v, aliastransform[0]) + aliastransform[0][3];
        av->fv[1] = mathlib_c.DotProduct(pverts->v, aliastransform[1]) + aliastransform[1][3];
        av->fv[2] = mathlib_c.DotProduct(pverts->v, aliastransform[2]) + aliastransform[2][3];

        fv->v[2] = pstverts->s;
        fv->v[3] = pstverts->t;

        fv->flags = pstverts->onseam;

        plightnormal = r_avertexnormals[pverts->lightnormalindex];
        lightcos = mathlib_c.DotProduct(plightnormal, r_plightvec);
        temp = r_ambientlight;

        if (lightcos < 0)
        {
            temp += (int)(r_shadelight * lightcos);

            if (temp < 0)
            {
                temp = 0;
            }
        }

        fv->v[4] = temp;
    }

#if !id386
    public static void R_AliasTransformAndProjectFinalVerts(d_iface_c.finalvert_t* fv, modelgen_c.stvert_t* pstverts)
    {
        int i, temp;
        float lightcos, zi;
        float* plightnormal;
        modelgen_c.trivertx_t* pverts;

        pverts = r_apverts;

        for (i = 0; i < r_anumverts; i++, fv++, pverts++, pstverts++)
        {
            zi = 1.0f / (mathlib_c.DotProduct(pverts->v, aliastransform[2]) + aliastransform[2][3]);

            fv->v[5] = (int)zi;

            fv->v[0] = (int)(((mathlib_c.DotProduct(pverts->v, aliastransform[0]) + aliastransform[0][3]) * zi) + r_main_c.aliasxcenter);
            fv->v[1] = (int)(((mathlib_c.DotProduct(pverts->v, aliastransform[1]) + aliastransform[1][3]) * zi) + r_main_c.aliasycenter);

            fv->v[2] = pstverts->s;
            fv->v[3] = pstverts->t;
            fv->flags = pstverts->onseam;

            plightnormal = r_avertexnormals[pverts->lightnormalindex];
            lightcos = mathlib_c.DotProduct(plightnormal, r_plightvec);
            temp = r_ambientlight;

            if (lightcos < 0)
            {
                temp += (int)(r_shadelight * lightcos);

                if (temp < 0)
                {
                    temp = 0;
                }
            }

            fv->v[4] = temp;
        }
    }
#endif

    public static void R_AliasProjectFinalVert(d_iface_c.finalvert_t* fv, r_local_c.auxvert_t* av)
    {
        float zi;

        zi = 1.0f / av->fv[2];

        fv->v[5] = (int)(zi * ziscale);

        fv->v[0] = (int)((av->fv[0] * r_main_c.aliasxscale * zi) + r_main_c.aliasxcenter);
        fv->v[1] = (int)((av->fv[1] * r_main_c.aliasyscale * zi) + r_main_c.aliasycenter);
    }

    public static void R_AliasPrepareUnclippedPoints()
    {
        modelgen_c.stvert_t* pstverts;
        d_iface_c.finalvert_t* fv;

        pstverts = (modelgen_c.stvert_t*)((byte*)paliashdr + paliashdr->stverts);
        r_anumverts = pmdl->numverts;
        fv = pfinalverts;

        R_AliasTransformAndProjectFinalVerts(fv, pstverts);

        if (r_affinetridesc.drawtype != 0)
        {
            d_polyse_c.D_PolysetDrawFinalVerts(fv, r_anumverts);
        }

        r_affinetridesc.pfinalverts = pfinalverts;
        r_affinetridesc.ptriangles = (model_c.mtriangle_t*)((byte*)paliashdr + paliashdr->triangles);
        r_affinetridesc.numtriangles = pmdl->numtris;

        d_polyse_c.D_PolysetDraw();
    }

    public static void R_AliasSetupSkin()
    {
        int skinnum;
        int i, numskins;
        model_c.maliasskingroup_t* paliasskingroup;
        float* pskinintervals;
        float fullskininterval;
        float skintargettime, skintime;

        fullskininterval = skintargettime = skintime = 0;

        skinnum = r_bsp_c.currententity->skinnum;

        if ((skinnum >= pmdl->numskins) || (skinnum < 0))
        {
            console_c.Con_DPrintf($"R_AliasSetupSkin: no such skin # {skinnum}\n");
            skinnum = 0;
        }

        pskindesc = ((model_c.maliasskindesc_t*)((byte*)paliashdr + paliashdr->skindesc)) + skinnum;
        a_skinwidth = pmdl->skinwidth;

        if (pskindesc->type == modelgen_c.aliasskintype_t.ALIAS_SKIN_GROUP)
        {
            paliasskingroup = (model_c.maliasskingroup_t*)((byte*)paliashdr + pskindesc->skin);
            pskinintervals = (float*)((byte*)paliashdr + paliasskingroup->intervals);
            numskins = paliasskingroup->numskins;
            fullskininterval = pskinintervals[numskins - 1];

            skintime = (float)cl_main_c.cl.time + r_bsp_c.currententity->syncbase;

            for (i = 0; i < (numskins - 1); i++)
            {
                if (pskinintervals[i] > skintargettime)
                {
                    break;
                }
            }

            *pskindesc = paliasskingroup->skindescs[i];
        }

        r_affinetridesc.pskindesc = pskindesc;
        r_affinetridesc.pskin = (byte*)paliashdr + pskindesc->skin;
        r_affinetridesc.skinwidth = a_skinwidth;
        r_affinetridesc.seamfixupX16 = (a_skinwidth >> 1) << 16;
        r_affinetridesc.skinheight = pmdl->skinheight;
    }

    public static void R_AliasSetupLighting(r_local_c.alight_t* plighting)
    {
        r_ambientlight = plighting->ambientlight;

        if (r_ambientlight < LIGHT_MIN)
        {
            r_ambientlight = LIGHT_MIN;
        }

        r_ambientlight = (255 - r_ambientlight) << vid_c.VID_CBITS;

        if (r_ambientlight < LIGHT_MIN)
        {
            r_ambientlight = LIGHT_MIN;
        }

        r_shadelight = plighting->shadelight;

        if (r_shadelight < 0)
        {
            r_shadelight = 0;
        }

        r_shadelight *= vid_c.VID_GRADES;

        r_plightvec[0] = mathlib_c.DotProduct(plighting->plightvec, alias_forward);
        r_plightvec[1] = -mathlib_c.DotProduct(plighting->plightvec, alias_right);
        r_plightvec[2] = mathlib_c.DotProduct(plighting->plightvec, alias_up);
    }

    public static void R_AliasSetupFrame()
    {
        int frame;
        int i, numframes;
        model_c.maliasgroup_t* paliasgroup;
        float* pintervals;
        float fullinterval, targettime, time;

        frame = r_bsp_c.currententity->frame;

        if ((frame >= pmdl->numframes) || (frame < 0))
        {
            console_c.Con_DPrintf($"R_AliasSetupFrame: no such frame {frame}\n");
            frame = 0;
        }

        if (paliashdr->frames[frame].type == modelgen_c.aliasframetype_t.ALIAS_SINGLE)
        {
            r_apverts = (modelgen_c.trivertx_t*)((byte*)paliashdr + paliashdr->frames[frame].frame);
            return;
        }

        paliasgroup = (model_c.maliasgroup_t*)((byte*)paliashdr + paliashdr->frames[frame].frame);
        pintervals = (float*)((byte*)paliashdr + paliasgroup->intervals);
        numframes = paliasgroup->numframes;
        fullinterval = pintervals[numframes - 1];

        time = (float)cl_main_c.cl.time + r_bsp_c.currententity->syncbase;

        targettime = time - ((int)(time / fullinterval)) * fullinterval;

        for (i = 0; i < (numframes - 1); i++)
        {
            if (pintervals[i] > targettime)
            {
                break;
            }
        }

        r_apverts = (modelgen_c.trivertx_t*)((byte*)paliashdr + paliasgroup->frames[i].frame);
    }

    public static void R_AliasDrawModel(r_local_c.alight_t* plighting)
    {
        d_iface_c.finalvert_t* finalverts = null;
        r_local_c.auxvert_t* auxverts = null;

        r_amodels_drawn++;

        pfinalverts = (d_iface_c.finalvert_t*)(((long)&finalverts[0] + quakedef_c.CACHE_SIZE - 1) & ~(quakedef_c.CACHE_SIZE - 1));
        pauxverts = &auxverts[0];

        paliashdr = (model_c.aliashdr_t*)model_c.Mod_Extradata(r_bsp_c.currententity->model);
        pmdl = (modelgen_c.mdl_t*)((byte*)paliashdr + paliashdr->model);

        R_AliasSetupSkin();
        R_AliasSetUpTransform(r_bsp_c.currententity->trivial_accept);
        R_AliasSetupLighting(plighting);
        R_AliasSetupFrame();

        if (r_bsp_c.currententity->colormap == null)
        {
            sys_win_c.Sys_Error("R_AliasDrawModel: r_bsp_c.currententity->colormap == null");
        }

        r_affinetridesc.drawtype = (r_bsp_c.currententity->trivial_accept == 3) && r_main_c.r_recursiveaffinetriangles ? 1 : 0;

        if (r_affinetridesc.drawtype != 0)
        {
            d_polyse_c.D_PolysetUpdateTables();
        }
        else
        {
#if id386
            D_Aff8Patch(r_bsp_c.currententity->colormap);
#endif
        }

        acolormap = r_bsp_c.currententity->colormap;

        if (r_bsp_c.currententity != &cl_main_c.cl.viewent)
        {
            ziscale = (float)0x8000 * (float)0x10000;
        }
        else
        {
            ziscale = (float)0x8000 * (float)0x10000 * 3.0f;
        }

        if (r_bsp_c.currententity->trivial_accept != 0)
        {
            R_AliasPrepareUnclippedPoints();
        }
        else
        {
            R_AliasPreparePoints();
        }
    }
}