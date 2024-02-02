namespace Quake;

public unsafe class r_surf_c
{
    public static d_iface_c.drawsurf_t r_drawsurf;

    public static int lightleft, sourcesstep, blocksize, sourcetstep;
    public static int lightdelta, lightdeltastep;
    public static int lightright, lightleftstep, lightrightstep, blockdivshift;
    public static uint blockdivmask;
    public static void* prowdestbase;
    public static char* pbasesource;
    public static int surfrowbytes;
    public static uint* r_lightptr;
    public static int r_stepback;
    public static int r_lightwidth;
    public static int r_numhblocks, r_numvblocks;
    public static char* r_source, r_sourcemax;

    public static Action* surfmiptable;

    public static uint* blocklights;

    public static void R_AddDynamicLights()
    {
        model_c.msurface_t* surf;
        int lnum;
        int sd, td;
        float dist, rad, minlight;
        Vector3 impact, local;
        int s, t;
        int i;
        int smax, tmax;
        model_c.mtexinfo_t* tex;

        impact = local = new();

        surf = r_drawsurf.surf;
        smax = (surf->extents[0] >> 4) + 1;
        tmax = (surf->extents[1] >> 4) + 1;
        tex = surf->texinfo;

        for (lnum = 0; lnum < client_c.MAX_DLIGHTS; lnum++)
        {
            if ((surf->dlightbits & (1 << lnum)) == 0)
            {
                continue;
            }

            rad = cl_main_c.cl_dlights[lnum].radius;
            dist = mathlib_c.DotProduct(cl_main_c.cl_dlights[lnum].origin, surf->plane->normal) - surf->plane->dist;
            rad -= MathF.Abs(dist);
            minlight = cl_main_c.cl_dlights[lnum].minlight;

            if (rad < minlight)
            {
                continue;
            }

            minlight = rad - minlight;

            for (i = 0; i < 3; i++)
            {
                impact[i] = cl_main_c.cl_dlights[lnum].origin[i] - surf->plane->normal[i] * dist;
            }

            local[0] = mathlib_c.DotProduct(impact, tex->vecs[0]) + tex->vecs[0][3];
            local[1] = mathlib_c.DotProduct(impact, tex->vecs[1]) + tex->vecs[1][3];

            local[0] -= surf->texturemins[0];
            local[1] -= surf->texturemins[1];

            for (t = 0; t < tmax; t++)
            {
                td = (int)local[1] - t * 16;

                if (td < 0)
                {
                    td = -td;
                }

                for (s = 0; s < smax; s++)
                {
                    sd = (int)local[0] - s * 16;

                    if (sd < 0)
                    {
                        sd = -sd;
                    }

                    if (sd > td)
                    {
                        dist = sd + (td >> 1);
                    }
                    else
                    {
                        dist = td + (sd >> 1);
                    }

                    if (dist < minlight)
#if QUAKE2
                    {
                        uint temp;
                        temp = (uint)(rad - dist) * 256;
                        i = t * smax + s;

                        if (!cl_main_c.cl_dlights[i].dark)
                        {
                            blocklights[i] += temp;
                        }
                        else
                        {
                            if (blocklights[i] > temp)
                            {
                                blocklights[i] -= temp;
                            }
                            else
                            {
                                blocklights[i] = 0;
                            }
                        }
                    }
#else
                    {
                        blocklights[t * smax + s] += (uint)(rad - dist) * 256;
                    }
#endif
                }
            }
        }
    }

    public static void R_BuildLightMap()
    {
        int smax, tmax;
        int t;
        int i, size;
        byte* lightmap;
        uint scale;
        int maps;
        model_c.msurface_t* surf;

        surf = r_drawsurf.surf;

        smax = (surf->extents[0] >> 4) + 1;
        tmax = (surf->extents[1] >> 4) + 1;
        size = smax * tmax;
        lightmap = surf->samples;

        if (r_main_c.r_fullbright.value != 0 || cl_main_c.cl.worldmodel->lightdata == null)
        {
            for (i = 0; i < size; i++)
            {
                blocklights[i] = 0;
            }

            return;
        }

        for (i = 0; i < size; i++)
        {
            blocklights[i] = (uint)r_main_c.r_refdef.ambientlight << 8;
        }

        if (lightmap != null)
        {
            for (maps = 0; maps < bspfile_c.MAXLIGHTMAPS && surf->styles[maps] != 255; maps++)
            {
                scale = (uint)r_drawsurf.lightadj[maps];

                for (i = 0; i < size; i++)
                {
                    blocklights[i] += lightmap[i] * scale;
                }

                lightmap += size;
            }
        }

        if (surf->dlightframe == r_main_c.r_framecount)
        {
            R_AddDynamicLights();
        }

        for (i = 0; i < size; i++)
        {
            t = (255 * 256 - (int)blocklights[i]) >> (8 - vid_c.VID_CBITS);

            if (t < (1 << 6))
            {
                t = (1 << 6);
            }

            blocklights[i] = (uint)t;
        }
    }

    public static model_c.texture_t* R_TextureAnimation(model_c.texture_t* tbase)
    {
        int reletive;
        int count;

        if (r_bsp_c.currententity->frame != 0)
        {
            if (tbase->alternate_anims != null)
            {
                tbase = tbase->alternate_anims;
            }
        }

        if (tbase->anim_total == 0)
        {
            return tbase;
        }

        reletive = (int)(cl_main_c.cl.time * 10) % tbase->anim_total;

        count = 0;

        while (tbase->anim_min > reletive || tbase->anim_max <= reletive)
        {
            tbase = tbase->anim_next;

            if (tbase == null)
            {
                sys_win_c.Sys_Error("R_TextureAnimation: broken cycle");
            }

            if (++count > 100)
            {
                sys_win_c.Sys_Error("R_TextureAnimation: infinite cycle");
            }
        }

        return tbase;
    }

    public static void R_DrawSurface()
    {
        char* basetptr;
        int smax, tmax, twidth;
        int u;
        int soffset, basetoffset, texwidth;
        int horzblockstep;
        char* pcolumndest;
        Action* pblockdrawer;
        model_c.texture_t* mt;

        pblockdrawer = null;

        R_BuildLightMap();

        surfrowbytes = r_drawsurf.rowbytes;

        mt = r_drawsurf.texture;

        r_source = (char*)mt + mt->offsets[r_drawsurf.surfmip];

        texwidth = (int)mt->width >> r_drawsurf.surfmip;

        blocksize = 16 >> r_drawsurf.surfmip;
        blockdivshift = 4 - r_drawsurf.surfmip;
        blockdivmask = (uint)(1 << blockdivshift) - 1;

        r_lightwidth = r_drawsurf.surfwidth >> blockdivshift;

        r_numhblocks = r_drawsurf.surfwidth >> blockdivshift;
        r_numvblocks = r_drawsurf.surfheight >> blockdivshift;

        if (r_main_c.r_pixbytes == 1)
        {
            pblockdrawer = &surfmiptable[r_drawsurf.surfmip];
            horzblockstep = blocksize;
        }
        else
        {
            *pblockdrawer = R_DrawSurfaceBlock16;
            horzblockstep = blocksize << 1;
        }

        smax = (int)mt->width >> r_drawsurf.surfmip;
        twidth = texwidth;
        tmax = (int)mt->height >> r_drawsurf.surfmip;
        sourcetstep = texwidth;
        r_stepback = tmax * twidth;

        r_sourcemax = r_source + (tmax * smax);

        soffset = r_drawsurf.surf->texturemins[0];
        basetoffset = r_drawsurf.surf->texturemins[1];

        soffset = ((soffset >> r_drawsurf.surfmip) + (smax << 16)) % smax;
        basetptr = &r_source[((basetoffset >> r_drawsurf.surfmip) + (tmax << 16)) % tmax * twidth];

        pcolumndest = (char*)r_drawsurf.surfdat;

        for (u = 0; u < r_numhblocks; u++)
        {
            r_lightptr = blocklights + u;

            prowdestbase = pcolumndest;

            pbasesource = basetptr + soffset;

            (*pblockdrawer)();

            soffset = soffset + blocksize;

            if (soffset >= smax)
            {
                soffset = 0;
            }

            pcolumndest += horzblockstep;
        }
    }

#if !id386
    public static void R_DrawSurfaceBlock8_mip0()
    {
        int v, i, b, lightstep, lighttemp, light;
        char pix;
        char* psource, prowdest;

        psource = pbasesource;
        prowdest = (char*)prowdestbase;

        for (v = 0; v < r_numvblocks; v++)
        {
            lightleft = (int)r_lightptr[0];
            lightright = (int)r_lightptr[1];
            r_lightptr += r_lightwidth;
            lightleftstep = (int)(r_lightptr[0] - lightleft) >> 4;
            lightrightstep = (int)(r_lightptr[1] - lightright) >> 4;

            for (i = 0; i < 16; i++)
            {
                lighttemp = lightleft - lightright;
                lightstep = lighttemp >> 4;

                light = lightright;

                for (b = 15; b >= 0; b--)
                {
                    pix = psource[b];
                    prowdest[b] = ((char*)vid_c.vid.colormap)[(light & 0xFF00) + pix];
                    light += lightstep;
                }

                psource += sourcetstep;
                lightright += lightrightstep;
                lightleft += lightleftstep;
                prowdest += surfrowbytes;
            }

            if (psource >= r_sourcemax)
            {
                psource -= r_stepback;
            }
        }
    }

    public static void R_DrawSurfaceBlock8_mip1()
    {
        int v, i, b, lightstep, lighttemp, light;
        char pix;
        char* psource, prowdest;

        psource = pbasesource;
        prowdest = (char*)prowdestbase;

        for (v = 0; v < r_numvblocks; v++)
        {
            lightleft = (int)r_lightptr[0];
            lightright = (int)r_lightptr[1];
            r_lightptr += r_lightwidth;
            lightleftstep = (int)(r_lightptr[0] - lightleft) >> 4;
            lightrightstep = (int)(r_lightptr[1] - lightright) >> 4;

            for (i = 0; i < 8; i++)
            {
                lighttemp = lightleft - lightright;
                lightstep = lighttemp >> 4;

                light = lightright;

                for (b = 7; b >= 0; b--)
                {
                    pix = psource[b];
                    prowdest[b] = ((char*)vid_c.vid.colormap)[(light & 0xFF00) + pix];
                    light += lightstep;
                }

                psource += sourcetstep;
                lightright += lightrightstep;
                lightleft += lightleftstep;
                prowdest += surfrowbytes;
            }

            if (psource >= r_sourcemax)
            {
                psource -= r_stepback;
            }
        }
    }

    public static void R_DrawSurfaceBlock8_mip2()
    {
        int v, i, b, lightstep, lighttemp, light;
        char pix;
        char* psource, prowdest;

        psource = pbasesource;
        prowdest = (char*)prowdestbase;

        for (v = 0; v < r_numvblocks; v++)
        {
            lightleft = (int)r_lightptr[0];
            lightright = (int)r_lightptr[1];
            r_lightptr += r_lightwidth;
            lightleftstep = (int)(r_lightptr[0] - lightleft) >> 4;
            lightrightstep = (int)(r_lightptr[1] - lightright) >> 4;

            for (i = 0; i < 4; i++)
            {
                lighttemp = lightleft - lightright;
                lightstep = lighttemp >> 4;

                light = lightright;

                for (b = 3; b >= 0; b--)
                {
                    pix = psource[b];
                    prowdest[b] = ((char*)vid_c.vid.colormap)[(light & 0xFF00) + pix];
                    light += lightstep;
                }

                psource += sourcetstep;
                lightright += lightrightstep;
                lightleft += lightleftstep;
                prowdest += surfrowbytes;
            }

            if (psource >= r_sourcemax)
            {
                psource -= r_stepback;
            }
        }
    }

    public static void R_DrawSurfaceBlock8_mip3()
    {
        int v, i, b, lightstep, lighttemp, light;
        char pix;
        char* psource, prowdest;

        psource = pbasesource;
        prowdest = (char*)prowdestbase;

        for (v = 0; v < r_numvblocks; v++)
        {
            lightleft = (int)r_lightptr[0];
            lightright = (int)r_lightptr[1];
            r_lightptr += r_lightwidth;
            lightleftstep = (int)(r_lightptr[0] - lightleft) >> 4;
            lightrightstep = (int)(r_lightptr[1] - lightright) >> 4;

            for (i = 0; i < 16; i++)
            {
                lighttemp = lightleft - lightright;
                lightstep = lighttemp >> 4;

                light = lightright;

                for (b = 15; b >= 0; b--)
                {
                    pix = psource[b];
                    prowdest[b] = ((char*)vid_c.vid.colormap)[(light & 0xFF00) + pix];
                    light += lightstep;
                }

                psource += sourcetstep;
                lightright += lightrightstep;
                lightleft += lightleftstep;
                prowdest += surfrowbytes;
            }

            if (psource >= r_sourcemax)
            {
                psource -= r_stepback;
            }
        }
    }

    public static void R_DrawSurfaceBlock16()
    {
        int k;
        char* psource;
        int lighttemp, lightstep, light;
        ushort* prowdest;

        prowdest = (ushort*)prowdestbase;

        for (k = 0; k < blocksize; k++)
        {
            ushort* pdest;
            char pix;
            int b;

            psource = pbasesource;
            lighttemp = lightright - lightleft;
            lightstep = lighttemp >> blockdivshift;

            light = lightleft;
            pdest = prowdest;

            for (b = 0; b < blocksize; b++)
            {
                pix = *psource;
                *pdest = vid_c.vid.colormap16[(light & 0xFF00) + pix];
                psource += sourcesstep;
                pdest++;
                light += lightstep;
            }

            pbasesource += sourcetstep;
            lightright += lightrightstep;
            lightleft += lightleftstep;
            prowdest = (ushort*)((long)prowdest + surfrowbytes);
        }

        prowdestbase = prowdest;
    }
#endif

    public static void R_GenTurbTile(byte* pbasetex, void* pdest)
    {
        int* turb;
        int i, j, s, t;
        byte* pd;

        turb = (int*)r_draw_c.sintable + ((int)(cl_main_c.cl.time * r_local_c.SPEED) & (d_iface_c.CYCLE - 1));
        pd = (byte*)pdest;

        for (i = 0; i < d_iface_c.TILE_SIZE; i++)
        {
            for (j = 0; j < d_iface_c.TILE_SIZE; j++)
            {
                s = (((j << 16) + turb[i & (d_iface_c.CYCLE - 1)]) >> 16) & 63;
                t = (((i << 16) + turb[j & (d_iface_c.CYCLE - 1)]) >> 16) & 63;
                *pd++ = *(pbasetex + (t << 6) + s);
            }
        }
    }

    public static void R_GenTurbTile16(byte* pbasetex, void* pdest)
    {
        int* turb;
        int i, j, s, t;
        ushort* pd;

        turb = (int*)r_draw_c.sintable + ((int)(cl_main_c.cl.time * r_local_c.SPEED) & (d_iface_c.CYCLE - 1));
        pd = (ushort*)pdest;

        for (i = 0; i < d_iface_c.TILE_SIZE; i++)
        {
            for (j = 0; j < d_iface_c.TILE_SIZE; j++)
            {
                s = (((j << 16) + turb[i & (d_iface_c.CYCLE - 1)]) >> 16) & 63;
                t = (((i << 16) + turb[j & (d_iface_c.CYCLE - 1)]) >> 16) & 63;
                *pd++ = vid_c.d_8to16table[*(pbasetex + (t << 6) + s)];
            }
        }
    }

    public static void R_GenTile(model_c.msurface_t* psurf, void* pdest)
    {
        if ((psurf->flags & model_c.SURF_DRAWTURB) != 0)
        {
            if (r_main_c.r_pixbytes == 1)
            {
                R_GenTurbTile((byte*)psurf->texinfo->texture + psurf->texinfo->texture->offsets[0], pdest);
            }
            else
            {
                R_GenTurbTile16((byte*)psurf->texinfo->texture + psurf->texinfo->texture->offsets[0], pdest);
            }
        }
        else if ((psurf->flags & model_c.SURF_DRAWSKY) != 0)
        {
            if (r_main_c.r_pixbytes == 1)
            {
                r_sky_c.R_GenSkyTile(pdest);
            }
            else
            {
                r_sky_c.R_GenSkyTile16(pdest);
            }
        }
        else
        {
            sys_win_c.Sys_Error("Unknown tile type");
        }
    }
}