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
    public static int painedtime;

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
        }
    }
}