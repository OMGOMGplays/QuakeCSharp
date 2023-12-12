namespace Quake;

public unsafe class console_c
{
    public static int con_totallines;
    public static int con_backscroll;
    public static bool con_forcedup;
    public static bool con_initialized;
    public static byte* con_chars;
    public static int con_notifylines;

    public static int con_linewidth;

    public static float con_cursorspeed = 4;

    public static int CON_TEXTSIZE = 16384;

    public static int con_current;
    public static char* con_text = null;

    public static cvar_c.cvar_t con_notifytime = new cvar_c.cvar_t { name = "con_notifytime", value = '3' };

    public static int NUM_CON_TIMES = 4;

    public static float* con_times;

    public static int con_vislines;

    public static bool con_debuglog;

    public static int MAXCMDLINE = 256;
    public static char[][] key_lines = new char[32][];
    public static int edit_line;
    public static int key_linepos;

    public static void Con_ToggleConsole_f()
    {
        if (keys_c.key_dest == keys_c.keydest_t.key_console)
        {
            if (client_c.cls.state == client_c.cactive_t.ca_connected)
            {
                keys_c.key_dest = keys_c.keydest_t.key_game;
            }
        }
    }
}