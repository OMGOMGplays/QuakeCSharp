namespace Quake;

public unsafe class net_comx_c
{
    public const int NUM_COM_PORTS = 2;

    public const int ERR_TTY_LINE_STATUS = -1;
    public const int ERR_TTY_MODEM_STATUS = -2;
    public const int ERR_TTY_NODATA = -3;

    public const int QUEUESIZE = 8192;
    public const int QUEUEMASK = QUEUESIZE - 1;

    public struct queue
    {
        public volatile int head;
        public volatile int tail;
        public volatile byte* data;
    }

    public static void FULL(queue q)
    {
        q.head = ((q.tail - 1) & QUEUEMASK);
    }

    public static void EMPTY(queue q)
    {
        q.tail = q.head;
    }

    public static void ENQUEUE(queue q, byte b)
    {
        q.data[q.head] = b;
        q.head = (q.head + 1) & QUEUEMASK;
    }

    public static void DEQUEUE(queue q, byte b)
    {
        b = q.data[q.tail];
        q.tail = (q.tail + 1) & QUEUEMASK;
    }

    public const int TRANSMIT_HOLDING_REGISTER = 0x00;
    public const int RECEIVE_BUFFER_REGISTER = 0x00;
    public const int INTERRUPT_ENABLE_REGISTER = 0x01;
    public const int IER_RX_DATA_READY = 0x01;
    public const int IER_TX_HOLDING_REGISTER_EMPTY = 0x02;
    public const int IER_LINE_STATUS = 0x04;
    public const int IER_MODEM_STATUS = 0x08;
    public const int INTERRUPT_ID_REGISTER = 0x02;
    public const int IIR_MODEM_STATUS_INTERRUPT = 0x00;
    public const int IIR_TX_HOLDING_REGISTER_INTERRUPT = 0x02;
    public const int IIR_RX_DATA_READY_INTERRUPT = 0x04;
    public const int IIR_LINE_STATUS_TIMEOUT = 0x06;
    public const int IIR_FIFO_TIMEOUT = 0x0c;
    public const int IIR_FIFO_ENABLED = 0xc0;
    public const int FIFO_CONTROL_REGISTER = 0x02;
    public const int FCR_FIFO_ENABLE = 0x01;
    public const int FCR_RCVR_FIFO_RESET = 0x02;
    public const int FCR_XMIT_FIFO_RESET = 0x04;
    public const int FCR_TRIGGER_01 = 0x00;
    public const int FCR_TRIGGER_04 = 0x40;
    public const int FCR_TRIGGER_08 = 0x80;
    public const int FCR_TRIGGER_16 = 0xc0;
    public const int LINE_CONTROL_REGISTER = 0x03;
    public const int LCR_DATA_BITS_5 = 0x00;
    public const int LCR_DATA_BITS_6 = 0x01;
    public const int LCR_DATA_BITS_7 = 0x02;
    public const int LCR_DATA_BITS_8 = 0x03;
    public const int LCR_STOP_BITS_1 = 0x00;
    public const int LCR_STOP_BITS_2 = 0x04;
    public const int LCR_PARITY_NONE = 0x00;
    public const int LCR_PARITY_ODD = 0x08;
    public const int LCR_PARITY_EVEN = 0x18;
    public const int LCR_PARITY_MARK = 0x28;
    public const int LCR_PARITY_SPACE = 0x38;
    public const int LCR_SET_BREAK = 0x40;
    public const int LCR_DLAB = 0x80;
    public const int MODEM_CONTROL_REGISTER = 0x04;
    public const int MCR_DTR = 0x01;
    public const int MCR_RTS = 0x02;
    public const int MCR_OUT1 = 0x04;
    public const int MCR_OUT2 = 0x08;
    public const int MCR_LOOPBACK = 0x10;
    public const int LINE_STATUS_REGISTER = 0x05;
    public const int LSR_DATA_READY = 0x01;
    public const int LSR_OVERRUN_ERROR = 0x02;
    public const int LSR_PARITY_ERROR = 0x04;
    public const int LSR_FRAMING_ERROR = 0x08;
    public const int LSR_BREAK_DETECT = 0x10;
    public const int LSR_TRANSMITTER_BUFFER_EMPTY = 0x20;
    public const int LSR_TRANSMITTER_EMPTY = 0x40;
    public const int LSR_FIFO_DIRTY = 0x80;
    public const int MODEM_STATUS_REGISTER = 0x06;
    public const int MSR_DELTA_CTS = 0x01;
    public const int MSR_DELTA_DSR = 0x02;
    public const int MSR_DELTA_RI = 0x04;
    public const int MSR_DELTA_CD = 0x08;
    public const int MSR_CTS = 0x10;
    public const int MSR_DSR = 0x20;
    public const int MSR_RI = 0x40;
    public const int MSR_CD = 0x80;
    public const int DIVISOR_LATCH_LOW = 0x00;
    public const int DIVISOR_LATCH_HIGH = 0x01;

    public const int MODEM_STATUS_MASK = MSR_CTS | MSR_DSR | MSR_CD;

    public const int UART_AUTO = 0;
    public const int UART_8250 = 1;
    public const int UART_16550 = 2;

    public static int[] ISA_uarts = { 0x3f8, 0x2f8, 0x3e8, 0x2e8 };
    public static int[] ISA_IRQs = { 4, 3, 4, 3 };

    public struct ComPort
    {
        public ComPort* next;
        public int uart;
        public volatile byte modemStatus;
        public byte modemStatusIgnore;
        public byte lineStatus;
        public byte bufferUsed;
        public bool enabled;
        public volatile bool statusUpdated;
    }
}