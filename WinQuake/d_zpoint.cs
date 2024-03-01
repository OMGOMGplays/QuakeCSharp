namespace Quake;

public unsafe class d_zpoint_c
{
    public void D_DrawZPoint()
    {
        byte* pdest;
        short* pz;
        int izi;

        pz = d_vars_c.d_pzbuffer + (d_vars_c.d_zwidth * r_draw_c.r_zpointdesc.v) + r_draw_c.r_zpointdesc.u;
        pdest = d_vars_c.d_viewbuffer + d_modech_c.d_scantable[r_draw_c.r_zpointdesc.v] + r_draw_c.r_zpointdesc.u;
        izi = (int)(r_draw_c.r_zpointdesc.zi * 0x8000);

        if (*pz <= izi)
        {
            *pz = (short)izi;
            *pdest = (byte)r_draw_c.r_zpointdesc.color;
        }
    }
}