namespace Quake;

public unsafe class d_part_c
{
    public static void D_EndParticles()
    {

    }

    public static void D_StartParticles()
    {

    }

#if !id386
    public static void D_DrawParticle(glquake_c.particle_t* pparticle)
    {
        Vector3 local, transformed;
        float zi;
        byte* pdest;
        short* pz;
        int i, izi, pix, count, u, v;

        local = transformed = new();

        mathlib_c.VectorSubtract(pparticle->org, render_c.r_origin, local);

        transformed[0] = mathlib_c.DotProduct(local, r_part_c.r_pright);
        transformed[1] = mathlib_c.DotProduct(local, r_part_c.r_pup);
        transformed[2] = mathlib_c.DotProduct(local, r_part_c.r_ppn);

        if (transformed[2] < d_iface_c.PARTICLE_Z_CLIP)
        {
            return;
        }

        zi = 1.0f / transformed[2];
        u = (int)(r_shared_c.xcenter + zi * transformed[0] + 0.5f);
        v = (int)(r_shared_c.ycenter - zi * transformed[1] + 0.5f);

        if ((v > d_local_c.d_vrectbottom_particle) || (u > d_local_c.d_vrectbottom_particle) || (v < d_local_c.d_vrecty) || (u < d_local_c.d_vrectx))
        {
            return;
        }

        pz = d_vars_c.d_pzbuffer + (d_vars_c.d_zwidth * v) + u;
        pdest = d_vars_c.d_viewbuffer + d_local_c.d_scantable[v] + u;
        izi = (int)(zi * 0x8000);

        pix = izi >> d_local_c.d_pix_shift;

        if (pix < d_local_c.d_pix_min)
        {
            pix = d_local_c.d_pix_min;
        }
        else if (pix > d_local_c.d_pix_max)
        {
            pix = d_local_c.d_pix_max;
        }

        switch (pix)
        {
            case 1:
                count = 1 << d_local_c.d_y_aspect_shift;

                for (; count != 0; count--, pz += d_local_c.d_zwidth, pdest += r_shared_c.screenwidth)
                {
                    if (pz[0] <= izi)
                    {
                        pz[0] = (short)izi;
                        pdest[0] = (byte)pparticle->color;
                    }
                }
                break;

            case 2:
                count = 2 << d_local_c.d_y_aspect_shift;

                for (; count != 0; count--, pz += d_local_c.d_zwidth, pdest += r_shared_c.screenwidth)
                {
                    if (pz[0] <= izi)
                    {
                        pz[0] = (short)izi;
                        pdest[0] = (byte)pparticle->color;
                    }

                    if (pz[1] <= izi)
                    {
                        pz[1] = (short)izi;
                        pdest[1] = (byte)pparticle->color;
                    }
                }
                break;

            case 3:
                count = 3 << d_local_c.d_y_aspect_shift;

                for (; count != 0; count--, pz += d_local_c.d_zwidth, pdest += r_shared_c.screenwidth)
                {
                    if (pz[0] <= izi)
                    {
                        pz[0] = (short)izi;
                        pdest[0] = (byte)pparticle->color;
                    }

                    if (pz[1] <= izi)
                    {
                        pz[1] = (short)izi;
                        pdest[1] = (byte)pparticle->color;
                    }

                    if (pz[2] <= izi)
                    {
                        pz[2] = (short)izi;
                        pdest[2] = (byte)pparticle->color;
                    }
                }
                break;

            case 4:
                count = 4 << d_local_c.d_y_aspect_shift;

                for (; count != 0; count--, pz += d_local_c.d_zwidth, pdest += r_shared_c.screenwidth)
                {
                    if (pz[0] <= izi)
                    {
                        pz[0] = (short)izi;
                        pdest[0] = (byte)pparticle->color;
                    }

                    if (pz[1] <= izi)
                    {
                        pz[1] = (short)izi;
                        pdest[1] = (byte)pparticle->color;
                    }

                    if (pz[2] <= izi)
                    {
                        pz[2] = (short)izi;
                        pdest[2] = (byte)pparticle->color;
                    }

                    if (pz[3] <= izi)
                    {
                        pz[3] = (short)izi;
                        pdest[3] = (byte)pparticle->color;
                    }
                }
                break;

            default:
                count = pix << d_local_c.d_y_aspect_shift;

                for (; count != 0; count--, pz += d_local_c.d_zwidth, pdest += r_shared_c.screenwidth)
                {
                    for (i = 0; i < pix; i++)
                    {
                        if (pz[i] <= izi)
                        {
                            pz[i] = (short)izi;
                            pdest[i] = (byte)pparticle->color;
                        }
                    }
                }
                break;
        }
    }
#endif
}