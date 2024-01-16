namespace Quake;

public unsafe class snd_null_c
{
    public static cvar_c.cvar_t bgmvolume = new cvar_c.cvar_t { name = "bgmvolume", value = (char)1, archive = true };
    public static cvar_c.cvar_t volume = new cvar_c.cvar_t { name = "volume", value = (char)0.7f, archive = true };

    public static void S_Init()
    {
    }

    public static void S_AmbientOff()
    {
    }

    public static void S_AmbientOn()
    {
    }

    public static void S_Shutdown()
    {
    }

    public static void S_TouchSound(char* sample)
    {
    }

    public static void S_ClearBuffer()
    {
    }

    public static void S_StaticSound(sound_c.sfx_t* sfx, Vector3 origin, float vol, float attenuation)
    {
    }

    public static void S_StartSound(int entnum, int entchannel, sound_c.sfx_t* sfx, Vector3 origin, float fvol, float attenuation)
    {
    }

    public static void S_StopSound(int entnum, int entchannel)
    {
    }

    public static sound_c.sfx_t* S_PrecacheSound(char* sample)
    {
        return null;
    }

    public static void S_ClearPrecache()
    {
    }

    public static void S_Update(Vector3 origin, Vector3 v_forward, Vector3 v_right, Vector3 v_up)
    {
    }

    public static void S_StopAllSounds(bool clear)
    {
    }

    public static void S_BeginPrecaching()
    {
    }

    public static void S_EndPrecaching()
    {
    }

    public static void S_ExtraUpdate()
    {
    }

    public static void S_LocalSound(char* s)
    {
    }
}