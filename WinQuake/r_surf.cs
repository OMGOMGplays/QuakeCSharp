namespace Quake;

public unsafe class r_surf_c
{
    public static d_iface_c.drawsurf_t r_drawsurf;

    public static int lightleft, sourcesstep, blocksize, sourcetstep;
    public static int lightdelta, lightdeltastep;
    public static int lightright, lightleftstep, lightrightstep, blockdivshift;
    public static uint blockdivmask;
}