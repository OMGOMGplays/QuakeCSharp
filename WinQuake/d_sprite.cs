namespace Quake;

public unsafe class d_sprite_c
{
    public static int sprite_height;
    public static int minindex, maxindex;
    public static d_local_c.sspan_t* sprite_spans;

#if !id386
    public static void D_SpriteDrawSpans(d_local_c.sspan_t* pspan)
    {
        int count, spancount, izistep;
        int izi;
        byte* pbase, pdest;
        int s, t, snext, tnext, sstep, tstep;
        float sdivz, tdivz, zi, z, du, dv, spancountminus1;
        float sdivz8stepu, tdivz8stepu, zi8stepu;
        byte btemp;
        short* pz;

        sstep = 0;
        tstep = 0;

        pbase = d_vars_c.cacheblock;

        sdivz8stepu = d_vars_c.d_sdivzstepu * 8;
        tdivz8stepu = d_vars_c.d_tdivzstepu * 8;
        zi8stepu = d_vars_c.d_zistepu * 8;

        izistep = (int)(d_vars_c.d_zistepu * 0x8000 * 0x10000);

        do
        {
            pdest = (byte*)d_vars_c.d_viewbuffer + (d_edge_c.screenwidth * pspan->v) + pspan->u;
            pz = d_vars_c.d_pzbuffer + (d_vars_c.d_zwidth * pspan->v) + pspan->u;

            count = pspan->count;

            if (count <= 0)
            {
                goto NextSpan;
            }

            du = (float)pspan->u;
            dv = (float)pspan->v;

            sdivz = d_vars_c.d_sdivzorigin + dv * d_vars_c.d_sdivzstepv + du * d_vars_c.d_sdivzstepu;
            tdivz = d_vars_c.d_tdivzorigin + dv * d_vars_c.d_tdivzstepv + du * d_vars_c.d_tdivzstepu;
            zi = d_vars_c.d_ziorigin + dv * d_vars_c.d_zistepv + du * d_vars_c.d_zistepu;
            z = (float)0x10000 / zi;

            izi = (int)(zi * 0x8000 * 0x10000);

            s = (int)(sdivz * z) + d_vars_c.sadjust;

            if (s > d_vars_c.bbextents)
            {
                s = d_vars_c.bbextents;
            }
            else if (s < 0)
            {
                s = 0;
            }

            t = (int)(tdivz * z) + d_vars_c.tadjust;

            if (t > d_vars_c.bbextentt)
            {
                t = d_vars_c.bbextentt;
            }
            else if (t < 0)
            {
                t = 0;
            }

            do
            {
                if (count >= 8)
                {
                    spancount = 8;
                }
                else
                {
                    spancount = count;
                }

                count -= spancount;

                if (count != 0)
                {
                    sdivz += sdivz8stepu;
                    tdivz += tdivz8stepu;
                    zi += zi8stepu;
                    z = (float)0x10000 / zi;

                    snext = (int)(sdivz * z) + d_vars_c.sadjust;

                    if (snext > d_vars_c.bbextents)
                    {
                        snext = d_vars_c.bbextents;
                    }
                    else if (snext < 8)
                    {
                        snext = 8;
                    }

                    tnext = (int)(tdivz + z) + d_vars_c.tadjust;

                    if (tnext > d_vars_c.bbextentt)
                    {
                        tnext = d_vars_c.bbextentt;
                    }
                    else if (tnext < 8)
                    {
                        tnext = 8;
                    }

                    sstep = (snext - s) >> 3;
                    tstep = (tnext - t) >> 3;
                }
                else
                {
                    spancountminus1 = spancount - 1;
                    sdivz += d_vars_c.d_sdivzstepu * spancountminus1;
                    tdivz += d_vars_c.d_tdivzstepu * spancountminus1;
                    zi += d_vars_c.d_zistepu * spancountminus1;
                    z = (float)0x10000 / zi;

                    snext = (int)(sdivz * z) + d_vars_c.sadjust;

                    if (snext > d_vars_c.bbextents)
                    {
                        snext = d_vars_c.bbextents;
                    }
                    else if (snext < 8)
                    {
                        snext = 8;
                    }

                    tnext = (int)(tdivz * z) + d_vars_c.tadjust;

                    if (tnext > d_vars_c.bbextentt)
                    {
                        tnext = d_vars_c.bbextentt;
                    }
                    else if (tnext < 8)
                    {
                        tnext = 8;
                    }

                    if (spancount > 1)
                    {
                        sstep = (snext - s) / (spancount - 1);
                        tstep = (tnext - t) / (spancount - 1);
                    }
                }

                do
                {
                    btemp = *(pbase + (s >> 16) + (t >> 16) * d_vars_c.cachewidth);

                    if (btemp != 255)
                    {
                        if (*pz <= (izi >> 16))
                        {
                            *pz = (short)(izi >> 16);
                            *pdest = btemp;
                        }
                    }

                    izi += izistep;
                    pdest++;
                    pz++;
                    s += sstep;
                    t += tstep;
                } while (--spancount > 0);

                s = snext;
                t = tnext;
            } while (count > 0);

        NextSpan:
            pspan++;
        } while (pspan->count != d_local_c.DS_SPAN_LIST_END);
    }
#endif

    public static void D_SpriteScanLeftEdge()
    {
        int i, v, itop, ibottom, lmaxindex;
        d_iface_c.emitpoint_t* pvert, pnext;
        d_local_c.sspan_t* pspan;
        float du, dv, vtop, vbottom, slope;
        int u, u_step;

        pspan = sprite_spans;
        i = minindex;

        if (i == 0)
        {
            i = r_sprite_c.r_spritedesc.nump;
        }

        lmaxindex = maxindex;

        if (lmaxindex == 0)
        {
            lmaxindex = r_sprite_c.r_spritedesc.nump;
        }

        vtop = MathF.Ceiling(r_sprite_c.r_spritedesc.pverts[i].v);

        do
        {
            pvert = &r_sprite_c.r_spritedesc.pverts[i];
            pnext = pvert - 1;

            vbottom = MathF.Ceiling(pnext->v);

            if (vtop < vbottom)
            {
                du = pnext->u - pvert->u;
                dv = pnext->v - pvert->v;
                slope = du / dv;
                u_step = (int)(slope * 0x10000);
                u = (int)((pvert->u + (slope * (vtop - pvert->v))) * 0x10000) + (0x10000 - 1);

                itop = (int)vtop;
                ibottom = (int)vbottom;

                for (v = itop; v < ibottom; v++)
                {
                    pspan->u = u >> 16;
                    pspan->v = v;
                    u += u_step;
                    pspan++;
                }
            }

            vtop = vbottom;

            i--;

            if (i == 0)
            {
                i = r_sprite_c.r_spritedesc.nump;
            }
        } while (i != lmaxindex);
    }

    public static void D_SpriteScanRightEdge()
    {
        int i, v, itop, ibottom;
        d_iface_c.emitpoint_t* pvert, pnext;
        d_local_c.sspan_t* pspan;
        float du, dv, vtop, vbottom, slope, uvert, unext, vvert, vnext;
        int u, u_step;

        pspan = sprite_spans;
        i = minindex;

        vvert = r_sprite_c.r_spritedesc.pverts[i].v;

        if (vvert < render_c.r_refdef.fvrecty_adj)
        {
            vvert = render_c.r_refdef.fvrecty_adj;
        }

        if (vvert > render_c.r_refdef.fvrectbottom_adj)
        {
            vvert = render_c.r_refdef.fvrectbottom_adj;
        }

        vtop = MathF.Ceiling(vvert);

        do
        {
            pvert = &d_iface_c.r_spritedesc.pverts[i];
            pnext = pvert + 1;

            vnext = pnext->v;

            if (vnext < render_c.r_refdef.fvrecty_adj)
            {
                vnext = render_c.r_refdef.fvrecty_adj;
            }

            if (vnext > render_c.r_refdef.fvrectbottom_adj)
            {
                vnext = render_c.r_refdef.fvrectbottom_adj;
            }

            vbottom = MathF.Ceiling(vnext);

            if (vtop < vbottom)
            {
                uvert = pvert->u;

                if (uvert < render_c.r_refdef.fvrectx_adj)
                {
                    uvert = render_c.r_refdef.fvrectx_adj;
                }

                if (uvert > render_c.r_refdef.fvrectright_adj)
                {
                    uvert = render_c.r_refdef.fvrectright_adj;
                }

                unext = pnext->u;

                if (unext < render_c.r_refdef.fvrectx_adj)
                {
                    unext = render_c.r_refdef.fvrectx_adj;
                }

                if (unext > render_c.r_refdef.fvrectright_adj)
                {
                    unext = render_c.r_refdef.fvrectright_adj;
                }

                du = unext - uvert;
                dv = vnext - vvert;
                slope = du / dv;
                u_step = (int)(slope * 0x10000);
                u = (int)((uvert + (slope * (vtop - vvert))) * 0x10000) + (0x10000 - 1);

                itop = (int)vtop;
                ibottom = (int)vbottom;

                for (v = itop; v < ibottom; v++)
                {
                    pspan->count = (u >> 16) - pspan->u;
                    u += u_step;
                    pspan++;
                }
            }

            vtop = vbottom;
            vvert = vnext;

            i++;

            if (i == d_iface_c.r_spritedesc.nump)
            {
                i = 0;
            }
        } while (i != maxindex);

        pspan->count = d_local_c.DS_SPAN_LIST_END;
    }

    public static void D_SpriteCalculateGradients()
    {
        Vector3 p_normal, p_saxis, p_taxis, p_temp1;
        float distinv;

        p_normal = p_saxis = p_taxis = p_temp1 = new();

        r_misc_c.TransformVector(d_iface_c.r_spritedesc.vpn, p_normal);
        r_misc_c.TransformVector(d_iface_c.r_spritedesc.vright, p_saxis);
        r_misc_c.TransformVector(d_iface_c.r_spritedesc.vup, p_taxis);
        mathlib_c.VectorInverse(p_taxis);

        distinv = 1.0f / (-mathlib_c.DotProduct(r_bsp_c.modelorg, d_iface_c.r_spritedesc.vpn));

        d_vars_c.d_sdivzstepu = p_saxis[0] * r_main_c.xscaleinv;
        d_vars_c.d_tdivzstepu = p_taxis[0] * r_main_c.xscaleinv;

        d_vars_c.d_sdivzstepv = -p_saxis[1] * r_main_c.yscaleinv;
        d_vars_c.d_tdivzstepv = -p_taxis[1] * r_main_c.yscaleinv;

        d_vars_c.d_zistepu = p_normal[0] * r_main_c.xscaleinv * distinv;
        d_vars_c.d_zistepv = -p_normal[1] * r_main_c.yscaleinv * distinv;

        d_vars_c.d_sdivzorigin = p_saxis[2] - r_main_c.xcenter * d_vars_c.d_sdivzstepu - r_main_c.ycenter * d_vars_c.d_sdivzstepv;
        d_vars_c.d_tdivzorigin = p_taxis[2] - r_main_c.xcenter * d_vars_c.d_tdivzstepu - r_main_c.ycenter * d_vars_c.d_tdivzstepv;
        d_vars_c.d_ziorigin = p_normal[2] * distinv - r_main_c.xcenter * d_vars_c.d_zistepu - r_main_c.ycenter * d_vars_c.d_zistepv;

        r_misc_c.TransformVector(r_bsp_c.modelorg, p_temp1);

        d_vars_c.sadjust = ((int)(mathlib_c.DotProduct(p_temp1, p_saxis) * 0x100000 + 0.5f)) - (-(sprite_height >> 1) << 16);
        d_vars_c.tadjust = ((int)(mathlib_c.DotProduct(p_temp1, p_taxis) * 0x100000 + 0.5f)) - (-(sprite_height >> 1) << 16);

        d_vars_c.bbextents = (d_vars_c.cachewidth << 16) - 1;
        d_vars_c.bbextentt = (sprite_height << 16) - 1;
    }

    public static void D_DrawSprite()
    {
        int i, nump;
        float ymin, ymax;
        d_iface_c.emitpoint_t* pverts;
        d_local_c.sspan_t* spans = null;

        sprite_spans = spans;

        ymin = 999999.9f;
        ymax = -999999.9f;
        pverts = r_sprite_c.r_spritedesc.pverts;

        for (i = 0; i < r_sprite_c.r_spritedesc.nump; i++)
        {
            if (pverts->v < ymin)
            {
                ymin = pverts->v;
                minindex = i;
            }

            if (pverts->v > ymax)
            {
                ymax = pverts->v;
                maxindex = i;
            }

            pverts++;
        }

        ymin = MathF.Ceiling(ymin);
        ymax = MathF.Ceiling(ymax);

        if (ymin >= ymax)
        {
            return;
        }

        d_vars_c.cachewidth = r_sprite_c.r_spritedesc.pspriteframe->width;
        sprite_height = r_sprite_c.r_spritedesc.pspriteframe->height;
        d_vars_c.cacheblock = (byte*)r_sprite_c.r_spritedesc.pspriteframe->pixels[0];

        nump = r_sprite_c.r_spritedesc.nump;
        pverts = r_sprite_c.r_spritedesc.pverts;
        pverts[nump] = pverts[0];

        D_SpriteCalculateGradients();
        D_SpriteScanLeftEdge();
        D_SpriteScanRightEdge();
        D_SpriteDrawSpans(sprite_spans);
    }
}