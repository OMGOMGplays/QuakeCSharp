using System.Security.Cryptography.X509Certificates;

namespace Quake;

public unsafe class cl_input_c
{
    public cvar_c.cvar_t cl_nodelta = new cvar_c.cvar_t { name = "cl_nodelta", value = (char)0 };

    public client_c.kbutton_t in_mlook, in_klook;
    public client_c.kbutton_t in_left, in_right, in_forward, in_back;
    public client_c.kbutton_t in_lookup, in_lookdown, in_moveleft, in_moveright;
    public client_c.kbutton_t in_strafe, in_speed, in_use, in_jump, in_attack;
    public client_c.kbutton_t in_up, in_down;

    public int in_impulse;

    public void KeyDown(client_c.kbutton_t b)
    {
        int k;
        char* c;

        c = cmd_c.Cmd_Argv(1);

        if (c[0] != 0)
        {
            k = common_c.Q_atoi(c->ToString());
        }
        else
        {
            k = -1;
        }

        if (k == b.down[0] || k == b.down[1])
        {
            return;
        }

        if (b.down[0] == 0)
        {
            b.down[0] = k;
        }
        else if (b.down[1] == 0)
        {
            b.down[1] = k;
        }
        else
        {
            console_c.Con_Printf("Three keys down for a button!\n");
            return;
        }

        if ((b.state & 1) != 0)
        {
            return;
        }

        b.state |= 1 + 2;
    }

    public void KeyUp(client_c.kbutton_t b)
    {
        int k;
        char* c;

        c = cmd_c.Cmd_Argv(1);

        if (c[0] != 0)
        {
            k = common_c.Q_atoi(c);
        }
        else
        {
            b.down[0] = b.down[1] = 0;
            b.state = 4;
            return;
        }

        if (b.down[0] == k)
        {
            b.down[0] = 0;
        }
        else if (b.down[1] == k)
        {
            b.down[1] = 0;
        }
        else
        {
            return;
        }

        if (b.down[0] != 0 || b.down[1] != 0)
        {
            return;
        }

        if ((b.state & 1) == 0)
        {
            return;
        }

        b.state &= ~1;
        b.state |= 4;
    }

    public void IN_KLookDown() { KeyDown(in_klook); }
    public void IN_KLookUp() { KeyUp(in_klook); }
    public void IN_MLookDown() { KeyDown(in_mlook); }
    public void IN_MLookUp()
    {
        KeyUp(in_mlook);

        if ((in_mlook.state & 1) == 0 && cl_main_c.lookspring.value != 0)
        {
            V_StartPitchDrift();
        }
    }
    public void IN_UpDown() { KeyDown(in_up); }
    public void IN_UpUp() { KeyUp(in_up); }
    public void IN_DownDown() { KeyDown(in_down); }
    public void IN_DownUp() { KeyUp(in_down); }
    public void IN_LeftDown() { KeyDown(in_left); }
    public void IN_LeftUp() { KeyUp(in_left); }
    public void IN_RightDown() { KeyDown(in_right); }
    public void IN_RightUp() { KeyUp(in_right); }
    public void IN_ForwardDown() { KeyDown(in_forward); }
    public void IN_ForwardUp() { KeyUp(in_forward); }
    public void IN_BackDown() { KeyDown(in_back); }
    public void IN_BackUp() { KeyUp(in_back); }
    public void IN_LookupDown() { KeyDown(in_lookup); }
    public void IN_LookupUp() { KeyUp(in_lookup); }
    public void IN_LookdownDown() { KeyDown(in_lookdown); }
    public void IN_LookdownUp() { KeyUp(in_lookdown); }
    public void IN_MoveleftDown() { KeyDown(in_moveleft); }
    public void IN_MoveleftUp() { KeyUp(in_moveleft); }
    public void IN_MoverightDown() { KeyDown(in_moveright); }
    public void IN_MoverightUp() { KeyUp(in_moveright); }

    public void IN_SpeedDown() { KeyDown(in_speed); }
    public void IN_SpeedUp() { KeyUp(in_speed); }
    public void IN_StrafeDown() { KeyDown(in_strafe); }
    public void IN_StrafeUp() { KeyUp(in_strafe); }

    public void IN_AttackDown() { KeyDown(in_attack); }
    public void IN_AttackUp() { KeyUp(in_attack); }

    public void IN_UseDown() { KeyDown(in_use); }
    public void IN_UseUp() { KeyUp(in_use); }
    public void IN_JumpDown() { KeyDown(in_jump); }
    public void IN_JumpUp() { KeyUp(in_jump); }

    public void IN_Impulse() { in_impulse = common_c.Q_atoi(cmd_c.Cmd_Argv(1)->ToString()); }

    public float CL_KeyState(client_c.kbutton_t key)
    {
        float val;
        bool impulsedown, impulseup, down;

        impulsedown = (key.state & 2) == 0 ? false : true;
        impulseup = (key.state & 4) == 0 ? false : true;
        down = (key.state & 1) == 0 ? false : true;
        val = 0;

        if (impulsedown && !impulseup)
        {
            if (down)
            {
                val = 0.5f;
            }
            else
            {
                val = 0.0f;
            }
        }

        if (impulseup && !impulsedown)
        {
            if (down)
            {
                val = 0.0f;
            }
            else
            {
                val = 0.0f;
            }
        }

        if (!impulsedown && !impulseup)
        {
            if (down)
            {
                val = 1.0f;
            }
            else
            {
                val = 0.0f;
            }
        }

        if (impulsedown && impulseup)
        {
            if (down)
            {
                val = 0.75f;
            }
            else
            {
                val = 0.25f;
            }
        }

        key.state &= 1;

        return val;
    }

    public cvar_c.cvar_t cl_upspeed = new cvar_c.cvar_t { name = "cl_upspeed", value = (char)200 };
    public cvar_c.cvar_t cl_forwardspeed = new cvar_c.cvar_t { name = "cl_forwardspeed", value = (char)200, archive = true };
    public cvar_c.cvar_t cl_backspeed = new cvar_c.cvar_t { name = "cl_backspeed", value = (char)200, archive = true };
    public cvar_c.cvar_t cl_sidespeed = new cvar_c.cvar_t { name = "cl_sidespeed", value = (char)350 };

    public cvar_c.cvar_t cl_movespeedkey = new cvar_c.cvar_t { name = "cl_movespeedkey", value = (char)2.0f };

    public cvar_c.cvar_t cl_yawspeed = new cvar_c.cvar_t { name = "cl_yawspeed", value = (char)140 };
    public cvar_c.cvar_t cl_pitchspeed = new cvar_c.cvar_t { name = "cl_pitchspeed", value = (char)150 };

    public cvar_c.cvar_t cl_anglespeedkey = new cvar_c.cvar_t { name = "cl_anglespeedkey", value = (char)1.5f };

    public void CL_AdjustAngles()
    {
        float speed;
        float up, down;

        if ((in_speed.state & 1) != 0)
        {
            speed = host_frametime * cl_anglespeedkey.value;
        }
        else
        {
            speed = host_frametime;
        }

        if ((in_strafe.state & 1) == 0)
        {
            cl_main_c.cl.viewangles[quakedef_c.YAW] -= speed * cl_yawspeed.value * CL_KeyState(in_right);
            cl_main_c.cl.viewangles[quakedef_c.YAW] += speed * cl_yawspeed.value * CL_KeyState(in_left);
            cl_main_c.cl.viewangles[quakedef_c.YAW] = mathlib_c.anglemod(cl_main_c.cl.viewangles[quakedef_c.YAW]);
        }

        if ((in_klook.state & 1) != 0)
        {
            V_StopPitchDrift();
            cl_main_c.cl.viewangles[quakedef_c.PITCH] -= speed * cl_pitchspeed.value * CL_KeyState(in_forward);
            cl_main_c.cl.viewangles[quakedef_c.PITCH] += speed * cl_pitchspeed.value * CL_KeyState(in_back);
        }

        up = CL_KeyState(in_lookup);
        down = CL_KeyState(in_lookdown);

        cl_main_c.cl.viewangles[quakedef_c.PITCH] -= speed * cl_pitchspeed.value * up;
        cl_main_c.cl.viewangles[quakedef_c.PITCH] += speed * cl_pitchspeed.value * down;

        if (up != 0 || down != 0)
        {
            V_StopPitchDrift();
        }

        if (cl_main_c.cl.viewangles[quakedef_c.PITCH] > 80)
        {
            cl_main_c.cl.viewangles[quakedef_c.PITCH] = 80;
        }

        if (cl_main_c.cl.viewangles[quakedef_c.PITCH] < -70)
        {
            cl_main_c.cl.viewangles[quakedef_c.PITCH] = -70;
        }

        if (cl_main_c.cl.viewangles[quakedef_c.ROLL] > 50)
        {
            cl_main_c.cl.viewangles[quakedef_c.ROLL] = 50;
        }

        if (cl_main_c.cl.viewangles[quakedef_c.ROLL] < -50)
        {
            cl_main_c.cl.viewangles[quakedef_c.ROLL] = -50;
        }
    }

    public void CL_BaseMove(client_c.usercmd_t cmd)
    {
        CL_AdjustAngles();

        common_c.Q_memset(cmd, 0, sizeof(client_c.usercmd_t));

        //VectorCopy(cl_main_c.cl.viewangles, cmd->angles);

        if ((in_strafe.state & 1) != 0)
        {
            cmd.sidemove += cl_sidespeed.value * CL_KeyState(in_right);
            cmd.sidemove -= cl_sidespeed.value * CL_KeyState(in_left);
        }

        cmd.sidemove += cl_sidespeed.value * CL_KeyState(in_moveright);
        cmd.sidemove -= cl_sidespeed.value * CL_KeyState(in_moveleft);

        cmd.upmove += cl_upspeed.value * CL_KeyState(in_up);
        cmd.upmove -= cl_upspeed.value * CL_KeyState(in_down);

        if ((in_klook.state & 1) == 0)
        {
            cmd.forwardmove += cl_forwardspeed.value * CL_KeyState(in_forward);
            cmd.forwardmove -= cl_backspeed.value * CL_KeyState(in_back);
        }

        if ((in_speed.state & 1) != 0)
        {
            cmd.forwardmove *= cl_movespeedkey.value;
            cmd.sidemove *= cl_movespeedkey.value;
            cmd.upmove *= cl_movespeedkey.value;
        }
    }

    public int MakeChar(int i)
    {
        i &= ~3;

        if (i < -127 * 4)
        {
            i = -127 * 4;
        }

        if (i > 127 * 4)
        {
            i = 127 * 4;
        }

        return i;
    }

    public void CL_FinishMove(client_c.usercmd_t cmd)
    {
        int i;
        int ms;

        if (cl_main_c.cl.movemessages <= 2)
        {
            return;
        }

        if ((in_attack.state & 3) != 0)
        {
            cmd.buttons |= 1;
        }

        in_attack.state &= ~2;

        if ((in_jump.state & 3) != 0)
        {
            cmd.buttons |= 2;
        }

        in_jump.state &= ~2;

        ms = host_frametime * 1000;

        if (ms > 250)
        {
            ms = 100;
        }

        cmd.msecs = ms;

        // VectorCopy(cl_main_c.cl.viewangles, cmd.angles);

        cmd.impulse = in_impulse;
        in_impulse = 0;

        cmd.forwardmove = MakeChar((int)cmd.forwardmove);
        cmd.sidemove = MakeChar((int)cmd.sidemove);
        cmd.upmove = MakeChar((int)cmd.upmove);

        for (i = 0; i < 3; i++)
        {
            cmd.angles[i] = ((int)(cmd.angles[i] * 65536.0f / 360) & 65535) * (360 / 65535.0f);
        }
    }

    public void CL_SendCmd()
    {
        common_c.sizebuf_t buf = default;
        byte[] data = new byte[128];
        int i;
        client_c.usercmd_t cmd, oldcmd;
        int checksumIndex;
        int lost;
        int seq_hash = 0;

        if (cl_main_c.cls.demoplayback)
        {
            return;
        }

        i = cl_main_c.cls.netchan.outgoing_sequence;

        CL_BaseMove(cmd);

        IN_Move(cmd);

        if (cl_main_c.cl.spectator)
        {
            Cam_Track(cmd);
        }

        CL_FinishMove();

        Cam_FinishMove();

        buf.maxsize = 128;
        buf.cursize = 0;
        buf.data = data;

        common_c.MSG_WriteByte(buf, clc_move);

        checksumIndex = buf.cursize;
        common_c.MSG_WriteByte(buf, 0);

        lost = CL_CalcNet();
        common_c.MSG_WriteByte(buf, lost);

        i = (cl_main_c.cls.netchan.outgoing_sequence - 2) & UPDATE_MASK;
        cmd = cl_main_c.cl.frames[i].cmd;
        common_c.MSG_WriteDeltaUsercmd(buf, nullcmd, cmd);
        oldcmd = cmd;

        i = (cl_main_c.cls.netchan.outgoing_sequence - 1) & UPDATE_MASK;
        cmd = cl_main_c.cl.frames[i].cmd;
        common_c.MSG_WriteDeltaUsercmd(buf, oldcmd, cmd);
        oldcmd = cmd;

        i = (cl_main_c.cls.netchan.outgoing_sequence) & UPDATE_MASK;
        cmd = cl_main_c.cl.frames[i].cmd;
        common_c.MSG_WriteDeltaUsercmd(buf, oldcmd, cmd);

        buf.data[checksumIndex] = common_c.COM_BlockSequenceCRByte(buf.data + checksumIndex + 1, buf.cursize - checksumIndex - 1, seq_hash);

        if (cl_main_c.cls.netchan.outgoing_sequence - cl_main_c.cl.validsequence >= UPDATE_BACKUP - 1)
        {
            cl_main_c.cl.validsequence = 0;
        }

        if (cl_main_c.cl.validsequence != 0 && cl_nodelta.value == 0 && cl_main_c.cls.state == ca_active && !cl_main_c.cls.demorecording)
        {
            cl_main_c.cl.frames[cl_main_c.cls.netchan.outgoing_sequence & UPDATE_MASK].delta_sequence = cl_main_c.cl.validsequence;
            common_c.MSG_WriteByte(buf, clc_delta);
            common_c.MSG_WriteByte(buf, cl_main_c.cl.validsequence & 255);
        }
        else
        {
            cl_main_c.cl.frames[cl_main_c.cls.netchan.outgoing_sequence & UPDATE_MASK].delta_sequence = -1;
        }

        if (cl_main_c.cls.demorecording)
        {
            CL_WriteDemoCmd(cmd);
        }

        Netchan_Transmit(cl_main_c.cls.netchan, buf.cursize, buf.data);
    }

    public void CL_InitInput()
    {
        cmd_c.Cmd_AddCommand("+moveup", IN_UpDown);
        cmd_c.Cmd_AddCommand("-moveup", IN_UpUp);
        cmd_c.Cmd_AddCommand("+movedown", IN_DownDown);
        cmd_c.Cmd_AddCommand("-movedown", IN_DownUp);
        cmd_c.Cmd_AddCommand("+left", IN_LeftDown);
        cmd_c.Cmd_AddCommand("-left", IN_LeftUp);
        cmd_c.Cmd_AddCommand("+right", IN_RightDown);
        cmd_c.Cmd_AddCommand("-right", IN_RightUp);
        cmd_c.Cmd_AddCommand("+forward", IN_ForwardDown);
        cmd_c.Cmd_AddCommand("-forward", IN_ForwardUp);
        cmd_c.Cmd_AddCommand("+back", IN_BackDown);
        cmd_c.Cmd_AddCommand("-back", IN_BackUp);
        cmd_c.Cmd_AddCommand("+lookup", IN_LookupDown);
        cmd_c.Cmd_AddCommand("-lookup", IN_LookupUp);
        cmd_c.Cmd_AddCommand("+lookdown", IN_LookdownDown);
        cmd_c.Cmd_AddCommand("-lookdown", IN_LookdownUp);
        cmd_c.Cmd_AddCommand("+strafe", IN_StrafeDown);
        cmd_c.Cmd_AddCommand("-strafe", IN_StrafeUp);
        cmd_c.Cmd_AddCommand("+moveleft", IN_MoveleftDown);
        cmd_c.Cmd_AddCommand("-moveleft", IN_MoveleftUp);
        cmd_c.Cmd_AddCommand("+moveright", IN_MoverightDown);
        cmd_c.Cmd_AddCommand("-moveright", IN_MoverightUp);
        cmd_c.Cmd_AddCommand("+speed", IN_SpeedDown);
        cmd_c.Cmd_AddCommand("-speed", IN_SpeedUp);
        cmd_c.Cmd_AddCommand("+attack", IN_AttackDown);
        cmd_c.Cmd_AddCommand("-attack", IN_AttackUp);
        cmd_c.Cmd_AddCommand("+use", IN_UseDown);
        cmd_c.Cmd_AddCommand("-use", IN_UseUp);
        cmd_c.Cmd_AddCommand("+jump", IN_JumpDown);
        cmd_c.Cmd_AddCommand("-jump", IN_JumpUp);
        cmd_c.Cmd_AddCommand("impulse", IN_Impulse);
        cmd_c.Cmd_AddCommand("+klook", IN_KLookDown);
        cmd_c.Cmd_AddCommand("-klook", IN_KLookUp);
        cmd_c.Cmd_AddCommand("+mlook", IN_MLookDown);
        cmd_c.Cmd_AddCommand("-mlook", IN_MLookUp);

        cvar_c.Cvar_RegisterVariable(cl_nodelta);
    }

    public void CL_ClearStates()
    {

    }
}