namespace Quake;

public unsafe class world_c
{
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

    public const int MOVE_NORMAL = 0;
    public const int MOVE_NOMONSTERS = 1;
    public const int MOVE_MISSILE = 2;

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

    public static model_c.hull_t* box_hull;
    public static bspfile_c.dclipnode_t* box_clipnodes;
    public static model_c.mplane_t* box_planes;

    public static void SV_InitBoxHull()
    {
        int i;
        int side;

        box_hull->clipnodes = box_clipnodes;
        box_hull->planes = box_planes;
        box_hull->firstclipnode = 0;
        box_hull->lastclipnode = 5;

        for (i = 0; i < 6; i++)
        {
            box_clipnodes[i].planenum = i;

            side = i & 1;

            box_clipnodes[i].children[side] = bspfile_c.CONTENTS_EMPTY;

            if (i != 5)
            {
                box_clipnodes[i].children[side ^ 1] = (short)(i + 1);
            }
            else
            {
                box_clipnodes[i].children[side ^ 1] = bspfile_c.CONTENTS_SOLID;
            }

            box_planes[i].type = (byte)(i >> 1);
            box_planes[i].normal[i >> 1] = 1;
        }
    }

    public static model_c.hull_t* SV_HullForBox(Vector3 mins, Vector3 maxs)
    {
        box_planes[0].dist = maxs[0];
        box_planes[1].dist = mins[0];
        box_planes[2].dist = maxs[1];
        box_planes[3].dist = mins[1];
        box_planes[4].dist = maxs[2];
        box_planes[5].dist = mins[2];

        return box_hull;
    }

    public static model_c.hull_t* SV_HullForEntity(progs_c.edict_t* ent, Vector3 mins, Vector3 maxs, Vector3 offset)
    {
        model_c.model_t* model;
        Vector3 size;
        Vector3 hullmins, hullmaxs;
        model_c.hull_t* hull;

        size = hullmins = hullmaxs = new();

        if (ent->v.solid == server_c.SOLID_BSP)
        {
            if (ent->v.movetype == server_c.MOVETYPE_PUSH)
            {
                sys_win_c.Sys_Error("SOLID_BSP without MOVETYPE_PUSH");
            }

            model = &server_c.sv.models[(int)ent->v.modelindex];

            if (model == null || model->type != model_c.modtype_t.mod_brush)
            {
                sys_win_c.Sys_Error("MOVETYPE_PUSH with a non bsp model");
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

    public struct areanode_t
    {
        public int axis;
        public float dist;
        public areanode_t* children;
        public common_c.link_t trigger_edicts;
        public common_c.link_t solid_edicts;
    }

    public const int AREA_DEPTH = 4;
    public const int AREA_NODES = 32;

    public static areanode_t* sv_areanodes;
    public static int sv_numareanodes;

    public static areanode_t* SV_CreateAreaNode(int depth, Vector3 mins, Vector3 maxs)
    {
        areanode_t* anode;
        Vector3 size;
        Vector3 mins1, maxs1, mins2, maxs2;

        size = mins1 = maxs1 = mins2 = maxs2 = new();

        anode = &sv_areanodes[sv_numareanodes];
        sv_numareanodes++;

        common_c.ClearLink(&anode->trigger_edicts);
        common_c.ClearLink(&anode->solid_edicts);

        if (depth == AREA_DEPTH)
        {
            anode->axis = -1;
            anode->children[0] = anode->children[1] = default;
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

    public static void SV_ClearWorld()
    {
        SV_InitBoxHull();

        memset_c.memset((object*)sv_areanodes, 0, (nint)sv_areanodes);
        sv_numareanodes = 0;
        SV_CreateAreaNode(0, server_c.sv.worldmodel->mins, server_c.sv.worldmodel->maxs);
    }

    public static void SV_UnlinkEdict(progs_c.edict_t* ent)
    {
        if (ent->area.prev == null)
        {
            return;
        }

        common_c.RemoveLink(ent->area);
        ent->area.prev = ent->area.next = null;
    }

    public static void SV_TouchLinks(progs_c.edict_t* ent, areanode_t* node)
    {
        common_c.link_t* l, next;
        progs_c.edict_t* touch;
        int old_self, old_other;

        next = default;
        touch = default;

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

            old_self = pr_edict_c.pr_global_struct->self;
            old_other = pr_edict_c.pr_global_struct->other;

            pr_edict_c.pr_global_struct->self = progs_c.EDICT_TO_PROG(touch);
            pr_edict_c.pr_global_struct->other = progs_c.EDICT_TO_PROG(ent);
            pr_edict_c.pr_global_struct->time = (float)server_c.sv.time;
            pr_exec_c.PR_ExecuteProgram(touch->v.touch);

            pr_edict_c.pr_global_struct->self = old_self;
            pr_edict_c.pr_global_struct->other = old_other;
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

    public static void SV_FindTouchedLeafs(progs_c.edict_t* ent, model_c.mnode_t* node)
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
            leafnum = (int)(leaf - server_c.sv.worldmodel->leafs - 1);

            ent->leafnums[ent->num_leafs] = (short)leafnum;
            ent->num_leafs++;
            return;
        }

        splitplane = node->plane;
        sides = mathlib_c.BoxOnPlaneSide(ent->v.absmin, ent->v.absmax, splitplane);

        if ((sides & 1) != 0)
        {
            SV_FindTouchedLeafs(ent, &node->children[0]);
        }

        if ((sides & 2) != 0)
        {
            SV_FindTouchedLeafs(ent, &node->children[1]);
        }
    }

    public static void SV_LinkEdict(progs_c.edict_t* ent, bool touch_triggers)
    {
        areanode_t* node;

        if (ent->area.prev != null)
        {
            SV_UnlinkEdict(ent);
        }

        if (ent == server_c.sv.edicts)
        {
            return;
        }

        if (ent->free)
        {
            return;
        }

#if QUAKE2
        if (ent->v.solid == server_c.SOLID_BSP && (ent->v.angles[0] != 0 || ent->v.angles[1] != 0 || ent->v.angles[2] != 0))
        {
            float max, v;
            int i;

            max = 0;

            for (i = 0; i < 3; i++)
            {
                v = MathF.Abs(ent->v.mins[i]);

                if (v > max)
                {
                    max = v;
                }

                v = MathF.Abs(ent->v.maxs[i]);

                if (v > max)
                {
                    max = v;
                }
            }

            for (i = 0; i < 3; i++)
            {
                ent->v.absmin[i] = ent->v.origin[i] - max;
                ent->v.absmax[i] = ent->v.origin[i] + max;
            }
        }
        else
#endif
        {
            mathlib_c.VectorAdd(ent->v.origin, ent->v.mins, ent->v.absmin);
            mathlib_c.VectorAdd(ent->v.origin, ent->v.maxs, ent->v.absmax);
        }

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
            SV_FindTouchedLeafs(ent, server_c.sv.worldmodel->nodes);
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

        if (ent->v.solid == server_c.SOLID_BSP)
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
    public static int SV_HullPointContents(model_c.hull_t* hull, int num, Vector3 p)
    {
        float d;
        bspfile_c.dclipnode_t* node;
        model_c.mplane_t* plane;

        while (num >= 0)
        {
            if (num < hull->firstclipnode || num > hull->lastclipnode)
            {
                sys_win_c.Sys_Error("SV_HullPointContents: bad node number");
            }

            node = hull->clipnodes + num;
            plane = hull->planes + node->planenum;

            if (plane->type < 3)
            {
                d = p[plane->type] - plane->dist;
            }
            else
            {
                d = mathlib_c.DotProduct(plane->normal, p) - plane->dist;
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

    public static int SV_PointContents(Vector3 p)
    {
        int cont;

        cont = SV_HullPointContents(&server_c.sv.worldmodel->hulls[0], 0, p);

        if (cont <= bspfile_c.CONTENTS_CURRENT_0 && cont >= bspfile_c.CONTENTS_CURRENT_DOWN)
        {
            cont = bspfile_c.CONTENTS_WATER;
        }

        return cont;
    }

    public static int SV_TruePointContents(Vector3 p)
    {
        return SV_HullPointContents(&server_c.sv.worldmodel->hulls[0], 0, p);
    }

    public static progs_c.edict_t* SV_TestEntityPosition(progs_c.edict_t* ent)
    {
        trace_t trace;

        trace = SV_Move(ent->v.origin, ent->v.mins, ent->v.maxs, ent->v.origin, 0, ent);

        if (trace.startsolid)
        {
            return server_c.sv.edicts;
        }

        return null;
    }

    public const float DIST_EPSILON = 0.03125f;

    public static bool SV_RecursiveHullCheck(model_c.hull_t* hull, int num, float p1f, float p2f, Vector3 p1, Vector3 p2, trace_t* trace)
    {
        bspfile_c.dclipnode_t* node;
        model_c.mplane_t* plane;
        float t1, t2;
        float frac;
        int i;
        Vector3 mid;
        int side;
        float midf;

        mid = new();

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
            sys_win_c.Sys_Error("SV_RecursiveHullCheck: bad node number");
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
            t1 = mathlib_c.DotProduct(plane->normal, p1) - plane->dist;
            t2 = mathlib_c.DotProduct(plane->normal, p2) - plane->dist;
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

        side = t1 < 0 == true ? 1 : 0;

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
                console_c.Con_DPrintf("backup past 0\n");
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

    public static trace_t SV_ClipMoveToEntity(progs_c.edict_t* ent, Vector3 start, Vector3 mins, Vector3 maxs, Vector3 end)
    {
        trace_t trace;
        Vector3 offset;
        Vector3 start_l, end_l;
        model_c.hull_t* hull;

        offset = start_l = end_l = new();

        memset_c.memset((object*)&trace, 0, sizeof(trace_t));
        trace.fraction = 1;
        trace.allsolid = true;
        mathlib_c.VectorCopy(end, trace.endpos);

        hull = SV_HullForEntity(ent, mins, maxs, offset);

        mathlib_c.VectorSubtract(start, offset, start_l);
        mathlib_c.VectorSubtract(end, offset, end_l);

#if QUAKE2
        if (ent->v.solid == server_c.SOLID_BSP && (ent->v.angles[0] != 0 || ent->v.angles[1] != 0 || ent->v.angles[2] != 0))
        {
            Vector3 a;
            Vector3 forward, right, up;
            Vector3 temp;

            a = forward = right = up = temp = new();

            mathlib_c.AngleVectors(ent->v.angles, forward, right, up);

            mathlib_c.VectorCopy(start_l, temp);
            start_l[0] = mathlib_c.DotProduct(temp, forward);
            start_l[1] = -mathlib_c.DotProduct(temp, right);
            start_l[2] = mathlib_c.DotProduct(temp, up);

            mathlib_c.VectorCopy(end_l, temp);
            end_l[0] = mathlib_c.DotProduct(temp, forward);
            end_l[1] = -mathlib_c.DotProduct(temp, right);
            end_l[2] = mathlib_c.DotProduct(temp, up);
        }
#endif

        SV_RecursiveHullCheck(hull, hull->firstclipnode, 0, 1, start_l, end_l, &trace);

#if QUAKE2
        if (ent->v.solid == server_c.SOLID_BSP && (ent->v.angles[0] != 0 || ent->v.angles[1] != 0 || ent->v.angles[2] != 0))
        {
            Vector3 a;
            Vector3 forward, right, up;
            Vector3 temp;

            a = forward = right = up = temp = new();

            if (trace.fraction != 1)
            {
                mathlib_c.VectorSubtract(mathlib_c.vec3_origin, ent->v.angles, a);
                mathlib_c.AngleVectors(a, forward, right, up);

                mathlib_c.VectorCopy(trace.endpos, temp);
                trace.endpos[0] = mathlib_c.DotProduct(temp, forward);
                trace.endpos[1] = -mathlib_c.DotProduct(temp, right);
                trace.endpos[2] = mathlib_c.DotProduct(temp, up);

                mathlib_c.VectorCopy(trace.plane.normal, temp);
                trace.plane.normal[0] = mathlib_c.DotProduct(temp, forward);
                trace.plane.normal[1] = -mathlib_c.DotProduct(temp, right);
                trace.plane.normal[2] = mathlib_c.DotProduct(temp, up);
            }
        }
#endif

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

    public static trace_t SV_ClipMoveToEntity(progs_c.edict_t* ent, float* start, Vector3 mins, Vector3 maxs, float* end)
    {
        return SV_ClipMoveToEntity(ent, mathlib_c.FloatPtrToVec(start), mins, maxs, mathlib_c.FloatPtrToVec(end));
    }

    public static trace_t SV_ClipMoveToEntity(progs_c.edict_t* ent, float* start, float* mins, float* maxs, float* end)
    {
        return SV_ClipMoveToEntity(ent, start, mathlib_c.FloatPtrToVec(mins), mathlib_c.FloatPtrToVec(maxs), end);
    }

    public static void SV_ClipToLinks(areanode_t* node, moveclip_t* clip)
    {
        common_c.link_t* l, next;
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
                sys_win_c.Sys_Error("Trigger in clipping list");
            }

            if (clip->type == MOVE_NOMONSTERS && touch->v.solid != server_c.SOLID_BSP)
            {
                continue;
            }

            if (clip->boxmins[0] > touch->v.absmax[0] || clip->boxmins[1] > touch->v.absmax[1] || clip->boxmins[2] > touch->v.absmax[2] || clip->boxmaxs[0] < touch->v.absmin[0] || clip->boxmaxs[1] < touch->v.absmin[1] || clip->boxmaxs[2] < touch->v.absmin[2])
            {
                continue;
            }

            if (clip->passedict != null && clip->passedict->v.size[0] != 0 && touch->v.size[0] == 0)
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
                trace = SV_ClipMoveToEntity(touch, clip->start, clip->mins2, clip->maxs2, clip->end);
            }
            else
            {
                trace = SV_ClipMoveToEntity(touch, clip->start, clip->mins, clip->maxs, clip->end);
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

    public static void SV_MoveBounds(Vector3 start, Vector3 mins, Vector3 maxs, Vector3 end, Vector3 boxmins, Vector3 boxmaxs)
    {
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

    public static trace_t SV_Move(Vector3 start, Vector3 mins, Vector3 maxs, Vector3 end, int type, progs_c.edict_t* passedict)
    {
        moveclip_t clip;
        int i;

        memset_c.memset((object*)&clip, 0, sizeof(moveclip_t));

        clip.trace = SV_ClipMoveToEntity(server_c.sv.edicts, start, mins, maxs, end);

        clip.start = mathlib_c.VecToFloatPtr(start);
        clip.end = mathlib_c.VecToFloatPtr(end);
        clip.mins = mathlib_c.VecToFloatPtr(mins);
        clip.maxs = mathlib_c.VecToFloatPtr(maxs);
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
}