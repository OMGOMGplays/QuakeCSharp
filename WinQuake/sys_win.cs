using System.Runtime.CompilerServices;

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
    public static bool isDedicated;
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

    public static int Sys_FileOpenRead(char* path)
    {
        int i;
        int t;

        
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
            Sys_Error($"Error opening {path->ToString()}: {ex.Message}");
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

    public static int Sys_FileRead(int handle, byte* dest, int count)
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

    public static int Sys_FileWrite(int handle, void* data, int count)
    {
        int t, x;

        t = vid_win_c.VID_ForceUnlockedAndReturnsState();

        try
        {
            x = FileStreamFromHandle(handle).Write(data, 0, count);
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
    private static extern bool QueryPerformanceFrequency(out LARGE_INTEGER lpFrequency);

    [DllImport("kernel32.dll")]
    private static extern bool GetVersionEx(ref OSVERSIONINFO osVersionInfo);

    private const int VER_PLATFORM_WIN32s = 0;
    private const int VER_PLATFORM_WIN32_NT = 2;

    public void Sys_Init()
    {
        LARGE_INTEGER PerformanceFreq;
        uint lowpart, highpart;
        OSVERSIONINFO vinfo = new();

        MaskExceptions();
        Sys_SetFPCW();

        if (!QueryPerformanceFrequency(out PerformanceFreq))
        {
            Sys_Error("No hardware timer available");
        }

        lowpart = (uint)PerformanceFreq.lowpart;
        highpart = (uint)PerformanceFreq.highpart;
        lowshift = 0;

        while (highpart != 0 || (lowpart > 2000000.0))
        {
            lowshift++;
            lowpart >>= 1;
            lowpart |= (highpart & 1) << 31;
            highpart >>= 1;
        }

        pfreq = 1.0 / lowpart;

        Sys_InitFloatTime();

        vinfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFO));

        if (!GetVersionEx(ref vinfo))
        {
            Sys_Error("Couldn't get OS info");
        }

        if (vinfo.dwMajorVersion < 4 || vinfo.dwPlatformId == VER_PLATFORM_WIN32s)
        {
            Sys_Error("Quake requires at least Win95 or NT 4.0");
        }

        bool WinNT = vinfo.dwPlatformId == VER_PLATFORM_WIN32_NT;
    }

    private struct LARGE_INTEGER
    {
        public int lowpart;
        public int highpart;
    }

    private struct OSVERSIONINFO
    {
        public int dwOSVersionInfoSize;
        public int dwMajorVersion;
        public int dwMinorVersion;
        public int dwBuildNumber;
        public int dwPlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szCSDVersion;
    }

    public static void Sys_Error(char* error, params object[] args)
    {
        char[] text = new char[1024], text2 = new char[1024];
        char* text3 = common_c.StringToChar("Press Enter To Exit\n");
        char* text4 = common_c.StringToChar("***********************************\n");
        char* text5 = common_c.StringToChar("\n");
        uint dummy;
        double starttime;
        int in_sys_error0 = 0;
        int in_sys_error1 = 0;
        int in_sys_error2 = 0;
        int in_sys_error3 = 0;

        if (in_sys_error3 == 0)
        {
            in_sys_error3 = 1;
            vid_win_c.VID_ForceUnlockedAndReturnState();
        }

        Console.WriteLine($"{text} {error->ToString()}");

        if (isDedicated)
        {
            Console.WriteLine($"{text} {error->ToString()}");

            Console.WriteLine($"ERROR: {text}\n");
            // Write errors to file... somehow

            starttime = Sys_FloatTime();
            sc_return_on_enter = true;

            while (Sys_ConsoleInput() == null && ((Sys_FloatTime() - starttime) < CONSOLE_ERROR_TIMEOUT))
            {
            }
        }
        else
        {
            if (in_sys_error0 == 0)
            {
                in_sys_error0 = 1;
                vid_win_c.VID_SetDefaultMode();
                Console.WriteLine($"Quake Error: {error->ToString()}");
            }
            else
            {
                Console.WriteLine($"Double Quake Error");
            }
        }

        if (in_sys_error1 == 0)
        {
            in_sys_error1 = 1;
            host_c.Host_Shutdown();
        }

        if (in_sys_error2 == 0)
        {
            in_sys_error2 = 1;
            conproc_c.DeinitConProc();
        }

        Environment.Exit(1);
    }

    public static void Sys_Error(string error, params object[] args)
    {
        Sys_Error(common_c.StringToChar(error), args);
    }

    public static void Sys_Printf(char* fmt, params object[] args)
    {
        char[] text = new char[1024];
        IntPtr dummy;

        if (isDedicated)
        {
            Console.WriteLine(text.ToString());

            // WriteFile...
        }
    }

    public static void Sys_Printf(string fmt, params object[] args)
    {
        Sys_Printf(common_c.StringToChar(fmt), args);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool FreeConsole();

    public static void Sys_Quit()
    {
        vid_win_c.VID_ForceUnlockedAndReturnState();

        host_c.Host_Shutdown();

        if (tevent != 0)
        {
            CloseHandle(tevent);
        }

        if (isDedicated)
        {
            FreeConsole();
        }

        conproc_c.DeinitConProc();

        Environment.Exit(0);
    }

    public static double Sys_FloatTime()
    {
        int sametimecount = 0;
        uint oldtime = 0;
        int first = 1;
        LARGE_INTEGER PerformanceCount;
        uint temp, t2;
        double time;

        Sys_PushFPCW_SetHigh();

        QueryPerformanceFrequency(out PerformanceCount);

        temp = ((uint)PerformanceCount.lowpart >> lowshift) | ((uint)PerformanceCount.highpart << (32 - lowshift));

        if (first != 0)
        {
            oldtime = temp;
            first = 0;
        }
        else
        {
            if ((temp <= oldtime) && ((oldtime - temp) < 0x10000000))
            {
                oldtime = temp;
            }
            else
            {
                t2 = temp - oldtime;

                time = (double)t2 * pfreq;
                oldtime = temp;

                curtime += time;

                if (curtime == lastcurtime)
                {
                    sametimecount++;

                    if (sametimecount > 100000)
                    {
                        curtime += 1.0;
                        sametimecount = 0;
                    }
                }
                else
                {
                    sametimecount = 0;
                }

                lastcurtime = curtime;
            }
        }

        Sys_PopFPCW();

        return curtime;
    }

    public static void Sys_InitFloatTime()
    {
        int j;

        Sys_FloatTime();

        j = common_c.COM_CheckParm("-starttime");

        if (j != 0)
        {
            curtime = (double)(common_c.Q_atof(&common_c.com_argv[j + 1]));
        }
        else
        {
            curtime = 0.0;
        }

        lastcurtime = curtime;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetNumberOfConsoleInputEvents(IntPtr hConsoleInput, uint* lpcNumberOfEvents);

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct INPUT_RECORD
    {
        [FieldOffset(0)]
        public ushort EventType;

        [FieldOffset(4)]
        public KeyEventRecord KeyEvent;
    }

    public struct KeyEventRecord
    {
        public bool keyDown;
        public ushort repeatCount;
        public ushort virtualKeyCode;
        public ushort virtualScanCode;
        public char unicodeChar;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool ReadConsoleInput(IntPtr hConsoleInput, [Out] INPUT_RECORD[] lpbuffer, uint nLength, uint* lpNumberOfEventsRead);

    public static char* Sys_ConsoleInput()
    {
        char[] text = new char[256];
        int len;
        INPUT_RECORD[] recs = new INPUT_RECORD[1024];
        int count;
        int i, dummy;
        int ch, numread = 0, numevents = 0;

        if (!isDedicated)
        {
            return null;
        }

        for (; ; )
        {
            if (!GetNumberOfConsoleInputEvents(hinput, (uint*)numevents))
            {
                Sys_Error(common_c.StringToChar("Error getting # of console events"));
            }

            if (numevents <= 0)
            {
                break;
            }

            if (!ReadConsoleInput(hinput, recs, 1, (uint*)numread))
            {
                Sys_Error(common_c.StringToChar("Error reading console input"));
            }

            if (numread != 1)
            {
                Sys_Error(common_c.StringToChar("Couldn't read console input"));
            }

            if (recs[0].EventType == 0x0001)
            {
                if (!recs[0].Event.KeyEvent.bKeyDown)
                {

                }
            }
        }
    }
}