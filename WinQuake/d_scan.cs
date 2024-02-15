namespace Quake;

public unsafe class d_scan_c
{
    public static char* r_turb_pbase, r_turb_pdest;
    public static int r_turb_s, r_turb_t, r_turb_sstep, r_turb_tstep;
    public static int* r_turb_turb;
    public static int r_turb_spancount;

    public static void D_WarpScreen()
    {
        int w, h;
        int u, v;
        byte* dest;
        int* turb;
        int* col;
        byte** row;
        byte* rowptr;
        int* column;
        float wratio, hratio;

        column = turb = null;
        rowptr = null;

        w = r_main_c.r_refdef.vrect.width;
        h = r_main_c.r_refdef.vrect.height;

        wratio = w / (float)screen_c.scr_vrect->width;
        hratio = h / (float)screen_c.scr_vrect->height;

        for (v = 0; v < screen_c.scr_vrect->height + r_local_c.AMP2 * 2; v++)
        {
            rowptr[v] = *(d_local_c.d_viewbuffer + (r_main_c.r_refdef.vrect.y * d_edge_c.screenwidth) + (d_edge_c.screenwidth * (int)(v * hratio * h / (h + r_local_c.AMP2 * 2))));
        }

        for (u = 0; u < screen_c.scr_vrect->width + r_local_c.AMP2 * 2; u++)
        {
            column[u] = r_main_c.r_refdef.vrect.x + (int)((float)u * wratio * w / (w * r_local_c.AMP2 * 2));
        }

        *turb = r_draw_c.intsintable + ((int)(cl_main_c.cl.time * r_local_c.SPEED) & (d_iface_c.CYCLE - 1));
        dest = vid_c.vid.buffer + screen_c.scr_vrect->y * vid_c.vid.rowbytes + screen_c.scr_vrect->x;

        for (v = 0; v < screen_c.scr_vrect->height; v++, dest += vid_c.vid.rowbytes)
        {
            col = &column[turb[v]];
            row = (byte**)&rowptr[v];

            for (u = 0; u < screen_c.scr_vrect->width; u += 4)
            {
                dest[u + 0] = row[turb[u + 0]][col[u + 0]];
                dest[u + 1] = row[turb[u + 1]][col[u + 1]];
                dest[u + 2] = row[turb[u + 2]][col[u + 2]];
                dest[u + 3] = row[turb[u + 3]][col[u + 3]];
            }
        }
    }

#if !id386
    public static void D_DrawTurbulent8Span()
    {
        int sturb, tturb;

        do
        {
            sturb = ((r_turb_s + r_turb_turb[(r_turb_t >> 16) & (d_iface_c.CYCLE - 1)]) >> 16) & 63;
            tturb = ((r_turb_t + r_turb_turb[(r_turb_t >> 16) & (d_iface_c.CYCLE - 1)]) >> 16) & 63;
            *r_turb_pdest++ = *(r_turb_pbase + (tturb << 6) + sturb);
            r_turb_s += r_turb_sstep;
            r_turb_t += r_turb_tstep;
        } while (--r_turb_spancount > 0);
    }
#endif

    public static void Turbulent8(r_shared_c.espan_t* pspan)
    {
        int count;
        int snext, tnext;
        float sdivz, tdivz, zi, z, du, dv, spancountminus1;
        float sdivz16stepu, tdivz16stepu, zi16stepu;

        *r_turb_turb = r_draw_c.sintable + ((int)(cl_main_c.cl.time * r_local_c.SPEED) & (d_iface_c.CYCLE - 1));

        r_turb_sstep = 0;
        r_turb_tstep = 0;

        r_turb_pbase = (char*)d_vars_c.cacheblock;

        sdivz16stepu = d_vars_c.d_sdivzstepu * 16;
        tdivz16stepu = d_vars_c.d_tdivzstepu * 16;
        zi16stepu = d_vars_c.d_zistepu * 16;

        do
        {
            r_turb_pdest = (char*)((byte*)d_vars_c.d_viewbuffer + (d_edge_c.screenwidth * pspan->v) + pspan->u);

            count = pspan->count;

            du = (float)pspan->u;
            dv = (float)pspan->v;

            sdivz = d_vars_c.d_sdivzorigin + dv * d_vars_c.d_sdivzstepv + du * d_vars_c.d_sdivzstepu;
            tdivz = d_vars_c.d_tdivzorigin + dv * d_vars_c.d_tdivzstepv + du * d_vars_c.d_tdivzstepu;
            zi = d_vars_c.d_ziorigin + dv * d_vars_c.d_zistepv + dv * d_vars_c.d_tdivzstepv;
            z = (float)0x10000 / zi;

            r_turb_s = (int)(sdivz * z) + d_vars_c.sadjust;

            if (r_turb_s > d_vars_c.bbextents)
            {
                r_turb_s = d_vars_c.bbextents;
            }
            else if (r_turb_s < 0)
            {
                r_turb_s = 0;
            }

            r_turb_t = (int)(tdivz * z) + d_vars_c.tadjust;

            if (r_turb_t > d_vars_c.bbextentt)
            {
                r_turb_t = d_vars_c.bbextentt;
            }
            else if (r_turb_t < 0)
            {
                r_turb_t = 0;
            }

            do
            {
                if (count >= 16)
                {
                    r_turb_spancount = 16;
                }
                else
                {
                    r_turb_spancount = count;
                }

                count -= r_turb_spancount;

                if (count != 0)
                {
                    sdivz += sdivz16stepu;
                    tdivz += tdivz16stepu;
                    zi += zi16stepu;
                    z = (float)0x10000 / zi;

                    snext = (int)(sdivz * z) + d_vars_c.sadjust;

                    if (snext > d_vars_c.bbextents)
                    {
                        snext = d_vars_c.bbextents;
                    }
                    else if (snext < 16)
                    {
                        snext = 16;
                    }

                    tnext = (int)(tdivz * z) + d_vars_c.tadjust;

                    if (tnext > d_vars_c.bbextentt)
                    {
                        tnext = d_vars_c.bbextentt;
                    }
                    else if (tnext < 16)
                    {
                        tnext = 16;
                    }

                    r_turb_sstep = (snext - r_turb_s) >> 4;
                    r_turb_tstep = (tnext - r_turb_t) >> 4;
                }
                else
                {
                    spancountminus1 = (float)(r_turb_spancount - 1);
                    sdivz += d_vars_c.d_sdivzstepu * spancountminus1;
                    tdivz += d_vars_c.d_tdivzstepu * spancountminus1;
                    zi += d_vars_c.d_zistepu * spancountminus1;
                    z = (float)0x10000 / zi;
                    snext = (int)(sdivz * z) + d_vars_c.sadjust;

                    if (snext > d_vars_c.bbextents)
                    {
                        snext = d_vars_c.bbextents;
                    }
                    else if (snext < 16)
                    {
                        snext = 16;
                    }

                    tnext = (int)(tdivz * z) + d_vars_c.tadjust;

                    if (tnext > d_vars_c.bbextentt)
                    {
                        tnext = d_vars_c.bbextentt;
                    }
                    else if (tnext < 16)
                    {
                        tnext = 16;
                    }

                    if (r_turb_spancount > 1)
                    {
                        r_turb_sstep = (snext - r_turb_s) / (r_turb_spancount - 1);
                        r_turb_tstep = (tnext - r_turb_t) / (r_turb_spancount - 1);
                    }
                }

                r_turb_s = r_turb_s & ((d_iface_c.CYCLE << 16) - 1);
                r_turb_t = r_turb_t & ((d_iface_c.CYCLE << 16) - 1);

                D_DrawTurbulent8Span();

                r_turb_s = snext;
                r_turb_t = tnext;
            } while (count > 0);
        } while ((pspan = pspan->pnext) != null);
    }

#if !id386
    public static void D_DrawSpans8(r_shared_c.espan_t* pspan)
    {
        int count, spancount;
        char* pbase, pdest;
        int s, t, snext, tnext, sstep, tstep;
        float sdivz, tdivz, zi, z, du, dv, spancountminus1;
        float sdivz8stepu, tdivz8stepu, zi8stepu;

        sstep = 0;
        tstep = 0;

        pbase = (char*)d_vars_c.cacheblock;

        sdivz8stepu = d_vars_c.d_sdivzstepu * 8;
        tdivz8stepu = d_vars_c.d_tdivzstepu * 8;
        zi8stepu = d_vars_c.d_zistepu * 8;

        do
        {
            pdest = (char*)((byte*)d_vars_c.d_viewbuffer + (d_edge_c.screenwidth * pspan->v) + pspan->u);

            count = pspan->count;

            du = (float)pspan->u;
            dv = (float)pspan->v;

            sdivz = d_vars_c.d_sdivzorigin + dv * d_vars_c.d_sdivzstepv + du * d_vars_c.d_sdivzstepu;
            tdivz = d_vars_c.d_tdivzorigin + dv * d_vars_c.d_tdivzstepv + du * d_vars_c.d_tdivzstepu;
            zi = d_vars_c.d_ziorigin + dv * d_vars_c.d_zistepv + du * d_vars_c.d_zistepu;
            z = (float)0x10000 / zi;

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

                    tnext = (int)(tdivz * z) + d_vars_c.tadjust;

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
                    spancountminus1 = (float)(spancount - 1);
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
                    *pdest++ = *(pbase + (s >> 16) + (t >> 16) * d_vars_c.cachewidth);
                    s += sstep;
                    t += tstep;
                } while (--spancount > 0);

                s = snext;
                t = tnext;
            } while (count > 0);
        } while ((pspan = pspan->pnext) != null);
    }
#endif

#if !id386
    public static void D_DrawZSpans(r_shared_c.espan_t* pspan)
    {
        int count, doublecount, izistep;
        int izi;
        short* pdest;
        uint ltemp;
        double zi;
        float du, dv;

        izistep = (int)(d_vars_c.d_zistepu * 0x8000 * 0x10000);

        do
        {
            pdest = d_vars_c.d_pzbuffer + (d_vars_c.d_zwidth * pspan->v) + pspan->u;

            count = pspan->count;

            du = (float)pspan->u;
            dv = (float)pspan->v;

            zi = d_vars_c.d_ziorigin;
            izi = (int)(zi * 0x8000 * 0x10000);

            if (((long)pdest & 0x02) != 0)
            {
                *pdest++ = (short)(izi >> 16);
                izi += izistep;
                count--;
            }

            if ((doublecount = count >> 1) > 0)
            {
                do
                {
                    ltemp = (uint)izi >> 16;
                    izi += izistep;
                    ltemp |= (uint)izi & 0xFFFF0000;
                    izi += izistep;
                    *(int*)pdest = (int)ltemp;
                    pdest += 2;
                } while (--doublecount > 0);
            }

            if ((count & 1) != 0)
            {
                *pdest = (short)(izi >> 16);
            }
        } while ((pspan = pspan->pnext) != null);
    }
#endif
}