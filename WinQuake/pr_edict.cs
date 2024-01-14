using Quake.Client;

namespace Quake;

public unsafe class pr_edict_c
{
    public static pr_comp_c.dprograms_t* progs;
    public static pr_comp_c.dfunction_t* pr_functions;
    public static char* pr_strings;
    public static pr_comp_c.ddef_t* pr_fielddefs;
    public static pr_comp_c.ddef_t* pr_globaldefs;
    public static pr_comp_c.dstatement_t* pr_statements;
    public static progdefs_c.globalvars_t* pr_global_struct;
    public static float* pr_globals;
    public static int pr_edict_size;

    public static ushort pr_crc;

    public static int[] type_size = { 1, sizeof(int) / 4, 1, 3, 1, 1, sizeof(int) / 4, sizeof(void*) / 4 };

    public static cvar_c.cvar_t nomonsters = new cvar_c.cvar_t { name = "nomonsters", value = (char)0 };
    public static cvar_c.cvar_t gamecfg = new cvar_c.cvar_t { name = "gamecfg", value = (char)0 };
    public static cvar_c.cvar_t scratch1 = new cvar_c.cvar_t { name = "scratch1", value = (char)0 };
    public static cvar_c.cvar_t scratch2 = new cvar_c.cvar_t { name = "scratch2", value = (char)0 };
    public static cvar_c.cvar_t scratch3 = new cvar_c.cvar_t { name = "scratch3", value = (char)0 };
    public static cvar_c.cvar_t scratch4 = new cvar_c.cvar_t { name = "scratch4", value = (char)0 };
    public static cvar_c.cvar_t savedgamecfg = new cvar_c.cvar_t { name = "savedgamecfg", value = (char)0, archive = true };
    public static cvar_c.cvar_t saved1 = new cvar_c.cvar_t { name = "saved1", value = (char)0, archive = true };
    public static cvar_c.cvar_t saved2 = new cvar_c.cvar_t { name = "saved2", value = (char)0, archive = true };
    public static cvar_c.cvar_t saved3 = new cvar_c.cvar_t { name = "saved3", value = (char)0, archive = true };
    public static cvar_c.cvar_t saved4 = new cvar_c.cvar_t { name = "saved4", value = (char)0, archive = true };

    public const int MAX_FIELD_LEN = 64;
    public const int GEFV_CACHESIZE = 2;

    public struct gefv_cache
    {
        public pr_comp_c.ddef_t* pcache;
        public char* field;
    }

    public static gefv_cache[] gefvCache = { new gefv_cache { field = null, pcache = null }, new gefv_cache { field = null, pcache = null } };

    public static void ED_ClearEdict(progs_c.edict_t* e)
    {
        common_c.Q_memset(e->v, 0, progs->entityfields * 4);
        e->free = false;
    }

    public static progs_c.edict_t* ED_Alloc()
    {
        int i;
        progs_c.edict_t* e;

        for (i = server_c.svs.maxclients + 1; i < server_c.sv.num_edicts; i++)
        {
            e = EDICT_NUM(i);

            if (e->free && (e->freetime < 2 || server_c.sv.time - e->freetime > 0.5))
            {
                ED_ClearEdict(e);
                return e;
            }
        }

        if (i == bothdefs_c.MAX_EDICTS)
        {
            sys_win_c.Sys_Error("ED_Alloc: no free edicts");
        }

        server_c.sv.num_edicts++;
        e = EDICT_NUM(i);
        ED_ClearEdict(e);

        return e;
    }

    public static void ED_Free(progs_c.edict_t* ed)
    {
        world_c.SV_UnlinkEdict(ed);

        ed->free = true;
        ed->v.model = null;
        ed->v.takedamage = 0;
        ed->v.modelindex = 0;
        ed->v.colormap = 0;
        ed->v.skin = 0;
        ed->v.frame = 0;
        mathlib_c.VectorCopy(mathlib_c.vec3_origin, ed->v.origin);
        mathlib_c.VectorCopy(mathlib_c.vec3_origin, ed->v.angles);
        ed->v.nextthink = -1;
        ed->v.solid = 0;

        ed->freetime = (float)server_c.sv.time;
    }

    public static pr_comp_c.ddef_t* ED_GlobalAtOfs(int ofs)
    {
        pr_comp_c.ddef_t* def;
        int i;

        for (i = 0; i < progs->numglobaldefs; i++)
        {
            def = &pr_globaldefs[i];

            if (def->ofs == ofs)
            {
                return def;
            }
        }

        return null;
    }

    public static pr_comp_c.ddef_t* ED_FieldAtOfs(int ofs)
    {
        pr_comp_c.ddef_t* def;
        int i;

        for (i = 0; i < progs->numfielddefs; i++)
        {
            def = &pr_fielddefs[i];

            if (def->ofs == ofs)
            {
                return def;
            }
        }

        return null;
    }

    public static pr_comp_c.ddef_t* ED_FindField(char* name)
    {
        pr_comp_c.ddef_t* def;
        int i;

        for (i = 0; i < progs->numfielddefs; i++)
        {
            def = &pr_fielddefs[i];

            if (!common_c.Q_strcmp((pr_strings + def->s_name)->ToString(), name->ToString()))
            {
                return def;
            }
        }

        return null;
    }

    public static pr_comp_c.ddef_t* ED_FindGlobal(char* name)
    {
        pr_comp_c.ddef_t* def;
        int i;

        for (i = 0; i < progs->numglobaldefs; i++)
        {
            def = &pr_globaldefs[i];

            if (!common_c.Q_strcmp((pr_strings + def->s_name)->ToString(), name->ToString()))
            {
                return def;
            }
        }

        return null;
    }

    public static pr_comp_c.dfunction_t* ED_FindFunction(char* name)
    {
        pr_comp_c.dfunction_t* func;
        int i;

        for (i = 0; i < progs->numfunctions; i++)
        {
            func = &pr_functions[i];

            if (!common_c.Q_strcmp((pr_strings + func->s_name)->ToString(), name->ToString()))
            {
                return func;
            }
        }

        return null;
    }

    public static progs_c.eval_t* GetEdictFieldValue(progs_c.edict_t* ed, char* field)
    {
        pr_comp_c.ddef_t* def = null;
        int i;
        int rep = 0;

        for (i = 0; i < GEFV_CACHESIZE; i++)
        {
            if (!common_c.Q_strcmp(field->ToString(), gefvCache[i].field->ToString()))
            {
                def = gefvCache[i].pcache;
                goto Done;
            }
        }

        def = ED_FindField(field);

        if (common_c.Q_strlen(field->ToString()) < MAX_FIELD_LEN)
        {
            gefvCache[rep].pcache = def;
            common_c.Q_strcpy(gefvCache[rep].field->ToString(), field->ToString());
            rep ^= 1;
        }

    Done:
        if (def == null)
        {
            return null;
        }

        return (progs_c.eval_t*)((char*)&ed->v + def->ofs * 4);
    }

    public static progs_c.eval_t* GetEdictFieldValue(progs_c.edict_t* ed, string field)
    {
        return GetEdictFieldValue(ed, common_c.StringToChar(field));
    }

    public static char* PR_ValueString(pr_comp_c.etype_t type, progs_c.eval_t* val)
    {
        char* line = null;
        pr_comp_c.ddef_t* def = null;
        pr_comp_c.dfunction_t* f;

        type &= ~pr_comp_c.DEF_SAVEGLOBAL;

        switch (type)
        {
            case pr_comp_c.etype_t.ev_string:
                Console.WriteLine(line->ToString(), $"{*pr_strings + *val->str}");
                break;

            case pr_comp_c.etype_t.ev_entity:
                Console.WriteLine(line->ToString(), $"entity {NUM_FOR_EDICT(PROG_TO_EDICT(val->edict))}");
                break;

            case pr_comp_c.etype_t.ev_function:
                f = pr_functions + val->function;
                Console.WriteLine(line->ToString(), $"{*pr_strings + f->s_name}");
                break;

            case pr_comp_c.etype_t.ev_field:
                def = ED_FieldAtOfs(val->_int);
                Console.WriteLine(line->ToString(), $"{*pr_strings + def->s_name}");
                break;

            case pr_comp_c.etype_t.ev_void:
                Console.WriteLine(line->ToString(), "void");
                break;

            case pr_comp_c.etype_t.ev_float:
                Console.WriteLine(line->ToString(), $"{val->_float}");
                break;

            case pr_comp_c.etype_t.ev_vector:
                Console.WriteLine(line->ToString(), $"{val->vector[0]} {val->vector[1]} {val->vector[2]}");
                break;

            case pr_comp_c.etype_t.ev_pointer:
                Console.WriteLine(line->ToString(), "pointer");
                break;

            default:
                Console.WriteLine(line->ToString(), $"bad type {type}");
                break;
        }

        return line;
    }

    public static char* PR_UglyValueString(pr_comp_c.etype_t type, progs_c.eval_t* val)
    {
        char* line = null;
        pr_comp_c.ddef_t* def;
        pr_comp_c.dfunction_t* f;

        type &= ~pr_comp_c.DEF_SAVEGLOBAL;

        switch (type)
        {
            case pr_comp_c.etype_t.ev_string:
                Console.WriteLine(line->ToString(), $"{*pr_strings + *val->str}");
                break;

            case pr_comp_c.etype_t.ev_entity:
                Console.WriteLine(line->ToString(), $"{NUM_FOR_EDICT(PROG_TO_EDICT(val->edict))}");
                break;

            case pr_comp_c.etype_t.ev_function:
                f = pr_functions + val->function;
                Console.WriteLine(line->ToString(), $"{*pr_strings + f->s_name}");
                break;

            case pr_comp_c.etype_t.ev_field:
                def = ED_FieldAtOfs(val->_int);
                Console.WriteLine(line->ToString(), $"{*pr_strings + def->s_name}");
                break;

            case pr_comp_c.etype_t.ev_void:
                Console.WriteLine(line->ToString(), "void");
                break;

            case pr_comp_c.etype_t.ev_float:
                Console.WriteLine(line->ToString(), $"{val->_float}");
                break;

            case pr_comp_c.etype_t.ev_vector:
                Console.WriteLine(line->ToString(), "pointer");
                break;

            default:
                Console.WriteLine(line->ToString(), $"bad type {type}");
                break;
        }

        return line;
    }

    public static char* PR_UglyValueString(pr_comp_c.etype_t type, progs_c.eval_t* val)
    {
        char* line = null;
        pr_comp_c.ddef_t* def;
        pr_comp_c.dfunction_t* f;

        type &= ~pr_comp_c.DEF_SAVEGLOBAL;

        switch (type)
        {
            case pr_comp_c.etype_t.ev_string:
                Console.WriteLine(line->ToString(), $"{*pr_strings + *val->str}");
                break;

            case pr_comp_c.etype_t.ev_entity:
                Console.WriteLine(line->ToString(), $"{NUM_FOR_EDICT(PROG_TO_EDICT(val->edict))}");
                break;

            case pr_comp_c.etype_t.ev_function:
                f = pr_functions + val->function;
                Console.WriteLine(line->ToString(), $"{*pr_strings + f->s_name}");
                break;

            case pr_comp_c.etype_t.ev_field:
                def = ED_FieldAtOfs(val->_int);
                Console.WriteLine(line->ToString(), $"{*pr_strings + def->s_name}");
                break;

            case pr_comp_c.etype_t.ev_void:
                Console.WriteLine(line->ToString(), "void");
                break;

            case pr_comp_c.etype_t.ev_float:
                Console.WriteLine(line->ToString(), $"{val->_float}");
                break;

            case pr_comp_c.etype_t.ev_vector:
                Console.WriteLine(line->ToString(), $"{val->vector[0]} {val->vector[1]} {val->vector[2]}");
                break;

            default:
                Console.WriteLine(line->ToString(), $"bad type {type}");
                break;
        }

        return line;
    }

    public static char* PR_GlobalString(int ofs)
    {
        char* s;
        int i;
        pr_comp_c.ddef_t* def;
        void* val;
        char* line = null;

        val = (void*)&pr_globals[ofs];
        def = ED_GlobalAtOfs(ofs);

        if (def == null)
        {
            Console.WriteLine(line->ToString(), $"{ofs}(???)");
        }
        else
        {
            s = PR_ValueString(def->type, (progs_c.eval_t*)val);
            Console.WriteLine(line->ToString(), $"{ofs}({*pr_strings + def->s_name}){*s}");
        }

        i = common_c.Q_strlen(line->ToString());

        for (; i < 20; i++)
        {
            common_c.Q_strcat(line->ToString(), " ");
        }

        common_c.Q_strcat(line->ToString(), " ");

        return line;
    }

    public static char* PR_GlobalStringNoContents(int ofs)
    {
        int i;
        pr_comp_c.ddef_t* def;
        char* line = null;

        def = ED_GlobalAtOfs(ofs);

        if (def == null)
        {
            Console.WriteLine(line->ToString(), $"{ofs}(???)");
        }
        else
        {
            Console.WriteLine(line->ToString(), $"{ofs}({*pr_strings + def->s_name})");
        }

        i = common_c.Q_strlen(line->ToString());

        for (; i < 20; i++)
        {
            common_c.Q_strcat(line->ToString(), " ");
        }

        common_c.Q_strcat(line->ToString(), " ");

        return line;
    }

    public static void ED_Print(progs_c.edict_t* ed)
    {
        int l;
        pr_comp_c.ddef_t* d;
        int* v;
        int i, j;
        char* name;
        int type;

        if (ed->free)
        {
            console_c.Con_Print("FREE\n");
            return;
        }

        console_c.Con_Printf($"\nEDICT {NUM_FOR_EDICT(ed)}:\n");

        for (i = 1; i < progs->numfielddefs; i++)
        {
            d = &pr_fielddefs[i];
            name = pr_strings + d->s_name;

            if (name[common_c.Q_strlen(name->ToString()) - 2] == '_')
            {
                continue;
            }

            v = (int*)((char*)&ed->v + d->ofs * 4);

            type = d->type & ~pr_comp_c.DEF_SAVEGLOBAL;

            for (j = 0; j < type_size[type]; j++)
            {
                if (v[j] != null)
                {
                    break;
                }
            }

            if (j == type_size[type])
            {
                continue;
            }

            console_c.Con_Printf($"{name->ToString()}");
            l = common_c.Q_strlen(name->ToString());

            while (l++ < 15)
            {
                console_c.Con_Printf(" ");
            }

            console_c.Con_Printf($"{PR_ValueString(d->type, (progs_c.eval_t*)v)}\n");
        }
    }

    public static void ED_Write(File* f, progs_c.edict_t* ed)
    {
        pr_comp_c.ddef_t* d;
        int* v;
        int i, j;
        char* name;
        int type;

        Console.Write("{\n");

        if (ed->free)
        {
            Console.Write("}\n");
            return;
        }

        for (i = 1; i < progs->numfielddefs; i++)
        {
            d = &pr_fielddefs[i];
            name = pr_strings + d->s_name;

            if (name[common_c.Q_strlen(name->ToString()) - 2] == '_')
            {
                continue;
            }

            v = (int*)((char*)&ed->v + d->ofs * 4);

            type = d->type & ~pr_comp_c.DEF_SAVEGLOBAL;

            for (j = 0; j < type_size[type]; j++)
            {
                if (v[j] != null)
                {
                    break;
                }
            }

            if (j == type_size[type])
            {
                continue;
            }

            Console.Write(f->ToString(), $"\"{name->ToString()}\" ");
            Console.Write(f->ToString(), $"\"{PR_UglyValueString(d->type, (progs_c.eval_t*)v)}\"\n");
        }

        Console.Write(f->ToString(), "}\n");
    }

    public static void ED_PrintNum(int ent)
    {
        ED_Print(EDICT_NUM(ent));
    }

    public static void ED_PrintEdicts()
    {
        int i;

        console_c.Con_Printf($"{server_c.sv.num_edicts} entities\n");

        for (i = 0; i < server_c.sv.num_edicts; i++)
        {
            ED_PrintNum(i);
        }
    }

    public static void ED_PrintEdict_f()
    {
        int i;

        i = common_c.Q_atoi(cmd_c.Cmd_Argv(1)->ToString());

        if (i >= server_c.sv.num_edicts)
        {
            console_c.Con_Printf("Bad edict number\n");
            return;
        }

        ED_PrintNum(i);
    }

    public static void ED_Count()
    {
        int i;
        progs_c.edict_t* ent;
        int active, models, solid, step;

        active = models = solid = step = 0;

        for (i = 0; i < server_c.sv.num_edicts; i++)
        {
            ent = EDICT_NUM(i);

            if (ent->free)
            {
                continue;
            }

            active++;

            if (ent->v.solid != 0)
            {
                solid++;
            }

            if (*ent->v.model != 0)
            {
                models++;
            }

            if (ent->v.movetype == server_c.MOVETYPE_STEP)
            {
                step++;
            }
        }

        console_c.Con_Printf($"num_edicts:{server_c.sv.num_edicts}\n");
        console_c.Con_Printf($"active    :{active}\n");
        console_c.Con_Printf($"view      :{models}\n");
        console_c.Con_Printf($"touch     :{solid}\n");
        console_c.Con_Printf($"step      :{step}\n");
    }

    public static void ED_WriteGlobals(File* f)
    {
        pr_comp_c.ddef_t* def;
        int i;
        char* name;
        int type;

        Console.Write(f->ToString(), "{\n");

        for (i = 0; i < progs->numglobaldefs; i++)
        {
            def = &pr_globaldefs[i];
            type = def->type;

            if ((def->type & pr_comp_c.DEF_SAVEGLOBAL) == 0)
            {
                continue;
            }

            type &= ~pr_comp_c.DEF_SAVEGLOBAL;

            if (type != (int)pr_comp_c.etype_t.ev_string && type != (int)pr_comp_c.etype_t.ev_float && type != (int)pr_comp_c.etype_t.ev_entity)
            {
                continue;
            }

            name = pr_strings + def->s_name;
            Console.Write(f->ToString(), $"\"{*name}\"");
            Console.Write(f->ToString(), $"\"{PR_UglyValueString(type, (progs_c.eval_t*)&pr_globals[def->ofs])}\"\n");
        }

        Console.Write(f->ToString(), "}\n");
    }

    public static void ED_ParseGlobals(char* data)
    {
        char* keyname = null;
        pr_comp_c.ddef_t* key;

        while (true)
        {
            data = common_c.COM_Parse(data);

            if (common_c.com_token[0] == "}")
            {
                break;
            }

            if (data == null)
            {
                sys_win_c.Sys_Error("ED_ParseEntity: EOF without closing brace");
            }

            common_c.Q_strcpy(keyname->ToString(), common_c.com_token.ToString());

            data = common_c.COM_Parse(data);

            if (data == null)
            {
                sys_win_c.Sys_Error("ED_ParseEntity: EOF without closing brace");
            }

            if (common_c.com_token[0] == "}")
            {
                sys_win_c.Sys_Error("ED_ParseEntity: closing brace without data");
            }

            key = ED_FindGlobal(keyname);

            if (key == null)
            {
                console_c.Con_Printf($"'{keyname->ToString()}' is not a global\n");
                continue;
            }

            if (ED_ParseEpair((void*)pr_globals, key, common_c.StringToChar(common_c.com_token)) == null)
            {
                host_c.Host_Error("ED_ParseGlobals: parse error");
            }
        }
    }

    public static char* ED_NewString(char* str)
    {
        char* _new, new_p;
        int i, l;

        l = common_c.Q_strlen(str->ToString()) + 1;
        _new = (char*)zone_c.Hunk_Alloc(l);
        new_p = _new;

        for (i = 0; i < l; i++)
        {
            if (str[i] == '\\' && i < l - 1)
            {
                i++;

                if (str[i] == 'n')
                {
                    *new_p++ = '\n';
                }
                else
                {
                    *new_p++ = '\\';
                }
            }
            else
            {
                *new_p++ = str[i];
            }
        }

        return _new;
    }

    public static bool ED_ParsePair(void* _base, pr_comp_c.ddef_t* key, char* s)
    {
        int i;
        char* str = null;
        pr_comp_c.ddef_t* def;
        char* v, w;
        void* d;
        pr_comp_c.dfunction_t* func;

        d = (int*)_base + key->ofs;

        switch (key->type & ~pr_comp_c.DEF_SAVEGLOBAL)
        {
            case pr_comp_c.etype_t.ev_string:
                *(int*)d = (int)(ED_NewString(s) - pr_strings);
                break;

            case pr_comp_c.etype_t.ev_float:
                *(float*)d = common_c.Q_atof(s);
                break;

            case pr_comp_c.etype_t.ev_vector:
                common_c.Q_strcpy(str->ToString(), s->ToString());
                v = str;
                w = str;

                for (i = 0; i < 3; i++)
                {
                    while (*v != null && *v != ' ')
                    {
                        v++;
                    }

                    *v = (char)0;

                    ((float*)d)[i] = common_c.Q_atof(w);
                    w = v = v + 1;
                }
                break;

            case pr_comp_c.etype_t.ev_entity:
                *(int*)d = EDICT_TO_PROG(EDICT_NUM(common_c.Q_atoi(s)));
                break;

            case pr_comp_c.etype_t.ev_field:
                def = ED_FindField(s);
                
                if (def == null)
                {
                    console_c.Con_Printf($"Can't find field {s->ToString()}\n");
                    return false;
                }

                *(int*)d = pr_edict_c.G_INT(def->ofs);
                break;

            case pr_comp_c.etype_t.ev_function:
                func = ED_FindFunction(s);

                if (func == null)
                {
                    console_c.Con_Printf($"Can't find function {*s}\n");
                    return false;
                }

                *(int*)d = (int)(func - pr_functions);
                break;

            default:
                break;
        }

        return true;
    }

    public static char* ED_ParseEdict(char* data, progs_c.edict_t* ent)
    {
        pr_comp_c.ddef_t* key;
        bool anglehack;
        bool init;
        char* keyname = null;
        int n;

        init = false;

        if (ent != server_c.sv.edicts)
        {
            common_c.Q_memset(ent->v, 0, progs->entityfields * 4);
        }

        while (true)
        {
            data = common_c.COM_Parse(data);

            if (common_c.com_token[0] == "}")
            {
                break;
            }

            if (data == null)
            {
                sys_win_c.Sys_Error("ED_ParseEntity: EOF without closing brace");
            }

            if (!common_c.Q_strcmp(common_c.com_token.ToString(), "angle"))
            {
                common_c.Q_strcpy(common_c.com_token.ToString(), "angles");
                anglehack = true;
            }
            else
            {
                anglehack = false;
            }

            if (!common_c.Q_strcmp(common_c.com_token.ToString(), "light"))
            {
                common_c.Q_strcpy(common_c.com_token.ToString(), "light_lev");
            }

            common_c.Q_strcpy(keyname->ToString(), common_c.com_token.ToString());

            n = common_c.Q_strlen(keyname->ToString());

            while (n != 0 && keyname[n-1] == ' ')
            {
                keyname[n - 1] = (char)0;
                n--;
            }

            data = common_c.COM_Parse(data);

            if (data == null)
            {
                sys_win_c.Sys_Error("ED_ParseEntity: EOF without closing brace");
            }

            if (common_c.com_token[0] == "}")
            {
                sys_win_c.Sys_Error("ED_ParseEntity: closing brace without data");
            }

            init = true;

            if (keyname[0] == '_')
            {
                continue;
            }

            key = ED_FindField(keyname);

            if (key == null)
            {
                console_c.Con_Printf($"'{*keyname}' is not a field\n");
                continue;
            }

            if (anglehack)
            {
                char* temp = null;
                common_c.Q_strcpy(temp->ToString(), common_c.com_token.ToString());
                Console.WriteLine(common_c.com_token.ToString(), $"0 {*temp} 0");
            }

            if (!ED_ParsePair(&ent->v, key, common_c.StringToChar(common_c.com_token.ToString())))
            {
                host_c.Host_Error("ED_ParseEdict: parse error");
            }
        }

        if (!init)
        {
            ent->free = true;
        }

        return data;
    }

    public static void ED_LoadFromFile(char* data)
    {
        progs_c.edict_t* ent;
        int inhibit;
        pr_comp_c.dfunction_t* func;

        ent = null;
        inhibit = 0;
        pr_global_struct->time = (float)server_c.sv.time;

        while (true)
        {
            data = common_c.COM_Parse(data);

            if (data == null)
            {
                break;
            }

            if (common_c.com_token[0] != "{")
            {
                sys_win_c.Sys_Error($"ED_LoadFromFile: found {common_c.com_token} when expecting " + "{");
            }

            if (ent == null)
            {
                ent = EDICT_NUM(0);
            }
            else
            {
                ent = ED_Alloc();
            }

            data = ED_ParseEdict(data, ent);

            if (host_c.deathmatch.value != 0)
            {
                if (((int)ent->v.spawnflags & server_c.SPAWNFLAG_NOT_DEATHMATCH) != 0)
                {
                    ED_Free(ent);
                    inhibit++;
                    continue;
                }
            }
            else if ((host_cmd_c.current_skill == 0 && ((int)ent->v.spawnflags & server_c.SPAWNFLAG_NOT_EASY) == 0) || (host_cmd_c.current_skill == 1 && ((int)ent->v.spawnflags & server_c.SPAWNFLAG_NOT_MEDIUM) != 0) ||(host_cmd_c.current_skill >= 2 && ((int)ent->v.spawnflags & server_c.SPAWNFLAG_NOT_HARD) != 0))
            {
                ED_Free(ent);
                inhibit++;
                continue;
            }

            if (ent->v.classname == null)
            {
                console_c.Con_Printf("No classname for:\n");
                ED_Print(ent);
                ED_Free(ent);
                continue;
            }

            func = ED_FindFunction(pr_strings + *common_c.StringToChar(ent->v.classname));

            if (func == null)
            {
                console_c.Con_Printf("No spawn function for:\n");
                ED_Print(ent);
                ED_Free(ent);
                continue;
            }

            pr_global_struct->self = EDICT_TO_PROG(ent);
            pr_exec_c.PR_ExecuteProgram(func - pr_functions);
        }

        console_c.Con_DPrintf($"{inhibit} entities inhibited\n");
    }

    public static void PR_LoadProgs()
    {
        int i;

        for (i = 0; i < GEFV_CACHESIZE; i++)
        {
            gefvCache[i].field[0] = (char)0;
        }

        crc_c.CRC_Init(&pr_crc);

        progs = (pr_comp_c.dprograms_t*)common_c.COM_LoadHunkFile("progs.dat");

        if (progs == null)
        {
            sys_win_c.Sys_Error("PR_LoadProgs: couldn't load progs.dat");
        }

        console_c.Con_DPrintf($"Programs occupy {common_c.com_filesize / 1024}K.\n");

        for (i = 0; i < *progs / 4; i++)
        {
            ((int*)progs)[i] = common_c.LittleLong(((int*)progs)[i]);
        }

        if (progs->version != pr_comp_c.PROG_VERSION)
        {
            sys_win_c.Sys_Error($"progs.dat has wrong version number ({progs->version} should be {pr_comp_c.PROG_VERSION})");
        }

        if (progs->crc != progdefs_c.PROGHEADER_CRC)
        {
            sys_win_c.Sys_Error("progs.dat system vars have been modified, progdefs.h is out of date");
        }

        pr_functions = (pr_comp_c.dfunction_t*)((byte*)progs + progs->ofs_functions);
        pr_strings = (char*)progs + progs->ofs_strings;
        pr_globaldefs = (pr_comp_c.ddef_t*)((byte*)progs + progs->ofs_globaldefs);
        pr_fielddefs = (pr_comp_c.ddef_t*)((byte*)progs + progs->ofs_fielddefs);
        pr_statements = (pr_comp_c.dstatement_t*)((byte*)progs + progs->ofs_statements);

        pr_global_struct = (progdefs_c.globalvars_t*)((byte*)progs + progs->ofs_globals);
        pr_globals = (float*)pr_global_struct;

        pr_edict_size = progs->entityfields * 4 + sizeof(progs_c.edict_t) - sizeof(progdefs_c.entvars_t);

        for (i = 0; i < progs->numstatements; i++)
        {
            pr_statements[i].op = common_c.LittleShort(pr_statements[i].op);
            pr_statements[i].a = common_c.LittleShort(pr_statements[i].a);
            pr_statements[i].b = common_c.LittleShort(pr_statements[i].b);
            pr_statements[i].c = common_c.LittleShort(pr_statements[i].c);
        }

        for (i = 0; i < progs->numfunctions; i++)
        {
            pr_functions[i].first_statement = common_c.LittleLong(pr_functions[i].first_statement);
            pr_functions[i].parm_start = common_c.LittleLong(pr_functions[i].parm_start);
            pr_functions[i].s_name = common_c.LittleLong(pr_functions[i].s_name);
            pr_functions[i].s_file = common_c.LittleLong(pr_functions[i].s_file);
            pr_functions[i].numparms = common_c.LittleLong(pr_functions[i].numparms);
            pr_functions[i].locals = common_c.LittleLong(pr_functions[i].locals);
        }

        for (i = 0; i < progs->numglobaldefs; i++)
        {
            pr_globaldefs[i].type = common_c.LittleShort(pr_globaldefs[i].type);
            pr_globaldefs[i].ofs = common_c.LittleShort(pr_globaldefs[i].ofs);
            pr_globaldefs[i].s_name = common_c.LittleLong(pr_globaldefs[i].s_name);
        }

        for (i = 0; i < progs->numfielddefs; i++)
        {
            pr_fielddefs[i].type = common_c.LittleShort(pr_fielddefs[i].type);

            if ((pr_fielddefs[i].type & pr_comp_c.DEF_SAVEGLOBAL) != 0)
            {
                sys_win_c.Sys_Error("PR_LoadProgs: pr_fielddefs[i].type & DEF_SAVEGLOBAL");
            }

            pr_fielddefs[i].ofs = common_c.LittleShort(pr_fielddefs[i].ofs);
            pr_fielddefs[i].s_name = common_c.LittleLong(pr_fielddefs[i].s_name);
        }

        for (i = 0; i< progs->numglobals; i++)
        {
            ((int*)pr_globaldefs)[i] = common_c.LittleLong(((int*)pr_globals)[i]);
        }
    }

    public static void PR_Init()
    {
        cmd_c.Cmd_AddCommand("edict", ED_PrintEdict_f);
        cmd_c.Cmd_AddCommand("edicts", ED_PrintEdicts);
        cmd_c.Cmd_AddCommand("edictcount", ED_Count);
        cmd_c.Cmd_AddCommand("profile", pr_exec_c.PR_Profile_f);
        cvar_c.Cvar_RegisterVariable(nomonsters);
        cvar_c.Cvar_RegisterVariable(gamecfg);
        cvar_c.Cvar_RegisterVariable(scratch1);
        cvar_c.Cvar_RegisterVariable(scratch2);
        cvar_c.Cvar_RegisterVariable(scratch3);
        cvar_c.Cvar_RegisterVariable(scratch4);
        cvar_c.Cvar_RegisterVariable(savedgamecfg);
        cvar_c.Cvar_RegisterVariable(saved1);
        cvar_c.Cvar_RegisterVariable(saved2);
        cvar_c.Cvar_RegisterVariable(saved3);
        cvar_c.Cvar_RegisterVariable(saved4);
    }

    public static progs_c.edict_t* EDICT_NUM(int n)
    {
        if (n < 0 || n >= server_c.sv.max_edicts)
        {
            sys_win_c.Sys_Error($"EDICT_NUM: bad number {n}");
        }

        return (progs_c.edict_t*)((byte*)server_c.sv.edicts + n * pr_edict_size);
    }

    public static int NUM_FOR_EDICT(progs_c.edict_t* e)
    {
        int b;

        b = (int)((byte*)e - (byte*)server_c.sv.edicts);
        b = b / pr_edict_size;

        if (b < 0 || b > server_c.sv.num_edicts)
        {
            sys_win_c.Sys_Error("NUM_FOR_EDICT: had pointer");
        }

        return b;
    }
}