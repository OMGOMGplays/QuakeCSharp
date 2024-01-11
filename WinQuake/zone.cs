namespace Quake;

public unsafe class zone_c
{
	public const int DYNAMIC_SIZE = 0xc000;

	public const int ZONEID = 0x1d4a11;
	public const int MINFRAGMENT = 64;

	[Serializable]
	public unsafe struct memblock_t
	{
		public int size;
		public int tag;
		public int id;
		public memblock_t* next, prev;
		public int pad;
	}

	[Serializable]
	public struct memzone_t
	{
		public int size;
		public memblock_t* blocklist;
		public memblock_t* rover;
	}

	public static memzone_t* mainzone;

	public static void Z_ClearZone(memzone_t* zone, int size)
	{
		memblock_t* block = default;

		zone->blocklist->next = zone->blocklist->prev = (memblock_t*)((byte*)zone + sizeof(memzone_t));
		zone->blocklist->tag = 1;
		zone->blocklist->id = 0;
		zone->blocklist->size = 0;
		zone->rover = block;

		block->prev = block->next = zone->blocklist;
		block->tag = 0;
		block->id = ZONEID;
		block->size = size - sizeof(memzone_t);
	}

	public static void Z_Free(void* ptr)
	{
		memblock_t* block, other;

		if (ptr == null)
		{
			sys_win_c.Sys_Error("Z_Free: null pointer");
		}

		block = (memblock_t*)((byte*)ptr - sizeof(memblock_t));

		if (block->id != ZONEID)
		{
			sys_win_c.Sys_Error("Z_Free: freed a pointer without ZONEID");
		}

		if (block->tag == 0)
		{
			sys_win_c.Sys_Error("Z_Free: freed a freed pointer");
		}

		block->tag = 0;

		other = block->prev;

		if (other->tag != 0)
		{
			other->size += block->size;
			other->next = block->next;
			other->next->prev = other;

			if (block == mainzone->rover)
			{
				mainzone->rover = other;
			}

			block = other;
		}

		other = block->next;

		if (other->tag != 0)
		{
			block->size += other->size;
			block->next = other->next;
			block->next->prev = block;

			if (other == mainzone->rover)
			{
				mainzone->rover = block;
			}
		}
	}

	public static void* Z_Malloc(int size)
	{
		void* buf;

		Z_CheckHeap();
		buf = Z_TagMalloc(size, 1);

		if (buf == null)
		{
			sys_win_c.Sys_Error($"Z_Malloc: failed on allocation of {size} bytes");
		}

		common_c.Q_memset((IntPtr)buf, 0, size);

		return buf;
	}

	public static void* Z_TagMalloc(int size, int tag)
	{
		int extra;
		memblock_t* start, rover, _new, _base;

		if (tag == 0)
		{
			sys_win_c.Sys_Error("Z_TagMalloc: tried to use a 0 tag");
		}

		size += sizeof(memblock_t);
		size += 4;
		size = (size + 7) & ~7;

		_base = rover = mainzone->rover;
		start = _base->prev;

		do
		{
			if (rover == start)
			{
				return null;
			}

			if (rover->tag != 0)
			{
				_base = rover = rover->next;
			}
			else
			{
				rover = rover->next;
			}
		} while (_base->tag != 0 || _base->size < size);

		extra = _base->size - size;

		if (extra > MINFRAGMENT)
		{
			_new = (memblock_t*)((byte*)_base + size);
			_new->size = extra;
			_new->tag = 0;
			_new->prev = _base;
			_new->id = ZONEID;
			_new->next = _base->next;
			_new->next->prev = _new;
			_base->next = _new;
			_base->size = size;
		}

		_base->tag = tag;

		mainzone->rover = _base->next;

		_base->id = ZONEID;

		*(int*)((byte*)_base + _base->size - 4) = ZONEID;

		return (byte*)_base + sizeof(memblock_t);
	}

	public void Z_Print(memzone_t* zone)
	{
		memblock_t* block = zone->blocklist;

		console_c.Con_Printf($"zone size: {zone->size}  location: {zone->ToString()}");

		while (true)
		{
			if (block->next == &zone->blocklist)
			{
				break;
			}

			if ((byte*)block + block->size != block->next)
			{
				console_c.Con_Printf("ERROR: block size does not touch the next block\n");
			}

			if (block->next->prev != block)
			{
				console_c.Con_Printf("ERROR: next block doesn't have proper back link\n");
			}

			if (block->tag == 0 && block->next->tag == 0)
			{
				console_c.Con_Printf("ERROR: two consecutive free blocks\n");
			}
		}
	}

	public static void Z_CheckHeap()
	{
		memblock_t* block = default;

		while (true)
		{
			if (block->next == mainzone->blocklist)
			{
				break;
			}

			if ((byte*)block + block->size != (byte*)block->next)
			{
				sys_win_c.Sys_Error("Z_CheckHeap: block size does not touch the next block\n");
			}

			if (block->next->prev != block)
			{
				sys_win_c.Sys_Error("Z_CheckHeap: next block doesn't have proper back link\n");
			}

			if (block->tag == 0 && block->next->tag == 0)
			{
				sys_win_c.Sys_Error("Z_CheckHeap: two consecutive free blocks\n");
			}
		}
	}

	public const int HUNK_SENTINAL = 0x1df001ed;

	struct hunk_t
	{
		public int sentinal;
		public int size;
		public string name;
	}

	public static byte* hunk_base;
	public static int hunk_size;

	public static int hunk_low_used;
	public static int hunk_high_used;

	public static bool hunk_tempactive;
	public static int hunk_tempmark;

	public void Hunk_Check()
	{
		hunk_t* h;

		for (h = (hunk_t*)hunk_base; (byte*)h != hunk_base + hunk_low_used;)
		{
			if (h->sentinal != HUNK_SENTINAL)
			{
				sys_win_c.Sys_Error("Hunk_Check: trashed sentinal");
			}

			if (h->size < 16 || h->size + (byte*)h - hunk_base > hunk_size)
			{
				sys_win_c.Sys_Error("Hunk_Check: bad size");
			}

			h = (hunk_t*)((byte*)h + h->size);
		}
	}

	public void Hunk_Print(bool all)
	{
		hunk_t* h, next, endlow, starthigh, endhigh;
		int count, sum;
		int totalblocks;
		string[] name = new string[9];

		name[8] = null;
		count = 0;
		sum = 0;
		totalblocks = 0;

		h = (hunk_t*)hunk_base;
		endlow = (hunk_t*)(hunk_base + hunk_low_used);
		starthigh = (hunk_t*)(hunk_base + hunk_size - hunk_high_used);
		endhigh = (hunk_t*)(hunk_base + hunk_size);

		console_c.Con_Printf($"	{hunk_size} total hunk size\n");
		console_c.Con_Printf("-------------------------\n");

		while (true)
		{
			if (h == endlow)
			{
				console_c.Con_Printf("-------------------------\n");
				console_c.Con_Printf($"	{hunk_size - hunk_low_used - hunk_high_used} REMAINING\n");
				console_c.Con_Printf("-------------------------\n");
				h = starthigh;
			}

			if (h == endhigh)
			{
				break;
			}

			if (h->sentinal != HUNK_SENTINAL)
			{
				sys_win_c.Sys_Error("Hunk_Check: trashed sentinal");
			}

			if (h->size < 16 || h->size + (byte*)h - hunk_base > hunk_size)
			{
				sys_win_c.Sys_Error("Hunk_Check: bad size");
			}

			next = (hunk_t*)((byte*)h + h->size);
			count++;
			totalblocks++;
			sum += h->size;

			common_c.Q_memcpy(name, h->name, 8);

			if (all)
			{
				console_c.Con_Printf($"{(int)h} :{h->size} {name}");
			}

			if (next == endlow || next == endhigh || common_c.Q_strcmp(h->name, next->name))
			{
				if (!all)
				{
					console_c.Con_Printf($"	:{(int)sum} {name} (TOTAL)\n");
				}

				count = 0;
				sum = 0;
			}

			h = next;
		}

		console_c.Con_Printf("-------------------------\n");
		console_c.Con_Printf($"{totalblocks} total blocks\n");
	}

	public static void* Hunk_AllocName(int size, string name)
	{
		hunk_t* h = default;

#if PARANOID
		Hunk_Check();
#endif
		if (size < 0)
		{
			sys_win_c.Sys_Error($"Hunk_Alloc: bad size {size}");
		}

		size = sizeof(hunk_t) + ((size + 15) & ~15);

		if (hunk_size - hunk_low_used - hunk_high_used < size)
		{
			sys_win_c.Sys_Error($"Hunk_Alloc: failed on {size} bytes");
		}

		hunk_low_used += size;

		Cache_FreeLow(hunk_low_used);

		common_c.Q_memset((int)h, 0, size);

		h->size = size;
		h->sentinal = HUNK_SENTINAL;
		common_c.Q_strncpy(h->name.ToCharArray(), name.ToCharArray(), 8);

		return h + 1;
	}

	public static void* Hunk_Alloc(int size)
	{
		return Hunk_AllocName(size, "unknown");
	}

	public static int Hunk_LowMark()
	{
		return hunk_low_used;
	}

	public static void Hunk_FreeToLowMark(int mark)
	{
		if (mark < 0 || mark > hunk_low_used)
		{
			sys_win_c.Sys_Error($"Hunk_FreeToLowMark: bad mark {mark}");
		}

		common_c.Q_memset((IntPtr)(hunk_base + mark), 0, hunk_low_used - mark);
		hunk_low_used = mark;
	}

	public static int Hunk_HighMark()
	{
		if (hunk_tempactive)
		{
			hunk_tempactive = false;
			Hunk_FreeToHighMark(hunk_tempmark);
		}

		return hunk_high_used;
	}

	public static void Hunk_FreeToHighMark(int mark)
	{
		if (hunk_tempactive)
		{
			hunk_tempactive = false;
			Hunk_FreeToHighMark(hunk_tempmark);
		}

		if (mark < 0 || mark > hunk_high_used)
		{
			sys_win_c.Sys_Error($"Hunk_FreeToHighMark: bad mark {mark}");
		}

		common_c.Q_memset((IntPtr)(hunk_base + hunk_size - hunk_high_used), 0, hunk_high_used - mark);
		hunk_high_used = mark;
	}

	public static void* Hunk_HighAllocName(int size, string name)
	{
		hunk_t* h;

		if (size < 0)
		{
			sys_win_c.Sys_Error($"Hunk_HighAllocName: bad size: {size}");
		}

		if (hunk_tempactive)
		{
			Hunk_FreeToHighMark(hunk_tempmark);
			hunk_tempactive = false;
		}

#if PARANOID
		Hunk_Check();
#endif

		size = sizeof(hunk_t) + ((size + 15) & ~15);

		if (hunk_size - hunk_low_used - hunk_high_used < size)
		{
			console_c.Con_Printf($"Hunk_HighAlloc: failed on {size} bytes\n");
			return null;
		}

		hunk_high_used += size;
		Cache_FreeHigh(hunk_high_used);

		h = (hunk_t*)(hunk_base + hunk_size - hunk_high_used);

		common_c.Q_memset((int)h, 0, size);
		h->size = size;
		h->sentinal = HUNK_SENTINAL;
		common_c.Q_strncpy(h->name.ToCharArray(), name.ToCharArray(), 8);

		return h + 1;
	}

	//public void* Hunk_Alloc(int size)
	//{
	//	return Hunk_AllocName(size, "unknown");
	//}

	public static void* Hunk_TempAlloc(int size)
	{
		void* buf;

		size = (size + 15) & ~15;

		if (hunk_tempactive)
		{
			Hunk_FreeToHighMark(hunk_tempmark);
			hunk_tempactive = false;
		}

		hunk_tempmark = Hunk_HighMark();

		buf = Hunk_HighAllocName(size, "temp");

		hunk_tempactive = true;

		return buf;
	}

	public struct cache_system_t
	{
		public int size;
		public cache_user_t* user;
		public string name;
		public cache_system_t* prev, next;
		public cache_system_t* lru_prev, lru_next;
	}

	public struct cache_user_t
	{
		public void* data;
	}

	public static cache_system_t* cache_head;

	public static void Cache_Move(cache_system_t* c)
	{
		cache_system_t* newc;

		newc = Cache_TryAlloc(c->size, true);

		if (newc != null)
		{
			common_c.Q_memcpy((int)newc + 1, (int)c + 1, c->size - sizeof(cache_system_t));
			newc->user = c->user;
			common_c.Q_memcpy(newc->name, c->name, 16);
			Cache_Free(c->user);
			newc->user->data = (void*)(newc + 1);
		}
		else
		{
			Cache_Free(c->user);
		}
	}

	public static void Cache_FreeLow(int new_low_hunk)
	{
		cache_system_t* c;

		while (true)
		{
			c = cache_head->next;
			if (c == cache_head)
			{
				return;
			}

			if ((byte*)c >= hunk_base + new_low_hunk)
			{
				return;
			}

			Cache_Move(c);
		}
	}

	public static void Cache_FreeHigh(int new_high_hunk)
	{
		cache_system_t* c, prev;

		prev = null;

		while (true)
		{
			c = cache_head->prev;

			if (c == cache_head)
			{
				return;
			}

			if ((byte*)c + c->size <= hunk_base + hunk_size - new_high_hunk)
			{
				return;
			}

			if (c == prev)
			{
				Cache_Free(c->user);
			}
			else
			{
				Cache_Move(c);
				prev = c;
			}
		}
	}

	public static void Cache_UnlinkLRU(cache_system_t* cs)
	{
		if (cs->lru_next == null || cs->lru_prev == null)
		{
			sys_win_c.Sys_Error("Cache_UnlinkLRU: null link");
		}

		cs->lru_next->lru_prev = cs;
		cs->lru_next = cache_head->lru_next;
		cs->lru_prev = cache_head;
		cache_head->lru_next = cs;
	}

	public static void Cache_MakeLRU(cache_system_t* cs)
	{
		if (cs->lru_next != null || cs->lru_prev != null)
		{
			sys_win_c.Sys_Error("Cache_MakeLRU: active link");
		}

		cache_head->lru_next->lru_prev = cs;
		cs->lru_next = cache_head->lru_next;
		cs->lru_prev = cache_head;
		cache_head->lru_next = cs;
	}

	public static cache_system_t* Cache_TryAlloc(int size, bool nobottom)
	{
		cache_system_t* cs, _new;

		if (!nobottom && cache_head->prev == cache_head)
		{
			if (hunk_size - hunk_high_used - hunk_low_used < size)
			{
				sys_win_c.Sys_Error($"Cache_TryAlloc: {size} is greater than free hunk");
			}

			_new = (cache_system_t*)(hunk_base + hunk_low_used);
			common_c.Q_memset((int)_new, 0, sizeof(cache_system_t*));
			_new->size = size;

			cache_head->prev = cache_head->next = _new;
			_new->prev = _new->next = cache_head;

			Cache_MakeLRU(_new);
			return _new;
		}

		_new = (cache_system_t*)(hunk_base + hunk_low_used);
		cs = cache_head->next;

		do
		{
			if (!nobottom || cs != cache_head->next)
			{
				if ((byte*)cs - (byte*)_new >= size)
				{
					common_c.Q_memset((int)_new, 0, sizeof(cache_system_t*));
					_new->size = size;

					_new->next = cs;
					_new->prev = cs->prev;
					cs->prev->next = _new;
					cs->prev = _new;

					Cache_MakeLRU(_new);

					return _new;
				}
			}

			_new = (cache_system_t*)((byte*)cs + cs->size);
			cs = cs->next;
		} while (cs != cache_head);

		if (hunk_base + hunk_size - hunk_high_used - (byte*)_new >= size)
		{
			common_c.Q_memset((int)_new, 0, sizeof(cache_system_t*));
			_new->size = size;

			_new->next = cache_head;
			_new->prev = cache_head->prev;
			cache_head->prev->next = _new;
			cache_head->prev = _new;

			Cache_MakeLRU(_new);

			return _new;
		}

		return null;
	}

	public static void Cache_Flush()
	{
		while (cache_head->next != cache_head)
		{
			Cache_Free(cache_head->next->user);
		}
	}

	public void Cache_Print()
	{
		cache_system_t* cd;

		for (cd = cache_head->next; cd != cache_head; cd = cd->next)
		{
			console_c.Con_Printf($"{cd->size} : {cd->name}\n");
		}
	}

	public void Cache_Report()
	{
		console_c.Con_Printf($"{(hunk_size - hunk_high_used - hunk_low_used) / (float)(1024 * 1024)} megabyte data cache\n");
	}

	public void Cache_Compact()
	{
	}

	public static void Cache_Init()
	{
		cache_head->next = cache_head->prev = cache_head;
		cache_head->lru_next = cache_head->lru_prev = cache_head;

		cmd_c.Cmd_AddCommand("flush", Cache_Flush);
	}

	public static void Cache_Free(cache_user_t* c)
	{
		cache_system_t* cs;

		if (c->data == null)
		{
			sys_win_c.Sys_Error("Cache_Free: not allocated");
		}

		cs = ((cache_system_t*)c->data) - 1;

		cs->prev->next = cs->next;
		cs->next->prev = cs->prev;
		cs->next = cs->prev = null;

		c->data = null;

		Cache_UnlinkLRU(cs);
	}

	public static void* Cache_Check(cache_user_t* c)
	{
		cache_system_t* cs;

		if (c->data == null)
		{
			return null;
		}

		cs = ((cache_system_t*)c->data) - 1;

		Cache_UnlinkLRU(cs);
		Cache_MakeLRU(cs);

		return c->data;
	}

	public static void* Cache_Alloc(cache_user_t* c, int size, string name)
	{
		cache_system_t* cs;

		if (c->data != null)
		{
			sys_win_c.Sys_Error("Cache_Alloc: already allocated");
		}

		if (size <= 0)
		{
			sys_win_c.Sys_Error($"Cache_Alloc: size {size}");
		}

		size = (size + sizeof(cache_system_t) + 15) & ~15;

		while (true)
		{
			cs = Cache_TryAlloc(size, false);

			if (cs != null)
			{
				common_c.Q_strncpy(cs->name.ToCharArray(), name.ToCharArray(), 15);
				c->data = (void*)(cs + 1);
				cs->user = c;
				break;
			}

			if (cache_head->lru_prev == cache_head)
			{
				sys_win_c.Sys_Error("Cache_Alloc: out of memory");
			}

			Cache_Free(cache_head->lru_prev->user);
		}

		return Cache_Check(c);
	}

	public static void Memory_Init(void* buf, int size)
	{
		int p;
		int zonesize = DYNAMIC_SIZE;

		hunk_base = (byte*)buf;
		hunk_size = size;
		hunk_low_used = 0;
		hunk_high_used = 0;

		Cache_Init();
		p = common_c.COM_CheckParm("-zone");
		if (p != 0)
		{
			if (p < common_c.com_argc - 1)
			{
				zonesize = common_c.Q_atoi(common_c.com_argv[p + 1].ToString()) * 1024;
			}
			else
			{
				sys_win_c.Sys_Error("Memory_Init: you must specify a size in KB after -zone");
			}
		}

		mainzone = (memzone_t*)Hunk_AllocName(zonesize, "zone");
		Z_ClearZone(mainzone, zonesize);
	}
}