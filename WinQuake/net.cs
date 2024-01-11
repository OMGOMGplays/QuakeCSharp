namespace Quake;

public unsafe class net_c
{
    public struct qsockaddr
    {
        public short sa_family;
        public char* sa_data;
    }

    public static int NET_NAMELEN = 64;

    public static int NET_MAXMESSAGE = 8192;
    public static int NET_HEADERSIZE = 2 * sizeof(uint);
    public static int NET_DATAGRAMSIZE = quakedef_c.MAX_DATAGRAM + NET_HEADERSIZE;

    public static int NETFLAG_LENGTH_MASK = 0x0000ffff;
    public static int NETFLAG_DATA = 0x00010000;
    public static int NETFLAG_ACK = 0x00020000;
    public static int NETFLAG_NAK = 0x00040000;
    public static int NETFLAG_EOM = 0x00080000;
    public static int NETFLAG_UNRELIABLE = 0x00100000;
    public static int NETFLAG_CTL = 0x8000000;

    public static int NET_PROTOCOL_VERSION = 3;

    public static int CCREQ_CONNECT = 0x01;
    public static int CCREQ_SERVER_INFO = 0x02;
    public static int CCREQ_PLAYER_INFO = 0x03;
    public static int CCREQ_RULE_INFO = 0x04;

    public static int CCREP_ACCEPT = 0x81;
    public static int CCREP_REJECT = 0x82;
    public static int CCREP_SERVER_INFO = 0x83;
    public static int CCREP_PLAYER_INFO = 0x84;
    public static int CCREP_RULE_INFO = 0x85;

    public struct qsocket_t
    {
        public qsocket_t* next;
        public double connecttime;
        public double lastMessageTime;
        public double lastSendTime;

        public bool disconnected;
        public bool canSend;
        public bool sendNext;

        public int driver;
        public int landriver;
        public int socket;
        public void* driverdata;

        public uint ackSequence;
        public uint sendSequence;
        public uint unreliableSendSequence;
        public int sendMessageLength;
        public byte* sendMessage;

        public uint receiveSequence;
        public uint unreliableReceiveSequence;
        public int receiveMessageLength;
        public byte* receiveMessage;

        public qsockaddr addr;
        public char* address;
    }

    public static qsocket_t* net_activeSockets;
    public static qsocket_t* net_freeSockets;
    public static int net_numsockets;

    public struct net_landriver_t
    {
        public char* name;
        public bool initialized;
        public int controlSock;
        public Func<int> Init { get; set; }
        public Action Shutdown { get; set; }
        public Action<bool> Listen { get; set; }
        public Func<int, int> OpenSocket { get; set; }
        public Func<int, int> CloseSocket { get; set; }
        public Func<int, qsockaddr, int> Connect { get; set; }
        public Func<int> CheckNewConnections { get; set; }
        public Func<int, byte[], int, qsockaddr, int> Read { get; set; }
        public Func<int, byte[], int, qsockaddr, int> Write { get; set; }
        public Func<int, byte[], int, int> Broadcast { get; set; }
        public Func<qsockaddr, char> AddrToString { get; set; }
        public Func<char, qsockaddr, int> StringToAddr { get; set; }
        public Func<int, qsockaddr, int> GetSocketAddr { get; set; }
        public Func<qsockaddr, char, int> GetNameFromAddr { get; set; }
        public Func<char, qsockaddr, int> GetAddrFromName { get; set; }
        public Func<qsockaddr, qsockaddr, int> AddrCompare { get; set; }
        public Func<qsockaddr, int> GetSocketPort { get; set; }
        public Func<qsockaddr, int, int> SetSocketPort { get; set; }
    }

    public static int MAX_NET_DRIVERS = 8;
    public static int net_numlandrivers;
    public static net_landriver_t* net_landrivers;

    public struct net_driver_t
    {
        public char* name;
        public bool initialized;
        public Func<int> Init { get; set; }
        public Action<bool> Listen { get; set; }
        public Action<bool> SearchForHosts { get; set; }
        public Func<char, qsocket_t> Connect { get; set; }
        public Func<qsocket_t> CheckNewConnections { get; set; }
        public Func<qsocket_t, int> QGetMessage { get; set; }
        public Func<qsocket_t, common_c.sizebuf_t, int> QSendMessage { get; set; }
        public Func<qsocket_t, common_c.sizebuf_t, int> SendUnreliableMessage { get; set; }
        public Func<qsocket_t, bool> CanSendMessage { get; set; }
        public Func<qsocket_t, bool> CanSendUnreliableMessage { get; set; }
        public Action<qsocket_t> Close { get; set; }
        public Action Shutdown { get; set; }
        public int controlSock;
    }

    public static int net_numdrivers;
    public static net_driver_t* net_drivers;

    public static int DEFAULTnet_hostport;
    public static int net_hostport;

    public static int net_driverlevel;
    public static cvar_c.cvar_t hostname;
    public static char* playername;
    public static int playercolor;

    public static int messagesSent;
    public static int messagesReceived;
    public static int unreliableMessagesSent;
    public static int unreliableMessagesReceived;

    public static int HOSTCACHESIZE = 8;

    public struct hostcache_t
    {
        public char* name;
        public char* map;
        public char* cname;
        public int users;
        public int maxusers;
        public int driver;
        public int ldriver;
        public qsockaddr addr;
    }

    public static int hostCacheCount;
    public static hostcache_t* hostcache;

    public static double net_time;
    public static common_c.sizebuf_t net_message;
    public static int net_activeconnections;

    public struct PollProcedure
    {
        public PollProcedure* next;
        public double nextTime;
        public Action procedure;
        public void* arg;
    }

    public static bool serialAvailable;
    public static bool ipxAvailable;
    public static bool tcpipAvailable;
    public static char* my_ipx_address;
    public static char* my_tcpip_address;

    public static bool slistInProgress;
    public static bool slistSilent;
    public static bool slistLocal;
}