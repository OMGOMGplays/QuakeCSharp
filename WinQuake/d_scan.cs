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

        rowptr = null;

        w = r_main_c.r_refdef.vrect.width;
        h = r_main_c.r_refdef.vrect.height;

        wratio = w / (float)screen_c.scr_vrect->width;
        hratio = h / (float)screen_c.scr_vrect->height;

        for (v = 0; v < screen_c.scr_vrect->height + r_local_c.AMP2 * 2; v++)
        {
            rowptr[v] = d_local_c.d_viewbuffer + (r_main_c.r_refdef.vrect.y * d_edge_c.screenwidth) + (d_edge_c.screenwidth * (int)(v * hratio * h / (h + r_local_c.AMP2 * 2)));
        }

        for (u = 0; u < screen_c.scr_vrect->width + r_local_c.AMP2 * 2; u++)
        {

        }
    }
}