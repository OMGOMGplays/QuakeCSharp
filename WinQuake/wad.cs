namespace Quake;

public unsafe class wad_c
{
	public const int CMP_NONE = 0;
	public const int CMP_LZSS = 1;

	public const int TYP_NONE = 0;
	public const int TYP_LABEL = 1;

	public const int TYP_LUMPY = 64;
	public const int TYP_PALETTE = 64;
	public const int TYP_QTEX = 65;
	public const int TYP_QPIC = 66;
	public const int TYP_SOUND = 67;
	public const int TYP_MIPTEX = 68;

	public struct qpic_t
	{
		public int width, height;
		public byte[] data;
	}

	public struct wadinfo_t
	{
		public string identification;
		public int numlumps;
		public int infotableofs;
	}

	public struct lumpinfo_t
	{
		public int filepos;
		public int disksize;
		public int size;
		public string type;
		public string compression;
		public string pad1, pad2;
		public string name;
	}

	public int wad_numlumps;
	public lumpinfo_t* wad_lumps;
	public byte* wad_base;

	public void W_CleanupName(string input, string output)
	{
		int i;
		int c;

		for (i = 0; i < 16; i++)
		{
			c = input[i];

			if (c == 0)
			{
				break;
			}

			if (c >= 'A' && c <= 'Z')
			{
				c += ('a' - 'A');
			}

			output.ToCharArray()[i] = (char)c;
		}

		for (; i < 16; i++)
		{
			output.ToCharArray()[i] = (char)0;
		}
	}

	public void W_LoadWadFile(string filename)
	{
		lumpinfo_t* lump_p;
		wadinfo_t* header;
		uint i;
		int infotableofs;

		wad_base = common_c.COM_LoadHunkFile(filename);

		if (wad_base == null)
		{
			sys_win_c.Sys_Error($"W_LoadWadFile: couldn't load {filename}");
		}

		header = (wadinfo_t*)wad_base;

		if (header->identification[0] != 'W'
		 || header->identification[1] != 'A'
		 || header->identification[2] != 'D'
		 || header->identification[3] != 2)
		{
			sys_win_c.Sys_Error($"Wad file {filename} doesn't have WAD2 id\n");
		}

		wad_numlumps = common_c.LittleLong(header->numlumps);
		infotableofs = common_c.LittleLong(header->infotableofs);
		wad_lumps = (lumpinfo_t*)(wad_base + infotableofs);

		for (i = 0, lump_p = wad_lumps; i < wad_numlumps; i++, lump_p++)
		{
			lump_p->filepos = common_c.LittleLong(lump_p->filepos);
			lump_p->size = common_c.LittleLong(lump_p->size);
			W_CleanupName(lump_p->name, lump_p->name);

			if (lump_p->type == TYP_QPIC.ToString())
			{
				SwapPic((qpic_t*)(wad_base + lump_p->filepos));
			}
		}
	}

	public lumpinfo_t* W_GetLumpinfo(string name)
	{
		int i;
		lumpinfo_t* lump_p;
		string clean = null;

		W_CleanupName(name, clean);

		for (lump_p = wad_lumps, i = 0; i < wad_numlumps; i++, lump_p++)
		{
			if (!common_c.Q_strcmp(clean, lump_p->name))
			{
				return lump_p;
			}
		}

		sys_win_c.Sys_Error($"W_GetLumpinfo: {name} not found");
		return null;
	}

	public void* W_GetLumpName(string name)
	{
		lumpinfo_t* lump;

		lump = W_GetLumpinfo(name);

		return wad_base + lump->filepos;
	}

	public void* W_GetLumpNum(int num)
	{
		lumpinfo_t* lump;

		if (num < 0 || num > wad_numlumps)
		{
			sys_win_c.Sys_Error($"W_GetLumpNum: bad number: {num}");
		}

		lump = wad_lumps + num;

		return wad_base + lump->filepos;
	}

	void SwapPic(qpic_t* pic)
	{
		pic->width = common_c.LittleLong(pic->width);
		pic->height = common_c.LittleLong(pic->height);
	}
}