namespace Quake;

public unsafe class r_main_c
{
    public void* colormap;
    public static Vector3 viewlightvec;
    public alight_t r_viewlighting = new alight_t { 128, 192, viewlightvec };
    public float r_time1;
    public static int r_numallocatededges;
    public static bool r_drawpolys;
    public static bool r_drawculledpolys;
    public static bool r_worldpolysbacktofront;
    public bool r_recursiveaffinetriangles = true;
    public static int r_pixbytes = 1;
    public static float r_aliasuvscale = 1.0f;
    public static int r_outofsurfaces;
    public static int r_outofedges;

    public static bool r_dowarp, r_dowarpold, r_viewchanged;

    public static int numbtofpolys;
    public static btofpoly_t* pbtofpolys;
    public static mvertex_t* r_pcurrentvertbase;

    public int c_surf;
    public int r_maxsurfsseen, r_maxedgesseen, r_cnumsurfs;
    public bool r_surfsonstack;
    public static int r_clipflags;

    public byte* r_warpbuffer;

    public byte* r_stack_start;

    public bool r_fov_greater_than_90;

    public Vector3 vup, base_vup;
    public Vector3 vpn, base_vpn;
    public Vector3 vright, base_vright;
    public static Vector3 r_origin;

    public static render_c.refdef_t r_refdef;
    public static float xcenter, ycenter;
    public static float xscale, yscale;
    public static float xscaleinv, yscaleinv;
    public static float xscaleshrink, yscaleshrink;
    public static float aliasxscale, aliasyscale, aliasxcenter, aliasycenter;

    public static int screenwidth;

    public static float pixelAspect;
    public static float screenAspect;
    public static float verticalFieldOfView;
    public static float xOrigin, yOrigin;

    public static mplane_t[] screenedge = new mplane_t[4];

    public static int r_framecount = 1;
    public static int r_visframecount;
    public static int d_spanpixcount;
    public static int r_polycount;
    public static int r_drawnpolycount;
    public static int r_wholepolycount;

    public const int VIDEOMODNAME_LENGTH = 256;
    public char[] viewmodname = new char[VIDEOMODNAME_LENGTH];
    public static int modcount;

    public static int* pfrustum_indexes;
    public static int[] r_frustrum_indexes = new int[4 * 6];

    public static int reinit_surfcache = 1;

    public static mleaf_t* r_viewleaf, r_oldviewleaf;

    public static model_c.texture_t* r_notexture_mip;

    public static float r_aliastransition, r_resfudge;

    public static int[] d_lightstylevalue = new int[256];

    public static float dp_time1, dp_time2, db_time1, db_time2, rw_time1, rw_time2;
    public static float se_time1, se_time2, de_time1, de_time2, dv_time1, dv_time2;

    public static cvar_c.cvar_t r_draworder = new cvar_c.cvar_t { name = "r_draworder", value = 0 };
    public cvar_c.cvar_t r_speeds = new cvar_c.cvar_t { name = "r_speeds", value = 0 };
    public cvar_c.cvar_t r_timegraph = new cvar_c.cvar_t { name = "r_timegraph", value = 0 };
    public cvar_c.cvar_t r_graphheight = new cvar_c.cvar_t { name = "r_graphheight", value = 10 };
    public cvar_c.cvar_t r_clearcolor = new cvar_c.cvar_t { name = "r_clearcolor", value = 2 };
    public cvar_c.cvar_t r_waterwarp = new cvar_c.cvar_t { name = "r_waterwarp", value = 1 };
    public cvar_c.cvar_t r_fullbright = new cvar_c.cvar_t { name = "r_fullbright", value = 0 };
    public cvar_c.cvar_t r_drawentities = new cvar_c.cvar_t { name = "r_drawentities", value = 1 };
    public cvar_c.cvar_t r_drawviewmodel = new cvar_c.cvar_t { name = "r_drawviewmodel", value = 1 };
    public cvar_c.cvar_t r_aliasstats = new cvar_c.cvar_t { name = "r_polymodelstats", value = 0 };
    public cvar_c.cvar_t r_dspeeds = new cvar_c.cvar_t { name = "r_dspeeds", value = 0};
    public cvar_c.cvar_t r_drawflat = new cvar_c.cvar_t { name = "r_drawflat", value = 0 };
    public cvar_c.cvar_t r_ambient = new cvar_c.cvar_t { name = "r_ambient", value = 0 };
    public cvar_c.cvar_t r_reportsurfout = new cvar_c.cvar_t { name = "r_reportsurfout", value = 0 };
    public cvar_c.cvar_t r_maxsurfs = new cvar_c.cvar_t { name = "r_maxsurfs", value = 0 };
    public cvar_c.cvar_t r_numsurfs = new cvar_c.cvar_t { name = "r_numsurfs", value = 0 };
    public cvar_c.cvar_t r_reportedgeout = new cvar_c.cvar_t { name = "r_reportedgeout", value = 0 };
    public cvar_c.cvar_t r_maxedges = new cvar_c.cvar_t { name = "r_maxedges", value = 0 };
    public cvar_c.cvar_t r_numedges = new cvar_c.cvar_t { name = "r_numedges", value = 0 };
    public cvar_c.cvar_t r_aliastransbase = new cvar_c.cvar_t { name = "r_aliastransbase", value = 200 };
    public cvar_c.cvar_t r_aliastransadj = new cvar_c.cvar_t { name = "r_aliastransadj", value = 100 };

    public cvar_c.cvar_t scr_fov;

    public void CreatePassages() { }
    public void SetVisibilityByPassages() { }

    public static void R_InitTextures()
    {
        int x, y, m;
        byte* dest;

        r_notexture_mip = (model_c.texture_t*)zone_c.Hunk_AllocName(sizeof(model_c.texture_t) + 16 * 16 + 8 * 8 + 4 * 4 + 2 * 2, "notexture");

        r_notexture_mip->width = r_notexture_mip->height = 16;
        r_notexture_mip->offsets[0] = (uint)sizeof(model_c.texture_t);
        r_notexture_mip->offsets[1] = r_notexture_mip->offsets[0] + 16 * 16;
        r_notexture_mip->offsets[2] = r_notexture_mip->offsets[1] + 8 * 8;
        r_notexture_mip->offsets[3] = r_notexture_mip->offsets[2] + 4 * 4;

        for (m = 0; m < 4; m++)
        {
            dest = (byte*)r_notexture_mip + r_notexture_mip->offsets[m];

            for (y = 0; y < (16 >> m); y++)
            {
                for (x = 0; x < (16 >> m); x++)
                {
                    if ((y < (8 >> m)) ^ (x < (8 >> m)))
                    {
                        *dest++ = 0;
                    }
                    else
                    {
                        *dest++ = 0xff;
                    }
                }
            }
        }
    }

    public static void R_Init()
    {
        int dummy;

        r_stack_start = (byte*)&dummy;

        R_InitTurb();

        cmd_c.Cmd_AddCommand("timerefresh", R_TimeRefresh_f);
        cmd_c.Cmd_AddCommand("pointfile", R_ReadPointFile_f);

        cvar_c.Cvar_RegisterVariable(r_draworder);
        cvar_c.Cvar_RegisterVariable(r_speeds);
        cvar_c.Cvar_RegisterVariable(r_timegraph);
        cvar_c.Cvar_RegisterVariable(r_graphheight);
        cvar_c.Cvar_RegisterVariable(r_drawflat);
        cvar_c.Cvar_RegisterVariable(r_ambient);
        cvar_c.Cvar_RegisterVariable(r_clearcolor);
        cvar_c.Cvar_RegisterVariable(r_waterwarp);
        cvar_c.Cvar_RegisterVariable(r_fullbright);
        cvar_c.Cvar_RegisterVariable(r_drawentities);
        cvar_c.Cvar_RegisterVariable(r_drawviewmodel);
        cvar_c.Cvar_RegisterVariable(r_aliasstats);
        cvar_c.Cvar_RegisterVariable(r_dspeeds);
        cvar_c.Cvar_RegisterVariable(r_reportsurfout);
        cvar_c.Cvar_RegisterVariable(r_maxsurfs);
        cvar_c.Cvar_RegisterVariable(r_numsurfs);
        cvar_c.Cvar_RegisterVariable(r_reportedgeout);
        cvar_c.Cvar_RegisterVariable(r_maxedges);
        cvar_c.Cvar_RegisterVariable(r_numedges);
        cvar_c.Cvar_RegisterVariable(r_aliastransbase);
        cvar_c.Cvar_RegisterVariable(r_aliastransadj);

        cvar_c.Cvar_SetValue("r_maxedges", (float)NUMSTACKEDGES);
        cvar_c.Cvar_SetValue("r_maxsurfs", (float)NUMSTACKSURFACES);

        view_clipplanes[0].leftedge = true;
        view_clipplanes[1].rightedge = true;
        view_clipplanes[1].leftedge = view_clipplanes[2].leftedge = view_clipplanes[3].leftedge = false;
        view_clipplanes[0].rightedge = view_clipplanes[2].rightedge = view_clipplanes[3].rightedge = false;

        r_refdef.xOrigin = XCENTERING;
        r_refdef.yOrigin = YCENTERING;

        R_InitParticles();

#if id386
        sys_win_c.Sys_MakeCodeWriteable((long)R_EdgeCodeStart, (long)R_EdgeCodeEnd - (long)R_EdgeCodeStart);
#endif

        D_Init();
    }

    public void R_NewMap()
    {
        int i;

        for (i = 0; i < cl.worldmodel->numleafs; i++)
        {
            cl.worldmodel->leafs[i].efrags = null;
        }

        r_viewleaf = null;
        R_ClearParticles();

        r_cnumsurfs = r_maxsurfs.value;

        if (r_cnumsurfs <= MINSURFACES)
        {
            r_cnumsurfs = MINSURFACES;
        }

        if (r_cnumsurfs > NUMSTACKSURFACES)
        {
            surfaces = zone_c.Hunk_AllocName(r_cnumsurfs * sizeof(r_shared_c.surf_t), "surfaces");
            surface_p = surfaces;
            surf_max = &surfaces[r_cnumsurfs];
            r_surfsonstack = false;

            surfaces--;
            R_SurfacePatch();
        }
        else
        {
            r_surfsonstack = true;
        }

        r_maxedgesseen = 0;
        r_maxsurfsseen = 0;

        r_numallocatededges = r_maxedges.value;

        if (r_numallocatededges < MINEDGES)
        {
            r_numallocatededges = MINEDGES;
        }

        if (r_numallocatededges <= NUMSTACKEDGES)
        {
            auxedges = null;
        }
        else
        {
            auxedges = zone_c.Hunk_AllocName(r_numallocatededges * sizeof(edge_t), "edges");
        }

        r_dowarp = false;
        r_viewchanged = false;
#if PASSAGES
        CreatePassages();
#endif
    }

    public static void R_SetVrect(vid_c.vrect_t* pvrectin, vid_c.vrect_t* pvrect, int lineadj)
    {
        int h;
        float size;

        size = screen_c.scr_viewsize.value > 100 ? 100 : screen_c.scr_viewsize.value;

        if (cl.intermission)
        {
            size = 100;
            lineadj = 0;
        }

        size /= 100;

        h = pvrectin->height - lineadj;
        pvrect->width = (int)(pvrectin->width * size);

        if (pvrect->width < 96)
        {
            size = 96.0f / pvrectin->width;
            pvrect->width = 96;
        }

        pvrect->width &= ~7;
        pvrect->height = (int)(pvrectin->height * size);

        if (pvrect->height > pvrectin->height - lineadj)
        {
            pvrect->height = pvrectin->height - lineadj;
        }

        pvrect->height &= ~1;

        pvrect->x = (pvrectin->width - pvrect->width) / 2;
        pvrect->y = (h - pvrect->height) / 2;

        {
            if (lcd_x.value != 0)
            {
                pvrect->y >>= 1;
                pvrect->height >>= 1;
            }
        }
    }

    public static void R_ViewChanged(vid_c.vrect_t* pvrect, int lineadj, float aspect)
    {
        int i;
        float res_scale;

        r_viewchanged = true;

        R_SetVrect(pvrect, render_c.r_refdef.vrect, lineadj);

        r_refdef.horizontalFieldOfView = 2.0f * MathF.Tan(r_refdef.fov_x / 360 * MathF.PI);
        r_refdef.fvrectx = (float)r_refdef.vrect.x;
        r_refdef.fvrectx_adj = (float)r_refdef.vrect.x;
        r_refdef.fvrectx_adj_shift20 = (r_refdef.vrect.x << 20) + (1 << 19) - 1;
        r_refdef.fvrecty = (float)r_refdef.vrect.y;
        r_refdef.fvrecty_adj = (float)r_refdef.vrect.y;
        r_refdef.vrectright = r_refdef.vrect.x + r_refdef.vrect.width;
        r_refdef.vrectright_adj_shift20 = (r_refdef.vrectright << 20) + (1 << 19) - 1;
        r_refdef.fvrectright = (float)r_refdef.vrectright;
        r_refdef.fvrectright_adj = (float)r_refdef.vrectright - 0.5f;
        r_refdef.vrectrightedge = (float)r_refdef.vrectright - 0.99f;
        r_refdef.vrectbottom = r_refdef.vrect.y + r_refdef.vrect.height;
        r_refdef.fvrectbottom = (float)r_refdef.vrectbottom;
        r_refdef.fvrectbottom_adj = (float)r_refdef.vrectbottom - 0.5f;

        r_refdef.aliasvrect.x = (int)(r_refdef.vrect.x * r_aliasuvscale);
        r_refdef.aliasvrect.y = (int)(r_refdef.vrect.y * r_aliasuvscale);
        r_refdef.aliasvrect.width = (int)(r_refdef.vrect.width * r_aliasuvscale);
        r_refdef.aliasvrect.height = (int)(r_refdef.vrect.height * r_aliasuvscale);
        r_refdef.aliasvrectright = r_refdef.aliasvrect.x + r_refdef.aliasvrect.width;
        r_refdef.aliasvrectbottom = r_refdef.aliasvrect.y + r_refdef.aliasvrect.height;

        pixelAspect = aspect;
        xOrigin = r_refdef.xOrigin;
        yOrigin = r_refdef.yOrigin;

        screenAspect = r_refdef.vrect.width * pixelAspect / r_refdef.vrect.height;

        verticalFieldOfView = r_refdef.horizontalFieldOfView / screenAspect;

        xcenter = ((float)r_refdef.vrect.width * XCENTERING) + r_refdef.vrect.x - 0.5f;
        aliasxcenter = xcenter * r_aliasuvscale;
        ycenter = ((float)r_refdef.vrect.height * YCENTERING) + r_refdef.vrect.y - 0.5f;
        aliasycenter = ycenter * r_aliasuvscale;

        xscale = r_refdef.vrect.width / r_refdef.horizontalFieldOfView;
        aliasxscale = xscale * r_aliasuvscale;
        xscaleinv = 1.0f / xscale;
        yscale = xscale * pixelAspect;
        aliasyscale = yscale * r_aliasuvscale;
        yscaleinv = 1.0f / yscale;
        xscaleshrink = (r_refdef.vrect.width - 6) / r_refdef.horizontalFieldOfView;
        yscaleshrink = xscaleshrink * pixelAspect;

        screenedge[0].normal[0] = -1.0f / (xOrigin * r_refdef.horizontalFieldOfView);
        screenedge[0].normal[1] = 0;
        screenedge[0].normal[2] = 1;
        screenedge[0].type = PLANE_ANYZ;

        screenedge[1].normal[0] = 1.0f / ((1.0f - xOrigin) * r_refdef.horizontalFieldOfView);
        screenedge[1].normal[1] = 0;
        screenedge[1].normal[2] = 1;
        screenedge[1].type = PLANE_ANYZ;

        screenedge[2].normal[0] = 0;
        screenedge[2].normal[1] = -1.0f / (yOrigin * verticalFieldOfView);
        screenedge[2].normal[2] = 1;
        screenedge[2].type = PLANE_ANYZ;

        screenedge[3].normal[0] = 0;
        screenedge[3].normal[1] = 1.0f / ((1.0f - yOrigin) * verticalFieldOfView);
        screenedge[3].normal[2] = 1;
        screenedge[3].type = PLANE_ANYZ;

        for (i = 0; i < 4; i++)
        {
            mathlib_c.VectorNormalize(screenedge[i].normal);
        }

        res_scale = mathlib_c.sqrt((double)(r_refdef.vrect.width * r_refdef.vrect.height) / (320.0f * 152.0f) * (2.0f / r_refdef.horizontalFieldOfView));
        r_aliastransition = r_aliastransbase.value * res_scale;
        r_resfudge = r_aliastransadj.value * res_scale;

        if (scr_fov.value <= 90.0f)
        {
            r_fov_greater_than_90 = false;
        }
        else
        {
            r_fov_greater_than_90 = true;
        }

#if id386
        if (r_pixbytes == 1) 
        {
            sys_win_c.Sys_MakeCodeWriteable((long)R_Surf8Start, (long)R_Surf8End - (long)R_Surf8Start);

            colormap = vid.colormap;
            R_Surf8Patch();
        }
        else 
        {
            sys_win_c.Sys_MakeCodeWriteable((long)R_Surf16Start, (long)R_Surf16End - (long)R_Surf16Start);

            colormap = vid.colormap;
            R_Surf16Patch();
        }
#endif

        render_c.D_ViewChanged();
    }

    public void R_MarkLeaves()
    {
        byte* vis;
        mnode_t* node;
        int i;

        if (r_oldviewleaf == r_viewleaf)
        {
            return;
        }

        r_visframecount++;
        r_oldviewleaf = r_viewleaf;

        vis = Mod_LeafPVS(r_viewleaf, cl.worldmodel);

        for (i = 0; i < cl.worldmodel->numleafs; i++)
        {
            if ((vis[i >> 3] & (1 << (i & 7))) != 0)
            {
                node = (mnode_t*)&cl.worldmodel->leafs[i + 1];

                do
                {
                    if (node->visframe == r_visframecount)
                    {
                        break;
                    }

                    node->visframe = r_visframecount;
                    node = node->parent;
                } while (node != null);
            }
        }
    }

    public void R_DrawEntitiesOnList()
    {
        int i, j;
        int lnum;
        alight_t lighting;
        float[] lightvec = { -1, 0, 0 };
        Vector3 dist;
        float add;

        if (r_drawentities.value == 0)
        {
            return;
        }

        for (i = 0; i < client_c.cl_numvisedicts; i++)
        {
            currententity = client_c.cl_visedicts[i];

            if (currententity == &client_c.cl_entities[cl.viewentity])
            {
                continue;
            }

            switch (currententity->model->type)
            {
                case mod_sprite:
                    Vector<r_entorigin>.CopyTo(currententity->origin);
                    Vector3.Subtract(r_origin, r_entorigin);
                    draw_c.R_DrawSprite();
                    break;

                case mod_alias:
                    Vector<r_entorigin>.CopyTo(currententity->origin);
                    Vector3.Subtract(r_origin, r_entorigin);

                    if (R_AliasCheckBBox())
                    {
                        j = R_LightPoint(currententity->origin);

                        lighting.ambientlight = j;
                        lighting.shadelight = j;

                        lighting.plightvec = lightvec;

                        for (lnum = 0; lnum < MAX_DLIGHTS; lnum++)
                        {
                            if (cl_dlights[lnum].die >= cl.time)
                            {
                                Vector3.Subtract(currententity->origin, cl_dlights[lnum].origin);
                                add = cl_dlights[lnum].radius - mathlib_c.Length(dist);

                                if (add > 0)
                                {
                                    lighting.ambientlight += add;
                                }
                            }
                        }

                        if (lighting.ambientlight > 128)
                        {
                            lighting.ambientlight = 128;
                        }

                        if (lighting.ambientlight + lighting.shadelight > 192)
                        {
                            lighting.shadelight = 192 - lighting.ambientlight;
                        }

                        R_AliasDrawModel(&lighting);
                    }

                    break;

                default:
                    break;
            }
        }
    }

    public void R_DrawViewModel()
    {
        float[] lightvec = { -1, 0, 0 };
        int j;
        int lnum;
        Vector3 dist;
        float add;
        client_c.dlight_t* dl;

        if (!r_drawviewmodel.value || r_fov_greater_than_90)
        {
            return;
        }

        if ((cl.items & quakedef_c.IT_INVISIBILITY) == 0) 
        {
            return;
        }

        if (cl.stats[STAT_HEALTH] <= 0)
        {
            return;
        }

        currententity = &cl.viewent;

        if (!currententity->model)
        {
            return;
        }

        Array.Copy(currententity->origin, r_entorigin);
        Vector3.Subtract(r_origin, r_entorigin);

        Array.Copy(vup, viewlightvec, (int)vup.Length());
    }
}