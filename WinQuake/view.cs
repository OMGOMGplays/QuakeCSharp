namespace Quake;

public unsafe class view_c
{
    public static cvar_c.cvar_t lcd_x = new cvar_c.cvar_t { name = "lcd_x", value = (char)0 };
    public static cvar_c.cvar_t lcd_yaw = new cvar_c.cvar_t { name = "lcd_yaw", value = (char)0 };

    public static cvar_c.cvar_t scr_ofsx = new cvar_c.cvar_t { name = "scr_ofsx", value = (char)0, server = false };
    public static cvar_c.cvar_t scr_ofsy = new cvar_c.cvar_t { name = "scr_ofsy", value = (char)0, server = false };
    public static cvar_c.cvar_t scr_ofsz = new cvar_c.cvar_t { name = "scr_ofsz", value = (char)0, server = false };

    public static cvar_c.cvar_t cl_rollspeed = new cvar_c.cvar_t { name = "cl_rollspeed", value = (char)200 };
    public static cvar_c.cvar_t cl_rollangle = new cvar_c.cvar_t { name = "cl_rollangle", value = (char)2.0f };

    public static cvar_c.cvar_t cl_bob = new cvar_c.cvar_t { name = "cl_bob", value = (char)0.02f, server = false };
    public static cvar_c.cvar_t cl_bobcycle = new cvar_c.cvar_t { name = "cl_bobcycle", value = (char)0.6f, server = false };
    public static cvar_c.cvar_t cl_bobup = new cvar_c.cvar_t { name = "cl_bobup", value = (char)0.5f, server = false };

    public static cvar_c.cvar_t v_kicktime = new cvar_c.cvar_t { name = "v_kicktime", value = (char)0.5f, server = false };
    public static cvar_c.cvar_t v_kickroll = new cvar_c.cvar_t { name = "v_kickroll", value = (char)0.6f, server = false };
    public static cvar_c.cvar_t v_kickpitch = new cvar_c.cvar_t { name = "v_kickpitch", value = (char)0.6f, server = false };

    public static cvar_c.cvar_t v_iyaw_cycle = new cvar_c.cvar_t { name = "v_iyaw_cycle", value = (char)2 };
    public static cvar_c.cvar_t v_iroll_cycle = new cvar_c.cvar_t { name = "v_iroll_cycle", value = (char)0.5f };
    public static cvar_c.cvar_t v_ipitch_cycle = new cvar_c.cvar_t { name = "v_ipitch_cycle", value = (char)1 };
    public static cvar_c.cvar_t v_iyaw_level = new cvar_c.cvar_t { name = "v_iyaw_level", value = (char)0.3f };
    public static cvar_c.cvar_t v_iroll_level = new cvar_c.cvar_t { name = "v_iroll_level", value = (char)0.1f };
    public static cvar_c.cvar_t v_ipitch_level = new cvar_c.cvar_t { name = "v_ipitch_level", value = (char)0.3f };

    public static cvar_c.cvar_t v_idlescale = new cvar_c.cvar_t { name = "v_idlescale", value = (char)0 };

    public static cvar_c.cvar_t crosshair = new cvar_c.cvar_t { name = "crosshair", value = (char)0 };
    public static cvar_c.cvar_t cl_crossx = new cvar_c.cvar_t { name = "cl_crossx", value = (char)0 };
    public static cvar_c.cvar_t cl_crossy = new cvar_c.cvar_t { name = "cl_crossy", value = (char)0 };
}