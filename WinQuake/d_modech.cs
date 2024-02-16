namespace Quake;

public unsafe class d_modech_c
{
    public static int d_vrectx, d_vrecty, d_vrectright_particle, d_vrectbottom_particle;

    public static int d_y_aspect_shift, d_pix_min, d_pix_max, d_pix_shift;

    public static int* d_scantable;
    public static short** zspantable;

    public static void D_Patch()
    {
#if id386
        bool protectset8 = false;

        if (!protectset8)
        {
            sys_win_c.Sys_MakeCodeWriteable((int)D_PolysetAff8Start, (int)D_PolysetAff8End - (int)D_PolysetAff8Start);

            protectset8 = true;
        }
#endif
    }

    public static void D_ViewChanged()
    {
        int rowbytes;

        if (r_main_c.r_dowarp)
        {
            rowbytes = d_iface_c.WARP_WIDTH;
        }
        else
        {
            rowbytes = (int)vid_c.vid.rowbytes;
        }

        d_edge_c.scale_for_mip = r_main_c.xscale;

        if (r_main_c.yscale > r_main_c.xscale)
        {
            d_edge_c.scale_for_mip = r_main_c.yscale;
        }

        d_vars_c.d_zrowbytes = vid_c.vid.width * 2;
        d_vars_c.d_zwidth = vid_c.vid.width;

        d_pix_min = r_main_c.r_refdef.vrect.width / 320;

        if (d_pix_min < 1)
        {
            d_pix_min = 1;
        }

        d_pix_max = (int)((float)r_main_c.r_refdef.vrect.width / (320.0f / 4.0f) + 0.5f);
        d_pix_shift = 8 - (int)((float)r_main_c.r_refdef.vrect.width / 320.0f + 0.5f);

        if (d_pix_max < 1)
        {
            d_pix_max = 1;
        }

        if (r_shared_c.pixelAspect > 1.4f)
        {
            d_y_aspect_shift = 1;
        }
        else
        {
            d_y_aspect_shift = 0;
        }

        d_vrectx = r_main_c.r_refdef.vrect.x;
        d_vrecty = r_main_c.r_refdef.vrect.y;
        d_vrectright_particle = r_main_c.r_refdef.vrectright - d_pix_max;
        d_vrectbottom_particle = r_main_c.r_refdef.vrectbottom - (d_pix_max << d_y_aspect_shift);

        {
            int i;

            for (i = 0; i < vid_c.vid.height; i++)
            {
                d_scantable[i] = i * rowbytes;
                zspantable[i] = d_vars_c.d_pzbuffer + i * d_vars_c.d_zwidth;
            }
        }

        D_Patch();
    }
}