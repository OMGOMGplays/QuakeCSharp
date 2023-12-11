namespace Quake.Server;

public unsafe class world_c
{
    // world.h
    public struct plane_t
    {
        public Vector3 normal;
        public float dist;
    }

    public struct trace_t
    {
        public bool allsolid;
        public bool startsolid;
        public bool inopen, inwater;
        public float fraction;
        public Vector3 endpos;
        public plane_t plane;
        public progs_c.edict_t* ent;
    }

    public static int MOVE_NORMAL = 0;
    public static int MOVE_NOMONSTERS = 1;
    public static int MOVE_MISSILE = 2;

    public struct areanode_t
    {
        public int axis;
        public float dist;
        public areanode_t* children;
        public common_c.link_t trigger_edicts;
        public common_c.link_t solid_edicts;
    }

    public static int AREA_DEPTH = 4;
    public static int AREA_NODES = 32;

    // world.c
    public struct moveclip_t
    {
        public Vector3 boxmins, boxmaxs;
        public float* mins, maxs;
        public Vector3 mins2, maxs2;
        public float* start, end;
        public trace_t trace;
        public int type;
        public progs_c.edict_t* passedict;
    }

    public static model_c.hull_t box_hull;
    public static bspfile_c.dclipnode_t* box_clipnodes;
    public static model_c.mplane_t* box_planes;

    public void SV_InitBoxHull()
    {
        int i;
        int side;

        box_hull.clipnodes = box_clipnodes;
        box_hull.planes = box_planes;
        box_hull.firstclipnode = 0;
        box_hull.lastclipnode = 5;

        for (i = 0; i < 6; i++)
        {
            box_clipnodes[i].planenum = i;

            side = i & 1;

            box_clipnodes[i].children[side] = bspfile_c.CONTENTS_EMPTY;

            if (i != 5)
            {
                box_clipnodes[i].children[side ^ 1] = i + 1;
            }
            else
            {
                box_clipnodes[i].children[side ^ 1] = bspfile_c.CONTENTS_SOLID;
            }

            box_planes[i].type >>= 1;
            box_planes[i].normal[i >> 1] = 1;
        }
    }

    public model_c.hull_t* SV_HullForBox(Vector3 mins, Vector3 maxs)
    {
        box_planes[0].dist = maxs[0];
        box_planes[1].dist = mins[0];
        box_planes[2].dist = maxs[1];
        box_planes[3].dist = mins[1];
        box_planes[4].dist = maxs[2];
        box_planes[5].dist = mins[2];

        return &box_hull;
    }

    public model_c.hull_t* SV_HullForEntity(progs_c.edict_t* ent, Vector3 mins, Vector3 maxs, Vector3 offset)
    {
        model_c.model_t* model;
        Vector3 size = new Vector3(0.0f);
        Vector3 hullmins = new Vector3(), hullmaxs = new Vector3();
        model_c.hull_t* hull;

        if (ent->v.solid == server_c.SOLID_BSP)
        {
            if (ent->v.movetype != server_c.MOVETYPE_PUSH)
            {
                sv_main_c.SV_Error(common_c.StringToChar("SOLID_BSP without MOVETYPE_PUSH"));
            }

            model = sv.models[(int)ent->v.modelindex];

            if (model == null || model->type != model_c.modtype_t.mod_brush)
            {
                sv_main_c.SV_Error(common_c.StringToChar("MOVETYPE_PUSH with a non bsp model"));
            }

            mathlib_c.VectorSubtract(maxs, mins, size);

            if (size[0] < 3)
            {
                hull = &model->hulls[0];
            }
            else if (size[0] <= 32)
            {
                hull = &model->hulls[1];
            }
            else
            {
                hull = &model->hulls[2];
            }

            mathlib_c.VectorSubtract(hull->clip_mins, mins, offset);
            mathlib_c.VectorAdd(offset, ent->v.origin, offset);
        }
        else
        {
            mathlib_c.VectorSubtract(ent->v.mins, maxs, hullmins);
            mathlib_c.VectorSubtract(ent->v.maxs, mins, hullmaxs);
            hull = SV_HullForBox(hullmins, hullmaxs);

            mathlib_c.VectorCopy(ent->v.origin, offset);
        }

        return hull;
    }

    public areanode_t* sv_areanodes;
    public int sv_numareanodes;

    public areanode_t* SV_CreateAreaNode(int depth, Vector3 mins, Vector3 maxs)
    {
        areanode_t* anode;
        Vector3 size;
        Vector3 mins1 = new(), maxs1 = new(), mins2 = new(), maxs2 = new();

        anode = &sv_areanodes[sv_numareanodes];
        sv_numareanodes++;

        common_c.ClearLink(&anode->trigger_edicts);
        common_c.ClearLink(&anode->solid_edicts);

        if (depth == AREA_DEPTH)
        {
            anode->axis = -1;
            anode->children[0] = anode->children[1] = null;

            return anode;
        }

        mathlib_c.VectorSubtract(maxs, mins, size);

        if (size[0] > size[1])
        {
            anode->axis = 0;
        }
        else
        {
            anode->axis = 1;
        }

        anode->dist = 0.5f * (maxs[anode->axis] + mins[anode->axis]);
        mathlib_c.VectorCopy(mins, mins1);
        mathlib_c.VectorCopy(mins, mins2);
        mathlib_c.VectorCopy(maxs, maxs1);
        mathlib_c.VectorCopy(maxs, maxs2);

        maxs1[anode->axis] = mins2[anode->axis] = anode->dist;

        anode->children[0] = *SV_CreateAreaNode(depth + 1, mins2, maxs2);
        anode->children[1] = *SV_CreateAreaNode(depth + 1, mins1, maxs1);

        return anode;
    }

    public void SV_ClearWorld()
    {
        SV_InitBoxHull();

        common_c.Q_memset(&sv_areanodes, 0, AREA_NODES);
        sv_numareanodes = 0;
        SV_CreateAreaNode(0, sv.worldmodel->mins, sv.worldmodel->maxs);
    }

    public void SV_UnlinkEdict(progs_c.edict_t* ent)
    {
        if (ent->area.prev == null)
        {
            return;
        }

        common_c.RemoveLink(&ent->area);
        ent->area.prev = ent->area.next = null;
    }

    public void SV_TouchLinks(progs_c.edict_t* ent, areanode_t* node)
    {
        common_c.link_t* l, next = null;
        progs_c.edict_t* touch;
        int old_self, old_other;

        for (l = node->trigger_edicts.next; l != &node->trigger_edicts; l = next)
        {
            next = l->next;
            touch = progs_c.EDICT_FROM_AREA(1);

            if (touch == ent)
            {
                continue;
            }

            if (touch->v.touch == null || touch->v.solid != server_c.SOLID_TRIGGER)
            {
                continue;
            }

            if (ent->v.absmin[0] > touch->v.absmax[0] || ent->v.absmin[1] > touch->v.absmax[1] || ent->v.absmin[2] > touch->v.absmax[2] || ent->v.absmax[0] < touch->v.absmin[0] || ent->v.absmax[1] < touch->v.absmin[1] || ent->v.absmax[2] < touch->v.absmin[2])
            {
                continue;
            }

            old_self = progs_c.pr_global_struct->self;
            old_other = progs_c.pr_global_struct->other;

            progs_c.pr_global_struct->self = progs_c.EDICT_TO_PROG(touch);
            progs_c.pr_global_struct->other = progs_c.EDICT_TO_PROG(ent);
            progs_c.pr_global_struct->time = sv.time;
            pr_exec_c.PR_ExecuteProgram(touch->v.touch);

            progs_c.pr_global_struct->self = old_self;
            progs_c.pr_global_struct->other = old_other;
        }

        if (node->axis == -1)
        {
            return;
        }

        if (ent->v.absmax[node->axis] > node->dist)
        {
            SV_TouchLinks(ent, &node->children[0]);
        }

        if (ent->v.absmin[node->axis] < node->dist)
        {
            SV_TouchLinks(ent, &node->children[1]);
        }
    }

    public void SV_FindTouchedLeafs(progs_c.edict_t* ent, model_c.mnode_t* node)
    {
        model_c.mplane_t* splitplane;
        model_c.mleaf_t* leaf;
        int sides;
        int leafnum;

        if (node->contents == bspfile_c.CONTENTS_SOLID)
        {
            return;
        }

        if (node->contents < 0)
        {
            if (ent->num_leafs == progs_c.MAX_ENT_LEAFS)
            {
                return;
            }

            leaf = (model_c.mleaf_t*)node;
            leafnum = leaf - sv.worldmodel->leafs - 1;

            ent->leafnums[ent->num_leafs] = leafnum;
            ent->num_leafs++;
            return;
        }

        splitplane = node->plane;
        sides = mathlib_c.BoxOnPlaneSide(ent->v.absmin, ent->v.absmix, splitplane);

        if ((sides & 1) != 0)
        {
            SV_FindTouchedLeafs(ent, &node->children[0]);
        }

        if ((sides & 2) != 0)
        {
            SV_FindTouchedLeafs(ent, &node->children[1]);
        }
    }

    public void SV_LinkEdict(progs_c.edict_t* ent, bool touch_triggers)
    {
        areanode_t* node;

        if (ent->area.prev != null)
        {
            SV_UnlinkEdict(ent);
        }

        if (ent == sv.edicts)
        {
            return;
        }

        if (ent->free)
        {
            return;
        }

        mathlib_c.VectorAdd(ent->v.origin, ent->v.mins, ent->v.absmin);
        mathlib_c.VectorAdd(ent->v.origin, ent->v.maxs, ent->v.absmax);

        if (((int)ent->v.flags & server_c.FL_ITEM) != 0)
        {
            ent->v.absmin[0] -= 15;
            ent->v.absmin[1] -= 15;
            ent->v.absmax[0] += 15;
            ent->v.absmax[1] += 15;
        }
        else
        {
            ent->v.absmin[0] -= 1;
            ent->v.absmin[1] -= 1;
            ent->v.absmin[2] -= 1;
            ent->v.absmax[0] += 1;
            ent->v.absmax[1] += 1;
            ent->v.absmax[2] += 1;
        }

        ent->num_leafs = 0;

        if (ent->v.modelindex != 0)
        {
            SV_FindTouchedLeafs(ent, sv.worldmodel->nodes);
        }

        if (ent->v.solid == server_c.SOLID_NOT)
        {
            return;
        }

        node = sv_areanodes;

        while (true)
        {
            if (node->axis == -1)
            {
                break;
            }

            if (ent->v.absmin[node->axis] > node->dist)
            {
                node = &node->children[0];
            }
            else if (ent->v.absmax[node->axis] < node->dist)
            {
                node = &node->children[1];
            }
            else
            {
                break;
            }
        }

        if (ent->v.solid == server_c.SOLID_TRIGGER)
        {
            common_c.InsertLinkBefore(&ent->area, &node->trigger_edicts);
        }
        else
        {
            common_c.InsertLinkBefore(&ent->area, &node->solid_edicts);
        }

        if (touch_triggers)
        {
            SV_TouchLinks(ent, sv_areanodes);
        }
    }

#if !id386

    public int SV_HullPointContents(model_c.hull_t* hull, int num, Vector3 p)
    {
        float d;
        bspfile_c.dclipnode_t* node;
        model_c.mplane_t* plane;

        while (num >= 0)
        {
            if (num < hull->firstclipnode || num > hull->lastclipnode)
            {
                sv_main_c.SV_Error(common_c.StringToChar("SV_HullPointContents: bad node number"));
            }

            node = hull->clipnodes + num;
            plane = hull->planes + node->planenum;

            if (plane->type < 3)
            {
                d = p[plane->type] - plane->dist;
            }
            else
            {
                d = mathlib_c.DotProduct_V(plane->normal, p) - plane->dist;
            }

            if (d < 0)
            {
                num = node->children[1];
            }
            else
            {
                num = node->children[0];
            }
        }

        return num;
    }

#endif

    public int SV_PointContents(Vector3 p)
    {
        return SV_HullPointContents(&sv.worldmodel->hulls[0], 0, p);
    }

    public progs_c.edict_t* SV_TestEntityPosition(progs_c.edict_t* ent)
    {
        trace_t trace;

        trace = SV_Move(ent->v.origin, ent->v.mins, ent->v.maxs, ent->v.origin, 0, ent);

        if (trace.startsolid)
        {
            return sv.edicts;
        }

        return null;
    }

    public static float DIST_EPSILON = 0.03125f;

    public bool SV_RecursiveHullCheck(model_c.hull_t* hull, int num, float p1f, float p2f, Vector3 p1, Vector3 p2, trace_t* trace)
    {
        bspfile_c.dclipnode_t* node;
        model_c.mplane_t* plane;
        float t1, t2;
        float frac;
        int i;
        Vector3 mid = new();
        int side = new();
        float midf;

        if (num < 0)
        {
            if (num != bspfile_c.CONTENTS_SOLID)
            {
                trace->allsolid = false;

                if (num == bspfile_c.CONTENTS_EMPTY)
                {
                    trace->inopen = true;
                }
                else
                {
                    trace->inwater = true;
                }
            }
            else
            {
                trace->startsolid = true;
            }

            return true;
        }

        if (num < hull->firstclipnode || num > hull->lastclipnode)
        {
            sv_main_c.SV_Error(common_c.StringToChar("SV_RecursiveHullCheck: bad node number"));
        }

        node = hull->clipnodes + num;
        plane = hull->planes + node->planenum;

        if (plane->type < 3)
        {
            t1 = p1[plane->type] - plane->dist;
            t2 = p2[plane->type] - plane->dist;
        }
        else
        {
            t1 = mathlib_c.DotProduct_V(plane->normal, p1) - plane->dist;
            t2 = mathlib_c.DotProduct_V(plane->normal, p2) - plane->dist;
        }

        if (t1 >= 0 && t2 >= 0)
        {
            return SV_RecursiveHullCheck(hull, node->children[0], p1f, p2f, p1, p2, trace);
        }

        if (t1 < 0 && t2 < 0)
        {
            return SV_RecursiveHullCheck(hull, node->children[1], p1f, p2f, p1, p2, trace);
        }

        if (t1 < 0)
        {
            frac = (t1 + DIST_EPSILON) / (t1 - t2);
        }
        else
        {
            frac = (t1 - DIST_EPSILON) / (t1 - t2);
        }

        if (frac < 0)
        {
            frac = 0;
        }

        if (frac > 1)
        {
            frac = 1;
        }

        midf = p1f + (p2f - p1f) * frac;

        for (i = 0; i < 3; i++)
        {
            mid[i] = p1[i] + frac * (p2[i] - p1[i]);
        }

        //side = (t1 < 0);

        if (!SV_RecursiveHullCheck(hull, node->children[side], p1f, midf, p1, mid, trace))
        {
            return false;
        }

#if PARANOID
        if (SV_HullPointContents(sv_hullmodel, mid, node->children[side]) == bspfile_c.CONTENTS_SOLID)
        {
            console_c.Con_Printf("mid PointInHullSolid\n");
            return false;
        }
#endif

        if (SV_HullPointContents(hull, node->children[side ^ 1], mid) != bspfile_c.CONTENTS_SOLID)
        {
            return SV_RecursiveHullCheck(hull, node->children[side ^ 1], midf, p2f, mid, p2, trace);
        }

        if (trace->allsolid)
        {
            return false;
        }

        if (side == 0)
        {
            mathlib_c.VectorCopy(plane->normal, trace->plane.normal);
            trace->plane.dist = plane->dist;
        }
        else
        {
            mathlib_c.VectorSubtract(mathlib_c.vec3_origin, plane->normal, trace->plane.normal);
            trace->plane.dist = -plane->dist;
        }

        while (SV_HullPointContents(hull, hull->firstclipnode, mid) == bspfile_c.CONTENTS_SOLID)
        {
            frac -= 0.1f;

            if (frac < 0)
            {
                trace->fraction = midf;
                mathlib_c.VectorCopy(mid, trace->endpos);
                console_c.Con_Printf("backup past 0\n");
                return false;
            }

            midf = p1f + (p2f - p1f) * frac;

            for (i = 0; i < 3; i++)
            {
                mid[i] = p1[i] + frac * (p2[i] - p1[i]);
            }
        }

        trace->fraction = midf;
        mathlib_c.VectorCopy(mid, trace->endpos);

        return false;
    }

    public trace_t SV_ClipMoveToEntity(progs_c.edict_t* ent, Vector3 start, Vector3 mins, Vector3 maxs, Vector3 end)
    {
        trace_t trace = new();
        Vector3 offset = new();
        Vector3 start_l = new(), end_l = new();
        model_c.hull_t* hull;

        common_c.Q_memset(trace, 0, sizeof(trace_t));
        trace.fraction = 1;
        trace.allsolid = true;
        mathlib_c.VectorCopy(end, trace.endpos);

        hull = SV_HullForEntity(ent, mins, maxs, offset);

        mathlib_c.VectorSubtract(start, offset, start_l);
        mathlib_c.VectorSubtract(end, offset, end_l);

        SV_RecursiveHullCheck(hull, hull->firstclipnode, 0, 1, start_l, end_l, &trace);

        if (trace.fraction != 1)
        {
            mathlib_c.VectorAdd(trace.endpos, offset, trace.endpos);
        }

        if (trace.fraction < 1 || trace.startsolid)
        {
            trace.ent = ent;
        }

        return trace;
    }

    public void SV_ClipToLinks(areanode_t* node, moveclip_t* clip)
    {
        common_c.link_t* l, next = null;
        progs_c.edict_t* touch;
        trace_t trace;

        for (l = node->solid_edicts.next; l != &node->solid_edicts; l = next)
        {
            next = l->next;

            touch = progs_c.EDICT_FROM_AREA(1);

            if (touch->v.solid == server_c.SOLID_NOT)
            {
                continue;
            }

            if (touch == clip->passedict)
            {
                continue;
            }

            if (touch->v.solid == server_c.SOLID_TRIGGER)
            {
                sv_main_c.SV_Error(common_c.StringToChar("Trigger in clipping list"));
            }

            if (clip->type == MOVE_NOMONSTERS && touch->v.solid != server_c.SOLID_BSP)
            {
                continue;
            }

            if (clip->boxmins[0] > touch->v.absmax[0] || clip->boxmins[1] > touch->v.absmax[1] || clip->boxmins[2] > touch->v.absmax[2] || clip->boxmaxs[0] < touch->v.absmin[0] || clip->boxmaxs[1] < touch->v.absmin[1] || clip->boxmaxs[2] < touch->v.absmin[2])
            {
                continue;
            }

            if (clip->passedict != null && clip->passedict->v.size[0] != null && touch->v.size[0] == null)
            {
                continue;
            }

            if (clip->trace.allsolid)
            {
                return;
            }

            if (clip->passedict != null)
            {
                if (progs_c.PROG_TO_EDICT(touch->v.owner) == clip->passedict)
                {
                    continue;
                }

                if (progs_c.PROG_TO_EDICT(clip->passedict->v.owner) == touch)
                {
                    continue;
                }
            }

            if (((int)touch->v.flags & server_c.FL_MONSTER) != 0)
            {
                trace = SV_ClipMoveToEntity(touch, common_c.FloatToVector(clip->start), clip->mins2, clip->maxs2, common_c.FloatToVector(clip->end));
            }
            else
            {
                trace = SV_ClipMoveToEntity(touch, common_c.FloatToVector(clip->start), common_c.FloatToVector(clip->mins), common_c.FloatToVector(clip->maxs), common_c.FloatToVector(clip->end));
            }

            if (trace.allsolid || trace.startsolid || trace.fraction < clip->trace.fraction)
            {
                trace.ent = touch;

                if (clip->trace.startsolid)
                {
                    clip->trace = trace;
                    clip->trace.startsolid = true;
                }
                else
                {
                    clip->trace = trace;
                }
            }
            else if (trace.startsolid)
            {
                clip->trace.startsolid = true;
            }
        }

        if (node->axis == -1)
        {
            return;
        }

        if (clip->boxmaxs[node->axis] > node->dist)
        {
            SV_ClipToLinks(&node->children[0], clip);
        }

        if (clip->boxmins[node->axis] < node->dist)
        {
            SV_ClipToLinks(&node->children[1], clip);
        }
    }

    public void SV_MoveBounds(Vector3 start, Vector3 mins, Vector3 maxs, Vector3 end, Vector3 boxmins, Vector3 boxmaxs)
    {
        //boxmins[0] = boxmins[1] = boxmins[2] = -9999;
        //boxmaxs[0] = boxmaxs[1] = boxmaxs[2] = 9999;

        int i;

        for (i = 0; i < 3; i++)
        {
            if (end[i] > start[i])
            {
                boxmins[i] = start[i] + mins[i] - 1;
                boxmaxs[i] = end[i] + maxs[i] + 1;
            }
            else
            {
                boxmins[i] = end[i] + mins[i] - 1;
                boxmaxs[i] = start[i] + maxs[i] + 1;
            }
        }
    }

    public trace_t SV_Move(Vector3 start, Vector3 mins, Vector3 maxs, Vector3 end, int type, progs_c.edict_t* passedict)
    {
        moveclip_t clip;
        int i;

        common_c.Q_memset(&clip, 0, sizeof(moveclip_t));

        clip.trace = SV_ClipMoveToEntity(sv.edicts, start, mins, maxs, end);

        clip.start = common_c.VectorToFloat(start);
        clip.end = common_c.VectorToFloat(end);
        clip.mins = common_c.VectorToFloat(mins);
        clip.maxs = common_c.VectorToFloat(maxs);
        clip.type = type;
        clip.passedict = passedict;

        if (type == MOVE_MISSILE)
        {
            for (i = 0; i < 3; i++)
            {
                clip.mins2[i] = -15;
                clip.maxs2[i] = 15;
            }
        }
        else
        {
            mathlib_c.VectorCopy(mins, clip.mins2);
            mathlib_c.VectorCopy(maxs, clip.maxs2);
        }

        SV_MoveBounds(start, clip.mins2, clip.maxs2, end, clip.boxmins, clip.boxmaxs);

        SV_ClipToLinks(sv_areanodes, &clip);

        return clip.trace;
    }

    public progs_c.edict_t* SV_TestPlayerPosition(progs_c.edict_t* ent, Vector3 origin)
    {
        model_c.hull_t* hull;
        progs_c.edict_t* check;
        Vector3 boxmins = new(), boxmaxs = new();
        Vector3 offset;
        int e;

        hull = &sv.worldmodel->hulls[1];

        if (SV_HullPointContents(hull, hull->firstclipnode, origin) != bspfile_c.CONTENTS_EMPTY)
        {
            return sv.edicts;
        }

        mathlib_c.VectorAdd(origin, ent->v.mins, boxmins);
        mathlib_c.VectorAdd(origin, ent->v.maxs, boxmaxs);

        check = progs_c.NEXT_EDICT(sv.edicts);

        for (e = 1; e < sv.num_edicts; e++, check = progs_c.NEXT_EDICT(check))
        {
            if (check->free)
            {
                continue;
            }

            if (check->v.solid != server_c.SOLID_BSP && check->v.solid != server_c.SOLID_BBOX && check->v.solid != server_c.SOLID_SLIDEBOX)
            {
                continue;
            }

            if (boxmins[0] > check->v.absmax[0] || boxmins[1] > check->v.absmax[1] || boxmins[2] > check->v.absmax[2] || boxmaxs[0] < check->v.absmin[0] || boxmaxs[1] < check->v.absmin[1] || boxmaxs[2] < check->v.absmin[2])
            {
                continue;
            }

            if (check == ent)
            {
                continue;
            }

            hull = SV_HullForEntity(check, ent->v.mins, ent->v.maxs, offset);

            mathlib_c.VectorSubtract(origin, offset, offset);

            if (SV_HullPointContents(hull, hull->firstclipnode, offset) != bspfile_c.CONTENTS_EMPTY)
            {
                return check;
            }
        }

        return null;
    }
}