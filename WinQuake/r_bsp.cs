using System.Runtime.Intrinsics.Arm;

namespace Quake;

public unsafe class r_bsp_c
{
    public static bool insubmodel;
    public static render_c.entity_t* currententity;
    public static Vector3 modelorg, base_modelorg;

    public static Vector3 r_entorigin;

    public static float[][] entity_rotation = new float[3][];

    public Vector3 r_worldmodelorg;

    public static int r_currentbkey;

    public enum solidstate_t { touchessolid, drawnode, nodrawnode }

    public static int MAX_BMODEL_VERTS = 500;
    public static int MAX_BMODEL_EDGES = 1000;

    public static model_c.mvertex_t* pbverts;
    public static r_local_c.bedge_t* pbedges;
    public static int numbverts, numbedges;

    public static model_c.mvertex_t* pfrontenter, pfrontexit;

    public static bool makeclippededge;

    public static void R_EntityRotate(Vector3 vec)
    {
        Vector3 tvec = new Vector3(0.0f);

        mathlib_c.VectorCopy(vec, tvec);
        vec[0] = mathlib_c.DotProduct_FV(entity_rotation[0], tvec);
        vec[1] = mathlib_c.DotProduct_FV(entity_rotation[1], tvec);
        vec[2] = mathlib_c.DotProduct_FV(entity_rotation[2], tvec);
    }

    public static void R_RotateBmodel()
    {
        float angle, s, c;
        float[][] temp1 = new float[3][], temp2 = new float[3][], temp3 = new float[3][];

        angle = currententity->angles[quakedef_c.YAW];
        angle = angle * MathF.PI * 2 / 360;
        s = MathF.Sin(angle);
        c = MathF.Cos(angle);

        temp1[0][0] = c;
        temp1[0][1] = s;
        temp1[0][2] = 0;
        temp1[1][0] = -s;
        temp1[1][1] = c;
        temp1[1][2] = 0;
        temp1[2][0] = 0;
        temp1[2][1] = 0;
        temp1[2][2] = 1;

        angle = currententity->angles[quakedef_c.PITCH];
        angle = angle * MathF.PI * 2 / 360;
        s = MathF.Sin(angle);
        c = MathF.Cos(angle);

        temp2[0][0] = c;
        temp2[0][1] = 0;
        temp2[0][2] = -s;
        temp2[1][0] = 0;
        temp2[1][1] = 1;
        temp2[1][2] = 0;
        temp2[2][0] = s;
        temp2[2][1] = 0;
        temp2[2][2] = c;

        mathlib_c.R_ConcatRotations(temp2, temp1, temp3);

        angle = currententity->angles[quakedef_c.ROLL];
        angle = angle * MathF.PI * 2 / 360;
        s = MathF.Sin(angle);
        c = MathF.Cos(angle);

        temp1[0][0] = 1;
        temp1[0][1] = 0;
        temp1[0][2] = 0;
        temp1[1][0] = 0;
        temp1[1][1] = c;
        temp1[1][2] = s;
        temp1[2][0] = 0;
        temp1[2][1] = -s;
        temp1[2][2] = c;

        mathlib_c.R_ConcatRotations(temp1, temp3, entity_rotation);

        R_EntityRotate(modelorg);
        R_EntityRotate(vpn);
        R_EntityRotate(vright);
        R_EntityRotate(vup);

        R_TransformFrustum();
    }

    public static void R_RecursiveClipBPoly(bedge_t* pedges, mnode_t* pnode, msurface_t* psurf)
    {
        r_local_c.bedge_t* psideedges = default, pnextedge = default, ptedge;
        int i, side, lastside;
        float dist, frac, lastdist;
        model_c.mplane_t* splitplane;
        model_c.mplane_t tplane;
        model_c.mvertex_t* pvert, plastvert, ptvert;
        model_c.mnode_t* pn;

        psideedges[0] = psideedges[1] = null;

        makeclippededge = false;

        splitplane = pnode->plane;
        tplane.dist = splitplane->dist - mathlib_c.DotProduct_V(r_entorigin, splitplane->normal);
        tplane.normal[0] = mathlib_c.DotProduct_FV(entity_rotation[0], splitplane->normal);
        tplane.normal[1] = mathlib_c.DotProduct_FV(entity_rotation[1], splitplane->normal);
        tplane.normal[2] = mathlib_c.DotProduct_FV(entity_rotation[2], splitplane->normal);

        for (; pedges != null; pedges = pnextedge)
        {
            pnextedge = pedges->next;

            plastvert = pedges->v[0];
            lastdist = mathlib_c.DotProduct_V(plastvert->position, tplane.normal) - tplane.dist;

            if (lastdist > 0)
            {
                lastside = 0;
            }
            else
            {
                lastside = 1;
            }

            pvert = pedges->v[1];

            dist = mathlib_c.DotProduct_V(pvert->position, tplane.normal) - tplane.dist;

            if (dist > 0)
            {
                side = 0;
            }
            else
            {
                side = 1;
            }

            if (side != lastside)
            {
                if (numbverts >= MAX_BMODEL_VERTS)
                {
                    return;
                }

                frac = lastdist / (lastdist - dist);
                ptvert = pbverts[numbverts++];
                ptvert->position[0] = plastvert->position[0] + frac * (pvert->position[0] - plastvert->position[0]);
                ptvert->position[1] = plastvert->position[1] + frac * (pvert->position[1] - plastvert->position[1]);
                ptvert->position[2] = plastvert->position[2] + frac * (pvert->position[2] - plastvert->position[2]);

                if (numbedges >= (MAX_BMODEL_EDGES - 1))
                {
                    console_c.Con_Printf("Out of edges for bmodel\n");
                    return;
                }

                ptedge = pbedges[numbedges];
                ptedge->pnext = psideedges[lastside];
                psideedges[lastside] = ptedge;
                ptedge->v[0] = plastvert;
                ptedge->v[1] = ptvert;

                ptedge = pbedges[numbedges + 1];
                ptedge->pnext = psideedges[side];
                psideedges[side] = ptedge;
                ptedge->v[0] = ptvert;
                ptedge->v[1] = pvert;

                numbedges += 2;

                if (side == 0)
                {
                    pfrontenter = ptvert;
                    makeclippededge = true;
                }
                else
                {
                    pfrontexit = ptvert;
                    makeclippededge = true;
                }
            }
            else
            {
                pedges->pnext = psideedges[side];
                psideedges[side] = pedges;
            }
        }

        if (makeclippededge)
        {
            if (numbedges >= (MAX_BMODEL_EDGES - 2))
            {
                console_c.Con_Printf("Out of edges for bmodel\n");
                return;
            }

            ptedge = pbedges[numbedges];
            ptedge->pnext = psideedges[0];
            psideedges[0] = ptedge;
            ptedge->v[0] = pfrontexit;
            ptedge->v[1] = pfrontenter;

            ptedge = pbedges[numbedges + 1];
            ptedge->pnext = psideedges[1];
            psideedges[1] = ptedge;
            ptedge->v[0] = pfrontenter;
            ptedge->v[1] = pfrontexit;

            numbedges += 2;
        }

        for (i = 0; i < 2; i++)
        {
            if (psideedges[i])
            {
                pn = pnode->children[i];

                if (pn->visframe == r_main_c.r_visframecount)
                {
                    if (pn->contents < 0)
                    {
                        if (pn->contents != bspfile_c.CONTENTS_SOLID)
                        {
                            r_currentbkey = ((model_c.mleaf_t*)pn)->key;
                            r_draw_c.R_RenderBmodelFace(psideedges[i], psurf);
                        }
                    }
                    else
                    {
                        R_RecursiveClipBPoly(psideedges[i], pnode->children[i], psurf);
                    }
                }
            }
        }
    }

    public static void R_DrawSolidClippedSubmodelPolygons(model_c.model_t* pmodel)
    {
        int i, j, lindex;
        float dot;
        model_c.msurface_t* psurf;
        int numsurfaces;
        model_c.mplane_t* pplane;
        model_c.mvertex_t bverts;
        r_local_c.bedge_t bedges;
        r_local_c.bedge_t* pbedge;
        model_c.medge_t* pedge, pedges;

        psurf = pmodel->surfaces[pmodel->firstmodelsurface];
        numsurfaces = pmodel->nummodelsurfaces;
        pedges = pmodel->edges;

        for (i = 0; i < numsurfaces; i++, psurf++)
        {
            pplane = psurf->plane;

            dot = mathlib_c.DotProduct_V(modelorg, pplane->normal) - pplane->dist;

            if (((psurf->flags & model_c.SURF_PLANEBACK) != 0 && (dot < -r_local_c.BACKFACE_EPSILON)) || ((psurf->flags & model_c.SURF_PLANEBACK) == 0 && (dot > r_local_c.BACKFACE_EPSILON)))
            {
                pbverts = bverts;
                pbedges = bedges;
                numbverts = numbedges = 0;

                if (psurf->numedges > 0)
                {
                    pbedge = bedges[numbedges];
                    numbedges += psurf->numedges;

                    for (j = 0; j < psurf->numedges; j++)
                    {
                        lindex = pmodel->surfedges[psurf->firstedge + j];

                        if (lindex > 0)
                        {
                            pedge = pedges[lindex];
                            pbedge[j].v[0] = r_main_c.r_pcurrentvertbase[pedge->v[0]];
                            pbedge[j].v[1] = r_main_c.r_pcurrentvertbase[pedge->v[1]];
                        }
                        else
                        {
                            lindex = -lindex;
                            pedge = pedges[lindex];
                            pbedge[j].v[0] = r_main_c.r_pcurrentvertbase[pedge->v[1]];
                            pbedge[j].v[1] = r_main_c.r_pcurrentvertbase[pedge->v[0]];
                        }

                        pbedge[j].pnext = pbedge[j + 1];
                    }

                    pbedge[j - 1].pnext = null;

                    R_RecursiveClipBPoly(pbedge, currententity->topnode, psurf);
                }
                else
                {
                    sys_win_c.Sys_Error("no edges in bmodel");
                }
            }
        }
    }

    public static void R_DrawSubmodelPolygons(model_c.model_t* pmodel, int clipflags)
    {
        int i;
        float dot;
        model_c.msurface_t* psurf;
        int numsurfaces;
        model_c.mplane_t* pplane;

        psurf = pmodel->surfaces[pmodel->firstmodelsurface];
        numsurfaces = pmodel->nummodelsurfaces;

        for (i = 0; i < numsurfaces; i++, psurf++)
        {
            pplane = psurf->plane;

            dot = mathlib_c.DotProduct_V(modelorg, pplane->normal) - pplane->dist;

            if (((psurf->flags & model_c.SURF_PLANEBACK) != 0 && (dot < -r_local_c.BACKFACE_EPSILON)) || ((psurf->flags & model_c.SURF_PLANEBACK) == 0 && (dot > r_local_c.BACKFACE_EPSILON)))
            {
                r_edge_c.r_currentkey = ((model_c.mleaf_t*)currententity->topnode)->key;

                r_draw_c.R_RenderFace(psurf, clipflags);
            }
        }
    }

    public void R_RecursiveWorldNode(model_c.mnode_t* node, int clipflags)
    {
        int i, c, side;
        int* pindex;
        Vector3 acceptpt = new Vector3(0.0f), rejectpt = new Vector3(0.0f);
        model_c.mplane_t* plane;
        model_c.msurface_t* surf;
        model_c.msurface_t** mark;
        model_c.mleaf_t* pleaf;
        double d, dot;

        if (node->contents == bspfile_c.CONTENTS_SOLID)
        {
            return;
        }

        if (node->visframe != r_main_c.r_visframecount)
        {
            return;
        }

        if (clipflags != 0)
        {
            for (i = 0; i < 4; i++)
            {
                if ((clipflags & (1<<i)) == 0)
                {
                    continue;
                }

                pindex = &r_main_c.pfrustum_indexes[i];

                rejectpt[0] = (float)node->minmaxs[pindex[0]];
                rejectpt[1] = (float)node->minmaxs[pindex[1]];
                rejectpt[2] = (float)node->minmaxs[pindex[2]];

                d = mathlib_c.DotProduct_FV(rejectpt, r_draw_c.view_clipplanes[i].normal);
                d -= r_draw_c.view_clipplanes[i].dist;

                if (d <= 0)
                {
                    return;
                }

                acceptpt[0] = (float)node->minmaxs[pindex[3 + 0]];
                acceptpt[1] = (float)node->minmaxs[pindex[3 + 1]];
                acceptpt[2] = (float)node->minmaxs[pindex[3 + 2]];

                d = mathlib_c.DotProduct_FV(acceptpt, r_draw_c.view_clipplanes[i].normal);
                d -= r_draw_c.view_clipplanes[i].dist;

                if (d >= 0)
                {
                    clipflags &= ~(1 << i);
                }
            }
        }

        if (node->contents < 0)
        {
            pleaf = (model_c.mleaf_t*)node;

            mark = pleaf->firstmarksurface;
            c = pleaf->nummarksurfaces;

            if (c != 0)
            {
                do
                {
                    (*mark)->visframe = r_main_c.r_framecount;
                    mark++;
                } while (c-- != 0);
            }

            if (pleaf->efrags != 0)
            {
                r_local_c.R_StoreEfrags(pleaf->efrags);
            }

            pleaf->key = r_edge_c.r_currentkey;
            r_edge_c.r_currentkey++;
        }
        else
        {
            plane = node->plane;

            switch (plane->type)
            {
                case PLANE_X:
                    dot = modelorg[0] - plane->dist;
                    break;

                case PLANE_Y:
                    dot = modelorg[1] - plane->dist;
                    break;

                case PLANE_Z:
                    dot = modelorg[2] - plane->dist;
                    break;

                default:
                    dot = mathlib_c.DotProduct_V(modelorg, plane->normal) - plane->dist;
                    break;
            }

            if (dot >= 0)
            {
                side = 0;
            }
            else
            {
                side = 1;
            }

            R_RecursiveWorldNode(node->children[side], clipflags);

            c = node->numsurfaces;

            if (c != 0)
            {
                surf = cl_main_c.cl.worldmodel->surfaces + node->firstsurface;

                if (dot < -r_local_c.BACKFACE_EPSILON)
                {
                    do
                    {
                        if ((surf->flags & model_c.SURF_PLANEBACK) != 0 && (surf->visframe == d_iface_c.r_framecount))
                        {
                            if (r_main_c.r_drawpolys)
                            {
                                if (r_main_c.r_worldpolysbacktofront)
                                {
                                    if (r_main_c.numbtofpolys < r_local_c.MAX_BTOFPOLYS)
                                    {
                                        r_main_c.pbtofpolys[r_main_c.numbtofpolys].clipflags = clipflags;
                                        r_main_c.pbtofpolys[r_main_c.numbtofpolys].psurf = surf;
                                        r_main_c.numbtofpolys++;
                                    }
                                }
                                else
                                {
                                    r_draw_c.R_RenderPoly(surf, clipflags);
                                }
                            }
                            else
                            {
                                r_draw_c.R_RenderFace(surf, clipflags);
                            }
                        }

                        surf++;
                    } while (c-- != 0);
                }
                else if (dot > r_local_c.BACKFACE_EPSILON)
                {
                    do
                    {
                        if ((surf->flags & model_c.SURF_PLANEBACK) == 0 && (surf->visframe == d_iface_c.r_framecount))
                        {
                            if (r_main_c.r_drawpolys)
                            {
                                if (r_main_c.r_worldpolysbacktofront)
                                {
                                    if (r_main_c.numbtofpolys < r_local_c.MAX_BTOFPOLYS)
                                    {
                                        r_main_c.pbtofpolys[r_main_c.numbtofpolys].clipflags = clipflags;
                                        r_main_c.pbtofpolys[r_main_c.numbtofpolys].psurf = surf;
                                        r_main_c.numbtofpolys++;
                                    }
                                }
                                else
                                {
                                    r_draw_c.R_RenderPoly(surf, clipflags);
                                }
                            }
                            else
                            {
                                r_draw_c.R_RenderFace(surf, clipflags);
                            }
                        }

                        surf++;
                    } while (c-- != 0);
                }

                r_edge_c.r_currentkey++;
            }

            R_RecursiveWorldNode(node->children[side == 0], clipflags);
        }
    }

    public static void R_RenderWorld()
    {
        int i;
        model_c.model_t* clmodel;
        r_local_c.btofpoly_t btofpolys;

        r_main_c.pbtofpolys = btofpolys;

        currententity = &cl_main_c.cl_entities[0];
        mathlib_c.VectorCopy(r_main_c.r_origin, modelorg);
        clmodel = currententity->model;
        r_main_c.r_pcurrentvertbase = clmodel->vertexes;

        R_RecursiveWorldNode(clmodel->nodes, 15);

        if (r_main_c.r_worldpolysbacktofront)
        {
            for (i = r_main_c.numbtofpolys - 1; i >= 0; i--)
            {
                r_draw_c.R_RenderPoly(btofpolys[i].psurf, btofpolys[i].clipflags);
            }
        }
    }
}