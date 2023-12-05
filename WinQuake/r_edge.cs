namespace Quake;

public unsafe class r_edge_c
{
    public static r_shared_c.edge_t* auxedges;
    public static r_shared_c.edge_t* r_edges, edge_p, edge_max;

    public static r_shared_c.surf_t* surfaces, surface_p, surf_max;

    public static r_shared_c.edge_t* newedges;
    public static r_shared_c.edge_t* removeedges;

    public static r_shared_c.espan_t* span_p, max_span_p;

    public static int r_currentkey;

    public static int screenwidth;

    public static int current_iv;

    public static int edge_head_u_shift20, edge_tail_u_shift20;

    public static Action pdrawfunc;

    public static r_shared_c.edge_t edge_head;
    public static r_shared_c.edge_t edge_tail;
    public static r_shared_c.edge_t edge_aftertail;
    public static r_shared_c.edge_t edge_sentinel;

    public static float fv;

    public static void R_DrawCulledPolys()
    {
        r_shared_c.surf_t* s;
        model_c.msurface_t* pface;

        r_shared_c.currententity = &cl_main_c.cl_entities[0];

        if (r_main_c.r_worldpolysbacktofront)
        {
            for (s = surface_p - 1; s > &surfaces[1]; s--)
            {
                if (s->spans == null)
                {
                    continue;
                }

                if ((s->flags & model_c.SURF_DRAWBACKGROUND) == 0)
                {
                    pface = (model_c.msurface_t*)s->data;
                    r_draw_c.R_RenderPoly(pface, 15);
                }
            }
        }
        else
        {
            for (s = &surfaces[1]; s < surface_p; s++)
            {
                if (s->spans == null)
                {
                    continue;
                }

                if ((s->flags & model_c.SURF_DRAWBACKGROUND) == 0)
                {
                    pface = (model_c.msurface_t*)s->data;
                    r_draw_c.R_RenderPoly(pface, 15);
                }
            }
        }
    }

    public static void R_BeginEdgeFrame()
    {
        int v;

        edge_p = r_edges;
        edge_max = &r_edges[r_main_c.r_numallocatededges];

        surface_p = &surfaces[2];

        surfaces[1].spans = null;
        surfaces[1].flags = model_c.SURF_DRAWBACKGROUND;

        if (r_main_c.r_draworder.value != 0)
        {
            pdrawfunc = R_GenerateSpansBackward;
            surfaces[1].key = 0;
            r_currentkey = 1;
        }
        else
        {
            pdrawfunc = R_GenerateSpans;
            surfaces[1].key = 0x7FFFFFFF;
            r_currentkey = 0;
        }

        for (v = r_main_c.r_refdef.vrect.y; v < r_main_c.r_refdef.vrectbottom; v++)
        {
            newedges[v] = removeedges[v] = default;
        }
    }

#if !id386

    public void R_InsertNewEdges(r_shared_c.edge_t* edgestoadd, r_shared_c.edge_t* edgelist)
    {
        r_shared_c.edge_t* next_edge;

        do
        {
            next_edge = edgestoadd->next;

        edgesearch:
            if (edgelist->u >= edgestoadd->u)
            {
                goto addedge;
            }

            edgelist = edgelist->next;

            if (edgelist->u >= edgestoadd->u)
            {
                goto addedge;
            }

            edgelist = edgelist->next;

            if (edgelist->u >= edgestoadd->u)
            {
                goto addedge;
            }

            edgelist = edgelist->next;

            if (edgelist->u >= edgestoadd->u)
            {
                goto addedge;
            }

            edgelist = edgelist->next;
            goto edgesearch;

        addedge:
            edgestoadd->next = edgelist;
            edgestoadd->prev = edgelist->prev;
            edgelist->prev->next = edgestoadd;
            edgelist->prev = edgestoadd;
        } while ((edgestoadd = next_edge) != null);
    }

#endif


#if !id386

    public void R_RemoveEdges(r_shared_c.edge_t* pedge)
    {
        do
        {
            pedge->next->prev = pedge->prev;
            pedge->prev->next = pedge->next;
        } while ((pedge = pedge->nextremove) != null);
    }

#endif

#if !id386

    public void R_StepActiveU(r_shared_c.edge_t* pedge)
    {
        r_shared_c.edge_t* pnext_edge, pwedge;

        while (true)
        {
        nextedge:
            pedge->u += pedge->u_step;

            if (pedge->u < pedge->prev->u)
            {
                goto pushback;
            }

            pedge = pedge->next;

            pedge->u += pedge->u_step;

            if (pedge->u < pedge->prev->u)
            {
                goto pushback;
            }

            pedge = pedge->next;

            pedge->u += pedge->u_step;

            if (pedge->u < pedge->prev->u)
            {
                goto pushback;
            }

            pedge = pedge->next;

            pedge->u += pedge->u_step;

            if (pedge->u < pedge->prev->u)
            {
                goto pushback;
            }

            pedge = pedge->next;

            goto nextedge;

        pushback:
            if (pedge == &edge_aftertail)
            {
                return;
            }

            pnext_edge = pedge->next;

            pedge->next->prev = pedge->prev;
            pedge->prev->next = pedge->next;

            pwedge = pedge->prev->prev;

            while (pwedge->u > pedge->u)
            {
                pwedge = pwedge->prev;
            }

            pedge->next = pwedge->next;
            pedge->prev = pwedge;
            pedge->next->prev = pedge;
            pwedge->next = pedge;

            pedge = pnext_edge;

            if (pedge == &edge_tail)
            {
                return;
            }
        }
    }

#endif

    public static void R_CleanupSpan()
    {
        r_shared_c.surf_t* surf;
        int iu;
        r_shared_c.espan_t* span;

        surf = surfaces[1].next;
        iu = edge_tail_u_shift20;

        if (iu > surf->last_u)
        {
            span = span_p++;
            span->u = surf->last_u;
            span->count = iu - span->u;
            span->v = current_iv;
            span->pnext = surf->spans;
            surf->spans = span;
        }

        do
        {
            surf->spanstate = 0;
            surf = surf->next;
        } while (surf != &surfaces[1]);
    }

    public static void R_LeadingEdgeBackwards(r_shared_c.edge_t* edge)
    {
        r_shared_c.espan_t* span;
        r_shared_c.surf_t* surf, surf2;
        int iu;

        surf = &surfaces[edge->surfs[1]];

        if (++surf->spanstate == 1)
        {
            surf2 = surfaces[1].next;

            if (surf->key > surf2->key)
            {
                goto newtop;
            }

            if (surf->insubmodel && (surf->key == surf2->key))
            {
                goto newtop;
            }

        continue_search:
            do
            {
                surf2 = surf2->next;
            } while (surf->key < surf2->key);

            if (surf->key == surf2->key)
            {
                if (!surf->insubmodel)
                {
                    goto continue_search;
                }
            }

            goto gotposition;

        newtop:
            iu = edge->u >> 20;

            if (iu > surf2->last_u)
            {
                span = span_p++;
                span->u = surf2->last_u;
                span->count = iu - span->u;
                span->v = current_iv;
                span->pnext = surf2->spans;
                surf2->spans = span;
            }

            surf->last_u = iu;

        gotposition:
            surf->next = surf2;
            surf->prev = surf2->prev;
            surf2->prev->next = surf;
            surf2->prev = surf;
        }
    }

    public static void R_TrailingEdge(r_shared_c.surf_t* surf, r_shared_c.edge_t* edge)
    {
        r_shared_c.espan_t* span;
        int iu;

        if (--surf->spanstate == 0)
        {
            if (surf->insubmodel)
            {
                r_vars_c.r_bmodelactive--;
            }

            if (surf == surfaces[1].next)
            {
                iu = edge->u >> 20;

                if (iu > surf->last_u)
                {
                    span = span_p++;
                    span->u = surf->last_u;
                    span->count = iu - span->u;
                    span->v = current_iv;
                    span->pnext = surf->spans;
                    surf->spans = span;
                }

                surf->next->last_u = iu;
            }

            surf->prev->next = surf->next;
            surf->next->prev = surf->prev;
        }
    }

#if !id386

    public static void R_LeadingEdge(r_shared_c.edge_t* edge)
    {
        r_shared_c.espan_t* span;
        r_shared_c.surf_t* surf, surf2;
        int iu;
        double fu, newzi, testzi, newzitop, newzibottom;

        if (edge->surfs[1] != 0)
        {
            surf = &surfaces[edge->surfs[1]];

            if (++surf->spanstate == 1)
            {
                if (surf->insubmodel)
                {
                    r_vars_c.r_bmodelactive++;
                }

                surf2 = surfaces[1].next;

                if (surf->key < surf2->key)
                {
                    goto newtop;
                }

                if (surf->insubmodel && (surf->key == surf2->key))
                {
                    fu = (edge->u - 0xFFFFF) * (1.0 / 0x100000);
                    newzi = surf->d_ziorigin + fv * surf->d_zistepv + fu * surf->d_zistepu;
                    newzibottom = newzi * 0.99;

                    testzi = surf2->d_ziorigin + fv * surf2->d_zistepv + fu * surf2->d_zistepu;

                    if (newzibottom >= testzi)
                    {
                        goto newtop;
                    }

                    newzitop = newzi * 1.01;

                    if (newzitop >= testzi)
                    {
                        if (surf->d_zistepu >= surf2->d_zistepu)
                        {
                            goto newtop;
                        }
                    }
                }

            continue_search:
                do
                {
                    surf2 = surf2->next;
                } while (surf->key > surf2->key);

                if (surf->key == surf2->key)
                {
                    if (!surf->insubmodel)
                    {
                        goto continue_search;
                    }

                    fu = (edge->u - 0xFFFFF) * (1.0 / 0x100000);
                    newzi = surf->d_ziorigin + fv * surf->d_zistepv + fu * surf->d_zistepu;
                    newzibottom = newzi * 0.99;

                    testzi = surf2->d_ziorigin + fv * surf2->d_zistepv + fu * surf2->d_zistepu;

                    if (newzibottom >= testzi)
                    {
                        goto gotposition;
                    }

                    newzitop = newzi * 1.01;

                    if (newzitop >= testzi)
                    {
                        if (surf->d_zistepu >= surf2->d_zistepu)
                        {
                            goto gotposition;
                        }
                    }

                    goto continue_search;
                }

                goto gotposition;

            newtop:

                iu = edge->u >> 20;

                if (iu > surf2->last_u)
                {
                    span = span_p++;
                    span->u = surf2->last_u;
                    span->count = iu - span->u;
                    span->v = current_iv;
                    span->pnext = surf2->spans;
                    surf2->spans = span;
                }

                surf->last_u = iu;

            gotposition:

                surf->next = surf2;
                surf->prev = surf2->prev;
                surf2->prev->next = surf;
                surf2->prev = surf;
            }
        }
    }

    public static void R_GenerateSpans()
    {
        r_shared_c.edge_t* edge;
        r_shared_c.surf_t* surf;

        r_vars_c.r_bmodelactive = 0;

        surfaces[1].next = surfaces[1].prev = &surfaces[1];
        surfaces[1].last_u = edge_head_u_shift20;

        for (edge = edge_head.next; edge != &edge_tail; edge = edge->next)
        {
            if (edge->surfs[0] != 0)
            {
                surf = &surfaces[edge->surfs[0]];

                R_TrailingEdge(surf, edge);

                if (edge->surfs[1] == 0)
                {
                    continue;
                }
            }

            R_LeadingEdge(edge);
        }

        R_CleanupSpan();
    }

#endif

    public static void R_GenerateSpansBackward()
    {
        r_shared_c.edge_t* edge;

        r_vars_c.r_bmodelactive = 0;

        surfaces[1].next = surfaces[1].prev = &surfaces[1];
        surfaces[1].last_u = edge_head_u_shift20;

        for (edge = edge_head.next; edge != &edge_tail; edge = edge->next)
        {
            if (edge->surfs[0] != 0)
            {
                R_TrailingEdge(&surfaces[edge->surfs[0]], edge);
            }

            if (edge->surfs[1] != 0)
            {
                R_LeadingEdgeBackwards(edge);
            }
        }

        R_CleanupSpan();
    }

    public void R_ScanEdges()
    {
        int iv, bottom;
        byte* basespans = null;
        r_shared_c.espan_t* basespan_p;
        r_shared_c.surf_t* s;

        basespan_p = (r_shared_c.espan_t*)((long)(basespans + bothdefs_c.CACHE_SIZE - 1) & ~(bothdefs_c.CACHE_SIZE - 1));

        max_span_p = &basespan_p[r_shared_c.MAXSPANS - r_main_c.r_refdef.vrect.width];

        span_p = basespan_p;

        edge_head.u = r_main_c.r_refdef.vrect.x << 20;
        edge_head_u_shift20 = edge_head.u >> 20;
        edge_head.u_step = 0;
        edge_head.prev = null;
        edge_head.next = &edge_tail;
        edge_head.surfs[0] = 0;
        edge_head.surfs[1] = 1;

        edge_tail.u = (r_main_c.r_refdef.vrectright << 20) + 0xFFFFF;
        edge_tail_u_shift20 = edge_tail.u >> 20;
        edge_tail.u_step = 0;
        edge_tail.prev = &edge_head;
        edge_tail.next = &edge_aftertail;
        edge_tail.surfs[0] = 1;
        edge_tail.surfs[1] = 0;

        edge_aftertail.u = -1;
        edge_aftertail.u_step = 0;
        edge_aftertail.next = &edge_sentinel;
        edge_aftertail.prev = &edge_tail;

        edge_sentinel.u = 2000 << 24;
        edge_sentinel.prev = &edge_aftertail;

        bottom = r_main_c.r_refdef.vrectbottom - 1;

        for (iv = r_main_c.r_refdef.vrect.y; iv < bottom; iv++)
        {
            current_iv = iv;
            fv = (float)iv;

            surfaces[1].spanstate = 1;

            if (&newedges[iv] != null)
            {
                R_InsertNewEdges(&newedges[iv], edge_head.next);
            }

            pdrawfunc();

            if (span_p >= max_span_p)
            {
                VID_UnlockBuffer();
                S_ExtraUpdate();
                VID_LockBuffer();

                if (r_main_c.r_drawculledpolys)
                {
                    R_DrawCulledPolys();
                }
                else
                {
                    d_edge_c.D_DrawSurfaces();
                }

                for (s = &surfaces[1]; s < surface_p; s++)
                {
                    s->spans = null;
                }

                span_p = basespan_p;
            }

            if (&removeedges[iv] != null)
            {
                R_RemoveEdges(&removeedges[iv]);
            }

            if (edge_head.next != &edge_tail)
            {
                R_StepActiveU(edge_head.next);
            }
        }

        current_iv = iv;
        fv = (float)iv;

        surfaces[1].spanstate = 1;

        if (&newedges[iv] != null)
        {
            R_InsertNewEdges(&newedges[iv], edge_head.next);
        }

        pdrawfunc();

        if (r_main_c.r_drawculledpolys)
        {
            R_DrawCulledPolys();
        }
        else
        {
            d_edge_c.D_DrawSurfaces();
        }
    }
}