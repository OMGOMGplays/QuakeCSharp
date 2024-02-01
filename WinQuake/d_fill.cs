namespace Quake;

public unsafe class d_fill_c
{
    public static void D_FillRect(vid_c.vrect_t* rect, int color)
    {
        int rx, ry, rwidth, rheight;
        char* dest;
        uint* ldest;

        rx = rect->x;
        ry = rect->y;
        rwidth = rect->width;
        rheight = rect->height;

        if (rx < 0)
        {
            rwidth += rx;
            rx = 0;
        }
        
        if (ry < 0)
        {
            rheight += ry;
            ry = 0;
        }

        if (rx + rwidth > vid_c.vid.width)
        {
            rwidth = (int)vid_c.vid.width - rx;
        }

        if (ry + rheight > vid_c.vid.height)
        {
            rheight = (int)vid_c.vid.height - ry;
        }

        if (rwidth < 1 || rheight < 1)
        {
            return;
        }

        dest = (char*)(vid_c.vid.buffer + ry * vid_c.vid.rowbytes + rx);

        if (((rwidth & 0x03) == 0) && (((long)dest & 0x03) == 0))
        {
            ldest = (uint*)dest;
            color += color << 16;

            rwidth >>= 2;
            color += color << 8;

            for (ry = 0; ry < rheight; ry++)
            {
                for (rx = 0; rx < rwidth; rx++)
                {
                    ldest[rx] = (uint)color;
                }

                ldest = (uint*)((byte*)ldest + vid_c.vid.rowbytes);
            }
        }
        else
        {
            for (ry = 0; ry < rheight; ry++)
            {
                for (rx = 0; rx < rwidth; rx++)
                {
                    dest[rx] = (char)color;
                }

                dest += vid_c.vid.rowbytes;
            }
        }
    }
}