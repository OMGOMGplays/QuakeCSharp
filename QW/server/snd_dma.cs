namespace Quake;

public unsafe class snd_dma_c
{
    public static sound_c.channel_t* channels;
    public static int total_channels;

    public static int snd_blocked = 0;
    public static bool snd_ambient = true;
    public static bool snd_initialized = false;

    public static sound_c.dma_t* shm = null;
    public static sound_c.dma_t sn;

    public static Vector3 listener_origin;
    public static Vector3 listener_forward;
    public static Vector3 listener_right;
    public static Vector3 listener_up;
    public static float sound_nominal_clip_dist = 1000.0f;

    public static int soundtime;
    public static int paintedtime;

    public const int MAX_SFX = 512;
    public static sound_c.sfx_t* known_sfx;
    public static int num_sfx;

    public static sound_c.sfx_t* ambient_sfx;

    public static int desired_speed = 11025;
    public static int desired_bits = 16;

    public static int sound_started = 0;

    public static cvar_c.cvar_t bgmvolume = new cvar_c.cvar_t { name = "bgmvolume", value = (char)1, archive = true };
    public static cvar_c.cvar_t volume = new cvar_c.cvar_t { name = "volume", value = (char)0.7f, archive = true };

    public static cvar_c.cvar_t nosound = new cvar_c.cvar_t { name = "nosound", value = (char)0 };
    public static cvar_c.cvar_t precache = new cvar_c.cvar_t { name = "precache", value = (char)1 };
    public static cvar_c.cvar_t loadas8bit = new cvar_c.cvar_t { name = "loadas8bit", value = (char)0 };
    public static cvar_c.cvar_t bgmbuffer = new cvar_c.cvar_t { name = "bgmbuffer", value = (char)4096 };
    public static cvar_c.cvar_t ambient_level = new cvar_c.cvar_t { name = "ambient_level", value = (char)0.3f };
    public static cvar_c.cvar_t ambient_fade = new cvar_c.cvar_t { name = "ambient_fade", value = (char)100 };
    public static cvar_c.cvar_t snd_noextraupdate = new cvar_c.cvar_t { name = "snd_noextraupdate", value = (char)0 };
    public static cvar_c.cvar_t snd_show = new cvar_c.cvar_t { name = "snd_show", value = (char)0 };
    public static cvar_c.cvar_t _snd_mixahead = new cvar_c.cvar_t { name = "_sndmixahead", value = (char)0.1f, archive = true };

    public static bool fakedma = false;
    public static int fakedma_updates = 15;

    public static void S_AmbientOff()
    {
        snd_ambient = false;
    }

    public static void S_AmbientOn()
    {
        snd_ambient = true;
    }

    public static void S_SoundInfo()
    {
        if (sound_started != 0 || shm == null)
        {
            console_c.Con_Printf("sound system not started\n");
            return;
        }

        console_c.Con_Printf($"{shm->channels - 1} stereo\n");
        console_c.Con_Printf($"{shm->samples} samples\n");
        console_c.Con_Printf($"{shm->samplepos} samplepos\n");
        console_c.Con_Printf($"{shm->samplebits} samplebits\n");
        console_c.Con_Printf($"{shm->submission_chunk} submission_chunk\n");
        console_c.Con_Printf($"{shm->speed} speed\ns");
        console_c.Con_Printf($"{*shm->buffer} dma buffer\n");
        console_c.Con_Printf($"{total_channels} total_channels\n");
    }

    public static void S_Startup()
    {
        int rc;

        if (!snd_initialized)
        {
            return;
        }

        if (!fakedma)
        {
            rc = snd_win_c.SNDDMA_Init();

            if (rc == 0)
            {
#if !_WIN32
                console_c.Con_Printf("S_Startup: SNDDMA_Init failed\n");
#endif
                sound_started = 0;
                return;
            }
        }

        sound_started = 1;
    }

    public static void S_Init()
    {
        console_c.Con_Printf("\nSound Initialization\n");

        if (common_c.COM_CheckParm("-nosound") != 0)
        {
            return;
        }

        if (common_c.COM_CheckParm("-simsound") != 0)
        {
            fakedma = true;
        }

        cmd_c.Cmd_AddCommand("play", S_Play);
        cmd_c.Cmd_AddCommand("playvol", S_PlayVol);
        cmd_c.Cmd_AddCommand("stopsound", S_StopAllSoundsC);
        cmd_c.Cmd_AddCommand("soundlist", S_SoundList);
        cmd_c.Cmd_AddCommand("soundinfo", S_SoundInfo_f);

        cvar_c.Cvar_RegisterVariable(nosound);
        cvar_c.Cvar_RegisterVariable(volume);
        cvar_c.Cvar_RegisterVariable(precache);
        cvar_c.Cvar_RegisterVariable(loadas8bit);
        cvar_c.Cvar_RegisterVariable(bgmvolume);
        cvar_c.Cvar_RegisterVariable(bgmbuffer);
        cvar_c.Cvar_RegisterVariable(ambient_level);
        cvar_c.Cvar_RegisterVariable(ambient_fade);
        cvar_c.Cvar_RegisterVariable(snd_noextraupdate);
        cvar_c.Cvar_RegisterVariable(snd_show);
        cvar_c.Cvar_RegisterVariable(_snd_mixahead);

        if (host_c.host_parms.memsize < 0x800000)
        {
            cvar_c.Cvar_Set("loadas8bit", "1");
            console_c.Con_Printf("loading all sounds as 8bit\n");
        }

        snd_initialized = true;

        S_Startup();

        snd_mix_c.SND_InitScaleTable();

        known_sfx = (sound_c.sfx_t*)zone_c.Hunk_AllocName(MAX_SFX * sizeof(sound_c.sfx_t), "sfx_t");
        num_sfx = 0;

        if (fakedma)
        {
            shm = (sound_c.dma_t*)zone_c.Hunk_AllocName((int)shm, "shm");
            shm->splitbuffer = false;
            shm->samplebits = 16;
            shm->speed = 22050;
            shm->channels = 2;
            shm->samples = 32768;
            shm->samplepos = 0;
            shm->soundalive = true;
            shm->gamealive = true;
            shm->submission_chunk = 1;
            shm->buffer = (char*)zone_c.Hunk_AllocName(1 << 16, "shmbuf");
        }

        console_c.Con_Printf($"Sound sampling rate: {shm->speed}\n");

        ambient_sfx[bspfile_c.AMBIENT_WATER] = S_PrecacheSound("ambience/water1.wav");
        ambient_sfx[bspfile_c.AMBIENT_SKY] = S_PrecacheSound("ambience/wind2.wav");

        S_StopAllSounds(true);
    }

    public static void S_Shutdown()
    {
        if (sound_started == 0)
        {
            return;
        }

        if (shm != null)
        {
            shm->gamealive = 0;
        }

        shm = null;
        sound_started = 0;

        if (!fakedma)
        {
            snd_win_c.SNDDMA_Shutdown();
        }
    }

    public static sound_c.sfx_t* S_FindName(char* name)
    {
        int i;
        sound_c.sfx_t* sfx;

        if (name == null)
        {
            sys_win_c.Sys_Error("S_FindName: null\n");
        }

        if (common_c.Q_strlen(name) >= quakedef_c.MAX_QPATH)
        {
            sys_win_c.Sys_Error($"Sound name too long: {*name}");
        }

        for (i = 0; i < num_sfx; i++)
        {
            if (common_c.Q_strcmp(known_sfx[i].name, name))
            {
                return &known_sfx[i];
            }
        }

        if (num_sfx == MAX_SFX)
        {
            sys_win_c.Sys_Error("S_FindName: out of sfx_t");
        }

        sfx = &known_sfx[i];
        strcpy_c.strcpy(sfx->name, name);

        num_sfx++;

        return sfx;
    }

    public static void S_TouchSound(char* name)
    {
        sound_c.sfx_t* sfx;

        if (sound_started == 0)
        {
            return;
        }

        sfx = S_FindName(name);
        zone_c.Cache_Check(&sfx->cache);
    }

    public static sound_c.sfx_t* S_PrecacheSound(char* name)
    {
        sound_c.sfx_t* sfx;

        if (sound_started == 0 || nosound.value != 0)
        {
            return null;
        }

        sfx = S_FindName(name);

        if (precache.value != 0)
        {
            snd_mem_c.S_LoadSound(sfx);
        }

        return sfx;
    }

    public static sound_c.sfx_t* S_PrecacheSound(string name)
    {
        return S_PrecacheSound(common_c.StringToChar(name));
    }

    public static sound_c.channel_t* SND_PickChannel(int entnum, int entchannel)
    {
        int ch_idx;
        int first_to_die;
        int life_left;

        first_to_die = -1;
        life_left = 0x7fffffff;

        for (ch_idx = bspfile_c.NUM_AMBIENTS; ch_idx < bspfile_c.NUM_AMBIENTS + sound_c.MAX_DYNAMIC_CHANNELS; ch_idx++)
        {
            if (entchannel != 0 && channels[ch_idx].entnum == entnum && (channels[ch_idx].entchannel == entchannel || entchannel == -1))
            {
                first_to_die = ch_idx;
                break;
            }

            if (channels[ch_idx].entnum == cl_main_c.cl.viewentity && entnum != cl_main_c.cl.viewentity && channels[ch_idx].sfx != null)
            {
                continue;
            }

            if (channels[ch_idx].end - paintedtime < life_left)
            {
                life_left = channels[ch_idx].end - paintedtime;
                first_to_die = ch_idx;
            }
        }

        if (first_to_die == -1)
        {
            return null;
        }

        if (channels[first_to_die].sfx != null)
        {
            channels[first_to_die].sfx = null;
        }

        return &channels[first_to_die];
    }

    public static void SND_Spatialize(sound_c.channel_t* ch)
    {
        float dot;
        float ldist, rdist, dist;
        float lscale, rscale, scale;
        Vector3 source_vec;
        sound_c.sfx_t* snd;

        source_vec = new();

        if (ch->entnum == cl_main_c.cl.viewentity)
        {
            ch->leftvol = ch->master_vol;
            ch->rightvol = ch->master_vol;
            return;
        }

        snd = ch->sfx;
        mathlib_c.VectorSubtract(ch->origin, listener_origin, source_vec);

        dist = mathlib_c.VectorNormalize(source_vec) * ch->dist_mult;

        dot = mathlib_c.DotProduct(listener_right, source_vec);

        if (shm->channels == 1)
        {
            rscale = 1.0f;
            lscale = 1.0f;
        }
        else
        {
            rscale = 1.0f + dot;
            lscale = 1.0f - dot;
        }

        scale = (1.0f - dist) * rscale;
        ch->rightvol = (int)(ch->master_vol * scale);

        if (ch->rightvol < 0)
        {
            ch->rightvol = 0;
        }

        scale = (1.0f - dist) * lscale;
        ch->leftvol = (int)(ch->master_vol * scale);

        if (ch->leftvol < 0)
        {
            ch->leftvol = 0;
        }
    }

    public static void S_StartSound(int entnum, int entchannel, sound_c.sfx_t* sfx, Vector3 origin, float fvol, float attenuation)
    {
        sound_c.channel_t* target_chan, check;
        sound_c.sfxcache_t* sc;
        int vol;
        int ch_idx;
        int skip;

        if (sound_started == 0)
        {
            return;
        }

        if (sfx == null)
        {
            return;
        }

        if (nosound.value != 0)
        {
            return;
        }

        vol = (int)fvol * 255;

        target_chan = SND_PickChannel(entnum, entchannel);

        if (target_chan == null)
        {
            return;
        }

        memset_c.memset((object*)target_chan, 0, sizeof(sound_c.channel_t));
        mathlib_c.VectorCopy(origin, target_chan->origin);
        target_chan->dist_mult = attenuation / sound_nominal_clip_dist;
        target_chan->master_vol = vol;
        target_chan->entnum = entnum;
        target_chan->entchannel = entchannel;
        SND_Spatialize(target_chan);

        if (target_chan->leftvol == 0 && target_chan->rightvol == 0)
        {
            return;
        }

        sc = S_LoadSound(sfx);

        if (sc == null)
        {
            target_chan->sfx = null;
            return;
        }

        target_chan->sfx = sfx;
        target_chan->pos = 0;
        target_chan->end = paintedtime + sc->length;

        check = &channels[bspfile_c.NUM_AMBIENTS];

        for (ch_idx = bspfile_c.NUM_AMBIENTS; ch_idx < bspfile_c.NUM_AMBIENTS + sound_c.MAX_DYNAMIC_CHANNELS; ch_idx++, check++)
        {
            if (check == target_chan)
            {
                continue;
            }

            if (check->sfx == sfx && check->pos == 0)
            {
                skip = rand_c.rand() % (int)(0.1f * shm->speed);

                if (skip >= target_chan->end)
                {
                    skip = target_chan->end - 1;
                }

                target_chan->pos += skip;
                target_chan->end -= skip;
                break;
            }
        }
    }

    public static void S_StopSound(int entnum, int entchannel)
    {
        int i;

        for (i = 0; i < sound_c.MAX_DYNAMIC_CHANNELS; i++)
        {
            if (channels[i].entnum == entnum && channels[i].entchannel == entchannel)
            {
                channels[i].end = 0;
                channels[i].sfx = null;
                return;
            }
        }
    }

    public static void S_StopAllSounds(bool clear)
    {
        int i;

        if (sound_started == 0)
        {
            return;
        }

        total_channels = sound_c.MAX_DYNAMIC_CHANNELS + bspfile_c.NUM_AMBIENTS;

        for (i = 0; i < sound_c.MAX_CHANNELS; i++)
        {
            if (channels[i].sfx != null)
            {
                channels[i].sfx = null;
            }
        }

        common_c.Q_memset(*channels, 0, sound_c.MAX_CHANNELS * sizeof(sound_c.channel_t));

        if (clear)
        {
            S_ClearBuffer();
        }
    }

    public static void S_StopAllSoundsC()
    {
        S_StopAllSounds(true);
    }

    public static void S_ClearBuffer()
    {
        int clear;

#if _WIN32
        if (sound_started == 0|| shm == null || (shm->buffer == null && pDSBuf == 0))
#else
        if (sound_started == 0 || shm == null || shm->buffer == null)
#endif
        {
            return;
        }

        if (shm->samplebits == 8)
        {
            clear = 0x80;
        }
        else
        {
            clear = 0;
        }

#if _WIN32
        // I ain't writing allat, glad that happened or sorry it did
#endif
        {
            common_c.Q_memset(*shm->buffer, clear, shm->samples * shm->samplebits / 8);
        }
    }

    public static void S_StaticSound(sound_c.sfx_t* sfx, Vector3 origin, float vol, float attenuation)
    {
        sound_c.channel_t* ss;
        sound_c.sfxcache_t* sc;

        if (sfx == null)
        {
            return;
        }

        if (total_channels == sound_c.MAX_CHANNELS)
        {
            console_c.Con_Printf("total_channels == MAX_CHANNELS\n");
            return;
        }

        ss = &channels[total_channels];
        total_channels++;

        sc = S_LoadSound(sfx);

        if (sc == null)
        {
            return;
        }

        if (sc->loopstart == -1)
        {
            console_c.Con_Printf($"Sound {*sfx->name} not looped\n");
            return;
        }

        ss->sfx = sfx;
        mathlib_c.VectorCopy(origin, ss->origin);
        ss->master_vol = (int)vol;
        ss->dist_mult = (attenuation / 64) / sound_nominal_clip_dist;
        ss->end = paintedtime + sc->length;

        SND_Spatialize(ss);
    }

    public static void S_UpdateAmbientSounds()
    {
        model_c.mleaf_t* l;
        float vol;
        int ambient_channel;
        sound_c.channel_t* chan;

        if (!snd_ambient)
        {
            return;
        }

        if (cl_main_c.cl.worldmodel == null)
        {
            return;
        }

        l = model_c.Mod_PointInLeaf(listener_origin, cl_main_c.cl.worldmodel);

        if (l == null || ambient_level.value == 0)
        {
            for (ambient_channel = 0; ambient_channel < bspfile_c.NUM_AMBIENTS; ambient_channel++)
            {
                channels[ambient_channel].sfx = null;
            }

            return;
        }

        for (ambient_channel = 0; ambient_channel < bspfile_c.NUM_AMBIENTS; ambient_channel++)
        {
            chan = &channels[ambient_channel];
            chan->sfx = &ambient_sfx[ambient_channel];

            vol = ambient_level.value * l->ambient_sound_level[ambient_channel];

            if (vol < 8)
            {
                vol = 0;
            }

            if (chan->master_vol < vol)
            {
                chan->master_vol += (int)host_c.host_frametime * ambient_fade.value;

                if (chan->master_vol > vol)
                {
                    chan->master_vol = (int)vol;
                }
            }
            else if (chan->master_vol > vol)
            {
                chan->master_vol -= (int)host_c.host_frametime * ambient_fade.value;

                if (chan->master_vol < vol)
                {
                    chan->master_vol = (int)vol;
                }
            }

            chan->leftvol = chan->rightvol = chan->master_vol;
        }
    }

    public static void S_Update(Vector3 origin, Vector3 forward, Vector3 right, Vector3 up)
    {
        int i, j;
        int total;
        sound_c.channel_t* ch;
        sound_c.channel_t* combine;

        if (sound_started == 0 || (snd_blocked > 0))
        {
            return;
        }

        mathlib_c.VectorCopy(origin, listener_origin);
        mathlib_c.VectorCopy(forward, listener_forward);
        mathlib_c.VectorCopy(right, listener_right);
        mathlib_c.VectorCopy(up, listener_up);

        S_UpdateAmbientSounds();

        combine = null;

        ch = channels + bspfile_c.NUM_AMBIENTS;

        for (i = bspfile_c.NUM_AMBIENTS; i < total_channels; i++, ch++)
        {
            if (ch->sfx == null)
            {
                continue;
            }

            SND_Spatialize(ch);

            if (ch->leftvol == 0 && ch->rightvol == 0)
            {
                continue;
            }

            if (i >= sound_c.MAX_DYNAMIC_CHANNELS + bspfile_c.NUM_AMBIENTS)
            {
                if (combine != null && combine->sfx == ch->sfx)
                {
                    combine->leftvol += ch->leftvol;
                    combine->rightvol += ch->rightvol;
                    ch->leftvol = ch->rightvol = 0;
                    continue;
                }

                combine = channels + sound_c.MAX_DYNAMIC_CHANNELS + bspfile_c.NUM_AMBIENTS;

                for (j = sound_c.MAX_DYNAMIC_CHANNELS + bspfile_c.NUM_AMBIENTS; j < i; j++, combine++)
                {
                    if (combine->sfx == ch->sfx)
                    {
                        break;
                    }
                }

                if (j == total_channels)
                {
                    combine = null;
                }
                else
                {
                    if (combine != ch)
                    {
                        combine->leftvol += ch->leftvol;
                        combine->rightvol += ch->rightvol;
                        ch->leftvol = ch->rightvol = 0;
                    }
                    continue;
                }
            }
        }

        if (snd_show.value != 0)
        {
            total = 0;
            ch = channels;

            for (i = 0; i < total_channels; i++, ch++)
            {
                if (ch->sfx != null && (ch->leftvol != 0 || ch->rightvol != 0))
                {
                    total++;
                }
            }

            console_c.Con_Printf($"----({total})----\n");
        }

        S_Update_();
    }

    public static void GetSoundTime()
    {
        int samplepos;
        int buffers;
        int oldsamplepos;
        int fullsamples;

        oldsamplepos = buffers = 0;

        fullsamples = shm->samples / shm->channels;

#if __sun__
        soundtime = SNDDMA_GetSamples();
#else
        samplepos = snd_win_c.SNDDMA_GetDMAPos();

        if (samplepos < oldsamplepos)
        {
            buffers++;

            if (paintedtime > 0x40000000)
            {
                buffers = 0;
                paintedtime = fullsamples;
                S_StopAllSounds(true);
            }
        }

        oldsamplepos = samplepos;

        soundtime = buffers * fullsamples + samplepos / shm->channels;
#endif
    }

    public static void S_ExtraUpdate()
    {
#if _WIN32
        IN_Accumulate();
#endif

        if (snd_noextraupdate.value != 0)
        {
            return;
        }

        S_Update_();
    }

    public static void S_Update_()
    {
        uint endtime;
        int samps;

        if (sound_started == 0 || (snd_blocked > 0))
        {
            return;
        }

        GetSoundTime();

        if (paintedtime < soundtime)
        {
            paintedtime = soundtime;
        }

        endtime = (uint)(soundtime + _snd_mixahead.value * shm->speed);
        samps = shm->samples >> (shm->channels - 1);

        if (endtime - soundtime > samps)
        {
            endtime = (uint)(soundtime + samps);
        }

#if _WIN32
        // Code that idk how code!!!
#endif

        S_PaintChannels(endtime);

        snd_win_c.SNDDMA_Submit();
    }

    public static void S_Play()
    {
        int hash = 345;
        int i;
        char* name = null;
        sound_c.sfx_t* sfx;

        i = 1;

        while (i < cmd_c.Cmd_Argc())
        {
            if (common_c.Q_strrchr(cmd_c.Cmd_Argv(i), '.') == null)
            {
                common_c.Q_strcpy(name, cmd_c.Cmd_Argv(i));
                common_c.Q_strcat(name, ".wav");
            }
            else
            {
                common_c.Q_strcpy(name, cmd_c.Cmd_Argv(i));
            }

            sfx = S_PrecacheSound(name);
            S_StartSound(hash++, 0, sfx, listener_origin, 1.0f, 1.0f);
            i++;
        }
    }

    public static void S_PlayVol()
    {
        int hash = 543;
        int i;
        float vol;
        char* name = null;
        sound_c.sfx_t* sfx;

        i = 1;

        while (i < cmd_c.Cmd_Argc())
        {
            if (common_c.Q_strrchr(cmd_c.Cmd_Argv(i), '.') == null)
            {
                common_c.Q_strcpy(name, cmd_c.Cmd_Argv(i));
                common_c.Q_strcat(name, ".wav");
            }
            else
            {
                common_c.Q_strcpy(name, cmd_c.Cmd_Argv(i));
            }

            sfx = S_PrecacheSound(name);
            vol = common_c.Q_atof(cmd_c.Cmd_Argv(i + 1));
            S_StartSound(hash++, 0, sfx, listener_origin, vol, 1.0f);
            i += 2;
        }
    }

    public static void S_SoundList()
    {
        int i;
        sound_c.sfx_t* sfx;
        sound_c.sfxcache_t* sc;
        int size, total;

        total = 0;

        for (sfx = known_sfx, i = 0; i < num_sfx; i++, sfx++)
        {
            sc = (sound_c.sfxcache_t*)zone_c.Cache_Check(&sfx->cache);

            if (sc == null)
            {
                continue;
            }

            size = sc->length * sc->width * (sc->stereo + 1);
            total += size;

            if (sc->loopstart >= 0)
            {
                console_c.Con_Printf("L");
            }
            else
            {
                console_c.Con_Printf(" ");
            }

            console_c.Con_Printf($"({sc->width*8}) {size} : {*sfx->name}");
        }

        console_c.Con_Printf($"Total resident: {total}\n");
    }

    public static void S_LocalSound(char* sound)
    {
        sound_c.sfx_t* sfx;

        if (nosound.value != 0)
        {
            return;
        }

        if (sound_started == 0)
        {
            return;
        }

        sfx = S_PrecacheSound(sound);

        if (sfx == null)
        {
            console_c.Con_Printf($"S_LocalSound: can't cache {*sound}\n");
            return;
        }

        S_StartSound(cl_main_c.cl.viewentity, -1, sfx, mathlib_c.vec3_origin, 1, 1);
    }

    public static void S_ClearPrecache()
    {
    }

    public static void S_BeginPrecaching()
    {
    }

    public static void S_EndPrecaching()
    {
    }
}