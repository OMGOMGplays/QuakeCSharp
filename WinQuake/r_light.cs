namespace Quake;

public unsafe class r_light_c
{
    public static int r_dlightframecount;

    public static void R_AnimateLight()
    {
        int i, j, k;

        i = (int)(cl_main_c.cl.time * 10);

        for (j = 0; j < quakedef_c.MAX_LIGHTSTYLES; j++)
        {
            if (cl_main_c.cl_lightstyle[j].length == 0)
            {
                r_main_c.d_lightstylevalue[j] = 256;
                continue;
            }

            k = i % cl_main_c.cl_lightstyle[j].length;
            k = cl_main_c.cl_lightstyle[j].map[k] - 'a';
            k = k * 22;
            r_main_c.d_lightstylevalue[j] = k;
        }
    }

    public static void R_MarkLights(client_c.dlight_t* light, int bit, model_c.mnode_t* node)
    {
        model_c.mplane_t* splitplane;
        float dist;
        model_c.msurface_t* surf;
        int i;

        if (node->contents < 0)
        {
            return;
        }

        splitplane = node->plane;
        dist = mathlib_c.DotProduct(light->origin, splitplane->normal) - splitplane->dist;

        if (dist > light->radius)
        {
            R_MarkLights(light, bit, &node->children[0]);
            return;
        }

        if (dist < -light->radius)
        {
            R_MarkLights(light, bit, &node->children[1]);
        }

        surf = cl_main_c.cl.worldmodel->surfaces + node->firstsurface;

        for (i = 0; i < node->numsurfaces; i++, surf++)
        {
            if (surf->dlightframe != r_dlightframecount)
            {
                surf->dlightbits = 0;
                surf->dlightframe = r_dlightframecount;
            }

            surf->dlightbits |= bit;
        }

        R_MarkLights(light, bit, &node->children[0]);
        R_MarkLights(light, bit, &node->children[1]);
    }

    public static void R_PushDlights()
    {
        int i;
        client_c.dlight_t* l;

        r_dlightframecount = r_main_c.r_framecount + 1;

        l = cl_main_c.cl_dlights;

        for (i = 0; i < client_c.MAX_DLIGHTS; i++, l++)
        {
            if (l->die < cl_main_c.cl.time || l->radius == 0)
            {
                continue;
            }

            R_MarkLights(l, 1 << i, cl_main_c.cl.worldmodel->nodes);
        }
    }

    public static int RecursiveLightPoint(model_c.mnode_t* node, Vector3 start, Vector3 end)
    {
        int r;
        float front, back, frac;
        int side;
        model_c.mplane_t* plane;
        Vector3 mid = new();
        model_c.msurface_t* surf;
        int s, t, ds, dt;
        int i;
        model_c.mtexinfo_t* tex;
        byte* lightmap;
        uint scale;
        int maps;

        if (node->contents < 0)
        {
            return -1;
        }

        plane = node->plane;
        front = mathlib_c.DotProduct(start, plane->normal) - plane->dist;
        back = mathlib_c.DotProduct(end, plane->normal) - plane->dist;
        side = front < 0 ? 1 : 0;

        if ((back < 0) && back == side)
        {
            return RecursiveLightPoint(&node->children[side], start, end);
        }

        frac = front / (front - back);
        mid[0] = start[0] + (end[0] - start[0]) * frac;
        mid[1] = start[1] + (end[1] - start[1]) * frac;
        mid[2] = start[2] + (end[2] - start[2]) * frac;

        r = RecursiveLightPoint(&node->children[side], start, mid);

        if (r >= 0)
        {
            return r;
        }

        if (back < 0 && back == side)
        {
            return -1;
        }

        surf = cl_main_c.cl.worldmodel->surfaces + node->firstsurface;

        for (i = 0; i < node->numsurfaces; i++, surf++)
        {
            if ((surf->flags & model_c.SURF_DRAWTILED) != 0)
            {
                continue;
            }

            tex = surf->texinfo;

            s = (int)(mathlib_c.DotProduct(mid, tex->vecs[0]) + tex->vecs[0][3]);
            t = (int)(mathlib_c.DotProduct(mid, tex->vecs[1]) + tex->vecs[1][3]);

            if (s < surf->texturemins[0] || t < surf->texturemins[1])
            {
                continue;
            }

            ds = s - surf->texturemins[0];
            dt = t - surf->texturemins[1];

            if (ds > surf->extents[0] || dt > surf->extents[1])
            {
                continue;
            }

            if (surf->samples == (byte*)0)
            {
                return 0;
            }

            ds >>= 4;
            dt >>= 4;

            lightmap = surf->samples;
            r = 0;

            if (lightmap != null)
            {
                lightmap += dt * ((surf->extents[0] >> 4) + 1) + ds;

                for (maps = 0; maps < bspfile_c.MAXLIGHTMAPS && surf->styles[maps] != 255; maps++)
                {
                    scale = (uint)r_main_c.d_lightstylevalue[surf->styles[maps]];
                    r += (int)(*lightmap * scale);
                    lightmap += ((surf->extents[0] >> 4) + 1) * ((surf->extents[1] >> 4) + 1);
                }

                r >>= 8;
            }

            return r;
        }

        return RecursiveLightPoint(&node->children[-side], mid, end);
    }

    public static int R_LightPoint(Vector3 p)
    {
        Vector3 end = new();
        int r;

        if (cl_main_c.cl.worldmodel->lightdata == null)
        {
            return 255;
        }

        end[0] = p[0];
        end[1] = p[1];
        end[2] = p[2] - 2048;

        r = RecursiveLightPoint(cl_main_c.cl.worldmodel->nodes, p, end);

        if (r == -1)
        {
            r = 0;
        }

        if (r < r_main_c.r_refdef.ambientlight)
        {
            r = r_main_c.r_refdef.ambientlight;
        }

        return r;
    }
}