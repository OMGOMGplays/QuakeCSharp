namespace Quake;

public unsafe class pr_comp_c
{
    public enum etype_t { ev_void, ev_string, ev_float, ev_vector, ev_entity, ev_field, ev_function, ev_pointer }

    public static int OFS_NULL = 0;
    public static int OFS_RETURN = 1;
    public static int OFS_PARM0 = 4;
    public static int OFS_PARM1 = 7;
    public static int OFS_PARM2 = 10;
    public static int OFS_PARM3 = 13;
    public static int OFS_PARM4 = 16;
    public static int OFS_PARM5 = 19;
    public static int OFS_PARM6 = 22;
    public static int OFS_PARM7 = 25;
    public static int RESERVED_OFS = 28;

    public enum operation
    {
        OP_DONE,
        OP_MUL_F,
        OP_MUL_V,
        OP_MUL_FV,
        OP_MUL_VF,
        OP_DIV_F,
        OP_ADD_F,
        OP_ADD_V,
        OP_SUB_F,
        OP_SUB_V,

        OP_EQ_F,
        OP_EQ_V,
        OP_EQ_S,
        OP_EQ_E,
        OP_EQ_FNC,

        OP_NE_F,
        OP_NE_V,
        OP_NE_S,
        OP_NE_E,
        OP_NE_FNC,

        OP_LE,
        OP_GE,
        OP_LT,
        OP_GT,

        OP_LOAD_F,
        OP_LOAD_V,
        OP_LOAD_S,
        OP_LOAD_ENT,
        OP_LOAD_FLD,
        OP_LOAD_FNC,

        OP_ADDRESS,

        OP_STORE_F,
        OP_STORE_V,
        OP_STORE_S,
        OP_STORE_ENT,
        OP_STORE_FLD,
        OP_STORE_FNC,

        OP_STOREP_F,
        OP_STOREP_V,
        OP_STOREP_S,
        OP_STOREP_ENT,
        OP_STOREP_FLD,
        OP_STOREP_FNC,

        OP_RETURN,
        OP_NOT_F,
        OP_NOT_V,
        OP_NOT_S,
        OP_NOT_ENT,
        OP_NOT_FNC,
        OP_IF,
        OP_IFNOT,
        OP_CALL0,
        OP_CALL1,
        OP_CALL2,
        OP_CALL3,
        OP_CALL4,
        OP_CALL5,
        OP_CALL6,
        OP_CALL7,
        OP_CALL8,
        OP_STATE,
        OP_GOTO,
        OP_AND,
        OP_OR,

        OP_BITAND,
        OP_BITOR
    }

    public struct dstatement_t
    {
        public short op;
        public short a, b, c;
    }

    public struct ddef_t
    {
        public short type;

        public short ofs;
        public int s_name;
    }

    public static int DEF_SAVEGLOBAL = (1 << 15);

    public static int MAX_PARMS = 8;

    public struct dfunction_t
    {
        public int first_statement;
        public int parm_start;
        public int locals;

        public int profile;

        public int s_name;
        public int s_file;

        public int numparms;
        public byte* parm_size;
    }

    public static int PROG_VERSION = 6;

    public struct dprograms_t
    {
        public int version;
        public int crc;

        public int ofs_statements;
        public int numstatements;

        public int ofs_globaldefs;
        public int numglobaldefs;

        public int ofs_fielddefs;
        public int numfielddefs;

        public int ofs_functions;
        public int numfunctions;

        public int ofs_strings;
        public int numstrings;

        public int ofs_globals;
        public int numglobals;

        public int entityfields;
    }
}