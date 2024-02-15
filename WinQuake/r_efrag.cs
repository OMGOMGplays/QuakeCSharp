namespace Quake;

public unsafe class r_efrag_c
{
    public static model_c.mnode_t* r_pefragtopnode;

    public static render_c.efrag_t** lastlink;

    public static Vector3 r_emins, r_emaxs;

    public static render_c.entity_t* r_addent;

    public static void R_RemoveEfrags(render_c.entity_t* ent)
    {
        render_c.efrag_t* ef, old, walk;
        render_c.efrag_t** prev;

        ef = ent->efrag;

        while (ef != null)
        {
            prev = &ef->leaf->efrags;

            while (true)
            {
                walk = *prev;

                if (walk == null)
                {
                    break;
                }

                if (walk == ef)
                {
                    *prev = ef->leafnext;
                    break;
                }
                else
                {
                    prev = &walk->leafnext;
                }
            }

            old = ef;
            ef = ef->entnext;

            old->entnext = cl_main_c.cl.free_efrags;
            cl_main_c.cl.free_efrags = old;
        }

        ent->efrag = null;
    }

    public static void R_SplitEntityOnNode(model_c.mnode_t* node)
    {
        render_c.efrag_t* ef;
        model_c.mplane_t* splitplane;
        model_c.mleaf_t* leaf;
        int sides;

        if (node->contents == bspfile_c.CONTENTS_SOLID)
        {
            return;
        }

        if (node->contents < 0)
        {
            if (r_pefragtopnode == null)
            {
                r_pefragtopnode = node;
            }

            leaf = (model_c.mleaf_t*)node;

            ef = cl_main_c.cl.free_efrags;

            if (ef == null)
            {
                console_c.Con_Printf("Too many efrags!\n");
                return;
            }

            cl_main_c.cl.free_efrags = cl_main_c.cl.free_efrags->entnext;

            ef->entity = r_addent;

            *lastlink = ef;
            lastlink = &ef->entnext;
            ef->entnext = null;

            ef->leaf = leaf;
            ef->leafnext = leaf->efrags;
            leaf->efrags = ef;

            return;
        }

        splitplane = node->plane;
        sides = mathlib_c.BOX_ON_PLANE_SIDE(r_emins, r_emaxs, splitplane);

        if (sides == 3)
        {
            if (r_pefragtopnode == null)
            {
                r_pefragtopnode = node;
            }
        }

        if ((sides & 1) != 0)
        {
            R_SplitEntityOnNode(&node->children[0]);
        }

        if ((sides & 2) != 0)
        {
            R_SplitEntityOnNode(&node->children[1]);
        }
    }

    public static void R_SplitEntityOnNode2(model_c.mnode_t* node)
    {
        model_c.mplane_t* splitplane;
        int sides;

        if (node->visframe != r_main_c.r_visframecount)
        {
            return;
        }

        if (node->contents < 0)
        {
            if (node->contents != bspfile_c.CONTENTS_SOLID)
            {
                r_pefragtopnode = node;
            }

            return;
        }

        splitplane = node->plane;
        sides = mathlib_c.BOX_ON_PLANE_SIDE(r_emins, r_emaxs, splitplane);

        if (sides == 3)
        {
            r_pefragtopnode = node;
            return;
        }

        if ((sides & 1) != 0)
        {
            R_SplitEntityOnNode2(&node->children[0]);
        }
        else
        {
            R_SplitEntityOnNode2(&node->children[1]);
        }
    }

    public static void R_AddEfrags(render_c.entity_t* ent)
    {
        model_c.model_t* entmodel = null;
        int i;

        if (ent->model == null)
        {
            return;
        }

        if (ent == cl_main_c.cl_entities)
        {
            return;
        }

        r_addent = ent;

        lastlink = &ent->efrag;
        r_pefragtopnode = null;

        for (i = 0; i < 3; i++)
        {
            r_emins[i] = ent->origin[i] + entmodel->mins[i];
            r_emaxs[i] = ent->origin[i] + entmodel->maxs[i];
        }

        R_SplitEntityOnNode(cl_main_c.cl.worldmodel->nodes);

        ent->topnode = r_pefragtopnode;
    }

    public static void R_StoreEfrags(render_c.efrag_t** ppefrag)
    {
        render_c.entity_t* pent;
        model_c.model_t* clmodel;
        render_c.efrag_t* pefrag;

        while ((pefrag = *ppefrag) != null)
        {
            pent = pefrag->entity;
            clmodel = pent->model;

            switch (clmodel->type)
            {
                case model_c.modtype_t.mod_alias:
                case model_c.modtype_t.mod_brush:
                case model_c.modtype_t.mod_sprite:
                    pent = pefrag->entity;

                    if ((pent->visframe != r_main_c.r_framecount) && (cl_main_c.cl_numvisedicts < client_c.MAX_VISEDICTS))
                    {
                        cl_main_c.cl_visedicts[cl_main_c.cl_numvisedicts++] = *pent;

                        pent->visframe = r_main_c.r_framecount;
                    }

                    ppefrag = &pefrag->leafnext;
                    break;

                default:
                    sys_win_c.Sys_Error($"R_StoreEfrags: Bad entity type {clmodel->type}");
                    break;
            }
        }
    }
}