namespace Quake;

public unsafe class r_misc_c
{
    public static void R_CheckVariables()
    {
        float oldbright = 0.0f;

        if (r_main_c.r_fullbright.value != oldbright)
        {
            oldbright = r_main_c.r_fullbright.value;
            d_surf_c.D_FlushCaches();
        }
    }

    public static void Show()
    {
        vid_c.vrect_t vr;

        vr.x = vr.y = 0;
        vr.width = (int)vid_c.vid.width;
        vr.height = (int)vid_c.vid.height;
        vr.pnext = null;
        vid_win_c.VID_Update(&vr);
    }

    public static void R_TimeRefresh_f()
    {
        int i;
        float start, stop, time;
        int startangle;
        vid_c.vrect_t vr;

        startangle = (int)r_main_c.r_refdef.viewangles[1];

        start = (float)sys_win_c.Sys_FloatTime();

        for (i = 0; i < 128; i++)
        {
            r_main_c.r_refdef.viewangles[1] = i / 128.0f * 360.0f;

            vid_win_c.VID_LockBuffer();

            r_main_c.R_RenderView();

            vid_win_c.VID_UnlockBuffer();

            vr.x = r_main_c.r_refdef.vrect.x;
            vr.y = r_main_c.r_refdef.vrect.y;
            vr.width = r_main_c.r_refdef.vrect.width;
            vr.height = r_main_c.r_refdef.vrect.height;
            vr.pnext = null;
            vid_win_c.VID_Update(&vr);
        }

        stop = (float)sys_win_c.Sys_FloatTime();
        time = stop - start;
        console_c.Con_Printf($"{time} seconds ({128 / time} fps)\n");

        r_main_c.r_refdef.viewangles[1] = startangle;
    }

    public static void R_LineGraph(int x, int y, int h)
    {
        int i;
        byte* dest;
        int s;

        x += r_main_c.r_refdef.vrect.x;
        y += r_main_c.r_refdef.vrect.y;

        dest = vid_c.vid.buffer + vid_c.vid.rowbytes * y + x;

        s = r_main_c.r_graphheight.value;

        if (h > s)
        {
            h = s;
        }

        for (i = 0; i < h; i++, dest -= vid_c.vid.rowbytes * 2)
        {
            dest[0] = 0xff;
            *(dest - vid_c.vid.rowbytes) = 0x30;
        }

        for (; i < s; i++, dest -= vid_c.vid.rowbytes * 2)
        {
            dest[0] = 0x30;
            *(dest - vid_c.vid.rowbytes) = 0x30;
        }
    }

    public const int MAX_TIMINGS = 100;
    public static float mouse_x, mouse_y;

    public static void R_TimeGraph()
    {
        int timex;
        int a;
        float r_time2;
        byte* r_timings;
        int x;

        timex = a = 0;
        r_timings = null;

        r_time2 = (float)sys_win_c.Sys_FloatTime();

        a = (int)((r_time2 - r_main_c.r_time1) / 0.01f);

        r_timings[timex] = (byte)a;
        a = timex;

        if (r_main_c.r_refdef.vrect.width <= MAX_TIMINGS)
        {
            x = r_main_c.r_refdef.vrect.width - 1;
        }
        else
        {
            x = r_main_c.r_refdef.vrect.width - (r_main_c.r_refdef.vrect.width - MAX_TIMINGS) / 2;
        }

        do
        {
            R_LineGraph(x, r_main_c.r_refdef.vrect.height - 2, r_timings[a]);

            if (x == 0)
            {
                break;
            }

            x--;
            a--;

            if (a == -1)
            {
                a = MAX_TIMINGS - 1;
            }
        } while (a != timex);

        timex = (timex + 1) % MAX_TIMINGS;
    }

    public static void R_PrintTimes()
    {
        float r_time2;
        float ms;

        r_time2 = (float)sys_win_c.Sys_FloatTime();

        ms = 1000 * (r_time2 - r_main_c.r_time1);

        console_c.Con_Printf($"{ms} ms {r_draw_c.c_faceclip}/{r_main_c.r_polycount}/{r_main_c.r_drawnpolycount} poly {r_main_c.c_surf} surf\n");
        r_main_c.c_surf = 0;
    }

    public static void R_PrintDSpeeds()
    {
        float ms, dp_time, r_time2, rw_time, db_time, se_time, de_time, dv_time;

        r_time2 = (float)sys_win_c.Sys_FloatTime();

        dp_time = (r_main_c.dp_time2 - r_main_c.dp_time1) * 1000;
        rw_time = (r_main_c.rw_time2 - r_main_c.rw_time1) * 1000;
        db_time = (r_main_c.db_time2 - r_main_c.db_time1) * 1000;
        se_time = (r_main_c.se_time2 - r_main_c.se_time2) * 1000;
        de_time = (r_main_c.de_time2 - r_main_c.de_time2) * 1000;
        dv_time = (r_main_c.dv_time2 - r_main_c.dv_time2) * 1000;
        ms = (r_time2 - r_main_c.r_time1) * 1000;

        console_c.Con_Printf($"{(int)ms} {dp_time} {(int)rw_time} {db_time} {(int)se_time} {de_time} {dv_time}\n");
    }

    public static void R_PrintAliasStats()
    {
        console_c.Con_Printf($"{r_alias_c.r_amodels_drawn} polygon model drawn\n");
    }

    public static void WarpPalette()
    {
        int i, j;
        byte* newpalette;
        int* basecolor;

        newpalette = null;
        basecolor = null;

        basecolor[0] = 130;
        basecolor[1] = 80;
        basecolor[2] = 50;

        for (i = 0; i < 256; i++)
        {
            for (j = 0; j < 3; j++)
            {
                newpalette[i * 3 + j] = (byte)((host_c.host_basepal[i * 3 + j] + basecolor[j]) / 2);
            }
        }

        vid_win_c.VID_ShiftPalette(newpalette);
    }

    public static void R_TransformFrustrum()
    {
        int i;
        Vector3 v, v2;

        v = v2 = new();

        for (i = 0; i < 4; i++)
        {
            v[0] = r_main_c.screenedge[i].normal[2];
            v[1] = -r_main_c.screenedge[i].normal[0];
            v[2] = r_main_c.screenedge[i].normal[1];

            v2[0] = v[1] * r_shared_c.vright[0] + v[2] * r_shared_c.vup[0] + v[0] * r_shared_c.vpn[0];
            v2[1] = v[1] * r_shared_c.vright[1] + v[2] * r_shared_c.vup[1] + v[0] * r_shared_c.vpn[1];
            v2[2] = v[1] * r_shared_c.vright[2] + v[2] * r_shared_c.vup[2] + v[0] * r_shared_c.vpn[2];

            mathlib_c.VectorCopy(v2, r_draw_c.view_clipplanes[i].normal);


            r_draw_c.view_clipplanes[i].dist = mathlib_c.DotProduct(r_bsp_c.modelorg, v2);
        }
    }

#if !id386
    public static void TransformVector(Vector3 input, Vector3 output)
    {
        output[0] = mathlib_c.DotProduct(input, r_shared_c.vright);
        output[1] = mathlib_c.DotProduct(input, r_shared_c.vup);
        output[2] = mathlib_c.DotProduct(input, r_shared_c.vpn);
    }

    public static void TransformVector(Vector3 input, float* output)
    {
        output[0] = mathlib_c.DotProduct(input, r_shared_c.vright);
        output[1] = mathlib_c.DotProduct(input, r_shared_c.vup);
        output[2] = mathlib_c.DotProduct(input, r_shared_c.vpn);
    }
#endif

    public static void R_TransformPlane(model_c.mplane_t* p, float* normal, float* dist)
    {
        float d;

        d = mathlib_c.DotProduct(r_main_c.r_origin, p->normal);
        *dist = p->dist - d;

        TransformVector(p->normal, normal);
    }

    public static void R_SetUpFrustrumIndexes()
    {
        int i, j;
        int* pindex;

        pindex = r_main_c.r_frustrum_indexes;

        for (i = 0; i < 4; i++)
        {
            for (j = 0; j < 3; j++)
            {
                if (r_draw_c.view_clipplanes[i].normal[j] < 0)
                {
                    pindex[j] = j;
                    pindex[j + 3] = j +3;
                }
                else
                {
                    pindex[j] = j + 3;
                    pindex[j + 3] = j;
                }
            }

            r_main_c.pfrustum_indexes[i] = *pindex;
            pindex += 6;
        }
    }

    public static void R_SetupFrame()
    {
        int edgecount;
        vid_c.vrect_t vrect;
        float w, h;

        if (cl_main_c.cl.maxclients > 1)
        {
            cvar_c.Cvar_Set("r_draworder", "0");
            cvar_c.Cvar_Set("r_fullbright", "0");
            cvar_c.Cvar_Set("r_ambient", "0");
            cvar_c.Cvar_Set("r_drawflat", "0");
        }

        if (r_main_c.r_numsurfs.value != 0)
        {
            if ((r_edge_c.surface_p - r_edge_c.surfaces) > r_main_c.r_maxsurfsseen)
            {
                r_main_c.r_maxsurfsseen = (int)(r_edge_c.surface_p - r_edge_c.surfaces);
            }

            console_c.Con_Printf($"Used {r_edge_c.surface_p - r_edge_c.surfaces} of {r_edge_c.surf_max - r_edge_c.surfaces}; {r_main_c.r_maxsurfsseen} max\n");
        }

        if (r_main_c.r_numedges.value != 0)
        {
            edgecount = (int)(r_edge_c.edge_p - r_edge_c.r_edges);

            if (edgecount > r_main_c.r_maxedgesseen)
            {
                r_main_c.r_maxedgesseen = edgecount;
            }

            console_c.Con_Printf($"Used {edgecount} of {r_main_c.r_numallocatededges} edges; {r_main_c.r_maxedgesseen} max\n");
        }

        r_main_c.r_refdef.ambientlight = r_main_c.r_ambient.value;

        if (r_main_c.r_refdef.ambientlight < 0)
        {
            r_main_c.r_refdef.ambientlight = 0;
        }

        if (!server_c.sv.active)
        {
            r_main_c.r_draworder.value = 0;
        }

        R_CheckVariables();

        r_light_c.R_AnimateLight();

        r_main_c.r_framecount++;

        r_main_c.numbtofpolys = 0;

        mathlib_c.VectorCopy(r_main_c.r_refdef.vieworg, r_bsp_c.modelorg);
        mathlib_c.VectorCopy(r_main_c.r_refdef.vieworg, r_main_c.r_origin);

        mathlib_c.AngleVectors(r_main_c.r_refdef.viewangles, r_main_c.vpn, r_main_c.vright, r_main_c.vup);

        r_main_c.r_oldviewleaf = r_main_c.r_viewleaf;
        r_main_c.r_viewleaf = model_c.Mod_PointInLeaf(r_main_c.r_origin, cl_main_c.cl.worldmodel);

        r_main_c.r_dowarpold = r_main_c.r_dowarp;
        r_main_c.r_dowarp = r_main_c.r_waterwarp.value != 0 && (r_main_c.r_viewleaf->contents <= bspfile_c.CONTENTS_SOLID);

        if ((r_main_c.r_dowarp != r_main_c.r_dowarpold) || r_main_c.r_viewchanged || view_c.lcd_x.value != null)
        {
            if (r_main_c.r_dowarp)
            {
                if ((vid_c.vid.width <= vid_c.vid.maxwarpwidth) && (vid_c.vid.height <= vid_c.vid.maxwarpheight))
                {
                    vrect.x = 0;
                    vrect.y = 0;
                    vrect.width = (int)vid_c.vid.width;
                    vrect.height = (int)vid_c.vid.height;

                    r_main_c.R_ViewChanged(&vrect, sbar_c.sb_lines, vid_c.vid.aspect);
                }
                else
                {
                    w = vid_c.vid.width;
                    h = vid_c.vid.height;

                    if (w > vid_c.vid.maxwarpwidth)
                    {
                        h *= vid_c.vid.maxwarpwidth / w;
                        w = vid_c.vid.maxwarpwidth;
                    }

                    if (h > vid_c.vid.maxwarpheight)
                    {
                        h = vid_c.vid.maxwarpheight;
                        w *= vid_c.vid.maxwarpheight / h;
                    }

                    vrect.x = 0;
                    vrect.y = 0;
                    vrect.width = (int)w;
                    vrect.height = (int)h;

                    r_main_c.R_ViewChanged(&vrect, (int)(sbar_c.sb_lines * (h / vid_c.vid.height)), vid_c.vid.aspect * (h / w) * (vid_c.vid.width / vid_c.vid.height));
                }
            }
            else
            {
                vrect.x = 0;
                vrect.y = 0;
                vrect.width = (int)vid_c.vid.width;
                vrect.height = (int)vid_c.vid.height;

                r_main_c.R_ViewChanged(&vrect, sbar_c.sb_lines, vid_c.vid.aspect);
            }

            r_main_c.r_viewchanged = false;
        }

        R_TransformFrustrum();

        mathlib_c.VectorCopy(r_main_c.vpn, r_main_c.base_vpn);
        mathlib_c.VectorCopy(r_main_c.vright, r_main_c.base_vright);
        mathlib_c.VectorCopy(r_main_c.vup, r_main_c.base_vup);
        mathlib_c.VectorCopy(r_bsp_c.modelorg, r_bsp_c.base_modelorg);

        r_sky_c.R_SetSkyFrame();

        R_SetUpFrustrumIndexes();

        gl_rmain_c.r_cache_thrash = false;

        r_draw_c.c_faceclip = 0;
        r_main_c.d_spanpixcount = 0;
        r_main_c.r_polycount = 0;
        r_main_c.r_drawnpolycount = 0;
        r_main_c.r_wholepolycount = 0;
        r_alias_c.r_amodels_drawn = 0;
        r_main_c.r_outofsurfaces = 0;
        r_main_c.r_outofedges = 0;

        d_init_c.D_SetupFrame();
    }
}