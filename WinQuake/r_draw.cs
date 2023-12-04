namespace Quake;

public unsafe class r_draw_c
{
    public static int MAXLEFTCLIPEDGES = 100;

    public static uint FULLY_CLIPPED_CACHED = 0x80000000;
    public static int FRAMECOUNT_MASK = 0x7FFFFFFF;

    public static uint cacheoffset;

    public static int c_faceclip;

    public d_iface_c.zpointdesc_t r_zpointdesc;

    public d_iface_c.polydesc_t r_polydesc;

    public r_local_c.clipplane_t* entity_clipplanes;
    public static r_local_c.clipplane_t[] view_clipplanes = new r_local_c.clipplane_t[4];
    public r_local_c.clipplane_t world_clipplanes;

    public static model_c.medge_t* r_pedge;

    public static bool r_leftclipped, r_rightclipped;
    public static bool makeleftedge, makerightedge;
    public static bool r_nearzionly;

    public int sintable;
    public int intsintable;

    public static model_c.mvertex_t r_leftenter, r_leftexit;
    public static model_c.mvertex_t r_rightenter, r_rightexit;

    public struct evert_t
    {
        public float u, v;
        public int ceilv;
    }

    public static int r_emitted;
    public static float r_nearzi;
    public static float r_u1, r_v1, r_lzi1;
    public static int r_ceilv1;

    public static bool r_lastvertvalid;

#if !id386

    public void R_EmitEdge(model_c.mvertex_t* pv0, model_c.mvertex_t* pv1)
    {
        r_shared_c.edge_t* edge, pcheck;
        int u_check;
        float u, u_step;
        Vector3 local = new Vector3(0f), transformed = new Vector3(0f);
        float* world = null;
        int v, v2, ceilv0;
        float scale, lzi0, u0, v0;
        int side;

        if (r_lastvertvalid)
        {
            u0 = r_u1;
            v0 = r_v1;
            lzi0 = r_lzi1;
            ceilv0 = r_ceilv1;
        }
        else
        {
            world = pv0->position[0];

            mathlib_c.VectorSubtract_FVV(world, r_bsp_c.modelorg, local);
            r_misc_c.TransformedVector(local, transformed);

            if (transformed[2] < r_local_c.NEAR_CLIP)
            {
                transformed[2] = r_local_c.NEAR_CLIP;
            }

            lzi0 = 1.0f / transformed[2];

            scale = r_main_c.xscale * lzi0;
            u0 = (r_main_c.xcenter + scale * transformed[0]);

            if (u0 < render_c.r_refdef.fvrectx_adj)
            {
                u0 = render_c.r_refdef.fvrectx_adj;
            }

            if (u0 > render_c.r_refdef.fvrectright_adj)
            {
                u0 = render_c.r_refdef.fvrectright_adj;
            }

            scale = r_main_c.yscale * lzi0;
            v0 = (r_main_c.ycenter - scale * transformed[1]);

            if (v0 < render_c.r_refdef.fvrecty_adj)
            {
                v0 = render_c.r_refdef.fvrecty_adj;
            }

            if (v0 > render_c.r_refdef.fvrectbottom_adj)
            {
                v0 = render_c.r_refdef.fvrectbottom_adj;
            }

            ceilv0 = (int)MathF.Ceiling(v0);
        }

        world = pv1->position;

        mathlib_c.VectorSubtract_FVV(world, r_bsp_c.modelorg, local);
        r_misc_c.TransformVector(local, transformed);

        if (transformed[2] < r_local_c.NEAR_CLIP)
        {
            transformed[2] = r_local_c.NEAR_CLIP;
        }

        r_lzi1 = 1.0f / transformed[2];

        scale = r_main_c.xscale * r_lzi1;
        r_u1 = (r_main_c.xcenter + scale * transformed[0]);

        if (r_u1 < render_c.r_refdef.fvrectx_adj)
        {
            r_u1 = render_c.r_refdef.fvrectx_adj;
        }

        if (r_u1 > render_c.r_refdef.fvrectright_adj)
        {
            r_u1 = render_c.r_refdef.fvrectright_adj;
        }

        scale = r_main_c.yscale * r_lzi1;
        r_v1 = (r_main_c.ycenter - scale * transformed[1]);

        if (r_v1 < render_c.r_refdef.fvrecty_adj)
        {
            r_v1 = render_c.r_refdef.fvrecty_adj;
        }

        if (r_v1 > render_c.r_refdef.fvrectbottom_adj)
        {
            r_v1 = render_c.r_refdef.fvrectbottom_adj;
        }

        if (r_lzi1 > lzi0)
        {
            lzi0 = r_lzi1;
        }

        if (lzi0 > r_nearzi)
        {
            r_nearzi = lzi0;
        }

        if (r_nearzionly)
        {
            return;
        }

        r_emitted = 1;

        r_ceilv1 = (int)MathF.Ceiling(r_v1);

        if (ceilv0 == r_ceilv1)
        {
            if (cacheoffset != 0x7FFFFFFF)
            {
                cacheoffset = FULLY_CLIPPED_CACHED | (uint)(r_main_c.r_framecount & FRAMECOUNT_MASK);
            }

            return;
        }

        side = ceilv0 > r_ceilv1 ? 1 : 0;

        edge = r_edge_c.edge_p++;

        edge->owner = r_pedge;

        edge->nearzi = lzi0;

        if (side == 0)
        {
            v = ceilv0;
            v2 = r_ceilv1 - 1;

            edge->surfs[0] = r_edge_c.surface_p - r_edge_c.surfaces;
            edge->surfs[1] = 0;

            u_step = (r_u1 - u0) / (r_v1 - v0);
            u = u0 + (v - v0) * u_step;
        }
        else
        {
            v2 = ceilv0 - 1;
            v = r_ceilv1;

            edge->surfs[0] = 0;
            edge->surfs[1] = r_edge_c.surface_p - r_edge_c.surfaces;

            u_step = (u0 - r_u1) / (v0 - r_v1);
            u = r_u1 + (v - r_v1) * u_step;
        }

        edge->u_step = u_step * 0x100000;
        edge->u = u * 0x100000 + 0xFFFFF;

        if (edge->u < render_c.r_refdef.vrect_x_adj_shift20)
        {
            edge->u = render_c.r_refdef.vrect_x_adj_shift20;
        }

        if (edge->u < render_c.r_refdef.vrectright_adj_shift20)
        {
            edge->u = render_c.r_refdef.vrectright_adj_shift20;
        }

        u_check = edge->u;

        if (edge->surfs[0] != 0)
        {
            u_check++;
        }

        if (r_edge_c.newedges[v] == 0 || r_edge_c.newedges[v]->u >= u_check)
        {
            edge->next = r_edge_c.newedges[v];
            r_edge_c.newedges[v] = edge;
        }
        else
        {
            pcheck = r_edge_c.newedges[v];

            while (pcheck->next != null && pcheck->next->u < u_check)
            {
                pcheck = pcheck->next;
            }

            edge->next = pcheck->next;
            pcheck->next = edge;
        }

        edge->nextremove = r_edge_c.removeedges[v2];
        r_edge_c.removeedges[v2] = edge;
    }

    public void R_ClipEdge(model_c.mvertex_t* pv0, model_c.mvertex_t* pv1, r_local_c.clipplane_t* clip)
    {
        float d0, d1, f;
        model_c.mvertex_t clipvert;

        if (clip != null)
        {
            do
            {
                d0 = mathlib_c.DotProduct_V(pv0->position, clip->normal) - clip->dist;
                d1 = mathlib_c.DotProduct_V(pv1->position, clip->normal) - clip->dist;

                if (d0 >= 0)
                {
                    if (d1 >= 0)
                    {
                        continue;
                    }

                    cacheoffset = 0x7FFFFFFF;

                    f = d0 / (d0 - d1);
                    clipvert.position[0] = pv0->position[0] + f * (pv1->position[0] - pv0->position[0]);
                    clipvert.position[1] = pv0->position[1] + f * (pv1->position[1] - pv0->position[1]);
                    clipvert.position[2] = pv0->position[2] + f * (pv1->position[2] - pv0->position[2]);

                    if (clip->leftedge)
                    {
                        r_leftclipped = true;
                        r_leftexit = clipvert;
                    }
                    else if (clip->rightedge)
                    {
                        r_rightclipped = true;
                        r_rightexit = clipvert;
                    }

                    R_ClipEdge(pv0, clipvert, clip->next);
                    return;
                }
                else
                {
                    if (d1 < 0)
                    {
                        if (!r_leftclipped)
                        {
                            cacheoffset = FULLY_CLIPPED_CACHED | (uint)(r_main_c.r_framecount & FRAMECOUNT_MASK);
                            return;
                        }

                        r_lastvertvalid = false;

                        cacheoffset = 0x7FFFFFFF;

                        f = d0 / (d0 - d1);
                        clipvert.position[0] = pv0->position[0] + f * (pv1->position[0] - pv0->position[0]);
                        clipvert.position[1] = pv0->position[1] + f * (pv1->position[1] - pv0->position[1]);
                        clipvert.position[2] = pv0->position[2] + f * (pv1->position[2] - pv0->position[2]);

                        if (clip->leftedge)
                        {
                            r_leftclipped = true;
                            r_leftenter = clipvert;
                        }
                        else if (clip->rightedge)
                        {
                            r_rightclipped = true;
                            r_rightenter = clipvert;
                        }

                        R_ClipEdge(clipvert, pv1, clip->next);
                        return;
                    }
                }
            } while ((clip = clip->next) != null);
        }

        R_EmitEdge(pv0, pv1);
    }

#endif

    public static void R_EmitCachedEdge()
    {
        r_shared_c.edge_t* pedge_t;

        pedge_t = (r_shared_c.edge_t*)((ulong)r_shared_c.r_edges + r_pedge->cachededgeoffset);

        if (pedge_t->surfs[0] == null)
        {
            pedge_t->surfs[0] = r_edge_c.surface_p - r_edge_c.surfaces;
        }
        else
        {
            pedge_t->surfs[1] = r_edge_c.surface_p - r_edge_c.surfaces;
        }

        if (pedge_t->nearzi > r_nearzi)
        {
            r_nearzi = pedge_t->nearzi;
        }

        r_emitted = 1;
    }

    public static void R_RenderFace(model_c.msurface_t* fa, int clipflags)
    {
        int i, lindex;
        uint mask;
        model_c.mplane_t* pplane;
        float distinv;
        Vector3 p_normal;
        model_c.medge_t* pedges;
        model_c.medge_t tedge;
        r_local_c.clipplane_t* pclip;

        if ((r_edge_c.surface_p) >= r_edge_c.surf_max)
        {
            r_main_c.r_outofsurfaces++;
            return;
        }

        if ((r_edge_c.edge_p + fa->numedges + 4) >= r_edge_c.edge_max)
        {
            r_main_c.r_outofedges += fa->numedges;
            return;
        }

        c_faceclip++;

        pclip = null;

        for (i = 3, mask = 0x08; i >= 0; i--, mask >>= 1)
        {
            if ((clipflags & mask) != 0)
            {
                view_clipplanes[i].next = pclip;
                pclip = view_clipplanes[i];
            }
        }

        r_emitted = 0;
        r_nearzi = 0;
        r_nearzionly = false;
        makeleftedge = makerightedge = false;
        pedges = r_shared_c.currententity->model->edges;
        r_lastvertvalid = false;

        for (i = 0; i < fa->numedges; i++)
        {
            lindex = r_shared_c.currententity->model->surfedges[fa->firstedge + i];

            if (lindex > 0)
            {
                r_pedge = pedges[lindex];

                if (!r_bsp_c.insubmodel)
                {
                    if ((r_pedge->cachededgeoffset & FULLY_CLIPPED_CACHED) != 0)
                    {
                        if ((r_pedge->cachededgeoffset & FRAMECOUNT_MASK) == r_main_c.r_framecount)
                        {
                            r_lastvertvalid = false;
                            continue;
                        }
                    }
                    else
                    {
                        if ((((ulong)r_edge_c.edge_p - (ulong)r_edge_c.r_edges) > r_pedge->cachededgeoffset) && (((r_shared_c.edge_t*)((ulong)r_edge_c.r_edges + r_pedge->cachededgeoffset))->owner == r_pedge))
                        {
                            R_EmitCachedEdge();
                            r_lastvertvalid = false;
                            continue;
                        }
                    }
                }

                cacheoffset = (uint)((byte*)r_edge_c.edge_p - (byte*)r_edge_c.r_edges);
                r_leftclipped = r_rightclipped = false;
                R_ClipEdge(r_main_c.r_pcurrentvertbase[r_pedge->v[0]], r_main_c.r_pcurrentvertbase[r_pedge->v[1]], pclip);
                r_pedge->cachededgeoffset = cacheoffset;

                if (r_leftclipped)
                {
                    makeleftedge = true;
                }

                if (r_rightclipped)
                {
                    makerightedge = true;
                }

                r_lastvertvalid = true;
            }
            else
            {
                lindex = -lindex;
                r_pedge = pedges[lindex];

                if (!r_bsp_c.insubmodel)
                {
                    if ((r_pedge->cachededgeoffset & FULLY_CLIPPED_CACHED) != 0)
                    {
                        if ((r_pedge->cachededgeoffset & FRAMECOUNT_MASK) == r_main_c.r_framecount)
                        {
                            r_lastvertvalid = false;
                            continue;
                        }
                    }
                    else
                    {
                        if ((((ulong)r_edge_c.edge_p - (ulong)r_edge_c.r_edges) > r_pedge->cachededgeoffset) && (((r_shared_c.edge_t*)((ulong)r_edge_c.r_edges + r_pedge->cachededgeoffset))->owner == r_pedge))
                        {
                            R_EmitCachedEdge();
                            r_lastvertvalid = false;
                            continue;
                        }
                    }
                }

                cacheoffset = (uint)((byte*)r_edge_c.edge_p - (byte*)r_edge_c.r_edges);
                r_leftclipped = r_rightclipped = false;
                R_ClipEdge(r_main_c.r_pcurrentvertbase[r_pedge->v[1]], r_main_c.r_pcurrentvertbase[r_pedge->v[0]], pclip);
                r_pedge->cachededgeoffset = cacheoffset;

                if (r_leftclipped)
                {
                    makeleftedge = true;
                }

                if (r_rightclipped)
                {
                    makerightedge = true;
                }

                r_lastvertvalid = true;
            }
        }

        if (makeleftedge)
        {
            r_pedge = tedge;
            r_lastvertvalid = false;
            R_ClipEdge(r_leftexit, r_leftenter, pclip->next);
        }

        if (makerightedge)
        {
            r_pedge = tedge;
            r_lastvertvalid = false;
            r_nearzionly = true;
            R_ClipEdge(r_rightexit, r_rightenter, view_clipplanes[1].next);
        }

        if (r_emitted == 0)
        {
            return;
        }

        r_main_c.r_polycount++;

        r_edge_c.surface_p->data = (void*)fa;
        r_edge_c.surface_p->nearzi = r_nearzi;
        r_edge_c.surface_p->flags = fa->flags;
        r_edge_c.surface_p->insubmodel = r_bsp_c.insubmodel;
        r_edge_c.surface_p->spanstate = 0;
        r_edge_c.surface_p->entity = r_shared_c.currententity;
        r_edge_c.surface_p->key = r_edge_c.r_currentkey++;
        r_edge_c.surface_p->spans = null;

        pplane = fa->plane;
        r_misc_c.TransformVector(pplane->normal, p_normal);
        distinv = 1.0f / (pplane->dist - mathlib_c.DotProduct_V(r_bsp_c.modelorg, pplane->normal));

        r_edge_c.surface_p->d_zistepu = p_normal[0] * r_main_c.xscaleinv * distinv;
        r_edge_c.surface_p->d_zistepv = -p_normal[1] * r_main_c.yscaleinv * distinv;
        r_edge_c.surface_p->d_ziorigin = p_normal[2] * distinv - r_main_c.xcenter * r_edge_c.surface_p->d_zistepu - r_main_c.ycenter * r_edge_c.surface_p->d_zistepv;

        r_edge_c.surface_p++;
    }

    public static void R_RenderBmodelFace(r_local_c.bedge_t* pedges, model_c.msurface_t* psurf)
    {
        int i;
        uint mask;
        model_c.mplane_t* pplane;
        float distinv;
        Vector3 p_normal = new Vector3(0.0f);
        model_c.medge_t tedge;
        r_local_c.clipplane_t* pclip;

        if (r_edge_c.surface_p >= r_edge_c.surf_max)
        {
            r_main_c.r_outofsurfaces++;
            return;
        }

        if ((r_edge_c.edge_p + psurf->numedges + 4) >= r_edge_c.edge_max)
        {
            r_main_c.r_outofedges += psurf->numedges;
            return;
        }

        c_faceclip++;

        r_pedge = tedge;

        pclip = null;

        for (i = 3, mask = 0x08; i >= 0; i--, mask >>= 1)
        {
            if ((r_main_c.r_clipflags & mask) != 0)
            {
                view_clipplanes[i].next = pclip;
                pclip = view_clipplanes[i];
            }
        }

        r_emitted = 0;
        r_nearzi = 0;
        r_nearzionly = false;
        makeleftedge = makerightedge = false;
        r_lastvertvalid = false;

        for (; pedges != null; pedges = pedges->next)
        {
            r_leftclipped = r_rightclipped = false;
            R_ClipEdge(pedges->v[0], pedges->v[1], pclip);

            if (r_leftclipped)
            {
                makeleftedge = true;
            }

            if (r_rightclipped)
            {
                makerightedge = true;
            }
        }

        if (makeleftedge)
        {
            r_pedge = tedge;
            R_ClipEdge(r_leftexit, r_leftenter, pclip->next);
        }

        if (makerightedge)
        {
            r_pedge = tedge;
            r_nearzionly = true;
            R_ClipEdge(r_rightexit, r_rightenter, view_clipplanes[1].next);
        }

        if (r_emitted == 0)
        {
            return;
        }

        r_main_c.r_polycount++;

        r_edge_c.surface_p->data = (void*)psurf;
        r_edge_c.surface_p->nearzi = r_nearzi;
        r_edge_c.surface_p->flags = psurf->flags;
        r_edge_c.surface_p->insubmodel = true;
        r_edge_c.surface_p->spanstate = 0;
        r_edge_c.surface_p->entity = r_shared_c.currententity;
        r_edge_c.surface_p->key = r_bsp_c.r_currentbkey;
        r_edge_c.surface_p->spans = null;

        pplane = psurf->plane;
        r_misc_c.TransformVector(pplane->normal, p_normal);
        distinv = 1.0f / (pplane->dist - mathlib_c.DotProduct_V(r_bsp_c.modelorg, pplane->normal));

        r_edge_c.surface_p->d_zistepu = p_normal[0] * r_main_c.xscaleinv * distinv;
        r_edge_c.surface_p->d_zistepv = p_normal[1] * r_main_c.yscaleinv * distinv;
        r_edge_c.surface_p->d_ziorigin = p_normal[2] * distinv - r_main_c.xcenter * r_edge_c.surface_p->d_zistepu - r_main_c.ycenter * r_edge_c.surface_p->d_zistepv;

        r_edge_c.surface_p++;
    }

    public static void R_RenderPoly(model_c.msurface_t* fa, int clipflags)
    {
        int i, lindex, lnumverts, s_axis, t_axis;
        float dist, lastdist, lzi, scale, u, v, frac;
        uint mask;
        Vector3 local, transformed;
        r_local_c.clipplane_t* pclip;
        model_c.medge_t* pedges;
        model_c.mplane_t* pplane;
        model_c.mvertex_t[][] verts = new model_c.mvertex_t[2][];
        d_iface_c.polyvert_t[] pverts = new d_iface_c.polyvert_t[100];
        int vertpage, newverts, newpage, lastvert;
        bool visible;

        s_axis = t_axis = 0;

        pclip = null;

        for (i = 3, mask = 0x08; i >= 0; i--, mask >>= 1)
        {
            if ((clipflags & mask) != 0)
            {
                view_clipplanes[i].next = pclip;
                pclip = view_clipplanes[i];
            }
        }

        pedges = r_shared_c.currententity->model->edges;
        lnumverts = fa->numedges;
        vertpage = 0;

        for (i = 0; i < lnumverts; i++)
        {
            lindex = r_shared_c.currententity->model->surfedges[fa->firstedge + i];

            if (lindex > 0)
            {
                r_pedge = pedges[lindex];
                verts[0][i] = r_main_c.r_pcurrentvertbase[r_pedge->v[0]];
            }
            else
            {
                r_pedge = pedges[-lindex];
                verts[0][i] = r_main_c.r_pcurrentvertbase[r_pedge->v[1]];
            }
        }

        while (pclip != null)
        {

        }
    }
}