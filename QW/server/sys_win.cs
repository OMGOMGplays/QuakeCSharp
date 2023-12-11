namespace Quake.Server;

public unsafe class sys_win_c
{
    public static cvar_c.cvar_t sys_nostdout = new cvar_c.cvar_t { name = "sys_nostdout", value = 0 };

    public static int Sys_FileTime(string path)
    {
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {

            }

            return 1;
        }
        catch (FileNotFoundException)
        {
            return -1;
        }
        catch (IOException)
        {
            return -1;
        }
    }

    public static void Sys_mkdir(string path)
    {
        Directory.CreateDirectory(path);
    }

    public static void Sys_Error(string error, params object[] args)
    {
        string text = string.Format(error, args);

        //Console.WriteLine($"{error} {args}");

        Console.WriteLine($"ERROR: {text}");
        Environment.Exit(1);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern void GetSystemTimePreciseAsFileTime(out long filetime);

    static long starttime;

    public static double Sys_DoubleTime()
    {
        double t;
        long currentTime;

        GetSystemTimePreciseAsFileTime(out currentTime);

        if (starttime == 0)
        {
            starttime = currentTime;
        }

        t = (currentTime - starttime) * 0.0001;

        return t;
    }

    public static string Sys_ConsoleInput()
    {
        char[] text = new char[256];
        int len = 0;

        while (Console.KeyAvailable)
        {
            ConsoleKeyInfo keyinfo = Console.ReadKey(true);
            char c = keyinfo.KeyChar;

            Console.Write(c);

            if (c == '\r')
            {
                text[len] = '\0';
                Console.WriteLine();
                len = 0;
                return new string(text);
            }

            if (c == 8)
            {
                if (len > 0)
                {
                    Console.Write(" \b");
                    len--;
                    text[len] = '\0';
                }

                continue;
            }

            text[len] = c;
            len++;
            text[len] = '\0';

            if (len == text.Length)
            {
                len = 0;
            }
        }

        return null;
    }

    public static void Sys_Printf(string fmt)
    {
        if (sys_nostdout.value != 0)
        {
            return;
        }

        Console.WriteLine(fmt);
    }

    public static void Sys_Quit()
    {
        Environment.Exit(0);
    }

    public static void Sys_Init()
    {
        cvar_c.Cvar_RegisterVariable(sys_nostdout);
    }

    string* newargv;

    int main(int argc, char** argv)
    {
        quakedef_c.quakeparms_t parms;
        double newtime, time, oldtime;
        char cwd;
        timeval timeout;
        fd_set fdset;
        int t;

        common_c.COM_InitArgv(argc, argv);

        parms.argc = common_c.com_argc;
        parms.argv = common_c.com_argv;

        parms.memsize = 16 * 1024 * 1024;

        if ((t = common_c.COM_CheckParm("-heapsize")) != 0 && t + 1 < common_c.com_argc)
        {
            parms.memsize = common_c.Q_atoi(common_c.com_argv[t + 1].ToString()) * 1024;
        }

        if ((t = common_c.COM_CheckParm("-mem")) != 0 && t + 1 < common_c.com_argc)
        {
            parms.memsize = common_c.Q_atoi(common_c.com_argv[t + 1].ToString()) * 1024 * 1024;
        }

        parms.membase = zone_c.Z_Malloc(parms.memsize);

        if (parms.membase == null)
        {
            Sys_Error("Insufficient memory.\n");
        }

        parms.basedir = ".";
        parms.cachedir = null;

        SV_Init(parms);

        SV_Frame(0.1);

        oldtime = Sys_DoubleTime() - 0.1;

        while (true)
        {
            FD_ZERO(fdset);
            FD_SET(net_socket, fdset);
            timeout.tv_sec = 0;
            timeout.tv_usec = 100;

            if (Socket.Select(fdset, null, null, timeout) == -1)
            {
                continue;
            }

            newtime = Sys_DoubleTime();
            time = newtime - oldtime;
            oldtime = newtime;

            SV_Frame(time);
        }

        return 1;
    }
}