using System.Runtime.CompilerServices;

namespace Quake;

public unsafe class r_sky_c
{
    public static int iskyspeed = 8;
    public static int iskyspeed2 = 2;
    public static float skyspeed, skyspeed2;

    public static float skytime;

    public static byte* r_skysource;

    public static int r_skymade;
    public static int r_skydirect;

    public static byte[] bottomsky = new byte[128 * 131];
    public static byte[] bottommask = new byte[128 * 131];
    public static byte* newsky;

    public static void R_InitSky(model_c.texture_t* mt)
    {
        int i, j;
        byte* src;

        src = (byte*)mt + mt->offsets[0];

        for (i = 0; i < 128; i++)
        {
            for (j = 0; j < 128; j++)
            {
                newsky[(i * 256) + j + 128] = src[i * 256 + j + 128];
            }
        }

        for (i = 0; i < 128; i++)
        {
            for (j = 0; j < 131; j++)
            {
                if (src[i * 256 + (j & 0x7F)] != 0)
                {
                    bottomsky[(i * 131) + j] = src[i * 256 + (j & 0x7F)];
                    bottommask[(i * 131) + j] = 0;
                }
                else
                {
                    bottomsky[(i * 131) + j] = 0;
                    bottommask[(i * 131) + j] = 0xff;
                }
            }
        }

        r_skysource = newsky;
    }

    public static void R_MakeSky()
    {
        int x, y;
        int ofs, baseofs;
        int xshift, yshift;
        uint* pnewsky;
        int xlast = -1, ylast = -1;

        xshift = (int)(skytime * skyspeed);
        yshift = (int)(skytime * skyspeed);

        if ((xshift == xlast) && (yshift == ylast))
        {
            return;
        }

        xlast = xshift;
        ylast = yshift;

        pnewsky = (uint*)&newsky[0];

        for (y = 0; y < d_iface_c.SKYSIZE; y++)
        {
            baseofs = ((y + yshift) & d_iface_c.SKYMASK) * 131;

#if UNALIGNED_OK

            for (x = 0; x < d_iface_c.SKYSIZE; x += 4)
            {
                ofs = baseofs + ((x + xshift) & d_iface_c.SKYMASK);

                *pnewsky = (*(pnewsky + (128 / sizeof(uint))) & *(uint*)&bottommask[ofs]) | *(uint*)&bottomsky[ofs];
                pnewsky++;
            }

#else

            for (x = 0; x < d_iface_c.SKYSIZE; x++)
            {
                ofs = baseofs + ((x + xshift) & d_iface_c.SKYMASK);

                *(byte*)pnewsky = (byte)((*((byte*)pnewsky + 128) & *(byte*)bottommask[ofs]) | *(byte*)bottomsky[ofs]);
                pnewsky = (uint*)((byte*)pnewsky + 1);
            }

#endif

            pnewsky += 128 / sizeof(uint);
        }

        r_skymade = 1;
    }

    public void R_GenSkyTile(void* pdest)
    {
        int x, y;
        int ofs, baseofs;
        int xshift, yshift;
        uint* pnewsky;
        uint* pd;

        xshift = (int)(skytime * skyspeed);
        yshift = (int)(skytime * skyspeed);

        pnewsky = (uint*)&newsky[0];
        pd = (uint*)pdest;

        for (y = 0; y < d_iface_c.SKYSIZE; y++)
        {
            baseofs = ((y + yshift) & d_iface_c.SKYMASK) * 131;

#if UNALIGNED_OK
            for (x = 0; x < d_iface_c.SKYSIZE; x += 4)
            {
                ofs = baseofs + ((x + xshift) & d_iface_c.SKYMASK);

                *pd = (*(pnewsky + (128 / sizeof(uint))) & *(uint*)bottommask[ofs]) | *(uint*)bottomsky[ofs];
                pnewsky++;
                pd++;
            }
#else
            for (x = 0; x < d_iface_c.SKYSIZE; x++)
            {
                ofs = baseofs + ((x + xshift) & d_iface_c.SKYMASK);

                *(byte*)pd = (byte)((*((byte*)pnewsky + 128) & *(byte*)bottommask[ofs]) | *(byte*)bottomsky[ofs]);
                pnewsky = (uint*)((byte*)pnewsky + 1);
                pd = (uint*)((byte*)pd + 1);
            }
#endif

            pnewsky += 128 / sizeof(uint);
        }
    }

    public static void R_GenSkyTile16(void* pdest)
    {
        int x, y;
        int ofs, baseofs;
        int xshift, yshift;
        byte* pnewsky;
        ushort* pd;

        xshift = (int)(skytime * skyspeed);
        yshift = (int)(skytime * skyspeed);

        pnewsky = (byte*)newsky[0];
        pd = (ushort*)pdest;

        for (y = 0; y < d_iface_c.SKYSIZE; y++)
        {
            baseofs = ((y + yshift) & d_iface_c.SKYMASK) * 131;

            for (x = 0; x < d_iface_c.SKYSIZE; x++)
            {
                ofs = baseofs + ((x + xshift) & d_iface_c.SKYMASK);

                *pd = vid_c.d_8to16table[(*(pnewsky + 128) & *(byte*)bottommask[ofs]) | *(byte*)bottomsky[ofs]];
                pnewsky++;
                pd++;
            }

            pnewsky += d_iface_c.TILE_SIZE;
        }
    }

    public static void R_SetSkyFrame()
    {
        int g, s1, s2;
        float temp;

        skyspeed = iskyspeed;
        skyspeed2 = iskyspeed2;

        g = mathlib_c.GreatestCommonDivisor(iskyspeed, iskyspeed2);
        s1 = iskyspeed / g;
        s2 = iskyspeed2 / g;
        temp = d_iface_c.SKYSIZE * s1 * s2;

        skytime = (float)cl_main_c.cl.time - ((int)(cl_main_c.cl.time / temp) * temp);

        r_skymade = 0;
    }
}