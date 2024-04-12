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

    public static bool FULL(queue q)
    {
        return (q.head = (q.tail - 1) & QUEUEMASK) == 0;
    }

    public static bool EMPTY(queue q)
    {
        q.tail = q.head;
        return true;
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
        public bool useModem;
        public bool modemInitialized;
        public bool modemRang;
        public bool modemConnected;
        public queue inputQueue;
        public queue outputQueue;
        public char* clear;
        public char* startup;
        public char* shutdown;
        public char* buffer;
        public net_c.PollProcedure poll;
        public double timestamp;
        public byte uartType;
        public byte irq;
        public byte baudBits;
        public byte lineControl;
        public byte portNumber;
        public char dialType;
        public char* name;
    }

    public static ComPort* portList = null;
    public static ComPort** handleToPort;

    //public static void ISR_8250(ComPort* p)
    //{
    //    byte source = 0;
    //    byte b;

    //    //disable();

    //    while ((source = inportb(p->uart + INTERRUPT_ID_REGISTER) & 0x07) != 1)
    //    {
    //        switch (source)
    //        {
    //            case IIR_RX_DATA_READY_INTERRUPT:
    //                b = inportb(p->uart + RECEIVE_BUFFER_REGISTER);

    //                if (!FULL(p->inputQueue))
    //                {

    //                }
    //        }
    //    }
    //}

    public static void TTY_GetComPortConfig(int portNumber, int* port, int* irq, int* baud, bool* useModem)
    {
        ComPort* p;

        p = handleToPort[portNumber];
        *port = p->uart;
        *irq = p->irq;
        *baud = 115200 / p->baudBits;
        *useModem = p->useModem;
    }

    public static void TTY_SetComPortConfig(int portNumber, int port, int irq, int baud, bool useModem)
    {
        ComPort* p;
        float temp;

        if (useModem)
        {
            if (baud == 14400)
            {
                baud = 19200;
            }

            if (baud == 28800)
            {
                baud = 38400;
            }
        }

        p = handleToPort[portNumber];
        p->uart = port;
        p->irq = (byte)irq;
        p->baudBits = (byte)(115200 / baud);
        p->useModem = useModem;

        if (useModem)
        {
            temp = 1.0f;
        }
        else
        {
            temp = 0.0f;
        }

        cvar_c.Cvar_SetValue("_config_com_port", (float)port);
        cvar_c.Cvar_SetValue("_config_com_irq", (float)irq);
        cvar_c.Cvar_SetValue("_config_com_baud", (float)baud);
        cvar_c.Cvar_SetValue("_config_com_modem", temp);
    }

    public static void TTY_GetModemConfig(int portNumber, char* dialType, char* clear, char* init, char* hangup)
    {
        ComPort* p;

        p = handleToPort[portNumber];
        *dialType = p->dialType;
        common_c.Q_strcpy(clear, p->clear);
        common_c.Q_strcpy(init, p->startup);
        common_c.Q_strcpy(hangup, p->shutdown);
    }

    public static void TTY_SetModemConfig(int portNumber, char* dialType, char* clear, char* init, char* hangup)
    {
        ComPort* p;

        p = handleToPort[portNumber];
        p->dialType = dialType[0];
        common_c.Q_strcpy(p->clear, clear);
        common_c.Q_strcpy(p->startup, init);
        common_c.Q_strcpy(p->shutdown, hangup);

        p->modemInitialized = false;

        cvar_c.Cvar_SetValue("_config_modem_dialtype", *dialType);
        cvar_c.Cvar_SetValue("_config_modem_clear", *clear);
        cvar_c.Cvar_SetValue("_config_modem_init", *init);
        cvar_c.Cvar_SetValue("_config_modem_hangup", *hangup);
    }

    public static void ResetComPortConfig(ComPort* p)
    {
        p->useModem = false;
        p->uartType = UART_AUTO;
        p->uart = ISA_uarts[p->portNumber];
        p->irq = (byte)ISA_IRQs[p->portNumber];
        p->modemStatusIgnore = MSR_CD | MSR_CTS | MSR_DSR;
        p->baudBits = 115200 / 57600;
        p->lineControl = LCR_DATA_BITS_8 | LCR_STOP_BITS_1 | LCR_PARITY_NONE;
        common_c.Q_strcpy(p->clear, "ATZ");
        common_c.Q_strcpy(p->startup, "");
        common_c.Q_strcpy(p->shutdown, "AT H");
        p->modemRang = false;
        p->modemConnected = false;
        p->statusUpdated = false;
        p->outputQueue.head = p->outputQueue.tail = 0;
        p->inputQueue.head = p->inputQueue.tail = 0;
    }

    public static void ComPort_Enable(ComPort* p)
    {
        int n;
        byte b;

        if (p->enabled)
        {
            console_c.Con_Printf("Already enabled!\n");
            return;
        }

        outportb(p->uart + INTERRUPT_ENABLE_REGISTER, 0);

        while (((inportb(p->uart + LINE_STATUS_REGISTER)) & LSR_DATA_READY) != 0)
        {
            inportb(p->uart + RECEIVE_BUFFER_REGISTER);
        }

        p->modemStatus = (inportb(p->uart + MODEM_STATUS_REGISTER) & MODEM_STATUS_REGISTER) | p->modemStatusIgnore;
        p->lineStatus = inportb(p->uart + LINE_STATUS_REGISTER);

        do
        {
            n = inportb(p->uart + INTERRUPT_ID_REGISTER) & 7;

            if (n == IIR_RX_DATA_READY_INTERRUPT)
            {
                inportb(p->uart + RECEIVE_BUFFER_REGISTER);
            }
        } while ((n & 1) == 0);

        if (p->uartType == UART_AUTO)
        {
            outportb(p->uart + FIFO_CONTROL_REGISTER, FCR_FIFO_ENABLE);
            b = inportb(p->uart + INTERRUPT_ID_REGISTER);

            if ((b & IIR_FIFO_ENABLED) == IIR_FIFO_ENABLED)
            {
                p->uartType = UART_16550;
            }
            else
            {
                p->uartType = UART_8250;
            }
        }

        //_go32_dpmi_get_protected_mode_interrupt_vector(p->irq + 8, &p->protectedModeSaveInfo);

        if (p->uartType == UART_8250)
        {
            outportb(p->uart + FIFO_CONTROL_REGISTER, 0);

            if (p == &handleToPort[0])
            {
                isr = COM1;
            }
        }
    }

    public static int CheckStatus(ComPort* p)
    {
        int ret = 0;

        if (p->statusUpdated)
        {
            p->statusUpdated = false;

            if ((p->lineStatus & (LSR_OVERRUN_ERROR | LSR_PARITY_ERROR | LSR_FRAMING_ERROR | LSR_BREAK_DETECT)) != 0)
            {
                if ((p->lineStatus & LSR_OVERRUN_ERROR) != 0)
                {
                    console_c.Con_DPrintf("Serial overrun error\n");
                }

                if ((p->lineStatus & LSR_PARITY_ERROR) != 0)
                {
                    console_c.Con_DPrintf("Serial parity error\n");
                }

                if ((p->lineStatus & LSR_FRAMING_ERROR) != 0)
                {
                    console_c.Con_DPrintf("Serial framing error\n");
                }

                if ((p->lineStatus & LSR_BREAK_DETECT) != 0)
                {
                    console_c.Con_DPrintf("Serial break detected\n");
                }

                ret = ERR_TTY_LINE_STATUS;
            }

            if ((p->modemStatus & MODEM_STATUS_MASK) != MODEM_STATUS_MASK)
            {
                if ((p->modemStatus & MSR_CTS) == 0)
                {
                    console_c.Con_Printf("Serial lost CTS\n");
                }

                if ((p->modemStatus & MSR_DSR) == 0)
                {
                    console_c.Con_Printf("Serial lost DSR\n");
                }

                if ((p->modemStatus & MSR_CD) == 0)
                {
                    console_c.Con_Printf("Serial lost Carrier\n");
                }

                ret = ERR_TTY_MODEM_STATUS;
            }
        }

        return ret;
    }

    public static void Modem_Init(ComPort* p)
    {
        double start;
        char* response;

        console_c.Con_Printf("Initalizing modem...\n");

        outportb(p->uart + MODEM_CONTROL_REGISTER, 0);
    }

    public static void TTY_Enable(int handle)
    {
        ComPort* p;

        p = handleToPort[handle];

        if (p->enabled)
        {
            return;
        }

        ComPort_Enable(p);

        if (p->useModem && !p->modemInitialized)
        {
            Modem_Init(p);
        }
    }

    public static int TTY_Open(int serialPortNumber)
    {
        return serialPortNumber;
    }

    public static void TTY_Close(int handle)
    {
        ComPort* p;
        double startTime;

        p = handleToPort[handle];

        startTime = sys_win_c.Sys_FloatTime();

        while ((sys_win_c.Sys_FloatTime() - startTime) < 1.0)
        {
            if (EMPTY(p->outputQueue))
            {
                break;
            }
        }

        if (p->useModem)
        {
            if (p->modemConnected)
            {
                Modem_Hangup(p);
            }
        }
    }

    public static int TTY_ReadByte(int handle)
    {
        int ret;
        ComPort* p;

        p = handleToPort[handle];

        if ((ret = CheckStatus(p)) != 0)
        {
            return ret;
        }

        if (EMPTY(p->inputQueue))
        {
            return ERR_TTY_NODATA;
        }

        DEQUEUE(p->inputQueue, (byte)ret);
        return ret & 0xff;
    }

    public static int TTY_WriteByte(int handle, byte data)
    {
        ComPort* p;

        p = handleToPort[handle];

        if (FULL(p->outputQueue))
        {
            return -1;
        }

        ENQUEUE(p->outputQueue, data);
        return 0;
    }

    public static void TTY_Flush(int handle)
    {
        byte b;
        ComPort* p;

        p = handleToPort[handle];

        if ((inportb(p->uart + LINE_STATUS_REGISTER) & LSR_TRANSMITTER_EMPTY) != 0)
        {
            DEQUEUE(p->outputQueue, b);
            outportb(p->uart, b);
        }
    }

    public static int TTY_Connect(int handle, char* host)
    {
        double start;
        ComPort* p;
        char* response = null;
        keys_c.keydest_t save_key_dest;
        byte* dialstring;
        byte b = 0;

        p = handleToPort[handle];

        if ((p->modemStatus & MODEM_STATUS_MASK) != MODEM_STATUS_MASK)
        {
            console_c.Con_Printf("Serial: line not ready (");

            if ((p->modemStatus & MSR_CTS) == 0)
            {
                console_c.Con_Printf(" CTS");
            }

            if ((p->modemStatus & MSR_DSR) == 0)
            {
                console_c.Con_Printf(" DSR");
            }

            if ((p->modemStatus & MSR_CD) == 0)
            {
                console_c.Con_Printf(" CD");
            }

            console_c.Con_Printf(" )");
            return -1;
        }

        while (!EMPTY(p->inputQueue))
        {
            DEQUEUE(p->inputQueue, b);
        }

        CheckStatus(p);

        if (p->useModem)
        {
            save_key_dest = keys_c.key_dest;
            keys_c.key_dest = keys_c.keydest_t.key_console;
            keys_c.key_count = -2;

            console_c.Con_Printf("Dialing...\n");
            Console.WriteLine($"AT D{p->dialType} {*host}\n");
            Modem_Command(p, dialstring);
            start = sys_win_c.Sys_FloatTime();

            while (true)
            {
                if ((sys_win_c.Sys_FloatTime() - start) > 60)
                {
                    console_c.Con_Printf("Dialing failure!\n");
                    break;
                }

                sys_win_c.Sys_SendKeyEvents();

                if (keys_c.key_count == 0)
                {
                    if (keys_c.key_lastpress != keys_c.K_ESCAPE)
                    {
                        keys_c.key_count = -2;
                        continue;
                    }

                    console_c.Con_Printf("Aborting...\n");

                    while ((sys_win_c.Sys_FloatTime() - start) < 5.0)
                    {
                        ;
                    }

                    disable();
                    p->outputQueue.head = p->outputQueue.tail = 0;
                    p->inputQueue.head = p->inputQueue.tail = 0;
                    outportb(p->uart + MODEM_CONTROL_REGISTER, inportb(p->uart + MODEM_CONTROL_REGISTER) & ~MCR_DTR);
                    enable();
                    start = sys_win_c.Sys_FloatTime();
                    
                    while ((sys_win_c.Sys_FloatTime() - start) < 0.75)
                    {
                        ;
                    }

                    outportb(p->uart + MODEM_CONTROL_REGISTER, inportb(p->uart + MODEM_CONTROL_REGISTER) | MCR_DTR);
                    response = common_c.StringToChar("Aborted");
                    break;
                }

                response = Modem_Response(p);

                if (response == null)
                {
                    continue;
                }

                if (common_c.Q_strncmp(response, "CONNECT", 7) == 0)
                {
                    disable();
                    p->modemRang = true;
                    p->modemConnected = true;
                    p->outputQueue.head = p->outputQueue.tail = 0;
                    p->inputQueue.head = p->inputQueue.tail = 0;
                    enable();
                    keys_c.key_dest = save_key_dest;
                    keys_c.key_count = 0;
                    m_return_onerror = false;
                    return 0;
                }

                if (common_c.Q_strncmp(response, "NO CARRIER", 10) == 0)
                {
                    break;
                }

                if (common_c.Q_strncmp(response, "NO DIALTONE", 11) == 0)
                {
                    break;
                }

                if (common_c.Q_strncmp(response, "NO DIAL TONE", 12) == 0)
                {
                    break;
                }

                if (common_c.Q_strncmp(response, "NO ANSWER", 9) == 0)
                {
                    break;
                }

                if (common_c.Q_strncmp(response, "BUSY", 4) == 0)
                {
                    break;
                }

                if (common_c.Q_strncmp(response, "ERROR", 5) == 0)
                {
                    break;
                }
            }

            keys_c.key_dest = save_key_dest;
            keys_c.key_count = 0;

            if (m_return_onerror)
            {
                keys_c.key_dest = keys_c.keydest_t.key_menu;
                m_state = m_return_state;
                m_return_onerror = false;
                common_c.Q_strncpy(m_return_reason, response, 31);
            }

            return -1;
        }

        m_return_onerror = false;
        return 0;
    }

    public static void TTY_Disconnect(int handle)
    {
        ComPort* p;

        p = handleToPort[handle];

        if (p->useModem && p->modemConnected)
        {
            Modem_Hangup(p);
        }
    }

    public static bool TTY_CheckForConnection(int handle)
    {
        ComPort* p;

        p = handleToPort[handle];

        CheckStatus(p);

        if (p->useModem)
        {
            if (!p->modemRang)
            {
                if (!Modem_Response(p))
                {
                    return false;
                }

                if (common_c.Q_strncmp(p->buffer, "RING", 4) == 0)
                {
                    Modem_Command(p, "ATA");
                    p->modemRang = true;
                    p->timestamp = net_c.net_time;
                }

                return false;
            }

            if (!p->modemConnected)
            {
                if ((net_c.net_time - p->timestamp) > 35.0)
                {
                    console_c.Con_Printf("Unable to establish modem connection\n");
                    p->modemRang = false;
                    return false;
                }

                if (!Modem_Response(p))
                {
                    return false;
                }

                if (common_c.Q_strncmp(p->buffer, "CONNECT", 7) != 0)
                {
                    return false;
                }

                disable();
                p->modemConnected = true;
                p->outputQueue.head = p->outputQueue.tail = 0;
                p->inputQueue.head = p->inputQueue.tail = 0;
                enable();
                console_c.Con_Printf("Modem Connect\n");
                return true;
            }

            return true;
        }

        if (EMPTY(p->inputQueue))
        {
            return false;
        }

        return true;
    }

    public static bool TTY_IsEnabled(int serialPortNumber)
    {
        return handleToPort[serialPortNumber]->enabled;
    }

    public static bool TTY_IsModem(int serialPortNumber)
    {
        return handleToPort[serialPortNumber]->useModem;
    }

    public static bool TTY_OutputQueueIsEmpty(int handle)
    {
        return EMPTY(handleToPort[handle]->outputQueue);
    }

    public static void Com_f()
    {
        ComPort* p;
        int portNumber;
        int i;
        int n;

        portNumber = common_c.Q_atoi(cmd_c.Cmd_Argv(0) + 3) - 1;

        if (portNumber > 1)
        {
            return;
        }

        p = handleToPort[portNumber];

        if (cmd_c.Cmd_Argc() == 1)
        {
            console_c.Con_Printf($"Settings for COM{portNumber + 1}\n");
            console_c.Con_Printf($"enabled:     {p->enabled}\n");
            console_c.Con_Printf("uart:         ");

            if (p->uartType == UART_AUTO)
            {
                console_c.Con_Printf("auto\n");
            }
            else if (p->uartType == UART_8250)
            {
                console_c.Con_Printf("8250\n");
            }
            else
            {
                console_c.Con_Printf("16550\n");
            }

            console_c.Con_Printf($"port:        {p->uart}\n");
            console_c.Con_Printf($"irq:         {p->irq}\n");
            console_c.Con_Printf($"baud:        {115200 / p->baudBits}\n");
            console_c.Con_Printf("CTS:         " + ((p->modemStatusIgnore & MSR_CTS) == 0 ? "ignored" : "honored") + "\n");
            console_c.Con_Printf("DSR:         " + ((p->modemStatusIgnore & MSR_DSR) == 0 ? "ignored" : "honored") + "\n");
            console_c.Con_Printf("CD:          " + ((p->modemStatusIgnore & MSR_CD) == 0 ? "ignored" : "honored") + "\n");

            if (p->useModem)
            {
                console_c.Con_Printf("type:     Modem\n");
                console_c.Con_Printf($"clear:   {*p->clear}\n");
                console_c.Con_Printf($"startup:     {*p->startup}\n");
                console_c.Con_Printf($"shutdown:    {*p->shutdown}\n");
            }
            else
            {
                console_c.Con_Printf("type:     Direct connect\n");
            }

            return;
        }

        if (cmd_c.Cmd_CheckParm("disable") != 0)
        {
            if (p->enabled)
            {
                ComPort_Disable(p);
            }

            p->modemInitialized = false;
            return;
        }

        if (cmd_c.Cmd_CheckParm("reset") != 0)
        {
            ComPort_Disable(p);
            ResetComPortConfig(p);
            return;
        }

        if ((i = cmd_c.Cmd_CheckParm("port")) != 0)
        {
            if (p->enabled)
            {
                console_c.Con_Printf("COM port must be disabled to change port\n");
                return;
            }

            p->uart = common_c.Q_atoi(cmd_c.Cmd_Argv(i + 1));
        }

        if ((i = cmd_c.Cmd_CheckParm("irq")) != 0)
        {
            if (p->enabled)
            {
                console_c.Con_Printf("COM port must be disabled to change irq\n");
                return;
            }

            p->irq = (byte)common_c.Q_atoi(cmd_c.Cmd_Argv(i + 1));
        }

        if ((i = cmd_c.Cmd_CheckParm("baud")) != 0)
        {
            if (p->enabled)
            {
                console_c.Con_Printf("COM port must be disabled to change baud\n");
                return;
            }

            n = common_c.Q_atoi(cmd_c.Cmd_Argv(i + 1));

            if (n == 0)
            {
                console_c.Con_Printf("Invalid baud rate specified\n");
            }
            else
            {
                p->baudBits = (byte)(115200 / n);
            }
        }

        if (cmd_c.Cmd_CheckParm("8250") != 0)
        {
            if (p->enabled)
            {
                console_c.Con_Printf("COM port must be disabled to change uart\n");
                return;
            }

            p->uartType = UART_8250;
        }

        if (cmd_c.Cmd_CheckParm("16550") != 0)
        {
            if (p->enabled)
            {
                console_c.Con_Printf("COM port must be disabled to change uart\n");
                return;
            }

            p->uartType = UART_16550;
        }

        if (cmd_c.Cmd_CheckParm("auto") != 0)
        {
            if (p->enabled)
            {
                console_c.Con_Printf("COM port must be disabled to change uart\n");
                return;
            }

            p->uartType = UART_AUTO;
        }

        if (cmd_c.Cmd_CheckParm("pulse") != 0)
        {
            p->dialType = 'P';
        }

        if (cmd_c.Cmd_CheckParm("tone") != 0)
        {
            p->dialType = 'T';
        }

        if (cmd_c.Cmd_CheckParm("direct") != 0)
        {
            p->useModem = false;
        }

        if (cmd_c.Cmd_CheckParm("modem") != 0)
        {
            p->useModem = true;
        }

        if ((i = cmd_c.Cmd_CheckParm("clear")) != 0)
        {
            common_c.Q_strncpy(p->clear, cmd_c.Cmd_Argv(i + 1), 16);
        }

        if ((i = cmd_c.Cmd_CheckParm("startup")) != 0)
        {
            common_c.Q_strncpy(p->startup, cmd_c.Cmd_Argv(i + 1), 32);
            p->modemInitialized = false;
        }

        if ((i = cmd_c.Cmd_CheckParm("shutdown")) != 0)
        {
            common_c.Q_strncpy(p->shutdown, cmd_c.Cmd_Argv(i + 1), 16);
        }

        if (cmd_c.Cmd_CheckParm("-cts") != 0)
        {
            p->modemStatusIgnore |= MSR_CTS;
            p->modemStatus |= MSR_CTS;
        }

        if (cmd_c.Cmd_CheckParm("+cts") != 0)
        {
            p->modemStatusIgnore &= MSR_CTS;
            p->modemStatus = (inportb(p->uart + MODEM_CONTROL_REGISTER) & MODEM_STATUS_MASK) | p->modemStatusIgnore;
        }

        if (cmd_c.Cmd_CheckParm("-dsr") != 0)
        {
            p->modemStatusIgnore |= MSR_DSR;
            p->modemStatus |= MSR_DSR;
        }

        if (cmd_c.Cmd_CheckParm("+dsr") != 0)
        {
            p->modemStatusIgnore &= MSR_DSR;
            p->modemStatus = (inportb(p->uart + MODEM_STATUS_REGISTER) & MODEM_STATUS_MASK) | p->modemStatusIgnore;
        }

        if (cmd_c.Cmd_CheckParm("-cd") != 0)
        {
            p->modemStatusIgnore |= MSR_CD;
            p->modemStatus |= MSR_CD;
        }

        if (cmd_c.Cmd_CheckParm("+cd") != 0)
        {
            p->modemStatusIgnore &= MSR_CD;
            p->modemStatus = (inportb(p->uart + MODEM_STATUS_REGISTER) & MODEM_STATUS_MASK) | p->modemStatusIgnore;
        }

        if (cmd_c.Cmd_CheckParm("enable") != 0)
        {
            if (!p->enabled)
            {
                ComPort_Enable(p);
            }

            if (p->useModem && !p->modemInitialized)
            {
                Modem_Init(p);
            }
        }
    }

    public static int TTY_Init()
    {
        int n;
        ComPort* p;

        for (n = 0; n < NUM_COM_PORTS; n++)
        {
            p = (ComPort*)zone_c.Hunk_AllocName(sizeof(ComPort), "comport");

            if (p == null)
            {
                sys_win_c.Sys_Error("Hunk alloc failed for com port\n");
            }

            p->next = portList;
            portList = p;
            handleToPort[n] = p;
            p->portNumber = (byte)n;
            p->dialType = 'T';
            Console.WriteLine($"com{n+1}");
            cmd_c.Cmd_AddCommand(p->name, Com_f);
            ResetComPortConfig(p);
        }

        net_main_c.GetComPortConfig = TTY_GetComPortConfig;
        net_main_c.SetComPortConfig = TTY_SetComPortConfig;
    }
}