namespace Quake;

public unsafe class mpdosock_c
{
    public static char u_char;
    public static ushort u_short;
    public static uint u_int;
    public static ulong u_long;

    public static uint SOCKET;

    public struct sockaddr
    {
        public ushort sa_family;
        public char* sa_data;
    }
}