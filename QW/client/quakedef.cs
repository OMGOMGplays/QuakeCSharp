global using System;
global using System.IO;
global using System.Linq;
global using System.Collections.Generic;
global using System.Runtime.InteropServices;
global using System.Text;
global using System.Numerics;
global using System.Collections.Immutable;
global using System.Diagnostics;
global using System.Security.Permissions;

namespace Quake;

public class quakedef
{
	public const double VERSION = 1.00;
	public const double GLQUAKE_VERSION = 1.00;
	public const double D3DQUAKE_VERSION = 0.01;
	public const double WINQUAKE_VERSION = 0.996;
	public const double LINUX_VERSION = 1.30;
	public const double X11_VERSION = 1.10;

	public const bool PARANOID = false; // Debugging!

#if QUAKE2
    public const string GAMENAME = "id1";
#else
	public const string GAMENAME = "id1";
#endif

#if _WIN32 && !WINDED

#if _M_IX86
    public const bool __i386__;
#endif
    
    public void VID_LockBuffer() {}
    public void VID_UnlockBuffer() {}

#else

	public static void VID_LockBuffer() { }
	public static void VID_UnlockBuffer() { }

#endif

#if __i386__
    public const bool id386 = true;
#else
	public const bool id386 = false;
#endif

#if id386
    public const bool UNALIGNED_OK = true;
#else
	public const bool UNALIGNED_OK = false;
#endif

	public const int CACHE_SIZE = 32;

	public static void UNUSED<T>(T variable) { }

	public const int MINIMUM_MEMORY = 0x550000;
	public const int MINIMUM_MEMORY_LEVELPAK = (MINIMUM_MEMORY + 0x100000);

	public const int MAX_NUM_ARGVS = 50;

	public const int PITCH = 0;

	public const int YAW = 1;

	public const int ROLL = 2;

	public const int MAX_QPATH = 64;
	public const int MAX_OSPATH = 128;

	public const double ON_EPSILON = 0.1;

	public const int MAX_MSGLEN = 8000;
	public const int MAX_DATAGRAM = 1024;

	public const int MAX_EDICTS = 600;
	public const int MAX_LIGHTSTYLES = 64;
	public const int MAX_MODELS = 256;
	public const int MAX_SOUNDS = 256;

	public const int SAVEGAME_COMMENT_LENGTH = 39;

	public const int MAX_STYLESTRING = 64;

	public const int MAX_CL_STATS = 32;
	public const int STAT_HEALTH = 0;
	public const int STAT_FRAGS = 1;
	public const int STAT_WEAPON = 2;
	public const int STAT_AMMO = 3;
	public const int STAT_ARMOR = 4;
	public const int STAT_WEAPONFRAME = 5;
	public const int STAT_SHELLS = 6;
	public const int STAT_NAILS = 7;
	public const int STAT_ROCKETS = 8;
	public const int STAT_CELLS = 9;
	public const int STAT_ACTIVEWEAPON = 10;
	public const int STAT_TOTALSECRETS = 11;
	public const int STAT_TOTALMONSTERS = 12;
	public const int STAT_SECRETS = 13;
	public const int STAT_MONSTERS = 14;

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

	public const int RIT_SHELLS = 128;
	public const int RIT_NAILS = 256;
	public const int RIT_ROCKETS = 512;
	public const int RIT_CELLS = 1024;
	public const int RIT_AXE = 2048;
	public const int RIT_LAVA_NAILGUN = 4096;
	public const int RIT_LAVA_SUPER_NAILGUN = 8192;
	public const int RIT_MULTI_GRENADE = 16384;
	public const int RIT_MULTI_ROCKET = 32768;
	public const int RIT_PLASMA_GUN = 65536;
	public const int RIT_ARMOR1 = 8388608;
	public const int RIT_ARMOR2 = 16777216;
	public const int RIT_ARMOR3 = 33554432;
	public const int RIT_LAVA_NAILS = 67108864;
	public const int RIT_PLASMA_AMMO = 134217728;
	public const int RIT_MULTI_ROCKETS = 268435456;
	public const int RIT_SHIELD = 536870912;
	public const int RIT_ANTIGRAV = 1073741824;
	public const uint RIT_SUPERHEALTH = 2147483648;

	public const int HIT_PROXIMITY_GUN_BIT = 16;
	public const int HIT_MJOLNIR_BIT = 7;
	public const int HIT_LASER_CANNON_BIT = 23;
	public const int HIT_PROXIMITY_GUN = (1 << HIT_PROXIMITY_GUN_BIT);
	public const int HIT_MJOLNIR = (1 << HIT_MJOLNIR_BIT);
	public const int HIT_LASER_CANNON = (1 << HIT_LASER_CANNON_BIT);
	public const int HIT_WETSUIT = (1 << (23 + 2));
	public const int HIT_EMPATHY_SHIELDS = (1 << (23 + 3));

	public const int MAX_SCOREBOARD = 16;
	public const int MAX_SCOREBOARDNAME = 32;

	public const int SOUND_CHANNELS = 8;

	public struct entity_state_t
	{
		public vec3_t origin;
		public vec3_t angles;
		public int modelindex;
		public int frame;
		public int colormap;
		public int skin;
		public int effects;
	}

	public unsafe struct quakeparms_t
	{
		public string basedir;
		public string cachedir;
		public int argc;
		public string argv;
		public void* membase;
		public int memsize;
	}

	public bool noclip_anglehack;

	public static quakeparms_t host_parms;

	public cvar_t sys_ticrate;
	public cvar_t sys_nostdout;
	public cvar_t developer;

	public bool host_initialized;
	public double host_frametime;
	public int host_basepal;
	public int host_colormap;
	public int host_framecount;
	public double realtime;


}