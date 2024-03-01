namespace Quake;

public unsafe class vregset_c
{
    public const int MISC_OUTPUT = 0x3C2;

    public const int SC_INDEX = 0x3C4;
    public const int SC_DATA = 0x3C5;
    public const int SYNC_RESET = 0;
    public const int MAP_MASK = 2;
    public const int MEMORY_MODE = 4;

    public const int GC_INDEX = 0x3CE;
    public const int GC_DATA = 0x3CF;
    public const int READ_MAP = 4;
    public const int GRAPHICS_MODE = 5;
    public const int MISCELLANOUS = 6;

    public const int CRTC_INDEX = 0x3D4;
    public const int CRTC_DATA = 0x3D5;
    public const int MAX_SCAN_LINE = 9;
    public const int UNDERLINE = 0x14;
    public const int MODE_CONTROL = 0x17;

    public const int VRS_END = 0;
    public const int VRS_BYTE_OUT = 1;
    public const int VRS_BYTE_RW = 2;
    public const int VRS_WORD_OUT = 3;

    public static void loutportb(int port, int val)
    {
        Console.WriteLine($"port, val: {port} {val}\n");
        //getch(); // Guh??
    }

    public static void VideoRegisterSet(int* pregset)
    {
        int port, temp0, temp1, temp2;

        for (; ; )
        {
            switch (*pregset++)
            {
                case VRS_END:
                    return;

                case VRS_BYTE_OUT:
                    port = *pregset++;
                    outportb(port, *pregset++);
                    break;

                case VRS_BYTE_RW:
                    port = *pregset++;
                    temp0 = *pregset++;
                    temp1 = *pregset++;
                    temp2 = inportb(port);
                    temp2 &= temp0;
                    temp2 |= temp1;
                    outportb(port, temp2);
                    break;

                case VRS_WORD_OUT:
                    port = *pregset++;
                    outportb(port, *pregset & 0xFF);
                    outportb(port + 1, *pregset >> 8);
                    pregset++;
                    break;

                default:
                    sys_win_c.Sys_Error("VideoRegisterSet: Invalid command\n");
                    break;
            }
        }
    }
}