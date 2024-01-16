namespace Quake;

public unsafe class sound_c
{
    public const int DEFAULT_SOUND_PACKET_VOLUME = 255;
    public const float DEFAULT_SOUND_PACKET_ATTENUATION = 1.0f;

    public struct portable_samplepair_t
    {
        public int left;
        public int right;
    }

    public struct sfx_t
    {
        public char* name;
        public zone_c.cache_user_t cache;
    }

    public struct sfxcache_t
    {
        public int length;
        public int loopstart;
        public int speed;
        public int width;
        public int stereo;
        public byte* data;
    }

    public struct dma_t
    {
        public bool gamealive;
        public bool soundalive;
        public bool splitbuffer;
        public int channels;
        public int samples;
        public int submission_chunk;
        public int samplepos;
        public int samplebits;
        public int speed;
        public char* buffer;
    }

    public struct channel_t
    {
        public sfx_t* sfx;
        public int leftvol;
        public int rightvol;
        public int end;
        public int pos;
        public int looping;
        public int entnum;
        public int entchannel;
        public Vector3 origin;
        public float dist_mult;
        public int master_vol;
    }

    public struct wavinfo_t
    {
        public int rate;
        public int width;
        public int channels;
        public int loopstart;
        public int samples;
        public int dataofs;
    }

    public const int MAX_CHANNELS = 128;
    public const int MAX_DYNAMIC_CHANNELS = 6;
}