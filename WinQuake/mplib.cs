namespace Quake;

public unsafe class mplib_c
{
    public static char BYTE;
    public static short WORD;
    public static long DWORD;

    public const int MGENVXD_REGISTER_ORD = 1;
    public const int MGENVXD_GETMEM_ORD = 2;
    public const int MGENVXD_DEREGISTER_ORD = 3;
    public const int MGENVXD_WAKEUP_ORD = 4;
    public const int MGENVXD_MAKEDQS_ORD = 5;

    public const int V86API_GETSELECTOR16_ORD = 1;
    public const int V86API_GETSELECTOR32_ORD = 2;
    public const int V86API_GETFLAT32_ORD = 3;
    public const int V86API_MOVERP_ORD = 6;
    public const int V86API_MOVEPR_ORD = 7;
    public const int V86API_POST_ORD = 8;
    public const int V86API_INIT_ORD = 9;
    public const int V86API_UNINIT_ORD = 10;
    public const int V86API_INSERTKEY_ORD = 11;
    public const int V86API_REMOVEHOTKEY_ORD = 12;
    public const int V86API_INSTALLHOTKEY_ORD = 13;
    public const int V86API_HOOKINT48_ORD = 14;
    public const int V86API_WAKEUPDLL_ORD = 15;

    public const int DPMIAPI_GETFLAT32_ORD = 1;
    public const int DPMIAPI_POST_WINDOWS_ORD = 2;
    public const int MGENVXD_GETQUEUECTR_ORD = 6;
    public const int MGENVXD_MOVENODE_ORD = 7;
    public const int MGENVXD_GETNODE_ORD = 8;
    public const int MGENVXD_FLUSHNODE_ORD = 9;
    public const int MGENVXD_MCOUNT_ORD = 10;
    public const int MGENVXD_MASTERNODE_ORD = 11;
    public const int MGENVXD_SANITYCHECK_ORD = 12;
    public const int MGENVXD_WAKEUPDLL_ORD = 13;
    public const int MGENVXD_WAIT_ORD = 14;

    public const int HWND_OFFSET = 0;
    public const int UMSG_OFFSET = 1;
    public const int SIZEREQUEST_OFFSET = 2;
    public const int HVXD_OFFSET = 3;
    public const int DATUM_OFFSET = 4;
    public const int SLOT_OFFSET = 5;
    public const int SIZEGIVEN_OFFSET = 6;
    public const int SELECTOR32_OFFSET = 7;
    public const int SELECTOR64_OFFSET = 8;

    public const int MGENVXD_DEVICE_ID = 0x18AA;

    public struct rtq_node
    {
        public rtq_node* self;
        public rtq_node* left;
        public rtq_node* right;
        public char* rtqDatum;
        public char* rtqInsert;
        public short rtqLen;
        public short rtqUpCtr;
        public short rtqQCtr;
        public short padding;
    }

    public static rtq_node RTQ_NODE;

    public struct rtq_param_movenode
    {
        public short rtqFromDQ;
        public short rtqToDQ;
    }

    public static rtq_param_movenode RTQ_PARAM_MOVENODE;

    public const int CHUNNEL_INT = 0x48;

    
}