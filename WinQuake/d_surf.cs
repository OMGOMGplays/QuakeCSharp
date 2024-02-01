namespace Quake;

public unsafe class d_surf_c
{
    public static float surfscale;
    public static bool r_cache_thrash;

    public static int sc_size;
    public static d_local_c.surfcache_t* sc_rover, sc_base;

    public const int GUARDSIZE = 4;

    public static int D_SurfaceCacheForRes(int width, int height)
    {
        int size, pix;

        if (common_c.COM_CheckParm("-surfcachesize") != 0)
        {
            size = common_c.Q_atoi(common_c.com_argv[common_c.COM_CheckParm("-surfcachesize") + 1]) * 1024;
            return size;
        }

        size = d_local_c.SURFCACHE_SIZE_AT_320X200;

        pix = width * height;

        if (pix > 64000)
        {
            size += (pix - 64000) * 3;
        }

        return size;
    }

    public static void D_CheckCacheGuard()
    {
        byte* s;
        int i;

        s = (byte*)sc_base + sc_size;

        for (i = 0; i < GUARDSIZE; i++)
        {
            if (s[i] != (byte)i)
            {
                sys_win_c.Sys_Error("D_CheckCacheGuard: failed");
            }
        }
    }

    public static void D_ClearCacheGuard()
    {
        byte* s;
        int i;

        s = (byte*)sc_base + sc_size;

        for (i = 0; i < GUARDSIZE; i++)
        {
            s[i] = (byte)i;
        }
    }

    public static void D_InitCaches(void* buffer, int size)
    {
        if (!common_c.msg_surpress_1)
        {
            console_c.Con_Printf($"{size / 1024}k surface cache\n");
        }

        sc_size = size - GUARDSIZE;
        sc_base = (d_local_c.surfcache_t*)buffer;
        sc_rover = sc_base;

        sc_base->next = null;
        sc_base->owner = null;
        sc_base->size = sc_size;

        D_ClearCacheGuard();
    }

    public static void D_FlushCaches()
    {
        glquake_c.surfcache_t* c;

        if (sc_base == null)
        {
            return;
        }

        for (c = &sc_base; c != null; c = c->next)
        {
            if (c->owner != null)
            {
                *c->owner = null;
            }
        }

        sc_rover = sc_base;
        sc_base->next = null;
        sc_base->owner = null;
        sc_base->size = sc_size;
    }

    public static d_local_c.surfcache_t* D_SCAlloc(int width, int size)
    {
        d_local_c.surfcache_t* snew;
        bool wrapped_this_time;

        if ((width < 0) || (width > 256))
        {
            sys_win_c.Sys_Error($"D_SCAlloc: bad cache width {width}\n");
        }

        if ((size < 0) || (size > 0x10000))
        {
            sys_win_c.Sys_Error($"D_SCAlloc: bad cache size {size}\n");
        }

        size = (int)&((glquake_c.surfcache_t*)0)->data[size];
        size = (size + 3) & ~3;

        if (size > sc_size)
        {
            sys_win_c.Sys_Error($"D_SCAlloc: {size} > cache size");
        }

        wrapped_this_time = false;

        if (sc_rover == null || (byte*)sc_rover - (byte*)sc_base > sc_size - size)
        {
            if (sc_rover != null)
            {
                wrapped_this_time = true;
            }

            sc_rover = sc_base;
        }

        snew = sc_rover;

        if (sc_rover->owner != null)
        {
            *sc_rover->owner = null;
        }

        while (snew->size < size)
        {
            sc_rover = sc_rover->next;

            if (sc_rover == null)
            {
                sys_win_c.Sys_Error("D_SCAlloc: hit the end of memory");
            }

            if (sc_rover->owner != null)
            {
                *sc_rover->owner = null;
            }

            snew->size += sc_rover->size;
            snew->next = sc_rover->next;
        }

        if (snew->size - size > 256)
        {
            sc_rover = (d_local_c.surfcache_t*)((byte*)snew + size);
            sc_rover->size = snew->size - size;
            sc_rover->next = snew->next;
            sc_rover->width = 0;
            sc_rover->owner = null;
            snew->next = sc_rover;
            snew->size = size;
        }
        else
        {
            sc_rover = snew->next;
        }

        snew->width = (uint)width;

        if (width > 0)
        {
            snew->height = (uint)((size - sizeof(d_local_c.surfcache_t)) / width);
        }

        snew->owner = null;

        if (d_init_c.d_roverwrapped)
        {
            if (wrapped_this_time || (sc_rover >= d_init_c.d_initial_rover))
            {
                r_cache_thrash = true;
            }
        }
        else if (wrapped_this_time)
        {
            d_init_c.d_roverwrapped = true;
        }

        D_CheckCacheGuard();
        return snew;
    }

    public static void D_SCDump()
    {
        d_local_c.surfcache_t* test;

        for (test = sc_base; test != null; test = test->next)
        {
            if (test == sc_rover)
            {
                sys_win_c.Sys_Printf("ROVER:\n");
            }

            Console.WriteLine($"{*test} : {test->size} bytes    {test->width} width\n");
        }
    }

    public static int MaskForNum(int num)
    {
        if (num == 128)
        {
            return 127;
        }

        if (num == 64)
        {
            return 63;
        }

        if (num == 32)
        {
            return 31;
        }

        if (num == 16)
        {
            return 15;
        }

        return 255;
    }

    public static int D_log2(int num)
    {
        int c;

        c = 0;

        while ((num >>= 1) != 0)
        {
            c++;
        }

        return c;
    }

    public static d_local_c.surfcache_t* D_CacheSurface(model_c.msurface_t* surface, int miplevel)
    {
        d_local_c.surfcache_t* cache;

        r_surf_c.r_drawsurf.texture = r_surf_c.R_TextureAnimation(surface->texinfo->texture);
        r_surf_c.r_drawsurf.lightadj[0] = r_main_c.d_lightstylevalue[surface->styles[0]];
        r_surf_c.r_drawsurf.lightadj[1] = r_main_c.d_lightstylevalue[surface->styles[1]];
        r_surf_c.r_drawsurf.lightadj[2] = r_main_c.d_lightstylevalue[surface->styles[2]];
        r_surf_c.r_drawsurf.lightadj[3] = r_main_c.d_lightstylevalue[surface->styles[3]];

        cache = &surface->cachespots[miplevel];

        if (cache != null && cache->dlight == null && surface->dlightframe != d_iface_c.r_framecount && cache->texture == r_surf_c.r_drawsurf.texture && cache->lightadj[0] == r_surf_c.r_drawsurf.lightadj[0] && cache->lightadj[1] == r_surf_c.r_drawsurf.lightadj[1] && cache->lightadj[2] == r_surf_c.r_drawsurf.lightadj[2] && cache->lightadj[3] == r_surf_c.r_drawsurf.lightadj[3])
        {
            return cache;
        }

        surfscale = 1.0f / (1 << miplevel);
        r_surf_c.r_drawsurf.surfmip = miplevel;
        r_surf_c.r_drawsurf.surfwidth = surface->extents[0] >> miplevel;
        r_surf_c.r_drawsurf.rowbytes = r_surf_c.r_drawsurf.surfwidth;
        r_surf_c.r_drawsurf.surfheight = surface->extents[1] >> miplevel;

        if (cache == null)
        {
            cache = D_SCAlloc(r_surf_c.r_drawsurf.surfwidth, r_surf_c.r_drawsurf.surfwidth * r_surf_c.r_drawsurf.surfheight);
            surface->cachespots[miplevel] = *cache;
            //cache->owner = surface->cachespots[miplevel];s
            cache->mipscale = surfscale;
        }

        if (surface->dlightframe == d_iface_c.r_framecount)
        {
            cache->dlight = 1;
        }
        else
        {
            cache->dlight = 0;
        }

        r_surf_c.r_drawsurf.surfdat = (byte*)cache->data;

        cache->texture = r_surf_c.r_drawsurf.texture;
        cache->lightadj[0] = r_surf_c.r_drawsurf.lightadj[0];
        cache->lightadj[1] = r_surf_c.r_drawsurf.lightadj[1];
        cache->lightadj[2] = r_surf_c.r_drawsurf.lightadj[2];
        cache->lightadj[3] = r_surf_c.r_drawsurf.lightadj[3];

        r_surf_c.r_drawsurf.surf = surface;

        r_main_c.c_surf++;
        r_surf_c.R_DrawSurface();

        return &surface->cachespots[miplevel];
    }
}