namespace Quake;

public unsafe class cvar_c
{
	public struct cvar_t
	{
		public char name;
		public char* str;
		public bool archive;
		public bool server;
		public float value;
		public cvar_t* next;
	}

	cvar_t* cvar_vars;
	char* cvar_null_string = null;

	public cvar_t* Cvar_FindVar(char* var_name)
	{
		cvar_t* var;

		for (var = cvar_vars; var != null; var=var->next)
		{
			if (!common_c.Q_strcmp(var_name->ToString(), var->name))
			{
				return var;
			}
		}

		return null;
	}

	public float Cvar_VariableValue(char* var_name)
	{
		cvar_t* var;

		var = Cvar_FindVar(var_name);

		if (var == null)
		{
			return 0;
		}

		return common_c.Q_atof(&var->name);
	}

	public char* Cvar_VariableString(char* var_name)
	{
		cvar_t* var;

		var = Cvar_FindVar(var_name);

		if (var == null)
		{
			return cvar_null_string;
		}

		return var->str;
	}

	public char* Cvar_CompleteVariable(char* partial)
	{
		cvar_t* cvar;
		int len;

		len = common_c.Q_strlen(partial->ToString());

		if (len == 0)
		{
			return null;
		}

		for (cvar=cvar_vars; cvar != null; cvar = cvar->next)
		{
			if (common_c.Q_strncmp(partial->ToString(), cvar->name.ToString(), len) == 0)
			{

			}
		}
	}
}