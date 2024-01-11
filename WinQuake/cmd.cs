namespace Quake;

public unsafe class cmd_c
{
    public static int MAX_ALIAS_NAME = 32;

    public struct cmdalias_t
    {
        public cmdalias_t* next;
        public char name;
        public char* value;
    }

    public static cmdalias_t* cmd_alias;

    public int trashtest;
    public int* trashspot;

    public static bool cmd_wait;

    public static void Cmd_Wait_f()
    {
        cmd_wait = true;
    }

    public static common_c.sizebuf_t cmd_text;

    public static void Cbuf_Init()
    {
        common_c.SZ_Alloc(cmd_text, 8192);
    }

    public static void Cbuf_AddText(char* text)
    {
        int l;

        l = common_c.Q_strlen(text->ToString());

        if (cmd_text.cursize + 1 >= cmd_text.maxsize)
        {
            console_c.Con_Printf("Cbuf_AddText: overflow\n");
            return;
        }

        common_c.SZ_Write(cmd_text, (int)text, common_c.Q_strlen(text->ToString()));
    }

    public static void Cbuf_InsertText(char* text)
    {
        char* temp;
        int templen;

        templen = cmd_text.cursize;

        if (templen != 0)
        {
            temp = (char*)zone_c.Z_Malloc(templen);
            common_c.Q_memcpy(*temp, *cmd_text.data, templen);
            common_c.SZ_Clear(cmd_text);
        }
        else
        {
            temp = null;
        }

        Cbuf_AddText(text);

        if (templen != 0)
        {
            common_c.SZ_Write(cmd_text, (int)temp, templen);
            zone_c.Z_Free(temp);
        }
    }

    public static void Cbuf_Execute()
    {
        int i;
        char* text;
        char* line = null;
        int quotes;

        while (cmd_text.cursize != 0)
        {
            text = (char*)cmd_text.data;

            quotes = 0;

            for (i = 0; i < cmd_text.cursize; i++)
            {
                if (text[i] == '"')
                {
                    quotes++;
                }

                if ((quotes & 1) == 0 && text[i] == ';')
                {
                    break;
                }

                if (text[i] == '\n')
                {
                    break;
                }
            }

            common_c.Q_memcpy(*line, *text, i);
            line[i] = '\0';

            if (i == cmd_text.cursize)
            {
                cmd_text.cursize = 0;
            }
            else
            {
                i++;
                cmd_text.cursize -= i;
                common_c.Q_memcpy(*text, *text + i, cmd_text.cursize);
            }

            Cmd_ExecuteString(line, cmd_source_t.src_command);

            if (cmd_wait)
            {
                cmd_wait = false;
                break;
            }
        }
    }

    public enum cmd_source_t
    {
        src_client,
        src_command
    }

    public static void Cmd_StuffCmds_f()
    {
        int i, j;
        int s;
        char* text, build;
        char c;

        if (Cmd_Argc() != 1)
        {
            console_c.Con_Printf("stuffcmds: execute command in line parameters\n");
            return;
        }

        s = 0;

        for (i = 1; i < common_c.com_argc; i++)
        {
            if (common_c.com_argv[i] == 0)
            {
                continue;
            }

            s += common_c.Q_strlen(common_c.com_argv[i].ToString()) + 1;
        }

        if (s == 0)
        {
            return;
        }

        text = (char*)zone_c.Z_Malloc(s + 1);
        text[0] = '\0';

        for (i = 1; i < common_c.com_argc; i++)
        {
            if (common_c.com_argv[i] == 0)
            {
                continue;
            }

            common_c.Q_strcat(text->ToString(), common_c.com_argv[i].ToString());

            if (i != common_c.com_argc - 1)
            {
                common_c.Q_strcat(text->ToString(), " ");
            }
        }

        build = (char*)zone_c.Z_Malloc(s + 1);
        build[0] = '\0';

        for (i = 0; i < s - 1; i++)
        {
            if (text[i] == '+')
            {
                i++;

                for (j = 1; (text[j] != 'x') && (text[j] != '-') && (text[j] != '\0'); j++)
                {
                    ;
                }

                c = text[j];
                text[j] = '\0';

                common_c.Q_strcat(build->ToString(), (text + i)->ToString());
                common_c.Q_strcat(build->ToString(), "\n");
                text[j] = c;
                i = j - 1;
            }
        }

        if (build[0] != 0)
        {
            Cbuf_InsertText(build);
        }

        zone_c.Z_Free(text);
        zone_c.Z_Free(build);
    }

    public static void Cmd_Exec_f()
    {
        char* f;
        int mark;

        if (Cmd_Argc() != 2)
        {
            console_c.Con_Printf("exec <filename> : execute a script file\n");
            return;
        }

        mark = zone_c.Hunk_LowMark();
        f = (char*)common_c.COM_LoadHunkFile(Cmd_Argv(1)->ToString());

        if (*f == '\0')
        {
            console_c.Con_Printf($"couldn't exec {Cmd_Argv(1)->ToString()}\n");
            return;
        }

        console_c.Con_Printf($"execing {Cmd_Argv(1)->ToString()}\n");

        Cbuf_InsertText(f);
        zone_c.Hunk_FreeToLowMark(mark);
    }

    public static void Cmd_Echo_f()
    {
        int i;

        for (i = 1; i < Cmd_Argc(); i++)
        {
            console_c.Con_Printf($"{Cmd_Argv(i)->ToString()}");
        }

        console_c.Con_Printf("\n");
    }

    public static char* CopyString(char* input)
    {
        char* output;

        output = (char*)zone_c.Z_Malloc(common_c.Q_strlen(input->ToString()) + 1);
        common_c.Q_strcpy(output->ToString(), input->ToString());
        return output;
    }

    public static void Cmd_Alias_f()
    {
        cmdalias_t* a;
        char* cmd = null;
        int i, c;
        char* s;

        if (Cmd_Argc() == 1)
        {
            console_c.Con_Printf("Current alias commands:\n");

            for (a = cmd_alias; a != null; a = a->next)
            {
                console_c.Con_Printf($"{a->name} : {a->value->ToString()}\n");
            }

            return;
        }

        s = Cmd_Argv(1);

        if (common_c.Q_strlen(s->ToString()) >= MAX_ALIAS_NAME)
        {
            console_c.Con_Printf("Alias name is too long\n");
            return;
        }

        for (a = cmd_alias; a != null; a = a->next)
        {
            if (!common_c.Q_strcmp(s->ToString(), a->name.ToString()))
            {
                zone_c.Z_Free(a->value);
                break;
            }
        }

        if (a == null)
        {
            a = (cmdalias_t*)zone_c.Z_Malloc(sizeof(cmdalias_t));
            a->next = cmd_alias;
            cmd_alias = a;
        }

        common_c.Q_strcpy(a->name.ToString(), s->ToString());

        cmd[0] = '\0';
        c = Cmd_Argc();

        for (i = 2; i < c; i++)
        {
            common_c.Q_strcat(cmd->ToString(), Cmd_Argv(i)->ToString());

            if (i != c)
            {
                common_c.Q_strcat(cmd->ToString(), " ");
            }
        }

        common_c.Q_strcat(cmd->ToString(), "\n");

        a->value = CopyString(cmd);
    }

    public struct cmd_function_t
    {
        public cmd_function_t* next;
        public char* name;
        public Action function;
    }

    public static int MAX_ARGS = 80;

    public static int cmd_argc;
    public static char* cmd_argv;
    public static char* cmd_null_string = null;
    public static char* cmd_args = null;

    public static cmd_source_t cmd_source;

    public static cmd_function_t* cmd_functions;

    public static void Cmd_Init()
    {
        Cmd_AddCommand("stuffcmds", Cmd_StuffCmds_f);
        Cmd_AddCommand("exec", Cmd_Exec_f);
        Cmd_AddCommand("echo", Cmd_Echo_f);
        Cmd_AddCommand("alias", Cmd_Alias_f);
        Cmd_AddCommand("cmd", Cmd_ForwardToServer);
        Cmd_AddCommand("wait", Cmd_Wait_f);
    }

    public static int Cmd_Argc()
    {
        return cmd_argc;
    }

    public static char* Cmd_Argv(int arg)
    {
        if ((uint)arg >= cmd_argc)
        {
            return cmd_null_string;
        }

        return &cmd_args[arg];
    }

    public char* Cmd_Args()
    {
        return cmd_args;
    }

    public static void Cmd_TokenizeString(char* text)
    {
        int i;

        for (i = 0; i < cmd_argc; i++)
        {
            zone_c.Z_Free(&cmd_argv[i]);
        }

        cmd_argc = 0;
        cmd_args = null;

        while (true)
        {
            while (*text != 0 && *text <= ' ' && *text != '\n')
            {
                text++;
            }

            if (*text == '\n')
            {
                text++;
                break;
            }

            if (*text == 0)
            {
                return;
            }

            if (cmd_argc == 1)
            {
                cmd_args = text;
            }

            text = common_c.COM_Parse(text);

            if (text == null)
            {
                return;
            }

            if (cmd_argc < MAX_ARGS)
            {
                cmd_argv[cmd_argc] = *(char*)zone_c.Z_Malloc(common_c.Q_strlen(common_c.com_token.ToString()) + 1);
                common_c.Q_strcpy(cmd_argv[cmd_argc].ToString(), common_c.com_token.ToString());
                cmd_argc++;
            }
        }
    }

    public static void Cmd_AddCommand(string cmd_name, Action function)
    {
        cmd_function_t* cmd;

        if (host_c.host_initialized)
        {
            sys_win_c.Sys_Error("Cmd_AddCommand after host_initialized");
        }

        char[] cmd_name_chararr = cmd_name.ToCharArray();
        char cmd_name_char = '\0';

        for (int i = 0; i < cmd_name_chararr.Length; i++)
        {
            cmd_name_char += cmd_name_chararr[i];
        }

        if (cvar_c.Cvar_VariableString(cmd_name)[0] != null)
        {
            console_c.Con_Printf($"Cmd_AddCommand: {cmd_name} already defined as a var\n");
            return;
        }

        for (cmd = cmd_functions; cmd != null; cmd = cmd->next)
        {
            if (!common_c.Q_strcmp(cmd_name, cmd->name->ToString()))
            {
                console_c.Con_Printf($"Cmd_AddCommand: {cmd_name} already defined\n");
                return;
            }
        }

        cmd = (cmd_function_t*)zone_c.Hunk_Alloc(sizeof(cmd_function_t));
        cmd->name = &cmd_name_char;
        cmd->function = function;
        cmd->next = cmd_functions;
        cmd_functions = cmd;
    }

    public static bool Cmd_Exists(char* cmd_name)
    {
        cmd_function_t* cmd;

        for (cmd = cmd_functions; cmd != null; cmd = cmd->next)
        {
            if (!common_c.Q_strcmp(cmd_name->ToString(), cmd->name->ToString()))
            {
                return true;
            }
        }

        return false;
    }

    public static char* Cmd_CompleteCommand(char* partial)
    {
        cmd_function_t* cmd;
        int len;

        len = common_c.Q_strlen(partial->ToString());

        if (len == 0)
        {
            return null;
        }

        for (cmd = cmd_functions; cmd != null; cmd = cmd->next)
        {
            if (common_c.Q_strncmp(partial, cmd->name, len) == 0)
            {
                return cmd->name;
            }
        }

        return null;
    }

    public static void Cmd_ExecuteString(char* text, cmd_source_t src)
    {
        cmd_function_t* cmd;
        cmdalias_t* a = null;

        cmd_source = src;
        Cmd_TokenizeString(text);

        if (Cmd_Argc() == 0)
        {
            return;
        }

        for (cmd = cmd_functions; cmd != null; cmd=cmd->next)
        {
            if (common_c.Q_strcasecmp(cmd_argv[0].ToString(), a->name.ToString()) == 0)
            {
                Cbuf_InsertText(a->value);
                return;
            }
        }

        if (!cvar_c.Cvar_Command())
        {
            console_c.Con_Printf($"Unknown command \"{Cmd_Argv(0)->ToString()}\"\n");
        }
    }

    public static void Cmd_ForwardToServer()
    {
        if (client_c.cls.state != client_c.cactive_t.ca_connected)
        {
            console_c.Con_Printf($"Can't \"{Cmd_Argv(0)->ToString()}\", not connected\n");
            return;
        }

        if (client_c.cls.demoplayback)
        {
            return;
        }

        common_c.MSG_WriteByte(client_c.cls.message, cl_main_c.clc_stringcmd);

        if (common_c.Q_strcasecmp(Cmd_Argv(0)->ToString(), "cmd") != 0)
        {
            common_c.SZ_Print(client_c.cls.message, Cmd_Argv(0)->ToString());
            common_c.SZ_Print(client_c.cls.message, " ");
        }

        if (Cmd_Argc() > 1)
        {
            common_c.SZ_Print(client_c.cls.message, Cmd_Argc().ToString());
        }
        else
        {
            common_c.SZ_Print(client_c.cls.message, "\n");
        }
    }

    public int Cmd_CheckParm(char* parm)
    {
        int i;

        if (parm == null)
        {
            sys_win_c.Sys_Error("Cmd_CheckParam: null");
        }

        for (i = 1; i < Cmd_Argc(); i++)
        {
            if (common_c.Q_strcasecmp(parm->ToString(), Cmd_Argv(i)->ToString()) == 0)
            {
                return i;
            }
        }

        return 0;
    }
}