namespace Quake;

public unsafe class d_vars_c
{
#if !id386
    public static float d_sdivzstepu, d_tdivzstepu, d_zistepu;
    public static float d_sdivzstepv, d_tdivzstepv, d_zistepv;
    public static float d_sdivzorigin, d_tdivzorigin, d_ziorigin;

    public static int sadjust, tadjust, bbextents, bbextentt;

    public static byte* cacheblock;
    public static int cachewidth;
    public static byte* d_viewbuffer;
    public static short* d_pzbuffer;
    public static uint d_zrowbytes;
    public static uint d_zwidth;
#endif
}