namespace Quake;

public unsafe class screen
{
	public static float scr_con_current;
	public static float scr_conlines;

	public static int scr_fullupdate;
	public static int sb_lines;

	public static bool scr_disabled_for_loading;
	public static bool scr_drawloading;
	public static float scr_disabled_time;
	public static bool scr_skipupdate;

	public static float oldscreensize, oldfov;
	public static cvar_t scr_viewsize = new cvar_t { "viewsize", "100", true };
	public static cvar_t scr_fov = new cvar_t { "fov", "90" };
	public static cvar_t scr_conspeed = new cvar_t { "scr_conspeed", "300" };
	public static cvar_t scr_centertime = new cvar_t { "scr_centertime", "2" };
	public static cvar_t scr_showram = new cvar_t { "showram", "1" };
	public static cvar_t scr_showturtle = new cvar_t { "showturtle", "0" };
	public static cvar_t scr_showpause = new cvar_t { "showpause", "1" };
	public static cvar_t scr_printspeed = new cvar_t { "scr_printspeed", "0" };

	public static bool scr_initialized;

	public static wad.qpic_t* scr_ram;
	public static wad.qpic_t* scr_net;
	public static wad.qpic_t* scr_turtle;

	public static int clearconsole;
	public static int clearnotify;

	public static viddef_t vid;

	public static vrect_t* pconupdate;
	public static vrect_t scr_vrect;

	public static int scr_copytop;
	public static int scr_copyeverything;

	public static bool block_drawing;

	public static string scr_centerstring;
	public static float scr_centertime_start;
	public static float scr_centertime_off;
	public static int scr_center_lines;
	public static int scr_erase_lines;
	public static int scr_erase_center;

	public void SCR_CenterPrint(char* str)
	{
		common.Q_strncpy(scr_centerstring.ToCharArray(), str->ToString().ToCharArray(), 1023);
		scr_centertime_off = scr_centertime.value;
		scr_centertime_start = cl.time;

		scr_center_lines = 1;

		while (*str != '\0')
		{
			if (*str == '\n')
			{
				scr_center_lines++;
			}

			str++;
		}
	}

	public void SCR_EraseCenterString()
	{
		int y;

		if (scr_erase_center++ > vid.numpages)
		{
			scr_erase_lines = 0;
			return;
		}

		if (scr_center_lines <= 4)
		{
			y = vid.height * 0.35;
		}
		else
		{
			y = 48;
		}

		scr_copytop = 1;
		Draw_TileClear(0, y, vid.width, 8 * scr_erase_lines);
	}

	public void SCR_DrawCenterString()
	{
		char* start;
		int l;
		int j;
		int x, y;
		int remaining;

		if (cl.intermission)
		{
			remaining = scr_printspeed.value * (cl.time - scr_centertime_start);
		}
		else
		{
			remaining = 9999;
		}

		scr_erase_center = 0;
		start = scr_centerstring;

		if (scr_enter_lines <= 4)
		{
			y = vid.height * 0.35;
		}
		else
		{
			y = 48;
		}

		do
		{
			for (l = 0; l < 40; l++)
			{
				if (start[l] == '\n' || start[l] == null)
				{
					break;
				}
			}

			x = (vid.width - l * 8) / 2;

			for (j = 0; j < l; x += 8)
			{
				Draw_Character(x, y, start[j]);

				if (remaining-- == 0)
				{
					return;
				}
			}

			y += 8;

			while (*start != null && *start != '\n')
			{
				start++;
			}

			if (*start == null)
			{
				break;
			}

			start++;
		} while (true);
	}

	public void SCR_CheckDrawCentering()
	{
		scr_copytop = 1;

		if (scr_center_lines > scr_erase_lines)
		{
			scr_erase_center = scr_center_lines;
		}

		scr_centertime_off -= host_frametime;

		if (scr_centertime_off <= 0 && !cl.intermission)
		{
			return;
		}

		if (key_dest != key_game)
		{
			return;
		}

		SCR_DrawCenterString();
	}

	public float CalcFov(float fov_x, float width, float height)
	{
		float a;
		float x;

		if (fov_x < 1 ||  fov_x > 179)
		{
			sys_win.Sys_Error($"Bad fov: {fov_x}");
		}

		x = width / MathF.Tan(fov_x / 360 * MathF.PI);

		a = MathF.Atan(height / x);

		a = a * 360 / MathF.PI;

		return a;
	}

	public static void SRC_CalcRefDef()
	{
		vrect_t vrect;
		float size;

		scr_fullupdate = 0;
		vid.recalc_refdef = 0;

		Sbar_Changed();

		if (scr_viewsize.value < 30)
		{
			Cvar_Set("viewsize", "30");
		}

		if (scr_viewsize.value > 120)
		{
			Cvar_Set("viewsize", "120");
		}

		if (scr_fov.value < 10)
		{
			Cvar_Set("fov", "10");
		}

		if (scr_fov.value > 170)
		{
			Cvar_Set("fov", "170");
		}

		r_refdef.fov_x = scr_fov.value;
		r_refdef.fov_y = CalcFov(r_refdef.fov_x, r_refdef.vrect.width, r_refdef.vrect.height);
	
		if (cl.intermission)
		{
			size = 120;
		}
		else
		{
			size = scr_viewsize.value;
		}

		if (size >= 120)
		{
			sb_lines = 0;
		}
		else if (size >= 110)
		{
			sb_lines = 24;
		}
		else
		{
			sb_lines = 24 + 16 + 8;
		}

		vrect.x = 0;
		vrect.y = 0;
		vrect.width = vid.width;
		vrect.height = vid.height;

		R_SetVrect(vrect, scr_vrect, sb_lines);

		if (scr_con_current > vid.height)
		{
			scr_con_current = vid.height;
		}

		R_ViewChanged(vrect, sb_lines, vid.aspect);
	}

	public void SCR_SizeUp_f()
	{
		Cvar_SetValue("viewsize", scr_viewsize.value + 10);
		vid.recalc_refdef = 1;
	}

	public void SCR_SizeDown_f()
	{
		Cvar_SetValue("viewsize", scr_viewsize.value - 10);
		vid.recalc_refdef = 1;
	}

	public void SCR_Init()
	{
		Cvar_RegisterVariable(scr_fov);
		Cvar_RegisterVariable(scr_viewsize);
		Cvar_RegisterVariable(scr_conspeed);
		Cvar_RegisterVariable(scr_showram);
		Cvar_RegisterVariable(scr_showturtle);
		Cvar_RegisterVariable(scr_showpause);
		Cvar_RegisterVariable(scr_centertime);
		Cvar_RegisterVariable(scr_printspeed);

		Cmd_AddCommand("screenshot", SCR_ScreenShot_f);
		Cmd_AddCommand("sizeup", SCR_SizeUp_f);
		Cmd_AddCommand("sizedown", SCR_SizeDown_f);

		scr_ram = Draw_PicFromWad("ram");
		scr_net = Draw_PicFromWad("net");
		scr_turtle = Draw_PicFromWad("turtle");

		scr_initialized = true;
	}

	public void SCR_DrawTurtle()
	{
		int count = 0;
		
		if (scr_showturtle.value == 0)
		{
			return;
		}

		if (host_frametime < 0.1)
		{
			count = 0;
			return;
		}

		count++;

		if (count < 3)
		{
			return;
		}

		Draw_Pic(scr_vrect.x, scr_vrect.y, scr_turtle);
	}

	public void SCR_DrawNet()
	{
		if (realtime - cl.last_received_message < 0.3)
		{
			return;
		}

		if (cls.demoplayback)
		{
			return;
		}

		Draw_Pic(scr_vrect.x + 64, scr_vrect.y, scr_net);
	}

	public void SCR_DrawPause()
	{
		qpic_t pic;

		if (scr_showpause.value == 0)
		{
			return;
		}

		if (!cl.paused)
		{
			return;
		}

		pic = DrawPic("gfx/pause.lmp");
		Draw_Pic((vid.width - pic->width) / 2, (vid.height - 48 - pic->height) / 2, pic);
	}
}