namespace Quake;

public unsafe class snd_win_c
{
    public const int WAV_BUFFERS = 64;
    public const int WAV_MASK = 0x3F;
    public const int WAV_BUFFER_SIZE = 0x0400;
    public const int SECONDARY_BUFFER_SIZE = 0x10000;

    public enum sndinitstat { SIS_SUCCESS, SIS_FAILURE, SIS_NOTAVAIL }

    public static bool wavonly;
    public static bool dsound_init;
    public static bool wav_init;
    public static bool snd_firsttime = true, snd_isdirect, snd_iswave;
    public static bool primary_format_set;

    public static int sample16;
    public static int snd_sent, snd_completed;

    // Handles and stuff, idk how to implement (yet)
}