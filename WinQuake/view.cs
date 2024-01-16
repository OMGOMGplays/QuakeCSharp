using lib.libc;

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

    public static cvar_c.cvar_t gl_cshiftpercent = new cvar_c.cvar_t { name = "gl_cshiftpercent", value = (char)100 };

    public static float v_dmg_time, v_dmg_roll, v_dmg_pitch;

    public static int in_forward, in_forward2, in_back;

    public static Vector3 forward, right, up;

    public static float V_CalcRoll(Vector3 angles, Vector3 velocity)
    {
        float sign;
        float side;
        float value;

        mathlib_c.AngleVectors(angles, forward, right, up);
        side = mathlib_c.DotProduct(velocity, right);
        sign = side < 0 ? -1 : 1;
        side = MathF.Abs(side);

        value = cl_rollangle.value;

        if (side < cl_rollspeed.value)
        {
            side = side * value / cl_rollspeed.value;
        }
        else
        {
            side = value;
        }

        return side * sign;
    }

    public static float V_CalcBob()
    {
        float bob;
        float cycle;

        cycle = (float)cl_main_c.cl.time - (int)(cl_main_c.cl.time / cl_bobcycle.value) * cl_bobcycle.value;
        cycle /= cl_bobcycle.value;

        if (cycle < cl_bobup.value)
        {
            cycle = MathF.PI * cycle / cl_bobup.value;
        }
        else
        {
            cycle = MathF.PI + MathF.PI * (cycle - cl_bobup.value) / (1.0f - cl_bobup.value);
        }

        bob = (float)mathlib_c.sqrt(cl_main_c.cl.velocity[0] * cl_main_c.cl.velocity[0] + cl_main_c.cl.velocity[1] * cl_main_c.cl.velocity[1]) * cl_bob.value;
        bob = bob * 0.3f + bob * 0.7f * MathF.Sin(cycle);

        if (bob > 4)
        {
            bob = 4;
        }
        else if (bob < -7)
        {
            bob = -7;
        }

        return bob;
    }

    public static cvar_c.cvar_t v_centermove = new cvar_c.cvar_t { name = "v_centermove", value = (char)0.15f };
    public static cvar_c.cvar_t v_centerspeed = new cvar_c.cvar_t { name = "v_centerspeed", value = (char)500 };

    public static void V_StartPitchDrift()
    {
        if (cl_main_c.cl.laststop == cl_main_c.cl.time)
        {
            return;
        }

        if (cl_main_c.cl.nodrift || cl_main_c.cl.pitchvel == 0)
        {
            cl_main_c.cl.pitchvel = v_centerspeed.value;
            cl_main_c.cl.nodrift = false;
            cl_main_c.cl.driftmove = 0;
        }
    }

    public static void V_StopPitchDrift()
    {
        cl_main_c.cl.laststop = cl_main_c.cl.time;
        cl_main_c.cl.nodrift = true;
        cl_main_c.cl.pitchvel = 0;
    }

    public static void V_DriftPitch()
    {
        float delta, move;

        if (host_cmd_c.noclip_anglehack || !cl_main_c.cl.onground || cl_main_c.cls.demoplayback)
        {
            cl_main_c.cl.driftmove = 0;
            cl_main_c.cl.pitchvel = 0;
            return;
        }

        if (cl_main_c.cl.nodrift)
        {
            if (MathF.Abs(cl_main_c.cl.cmd.forwardmove) < cl_input_c.cl_forwardspeed.value)
            {
                cl_main_c.cl.driftmove = 0;
            }
            else
            {
                cl_main_c.cl.driftmove += (float)host_c.host_frametime;
            }

            if (cl_main_c.cl.driftmove > v_centermove.value)
            {
                V_StartPitchDrift();
            }

            return;
        }

        delta = cl_main_c.cl.idealpitch - cl_main_c.cl.viewangles[quakedef_c.PITCH];

        if (delta == 0)
        {
            cl_main_c.cl.pitchvel = 0;
            return;
        }

        move = (float)host_c.host_frametime * cl_main_c.cl.pitchvel;
        cl_main_c.cl.pitchvel += (float)host_c.host_frametime * v_centerspeed.value;

        if (delta > 0)
        {
            if (move > delta)
            {
                cl_main_c.cl.pitchvel = 0;
                move = delta;
            }

            cl_main_c.cl.viewangles[quakedef_c.PITCH] += move;
        }
        else if (delta < 0)
        {
            if (move > -delta)
            {
                cl_main_c.cl.pitchvel = 0;
                move = -delta;
            }

            cl_main_c.cl.viewangles[quakedef_c.PITCH] -= move;
        }
    }

    public static client_c.cshift_t cshift_empty = new client_c.cshift_t { destcolor = new int[130, 80, 50], percent = 0 };
    public static client_c.cshift_t cshift_water = new client_c.cshift_t { destcolor = new int[130, 80, 50], percent = 128 };
    public static client_c.cshift_t cshift_slime = new client_c.cshift_t { destcolor = new int[0, 25, 5], percent = 150 };
    public static client_c.cshift_t cshift_lava = new client_c.cshift_t { destcolor = new int[255, 80, 0], percent = 150 };

    public static cvar_c.cvar_t v_gamma = new cvar_c.cvar_t { name = "gamma", value = (char)1, archive = true };

    public static byte* gammatable;

#if GLQUAKE
    public static byte[][] ramps;
    public static float[] v_blend;
#endif

    public static void BuildGammaTable(float g)
    {
        int i, inf;

        if (g == 1.0f)
        {
            for (i = 0; i < 256; i++)
            {
                gammatable[i] = (byte)i;
            }

            return;
        }

        for (i = 0; i < 256; i++)
        {
            inf = (int)(255 * MathF.Pow((i + 0.5f) / 255.5f, g) + 0.5f);

            if (inf < 0)
            {
                inf = 0;
            }

            if (inf > 255)
            {
                inf = 255;
            }

            gammatable[i] = (byte)inf;
        }
    }

    public static bool V_CheckGamma()
    {
        float oldgammavalue = 0;

        if (v_gamma.value == oldgammavalue)
        {
            return false;
        }

        oldgammavalue = v_gamma.value;

        BuildGammaTable(v_gamma.value);
        vid_c.vid.recalc_refdef = 1;

        return true;
    }

    public static void V_ParseDamage()
    {
        int armor, blood;
        Vector3 from;
        int i;
        Vector3 forward, right, up;
        render_c.entity_t* ent;
        float side;
        float count;

        from = forward = right = up = new();

        armor = common_c.MSG_ReadByte();
        blood = common_c.MSG_ReadByte();

        for (i = 0; i < 3; i++)
        {
            from[i] = common_c.MSG_ReadCoord();
        }

        count = blood * 0.5f + armor * 0.5f;

        if (count < 10)
        {
            count = 10;
        }

        cl_main_c.cl.faceanimtime = (float)cl_main_c.cl.time + 0.2f;

        cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].percent += 3 * (int)count;

        if (cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].percent < 0)
        {
            cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].percent = 0;
        }

        if (cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].percent > 150)
        {
            cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].percent = 150;
        }

        if (armor > blood)
        {
            cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].destcolor[0] = 200;
            cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].destcolor[1] = 100;
            cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].destcolor[2] = 100;
        }
        else if (armor != 0)
        {
            cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].destcolor[0] = 220;
            cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].destcolor[1] = 50;
            cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].destcolor[2] = 50;
        }
        else
        {
            cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].destcolor[0] = 255;
            cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].destcolor[1] = 0;
            cl_main_c.cl.cshifts[client_c.CSHIFT_DAMAGE].destcolor[2] = 0;
        }

        ent = &cl_main_c.cl_entities[cl_main_c.cl.viewentity];

        mathlib_c.VectorSubtract(from, ent->origin, from);
        mathlib_c.VectorNormalize(from);

        mathlib_c.AngleVectors(ent->angles, forward, right, up);

        side = mathlib_c.DotProduct(from, right);
        v_dmg_roll = count * side * v_kickroll.value;

        side = mathlib_c.DotProduct(from, forward);
        v_dmg_pitch = count * side * v_kickpitch.value;

        v_dmg_time = v_kicktime.value;
    }

    public static void V_cshift_f()
    {
        cshift_empty.destcolor[0] = atoi_c.atoi(cmd_c.Cmd_Argv(1));
        cshift_empty.destcolor[1] = atoi_c.atoi(cmd_c.Cmd_Argv(2));
        cshift_empty.destcolor[2] = atoi_c.atoi(cmd_c.Cmd_Argv(3));
        cshift_empty.percent = atoi_c.atoi(cmd_c.Cmd_Argv(4));
    }

    public static void V_BonusFlash_f()
    {
        cl_main_c.cl.cshifts[client_c.CSHIFT_BONUS].destcolor[0] = 215;
        cl_main_c.cl.cshifts[client_c.CSHIFT_BONUS].destcolor[1] = 186;
        cl_main_c.cl.cshifts[client_c.CSHIFT_BONUS].destcolor[2] = 69;
        cl_main_c.cl.cshifts[client_c.CSHIFT_BONUS].percent = 50;
    }

    public static void V_SetContentsColor(int contents)
    {
        switch (contents)
        {
            case bspfile_c.CONTENTS_EMPTY:
            case bspfile_c.CONTENTS_SOLID:
                cl_main_c.cl.cshifts[client_c.CSHIFT_CONTENTS] = cshift_empty;
                break;

            case bspfile_c.CONTENTS_LAVA:
                cl_main_c.cl.cshifts[client_c.CSHIFT_CONTENTS] = cshift_lava;
                break;

            case bspfile_c.CONTENTS_SLIME:
                cl_main_c.cl.cshifts[client_c.CSHIFT_CONTENTS] = cshift_slime;
                break;

            default:
                cl_main_c.cl.cshifts[client_c.CSHIFT_CONTENTS] = cshift_water;
                break;
        }
    }


}