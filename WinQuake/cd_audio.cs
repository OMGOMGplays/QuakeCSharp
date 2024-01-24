namespace Quake;

public unsafe class cd_audio_c
{
    public static cvar_c.cvar_t bgmvolume;

    public const int ADDRESS_MODE_HSG = 0;
    public const int ADDRESS_MODE_RED_BOOK = 1;

    public const int STATUS_ERROR_BIT = 0x8000;
    public const int STATUS_BUSY_BIT = 0x0200;
    public const int STATUS_DONE_BIT = 0x0100;
    public const int STATUS_ERROR_MASK = 0x00ff;

    public const int ERROR_WRITE_PROTECT = 0;
    public const int ERROR_UNKNOWN_UNIT = 1;
    public const int ERROR_DRIVE_NOT_READY = 2;
    public const int ERROR_UNKNOWN_COMMAND = 3;
    public const int ERROR_CRC_ERROR = 4;
    public const int ERROR_BAD_REQUEST_LEN = 5;
    public const int ERROR_SEEK_ERROR = 6;
    public const int ERROR_UNKNOWN_MEDIA = 7;
    public const int ERROR_SECTOR_NOT_FOUND = 8;
    public const int ERROR_OUT_OF_PAPER = 9;
    public const int ERROR_WRITE_FAULT = 10;
    public const int ERROR_READ_FAULT = 11;
    public const int ERROR_GENERAL_FAILURE = 12;
    public const int ERROR_RESERVED_13 = 13;
    public const int ERROR_RESERVED_14 = 14;
    public const int ERROR_BAD_DISK_CHANGE = 15;

    public const int COMMAND_READ = 3;
    public const int COMMAND_WRITE = 12;
    public const int COMMAND_PLAY_AUDIO = 132;
    public const int COMMAND_STOP_AUDIO = 133;
    public const int COMMAND_RESUME_AUDIO = 136;

    public const int READ_REQUEST_AUDIO_CHANNEL_INFO = 4;
    public const int READ_REQUEST_DEVICE_STATUS = 6;
    public const int READ_REQUEST_MEDIA_CHANGE = 9;
    public const int READ_REQUEST_AUDIO_DISK_INFO = 10;
    public const int READ_REQUEST_AUDIO_TRACK_INFO = 11;
    public const int READ_REQUEST_AUDIO_STATUS = 15;

    public const int WRITE_REQUEST_EJECT = 0;
    public const int WRITE_REQUEST_RESET = 2;
    public const int WRITE_REQUEST_AUDIO_CHANNEL_INFO = 3;

    public const int STATUS_DOOR_OPEN = 0x00000001;
    public const int STATUS_DOOR_UNLOCKED = 0x00000002;
    public const int STATUS_RAW_SUPPORT = 0x00000004;
    public const int STATUS_READ_WRITE = 0x00000008;
    public const int STATUS_AUDIO_SUPPORT = 0x00000010;
    public const int STATUS_INTERLEAVE_SUPPORT = 0x00000020;
    public const int STATUS_BIT_6_RESERVED = 0x00000040;
    public const int STATUS_PREFETCH_SUPPORT = 0x00000080;
    public const int STATUS_AUDIO_MANIPULATION_SUPPORT = 0x00000100;
    public const int STATUS_RED_BOOK_ADDRESS_SUPPORT = 0x00000200;

    public const int MEDIA_NOT_CHANGED = 1;
    public const int MEDIA_STATUS_UNKNOWN = 0;
    public const int MEDIA_CHANGED = -1;

    public const int AUDIO_CONTROL_MASK = 0xd0;
    public const int AUDIO_CONTROL_DATA_TRACK = 0x40;
    public const int AUDIO_CONTROL_AUDIO_2_TRACK = 0x00;
    public const int AUDIO_CONTROL_AUDIO_2P_TRACK = 0x10;
    public const int AUDIO_CONTROL_AUDIO_4_TRACK = 0x80;
    public const int AUDIO_CONTROL_AUDIO_4P_TRACK = 0x90;

    public const int AUDIO_STATUS_PAUSED = 0x0001;

    public struct playAudioRequest
    {
        public char addressingMode;
        public int startLocation;
        public int sectors;
    }

    public struct readRequest
    {
        public char mediaDescriptor;
        public short bufferOffset;
        public short bufferSegment;
        public short length;
        public short startSector;
        public int volumeID;
    }

    public struct writeRequest
    {
        public char mediaDescriptor;
        public short bufferOffset;
        public short bufferSegment;
        public short length;
        public short startSector;
        public int volumeID;
    }

    public struct cd_request_union
    {
        public playAudioRequest playAudio;
        public readRequest read;
        public writeRequest write;
    }

    public struct cd_request
    {
        public char headerLength;
        public char unit;
        public char command;
        public short status;
        public char* reserved;
        public cd_request_union x;
    }

    public struct audioChannelInfo_s
    {
        public char code;
        public char channel0input;
        public char channel0volume;
        public char channel1input;
        public char channel1volume;
        public char channel2input;
        public char channel2volume;
        public char channel3input;
        public char channel3volume;
    }

    public struct deviceStatus_s
    {
        public char code;
        public int status;
    }

    public struct mediaChange_s
    {
        public char code;
        public char status;
    }

    public struct audioDiskInfo_s
    {
        public char code;
        public char lowTrack;
        public char highTrack;
        public int leadOutStart;
    }

    public struct audioTrackInfo_s
    {
        public char code;
        public char track;
        public int start;
        public char control;
    }

    public struct audioStatus_s
    {
        public char code;
        public short status;
        public int PRstartLocation;
        public int PRendLocation;
    }

    public struct reset_s
    {
        public char code;
    }

    public struct readInfo_u
    {
        public audioChannelInfo_s audioChannelInfo;
        public deviceStatus_s deviceStatus;
        public mediaChange_s mediaChange;
        public audioDiskInfo_s audioDiskInfo;
        public audioTrackInfo_s audioTrackInfo;
        public audioStatus_s audioStatus;
        public reset_s reset;
    }

    public const int MAXMIMUM_TRACKS = 100;

    public struct track_info
    {
        public int start;
        public int length;
        public bool isData;
    }

    public struct cd_info
    {
        public bool valid;
        public int leadOutAddress;
        public track_info* track;
        public byte lowTrack;
        public byte highTrack;
    }

    public static cd_request* cdRequest;
    public static readInfo_u* readInfo;
    public static cd_info cd;

    public static bool playing = false;
    public static bool wasPlaying = false;
    public static bool mediaCheck = false;
    public static bool initialized = false;
    public static bool enabled = true;
    public static bool playLooping = false;
    public static short cdRequestSegment;
    public static short cdRequestOffset;
    public static short readInfoSegment;
    public static short readInfoOffset;
    public static byte* remap;
    public static byte cdrom;
    public static byte playTrack;
    public static byte cdvolume;

    public static int RedBookToSector(int rb)
    {
        byte minute;
        byte second;
        byte frame;

        minute = (byte)((rb >> 16) & 0xff);
        second = (byte)((rb >> 8) & 0xff);
        frame = (byte)(rb & 0xff);

        return minute * 60 * 75 + second * 75 + frame;
    }

    public static void CDAudio_Reset()
    {
        cdRequest->headerLength = (char)13;
        cdRequest->unit = (char)0;
        cdRequest->command = (char)COMMAND_WRITE;
        cdRequest->status = 0;

        cdRequest->x.write.mediaDescriptor = (char)0;
        cdRequest->x.write.bufferOffset = readInfoOffset;
        cdRequest->x.write.bufferSegment = readInfoSegment;
        cdRequest->x.write.length = (short)sizeof(reset_s);
        cdRequest->x.write.startSector = 0;
        cdRequest->x.write.volumeID = 0;

        readInfo->reset.code = (char)WRITE_REQUEST_RESET;

        // DOS stuff
    }

    public static void CDAudio_Eject()
    {
        cdRequest->headerLength = (char)13;
        cdRequest->unit = (char)0;
        cdRequest->command = (char)COMMAND_WRITE;
        cdRequest->status = 0;

        cdRequest->x.write.mediaDescriptor = (char)0;
        cdRequest->x.write.bufferOffset = readInfoOffset;
        cdRequest->x.write.bufferSegment = readInfoSegment;
        cdRequest->x.write.length = (short)sizeof(reset_s);
        cdRequest->x.write.startSector = 0;
        cdRequest->x.write.volumeID = 0;

        readInfo->reset.code = (char)WRITE_REQUEST_EJECT;
    }

    public static int CDAudio_GetAudioTrackInfo(byte track, int* start)
    {
        byte control;

        cdRequest->headerLength = (char)13;
        cdRequest->unit = (char)0;
        cdRequest->command = (char)COMMAND_READ;
        cdRequest->status = 0;

        cdRequest->x.read.mediaDescriptor = (char)0;
        cdRequest->x.read.bufferOffset = readInfoOffset;
        cdRequest->x.read.bufferSegment = readInfoSegment;
        cdRequest->x.read.length = (short)sizeof(audioTrackInfo_s);
        cdRequest->x.read.startSector = 0;
        cdRequest->x.read.volumeID = 0;

        readInfo->audioTrackInfo.code = (char)READ_REQUEST_AUDIO_TRACK_INFO;
        readInfo->audioTrackInfo.track = (char)track;

        if ((cdRequest->status & STATUS_ERROR_BIT) != 0)
        {
            console_c.Con_DPrintf($"CDAudio_GetAudioTrackInfo {cdRequest->status & 0xffff}\n");
            return -1;
        }

        *start = readInfo->audioTrackInfo.start;
        control = (byte)(readInfo->audioTrackInfo.control & AUDIO_CONTROL_MASK);
        return (control & AUDIO_CONTROL_DATA_TRACK);
    }

    public static int CDAudio_GetAudioDiskInfo()
    {
        int n;

        cdRequest->headerLength = (char)13;
        cdRequest->unit = (char)0;
        cdRequest->command = (char)COMMAND_READ;
        cdRequest->status = 0;

        cdRequest->x.read.mediaDescriptor = (char)0;
        cdRequest->x.read.bufferOffset = readInfoOffset;
        cdRequest->x.read.bufferSegment = readInfoSegment;
        cdRequest->x.read.length = (short)sizeof(audioDiskInfo_s);
        cdRequest->x.read.startSector = 0;
        cdRequest->x.read.volumeID = 0;

        readInfo->audioDiskInfo.code = (char)READ_REQUEST_AUDIO_DISK_INFO;

        if ((cdRequest->status & STATUS_ERROR_BIT) != 0)
        {
            console_c.Con_DPrintf($"CDAudio_GetAudioDiskInfo {cdRequest->status & 0xffff}\n");
            return -1;
        }

        cd.valid = true;
        cd.lowTrack = (byte)readInfo->audioDiskInfo.lowTrack;
        cd.highTrack = (byte)readInfo->audioDiskInfo.highTrack;
        cd.leadOutAddress = readInfo->audioDiskInfo.leadOutStart;

        for (n = cd.lowTrack; n <= cd.highTrack; n++)
        {
            cd.track[n].isData = CDAudio_GetAudioTrackInfo((byte)n, &cd.track[n].start) == -1 ? false : true;

            if (n > cd.lowTrack)
            {
                cd.track[n - 1].length = RedBookToSector(cd.track[n].start) - RedBookToSector(cd.track[n - 1].start);

                if (n == cd.highTrack)
                {
                    cd.track[n].length = RedBookToSector(cd.leadOutAddress) - RedBookToSector(cd.track[n].start);
                }
            }
        }

        return 0;
    }

    public static int CDAudio_GetAudioStatus()
    {
        cdRequest->headerLength = (char)13;
        cdRequest->unit = (char)0;
        cdRequest->command = (char)COMMAND_READ;
        cdRequest->status = 0;

        cdRequest->x.read.mediaDescriptor = (char)0;
        cdRequest->x.read.bufferOffset = readInfoOffset;
        cdRequest->x.read.bufferSegment = readInfoSegment;
        cdRequest->x.read.length = (short)sizeof(audioStatus_s);
        cdRequest->x.read.startSector = 0;
        cdRequest->x.read.volumeID = 0;

        if ((cdRequest->status & STATUS_ERROR_BIT) != 0)
        {
            return -1;
        }

        return 0;
    }

    public static int CDAudio_MediaChange()
    {
        cdRequest->headerLength = (char)13;
        cdRequest->unit = (char)0;
        cdRequest->command = (char)COMMAND_READ;
        cdRequest->status = 0;

        cdRequest->x.read.mediaDescriptor = (char)0;
        cdRequest->x.read.bufferOffset = readInfoOffset;
        cdRequest->x.read.bufferSegment = readInfoSegment;
        cdRequest->x.read.length = (short)sizeof(mediaChange_s);
        cdRequest->x.read.startSector = 0;
        cdRequest->x.read.volumeID = 0;

        readInfo->mediaChange.code = (char)READ_REQUEST_MEDIA_CHANGE;

        return readInfo->mediaChange.status;
    }

    public static void CDAudio_SetVolume(byte volume)
    {
        if (!initialized || !enabled)
        {
            return;
        }

        cdRequest->headerLength = (char)13;
        cdRequest->unit = (char)0;
        cdRequest->command = (char)COMMAND_WRITE;
        cdRequest->status = 0;

        cdRequest->x.read.mediaDescriptor = (char)0;
        cdRequest->x.read.bufferOffset = readInfoOffset;
        cdRequest->x.read.bufferSegment = readInfoSegment;
        cdRequest->x.read.length = (short)sizeof(audioChannelInfo_s);
        cdRequest->x.read.startSector = 0;
        cdRequest->x.read.volumeID = 0;

        readInfo->audioChannelInfo.code = (char)WRITE_REQUEST_AUDIO_CHANNEL_INFO;
        readInfo->audioChannelInfo.channel0input = (char)0;
        readInfo->audioChannelInfo.channel0volume = (char)0;
        readInfo->audioChannelInfo.channel1input = (char)1;
        readInfo->audioChannelInfo.channel1volume = (char)0;
        readInfo->audioChannelInfo.channel2input = (char)2;
        readInfo->audioChannelInfo.channel2volume = (char)0;
        readInfo->audioChannelInfo.channel3input = (char)3;
        readInfo->audioChannelInfo.channel3volume = (char)0;

        readInfo->audioChannelInfo.channel0volume = (char)volume;
        readInfo->audioChannelInfo.channel1volume = (char)volume;

        cdvolume = volume;
    }

    public static void CDAudio_Play(byte track, bool looping)
    {
        int volume;

        if (!initialized || !enabled)
        {
            return;
        }

        if (!cd.valid)
        {
            return;
        }

        track = remap[track];

        if (playing)
        {
            if (playTrack == track)
            {
                return;
            }

            CDAudio_Stop();
        }

        playLooping = looping;

        if (track < cd.lowTrack || track > cd.highTrack)
        {
            console_c.Con_DPrintf($"CDAudio_Play: Bad track number {track}.\n");
            return;
        }

        playTrack = track;

        if (cd.track[track].isData)
        {
            console_c.Con_DPrintf($"CDAudio_Play: Can not play data.\n");
            return;
        }

        volume = (int)(bgmvolume.value * 255.0);

        if (volume < 0)
        {
            cvar_c.Cvar_SetValue("bgmvolume", 0.0f);
            volume = 0;
        }
        else if (volume > 255)
        {
            cvar_c.Cvar_SetValue("bgmvolume", 255.0f);
            volume = 255;
        }

        CDAudio_SetVolume((byte)volume);

        cdRequest->headerLength = (char)13;
        cdRequest->unit = (char)0;
        cdRequest->command = (char)COMMAND_PLAY_AUDIO;
        cdRequest->status = 0;

        cdRequest->x.playAudio.addressingMode = (char)ADDRESS_MODE_RED_BOOK;
        cdRequest->x.playAudio.startLocation = cd.track[track].start;
        cdRequest->x.playAudio.sectors = cd.track[track].length;

        if ((cdRequest->status & STATUS_ERROR_BIT) != 0)
        {
            console_c.Con_DPrintf($"CDAudio_Play: track {track} failed\n");
            cd.valid = false;
            playing = false;
            return;
        }

        playing = true;
    }

    public static void CDAudio_Stop()
    {
        if (!initialized || !enabled)
        {
            return;
        }

        cdRequest->headerLength = (char)13;
        cdRequest->unit = (char)0;
        cdRequest->command = (char)COMMAND_STOP_AUDIO;
        cdRequest->status = 0;

        wasPlaying = playing;
        playing = false;
    }

    public static void CDAudio_Pause()
    {
        CDAudio_Stop();
    }

    public static void CDAudio_Resume()
    {
        if (!initialized || !enabled)
        {
            return;
        }

        if (!cd.valid)
        {
            return;
        }

        if (!wasPlaying)
        {
            return;
        }

        cdRequest->headerLength = (char)13;
        cdRequest->unit = (char)0;
        cdRequest->command = (char)COMMAND_RESUME_AUDIO;
        cdRequest->status = 0;

        playing = true;
    }

    public static void CD_f()
    {
        char* command;
        int ret;
        int n;
        int startAddress;

        if (cmd_c.Cmd_Argc() < 2)
        {
            return;
        }

        command = cmd_c.Cmd_Argv(1);

        if (common_c.Q_strcasecmp(command->ToString(), "on") == 0)
        {
            enabled = true;
            return;
        }

        if (common_c.Q_strcasecmp(command->ToString(), "off") == 0)
        {
            if (playing)
            {
                CDAudio_Stop();
            }

            enabled = false;
            return;
        }

        if (common_c.Q_strcasecmp(command->ToString(), "reset") == 0)
        {
            enabled = true;

            if (playing)
            {
                CDAudio_Stop();
            }

            for (n = 0; n < 256; n++)
            {
                remap[n] = (byte)n;
            }

            CDAudio_Reset();
            CDAudio_GetAudioDiskInfo();
            return;
        }

        if (common_c.Q_strcasecmp(command->ToString(), "remap") == 0)
        {
            ret = cmd_c.Cmd_Argc() - 2;

            if (ret <= 0)
            {
                for (n = 1; n < 256; n++)
                {
                    if (remap[n] != n)
                    {
                        console_c.Con_Printf($" {n} -> {remap[n]}\n");
                    }
                }

                return;
            }

            for (n = 1; n <= ret; n++)
            {
                remap[n] = (byte)common_c.Q_atoi(cmd_c.Cmd_Argv(n + 1));
            }

            return;
        }

        if (!cd.valid)
        {
            console_c.Con_Printf("No CD in player.\n");
            return;
        }

        if (common_c.Q_strcasecmp(command->ToString(), "play") == 0)
        {
            CDAudio_Play((byte)common_c.Q_atoi(cmd_c.Cmd_Argv(2)), false);
            return;
        }

        if (common_c.Q_strcasecmp(command->ToString(), "loop") == 0)
        {
            CDAudio_Play((byte)common_c.Q_atoi(cmd_c.Cmd_Argv(2)), true);
            return;
        }

        if (common_c.Q_strcasecmp(command->ToString(), "stop") == 0)
        {
            CDAudio_Stop();
            return;
        }

        if (common_c.Q_strcasecmp(command->ToString(), "pause") == 0)
        {
            CDAudio_Pause();
            return;
        }

        if (common_c.Q_strcasecmp(command->ToString(), "resume") == 0)
        {
            CDAudio_Resume();
            return;
        }

        if (common_c.Q_strcasecmp(command->ToString(), "eject") == 0)
        {
            if (playing)
            {
                CDAudio_Stop();
            }

            CDAudio_Eject();
            cd.valid = false;
            return;
        }

        if (common_c.Q_strcasecmp(command->ToString(), "info") == 0)
        {
            console_c.Con_Printf($"{cd.highTrack - cd.lowTrack + 1} tracks\n");

            for (n = cd.lowTrack; n <= cd.highTrack; n++)
            {
                ret = CDAudio_GetAudioTrackInfo((byte)n, &startAddress);
                string retNull = ret == null ? "data " : "music ";
                console_c.Con_Printf($"Track {n}: {retNull} at {(startAddress>>16)&0xff}:{(startAddress>>8)&0xff}\n");
            }

            if (playing)
            {
                string looping = playLooping == true ? "looping" : "playing";
                console_c.Con_Printf($"Currently {looping} track {playTrack}\n");
            }

            console_c.Con_Printf($"Volume is {cdvolume}\n");
            CDAudio_MediaChange();
            console_c.Con_Printf($"Status {cdRequest->status & 0xffff}\n");
            return;
        }
    }

    public static void CDAudio_Update()
    {
        int ret;
        int newVolume;
        double lastUpdate = 0.0;

        if (!initialized || !enabled)
        {
            return;
        }

        if ((host_c.realtime - lastUpdate) < 0.25)
        {
            return;
        }

        lastUpdate = host_c.realtime;

        if (mediaCheck)
        {
            double lastCheck = 0.0;

            if ((host_c.realtime - lastCheck) < 5.0)
            {
                return;
            }

            lastCheck = host_c.realtime;

            ret = CDAudio_MediaChange();

            if (ret == MEDIA_CHANGED)
            {
                console_c.Con_DPrintf("CDAudio: media changed\n");
                playing = false;
                wasPlaying = false;
                cd.valid = false;
                CDAudio_GetAudioDiskInfo();
                return;
            }
        }

        newVolume = (int)(bgmvolume.value * 255.0);

        if (newVolume != cdvolume)
        {
            if (newVolume < 0)
            {
                cvar_c.Cvar_SetValue("bgmvolume", 0.0f);
                newVolume = 0;
            }
            else if (newVolume > 255)
            {
                cvar_c.Cvar_SetValue("bgmvolume", 1.0f);
                newVolume = 255;
            }

            CDAudio_SetVolume((byte)newVolume);
        }

        if (playing)
        {
            CDAudio_GetAudioStatus();

            if ((cdRequest->status & STATUS_BUSY_BIT) == 0)
            {
                playing = false;

                if (playLooping)
                {
                    CDAudio_Play(playTrack, true);
                }
            }
        }
    }

    //public static int CDAudio_Init() // A lot of DOS stuff...
    //{
    //    char* memory;
    //    int n;

    //    if (client_c.cls.state == client_c.cactive_t.ca_dedicated)
    //    {
    //        return -1;
    //    }

    //    if (common_c.COM_CheckParm("-nocdaudio") != 0)
    //    {
    //        return -1;
    //    }

    //    if (common_c.COM_CheckParm("-cdmediacheck") != 0)
    //    {
    //        mediaCheck = true;
    //    }


    //}

    public static void CDAudio_Shutdown()
    {
        if (!initialized)
        {
            return;
        }

        CDAudio_Stop();
    }
}