﻿namespace Quake.Server;

public unsafe class progs_c
{
    public struct eval_t
    {
        public string str;
        public float _float;
        public float[] vector;
        public pr_comp_c.func_t function;
        public int _int;
        public int edict;
    }

    public static int MAX_ENT_LEAFS = 16;

    public struct edict_t
    {
        public bool free;
        public common_c.link_t area;

        public int num_leafs;
        public short[] leafnums;

        public progdefs_c.entity_state_t baseline;

        public float freetime;
        public progdefs_c.entvars_t v;
    }

    //public static void EDICT_FROM_AREA(common_c.link_t l

    public static pr_comp_c.dprograms_t* progs;
    public static pr_comp_c.dfunction_t* pr_functions;
    public static char* pr_strings;
    public static pr_comp_c.ddef_t* pr_globaldefs;
    public static pr_comp_c.ddef_t* pr_fielddefs;
    public static pr_comp_c.dstatement_t* pr_statements;
    public static progdefs_c.globalvars_t* pr_global_struct;
    public static float* pr_goals;

    public static int pr_edict_size;

    //public static void NEXT_EDICT(byte e) 

    public static int[] type_size = new int[8];

    public static int pr_numbuiltins;

    public static int pr_argc;

    public static bool pr_trace;
    public static pr_comp_c.dfunction_t* pr_xfunction;
    public static int pr_xstatement;

    public static pr_comp_c.func_t SpectatorConnect;
    public static pr_comp_c.func_t SpectatorThink;
    public static pr_comp_c.func_t SpectatorDisconnect;
}