namespace Quake;

public unsafe class cvar_c
{
    public struct cvar_t
    {
        public string name;
        public char* str;
        public bool archive;
        public bool server;
        public char value;
        public cvar_t* next;
    }

    public static cvar_t* cvar_vars;
    public static char* cvar_null_string = null;

    public static cvar_t* Cvar_FindVar(string var_name)
    {
        cvar_t* var;

        for (var = cvar_vars; var != null; var = var->next)
        {
            if (!common_c.Q_strcmp(var_name, var->name.ToString()))
            {
                return var;
            }
        }

        return null;
    }

    public float Cvar_VariableValue(string var_name)
    {
        cvar_t* var;

        var = Cvar_FindVar(var_name);

        if (var == null)
        {
            return 0;
        }

        char[] var_name_arr = var->name.ToCharArray();
        char* var_name_char = null;

        for (int i = 0; i < var_name_arr.Length; i++)
        {
            var_name_char += var_name_arr[i];
        }

        return common_c.Q_atof(var_name_char);
    }

    public static char* Cvar_VariableString(string var_name)
    {
        cvar_t* var;

        var = Cvar_FindVar(var_name);

        if (var == null)
        {
            return cvar_null_string;
        }

        return var->str;
    }

    public static char* Cvar_CompleteVariable(string partial)
    {
        cvar_t* cvar = null;
        int len;

        len = common_c.Q_strlen(partial);

        if (len == 0)
        {
            return null;
        }

        char[] var_name_arr = cvar->name.ToCharArray();
        char* var_name_char = null;

        for (int i = 0; i < var_name_arr.Length; i++)
        {
            var_name_char += var_name_arr[i];
        }

        char[] partial_arr = partial.ToCharArray();
        char* partial_char = null;

        for (int i = 0; i < partial_arr.Length; i++)
        {
            partial_char += partial_arr[i];
        }

        for (cvar = cvar_vars; cvar != null; cvar = cvar->next)
        {
            if (common_c.Q_strncmp(partial_char, var_name_char, len) == 0)
            {
                return var_name_char;
            }
        }

        return null;
    }

    public static void Cvar_Set(string var_name, string value)
    {
        cvar_t* var;
        bool changed;

        var = Cvar_FindVar(var_name);

        if (var == null)
        {
            console_c.Con_Printf($"Cvar_Set: variable {var_name} not found\n");
            return;
        }

        changed = common_c.Q_strcmp(var->str->ToString(), value);

        zone_c.Z_Free(var->str);

        var->str = (char*)zone_c.Z_Malloc(common_c.Q_strlen(value) + 1);
        common_c.Q_strcpy(var->str->ToString(), value);
        var->value = (char)common_c.Q_atof(var->str);

        if (var->value != 0 && changed)
        {
            if (sv.active)
            {
                SV_BroadcastPrintf($"\"{var->name}\" changed to \"{var->str}\"\n");
            }
        }
    }

    public void Cvar_SetValue(string var_name, float value)
    {
        string val = null;

        Console.WriteLine(val, $"{value}");
        Cvar_Set(var_name, val);
    }

    public static void Cvar_RegisterVariable(cvar_t* variable)
    {
        char* oldstr;

        char[] var_name_arr = variable->name.ToCharArray();
        char* var_name_char = null;

        for (int i = 0; i < var_name_arr.Length; i++)
        {
            var_name_char += var_name_arr[i];
        }

        if (Cvar_FindVar(variable->name) != null)
        {
            console_c.Con_Printf($"Can't register variable {variable->name}, already defined\n");
            return;
        }

        if (cmd_c.Cmd_Exists(var_name_char))
        {
            console_c.Con_Printf($"Cvar_RegisterVariable: {variable->name} is a command\n");
            return;
        }

        oldstr = variable->str;
        variable->str = (char*)zone_c.Z_Malloc(common_c.Q_strlen(variable->str->ToString()) + 1);
        common_c.Q_strcpy(variable->str->ToString(), oldstr->ToString());
        variable->value = (char)common_c.Q_atof(variable->str);

        variable->next = cvar_vars;
        cvar_vars = variable;
    }

    public static bool Cvar_Command()
    {
        cvar_t* v;

        v = Cvar_FindVar(cmd_c.Cmd_Argv(0)->ToString());

        if (v == null)
        {
            return false;
        }

        if (cmd_c.Cmd_Argc() == 1)
        {
            console_c.Con_Printf($"\"{v->name}\" is \"{v->str->ToString()}\"\n");
            return true;
        }

        Cvar_Set(v->name, cmd_c.Cmd_Argv(1)->ToString());
        return true;
    }

    public void Cvar_WriteVariables(FileStream* f)
    {
        cvar_t* var;

        for (var = cvar_vars; var != null; var = var->next)
        {
            if (var->archive)
            {
                Console.WriteLine($"{var->name} \"{var->str->ToString()}\"\n");
            }
        }
    }
}