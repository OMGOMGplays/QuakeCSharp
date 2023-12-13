namespace Quake;

public unsafe class screen_c
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
	public static cvar_c.cvar_t scr_viewsize = new cvar_t { "viewsize", "100", true };
	public static cvar_c.cvar_t scr_fov = new cvar_t { "fov", "90" };
	public static cvar_c.cvar_t scr_conspeed = new cvar_t { "scr_conspeed", "300" };
	public static cvar_c.cvar_t scr_centertime = new cvar_t { "scr_centertime", "2" };
	public static cvar_c.cvar_t scr_showram = new cvar_t { "showram", "1" };
	public static cvar_c.cvar_t scr_showturtle = new cvar_t { "showturtle", "0" };
	public static cvar_c.cvar_t scr_showpause = new cvar_t { "showpause", "1" };
	public static cvar_c.cvar_t scr_printspeed = new cvar_t { "scr_printspeed", "0" };

	public static bool scr_initialized;

	public static wad_c.qpic_t* scr_ram;
	public static wad_c.qpic_t* scr_net;
	public static wad_c.qpic_t* scr_turtle;

	public static int clearconsole;
	public static int clearnotify;

	public static vid_win_c.viddef_t vid;

	public static vid_win_c.vrect_t* pconupdate;
	public static vid_win_c.vrect_t* scr_vrect;

	public static int scr_copytop;
	public static int scr_copyeverything;

	public static bool block_drawing;

	public static string* scr_centerstring;
	public static float scr_centertime_start;
	public static float scr_centertime_off;
	public static int scr_center_lines;
	public static int scr_erase_lines;
	public static int scr_erase_center;

	public void SCR_CenterPrint(char* str)
	{
		common_c.Q_strncpy(scr_centerstring->ToCharArray(), str->ToString().ToCharArray(), 1023);
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
			y = (int)(vid_win_c.vid.height * 0.35f);
		}
		else
		{
			y = 48;
		}

		scr_copytop = 1;
		draw_c.Draw_TileClear(0, y, vid.width, 8 * scr_erase_lines);
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
		start = (char*)scr_centerstring;

		if (scr_center_lines <= 4)
		{
			y = (int)(vid_win_c.vid.height * 0.35);
		}
		else
		{
			y = 48;
		}

		do
		{
			for (l = 0; l < 40; l++)
			{
				if (start[l] == '\n' || start[l] == 0)
				{
					break;
				}
			}

			x = (vid.width - l * 8) / 2;

			for (j = 0; j < l; x += 8)
			{
				draw_c.Draw_Character(x, y, start[j]);

				if (remaining-- == 0)
				{
					return;
				}
			}

			y += 8;

			while (*start != 0 && *start != '\n')
			{
				start++;
			}

			if (*start == 0)
			{
				break;
			}

			start++;
		} while (true);
	}

	public void SCR_CheckDrawCenterString()
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

		if (keys_c.key_dest != keys_c.keydest_t.key_game)
		{
			return;
		}

		SCR_DrawCenterString();
	}

	public static float CalcFov(float fov_x, float width, float height)
	{
		float a;
		float x;

		if (fov_x < 1 || fov_x > 179)
		{
			sys_win_c.Sys_Error($"Bad fov: {fov_x}");
		}

		x = width / MathF.Tan(fov_x / 360 * MathF.PI);

		a = MathF.Atan(height / x);

		a = a * 360 / MathF.PI;

		return a;
	}

	public static void SCR_CalcRefDef()
	{
		vid_win_c.vrect_t* vrect = null;
		float size;

		scr_fullupdate = 0;
		vid.recalc_refdef = 0;

		Sbar_Changed();

		if (scr_viewsize.value < 30)
		{
			cvar_c.Cvar_Set("viewsize", "30");
		}

		if (scr_viewsize.value > 120)
		{
			cvar_c.Cvar_Set("viewsize", "120");
		}

		if (scr_fov.value < 10)
		{
            cvar_c.Cvar_Set("fov", "10");
		}

		if (scr_fov.value > 170)
		{
            cvar_c.Cvar_Set("fov", "170");
		}

        r_main_c.r_refdef.fov_x = scr_fov.value;
		r_main_c.r_refdef.fov_y = CalcFov(r_main_c.r_refdef.fov_x, r_main_c.r_refdef.vrect.width, r_main_c.r_refdef.vrect.height);

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

		vrect->x = 0;
		vrect->y = 0;
		vrect->width = vid.width;
		vrect->height = vid.height;

		r_main_c.R_SetVrect(vrect, scr_vrect, sb_lines);

		if (scr_con_current > vid.height)
		{
			scr_con_current = vid.height;
		}

		r_main_c.R_ViewChanged(vrect, sb_lines, vid.aspect);
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

	public void SCR_DrawRam()
	{
		if (!scr_showram.value)
		{
			return;
		}

		if (!r_cache_trash)
		{
			return;
		}

		Draw_Pic(scr_vrect.x + 32, scr_vrect.y, scr_ram);
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
		if (quakedef_c.realtime - cl.last_received_message < 0.3)
		{
			return;
		}

		if (cls.demoplayback)
		{
			return;
		}

		draw_c.Draw_Pic(scr_vrect.x + 64, scr_vrect.y, scr_net);
	}

	public void SCR_DrawPause()
	{
		wad_c.qpic_t* pic;

		if (scr_showpause.value == 0)
		{
			return;
		}

		if (!cl.paused)
		{
			return;
		}

		pic = draw_c.Draw_CachePic("gfx/pause.lmp");
		draw_c.Draw_Pic((vid.width - pic->width) / 2, (vid.height - 48 - pic->height) / 2, pic);
	}

	public void SCR_DrawLoading()
	{
		wad_c.qpic_t* pic;

		if (!scr_drawloading)
		{
			return;
		}

		pic = Draw_CachePic("gfx/loading.lmp");
		Draw_Pic((vid.width - pic->width) / 2, (vid.height - 48 - pic->height) / 2, pic);
	}

	public static void SCR_SetUpToDrawConsole()
	{
		console_c.Con_CheckResize();

		if (scr_drawloading)
		{
			return;
		}

		bool con_forcedup = cl.worldmodel == null || cls.signon != SIGNONS;

		if (con_forcedup)
		{
			scr_conlines = vid.height;
			scr_con_current = scr_conlines;
		}
		else if (keys_c.key_dest == keys_c.keydest_t.key_console)
		{
			scr_conlines = vid.height / 2;
		}
		else
		{
			scr_conlines = 0;
		}

		if (scr_conlines < scr_con_current)
		{
			scr_con_current -= scr_conspeed.value * host_frametime;

			if (scr_conlines > scr_con_current)
			{
				scr_con_current = scr_conlines;
			}
		}
		else if (scr_conlines > scr_con_current)
		{
			scr_con_current += scr_conspeed.value * host_frametime;

			if (scr_conlines < scr_con_current)
			{
				scr_con_current = scr_conlines;
			}
		}

		if (clearconsole++ < vid.numpages)
		{
			scr_copytop = 1;
			Draw_TileClear(0, (int)scr_con_current, vid.width, vid.height - (int)scr_con_current);
			Sbar_Changed();
		}
		else if (clearnotify++ < vid.numpages)
		{
			scr_copytop = 1;
			Draw_TileClear(0, 0, vid.width, console_c.con_notifylines);
		}
		else
		{
			console_c.con_notifylines = 0;
		}
	}

	public void SCR_DrawConsole()
	{
		if (scr_con_current != 0)
		{
			scr_copyeverything = 1;
			console_c.Con_DrawConsole((int)scr_con_current);
			clearconsole = 0;
		}
		else
		{
			if (keys_c.key_dest == keys_c.keydest_t.key_game || keys_c.key_dest == keys_c.keydest_t.key_message)
			{
				console_c.Con_DrawNotify();
			}
		}
	}

	struct pcx_t
	{
		public char manufacturer;
		public char version;
		public char encoding;
		public char bits_per_pixel;
		public ushort xmin, ymin, xmax, ymax;
		public ushort hres, vres;
		public char palette;
		public char reserved;
		public char color_planes;
		public ushort bytes_per_line;
		public ushort palette_type;
		public char filler;
		public char data;
	}

	public void WritePCXfile(string filename, byte* data, int width, int height, int rowbytes, byte* palette)
	{
		int i, j, length;
		pcx_t* pcx;
		byte* pack;

		pcx = (pcx_t*)zone_c.Hunk_TempAlloc(width * height * 2 + 1000);

		if (pcx == default)
		{
			console_c.Con_Printf("SCR_ScreenShot_f: not enough memory\n");
			return;
		}

		pcx->manufacturer = (char)0x0a;
		pcx->version = (char)5;
		pcx->encoding = (char)1;
		pcx->bits_per_pixel = (char)8;
		pcx->xmin = '\0';
		pcx->ymin = '\0';
		pcx->xmax = (char)common_c.LittleShort((short)(width - 1));
		pcx->ymax = (char)common_c.LittleShort((short)(height - 1));
		pcx->hres = (char)common_c.LittleShort((short)width);
		pcx->vres = (char)common_c.LittleShort((short)(height));
		common_c.Q_memset(pcx->palette, 0, 48);
		pcx->color_planes = (char)1;
		pcx->bytes_per_line = (ushort)common_c.LittleShort((short)width);
		pcx->palette_type = (ushort)common_c.LittleShort(2);
		common_c.Q_memset(pcx->filler, 0, 58);

		pack = (byte*)&pcx->data;

		for (i = 0; i < height; i++)
		{
			for (j = 0; j < width; j++)
			{
				if ((*data & 0xc0) != 0xc0)
				{
					*pack++ = *data++;
				}
				else
				{
					*pack++ = 0xc1;
					*pack++ = *data++;
				}
			}

			data += rowbytes - width;
		}

		*pack++ = 0x0c;

		for (i = 0; i < 768; i++)
		{
			*pack++ = *palette++;
		}

		length = (int)(pack - (byte*)pcx);
		common_c.COM_WriteFile(filename, pcx, length);
	}

	public void SCR_ScreenShot_f()
	{
		int i;
		char[] pcxname = new char[80];
		char[] checkname = new char[quakedef_c.MAX_OSPATH];

		common_c.Q_strcpy(pcxname.ToString(), "quake00.pcx");

		for (i = 0; i <= 99; i++)
		{
			pcxname[5] = (char)(i / 10 + '0');
			pcxname[6] = (char)(i % 10 + '0');
			Console.WriteLine($"{common_c.com_gamedir}/{pcxname}");

			if (sys_win_c.Sys_FileTime(checkname.ToString()) == -1)
			{
				break;
			}
		}

		if (i == 100)
		{
			console_c.Con_Printf("SCR_ScreenShot_f: Couldn't create a PCX file\n");
			return;
		}

		D_EnableBackBufferAccess();

		WritePCXfile(pcxname, vid.buffer, vid.width, vid.height, vid.rowbytes, host_basepal);

		D_DisableBackBufferAccess();

		console_c.Con_Printf($"Wrote {pcxname}\n");
	}

	public static void SCR_BeginLoadingPlaque()
	{
		S_StopAllSounds(true);

		if (cl_main_c.cls.state != ca_connected)
		{
			return;
		}

		if (cls.signon != SIGNONS)
		{
			return;
		}

		console_c.Con_ClearNotify();
		scr_centertime_off = 0;
		scr_con_current = 0;

		scr_drawloading = true;
		scr_fullupdate = 0;
		Sbar_Changed();
		SCR_UpdateScreen();
		scr_drawloading = false;

		scr_disabled_for_loading = true;
		scr_disabled_time = (float)quakedef_c.realtime;
		scr_fullupdate = 0;
	}

	public static void SCR_EndLoadingPlaque()
	{
		scr_disabled_for_loading = false;
		scr_fullupdate = 0;
		console_c.Con_ClearNotify();
	}

	public static string scr_notifystring;
	public static bool scr_drawdialog;

	public void SCR_DrawNotifyString()
	{
		char* start;
		int l;
		int j;
		int x, y;

		start = scr_notifystring;

		y = vid.height * 0.35;

		do
		{
			for (l = 0; l < 40; l++)
			{
				if (start[l] == '\n' || start[l] == '\0')
				{
					break;
				}

				x = (vid.width - l * 8) / 2;

				for (j = 0; j < l; j++, x += 8)
				{
					Draw_Character(x, y, start[j]);
				}

				y += 8;

				while (start != null && *start != '\n')
				{
					start++;
				}

				if (start == null)
				{
					break;
				}

				start++;
			}
		} while (true);
	}

	public static int SCR_ModalMessage(char* text)
	{
		if (client_c.cls.state == client_c.cactive_t.ca_dedicated)
		{
			return 1;
		}

		scr_notifystring = text->ToString();

		scr_fullupdate = 0;
		scr_drawdialog = true;
		SCR_UpdateScreen();
		scr_drawdialog = false;

		S_ClearBuffer();

		do
		{
			keys_c.key_count -= 1;
			sys_win_c.Sys_SendKeyEvents();
		} while (keys_c.key_lastpress != 'y' && keys_c.key_lastpress != 'n' && keys_c.key_lastpress != keys_c.K_ESCAPE);

		scr_fullupdate = 0;
		SCR_UpdateScreen();

		return keys_c.key_lastpress == 'y' ? 1 : 0;
	}

	public void SCR_BringDownConsole()
	{
		int i;

		scr_centertime_off = 0;

		for (i = 0; i < 20 && scr_conlines != scr_con_current; i++)
		{
			SCR_UpdateScreen();
		}

		cl.cshifts[0].percent = 0;
		VID_SetPalette(host_basepal);
	}

	public static void SCR_UpdateScreen()
	{
		float oldscr_viewsize = 0.0f;
		float oldlcd_x = 0.0f;
		vid_win_c.vrect_t vrect;

		if (scr_skipupdate || block_drawing)
		{
			return;
		}

		scr_copytop = 0;
		scr_copyeverything = 0;

		if (scr_disabled_for_loading)
		{
			if (quakedef_c.realtime - scr_disabled_time > 60)
			{
				scr_disabled_for_loading = false;
				console_c.Con_Printf("Load failed.\n");
			}
			else
			{
				return;
			}
		}

		if (cls.state == ca_dedicated)
		{
			return;
		}

		if (!scr_initialized || !console_c.con_initialized)
		{
			return;
		}

		if (scr_viewsize.value != oldscr_viewsize)
		{
			oldscr_viewsize = scr_viewsize.value;
			vid.recalc_refdef = 1;
		}

		if (oldfov != scr_fov.value)
		{
			oldfov = scr_fov.value;
			vid.recalc_refdef = 1;
		}

		if (oldlcd_x != lcd_x.value)
		{
			oldlcd_x = lcd_x.value;
			vid.recalc_refdef = 1;
		}

		if (oldscreensize != scr_viewsize.value)
		{
			oldscreensize = scr_viewsize.value;
			vid.recalc_refdef = 1;
		}

		if (vid.recalc_refdef == 1)
		{
			SCR_CalcRefDef();
		}

		D_EnableBackBufferAccess();

		if (scr_fullupdate++ < vid.numpages)
		{
			scr_copyeverything = 1;
			Draw_TileClear(0, 0, vid.width, vid.height);
			Sbar_Changed();
		}

		pconupdate = null;

		SCR_SetUpToDrawConsole();
		SCR_EraseCenterString();

		D_DisableBackBufferAccess();

		VID_LockBuffer();

		V_RenderView();

		VID_UnlockBuffer();

		D_EnableBackBufferAccess();

		if (scr_drawdialog)
		{
			Sbar_Draw();
			Draw_FadeScreen();
			SCR_DrawNotifyString();
			scr_copyeverything = 1;
		}
		else if (scr_drawloading)
		{
			SCR_DrawLoading();
			Sbar_Draw();
		}
		else if (cl.intermission == 1 && keys_c.key_dest == keys_c.keydest_t.key_game)
		{
			Sbar_IntermissionOverlay();
		}
		else if (cl.intermission == 2 && keys_c.key_dest == keys_c.keydest_t.key_game)
		{
			Sbar_FinaleOverlay();
			SCR_CheckDrawCenterString();
		}
		else if (cl.intermission == 3 && keys_c.key_dest == keys_c.keydest_t.key_game)
		{
			SCR_CheckDrawCenterString();
		}
		else
		{
			SCR_DrawRam();
			SCR_DrawNet();
			SCR_DrawTurtle();
			SCR_DrawPause();
			SCR_CheckDrawCenterString();
			Sbar_Draw();
			SCR_DrawConsole();
			M_Draw();
		}

		D_DisableBackBufferAccess();

		if (pconupdate != default)
		{
			D_UpdateRects(pconupdate);
		}

		V_UpdatePalette();

		if (scr_copyeverything == 1)
		{
			vrect.x = 0;
			vrect.y = 0;
			vrect.width = vid.width;
			vrect.height = vid.height;
			vrect.pnext = 0;

			VID_Update(&vrect);
		}
		else if (scr_copytop == 1)
		{
			vrect.x = 0;
			vrect.y = 0;
			vrect.width = vid.width;
			vrect.height = vid.height - sb_lines;
			vrect.pnext = 0;

			VID_Update(&vrect);
		}
		else
		{
			vrect.x = scr_vrect.x;
			vrect.y = scr_vrect.y;
			vrect.width = scr_vrect.width;
			vrect.height = scr_vrect.height;
			vrect.pnext = 0;

			VID_Update(&vrect);
		}
	}

	public void SCR_UpdateWholeScreen()
	{
		scr_fullupdate = 0;
		SCR_UpdateScreen();
	}
}
