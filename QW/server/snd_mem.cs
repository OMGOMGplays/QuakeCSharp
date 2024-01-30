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
                    ((short*)sc->data[i]) = sample;
                }
            }
        }
    }
}