namespace Quake;

public unsafe class net_ser_c
{
    public const int SERIAL_PROTOCOL_VERSION = 3;

    public const int MTYPE_RELIABLE = 0x01;
    public const int MTYPE_UNRELIABLE = 0x02;
    public const int MTYPE_CONTROL = 0x03;
    public const int MTYPE_ACK = 0x04;
    public const int MTYPE_CLIENT = 0x80;

    public const int ESCAPE_COMMAND = 0xe0;
    public const int ESCAPE_EOM = 0x19;

    public static bool listening = false;

    public struct SerialLine
    {
        public SerialLine* next;
        public net_c.qsocket_t* sock;
        public int lengthStated;
        public int lengthFound;
        public int tty;
        public bool connected;
        public bool connecting;
        public bool client;
        public double connect_time;
        public ushort crcStated;
        public ushort crcValue;
        public byte currState;
        public byte prevState;
        public byte mtype;
        public byte sequence;
    }

    public const int STATE_READY = 0;
    public const int STATE_SEQUENCE = 1;
    public const int STATE_LENGTH1 = 2;
    public const int STATE_LENGTH2 = 3;
    public const int STATE_DATA = 4;
    public const int STATE_CRC1 = 5;
    public const int STATE_CRC2 = 6;
    public const int STATE_EOM = 7;
    public const int STATE_ESCAPE = 8;
    public const int STATE_ABORT = 9;

    public static SerialLine serialLine[net_comx_c.NUM_COM_PORTS];

    public static int myDriverLevel;

    public static void ResetSerialLineProtocol(SerialLine* p)
    {
        p->connected = false;
        p->connecting = false;
        p->currState = STATE_READY;
        p->prevState = STATE_READY;
        p->lengthFound = 0;
    }

    public static int ProcessInQueue(SerialLine* p)
    {
        int b;

        while (true)
        {
            b = net_comx_c.TTY_ReadByte(p->tty);

            if (b == net_comx_c.ERR_TTY_NODATA)
            {
                break;
            }

            if (b == net_comx_c.ERR_TTY_LINE_STATUS)
            {
                p->currState = STATE_ABORT;
                continue;
            }

            if (b == net_comx_c.ERR_TTY_MODEM_STATUS)
            {
                p->currState = STATE_ABORT;
                return -1;
            }

            if (b == ESCAPE_COMMAND)
            {
                if (p->currState != STATE_ESCAPE)
                {
                    p->prevState = p->currState;
                    p->currState = STATE_ESCAPE;
                    continue;
                }
            }

            if (p->currState == STATE_ESCAPE)
            {
                if (b == ESCAPE_EOM)
                {
                    if (p->prevState == STATE_ABORT)
                    {
                        p->currState = STATE_READY;
                        p->lengthFound = 0;
                        continue;
                    }

                    if (p->prevState != STATE_EOM)
                    {
                        p->currState = STATE_READY;
                        p->lengthFound = 0;
                        console_c.Con_DPrintf("Serial: premautre EOM\n");
                        continue;
                    }

                    switch (p->mtype)
                    {
                        case MTYPE_RELIABLE:
                            console_c.Con_DPrintf($"Serial: sending ack {p->sequence}\n");
                            Serial_SendACK(p, p->sequence);

                            if (p->sequence == p->sock->receiveSequence)
                            {
                                p->sock->receiveSequence = (uint)(p->sequence + 1) & 0xff;
                                p->sock->receiveMessageLength += p->lengthFound;
                            }
                            else
                            {
                                console_c.Con_DPrintf($"Serial: reliable out of order; got {p->sequence} wanted {p->sock->receiveSequence}\n");
                            }
                            break;

                        case MTYPE_UNRELIABLE:
                            p->sock->unreliableReceiveSequence = (uint)(p->sequence + 1) & 0xff;
                            p->sock->receiveMessageLength += p->lengthFound;
                            break;

                        case MTYPE_ACK:
                            console_c.Con_DPrintf($"Serial: got ack {p->sequence}");

                            if (p->sequence == p->sock->sendSequence)
                            {
                                p->sock->sendSequence = (p->sock->sendSequence + 1) & 0xff;
                                p->sock->canSend = true;
                            }
                            else
                            {
                                console_c.Con_DPrintf($"Serial: ack out of order; got {p->sequence} wanted {p->sock->sendSequence}\n");
                            }
                            break;

                        case MTYPE_CONTROL:
                            p->sock->receiveMessageLength += p->lengthFound;
                            break;
                    }

                    p->currState = STATE_READY;
                    p->lengthFound = 0;
                    continue;
                }

                if (b != ESCAPE_COMMAND)
                {
                    p->currState = STATE_ABORT;
                    console_c.Con_DPrintf("Serial: Bad escape sequence\n");
                    continue;
                }

                p->currState = p->prevState;
            }

            p->prevState = p->currState;

            if (p->sock->receiveMessageLength + p->lengthFound > net_c.NET_MAXMESSAGE)
            {
                console_c.Con_DPrintf($"Serial blew out receive buffer: {p->sock->receiveMessageLength + p->lengthFound}\n");
                p->currState = STATE_ABORT;
            }

            if (p->sock->receiveMessageLength + p->lengthFound == net_c.NET_MAXMESSAGE)
            {
                console_c.Con_DPrintf($"Serial hit receive buffer limit: {p->sock->receiveMessageLength + p->lengthFound}\n");
                p->currState = STATE_ABORT;
            }

            switch (p->currState)
            {
                case STATE_READY:
                    crc_c.CRC_Init(&p->crcValue);
                    crc_c.CRC_ProcessByte(&p->crcValue, b);

                    if (p->client)
                    {
                        if ((b & MTYPE_CLIENT) != 0)
                        {
                            p->currState = STATE_ABORT;
                            console_c.Con_DPrintf("Serial: client got own message\n");
                            break;
                        }
                    }
                    else
                    {
                        if ((b & MTYPE_CLIENT) == 0)
                        {
                            p->currState = STATE_ABORT;
                            console_c.Con_DPrintf("Serial: server got own message\n");
                            break;
                        }

                        b &= 0x7f;
                    }

                    p->mtype = (byte)b;

                    if (b != MTYPE_CONTROL)
                    {
                        p->currState = STATE_SEQUENCE;
                    }
                    else
                    {
                        p->currState = STATE_LENGTH1;
                    }

                    if (p->mtype < MTYPE_ACK)
                    {
                        p->sock->receiveMessage[p->sock->receiveMessageLength] = b;
                        p->lengthFound++;
                    }

                    break;

                case STATE_SEQUENCE:
                    p->sequence = b;
                    crc_c.CRC_ProcessByte(&p->crcValue, b);

                    if (p->mtype != MTYPE_ACK)
                    {
                        p->currState = STATE_LENGTH1;
                    }
                    else
                    {
                        p->currState = STATE_CRC1;
                    }

                    break;

                case STATE_LENGTH1:
                    p->lengthStated = b * 256;
                    crc_c.CRC_ProcessByte(&p->crcValue, b);
                    p->currState = STATE_LENGTH2;
                    break;

                case STATE_LENGTH2:
                    p->lengthStated += b;
            }
        }
    }
}