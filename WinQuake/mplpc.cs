namespace Quake;

public unsafe class mplpc_c
{
    public struct LPCData
    {
        public short version;
        public short sizeOfArgs;
        public short service;
        public char* Data;
    }

    public struct LPCReturn
    {
        public short version;
        public short sizeOfReturn;
        public short error;
        public short noRet;
        public char* Data;
    }

    public const int MAXSOCKETS = 20;

    public const int LPC_SOCKBIND = 4;
    public const int LPC_SOCKGETHOSTBYNAME = 5;
    public const int LPC_SOCKGETHOSTNAME = 6;
    public const int LPC_SOCKGETHOSTBYADDR = 7;
    public const int LPC_SOCKCLOSE = 8;
    public const int LPC_SOCKSOCKET = 9;
    public const int LPC_SOCKRECVFROM = 10;
    public const int LPC_SOCKSENDTO = 11;
    public const int LPC_SOCKIOCTL = 12;
    public const int LPC_SOCKGETSOCKNAME = 13;
    public const int LPC_SOCKFLUSH = 14;
    public const int LPC_SOCKSETOPT = 15;
    public const int LPC_SOCKGETLASTERROR = 16;
    public const int LPC_SOCKINETADDR = 17;

    public const int LPC_UNRECOGNIZED_SERVICE = -1;
    public const int LPC_NOERROR = 0;

    public struct BindArgs
    {
        public uint s;
        public int namelen;
        public char* name;
    }

    public struct IoctlArgs
    {
        public uint s;
        public int namelen;
        public char* name;
    }

    public struct GetSockNameRet
    {
        public int retVal;
        public int namelen;
        public char* name;
    }

    public static GetSockNameRet GetHostNameRet;

    public struct GetHostByNameRet
    {
        public int retVal;
        public int h_addr_0;
    }

    public struct GetHostByAddrArgs
    {
        public int len;
        public int type;
        public char* addr;
    }

    public struct GetHostByAddrRet
    {
        public int retVal;
        public char* h_name;
    }

    public struct RecvFromArgs
    {
        public uint s;
        public int flags;
    }

    public struct RecvFromRet
    {
        public int retVal;
        public int errCode;
        public int len;
        public mpdosock_c.sockaddr sockaddr;
        public int sockaddrlen;
        public char* Data;
    }

    public struct SendToArgs
    {
        public uint s;
        public int flags;
        public int len;
        public mpdosock_c.sockaddr sockaddr;
        public int sockaddrlen;
        public char* Data;
    }

    public struct SendToRet
    {
        public int retVal;
        public int errCode;
    }

    public struct SocketChannelData
    {
        public int bufflen;
        public uint s;
        public int len;
        public int sockaddrlen;
        public mpdosock_c.sockaddr address;
        public char* data;
    }

    public struct SocketArgs
    {
        public int af;
        public int type;
        public int protocol;
    }

    public struct WinSockData
    {
        public uint s;
        public int len;
        public int flags;
        public int addrlen;
        public mpdosock_c.sockaddr addr;
        public char* data;
    }

    public struct SetSockOptArgs
    {
        public uint s;
        public int level;
        public int optname;
        public int optlen;
        public char* optval;
    }

    public struct SocketMap
    {
        public uint* sock;
    }
}