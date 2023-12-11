namespace Quake;

public unsafe class sys_win_c
{
    public static int MINIMUM_WIN_MEMORY = 0x0880000;
    public static int MAXIMUM_WIN_MEMORY = 0x1000000;

    public static double CONSOLE_ERROR_TIMEOUT = 60.0;

    public static int PAUSE_SLEEP = 50;
    public static int NOT_FOCUS_SLEEP = 20;

    public int starttime;
    public bool ActiveApp, Minimized;
    public bool WinNT;

    public static double pfreq;
    public static double curtime = 0.0;
    public static double lastcurtime = 0.0;
    public static int lowshift;
    public bool isDedicated;
    public static bool sc_return_on_enter = false;
    public IntPtr hinput, houtput;

    public static char* tracking_tag = common_c.StringToChar("Clams & Mooses");

    public static IntPtr tevent;
    public static IntPtr hFile;
    public static IntPtr heventParent;
    public static IntPtr heventChild;

    public volatile int sys_checksum;

    public void Sys_PageIn(void* ptr, int size)
    {
        byte* x;
        int j, m, n;

        x = (byte*)ptr;

        for (n = 0; n < 4; n++)
        {
            for (m = 0; m < (size - 16 * 0x1000); m += 4)
            {
                sys_checksum += *(int*)&x[m];
                sys_checksum += *(int*)&x[m + 16 * 0x1000];
            }
        }
    }

    public static int MAX_HANDLES = 10;
    public static FileStream* sys_handles;

    public static int findhandle()
    {
        int i;

        for (i = 1; i < MAX_HANDLES; i++)
        {
            if (sys_handles[i] == null)
            {
                return i;
            }
        }

        Sys_Error("out of handles");
        return -1;
    }

    public static int filelength(char* f)
    {
        int t;

        t = vid_win_c.VID_ForceUnlockedAndReturnState();

        try
        {
            using (FileStream fs = File.Open(f->ToString(), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int length = (int)fs.Length;

                vid_win_c.VID_ForceLockState(t);

                return length;
            }
        }
        catch (Exception ex)
        {
            Sys_Error($"Error getting file length for {f}: {ex.Message}");
            return -1;
        }
    }

    public static int Sys_FileOpenRead(char* path, out int handle, out int length)
    {
        int t;

        t = vid_win_c.VID_ForceUnlockedAndReturnState();
        handle = -1;
        length = -1;

        try
        {
            int i = findhandle();

            using (FileStream fs = File.Open(path->ToString(), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                sys_handles[i] = fs;
                handle = i;
                length = (int)fs.Length;

                vid_win_c.VID_ForceLockState(t);

                return length;
            }
        }
        catch (Exception ex)
        {
            Sys_Error($"Error opening file {path} for reading: {ex.Message}");
            return -1;
        }
    }

    public static int Sys_FileOpenWrite(char* path)
    {
        File* f;
        int i;
        int t;
        FileStream fs;

        t = vid_win_c.VID_ForceUnlockedAndReturnState();

        i = findhandle();

        try
        {
            using (FileStream file = File.Open(path->ToString(), FileMode.Open))
            {
                sys_handles[i] = file;
            }
        }
        catch (Exception ex)
        {
            Sys_Error($"Error opening {path}: {ex.Message}");
        }

        vid_win_c.VID_ForceLockState(t);

        return i;
    }

    public static void Sys_FileClose(int handle)
    {
        int t;

        t = vid_win_c.VID_ForceUnlockedAndReturnState();

        try
        {
            if (sys_handles[handle] is FileStream fs)
            {
                fs.Close();
                sys_handles[handle] = null;
            }
            else
            {
                Sys_Error($"Error: Attempted to close invalid handle {handle}");
            }
        }
        catch (Exception ex)
        {
            Sys_Error($"Error closing file with handle {handle}: {ex.Message}");
        }

        vid_win_c.VID_ForceLockState(t);
    }

    public static void Sys_FileSeek(int handle, int position)
    {
        int t;

        t = vid_win_c.VID_ForceUnlockedAndReturnState();
        
        try
        {
            if (sys_handles[handle] is FileStream fs)
            {
                fs.Seek(position, SeekOrigin.Begin);
            }
            else
            {
                Sys_Error($"Error: Attempted to seek on invalid handle {handle}");
            }
        }
        catch (Exception ex)
        {
            Sys_Error($"Error seeking on file with handle {handle}: {ex.Message}");
        }

        vid_win_c.VID_ForceLockState(t);
    }

    public static int Sys_FileRead(int handle, byte[] dest, int count)
    {
        int t, x;

        t = vid_win_c.VID_ForceUnlockedAndReturnState();

        try
        {
            x = FileStreamFromHandle(handle).Read(dest, 0, count);
        }
        finally
        {
            vid_win_c.VID_ForceLockState(t);
        }

        return x;
    }

    private static FileStream FileStreamFromHandle(int handle)
    {
        return sys_handles[handle] ?? throw new InvalidOperationException("Invalid handle type.");
    }

    public static int Sys_FileWrite(int handle, byte[] data, int count)
    {
        int t, x;

        t = vid_win_c.VID_ForceUnlockedAndReturnsState();

        try
        {
            x = FileStreamFromHandle(hande).Write(data, 0, count);
        }
        finally
        {
            vid_win_c.VID_ForceLockState(t);
        }

        return x;
    }

    public static int Sys_FileTime(char* path)
    {
        int t, retval;

        t = vid_win_c.VID_ForceUnlockedAndReturnState();

        try
        {
            using (FileStream fs = new FileStream(path->ToString(), FileMode.Open, FileAccess.Read))
            {
                fs.Close();
                retval = 1;
            }
        }
        catch (FileNotFoundException)
        {
            retval = -1;
        }
        finally
        {
            vid_win_c.VID_ForceLockState(t);
        }

        return retval;
    }

    public static void Sys_mkdir(char* path)
    {
        Directory.CreateDirectory(path->ToString());
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

    public static void Sys_MakeCodeWriteable(ulong startaddr, ulong length)
    {
        IntPtr startaddress = (IntPtr)startaddr;
        uint flOldProtect;

        if (!VirtualProtect(startaddress, (uint)length, 0x04, out flOldProtect))
        {
            throw new Exception("Protection change failed");
        }
    }

#if !_M_IX86

    public static void Sys_SetFPCW()
    {
    }

    public static void Sys_PushFPCW_SetHigh()
    {
    }

    public static void Sys_PopFPCW()
    {
    }

    public static void MaskExceptions()
    {
    }

#endif

    [DllImport("kernel32.dll")]
    private static extern bool QueryPerformanceFrequency(out long lpFrequency);

    public static long GetPerformanceFrequency()
    {

    }

    public void Sys_Init()
    {
        uint PerformanceFreq;
        uint lowpart, highpart;

    }
}