namespace Quake;

public unsafe class draw_c
{
    public struct rectdesc_t
    {
        public vid_c.vrect_t rect;
        public int width;
        public int height;
        public byte* ptexbytes;
        public int rowbytes;
    }

    public static rectdesc_t r_rectdesc;

    public static byte* draw_chars;
    public static wad_c.qpic_t* draw_disc;
    public static wad_c.qpic_t* draw_backtile;

    public struct cachepic_t
    {
        public char[] name;
        public zone_c.cache_user_t cache;
    }

    public const int MAX_CACHED_PICS = 128;
    public static cachepic_t* menu_cachepics;
    public static int menu_numcachepics;

    public static wad_c.qpic_t* Draw_PicFromWad(string name)
    {
        return (wad_c.qpic_t*)wad_c.W_GetLumpName(name);
    }

    public static wad_c.qpic_t* Draw_CachePic(string path)
    {
        cachepic_t* pic;
        int i;
        wad_c.qpic_t* dat;

        for (pic = menu_cachepics, i = 0; i < menu_numcachepics; pic++, i++)
        {
            if (!common_c.Q_strcmp(path, pic->name.ToString()))
            {
                break;
            }
        }

        if (i == menu_numcachepics)
        {
            if (menu_numcachepics == MAX_CACHED_PICS)
            {
                sys_win_c.Sys_Error(common_c.StringToChar("menu_numcachepics == MAX_CACHED_PICS"));
            }

            menu_numcachepics++;
            common_c.Q_strcpy(pic->name.ToString(), path);
        }

        dat = (wad_c.qpic_t*)zone_c.Cache_Check(&pic->cache);

        if (dat != null)
        {
            return dat;
        }

        common_c.COM_LoadCacheFile(path, pic->cache);

        dat = (wad_c.qpic_t*)pic->cache.data;

        if (dat == null)
        {
            sys_win_c.Sys_Error(common_c.StringToChar($"Draw_CachePic: failed to load {path}"));
        }

        wad_c.SwapPic(dat);

        return dat;
    }

    public static void Draw_Init()
    {
        int i;

        draw_chars = (byte*)wad_c.W_GetLumpName("conchars");
        draw_disc = (wad_c.qpic_t*)wad_c.W_GetLumpName("disc");
        draw_backtile = (wad_c.qpic_t*)wad_c.W_GetLumpName("backtile");

        r_rectdesc.width = draw_backtile->width;
        r_rectdesc.height = draw_backtile->height;
        r_rectdesc.ptexbytes = draw_backtile->data;
        r_rectdesc.rowbytes = draw_backtile->width;
    }

    public static void Draw_Character(int x, int y, int num)
    {
        byte* dest;
        byte* source;
        ushort* pusdest;
        int drawline;
        int row, col;

        num &= 255;

        if (y <= -8)
        {
            return;
        }

#if PARANOID
        if (y > vid.height - 8 || x<0 || x>width - 8) 
        {
            sys_win_c.Sys_Error($"Con_DrawCharacter: ({x}, {y})");
        }

        if (num < 0 || num > 255 ) 
        {
            sys_win_c.Sys_Error($"Con_DrawCharacter: char {num}");
        }
#endif

        row = num >> 4;
        col = num & 15;
        source = draw_chars + (row << 10) + (col << 3);

        if (y < 0)
        {
            drawline = 8 + y;
            source -= 128 * y;
            y = 0;
        }
        else
        {
            drawline = 8;
        }

        if (r_main_c.r_pixbytes == 1)
        {
            dest = (byte*)vid_c.vid.conbuffer + y * vid_c.vid.conrowbytes + x;

            while (drawline-- != 0)
            {
                if (source[0] != 0)
                {
                    dest[0] = source[0];
                }

                if (source[1] != 0)
                {
                    dest[1] = source[1];
                }

                if (source[2] != 0)
                {
                    dest[2] = source[2];
                }

                if (source[3] != 0)
                {
                    dest[3] = source[3];
                }

                if (source[4] != 0)
                {
                    dest[4] = source[4];
                }

                if (source[5] != 0)
                {
                    dest[5] = source[5];
                }

                if (source[6] != 0)
                {
                    dest[6] = source[6];
                }

                if (source[7] != 0)
                {
                    dest[7] = source[7];
                }

                source += 128;
                dest += vid_c.vid.conrowbytes;
            }
        }
        else
        {
            pusdest = (ushort*)((byte*)vid_c.vid.conbuffer + y * vid_c.vid.conrowbytes + (x << 1));

            while (drawline-- != 0)
            {
                if (source[0] != 0)
                {
                    pusdest[0] = vid_c.d_8to16table[source[0]];
                }

                if (source[1] != 0)
                {
                    pusdest[1] = vid_c.d_8to16table[source[1]];
                }

                if (source[2] != 0)
                {
                    pusdest[2] = vid_c.d_8to16table[source[2]];
                }

                if (source[3] != 0)
                {
                    pusdest[3] = vid_c.d_8to16table[source[3]];
                }

                if (source[4] != 0)
                {
                    pusdest[4] = vid_c.d_8to16table[source[4]];
                }

                if (source[4] != 0)
                {
                    pusdest[4] = vid_c.d_8to16table[source[4]];
                }

                if (source[5] != 0)
                {
                    pusdest[5] = vid_c.d_8to16table[source[5]];
                }

                if (source[6] != 0)
                {
                    pusdest[6] = vid_c.d_8to16table[source[6]];
                }

                if (source[7] != 0)
                {
                    pusdest[7] = vid_c.d_8to16table[source[7]];
                }

                source += 128;
                pusdest += vid_c.vid.conrowbytes >> 1;
            }
        }
    }

    public void Draw_String(int x, int y, string* str)
    {
        while (str != null)
        {
            Draw_Character(x, y, (int)str);
            str++;
            x += 8;
        }
    }

    public void Draw_DebugChar(char num)
    {
        byte* dest;
        byte* source;
        int drawline;
        byte* draw_chars = null;
        int row, col;

        if (vid_c.vid.direct == null)
        {
            return;
        }

        drawline = 8;

        row = num >> 4;
        col = num & 15;
        source = draw_chars + (row << 10) + (col << 3);

        dest = (byte*)vid_c.vid.direct + 312;

        while (drawline-- != 0)
        {
            dest[0] = source[0];
            dest[1] = source[1];
            dest[2] = source[2];
            dest[3] = source[3];
            dest[4] = source[4];
            dest[5] = source[5];
            dest[6] = source[6];
            dest[7] = source[7];
            source += 128;
            dest += 320;
        }
    }

    public static void Draw_Pic(int x, int y, wad_c.qpic_t* pic)
    {
        byte* dest, source;
        ushort* pusdest;
        int v, u;

        if ((x < 0) || (x + pic->width > vid_c.vid.width) || (y < 0) || (y + pic->height > vid_c.vid.height))
        {
            sys_win_c.Sys_Error(common_c.StringToChar("Draw_Pic: bad coordinates"));
        }

        source = pic->data;

        if (r_main_c.r_pixbytes == 1)
        {
            dest = (byte*)vid_c.vid.buffer + y * vid_c.vid.rowbytes + x;

            for (v = 0; v < pic->height; v++)
            {
                common_c.Q_memcpy(*dest, *source, pic->width);
                dest += vid_c.vid.rowbytes;
                source += pic->width;
            }
        }
        else
        {
            pusdest = (ushort*)vid_c.vid.buffer + y * (vid_c.vid.rowbytes >> 1) + x;

            for (v = 0; v < pic->height; v++)
            {
                for (u = 0; u < pic->width; u++)
                {
                    pusdest[u] = vid_c.d_8to16table[source[u]];
                }

                pusdest += vid_c.vid.rowbytes >> 1;
                source += pic->width;
            }
        }
    }

    public static void Draw_TransPic(int x, int y, wad_c.qpic_t* pic)
    {
        byte* dest, source, tbyte;
        ushort* pusdest;
        int v, u;

        if (x < 0 || x + pic->width > vid_c.vid.width || y < 0 || y + pic->height > vid_c.vid.height)
        {
            sys_win_c.Sys_Error(common_c.StringToChar("Draw_TransPic: bad coordinates"));
        }

        source = pic->data;

        if (r_main_c.r_pixbytes == 1)
        {
            dest = (byte*)vid_c.vid.buffer + y * vid_c.vid.rowbytes + x;

            if ((pic->width & 7) != 0)
            {
                for (v = 0; v < pic->height; v++)
                {
                    for (u = 0; u < pic->width; u++)
                    {
                        if ((tbyte = (byte*)source[u]) != TRANSPARENT_COLOR)
                        {
                            dest[u] = *tbyte;
                        }
                    }

                    dest += vid_c.vid.rowbytes;
                    source += pic->width;
                }
            }
            else
            {
                for (v = 0; v < pic->height; v++)
                {
                    for (u = 0; u < pic->width; u++)
                    {
                        if ((tbyte = (byte*)source[u]) != TRANSPARENT_COLOR)
                        {
                            dest[u] = *tbyte;
                        }

                        if ((tbyte = (byte*)source[u + 1]) != TRANSPARENT_COLOR)
                        {
                            dest[u + 1] = *tbyte;
                        }

                        if ((tbyte = (byte*)source[u + 2]) != TRANSPARENT_COLOR)
                        {
                            dest[u + 2] = *tbyte;
                        }

                        if ((tbyte = (byte*)source[u + 3]) != TRANSPARENT_COLOR)
                        {
                            dest[u + 3] = *tbyte;
                        }

                        if ((tbyte = (byte*)source[u + 4]) != TRANSPARENT_COLOR)
                        {
                            dest[u + 4] = *tbyte;
                        }

                        if ((tbyte = (byte*)source[u + 5]) != TRANSPARENT_COLOR)
                        {
                            dest[u + 5] = *tbyte;
                        }

                        if ((tbyte = (byte*)source[u + 6]) != TRANSPARENT_COLOR)
                        {
                            dest[u + 6] = *tbyte;
                        }

                        if ((tbyte = (byte*)source[u + 7]) != TRANSPARENT_COLOR)
                        {
                            dest[u + 7] = *tbyte;
                        }
                    }

                    dest += vid_c.vid.rowbytes;
                    source += pic->width;
                }
            }
        }
        else
        {
            pusdest = (ushort*)vid_c.vid.buffer + y * (vid_c.vid.rowbytes >> 1) + x;

            for (v = 0; v < pic->height; v++)
            {
                for (u = 0; u < pic->width; u++)
                {
                    tbyte = (byte*)source[u];

                    if (tbyte != TRANSPARENT_COLOR)
                    {
                        pusdest[u] = vid_c.d_8to16table[(int)tbyte];
                    }
                }

                pusdest += vid_c.vid.rowbytes >> 1;
                source += pic->width;
            }
        }
    }

    public static void Draw_TransPicTranslate(int x, int y, wad_c.qpic_t* pic, byte* translation)
    {
        byte* dest, source, tbyte = null;
        ushort* pusdest;
        int v, u;

        if (x < 0 || (uint)(x + pic->width) > vid_c.vid.width || y < 0 || (uint)(y + pic->height) > vid_c.vid.width)
        {
            sys_win_c.Sys_Error(common_c.StringToChar("Draw_TransPic: bad coordinates"));
        }

        source = pic->data;

        if (r_main_c.r_pixbytes == 1)
        {
            dest = vid_c.vid.buffer + y * vid_c.vid.rowbytes + x;

            if ((pic->width & 7) != 0)
            {
                for (v = 0; v < pic->height; v++)
                {
                    for (u = 0; u < pic->width; u++)
                    {
                        if ((*tbyte = source[u]) != d_iface_c.TRANSPARENT_COLOR)
                        {
                            dest[u] = translation[(int)tbyte];
                        }
                    }

                    dest += vid_c.vid.rowbytes;
                    source += pic->width;
                }
            }
            else
            {
                for (v = 0; v < pic->height; v++)
                {
                    for (u = 0; u < pic->width; u++)
                    {
                        if ((int)(tbyte = (byte*)source[u]) != d_iface_c.TRANSPARENT_COLOR)
                        {
                            dest[u] = *tbyte;
                        }

                        if ((int)(tbyte = (byte*)source[u + 1]) != d_iface_c.TRANSPARENT_COLOR)
                        {
                            dest[u + 1] = *tbyte;
                        }

                        if ((int)(tbyte = (byte*)source[u + 2]) != d_iface_c.TRANSPARENT_COLOR)
                        {
                            dest[u + 2] = *tbyte;
                        }

                        if ((int)(tbyte = (byte*)source[u + 3]) != d_iface_c.TRANSPARENT_COLOR)
                        {
                            dest[u + 3] = *tbyte;
                        }

                        if ((int)(tbyte = (byte*)source[u + 4]) != d_iface_c.TRANSPARENT_COLOR)
                        {
                            dest[u + 4] = *tbyte;
                        }

                        if ((int)(tbyte = (byte*)source[u + 5]) != d_iface_c.TRANSPARENT_COLOR)
                        {
                            dest[u + 5] = *tbyte;
                        }

                        if ((int)(tbyte = (byte*)source[u + 6]) != d_iface_c.TRANSPARENT_COLOR)
                        {
                            dest[u + 6] = *tbyte;
                        }

                        if ((int)(tbyte = (byte*)source[u + 7]) != d_iface_c.TRANSPARENT_COLOR)
                        {
                            dest[u + 7] = *tbyte;
                        }
                    }

                    dest += vid_c.vid.rowbytes;
                    source += pic->width;
                }
            }
        }
        else
        {
            pusdest = (ushort*)vid_c.vid.buffer + y * (vid_c.vid.rowbytes >> 1) + x;

            for (v = 0; v < pic->height; v++)
            {
                for (u = 0; u < pic->width; u++)
                {
                    tbyte = &source[u];

                    if ((int)tbyte != d_iface_c.TRANSPARENT_COLOR)
                    {
                        pusdest[u] = vid_c.d_8to16table[(int)tbyte];
                    }
                }

                pusdest += vid_c.vid.rowbytes >> 1;
                source += pic->width;
            }
        }
    }

    public static void Draw_CharToConBack(int num, byte* dest)
    {
        int row, col;
        byte* source;
        int drawline;
        int x;

        row = num >> 4;
        col = num & 15;
        source = draw_chars + (row << 10) + (col << 3);

        drawline = 8;

        while (drawline-- != 0)
        {
            for (x = 0; x < 8; x++)
            {
                if (source[x] != 0)
                {
                    dest[x] = (byte)(0x60 + source[x]);
                }
            }

            source += 128;
            dest += 320;
        }
    }

    public static void Draw_ConsoleBackground(int lines)
    {
        int x, y, v;
        byte* src, dest;
        ushort* pusdest;
        int f, fstep;
        wad_c.qpic_t* conback;
        char[] ver = new char[100];

        conback = Draw_CachePic("gfx/conback.lmp");

#if _WIN32
        Console.WriteLine($"(WinQuake) {quakedef_c.VERSION}");
        dest = conback->data + 320 * 186 + 320 - 11 - 8 * common_c.Q_strlen(ver.ToString());
#elif X11
        Console.WriteLine($"(X11 Quake {quakedef_c.X11_VERSION}) {quakedef_c.VERSION}");
        dest = conback->data + 320 * 186 + 320 - 11 - 8 * common_c.Q_strlen(ver.ToString());
#elif __linux__
        Console.WriteLine($"(Linux Quake {quakedef_c.LINUX_VERSION}) {quakedef_c.VERSION}");
        dest = conback->data + 320 * 186 + 320 - 11 - 8 * common_c.Q_strlen(ver.ToString());
#else
        dest = conback->data + 320 - 43 + 320 * 186;
        Console.WriteLine($"{quakedef_c.VERSION}");
#endif
        for (x = 0; x < common_c.Q_strlen(ver.ToString()); x++)
        {
            Draw_CharToConBack(ver[x], dest + (x << 3));
        }

        if (r_main_c.r_pixbytes == 1)
        {
            dest = (byte*)vid_c.vid.conbuffer;

            for (y = 0; y < lines; y++, dest += vid_c.vid.conrowbytes)
            {
                v = (int)((vid_c.vid.conheight - lines + y) * 200 / vid_c.vid.conheight);
                src = conback->data + v * 320;

                if (vid_c.vid.conwidth == 320)
                {
                    common_c.Q_memcpy(*dest, *src, (int)vid_c.vid.conwidth);
                }
                else
                {
                    f = 0;
                    fstep = (int)(320 * 0x10000 / vid_c.vid.conwidth);

                    for (x = 0; x < vid_c.vid.conwidth; x += 4)
                    {
                        dest[x] = src[f >> 16];
                        f += fstep;
                        dest[x + 1] = src[f >> 16];
                        f += fstep;
                        dest[x + 2] = src[f >> 16];
                        f += fstep;
                        dest[x + 3] = src[f >> 16];
                        f += fstep;
                    }
                }
            }
        }
        else
        {
            pusdest = (ushort*)vid_c.vid.conbuffer;

            for (y = 0; y < lines; y++, pusdest += (vid_c.vid.conrowbytes >> 1))
            {
                v = (int)((vid_c.vid.conheight - lines + y) * 200 / vid_c.vid.conheight);
                src = conback->data + v * 320;
                f = 0;
                fstep = (int)(320 * 0x10000 / vid_c.vid.conwidth);

                for (x = 0; x < vid_c.vid.conwidth; x += 4)
                {
                    pusdest[x] = vid_c.d_8to16table[src[f >> 16]];
                    f += fstep;
                    pusdest[x + 1] = vid_c.d_8to16table[src[f >> 16]];
                    f += fstep;
                    pusdest[x + 2] = vid_c.d_8to16table[src[f >> 16]];
                    f += fstep;
                    pusdest[x + 3] = vid_c.d_8to16table[src[f >> 16]];
                    f += fstep;
                }
            }
        }
    }

    public static void R_DrawRect8(vid_c.vrect_t* prect, int rowbytes, byte* psrc, int transparent)
    {
        byte t;
        int i, j, srcdelta, destdelta;
        byte* pdest;

        pdest = (byte*)vid_c.vid.buffer + (prect->y * vid_c.vid.rowbytes) + prect->x;

        srcdelta = rowbytes - prect->width;
        destdelta = (int)vid_c.vid.rowbytes - prect->width;

        if (transparent == 1)
        {
            for (i = 0; i < prect->height; i++)
            {
                for (j = 0; j < prect->width; j++)
                {
                    t = *psrc;

                    if (t != TRANSPARENT_COLOR)
                    {
                        *pdest = t;
                    }

                    psrc++;
                    pdest++;
                }

                psrc += srcdelta;
                pdest += destdelta;
            }
        }
        else
        {
            for (i = 0; i < prect->height; i++)
            {
                common_c.Q_memcpy(*pdest, *psrc, prect->width);
                psrc += rowbytes;
                pdest += vid_c.vid.rowbytes;
            }
        }
    }

    public static void R_DrawRect16(vid_c.vrect_t* prect, int rowbytes, byte* psrc, int transparent)
    {
        byte t;
        int i, j, srcdelta, destdelta;
        ushort* pdest;

        pdest = (ushort*)vid_c.vid.buffer + (prect->y * (vid_c.vid.rowbytes >> 1)) + prect->x;

        srcdelta = rowbytes - prect->width;
        destdelta = (int)(vid_c.vid.rowbytes >> 1) - prect->width;

        if (transparent == 1)
        {
            for (i = 0; i < prect->height; i++)
            {
                for (j = 0; j < prect->width; j++)
                {
                    t = *psrc;

                    if (t != TRANSPARENT_COLOR)
                    {
                        *pdest = vid_c.d_8to16table[t];
                    }

                    psrc++;
                    pdest++;
                }

                psrc += srcdelta;
                pdest += destdelta;
            }
        }
        else
        {
            for (i = 0; i < prect->height; i++)
            {
                for (j = 0; j < prect->width; j++)
                {
                    *pdest = vid_c.d_8to16table[*psrc];
                    psrc++;
                    pdest++;
                }

                psrc += srcdelta;
                pdest += destdelta;
            }
        }
    }

    public static void Draw_TileClear(int x, int y, int w, int h)
    {
        int width, height, tileoffsetx, tileoffsety;
        byte* psrc;
        vid_c.vrect_t vr;

        r_rectdesc.rect.x = x;
        r_rectdesc.rect.y = y;
        r_rectdesc.rect.width = w;
        r_rectdesc.rect.height = h;

        vr.y = r_rectdesc.rect.y;
        height = r_rectdesc.rect.height;

        tileoffsety = vr.y % r_rectdesc.height;

        while (height > 0)
        {
            vr.x = r_rectdesc.rect.x;
            width = r_rectdesc.rect.width;

            if (tileoffsety != 0)
            {
                vr.height = r_rectdesc.height - tileoffsety;
            }
            else
            {
                vr.height = r_rectdesc.height;
            }

            if (vr.height > height)
            {
                vr.height = height;
            }

            tileoffsetx = vr.x % r_rectdesc.width;

            while (width > 0)
            {
                if (tileoffsetx != 0)
                {
                    vr.width = r_rectdesc.width - tileoffsetx;
                }
                else
                {
                    vr.width = r_rectdesc.width;
                }

                if (vr.width > width)
                {
                    vr.width = width;
                }

                psrc = r_rectdesc.ptexbytes + (tileoffsety * r_rectdesc.rowbytes) + tileoffsetx;

                if (r_main_c.r_pixbytes == 1)
                {
                    R_DrawRect8(&vr, r_rectdesc.rowbytes, psrc, 0);
                }
                else
                {
                    R_DrawRect16(&vr, r_rectdesc.rowbytes, psrc, 0);
                }

                vr.x += vr.width;
                width -= vr.width;
                tileoffsetx = 0;
            }

            vr.y += vr.height;
            height -= vr.height;
            tileoffsety = 0;
        }
    }

    public void Draw_Fill(int x, int y, int w, int h, int c)
    {
        byte* dest;
        ushort* pusdest;
        uint uc;
        int u, v;

        if (r_main_c.r_pixbytes == 1)
        {
            dest = (byte*)vid_c.vid.buffer + y * vid_c.vid.rowbytes + x;

            for (v = 0; v < h; v++, dest += vid_c.vid.rowbytes)
            {
                for (u = 0; u < w; u++)
                {
                    dest[u] = (byte)c;
                }
            }
        }
        else
        {
            uc = vid_c.d_8to16table[c];

            pusdest = (ushort*)vid_c.vid.buffer + y * (vid_c.vid.rowbytes >> 1) + x;

            for (v = 0; v < h; v++, pusdest += (vid_c.vid.rowbytes >> 1))
            {
                for (u = 0; u < w; u++)
                {
                    pusdest[u] = (byte)uc;
                }
            }
        }
    }

    public void Draw_FadeScreen()
    {
        int x, y;
        byte* pbuf;

        VID_UnlockBuffer();
        S_ExtraUpdate();
        VID_LockBuffer();

        for (y = 0; y < vid_c.vid.height; y++)
        {
            int t;

            pbuf = (byte*)(vid_c.vid.buffer + vid_c.vid.rowbytes * y);
            t = (y & 1) << 1;

            for (x = 0; x < vid_c.vid.width; x++)
            {
                if ((x & 3) != t)
                {
                    pbuf[x] = 0;
                }
            }
        }

        VID_UnlockBuffer();
        S_ExtraUpdate();
        VID_LockBuffer();
    }

    public static void Draw_BeginDisc()
    {
        D_BeginDirectRect(vid_win_c.vid.width - 24, 0, draw_disc->data, 24, 24);
    }

    public static void Draw_EndDisc()
    {
        D_EndDirectRect(vid_win_c.vid.width - 24, 0, 24, 24);
    }
}