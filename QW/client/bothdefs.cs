namespace Quake.Client;

public class bothdefs_c
{
    public const double GLQUAKE_VERSION = 1.00;
    public const double VERSION = 2.40;
    public const double LINUX_VERSION = 0.98;

#if (_M_IX86 || __i386__) && !id386
        public const bool id386 = true;
#else
    public const bool id386 = false;
#endif

#if SERVERONLY // No asm in dedicated server
// ??? Hur kan man implementera #undef???
#endif

#if id386
        public const bool UNALIGNED_OK = true; // Set to false if unaligned accesses are not supported
#else
    public const bool UNALIGNED_OK = false;
#endif

    public const int CACHE_SIZE = 32;

    public static void UNUSED<T>(T variable) { } // For pesky compiler / lint warnings

    public const int MINIMUM_MEMORY = 0x550000;

    // Up / down
    public const int PITCH = 0;

    // Left / right
    public const int YAW = 1;

    // Fall over
    public const int ROLL = 2;

    public const int MAX_SCOREBOARD = 16; // Max number of players

    public const int SOUND_CHANNELS = 8;

    public const int MAX_QPATH = 64;    // Max length of a quake game pathname
    public const int MAX_OSPATH = 128;  // Max length of a filesystem pathname

    public const double ON_EPSILON = 0.1; // Point on plane side epsilon

    public const int MAX_MSGLEN = 1450; // Max length of a reliable message
    public const int MAX_DATAGRAM = 1450; // Max length of unreliable message

    //
    // Per-level limits
    //
    public const int MAX_EDICTS = 768; // FIXME: ouch! ouch! ouch! (real pain of id devs?!)
    public const int MAX_LIGHTSTYLES = 64;
    public const int MAX_MODELS = 256; // These are sent over the net as bytes
    public const int MAX_SOUNDS = 256; // So they cannot be blindly increased

    public const int SAVEGAME_COMMENT_LENGTH = 39;

    public const int MAX_STYLESTRING = 64;

    //
    // Stats are integers communicated to the client by the server
    //
    public const int MAX_CL_STATS = 32;
    public const int STAT_HEALTH = 0;
    // public const int STAT_FRAGS = 1;
    public const int STAT_WEAPON = 2;
    public const int STAT_AMMO = 3;
    public const int STAT_ARMOR = 4;
    // public const int STAT_WEAPONFRAME = 5;
    public const int STAT_SHELLS = 6;
    public const int STAT_NAILS = 7;
    public const int STAT_ROCKETS = 8;
    public const int STAT_CELLS = 9;
    public const int STAT_ACTIVEWEAPON = 10;
    public const int STAT_TOTALSECRETS = 11;
    public const int STAT_TOTALMONSTERS = 12;
    public const int STAT_SECRETS = 13;     // Bumped on client side by svc_foundsecret
    public const int STAT_MONSTERS = 14;    // Bumped by svc_killedmonster
    public const int STAT_ITEMS = 15;
    // public const int STAT_VIEWHEIGHT = 16;

    //
    // Item flags
    //
    public const int IT_SHOTGUN = 1;
    public const int IT_SUPER_SHOTGUN = 2;
    public const int IT_NAILGUN = 4;
    public const int IT_SUPER_NAILGUN = 8;

    public const int IT_GRENADE_LAUNCHER = 16;
    public const int IT_ROCKET_LAUNCHER = 32;
    public const int IT_LIGHTNING = 64;
    public const int IT_SUPER_LIGHTNING = 128;

    public const int IT_SHELLS = 256;
    public const int IT_NAILS = 512;
    public const int IT_ROCKETS = 1024;
    public const int IT_CELLS = 2048;

    public const int IT_AXE = 4096;

    public const int IT_ARMOR1 = 8192;
    public const int IT_ARMOR2 = 16384;
    public const int IT_ARMOR3 = 32768;

    public const int IT_SUPERHEALTH = 65536;

    public const int IT_KEY1 = 131072;
    public const int IT_KEY2 = 262144;

    public const int IT_INVISIBILITY = 524288;

    public const int IT_INVULNERABILITY = 1048576;
    public const int IT_SUIT = 2097152;
    public const int IT_QUAD = 4194304;

    public const int IT_SIGIL1 = (1 << 28);

    public const int IT_SIGIL2 = (1 << 29);
    public const int IT_SIGIL3 = (1 << 30);
    public const int IT_SIGIL4 = (1 << 31);

    //
    // Print flags
    //
    public const int PRINT_LOW = 0;     // Pickup messages
    public const int PRINT_MEDIUM = 1;  // Death messages
    public const int PRINT_HIGH = 2;    // Critical messages
    public const int PRINT_CHAT = 3;    // Chat messages
}
