namespace Quake;

public unsafe class pr_exec_c
{
    public struct prstack_t
    {
        public int s;
        public pr_comp_c.dfunction_t* f;
    }

    public static int MAX_STACK_DEPTH = 32;
    public static prstack_t* pr_stack;
    public static int pr_depth;

    public static int LOCALSTACK_SIZE = 2048;
    public static int* localstack;
    public static int localstack_used;

    public static bool pr_trace;
    public static pr_comp_c.dfunction_t* pr_xfunction;
    public static int pr_xstatement;

    public static int pr_argc;

    public static string[] pr_opnames =
        {
            "DONE",

            "MUL_F",
            "MUL_V",
            "MUL_FV",
            "MUL_VF",

            "DIV",

            "ADD_F",
            "ADD_V",

            "SUB_F",
            "SUB_V",

            "EQ_F",
            "EQ_V",
            "EQ_S",
            "EQ_E",
            "EQ_FNC",

            "NE_F",
            "NE_V",
            "NE_S",
            "NE_E",
            "NE_FNC",

            "LE",
            "GE",
            "LT",
            "GT",

            "INDIRECT",
            "INDIRECT",
            "INDIRECT",
            "INDIRECT",
            "INDIRECT",
            "INDIRECT",

            "ADDRESS",

            "STORE_F",
            "STORE_V",
            "STORE_S",
            "STORE_ENT",
            "STORE_FLD",
            "STORE_FNC",

            "STOREP_F",
            "STOREP_V",
            "STOREP_S",
            "STOREP_ENT",
            "STOREP_FLD",
            "STOREP_FNC",

            "RETURN",

            "NOT_F",
            "NOT_V",
            "NOT_S",
            "NOT_ENT",
            "NOT_FNC",

            "IF",
            "IFNOT",

            "CALL0",
            "CALL1",
            "CALL2",
            "CALL3",
            "CALL4",
            "CALL5",
            "CALL6",
            "CALL7",
            "CALL8",

            "STATE",

            "GOTO",

            "AND",
            "OR",

            "BITAND",
            "BITOR"
        };

    public static void PR_PrintStatement(pr_comp_c.dstatement_t* s)
    {
        int i;

        if (s->op < sizeof(pr_opnames) / sizeof(pr_opnames[0]))
        {
            console_c.Con_Printf($"{pr_opnames[s->op]} ");
            i = common_c.Q_strlen(pr_opnames[s->op]);

            for (; i < 10; i++)
            {
                console_c.Con_Printf(" ");
            }
        }

        if (s->op == OP_IF || s->op == OP_IFNOT)
        {
            console_c.Con_Printf($"{*pr_edict_c.PR_GlobalString(s->a)} {s->b}");
        }
        else if (s->op == OP_GOTO)
        {
            console_c.Con_Printf($"branch {s->a}");
        }
        else if ((uint)(s->op - OP_STORE_F) < 6)
        {
            console_c.Con_Printf($"{*pr_edict_c.PR_GlobalString(s->a)}");
            console_c.Con_Printf($"{*pr_edict_c.PR_GlobalStringNoContents(s->b)}");
        }
        else
        {
            if (s->a != 0)
            {
                console_c.Con_Printf(pr_edict_c.PR_GlobalString(s->a));
            }

            if (s->b != 0)
            {
                console_c.Con_Printf(pr_edict_c.PR_GlobalString(s->b));
            }

            if (s->c != 0)
            {
                console_c.Con_Printf(pr_edict_c.PR_GlobalStringNoContents(s->c));
            }
        }

        console_c.Con_Printf("\n");
    }

    public static void PR_StackTrace()
    {
        pr_comp_c.dfunction_t* f;
        int i;

        if (pr_depth == 0)
        {
            console_c.Con_Printf("<NO STACK>\n");
            return;
        }

        pr_stack[pr_depth].f = pr_xfunction;

        for (i = pr_depth; i >= 0; i--)
        {
            f = pr_stack[i].f;

            if (f == null)
            {
                console_c.Con_Printf("<NO FUNCTION>\n");
            }
            else
            {
                console_c.Con_Printf($"{*pr_edict_c.pr_strings + f->s_file} : {*pr_edict_c.pr_strings + f->s_name}\n");
            }
        }
    }

    public static void PR_Profile_f()
    {
        pr_comp_c.dfunction_t* f, best;
        int max;
        int num;
        int i;

        num = 0;

        do
        {
            max = 0;
            best = null;

            for (i = 0; i < pr_edict_c.progs->numfunctions; i++)
            {
                f = &pr_edict_c.pr_functions[i];

                if (f->profile > max)
                {
                    max = f->profile;
                    best = f;
                }
            }

            if (best != null)
            {
                if (num < 10)
                {
                    console_c.Con_Printf($"{best->profile} {*pr_edict_c.pr_strings + best->s_name}");
                }

                num++;
                best->profile = 0;
            }
        } while (best != null);
    }

    public static void PR_RunError(char* error, params object[] args)
    {
        Console.WriteLine(error->ToString());

        PR_PrintStatement(pr_edict_c.pr_statements + pr_xstatement);
        PR_StackTrace();
        console_c.Con_Printf($"{*error}\n");

        pr_depth = 0;

        host_c.Host_Error("Program error");
    }

    public static void PR_RunError(string error, params object[] args)
    {
        PR_RunError(common_c.StringToChar(error));
    }

    public static int PR_EnterFunction(pr_comp_c.dfunction_t* f)
    {
        int i, j, c, o;

        pr_stack[pr_depth].s = pr_xstatement;
        pr_stack[pr_depth].f = pr_xfunction;
        pr_depth++;

        if (pr_depth >= MAX_STACK_DEPTH)
        {
            PR_RunError("stack overflow");
        }

        c = f->locals;

        if (localstack_used + c > LOCALSTACK_SIZE)
        {
            PR_RunError("PR_ExecuteProgram: locals stack overflow\n");
        }

        for (i = 0; i < c; i++)
        {
            localstack[localstack_used + i] = ((int*)pr_edict_c.pr_globals)[f->parm_start + i];
        }

        localstack_used += c;

        o = f->parm_start;

        for (i = 0; i < f->numparms; i++)
        {
            for (j = 0; j < f->parm_size[i]; j++)
            {
                ((int*)pr_edict_c.pr_globals)[o] = ((int*)pr_edict_c.pr_globals)[OFS_PARM0 + i * 3 + j];
                o++;
            }
        }

        pr_xfunction = f;
        return f->first_statement - 1;
    }

    public static int PR_LeaveFunction()
    {
        int i, c;

        if (pr_depth <= 0)
        {
            sys_win_c.Sys_Error("prog stack underflow");
        }

        c = pr_xfunction->locals;
        localstack_used -= c;

        if (localstack_used < 0)
        {
            PR_RunError("PR_ExecuteProgram: locals stack underflow\n");
        }

        for (i = 0; i < c; i++)
        {
            ((int*)pr_edict_c.pr_globals)[pr_xfunction->parm_start + i] = localstack[localstack_used + i];
        }

        pr_depth--;
        pr_xfunction = pr_stack[pr_depth].f;
        return pr_stack[pr_depth].s;
    }

    public static void PR_ExecuteProgram(Action fnum)
    {
        progs_c.eval_t* a, b, c;
        int s;
        pr_comp_c.dstatement_t* st;
        pr_comp_c.dfunction_t* f, newf;
        int runaway;
        int i;
        progs_c.edict_t* ed;
        int exitdepth;
        progs_c.eval_t* ptr;

        if (fnum == null || fnum >= pr_edict_c.progs->numfunctions)
        {
            if (pr_edict_c.pr_global_struct->self != 0)
            {
                pr_edict_c.ED_Print(progs_c.PROG_TO_EDICT(pr_edict_c.pr_global_struct->self));
            }

            host_c.Host_Error("PR_ExecuteProgram: null function");
        }

        f = &pr_edict_c.pr_functions[fnum];

        runaway = 100000;
        pr_trace = false;

        exitdepth = pr_depth;

        s = PR_EnterFunction(f);

        while (true)
        {
            s++;

            st = &pr_edict_c.pr_statements[s];
            a = (progs_c.eval_t*)&pr_edict_c.pr_globals[st->a];
            b = (progs_c.eval_t*)&pr_edict_c.pr_globals[st->b];
            c = (progs_c.eval_t*)&pr_edict_c.pr_globals[st->c];

            if (--runaway == 0)
            {
                PR_RunError("runaway loop error");
            }

            pr_xfunction->profile++;
            pr_xstatement = s;

            if (pr_trace)
            {
                PR_PrintStatement(st);
            }

            switch (st->op)
            {
                case OP_ADD_F:
                    c->_float = a->_float + b->_float;
                    break;

                case OP_ADD_V:
                    c->vector[0] = a->vector[0] + b->vector[0];
                    c->vector[1] = a->vector[1] + b->vector[1];
                    c->vector[2] = a->vector[2] + b->vector[2];
                    break;

                case OP_SUB_F:
                    c->_float = a->_float - b->_float;
                    break;

                case OP_SUB_V:
                    c->vector[0] = a->vector[0] - b->vector[0];
                    c->vector[1] = a->vector[1] - b->vector[1];
                    c->vector[2] = a->vector[2] - b->vector[2];
                    break;

                case OP_MUL_F:
                    c->_float = a->_float * b->_float;
                    break;

                case OP_MUL_V:
                    c->vector[0] = a->vector[0] * b->vector[0] + a->vector[1] * b->vector[1] + a->vector[2] * b->vector[2];
                    break;

                case OP_MUL_FV:
                    c->vector[0] = a->_float * b->vector[0];
                    c->vector[1] = a->_float * b->vector[1];
                    c->vector[2] = a->_float * b->vector[2];
                    break;

                case OP_MUL_VF:
                    c->vector[0] = b->_float * a->vector[0];
                    c->vector[1] = b->_float * a->vector[1];
                    c->vector[2] = b->_float * a->vector[2];
                    break;

                case OP_DIV_F:
                    c->_float = a->_float / b->_float;
                    break;

                case OP_BITAND:
                    c->_float = (int)a->_float & (int)b->_float;
                    break;

                case OP_BITOR:
                    c->_float = (int)a->_float | (int)b->_float;
                    break;

                case OP_GE:
                    c->_float = a->_float >= b->_float ? 1 : 0;
                    break;

                case OP_LE:
                    c->_float = a->_float <= b->_float ? 1 : 0;
                    break;

                case OP_GT:
                    c->_float = a->_float > b->_float ? 1 : 0;
                    break;

                case OP_LT:
                    c->_float = a->_float < b->_float ? 1 : 0;
                    break;

                //case OP_AND:
                //    c->_float = a->_float && b->_float ? 1 : 0;
                //    break;

                //case OP_OR:
                //    c->_float = a->_float || b->_float ? 1 : 0;
                //    break;

                case OP_NOT_F:
                    c->_float = a->_float * -1f;
                    break;

                case OP_NOT_V:
                    c->_float = (a->vector[0] * -1f) + (a->vector[1] * -1f) + (a->vector[2] * -1f);
                    break;

                //case OP_NOT_S:
                //    c->_float = a


            }
        }
    }
}