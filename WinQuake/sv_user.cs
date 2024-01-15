namespace Quake;

public unsafe class sv_user_c
{
    public static progs_c.edict_t* sv_player;

    public static cvar_c.cvar_t sv_edgefriction = new cvar_c.cvar_t { name = "edgefriction", value = (char)2 };

    public static Vector3 forward, right, up;

    public static Vector3 wishdir;
    public static float wishspeed;

    public static float* angles;
    public static float* origin;
    public static float* velocity;

    public static bool onground;

    public static client_c.usercmd_t cmd;

    public static cvar_c.cvar_t sv_idealpitchscale = new cvar_c.cvar_t { name = "sv_idealpitchscale", value = (char)0.8 };

    public const int MAX_FORWARD = 6;

    public static void SV_SetIdealPitch()
    {
        float angleval, sinval, cosval;
        world_c.trace_t tr;
        Vector3 top, bottom;
        float* z;
        int i, j;
        int step, dir, steps;

        top = bottom = new();

        if (((int)sv_player->v.flags & server_c.FL_ONGROUND) == 0)
        {
            return;
        }

        angleval = sv_player->v.angles[quakedef_c.YAW] * MathF.PI * 2 / 360;
        sinval = MathF.Sin(angleval);
        cosval = MathF.Cos(angleval);

        for (i = 0; i < MAX_FORWARD; i++)
        {
            top[0] = sv_player->v.origin[0] + cosval * (i * 3) * 12;
            top[1] = sv_player->v.origin[1] + cosval * (i * 3) * 12;
            top[2] = sv_player->v.origin[2] + sv_player->v.view_ofs[2];

            bottom[0] = top[0];
            bottom[1] = top[1];
            bottom[2] = top[2] - 160;

            tr = world_c.SV_Move(top, mathlib_c.vec3_origin, mathlib_c.vec3_origin, bottom, 1, sv_player);

            if (tr.allsolid)
            {
                return;
            }

            if (tr.fraction == 1)
            {
                return;
            }

            z[i] = top[2] + tr.fraction * (bottom[2] - top[2]);
        }

        dir = 0;
        steps = 0;

        for (j = 1; j < i; j++)
        {
            step = (int)(z[j] - z[j - 1]);

            if (step > -quakedef_c.ON_EPSILON && step < quakedef_c.ON_EPSILON)
            {
                continue;
            }

            if (dir != null && (step - dir > quakedef_c.ON_EPSILON || step - dir < -quakedef_c.ON_EPSILON))
            {
                return;
            }

            steps++;
            dir = step;
        }

        if (dir == 0)
        {
            sv_player->v.idealpitch = 0;
            return;
        }

        if (steps < 2)
        {
            return;
        }

        sv_player->v.idealpitch = -dir * sv_idealpitchscale.value;
    }

    public static void SV_UserFriction()
    {
        float* vel;
        float speed, newspeed, control;
        Vector3 start, stop;
        float friction;
        world_c.trace_t trace;

        start = stop = new();

        vel = velocity;

        speed = (float)mathlib_c.sqrt(vel[0] * vel[0] + vel[1] * vel[1]);

        if (speed == 0)
        {
            return;
        }

        start[0] = stop[0] = origin[0] + vel[0] / speed * 16;
        start[1] = stop[1] = origin[1] + vel[1] / speed * 16;
        start[2] = origin[2] + sv_player->v.mins[2];
        stop[2] = start[2] - 34;

        trace = world_c.SV_Move(start, mathlib_c.vec3_origin, mathlib_c.vec3_origin, stop, 1, sv_player);

        if (trace.fraction == 1.0f)
        {
            friction = sv_phys_c.sv_friction.value * sv_edgefriction.value;
        }
        else
        {
            friction = sv_phys_c.sv_friction.value;
        }

        control = speed < sv_phys_c.sv_stopspeed.value ? sv_phys_c.sv_stopspeed.value : speed;
        newspeed = speed - (float)host_c.host_frametime * control * friction;

        if (newspeed < 0)
        {
            newspeed = 0;
        }

        newspeed /= speed;

        vel[0] = vel[0] * newspeed;
        vel[1] = vel[1] * newspeed;
        vel[2] = vel[2] * newspeed;
    }

    public static cvar_c.cvar_t sv_maxspeed = new cvar_c.cvar_t { name = "sv_maxspeed", value = (char)320, archive = false, server = true };
    public static cvar_c.cvar_t sv_accelerate = new cvar_c.cvar_t { name = "sv_accelerate", value = (char)10 };

    public static void SV_Accelerate()
    {
        int i;
        float addspeed, accelspeed, currentspeed;

        currentspeed = mathlib_c.DotProduct(velocity, wishdir);
        addspeed = wishspeed - currentspeed;

        if (addspeed <= 0)
        {
            return;
        }

        accelspeed = sv_accelerate.value * (float)host_c.host_frametime * wishspeed;

        if (accelspeed > addspeed)
        {
            accelspeed = addspeed;
        }

        for (i = 0; i < 3; i++)
        {
            velocity[i] += accelspeed * wishdir[i];
        }
    }

    public static void SV_AirAccelerate(Vector3 wishveloc)
    {
        int i;
        float addspeed, wishspd, accelspeed, currentspeed;

        wishspd = mathlib_c.VectorNormalize(wishveloc);

        if (wishspd > 30)
        {
            wishspd = 30;
        }

        currentspeed = mathlib_c.DotProduct(velocity, wishveloc);
        addspeed = wishspd - currentspeed;

        if (addspeed <= 0)
        {
            return;
        }

        accelspeed = sv_accelerate.value * wishspeed * (float)host_c.host_frametime;

        if (accelspeed > addspeed)
        {
            accelspeed = addspeed;
        }

        for (i = 0; i < 3; i++)
        {
            velocity[i] += accelspeed * wishveloc[i];
        }
    }

    public static void DropPunchAngle()
    {
        float len;

        len = mathlib_c.VectorNormalize(sv_player->v.punchangle);

        len -= 10 * (float)host_c.host_frametime;

        if (len < 0)
        {
            len = 0;
        }

        mathlib_c.VectorScale(sv_player->v.punchangle, len, sv_player->v.punchangle);
    }

    public static void SV_WaterMove()
    {
        int i;
        Vector3 wishvel;
        float speed, newspeed, wishspeed, addspeed, accelspeed;

        wishvel = new();

        mathlib_c.AngleVectors(sv_player->v.v_angle, forward, right, up);

        for (i = 0; i < 3; i++)
        {
            wishvel[i] = forward[i] * cmd.forwardmove + right[i] * cmd.sidemove;
        }

        if (cmd.forwardmove == 0 && cmd.sidemove == 0 && cmd.upmove == 0)
        {
            wishvel[2] -= 60;
        }
        else
        {
            wishvel[2] += cmd.upmove;
        }

        wishspeed = mathlib_c.Length(wishvel);

        if (wishspeed > sv_maxspeed.value)
        {
            mathlib_c.VectorScale(wishvel, sv_maxspeed.value / wishspeed, wishvel);
            wishspeed = sv_maxspeed.value;
        }

        wishspeed *= 0.7f;

        speed = mathlib_c.Length(velocity);

        if (speed != 0)
        {
            newspeed = speed - (float)host_c.host_frametime * speed * sv_phys_c.sv_friction.value;

            if (newspeed < 0)
            {
                newspeed = 0;
            }

            mathlib_c.VectorScale(velocity, newspeed / speed, velocity);
        }
        else
        {
            newspeed = 0;
        }

        if (wishspeed == 0)
        {
            return;
        }

        addspeed = wishspeed - newspeed;

        if (addspeed <= 0)
        {
            return;
        }

        mathlib_c.VectorNormalize(wishvel);
        accelspeed = sv_accelerate.value * wishspeed * (float)host_c.host_frametime;

        if (accelspeed > addspeed)
        {
            accelspeed = addspeed;
        }

        for (i = 0; i < 3; i++)
        {
            velocity[i] += accelspeed * wishvel[i];
        }
    }

    public static void SV_WaterJump()
    {
        if (server_c.sv.time > sv_player->v.teleport_time || sv_player->v.waterlevel == 0)
        {
            sv_player->v.flags = (int)sv_player->v.flags & ~server_c.FL_WATERJUMP;
            sv_player->v.teleport_time = 0;
        }

        sv_player->v.velocity[0] = sv_player->v.movedir[0];
        sv_player->v.velocity[1] = sv_player->v.movedir[1];
    }

    public static void SV_AirMove()
    {
        int i;
        Vector3 wishvel;
        float fmove, smove;

        mathlib_c.AngleVectors(sv_player->v.angles, forward, right, up);

        fmove = cmd.forwardmove;
        smove = cmd.sidemove;

        if (server_c.sv.time < sv_player->v.teleport_time && fmove < 0)
        {
            fmove = 0;
        }

        for (i = 0; i < 3; i++)
        {
            wishvel[i] = forward[i] * fmove + right[i] * smove;
        }

        if ((int)sv_player->v.movetype != server_c.MOVETYPE_WALK)
        {
            wishvel[2] = cmd.upmove;
        }
        else
        {
            wishvel[2] = 0;
        }

        mathlib_c.VectorCopy(wishvel, wishdir);
        wishspeed = mathlib_c.VectorNormalize(wishdir);

        if (wishspeed > sv_maxspeed.value)
        {
            mathlib_c.VectorScale(wishvel, sv_maxspeed.value / wishspeed, wishvel);
            wishspeed = sv_maxspeed.value;
        }

        if (sv_player->v.movetype == server_c.MOVETYPE_NOCLIP)
        {
            mathlib_c.VectorCopy(wishvel, velocity);
        }
        else if (onground)
        {
            SV_UserFriction();
            SV_Accelerate();
        }
        else
        {
            SV_AirAccelerate(wishvel);
        }
    }

    public static void SV_ClientThink()
    {
        Vector3 v_angle = new();

        if (sv_player->v.movetype == server_c.MOVETYPE_NONE)
        {
            return;
        }

        onground = ((int)sv_player->v.flags & server_c.FL_ONGROUND) == 1 ? true : false;

        origin = mathlib_c.VecToFloatPtr(sv_player->v.origin);
        velocity = mathlib_c.VecToFloatPtr(sv_player->v.velocity);

        DropPunchAngle();

        if (sv_player->v.health <= 0)
        {
            return;
        }

        cmd = host_c.host_client->cmd;
        angles = mathlib_c.VecToFloatPtr(sv_player->v.angles);

        mathlib_c.VectorAdd(sv_player->v.v_angle, sv_player->v.punchangle, v_angle);
        angles[quakedef_c.ROLL] = view_c.V_CalcRoll(sv_player->v.angles, sv_player->v.velocity) * 4;

        if (sv_player->v.fixangle == 0)
        {
            angles[quakedef_c.PITCH] = -v_angle[quakedef_c.PITCH] / 3;
            angles[quakedef_c.YAW] = v_angle[quakedef_c.YAW];
        }

        if (((int)sv_player->v.flags & server_c.FL_WATERJUMP) != 0)
        {
            SV_WaterJump();
            return;
        }

        if ((sv_player->v.waterlevel >= 2) && (sv_player->v.movetype != server_c.MOVETYPE_NOCLIP))
        {
            SV_WaterMove();
            return;
        }

        SV_AirMove();
    }

    public static void SV_ReadClientMove(client_c.usercmd_t* move)
    {
        int i;
        Vector3 angle = new();
        int bits;

        host_c.host_client->ping_times[host_c.host_client->num_pings % NUM_PING_TIMES] = server_c.sv.time - common_c.MSG_ReadFloat();
        host_c.host_client->num_pings++;

        for (i = 0; i < 3; i++)
        {
            angle[i] = common_c.MSG_ReadAngle();
        }

        mathlib_c.VectorCopy(angle, host_c.host_client->edict->v.v_angle);

        move->forwardmove = common_c.MSG_ReadShort();
        move->sidemove = common_c.MSG_ReadShort();
        move->upmove = common_c.MSG_ReadShort();

        bits = common_c.MSG_ReadByte();
        host_c.host_client->edict->v.button0 = bits & 1;
        host_c.host_client->edict->v.button2 = (bits & 2) >> 1;

        i = common_c.MSG_ReadByte();

        if (i != 0)
        {
            host_c.host_client->edict->v.impulse = i;
        }

#if QUAKE2
        host_c.host_client->edict->v.light_level = common_c.MSG_ReadByte();
#endif
    }

    public static bool SV_ReadClientMessage()
    {
        int ret;
        int cmd;
        char* s;

        do
        {
        message:
            ret = net_main_c.NET_GetMessage(host_c.host_client->netconnection);

            if (ret == -1)
            {
                sys_win_c.Sys_Printf("SV_ReadClientMessage: NET_GetMessage failed\n");
                return false;
            }

            if (ret == 0)
            {
                return true;
            }

            common_c.MSG_BeginReading();

            while (true)
            {
                if (!host_c.host_client->active)
                {
                    return false;
                }

                if (common_c.msg_badread)
                {
                    sys_win_c.Sys_Printf("SV_ReadClientMessage: badread\n");
                    return false;
                }

                cmd = common_c.MSG_ReadChar();

                switch (cmd)
                {
                    case -1:
                        goto nextmsg;
                        break;

                    default:
                        sys_win_c.Sys_Printf("SV_ReadClientMessage: unknown command char\n");
                        return false;

                    case protocol_c.clc_nop:
                        break;

                    case protocol_c.clc_stringcmd:
                        s = common_c.MSG_ReadString();

                        if (host_c.host_client->privileged)
                        {
                            ret = 2;
                        }
                        else
                        {
                            ret = 0;
                        }

                        if (common_c.Q_strncasecmp(s, "status", 6) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "god", 3) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "notarget", 8) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "fly", 3) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "name", 4) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "noclip", 6) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "say", 3) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "say_team", 8) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "tell", 4) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "color", 5) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "kill", 4) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "pause", 5) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "spawn", 5) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "begin", 5) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "prespawn", 8) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "kick", 4) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "ping", 4) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "give", 4) == 0)
                        {
                            ret = 1;
                        }
                        else if (common_c.Q_strncasecmp(s, "ban", 3) == 0)
                        {
                            ret = 1;
                        }

                        if (ret == 2)
                        {
                            cmd_c.Cbuf_AddText(s);
                        }
                        else if (ret == 1)
                        {
                            cmd_c.Cmd_ExecuteString(s, cmd_c.cmd_source_t.src_client);
                        }
                        else
                        {
                            console_c.Con_DPrintf($"{*host_c.host_client->name} tried to {*s}\n");
                        }
                        break;

                    case protocol_c.clc_disconnect:
                        return false;

                    case protocol_c.clc_move:
                        SV_ReadClientMove(&host_c.host_client->cmd);
                        break;
                }
            }
        } while (ret == 1);

        return true;
    }

    public static void SV_RunClients()
    {
        int i;

        for (i = 0, host_c.host_client = server_c.svs.clients; i < server_c.svs.maxclients; i++, host_c.host_client++)
        {
            if (!host_c.host_client->active)
            {
                continue;
            }

            sv_player = host_c.host_client->edict;

            if (!SV_ReadClientMessage())
            {
                sv_main_c.SV_DropClient(false);
                continue;
            }

            if (!host_c.host_client->spawned)
            {
                common_c.Q_memset(host_c.host_client->cmd, 0, host_c.host_client->cmd);
                continue;
            }

            if (!server_c.sv.paused && (server_c.svs.maxclients > 1 || keys_c.key_dest == keys_c.keydest_t.key_game))
            {
                SV_ClientThink();
            }
        }
    }
}