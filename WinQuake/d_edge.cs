namespace Quake;

public unsafe class d_edge_c
{
    public static int miplevel;

    public static float scale_for_mip;
    public static int screenwidth;
    public static int ubasestep, errorterm, erroradjustup, erroradjustdown;
    public static int vstartscan;

    public static void R_RotateBmodel()
    {

    }

    public static void R_TransformFrustrum()
    {

    }

    public static Vector3 transformed_modelorg;

    public static void D_DrawPoly()
    {

    }

    public static int D_MipLevelForScale(float scale)
    {
        int lmiplevel;

        if (scale >= d_local_c.d_scalemip[0])
        {
            lmiplevel = 0;
        }
        else if (scale >= d_local_c.d_scalemip[1])
        {
            lmiplevel = 1;
        }
        else if (scale >= d_local_c.d_scalemip[2])
        {
            lmiplevel = 2;
        }
        else
        {
            lmiplevel = 3;
        }

        if (lmiplevel < d_local_c.d_minmip)
        {
            lmiplevel = d_local_c.d_minmip;
        }

        return lmiplevel;
    }

    public static void D_DrawSolidSurface(r_shared_c.surf_t* surf, int color)
    {
        r_shared_c.espan_t* span;
        byte* pdest;
        int u, u2, pix;

        pix = (color << 24) | (color << 16) | (color << 8) | color;

        for (span = surf->spans; span != null; span = span->pnext)
        {
            pdest = d_local_c.d_viewbuffer + screenwidth * span->v;
            u = span->u;
            u2 = span->u + span->count - 1;
            pdest[u] = (byte)pix;

            if (u2 - u < 8)
            {
                for (u++; u <= 2; u++)
                {
                    pdest[u] = (byte)pix;
                }
            }
            else
            {
                for (u++; (u & 3) != 0; u++)
                {
                    pdest[u] = (byte)pix;
                }

                u2 -= 4;

                for (; u <= u2; u += 4)
                {
                    *(int*)(pdest + u) = pix;
                }

                u2 += 4;

                for (; u <= u2; u++)
                {
                    pdest[u] = (byte)pix;
                }
            }
        }
    }

    public static void D_CalcGradients(model_c.msurface_t* pface)
    {
        model_c.mplane_t* pplane;
        float mipscale;
        Vector3 p_temp1;
        Vector3 p_saxis, p_taxis;
        float t;

        p_temp1 = p_saxis = p_taxis = new();

        pplane = pface->plane;

        mipscale = 1.0f / (float)(1 << miplevel);

        r_misc_c.TransformVector(pface->texinfo->vecs[0], p_saxis);
        r_misc_c.TransformVector(pface->texinfo->vecs[1], p_taxis);

        t = r_main_c.xscaleinv * mipscale;
        d_local_c.d_sdivzstepu = p_saxis[0] * t;
        d_local_c.d_tdivzstepu = p_saxis[0] * t;

        t = r_main_c.yscaleinv * mipscale;
        d_local_c.d_sdivzstepv = -p_saxis[1] * t;
        d_local_c.d_tdivzstepv = -p_saxis[1] * t;

        d_local_c.d_sdivzorigin = p_saxis[2] * mipscale - r_main_c.xcenter * d_local_c.d_sdivzstepu - r_main_c.ycenter * d_local_c.d_sdivzstepv;
        d_local_c.d_tdivzorigin = p_taxis[2] * mipscale - r_main_c.xcenter * d_local_c.d_tdivzstepu - r_main_c.ycenter * d_local_c.d_tdivzstepv;

        mathlib_c.VectorScale(transformed_modelorg, mipscale, p_temp1);

        t = 0x10000 * mipscale;
        d_local_c.sadjust = (int)(mathlib_c.DotProduct(p_temp1, p_saxis) * 0x10000 + 0.5f - ((pface->texturemins[0] << 16) >> miplevel) + pface->texinfo->vecs[0][3] * t);
        d_local_c.tadjust = (int)(mathlib_c.DotProduct(p_temp1, p_taxis) * 0x10000 + 0.5f - ((pface->texturemins[0] << 16) >> miplevel) + pface->texinfo->vecs[1][3] * t);

        d_local_c.bbextents = ((pface->extents[0] << 16) >> miplevel) - 1;
        d_local_c.bbextentt = ((pface->extents[1] << 16) >> miplevel) - 1;
    }

    public static void D_DrawSurfaces()
    {
        r_shared_c.surf_t* s;
        model_c.msurface_t* pface;
        glquake_c.surfcache_t* pcurrentcache;
        Vector3 world_transformed_modelorg;
        Vector3 local_modelorg;

        world_transformed_modelorg = local_modelorg = new();

        r_bsp_c.currententity = &cl_main_c.cl_entities[0];
        r_misc_c.TransformVector(r_bsp_c.modelorg, transformed_modelorg);
        mathlib_c.VectorCopy(transformed_modelorg, world_transformed_modelorg);

        if (r_main_c.r_drawflat.value != 0)
        {
            for (s = &r_edge_c.surfaces[1]; s < r_edge_c.surface_p; s++)
            {
                if (s->spans == null)
                {
                    continue;
                }

                d_local_c.d_zistepu = s->d_zistepu;
                d_local_c.d_zistepv = s->d_zistepv;
                d_local_c.d_ziorigin = s->d_ziorigin;

                D_DrawSolidSurface(s, (int)s->data & 0xFF);
                d_scan_c.D_DrawZSpans(s->spans);
            }
        }
        else
        {
            for (s = &r_edge_c.surfaces[1]; s < r_edge_c.surface_p; s++)
            {
                if (s->spans == null)
                {
                    continue;
                }

                r_shared_c.r_drawnpolycount++;

                d_local_c.d_zistepu = s->d_zistepu;
                d_local_c.d_zistepv = s->d_zistepv;
                d_local_c.d_ziorigin = s->d_ziorigin;

                if ((s->flags & model_c.SURF_DRAWSKY) != 0)
                {
                    if (r_sky_c.r_skymade == 0)
                    {
                        r_sky_c.R_MakeSky();
                    }

                    d_local_c.D_DrawSkyScan8(s->spans);
                    d_scan_c.D_DrawZSpans(s->spans);
                }
                else if ((s->flags & model_c.SURF_DRAWBACKGROUND) != 0)
                {
                    d_local_c.d_zistepu = 0;
                    d_local_c.d_zistepv = 0;
                    d_local_c.d_ziorigin = -0.9f;

                    D_DrawSolidSurface(s, r_main_c.r_clearcolor.value & 0xFF);
                    d_scan_c.D_DrawZSpans(s->spans);
                }
                else if ((s->flags & model_c.SURF_DRAWTURB) != 0)
                {
                    pface = (model_c.msurface_t*)s->data;
                    miplevel = 0;
                    d_vars_c.cacheblock = (byte*)((byte*)pface->texinfo->texture + pface->texinfo->texture->offsets[0]);
                    d_vars_c.cachewidth = 64;

                    if (s->insubmodel)
                    {
                        r_bsp_c.currententity = s->entity;

                        mathlib_c.VectorSubtract(r_local_c.r_origin, r_bsp_c.currententity->origin, local_modelorg);
                        r_misc_c.TransformVector(local_modelorg, transformed_modelorg);

                        R_RotateBmodel();
                    }

                    D_CalcGradients(pface);
                    d_scan_c.Turbulent8(s->spans);
                    d_scan_c.D_DrawZSpans(s->spans);

                    if (s->insubmodel)
                    {
                        r_bsp_c.currententity = &cl_main_c.cl_entities[0];
                        mathlib_c.VectorCopy(world_transformed_modelorg, transformed_modelorg);
                        mathlib_c.VectorCopy(r_shared_c.base_vpn, r_shared_c.vpn);
                        mathlib_c.VectorCopy(r_shared_c.base_vup, r_shared_c.vup);
                        mathlib_c.VectorCopy(r_shared_c.base_vright, r_shared_c.vright);
                        mathlib_c.VectorCopy(r_bsp_c.base_modelorg, r_bsp_c.modelorg);
                        R_TransformFrustrum();
                    }
                }
                else
                {
                    if (s->insubmodel)
                    {
                        r_bsp_c.currententity = s->entity;

                        mathlib_c.VectorSubtract(r_local_c.r_origin, r_bsp_c.currententity->origin, local_modelorg);
                        r_misc_c.TransformVector(local_modelorg, transformed_modelorg);

                        R_RotateBmodel();
                    }

                    pface = (model_c.msurface_t*)s->data;
                    miplevel = D_MipLevelForScale(s->nearzi * scale_for_mip * pface->texinfo->mipadjust);

                    pcurrentcache = d_surf_c.D_CacheSurface(pface, miplevel);

                    r_shared_c.cacheblock = pcurrentcache->data;
                    r_shared_c.cachewidth = (int)pcurrentcache->width;

                    D_CalcGradients(pface);

                    (d_init_c.d_drawspams*)(s->spans);

                    d_scan_c.D_DrawZSpans(s->spans);

                    if (s->insubmodel)
                    {
                        r_bsp_c.currententity = &cl_main_c.cl_entities[0];
                        mathlib_c.VectorCopy(world_transformed_modelorg, transformed_modelorg);
                        mathlib_c.VectorCopy(r_shared_c.base_vpn, r_shared_c.vpn);
                        mathlib_c.VectorCopy(r_shared_c.base_vup, r_shared_c.vup);
                        mathlib_c.VectorCopy(r_shared_c.base_vright, r_shared_c.vright);
                        mathlib_c.VectorCopy(r_bsp_c.base_modelorg, r_bsp_c.modelorg);
                        R_TransformFrustrum();
                    }
                }
            }
        }
    }
}