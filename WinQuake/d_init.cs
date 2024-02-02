namespace Quake;

public unsafe class d_init_c
{
    public const int NUM_MIPS = 4;

    public static cvar_c.cvar_t d_subdiv16 = new cvar_c.cvar_t { name = "d_subdiv16", value = (char)1 };
    public static cvar_c.cvar_t d_mipcap = new cvar_c.cvar_t { name = "d_mipcap", value = (char)0 };
    public static cvar_c.cvar_t d_mipscale = new cvar_c.cvar_t { name = "d_mipscale", value = (char)1 };

    public static d_local_c.surfcache_t* d_initial_rover;
    public static bool d_roverwrapped;
    public static int d_minmip;
    public static float* d_scalemip;

    public static float[] basemip = { 1.0f, 0.5f * 0.8f, 0.25f * 0.8f };

    public static int d_aflatcolor;

    public static Action* d_drawspans;

    public static void D_Init()
    {
        r_sky_c.r_skydirect = 1;

        cvar_c.Cvar_RegisterVariable(d_subdiv16);
        cvar_c.Cvar_RegisterVariable(d_mipcap);
        cvar_c.Cvar_RegisterVariable(d_mipscale);

        r_main_c.r_drawpolys = false;
        r_main_c.r_worldpolysbacktofront = false;
        r_main_c.r_recursiveaffinetriangles = true;
        r_main_c.r_pixbytes = 1;
        r_main_c.r_aliasuvscale = 1.0f;
    }

    public static void D_CopyRects(vid_c.vrect_t* prects, int transparent)
    {
        quakedef_c.UNUSED(*prects);
        quakedef_c.UNUSED(transparent);
    }

    public static void D_EnableBackBufferAccess()
    {
        vid_win_c.VID_LockBuffer();
    }

    public static void D_TurnZOn()
    {

    }

    public static void D_DisableBackBufferAccess()
    {
        vid_win_c.VID_UnlockBuffer();
    }

    public static void D_SetupFrame()
    {
        int i;

        if (r_main_c.r_dowarp)
        {
            d_local_c.d_viewbuffer = r_main_c.r_warpbuffer;
        }
        else
        {
            d_local_c.d_viewbuffer = vid_c.vid.buffer;
        }

        if (r_main_c.r_dowarp)
        {
            r_main_c.screenwidth = d_iface_c.WARP_WIDTH;
        }
        else
        {
            r_main_c.screenwidth = (int)vid_c.vid.rowbytes;
        }

        d_roverwrapped = false;
        d_initial_rover = d_local_c.sc_rover;

        d_minmip = d_mipcap.value;

        if (d_minmip > 3)
        {
            d_minmip = 3;
        }
        else if (d_minmip < 0)
        {
            d_minmip = 0;
        }

        for (i = 0; i < (NUM_MIPS - 1); i++)
        {
            d_scalemip[i] = basemip[i] * d_mipscale.value;
        }
#if id386
            if (d_subdiv16.value != 0)
            {
                d_drawspans = d_scan_c.D_DrawSpans16;
            }
            else 
            {
                d_drawspans = d_scan_c.D_DrawSpans8;
            }
#else
        d_drawspans = d_scan_c.D_DrawSpans8;
#endif

        d_aflatcolor = 0;
    }

    public static void D_UpdateRects(vid_c.vrect_t* prect)
    {
        bothdefs_c.UNUSED(*prect);
    }
}