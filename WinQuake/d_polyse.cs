namespace Quake;

public unsafe class d_polyse_c
{
    //public const int DPS_MAXSPANS = 

    public struct spanpackage_t
    {
        public void* pdest;
        public short* pz;
        public int count;
        public byte* ptex;
        public int sfrac, tfrac, light, zi;
    }

    public struct edgetable
    {
        public int isflattop;
        public int numleftedges;
        public int* pleftedgevert0;
        public int* pleftedgevert1;
        public int* pleftedgevert2;
        public int numrightedges;
        public int* prightedgevert0;
        public int* prightedgevert1;
        public int* prightedgevert2;
    }

    public static int* r_p0, r_p1, r_p2;

    public static byte* d_pcolormap;

    public static int d_aflatcolor;
    public static int d_xdenom;

    public static edgetable* pedgetable;

    public static edgetable[] edgetables =
    {
        new edgetable { isflattop = 0, numleftedges = 1, pleftedgevert0 = r_p0, pleftedgevert1 = r_p2, pleftedgevert2 = null, numrightedges = 2, prightedgevert0 = r_p0, prightedgevert1 = r_p1, prightedgevert2 = r_p2 },
        new edgetable { isflattop = 0, numleftedges = 2, pleftedgevert0 = r_p1, pleftedgevert1 = r_p0, pleftedgevert2 = r_p2, numrightedges = 1, prightedgevert0 = r_p1, prightedgevert1 = r_p2, prightedgevert2 = null },
        new edgetable { isflattop = 1, numleftedges = 1, pleftedgevert0 = r_p0, pleftedgevert1 = r_p2, pleftedgevert2 = null, numrightedges = 1, prightedgevert0 = r_p1, prightedgevert1 = r_p2, prightedgevert2 = null },
        new edgetable { isflattop = 0, numleftedges = 1, pleftedgevert0 = r_p1, pleftedgevert1 = r_p0, pleftedgevert2 = null, numrightedges = 2, prightedgevert0 = r_p1, prightedgevert1 = r_p2, prightedgevert2 = r_p0 },
        new edgetable { isflattop = 0, numleftedges = 2, pleftedgevert0 = r_p0, pleftedgevert1 = r_p2, pleftedgevert2 = r_p1, numrightedges = 1, prightedgevert0 = r_p0, prightedgevert1 = r_p1, prightedgevert2 = null },
        new edgetable { isflattop = 0, numleftedges = 1, pleftedgevert0 = r_p2, pleftedgevert1 = r_p1, pleftedgevert2 = null, numrightedges = 1, prightedgevert0 = r_p2, prightedgevert1 = r_p0, prightedgevert2 = null },
        new edgetable { isflattop = 0, numleftedges = 1, pleftedgevert0 = r_p2, pleftedgevert1 = r_p1, pleftedgevert2 = null, numrightedges = 2, prightedgevert0 = r_p2, prightedgevert1 = r_p0, prightedgevert2 = r_p1 },
        new edgetable { isflattop = 0, numleftedges = 2, pleftedgevert0 = r_p2, pleftedgevert1 = r_p1, pleftedgevert2 = r_p0, numrightedges = 1, prightedgevert0 = r_p2, prightedgevert1 = r_p0, prightedgevert2 = null },
        new edgetable { isflattop = 0, numleftedges = 1, pleftedgevert0 = r_p1, pleftedgevert1 = r_p0, pleftedgevert2 = null, numrightedges = 1, prightedgevert0 = r_p1, prightedgevert1 = r_p2, prightedgevert2 = null },
        new edgetable { isflattop = 1, numleftedges = 1, pleftedgevert0 = r_p2, pleftedgevert1 = r_p1, pleftedgevert2 = null, numrightedges = 1, prightedgevert0 = r_p0, prightedgevert1 = r_p1, prightedgevert2 = null },
        new edgetable { isflattop = 1, numleftedges = 1, pleftedgevert0 = r_p1, pleftedgevert1 = r_p0, pleftedgevert2 = null, numrightedges = 1, prightedgevert0 = r_p2, prightedgevert1 = r_p0, prightedgevert2 = null },
        new edgetable { isflattop = 0, numleftedges = 1, pleftedgevert0 = r_p0, pleftedgevert1 = r_p2, pleftedgevert2 = null, numrightedges = 1, prightedgevert0 = r_p0, prightedgevert1 = r_p1, prightedgevert2 = null },
    };

    public static int a_sstepxfrac, a_tstepxfrac, r_lstepx, a_ststepxwhole;
    public static int r_sstepx, r_tstepx, r_lstepy, r_sstepy, r_tstepy;
    public static int r_zistepx, r_zistepy;
    public static int d_aspancount, d_countextrastep;

    public static spanpackage_t* a_spans;
    public static spanpackage_t* d_pedgespanpackage;
    public static int ystart;
    public static byte* d_pdest, d_ptex;
    public static short* d_pz;
    public static int d_sfrac, d_tfrac, d_light, d_zi;
    public static int d_ptexextrastep, d_sfracextrastep;
    public static int d_tfracextrastep, d_lightextrastep, d_pdestextrastep;
    public static int d_lightbasestep, d_pdestbasestep, d_ptexbasestep;
    public static int d_sfracbasestep, d_tfracbasestep;
    public static int d_ziextrastep, d_zibasestep;
    public static int d_pzextrastep, d_pzbasestep;

    public struct adivtab_t
    {
        public int quotient;
        public int remainder;
    }

    public static adivtab_t* adivtab;

    public static byte** skintable;
    public static int skinwidth;
    public static byte* skinstart;

#if !id386
    public static void D_PolysetDraw()
    {
        spanpackage_t* spans = null;

        a_spans = (spanpackage_t*)(((long)&spans[0] + quakedef_c.CACHE_SIZE - 1) & ~(quakedef_c.CACHE_SIZE - 1));

        if (r_alias_c.r_affinetridesc.drawtype != 0)
        {
            D_DrawSubdiv();
        }
        else
        {
            D_DrawNonSubdiv();
        }
    }

    public static void D_PolysetDrawFinalVerts(d_iface_c.finalvert_t* fv, int numverts)
    {
        int i, z;
        short* zbuf;

        for (i = 0; i < numverts; i++, fv++)
        {
            if ((fv->v[0] < r_main_c.r_refdef.vrectright) && (fv->v[1] < r_main_c.r_refdef.vrectbottom))
            {
                z = fv->v[5] >> 16;
                zbuf = &d_local_c.zspantable[fv->v[1]] + fv->v[0];

                if (z >= *zbuf)
                {
                    int pix;

                    *zbuf = (short)z;
                    pix = skintable[fv->v[3] >> 16][fv->v[2] >> 16];
                    pix = ((byte*)r_alias_c.acolormap)[pix + (fv->v[4] & 0xFF00)];
                    d_vars_c.d_viewbuffer[d_modech_c.d_scantable[fv->v[1]] + fv->v[0]] = (byte)pix;
                }
            }
        }
    }

    public static void D_DrawSubdiv()
    {
        model_c.mtriangle_t* ptri;
        d_iface_c.finalvert_t* pfv, index0, index1, index2;
        int i;
        int lnumtriangles;

        pfv = r_alias_c.r_affinetridesc.pfinalverts;
        ptri = r_alias_c.r_affinetridesc.ptriangles;
        lnumtriangles = r_alias_c.r_affinetridesc.numtriangles;

        for (i = 0; i < lnumtriangles; i++)
        {
            index0 = pfv + ptri[i].vertindex[0];
            index1 = pfv + ptri[i].vertindex[1];
            index2 = pfv + ptri[i].vertindex[2];

            if (((index0->v[1] - index1->v[1]) * (index0->v[0] - index2->v[0]) - (index0->v[0] - index2->v[0]) * (index0->v[1] - index1->v[1])) >= 0)
            {
                continue;
            }

            d_pcolormap = &((byte*)r_alias_c.acolormap)[index0->v[4] & 0xFF00];

            if (ptri[i].facesfront != 0)
            {
                D_PolysetRecursiveTriangle(index0->v, index1->v, index2->v);
            }
            else
            {
                int s0, s1, s2;

                s0 = index0->v[2];
                s1 = index1->v[2];
                s2 = index2->v[2];

                if ((index0->flags & r_shared_c.ALIAS_ONSEAM) != 0)
                {
                    index0->v[2] += r_alias_c.r_affinetridesc.seamfixupX16;
                }

                if ((index1->flags & r_shared_c.ALIAS_ONSEAM) != 0)
                {
                    index1->v[1] += r_alias_c.r_affinetridesc.seamfixupX16;
                }

                if ((index2->flags & r_shared_c.ALIAS_ONSEAM) != 0)
                {
                    index2->v[2] += r_alias_c.r_affinetridesc.seamfixupX16;
                }

                D_PolysetRecursiveTriangle(index0->v, index1->v, index2->v);

                index0->v[2] = s0;
                index1->v[2] = s1;
                index2->v[2] = s2;
            }
        }
    }

    public static void D_DrawNonSubdiv()
    {
        model_c.mtriangle_t* ptri;
        d_iface_c.finalvert_t* pfv, index0, index1, index2;
        int i;
        int lnumtriangles;

        pfv = r_alias_c.r_affinetridesc.pfinalverts;
        ptri = r_alias_c.r_affinetridesc.ptriangles;
        lnumtriangles = r_alias_c.r_affinetridesc.numtriangles;

        for (i = 0; i < lnumtriangles; i++, ptri++)
        {
            index0 = pfv + ptri->vertindex[0];
            index1 = pfv + ptri->vertindex[1];
            index2 = pfv + ptri->vertindex[2];

            d_xdenom = (index0->v[1] - index1->v[1]) * (index0->v[0] - index2->v[0]) - (index0->v[0] - index1->v[0]) * (index0->v[1] - index2->v[1]);

            if (d_xdenom >= 0)
            {
                continue;
            }

            r_p0[0] = index0->v[0];
            r_p0[1] = index0->v[1];
            r_p0[2] = index0->v[2];
            r_p0[3] = index0->v[3];
            r_p0[4] = index0->v[4];
            r_p0[5] = index0->v[5];

            r_p1[0] = index1->v[0];
            r_p1[1] = index1->v[1];
            r_p1[2] = index1->v[2];
            r_p1[3] = index1->v[3];
            r_p1[4] = index1->v[4];
            r_p1[5] = index1->v[5];

            r_p2[0] = index2->v[0];
            r_p2[1] = index2->v[1];
            r_p2[2] = index2->v[2];
            r_p2[3] = index2->v[3];
            r_p2[4] = index2->v[4];
            r_p2[5] = index2->v[5];

            if (ptri->facesfront == 0)
            {
                if ((index0->flags & r_shared_c.ALIAS_ONSEAM) != 0)
                {
                    r_p0[2] += r_alias_c.r_affinetridesc.seamfixupX16;
                }

                if ((index1->flags & r_shared_c.ALIAS_ONSEAM) != 0)
                {
                    r_p1[2] += r_alias_c.r_affinetridesc.seamfixupX16;
                }

                if ((index2->flags & r_shared_c.ALIAS_ONSEAM) != 0)
                {
                    r_p2[2] += r_alias_c.r_affinetridesc.seamfixupX16;
                }
            }

            D_PolysetSetEdgeTable();
            D_RasterizeAliasPolySmooth();
        }
    }

    public static void D_PolysetRecursiveTriangle(int* lp1, int* lp2, int* lp3)
    {
        int* temp;
        int d;
        int* _new = null;
        int z;
        short* zbuf;

        d = lp2[0] - lp1[0];

        if (d < -1 || d > 1)
        {
            goto split;
        }

        d = lp2[1] - lp1[1];

        if (d < -1 || d > 1)
        {
            goto split;
        }

        d = lp3[0] - lp2[0];

        if (d < -1 || d > 1)
        {
            goto split2;
        }

        d = lp3[1] - lp2[1];

        if (d < -1 || d > 1)
        {
            goto split2;
        }

        d = lp1[0] - lp3[0];

        if (d < -1 || d > 1)
        {
            goto split3;
        }

        d = lp1[1] - lp3[1];

        if (d < -1 || d > 1)
        {
            goto split3;
        }

    split3:
        temp = lp1;
        lp1 = lp3;
        lp3 = lp2;
        lp2 = temp;

        goto split;


        return;

    split2:
        temp = lp1;
        lp1 = lp2;
        lp2 = lp3;
        lp3 = temp;

    split:
        _new[0] = (lp1[0] + lp2[0]) >> 1;
        _new[1] = (lp1[1] + lp2[1]) >> 1;
        _new[2] = (lp1[2] + lp2[2]) >> 1;
        _new[3] = (lp1[3] + lp2[3]) >> 1;
        _new[5] = (lp1[5] + lp2[5]) >> 1;

        if (lp2[1] > lp1[1])
        {
            goto nodraw;
        }

        if ((lp2[1] == lp1[1]) && (lp2[0] < lp1[0]))
        {
            goto nodraw;
        }

        z = _new[5] >> 16;
        zbuf = d_modech_c.zspantable[_new[1]] + _new[0];

        if (z >= *zbuf)
        {
            int pix;

            *zbuf = (short)z;
            pix = d_pcolormap[skintable[_new[3] >> 16][_new[2] >> 16]];
            d_vars_c.d_viewbuffer[d_modech_c.d_scantable[_new[1]] + _new[0]] = (byte)pix;
        }

    nodraw:
        D_PolysetRecursiveTriangle(lp3, lp1, _new);
        D_PolysetRecursiveTriangle(lp3, _new, lp2);
    }
#endif

    public static void D_PolysetUpdateTables()
    {
        int i;
        byte* s;

        if (r_alias_c.r_affinetridesc.skinwidth != skinwidth || r_alias_c.r_affinetridesc.skinheight != *skinstart)
        {
            skinwidth = r_alias_c.r_affinetridesc.skinwidth;
            *skinstart = (byte)r_alias_c.r_affinetridesc.skinheight;
            s = skinstart;

            for (i = 0; i < d_iface_c.MAX_LBM_HEIGHT; i++, s += skinwidth)
            {
                skintable[i] = s;
            }
        }
    }

#if !id386
    public static void D_PolysetScanLeftEdge(int height)
    {
        do
        {
            d_pedgespanpackage->pdest = d_pdest;
            d_pedgespanpackage->pz = d_pz;
            d_pedgespanpackage->count = d_aspancount;
            d_pedgespanpackage->ptex = d_ptex;

            d_pedgespanpackage->sfrac = d_sfrac;
            d_pedgespanpackage->tfrac = d_tfrac;

            d_pedgespanpackage->light = d_light;
            d_pedgespanpackage->zi = d_zi;

            d_pedgespanpackage++;

            d_edge_c.errorterm += d_edge_c.erroradjustup;

            if (d_edge_c.errorterm >= 0)
            {
                d_pdest += d_pdestextrastep;
                d_pz += d_pzextrastep;
                d_aspancount += d_countextrastep;
                d_ptex += d_ptexextrastep;
                d_sfrac += d_sfracextrastep;
                d_ptex += d_sfrac >> 16;

                if ((d_tfrac & 0x10000) != 0)
                {
                    d_ptex += r_alias_c.r_affinetridesc.skinwidth;
                    d_tfrac &= 0xFFFF;
                }

                d_light += d_lightextrastep;
                d_zi += d_ziextrastep;

                d_edge_c.errorterm -= d_edge_c.erroradjustdown;
            }
            else
            {
                d_pdest += d_pdestbasestep;
                d_pz += d_pzbasestep;
                d_aspancount += d_edge_c.ubasestep;
                d_ptex += d_ptexbasestep;
                d_sfrac += d_sfracbasestep;
                d_ptex += d_sfrac >> 16;
                d_sfrac &= 0xFFFF;
                d_tfrac += d_tfracbasestep;

                if ((d_tfrac & 0x100000) != 0)
                {
                    d_ptex += r_alias_c.r_affinetridesc.skinwidth;
                    d_tfrac &= 0xFFFF;
                }

                d_light += d_lightbasestep;
                d_zi += d_zibasestep;
            }
        } while (--height != 0);
    }
#endif

    public static void D_PolysetSetUpForLineScan(int startvertu, int startvertv, int endvertu, int endvertv)
    {
        double dm, dn;
        int tm, tn;
        adivtab_t* ptemp;

        d_edge_c.errorterm = -1;

        tm = endvertu - startvertu;
        tn = endvertv - startvertv;

        if (((tm <= 16) && (tm >= -15)) && ((tn <= 16) && (tn >= -15)))
        {
            ptemp = &adivtab[((tm + 15) << 5) + (tn + 15)];
            d_edge_c.ubasestep = ptemp->quotient;
            d_edge_c.erroradjustup = ptemp->remainder;
            d_edge_c.erroradjustdown = tn;
        }
        else
        {
            dm = tm;
            dn = tn;

            mathlib_c.FloorDivMod(dm, dn, (int*)d_edge_c.ubasestep, (int*)d_edge_c.erroradjustup);

            d_edge_c.erroradjustdown = (int)dn;
        }
    }

#if !id386
    public static void D_PolysetCalcGradients(int skinwidth)
    {
        float xstepdenominv, ystepdenominv, t0, t1;
        float p01_minus_p21, p11_minus_p21, p00_minus_p20, p10_minus_p20;

        p00_minus_p20 = r_p0[0] - r_p2[0];
        p01_minus_p21 = r_p0[1] - r_p2[1];
        p10_minus_p20 = r_p1[0] - r_p2[0];
        p11_minus_p21 = r_p1[1] - r_p2[1];

        xstepdenominv = 1.0f / (float)d_xdenom;

        ystepdenominv = -xstepdenominv;

        t0 = r_p0[4] - r_p2[4];
        t1 = r_p1[4] - r_p2[4];
        r_lstepx = (int)MathF.Ceiling((t1 * p01_minus_p21 - t0 * p11_minus_p21) * xstepdenominv);
        r_lstepy = (int)MathF.Ceiling((t1 * p00_minus_p20 - t0 * p10_minus_p20) * ystepdenominv);

        t0 = r_p0[2] - r_p2[2];
        t1 = r_p1[2] - r_p2[2];
        r_sstepx = (int)((t1 * p01_minus_p21 - t0 * p11_minus_p21) * xstepdenominv);
        r_sstepy = (int)((t1 * p00_minus_p20 - t0 * p10_minus_p20) * ystepdenominv);

        t0 = r_p0[3] - r_p2[3];
        t1 = r_p1[3] - r_p2[3];
        r_tstepx = (int)((t1 * p01_minus_p21 - t0 * p11_minus_p21) * xstepdenominv);
        r_tstepy = (int)((t1 * p00_minus_p20 - t0 * p10_minus_p20) * ystepdenominv);

        t0 = r_p0[5] - r_p2[5];
        t1 = r_p1[5] - r_p2[5];
        r_zistepx = (int)((t1 * p01_minus_p21 - t0 * p11_minus_p21) * xstepdenominv);
        r_zistepy = (int)((t1 * p01_minus_p21 - t0 * p10_minus_p20) * ystepdenominv);

#if id386
        a_sstepxfrac = r_sstepx << 16;
        a_tstepxfrac = r_tstepx << 16;
#else
        a_sstepxfrac = r_sstepx & 0xFFFF;
        a_tstepxfrac = r_tstepx & 0xFFFF;
#endif

        a_ststepxwhole = skinwidth * (r_tstepx >> 16) + (r_sstepx >> 16);
    }
#endif

#if !id386
    public static void D_PolysetDrawSpans8(spanpackage_t* pspanpackage)
    {
        int lcount;
        byte* lpdest;
        byte* lptex;
        int lsfrac, ltfrac;
        int llight;
        int lzi;
        short* lpz;

        do
        {
            lcount = d_aspancount - pspanpackage->count;

            d_edge_c.errorterm += d_edge_c.erroradjustup;

            if (d_edge_c.errorterm >= 0)
            {
                d_aspancount += d_countextrastep;
                d_edge_c.errorterm -= d_edge_c.erroradjustdown;
            }
            else
            {
                d_aspancount += d_edge_c.ubasestep;
            }

            if (lcount != 0)
            {
                lpdest = (byte*)pspanpackage->pdest;
                lptex = pspanpackage->ptex;
                lpz = pspanpackage->pz;
                lsfrac = pspanpackage->sfrac;
                ltfrac = pspanpackage->tfrac;
                llight = pspanpackage->light;
                lzi = pspanpackage->zi;

                do
                {
                    if ((lzi >> 16) >= *lpz)
                    {
                        *lpdest = ((byte*)r_alias_c.acolormap)[*lptex + (llight & 0xFF00)];
                        *lpz = (short)(lzi >> 16);
                    }

                    lpdest++;
                    lzi += r_zistepx;
                    lpz++;
                    llight += r_lstepx;
                    lptex += a_ststepxwhole;
                    lsfrac += a_sstepxfrac;
                    lptex += lsfrac >> 16;
                    lsfrac &= 0xFFFF;
                    ltfrac += a_tstepxfrac;

                    if ((ltfrac & 0x100000) != 0)
                    {
                        lptex += r_alias_c.r_affinetridesc.skinwidth;
                        ltfrac &= 0xFFFF;
                    }
                } while (--lcount != 0);
            }

            pspanpackage++;
        } while (pspanpackage->count != -999999);
    }
#endif

    public static void D_PolysetFillSpans8(spanpackage_t* pspanpackage)
    {
        int color;

        color = d_aflatcolor++;

        while (true)
        {
            int lcount;
            byte* lpdest;

            lcount = pspanpackage->count;

            if (lcount == -1)
            {
                return;
            }

            if (lcount != 0)
            {
                lpdest = (byte*)pspanpackage->pdest;

                do
                {
                    *lpdest++ = (byte)color;
                } while (--lcount != 0);
            }

            pspanpackage++;
        }
    }

    public static void D_RasterizeAliasPolySmooth()
    {
        int initialleftheight, initialrightheight;
        int* plefttop, prighttop, pleftbottom, prightbottom;
        int working_lstepx, originalcount;

        plefttop = pedgetable->pleftedgevert0;
        prighttop = pedgetable->prightedgevert0;

        pleftbottom = pedgetable->pleftedgevert1;
        prightbottom = pedgetable->prightedgevert1;

        initialleftheight = pleftbottom[1] - plefttop[1];
        initialrightheight = prightbottom[1] - prighttop[1];

        D_PolysetCalcGradients(r_alias_c.r_affinetridesc.skinwidth);

        d_pedgespanpackage = a_spans;

        ystart = plefttop[1];
        d_aspancount = plefttop[0] - prighttop[0];

        d_ptex = (byte*)r_alias_c.r_affinetridesc.pskin + (plefttop[2] >> 16) + (plefttop[3] >> 16) * r_alias_c.r_affinetridesc.skinwidth;

#if id386
        d_sfrac = (plefttop[2]&0xFFFF)<<16;
        d_tfrac = (plefttop[3]&0xFFFF)<<16;
#else
        d_sfrac = plefttop[2] & 0xFFFF;
        d_tfrac = plefttop[3] & 0xFFFF;
#endif
        d_light = plefttop[4];
        d_zi = plefttop[5];

        d_pdest = (byte*)d_vars_c.d_viewbuffer + ystart * d_edge_c.screenwidth + plefttop[0];
        d_pz = d_vars_c.d_pzbuffer + ystart + d_vars_c.d_zwidth + plefttop[0];

        if (initialleftheight == 1)
        {
            d_pedgespanpackage->pdest = d_pdest;
            d_pedgespanpackage->pz = d_pz;
            d_pedgespanpackage->count = d_aspancount;
            d_pedgespanpackage->ptex = d_ptex;

            d_pedgespanpackage->sfrac = d_sfrac;
            d_pedgespanpackage->tfrac = d_tfrac;

            d_pedgespanpackage->light = d_light;
            d_pedgespanpackage->zi = d_zi;

            d_pedgespanpackage++;
        }
        else
        {
            D_PolysetSetUpForLineScan(plefttop[0], plefttop[1], pleftbottom[0], pleftbottom[1]);

#if id386
            d_pzbasestep = (d_zwidth + ubasestep) << 1;
            d_pzextrastep = d_pzbasestep + 2,
#else
            d_pzbasestep = (int)(d_vars_c.d_zwidth + d_edge_c.ubasestep);
            d_pzextrastep = d_pzbasestep + 1;
#endif

            d_pdestbasestep = d_edge_c.screenwidth + d_edge_c.ubasestep;
            d_pdestextrastep = d_pdestbasestep + 1;

            if (d_edge_c.ubasestep < 0)
            {
                working_lstepx = r_lstepx - 1;
            }
            else
            {
                working_lstepx = r_lstepx;
            }

            d_countextrastep = d_edge_c.ubasestep + 1;
            d_ptexbasestep = ((r_sstepy + r_sstepx * d_edge_c.ubasestep) >> 16) + ((r_tstepy + r_tstepx * d_edge_c.ubasestep) >> 16) * r_alias_c.r_affinetridesc.skinwidth;

#if id386
            d_sfracbasestep = (r_sstepy + r_sstepx * d_edge_c.ubasestep) << 16;
            d_tfracbasestep = (r_tstepy + r_tstepx * d_edge_c.ubasestep) << 16;
#else
            d_sfracbasestep = (r_sstepy + r_sstepx * d_edge_c.ubasestep) & 0xFFFF;
            d_tfracbasestep = (r_tstepy + r_tstepx * d_edge_c.ubasestep) & 0xFFFF;
#endif
            d_lightbasestep = r_lstepy + working_lstepx * d_edge_c.ubasestep;
            d_zibasestep = r_zistepy + r_zistepx * d_edge_c.ubasestep;

            d_ptexextrastep = ((r_sstepy + r_sstepx * d_countextrastep) >> 16) + ((r_tstepy + r_tstepx * d_countextrastep) >> 16) * r_alias_c.r_affinetridesc.skinwidth;

#if id386
            d_sfracextrastep = (r_sstepy + r_sstepx * d_countextrastep) << 16;
            d_tfracextrastep = (r_tstepy + r_tstepx * d_countextrastep) << 16;
#else
            d_sfracextrastep = (r_sstepy + r_sstepx * d_countextrastep) & 0xFFFF;
            d_tfracextrastep = (r_tstepy + r_tstepy * d_countextrastep) & 0xFFFF;
#endif
            d_lightextrastep = d_lightbasestep + working_lstepx;
            d_ziextrastep = d_zibasestep + r_zistepx;

            D_PolysetScanLeftEdge(initialleftheight);
        }

        if (pedgetable->numleftedges == 2)
        {
            int height;

            plefttop = pleftbottom;
            pleftbottom = pedgetable->pleftedgevert2;

            height = pleftbottom[1] - plefttop[1];

            ystart = plefttop[1];
            d_aspancount = plefttop[0] - prighttop[0];
            d_ptex = (byte*)r_alias_c.r_affinetridesc.pskin + (plefttop[2] >> 16) + (plefttop[3] >> 16) * r_alias_c.r_affinetridesc.skinwidth;
            d_sfrac = 0;
            d_tfrac = 0;
            d_light = plefttop[4];
            d_zi = plefttop[5];

            d_pdest = (byte*)d_vars_c.d_viewbuffer + ystart * d_edge_c.screenwidth + plefttop[0];
            d_pz = d_vars_c.d_pzbuffer + ystart * d_vars_c.d_zwidth + plefttop[0];

            if (height == 1)
            {
                d_pedgespanpackage->pdest = d_pdest;
                d_pedgespanpackage->pz = d_pz;
                d_pedgespanpackage->count = d_aspancount;
                d_pedgespanpackage->ptex = d_ptex;

                d_pedgespanpackage->sfrac = d_sfrac;
                d_pedgespanpackage->tfrac = d_tfrac;

                d_pedgespanpackage->light = d_light;
                d_pedgespanpackage->zi = d_zi;

                d_pedgespanpackage++;
            }
            else
            {
                D_PolysetSetUpForLineScan(plefttop[0], plefttop[1], pleftbottom[0], pleftbottom[1]);

                d_pdestbasestep = d_edge_c.screenwidth + d_edge_c.ubasestep;
                d_pdestextrastep = d_pdestbasestep + 1;

#if id386
                d_pzbasestep = (d_vars_c.d_zwidth + d_edge_c.ubasestep) << 1;
                d_pzextrastep = d_pzbasestep + 1;
#else
                d_pzbasestep = (int)d_vars_c.d_zwidth + d_edge_c.ubasestep;
                d_pzextrastep = d_pzbasestep + 1;
#endif

                if (d_edge_c.ubasestep < 0)
                {
                    working_lstepx = r_lstepx - 1;
                }
                else
                {
                    working_lstepx = r_lstepx;
                }

                d_countextrastep = d_edge_c.ubasestep + 1;
                d_ptexbasestep = ((r_sstepy + r_sstepx * d_edge_c.ubasestep) >> 16) + ((r_tstepy + r_tstepx * d_edge_c.ubasestep) >> 16) * r_alias_c.r_affinetridesc.skinwidth;

#if id386
                d_sfracbasestep = (r_sstepy + r_sstepx * d_edge_c.ubasestep) << 16;
                d_tfracbasestep = (r_tstepy + r_tstepx * d_edge_c.ubasestep) << 16;
#else
                d_sfracbasestep = (r_sstepy + r_sstepx * d_edge_c.ubasestep) & 0xFFFF;
                d_tfracbasestep = (r_tstepy + r_tstepx * d_edge_c.ubasestep) & 0xFFFF;
#endif
                d_lightbasestep = r_lstepy + working_lstepx * d_edge_c.ubasestep;
                d_zibasestep = r_zistepy + r_zistepx * d_edge_c.ubasestep;

                d_ptexextrastep = ((r_sstepy + r_sstepx * d_countextrastep) >> 16) + ((r_tstepy + r_tstepx * d_countextrastep) >> 16) * r_alias_c.r_affinetridesc.skinwidth;

#if id386
                d_sfracextrastep = ((r_sstepy+r_sstepx*d_countextrastep) & 0xFFFF)<<16;
			    d_tfracextrastep = ((r_tstepy+r_tstepx*d_countextrastep) & 0xFFFF)<<16;
#else
                d_sfracbasestep = (r_sstepy + r_sstepx * d_countextrastep) & 0xFFFF;
                d_tfracbasestep = (r_tstepy + r_tstepx * d_countextrastep) & 0xFFFF;
#endif
                d_lightextrastep = d_lightbasestep + working_lstepx;
                d_ziextrastep = d_zibasestep + r_zistepx;

                D_PolysetScanLeftEdge(height);
            }
        }

        d_pedgespanpackage = a_spans;

        D_PolysetSetUpForLineScan(prighttop[0], prighttop[1], prightbottom[0], prightbottom[1]);

        d_aspancount = 0;
        d_countextrastep = d_edge_c.ubasestep + 1;
        originalcount = a_spans[initialrightheight].count;
        a_spans[initialrightheight].count = -999999;
        D_PolysetDrawSpans8(a_spans);

        if (pedgetable->numrightedges == 2)
        {
            int height = 2;
            spanpackage_t* pstart;

            pstart = a_spans + initialrightheight;
            pstart->count = originalcount;

            d_aspancount = prightbottom[0] - prighttop[0];

            prighttop = prightbottom;
            prightbottom = pedgetable->prightedgevert2;

            height = prightbottom[1] - prighttop[1];

            D_PolysetSetUpForLineScan(prighttop[0], prighttop[1], prightbottom[0], prightbottom[1]);

            d_countextrastep = d_edge_c.ubasestep + 1;
            a_spans[initialrightheight + height].count = -999999;

            D_PolysetDrawSpans8(pstart);
        }
    }

    public static void D_PolysetSetEdgeTable()
    {
        int edgetableindex;

        edgetableindex = 0;

        if (r_p0[1] >= r_p1[1])
        {
            if (r_p0[1] == r_p1[1])
            {
                if (r_p0[1] < r_p2[1])
                {
                    *pedgetable = edgetables[2];
                }
                else
                {
                    *pedgetable = edgetables[5];
                }

                return;
            }
            else
            {
                edgetableindex = 1;
            }
        }

        if (r_p0[1] == r_p2[1])
        {
            if (edgetableindex != 0)
            {
                *pedgetable = edgetables[8];
            }
            else
            {
                *pedgetable = edgetables[9];
            }

            return;
        }
        else if (r_p1[1] == r_p2[1])
        {
            if (edgetableindex != 0)
            {
                *pedgetable = edgetables[10];
            }
            else
            {
                *pedgetable = edgetables[11];
            }

            return;
        }

        if (r_p0[1] > r_p2[1])
        {
            edgetableindex += 2;
        }

        if (r_p1[1] > r_p2[1])
        {
            edgetableindex += 4;
        }

        *pedgetable = edgetables[edgetableindex];
    }
}