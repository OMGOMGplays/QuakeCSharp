namespace Quake;

public unsafe class snd_mem_c
{
    public static int cache_full_cycle;

    public static void ResampleSfx(sound_c.sfx_t* sfx, int inrate, int inwidth, byte* data)
    {
        int outcount;
        int srcsample;
        float stepscale;
        int i;
        int sample, samplefrac, fracstep;
        sound_c.sfxcache_t* sc;

        sc = (sound_c.sfxcache_t*)zone_c.Cache_Check(&sfx->cache);

        if (sc == null)
        {
            return;
        }

        stepscale = (float)inrate / snd_dma_c.shm->speed;

        outcount = (int)(sc->length / stepscale);
        sc->length = outcount;

        if (sc->loopstart != -1)
        {
            sc->loopstart = (int)(sc->loopstart / stepscale);
        }

        sc->speed = snd_dma_c.shm->speed;

        if (snd_dma_c.loadas8bit.value != 0)
        {
            sc->width = 1;
        }
        else
        {
            sc->width = inwidth;
        }

        sc->stereo = 0;

        if (stepscale == 1 && inwidth == 1 && sc->width == 1)
        {
            for (i = 0; i < outcount; i++)
            {
                ((char*)sc->data)[i] = (char)((char)data[i] -128);
            }
        }
        else
        {
            samplefrac = 0;
            fracstep = (int)(stepscale * 256);

            for (i = 0; i < outcount; i++)
            {
                srcsample = samplefrac >> 8;
                samplefrac += fracstep;

                if (inwidth == 2)
                {
                    sample = common_c.LittleShort(((short*)data)[srcsample]);
                }
                else
                {
                    sample = (char)data[srcsample] - 128 << 8;
                }

                if (sc->width == 2)
                {
                    sc->data[i] = (byte)sample;
                }
                else
                {
                    sc->data[i] = (byte)(sample >> 8);
                }
            }
        }
    }

    public static sound_c.sfxcache_t* S_LoadSound(sound_c.sfx_t* s)
    {
        char* namebuffer;
        byte* data;
        sound_c.wavinfo_t info;
        int len;
        float stepscale;
        sound_c.sfxcache_t* sc;
        byte* stackbuf;

        namebuffer = null;
        stackbuf = null;

        sc = (sound_c.sfxcache_t*)zone_c.Cache_Check(&s->cache);

        if (sc != null)
        {
            return sc;
        }

        common_c.Q_strcpy(namebuffer, "sound/");
        common_c.Q_strcat(namebuffer, s->name);

        data = common_c.COM_LoadStackFile(namebuffer->ToString(), stackbuf, sizeof(byte*));

        if (data == null)
        {
            console_c.Con_Printf($"Couldn't load {*namebuffer}\n");
            return null;
        }

        info = GetWavInfo(s->name, data, common_c.com_filesize);

        if (info.channels != 1)
        {
            console_c.Con_Printf($"{*s->name} is a stereo sample\n");
            return null;
        }

        stepscale = (float)info.rate / snd_dma_c.shm->speed;
        len = info.samples / (int)stepscale;

        len = len * info.width * info.channels;

        sc = (sound_c.sfxcache_t*)zone_c.Cache_Alloc(&s->cache, len + sizeof(sound_c.sfxcache_t), s->name->ToString());

        if (sc == null)
        {
            return null;
        }

        sc->length = info.samples;
        sc->loopstart = info.loopstart;
        sc->speed = info.rate;
        sc->width = info.width;
        sc->stereo = info.channels;

        ResampleSfx(s, sc->speed, sc->width, data + info.dataofs);

        return sc;
    }

    public static byte* data_p;
    public static byte* iff_end;
    public static byte* last_chunk;
    public static byte* iff_data;
    public static int iff_chunk_len;

    public static short GetLittleShort()
    {
        short val = 0;
        val = *data_p;
        val = (short)(val + (*(data_p + 1) << 8));
        data_p += 2;
        return val;
    }

    public static int GetLittleLong()
    {
        int val = 0;
        val = *data_p;
        val = val + (*(data_p + 1) << 8);
        val = val + (*(data_p + 2) << 16);
        val = val + (*(data_p + 3) << 24);
        data_p += 4;
        return val;
    }

    public static void FindNextChunk(char* name)
    {
        while (true)
        {
            data_p = last_chunk;

            if (data_p >= iff_end)
            {
                data_p = null;
                return;
            }

            data_p += 4;
            iff_chunk_len = GetLittleLong();

            if (iff_chunk_len <= 0)
            {
                data_p = null;
                return;
            }

            data_p -= 8;
            last_chunk = data_p + 8 + ((iff_chunk_len + 1) & ~1);

            if (common_c.Q_strncmp((char*)data_p, name, 4) == 0)
            {
                return;
            }
        }
    }

    public static void FindNextChunk(string name)
    {
        FindNextChunk(common_c.StringToChar(name));
    }

    public static void FindChunk(char* name)
    {
        last_chunk = iff_data;
        FindNextChunk(name);
    }

    public static void FindChunk(string name)
    {
        FindChunk(common_c.StringToChar(name));
    }

    public static void DumpChunks()
    {
        char* str = null;

        str[4] = '\0';
        data_p = iff_data;

        do
        {
            memcpy_c.memcpy((object*)str, (object*)data_p, 4);
            data_p += 4;
            iff_chunk_len = GetLittleLong();
            console_c.Con_Printf($"0x{(int)(data_p - 4)} : {*str} ({iff_chunk_len})\n");
            data_p += (iff_chunk_len + 1) & ~1;
        } while (data_p < iff_end);
    }

    public static sound_c.wavinfo_t GetWavInfo(char* name, byte* wav, int wavlength)
    {
        sound_c.wavinfo_t info;
        int i;
        int format;
        int samples;

        memset_c.memset((object*)&info, 0, sizeof(sound_c.wavinfo_t));

        if (wav == null)
        {
            return info;
        }

        iff_data = wav;
        iff_end = wav + wavlength;

        FindChunk("RIFF");

        if (!(data_p != null && common_c.Q_strncmp((char*)(data_p + 8), "WAVE", 4) == 0))
        {
            console_c.Con_Printf("Missing RIFF/WAVE chunks\n");
            return info;
        }

        iff_data = data_p + 12;

        FindChunk("fmt ");

        if (data_p == null)
        {
            console_c.Con_Printf("Missing fmt chunk\n");
            return info;
        }

        data_p += 8;
        format = GetLittleShort();

        if (format != 1)
        {
            console_c.Con_Printf("Microsoft PCM format only\n");
            return info;
        }

        info.channels = GetLittleShort();
        info.rate = GetLittleLong();
        data_p += 4 + 2;
        info.width = GetLittleShort() / 8;

        FindChunk("cue ");

        if (data_p != null)
        {
            data_p += 32;
            info.loopstart = GetLittleLong();

            FindNextChunk("LIST");

            if (data_p != null)
            {
                if (strncmp_c.strncmp((char*)data_p + 28, "mark", 4) == 0)
                {
                    data_p += 24;
                    i = GetLittleLong();
                    info.samples = info.loopstart + 1;
                }
            }
        }
        else
        {
            info.loopstart = -1;
        }

        FindChunk("data");

        if (data_p == null)
        {
            console_c.Con_Printf("Missing data chunk\n");
            return info;
        }

        data_p += 4;
        samples = GetLittleLong() / info.width;

        if (info.samples != 0)
        {
            if (samples < info.samples)
            {
                sys_win_c.Sys_Error($"Sound {*name} has a bad loop length");
            }
        }
        else
        {
            info.samples = samples;
        }

        info.dataofs = (int)(data_p - wav);

        return info;
    }
}