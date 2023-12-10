namespace Quake;

public unsafe class model_c
{
    public struct mvertex_t
    {
        public Vector3 position;
    }

    public static int SIDE_FRONT = 0;
    public static int SIDE_BACK = 1;
    public static int SIDE_ON = 2;

    public struct mplane_t
    {
        public Vector3 normal;
        public float dist;
        public byte type;
        public byte signbits;
        public byte[] pad;
    }

    public struct texture_t
    {
        public char* name;
        public uint width, height;
        public int anim_total;
        public int anim_min, anim_max;
        public texture_t* anim_next;
        public texture_t* alternate_anims;
        public uint[] offsets;
    }

    public static int SURF_PLANEBACK = 2;
    public static int SURF_DRAWSKY = 4;
    public static int SURF_DRAWSPRITE = 8;
    public static int SURF_DRAWTURB = 0x10;
    public static int SURF_DRAWTILED = 0x20;
    public static int SURF_DRAWBACKGROUND = 0x40;

    public struct medge_t
    {
        public ushort[] v;
        public uint cachededgeoffset;
    }

    public struct mtexinfo_t
    {
        public float[][] vecs;
        public float mipadjust;
        public texture_t* texture;
        public int flags;
    }

    public struct msurface_t
    {
        public int visframe;

        public int dlightframe;
        public int dlightbits;

        public mplane_t* plane;
        public int flags;

        public int firstedge;
        public int numedges;

        public surfcache_t* cachespots;

        public short[] texturemins;
        public short[] extents;

        public mtexinfo_t* texinfo;

        public byte[] styles;
        public byte* samples;
    }

    public struct mnode_t
    {
        public int contents;
        public int visframe;

        public short[] minmaxs;

        public mnode_t* parent;

        public mplane_t* plane;
        public mnode_t* children;

        public ushort firstsurface;
        public ushort numsurfaces;
    }

    public struct mleaf_t
    {
        public int contents;
        public int visframe;

        public short[] minmaxs;

        public mnode_t* parent;

        public byte* compressed_vis;
        public render_c.efrag_t* efrags;

        public msurface_t** firstmarksurface;
        public int nummarksurfaces;
        public int key;
        public byte[] ambient_sound_level;
    }

    public struct hull_t
    {
        public bspfile_c.dclipnode_t* clipnodes;
        public mplane_t* planes;
        public int firstclipnode;
        public int lastclipnode;
        public Vector3 clip_mins;
        public Vector3 clip_maxs;
    }

    public struct mspriteframe_t
    {
        public int width;
        public int height;
        public void* pcachespot;
        public float up, down, left, right;
        public byte[] pixels;
    }

    public struct mspritegroup_t
    {
        public int numframes;
        public float* intervals;
        public mspriteframe_t* frames;
    }

    public struct mspriteframedesc_t
    {
        public spritegn_c.spriteframetype_t type;
        public mspriteframe_t* frameptr;
    }

    public struct msprite_t
    {
        public int type;
        public int maxwidth;
        public int maxheight;
        public int numframes;
        public float beamlength;
        public void* cachespot;
        public mspriteframedesc_t[] frames;
    }

    public struct maliasframedesc_t
    {
        public modelgen_c.aliasframetype_t type;
        public modelgen_c.trivertx_t bboxmin;
        public modelgen_c.trivertx_t bboxmax;
        public int frame;
        public char[] name;
    }

    public struct maliasskindesc_t
    {
        public modelgen_c.aliasskintype_t type;
        public void* pcachespot;
        public int skin;
    }

    public struct maliasgroupframedesc_t
    {
        public modelgen_c.trivertx_t bboxmin;
        public modelgen_c.trivertx_t bboxmax;
        public int frame;
    }

    public struct maliasgroup_t
    {
        public int numframes;
        public int intervals;
        public maliasgroupframedesc_t[] frames;
    }

    public struct maliasskingroup_t
    {
        public int numskins;
        public int intervals;
        public maliasskindesc_t[] skindescs;
    }

    public struct mtriangle_t
    {
        public int facesfront;
        public int[] vertindex;
    }

    public struct aliashdr_t
    {
        public int model;
        public int stverts;
        public int skindesc;
        public int triangles;
        public maliasframedesc_t[] frames;
    }

    public enum modtype_t { mod_brush, mod_sprite, mod_alias }

    public static int EF_ROCKET = 1;
    public static int EF_GRENADE = 2;
    public static int EF_GIB = 4;
    public static int EF_ROTATE = 8;
    public static int EF_TRACER = 16;
    public static int EF_ZOMGIB = 32;
    public static int EF_TRACER2 = 64;
    public static int EF_TRACER3 = 128;

    public struct model_t
    {
        public char[] name;
        public bool needload;

        public modtype_t type;
        public int numframes;
        public modelgen_c.synctype_t synctype;

        public int flags;

        public Vector3 mins, maxs;
        public float radius;

        public int firstmodelsurface, nummodelsurfaces;

        public int numsubmodels;
        public bspfile_c.dmodel_t* submodels;

        public int numplanes;
        public mplane_t* planes;

        public int numleafs;
        public mleaf_t* leafs;

        public int numvertexes;
        public mvertex_t* vertexes;

        public int numedges;
        public medge_t* edges;

        public int numnodes;
        public mnode_t* nodes;

        public int numtexinfo;
        public mtexinfo_t* texinfo;

        public int numsurfaces;
        public msurface_t* surfaces;

        public int numsurfegdes;
        public int* surfedges;

        public int numclipnodes;
        public bspfile_c.clipnodes* clipnodes;

        public int nummarksurfaces;
        public msurface_t** marksurfaces;

        public hull_t[] hulls;

        public int numtextures;
        public texture_t** textures;

        public byte* visdata;
        public byte* lightdata;
        public char* entities;

        public zone_c.cache_user_t cache;
    }

    public model_t* loadmodel;
    public char[] loadname = new char[32];

    public byte* mod_novis;

    public static int MAX_MOD_KNOWN = 256;
    public model_t* mod_known;
    public int mod_numknown;

    public static int NL_PRESENT = 0;
    public static int NL_NEEDS_LOADED = 1;
    public static int NL_UNREFERENCED = 2;

    public void Mod_Init()
    {
        common_c.Q_memset(mod_novis, 0xff, bspfile_c.MAX_MAP_LEAVES / 8);
    }

    public void* Mod_ExtraData(model_t* mod)
    {
        void* r;

        r = zone_c.Cache_Check(&mod->cache);

        if (r != null)
        {
            return r;
        }

        Mod_LoadModel(mod, true);

        if (mod->cache.data == null)
        {
            sys_win_c.Sys_Error("Mod_ExtraData: caching failed");
        }

        return mod->cache.data;
    }

    public mleaf_t* Mod_PointInLeaf(Vector3 p, model_t* model)
    {
        mnode_t* node;
        float d;
        mplane_t* plane;

        if (model == null || model->nodes == null)
        {
            sys_win_c.Sys_Error("Mod_PointInLeaf: bad model");
        }

        node = model->nodes;

        while (true)
        {
            if (node->contents < 0)
            {
                return (mleaf_t*)node;
            }

            plane = node->plane;
            d = mathlib_c.DotProduct_V(p, plane->normal) - plane->dist;

            if (d > 0)
            {
                node = &node->children[0];
            }
            else
            {
                node = &node->children[1];
            }
        }

        return null;
    }

    public byte* Mod_DecompressVis(byte* input, model_t* model)
    {
        byte* decompressed = null;
        int c;
        byte* output;
        int row;

        row = (model->numleafs + 7) >> 3;
        output = decompressed;

        if (input == null)
        {
            while (row != 0)
            {
                *output++ = 0xff;
                row--;
            }

            return decompressed;
        }

        do
        {
            if (*input != 0)
            {
                *output++ = *input++;
                continue;
            }

            c = input[1];
            input += 2;

            while (c != 0)
            {
                *output++ = 0;
                c--;
            }
        } while (output - decompressed < row);

        return decompressed;
    }

    public byte* Mod_LeafPVS(mleaf_t* leaf, model_t* model)
    {
        if (leaf == model->leafs)
        {
            return mod_novis;
        }

        return Mod_DecompressVis(leaf->compressed_vis, model);
    }

    public void Mod_ClearAll()
    {
        int i;
        model_t* mod;

        for (i = 0, mod = mod_known; i < mod_numknown; i++, mod++)
        {
            mod->needload = NL_UNREFERENCED == 0 ? false : true;

            if (mod->type == modtype_t.mod_sprite)
            {
                mod->cache.data = null;
            }
        }
    }

    public model_t* Mod_FindName(char* name)
    {
        int i;
        model_t* mod;
        model_t* avail = null;

        if (name[0] == null)
        {
            sys_win_c.Sys_Error("Mod_ForName: null name");
        }

        for (i = 0, mod = mod_known; i < mod_numknown; i++, mod++)
        {
            if (!common_c.Q_strcmp(mod->name.ToString(), name->ToString()))
            {
                break;
            }

            if (mod->needload == (NL_UNREFERENCED == 0 ? false : true))
            {
                if (avail == null || mod->type != modtype_t.mod_alias)
                {
                    avail = mod;
                }
            }
        }

        if (i == mod_numknown)
        {
            if (mod_numknown == MAX_MOD_KNOWN)
            {
                if (avail != null)
                {
                    mod = avail;

                    if (mod->type == modtype_t.mod_alias)
                    {
                        if (zone_c.Cache_Check(&mod->cache) != null)
                        {
                            zone_c.Cache_Free(&mod->cache);
                        }
                    }
                }
                else
                {
                    sys_win_c.Sys_Error("mod_numknown == MAX_MOD_KNOWN");
                }
            }
            else
            {
                mod_numknown++;
            }

            common_c.Q_strcpy(mod->name.ToString(), name->ToString());
            mod->needload = NL_NEEDS_LOADED == 0 ? false : true;
        }

        return mod;
    }

    public void Mod_TouchModel(char* name)
    {
        model_t* mod;

        mod = Mod_FindName(name);

        if (mod->needload == (NL_PRESENT == 0 ? false : true))
        {
            if (mod->type == modtype_t.mod_alias)
            {
                zone_c.Cache_Check(&mod->cache);
            }
        }
    }

    public model_t* Mod_LoadModel(model_t* mod, bool crash)
    {
        uint* buf;
        byte* stackbuf = null;

        if (mod->type == modtype_t.mod_alias)
        {
            if (zone_c.Cache_Check(&mod->cache) != null)
            {
                mod->needload = NL_PRESENT == 0 ? false : true;
                return mod;
            }
        }
        else
        {
            if (mod->needload == (NL_PRESENT == 0 ? false : true))
            {
                return mod;
            }
        }

        buf = (uint*)common_c.COM_LoadStackFile(mod->name.ToString(), stackbuf, 1024);

        if (buf == null)
        {
            if (crash)
            {
                sys_win_c.Sys_Error($"Mod_NumForName: {mod->name} not found");
            }

            return null;
        }

        common_c.COM_FileBase(mod->name.ToString(), loadname.ToString());

        loadmodel = mod;

        mod->needload = NL_PRESENT == 0 ? false : true;

        switch (common_c.LittleLong(*(int*)buf))
        {
            case modelgen_c.IDPOLYHEADER:
                Mod_LoadAliasModel(mod, buf);
                break;

            case modelgen_c.IDSPRITEHEADER:
                Mod_LoadSpriteModel(mod, buf);
                break;

            default:
                Mod_LoadBrushModel(mod, buf);
                break;
        }

        return mod;
    }

    public model_t* Mod_ForName(char* name, bool crash)
    {
        model_t* mod;

        mod = Mod_FindName(name);

        return Mod_LoadModel(mod, crash);
    }

    public byte* mod_base;

    public void Mod_LoadTextures(bspfile_c.lump_t* l)
    {
        int i, j, pixels, num, max, altmax;
        bspfile_c.miptex_t* mt;
        texture_t* tx, tx2;
        texture_t* anims = null;
        texture_t* altanims = null;
        bspfile_c.dmiptexlump_t* m;

        if (l->filelen == 0)
        {
            loadmodel->textures = null;
            return;
        }

        m = (bspfile_c.dmiptexlump_t*)(mod_base + l->fileofs);

        m->nummiptex = common_c.LittleLong(m->nummiptex);

        loadmodel->numtextures = m->nummiptex;
        loadmodel->textures = zone_c.Hunk_AllocName(m->nummiptex, loadname);

        for (i = 0; i < m->nummiptex; i++)
        {
            m->dataofs[i] = common_c.LittleLong(m->dataofs[i]);

            if (m->dataofs[i] == -1)
            {
                continue;
            }

            mt = (bspfile_c.miptex_t*)((byte*)m + m->dataofs[i]);
            mt->width = common_c.LittleLong(mt->width);
            mt->height = common_c.LittleLong(mt->height);

            for (j = 0; j < bspfile_c.MIPLEVELS; j++)
            {
                mt->offsets[j] = common_c.LittleLong(mt->offsets[j]);
            }

            if ((mt->width & 15) != 0 || (mt->height & 15) != 0)
            {
                sys_win_c.Sys_Error($"Texture {mt->name} is not 16 aligned");
            }

            pixels = mt->width * mt->height / 64 * 85;
            tx = (texture_t*)zone_c.Hunk_AllocName(sizeof(texture_t) + pixels, loadname.ToString());
            loadmodel->textures[i] = tx;

            common_c.Q_memcpy(tx->name, mt->name, 16);
            tx->width = mt->width;
            tx->height = mt->height;

            for (j = 0; j < bspfile_c.MIPLEVELS; j++)
            {
                tx->offsets[j] = mt->offsets[j] + sizeof(texture_t) - sizeof(bspfile_c.miptex_t);
            }

            common_c.Q_memcpy(*(tx + 1), *(mt + 1), pixels);

            if (common_c.Q_strncmp(mt->name, common_c.StringToChar("sky"), 3) == 0)
            {
                r_sky_c.R_InitSky(tx);
            }
        }

        for (i = 0; i < m->nummiptex; i++)
        {
            tx = loadmodel->textures[i];

            if (tx == null || tx->name[0] != '+')
            {
                continue;
            }

            if (tx->anim_next != null)
            {
                continue;
            }

            common_c.Q_memset(*anims, 0, (int)anims);
            common_c.Q_memset(*altanims, 0, (int)altanims);

            max = tx->name[1];
            altmax = 0;

            if (max >= 'a' && max <= 'z')
            {
                max -= 'a' - 'A';
            }

            if (max >= '0' && max <= '9')
            {
                max -= '0';
                altmax = 0;
                anims[max] = *tx;
                max++;
            }
            else if (max >= 'A' && max <= 'J')
            {
                altmax = max - 'A';
                max = 0;
                altanims[altmax] = *tx;
                altmax++;
            }
            else
            {
                sys_win_c.Sys_Error($"Bad animating texture {tx->name}");
            }

            for (j = i + 1; j < m->nummiptex; j++)
            {
                tx2 = loadmodel->textures[j];

                if (tx2 == null || tx2->name[0] != '+')
                {
                    continue;
                }

                if (common_c.Q_strcmp(tx2->name.ToString() + 2, tx->name.ToString() + 2))
                {
                    continue;
                }

                num = tx2->name[1];

                if (num >= 'a' && num <= 'z')
                {
                    num -= 'a' - 'A';
                }

                if (num >= '0' && num <= '9')
                {
                    num -= '0';
                    anims[num] = *tx2;

                    if (num + 1 > max)
                    {
                        max = num + 1;
                    }
                }
                else if (num >= 'A' && num <= 'J')
                {
                    num = num - 'A';
                    altanims[num] = *tx2;

                    if (num + 1 > altmax)
                    {
                        altmax = num + 1;
                    }
                }
                else
                {
                    sys_win_c.Sys_Error($"Bad animating texture {tx->name}");
                }
            }

            for (j = 0; j < max; j++)
            {
                tx2 = &anims[j];

                if (tx2 == null)
                {
                    sys_win_c.Sys_Error($"Missing frame {j} of {tx->name}");
                }

                tx2->anim_total = max * ANIM_CYCLE;
                tx2->anim_min = j * ANIM_CYCLE;
                tx2->anim_max = (j + 1) * ANIM_CYCLE;
                tx2->anim_next = &anims[(j + 1) % max];

                if (altmax != 0)
                {
                    tx2->alternate_anims = &altanims[0];
                }
            }

            for (j = 0; j < altmax; j++)
            {
                tx2 = &altanims[j];

                if (tx2 == null)
                {
                    sys_win_c.Sys_Error($"Missing frame {j} of {tx->name}");
                }

                tx2->anim_total = altmax * ANIM_CYCLE;
                tx2->anim_min = j * ANIM_CYCLE;
                tx2->anim_max = (j + 1) * ANIM_CYCLE;
                tx2->anim_next = &altanims[(j + 1) % altmax];

                if (max != 0)
                {
                    tx2->alternate_anims = &anims[0];
                }
            }
        }
    }

    public static int ANIM_CYCLE = 2;

    public void Mod_LoadLighting(bspfile_c.lump_t* l)
    {
        if (l->filelen == 0)
        {
            loadmodel->lightdata = null;
            return;
        }

        loadmodel->lightdata = zone_c.Hunk_AllocName(l->filelen, loadname);
        common_c.Q_memcpy(loadmodel->lightdata, mod_base + l->fileofs, l->filelen);
    }

    public void Mod_LoadVisibility(bspfile_c.lump_t* l)
    {
        if (l->filelen == 0)
        {
            loadmodel->visdata = null;
            return;
        }

        loadmodel->visdata = zone_c.Hunk_AllocName(l->filelen, loadname);
        common_c.Q_memcpy(loadmodel->visdata, mod_base + l->fileofs, l->filelen);
    }

    public void Mod_LoadEntities(bspfile_c.lump_t* l)
    {
        if (l->filelen == 0)
        {
            loadmodel->entities = null;
            return;
        }

        loadmodel->entities = zone_c.Hunk_AllocName(l->filelen, loadname);
        common_c.Q_memcpy(loadmodel->entities, mod_base + l->fileofs, l->filelen);
    }

    public void Mod_LoadVertexes(bspfile_c.lump_t* l)
    {
        bspfile_c.dvertex_t* input;
        mvertex_t* output;
        int i, count;

        input = (void*)(mod_base + l->fileofs);

        if ((int)(l->filelen % sizeof(bspfile_c.dvertex_t)) != 0)
        {
            sys_win_c.Sys_Error($"MOD_LoadBmodel: funny lump size in {loadmodel->name}");
        }

        count = l->filelen / sizeof(bspfile_c.dvertex_t);
        output = zone_c.Hunk_AllocName(count * sizeof(mvertex_t), loadname);

        loadmodel->vertexes = output;
        loadmodel->numvertexes = count;

        for (i = 0; i < count; i++, input++, output++)
        {
            output->position[0] = common_c.LittleFloat(input->point[0]);
            output->position[1] = common_c.LittleFloat(input->point[1]);
            output->position[2] = common_c.LittleFloat(input->point[2]);
        }
    }

    public void Mod_LoadSubModels(bspfile_c.lump_t* l)
    {
        bspfile_c.dmodel_t* input;
        bspfile_c.dmodel_t* output;
        int i, j, count;

        input = (void*)(mod_base + l->fileofs);

        if ((l->filelen % sizeof(bspfile_c.dmodel_t)) != 0)
        {
            sys_win_c.Sys_Error($"MOD_LoadBmodel: funny lump size in {loadmodel->name}");
        }

        count = l->filelen / sizeof(bspfile_c.dmodel_t);
        output = zone_c.Hunk_AllocName(count * sizeof(bspfile_c.dmodel_t), loadname);

        loadmodel->submodels = output;
        loadmodel->numsubmodels = count;

        for (i = 0; i < count; i++, input++, output++)
        {
            for (j = 0; j < 3; j++)
            {
                output->mins[j] = common_c.LittleFloat(input->mins[j]) - 1;
                output->maxs[j] = common_c.LittleFloat(input->maxs[j]) + 1;
                output->origin[j] = common_c.LittleFloat(input->origin[j]);
            }

            for (j = 0; j < bspfile_c.MAX_MAP_HULLS; j++)
            {
                output->headnode[j] = common_c.LittleLong(input->headnode[j]);
            }

            output->visleafs = common_c.LittleLong(input->visleafs);
            output->firstface = common_c.LittleLong(input->firstface);
            output->numfaces = common_c.LittleLong(input->numfaces);
        }
    }

    public void Mod_LoadEdges(bspfile_c.lump_t* l)
    {
        bspfile_c.dedge_t* input;
        medge_t* output;
        int i, count;

        input = (void*)(mod_base + l->fileofs);

        if ((l->filelen % sizeof(bspfile_c.dedge_t)) != 0)
        {
            sys_win_c.Sys_Error($"MOD_LoadBmodel: funny lump size in {loadmodel->name}");
        }

        count = l->filelen / sizeof(bspfile_c.dedge_t);
        output = zone_c.Hunk_AllocName((count + 1) * sizeof(bspfile_c.dedge_t), loadname);

        loadmodel->edges = output;
        loadmodel->numedges = count;

        for (i = 0; i < count; i++, input++, output++)
        {
            output->v[0] = (ushort)common_c.LittleShort(input->v[0]);
            output->v[1] = (ushort)common_c.LittleShort(input->v[1]);
        }
    }

    public void Mod_LoadTexInfo(bspfile_c.lump_t* l)
    {
        bspfile_c.texinfo_t* input;
        mtexinfo_t* output;
        int i, j, count;
        int miptex;
        float len1, len2;

        input = (void*)(mod_base + l->fileofs);

        if ((l->filelen % sizeof(bspfile_c.texinfo_t)) != 0)
        {
            sys_win_c.Sys_Error($"MOD_LoadBmodel: funny lump size in {loadmodel->name}");
        }

        count = l->filelen / sizeof(bspfile_c.texinfo_t);
        output = zone_c.Hunk_AllocName(count * sizeof(mtexinfo_t), loadname);

        loadmodel->texinfo = output;
        loadmodel->numtexinfo = count;

        for (i = 0; i < count; i++, input++, output++)
        {
            for (j = 0; j < 8; j++)
            {
                output->vecs[0][j] = common_c.LittleFloat(input->vecs[0][j]);
            }

            len1 = mathlib_c.Length_F(output->vecs[0]);
            len2 = mathlib_c.Length_F(output->vecs[1]);
            len1 = (len1 + len2) / 2;

            if (len1 < 0.32f)
            {
                output->mipadjust = 4;
            }
            else if (len1 < 0.49f)
            {
                output->mipadjust = 3;
            }
            else if (len1 < 0.99f)
            {
                output->mipadjust = 2;
            }
            else
            {
                output->mipadjust = 1;
            }

            miptex = common_c.LittleLong(input->miptex);
            output->flags = common_c.LittleLong(input->flags);

            if (loadmodel->textures == null)
            {
                output->texture = r_main_c.r_notexture_mip;
                output->flags = 0;
            }
            else
            {
                if (miptex >= loadmodel->numtextures)
                {
                    sys_win_c.Sys_Error("miptex >= loadmodel->numtextures");
                }

                output->texture = loadmodel->textures[miptex];

                if (output->texture == null)
                {
                    output->texture = r_main_c.r_notexture_mip;
                    output->flags = 0;
                }
            }
        }
    }

    public void CalcSurfaceExtents(msurface_t* s)
    {
        float[] mins = new float[2], maxs = new float[2];
        float val;
        int i, j, e;
        mvertex_t* v;
        mtexinfo_t* tex;
        int[] bmins = new int[2], bmaxs = new int[2];

        mins[0] = mins[1] = 999999;
        maxs[0] = maxs[1] = -99999;

        tex = s->texinfo;

        for (i = 0; i < s->numedges; i++)
        {
            e = loadmodel->surfedges[s->firstedge + i];

            if (e >= 0)
            {
                v = &loadmodel->vertexes[loadmodel->edges[e].v[0]];
            }
            else
            {
                v = &loadmodel->vertexes[loadmodel->edges[-e].v[1]];
            }

            for (j = 0; j < 2; j++)
            {
                val = v->position[0] * tex->vecs[j][0] + v->position[1] * tex->vecs[j][1] + v->position[2] + tex->vecs[j][3];

                if (val < mins[j])
                {
                    mins[j] = val;
                }

                if (val > maxs[j])
                {
                    maxs[j] = val;
                }
            }
        }

        for (i = 0; i < 2; i++)
        {
            bmins[i] = (int)MathF.Floor(mins[i] / 16);
            bmaxs[i] = (int)MathF.Ceiling(maxs[i] / 16);

            s->texturemins[i] = (short)(bmins[i] * 16);
            s->extents[i] = (short)((bmaxs[i] - bmins[i]) * 16);

            if ((tex->flags & bspfile_c.TEX_SPECIAL) == 0 && s->extents[i] > 256)
            {
                sys_win_c.Sys_Error("Bad surface extents");
            }
        }
    }

    public void Mod_LoadFaces(bspfile_c.lump_t* l)
    {
        bspfile_c.dface_t* input;
        msurface_t* output;

        int i, count, surfnum;
        int planenum, side;

        input = (void*)(mod_base + l->fileofs);

        if ((l->filelen % sizeof(bspfile_c.dface_t)) != 0)
        {
            sys_win_c.Sys_Error($"MOD_LoadBmodel: funny lump size in {loadmodel->name}");
        }

        count = l->filelen / sizeof(bspfile_c.dface_t);
        output = (msurface_t*)zone_c.Hunk_AllocName(count * sizeof(msurface_t), loadname.ToString());

        loadmodel->surfaces = output;
        loadmodel->numsurfaces = count;

        for (surfnum = 0; surfnum < count; surfnum++, input++, output++)
        {
            output->firstedge = common_c.LittleLong(input->firstedge);
            output->numedges = common_c.LittleShort(input->numedges);
            output->flags = 0;

            planenum = common_c.LittleShort(input->planenum);
            side = common_c.LittleShort(input->side);

            if (side != 0)
            {
                output->flags |= SURF_PLANEBACK;
            }

            output->plane = loadmodel->planes + planenum;

            output->texinfo = loadmodel->texinfo + common_c.LittleShort(input->texinfo);

            CalcSurfaceExtents(output);

            for (i = 0; i < bspfile_c.MAXLIGHTMAPS; i++)
            {
                output->styles[i] = input->styles[i];
            }

            i = common_c.LittleLong(input->lightofs);

            if (i == -1)
            {
                output->samples = null;
            }
            else
            {
                output->samples = loadmodel->lightdata + i;
            }

            if (common_c.Q_strncmp(output->texinfo->texture->name, common_c.StringToChar("sky"), 3) == 0)
            {
                output->flags |= (SURF_DRAWSKY | SURF_DRAWTILED);
                continue;
            }

            if (common_c.Q_strncmp(output->texinfo->texture->name, common_c.StringToChar("*"), 1) == 0)
            {
                output->flags |= (SURF_DRAWTURB | SURF_DRAWTILED);

                for (i = 0; i < 2; i++)
                {
                    output->extents[i] = 16384;
                    output->texturemins[i] = -8192;
                }

                continue;
            }
        }
    }

    public void Mod_SetParent(mnode_t* node, mnode_t* parent)
    {
        node->parent = parent;

        if (node->contents < 0)
        {
            return;
        }

        Mod_SetParent(&node->children[0], node);
        Mod_SetParent(&node->children[1], node);
    }

    public void Mod_LoadNodes(bspfile_c.lump_t* l)
    {
        int i, j, count, p;
        bspfile_c.dnode_t* input;
        mnode_t* output;

        input = (void*)(mod_base + l->fileofs);

        if ((l->filelen % sizeof(bspfile_c.dnode_t)) != 0)
        {
            sys_win_c.Sys_Error($"MOD_LoadBmodel: funny lump size in {loadmodel->name}");
        }

        count = l->filelen / sizeof(bspfile_c.dnode_t);
        output = (mnode_t*)zone_c.Hunk_AllocName(count * sizeof(mnode_t), loadname.ToString());

        loadmodel->nodes = output;
        loadmodel->numnodes = count;

        for (i = 0; i < count; i++, input++, output++)
        {
            for (j = 0; j < 3; j++)
            {
                output->minmaxs[j] = common_c.LittleShort(input->mins[j]);
                output->minmaxs[3 + j] = common_c.LittleShort(input->maxs[j]);
            }

            p = common_c.LittleLong(input->planenum);
            output->plane = loadmodel->planes + p;

            output->firstsurface = common_c.LittleShort(input->firstface);
            output->numsurfaces = common_c.LittleShort(input->numfaces);

            for (j = 0; j < 2; j++)
            {
                p = common_c.LittleShort(input->children[j]);

                if (p >= 0)
                {
                    output->children[j] = *(loadmodel->nodes + p);
                }
                else
                {
                    output->children[j] = (mnode_t*)(loadmodel->leafs + (-1 - p));
                }
            }
        }

        Mod_SetParent(loadmodel->nodes, null);
    }

    public void Mod_LoadLeafs(bspfile_c.lump_t* l)
    {
        bspfile_c.dleaf_t* input;
        mleaf_t* output;
        int i, j, count, p;

        input = (void*)(mod_base + l->fileofs);

        if ((l->filelen % sizeof(bspfile_c.dleaf_t)) != 0)
        {
            sys_win_c.Sys_Error($"MOD_LoadBmodel: funny lump size in {loadmodel->name}");
        }

        count = l->filelen / sizeof(bspfile_c.dleaf_t);
        output = (mleaf_t*)zone_c.Hunk_AllocName(count * sizeof(mleaf_t), loadname.ToString());

        loadmodel->leafs = output;
        loadmodel->numleafs = count;

        for (i = 0; i < count; i++, input++, output++)
        {
            for (j = 0; j < 3; j++)
            {
                output->minmaxs[j] = common_c.LittleShort(input->mins[j]);
                output->minmaxs[3 + j] = common_c.LittleShort(input->maxs[j]);
            }

            p = common_c.LittleLong(input->contents);
            output->contents = p;

            output->firstmarksurface = loadmodel->marksurfaces + common_c.LittleShort(input->firstmarksurface);
            output->nummarksurfaces = common_c.LittleShort(input->nummarksurfaces);

            p = common_c.LittleLong(input->visofs);

            if (p == -1)
            {
                output->compressed_vis = null;
            }
            else
            {
                output->compressed_vis = loadmodel->visdata + p;
            }

            output->efrags = null;

            for (j = 0; j < 4; j++)
            {
                output->ambient_sound_level[j] = input->ambient_level[j];
            }
        }
    }

    public void Mod_LoadClipNodes(bspfile_c.lump_t* l)
    {
        bspfile_c.dclipnode_t* input, output;
        int i, count;
        hull_t* hull;

        input = (void*)(mod_base + l->fileofs);

        if ((l->filelen % sizeof(bspfile_c.dclipnode_t)) != 0)
        {
            sys_win_c.Sys_Error($"MOD_LoadBmodel: funny lump size in {loadmodel->name}");
        }

        count = l->filelen / sizeof(bspfile_c.dclipnode_t);
        output = (bspfile_c.dclipnode_t*)zone_c.Hunk_AllocName(count * sizeof(bspfile_c.dclipnode_t), loadname);

        loadmodel->clipnodes = output;
        loadmodel->numclipnodes = count;

        hull = &loadmodel->hulls[1];
        hull->clipnodes = output;
        hull->firstclipnode = 0;
        hull->lastclipnode = count - 1;
        hull->planes = loadmodel->planes;
        hull->clip_mins[0] = -16;
        hull->clip_mins[1] = -16;
        hull->clip_mins[2] = -24;
        hull->clip_maxs[0] = 16;
        hull->clip_maxs[1] = 16;
        hull->clip_maxs[2] = 32;

        hull = &loadmodel->hulls[2];
        hull->clipnodes = output;
        hull->firstclipnode = 0;
        hull->lastclipnode = count - 1;
        hull->planes = loadmodel->planes;
        hull->clip_mins[0] = -32;
        hull->clip_mins[1] = -32;
        hull->clip_mins[2] = -24;
        hull->clip_maxs[0] = 32;
        hull->clip_maxs[1] = 32;
        hull->clip_maxs[2] = 64;

        for (i = 0; i < count; i++, input++, output++)
        {
            output->planenum = common_c.LittleLong(input->planenum);
            output->children[0] = common_c.LittleShort(input->children[0]);
            output->children[1] = common_c.LittleShort(input->children[1]);
        }
    }

    public void Mod_MakeHull0()
    {
        mnode_t* input, child;
        bspfile_c.dclipnode_t* output;
        int i, j, count;
        hull_t* hull;

        hull = &loadmodel->hulls[0];

        input = loadmodel->nodes;
        count = loadmodel->numnodes;
        output = zone_c.Hunk_AllocName(count * sizeof(bspfile_c.dclipnode_t), loadname.ToString());

        hull->clipnodes = output;
        hull->firstclipnode = 0;
        hull->lastclipnode = count - 1;
        hull->planes = loadmodel->planes;

        for (i = 0; i < count; i++, output++, input++)
        {
            output->planenum = input->plane - loadmodel->planes;

            for (j = 0; j < 2; j++)
            {
                child = &input->children[j];

                if (child->contents < 0)
                {
                    output->children[j] = child->contents;
                }
                else
                {
                    output->children[j] = child - loadmodel->nodes;
                }
            }
        }
    }

    public void Mod_LoadMarksurfaces(bspfile_c.lump_t* l)
    {
        int i, j, count;
        short* input;
        msurface_t** output;

        input = (void*)(mod_base + l->fileofs);

        if ((l->filelen % sizeof(short*)) != 0)
        {
            sys_win_c.Sys_Error($"MOD_LoadBmodel: funny lump size in {loadmodel->name}");
        }

        count = l->filelen / sizeof(short*);
        output = (msurface_t**)zone_c.Hunk_AllocName(count * sizeof(msurface_t), loadname.ToString());

        loadmodel->marksurfaces = output;
        loadmodel->nummarksurfaces = count;

        for (i = 0; i < count; i++)
        {
            j = common_c.LittleShort(input[i]);

            if (j >= loadmodel->numsurfaces)
            {
                sys_win_c.Sys_Error("Mod_ParseMarksurfaces: bad surface number");
            }

            output[i] = loadmodel->surfaces;
        }
    }

    public void Mod_LoadSurfedges(bspfile_c.lump_t* l)
    {
        int i, count;
        int* input, output;

        input = (void*)(mod_base + l->fileofs);

        if ((l->filelen % sizeof(int*)) != 0)
        {
            sys_win_c.Sys_Error($"MOD_LoadBmodel: funny lump size in {loadmodel->name}");
        }

        count = l->filelen / sizeof(int*);
        output = (int*)zone_c.Hunk_AllocName(count * sizeof(int*), loadname.ToString());

        loadmodel->surfedges = output;
        loadmodel->numsurfegdes = count;

        for (i = 0; i < count; i++)
        {
            output[i] = common_c.LittleLong(input[i]);
        }
    }

    public void Mod_LoadPlanes(bspfile_c.lump_t* l)
    {
        int i, j;
        mplane_t* output;
        bspfile_c.dplane_t* input;
        int count;
        int bits;

        input = (void*)(mod_base + l->fileofs);

        if ((l->filelen % sizeof(bspfile_c.dplane_t)) != 0)
        {
            sys_win_c.Sys_Error($"MOD_LoadBmodel: funny lump size in {loadmodel->name}");
        }

        count = l->filelen / sizeof(bspfile_c.dplane_t);
        output = (mplane_t*)zone_c.Hunk_AllocName(count * 2 * sizeof(mplane_t), loadname.ToString());

        loadmodel->planes = output;
        loadmodel->numplanes = count;

        for (i = 0; i < count; i++, input++, output++)
        {
            bits = 0;

            for (j = 0; j < 3; j++)
            {
                output->normal[j] = common_c.LittleFloat(input->normal[j]);

                if (output->normal[j] < 0)
                {
                    bits |= 1 << j;
                }
            }

            output->dist = common_c.LittleFloat(input->dist);
            output->type = common_c.LittleLong(input->type);
            output->signbits = (byte)bits;
        }
    }

    public float RadiusFromBounds(Vector3 mins, Vector3 maxs)
    {
        int i;
        Vector3 corner = new Vector3(0f);

        for (i = 0; i < 3; i++)
        {
            corner[i] = MathF.Abs(mins[i]) > MathF.Abs(maxs[i]) ? MathF.Abs(mins[i]) : MathF.Abs(maxs[i]);
        }

        return mathlib_c.Length(corner);
    }

    public void Mod_LoadBrushModel(model_t* mod, void* buffer)
    {
        int i, j;
        bspfile_c.dheader_t* header;
        bspfile_c.dmodel_t* bm;

        loadmodel->type = modtype_t.mod_brush;

        header = (bspfile_c.dheader_t*)buffer;

        i = common_c.LittleLong(header->version);

        if (i != bspfile_c.BSPVERSION)
        {
            sys_win_c.Sys_Error($"Mod_LoadBrushModel: {mod->name} has wrong version number ({i} should be {bspfile_c.BSPVERSION})");
        }

        mod_base = (byte*)header;

        for (i = 0; i < sizeof(bspfile_c.dheader_t) / 4; i++)
        {
            ((int*)header)[i] = common_c.LittleLong(((int*)header)[i]);
        }

        Mod_LoadVertexes(&header->lumps[bspfile_c.LUMP_VERTEXES]);
        Mod_LoadEdges(&header->lumps[bspfile_c.LUMP_EDGES]);
        Mod_LoadSurfedges(&header->lumps[bspfile_c.LUMP_SURFEDGES]);
        Mod_LoadTextures(&header->lumps[bspfile_c.LUMP_TEXTURES]);
        Mod_LoadLighting(&header->lumps[bspfile_c.LUMP_LIGHTING]);
        Mod_LoadPlanes(&header->lumps[bspfile_c.LUMP_PLANES]);
        Mod_LoadTexInfo(&header->lumps[bspfile_c.LUMP_TEXINFO]);
        Mod_LoadFaces(&header->lumps[bspfile_c.LUMP_FACES]);
        Mod_LoadMarksurfaces(&header->lumps[bspfile_c.LUMP_MARKSURFACES]);
        Mod_LoadVisibility(&header->lumps[bspfile_c.LUMP_VISIBILITY]);
        Mod_LoadLeafs(&header->lumps[bspfile_c.LUMP_LEAFS]);
        Mod_LoadNodes(&header->lumps[bspfile_c.LUMP_NODES]);
        Mod_LoadClipNodes(&header->lumps[bspfile_c.LUMP_CLIPNODES]);
        Mod_LoadEntities(&header->lumps[bspfile_c.LUMP_ENTITIES]);
        Mod_LoadSubModels(&header->lumps[bspfile_c.LUMP_MODELS]);

        Mod_MakeHull0();

        mod->numframes = 2;
        mod->flags = 0;

        for (i = 0; i < mod->numsubmodels; i++)
        {
            bm = &mod->submodels[i];

            mod->hulls[0].firstclipnode = bm->headnode[0];

            for (j = 1; j < bspfile_c.MAX_MAP_HULLS; j++)
            {
                mod->hulls[j].firstclipnode = bm->headnode[j];
                mod->hulls[j].lastclipnode = mod->numclipnodes - 1;
            }

            mod->firstmodelsurface = bm->firstsurface;
            mod->nummodelsurfaces = bm->numsurfaces;

            mathlib_c.VectorCopy(bm->maxs, mod->maxs);
            mathlib_c.VectorCopy(bm->mins, mod->mins);
            mod->radius = RadiusFromBounds(mod->mins, mod->maxs);

            mod->numleafs = bm->visleafs;

            if (i < mod->numsubmodels - 1)
            {
                char* name = null;

                Console.WriteLine(name->ToString(), $"{i + 1}");
                loadmodel = Mod_FindName(name);
                *loadmodel = *mod;
                common_c.Q_strcpy(loadmodel->name.ToString(), name->ToString());
                mod = loadmodel;
            }
        }
    }

    public void* Mod_LoadAliasFrame(void* pin, int* pframeindex, int numv, modelgen_c.trivertx_t* pbboxmin, modelgen_c.trivertx_t* pbboxmax, aliashdr_t* pheader, char* name)
    {
        modelgen_c.trivertx_t* pframe, pinframe;
        int i, j;
        modelgen_c.daliasframe_t* pdaliasframe;

        pdaliasframe = (modelgen_c.daliasframe_t*)pin;

        common_c.Q_strcpy(name, pdaliasframe->name);

        for (i = 0; i < 3; i++)
        {
            pbboxmin->v[i] = pdaliasframe->bboxmin.v[i];
            pbboxmax->v[i] = pdaliasframe->bboxmax.v[i];
        }

        pinframe = (modelgen_c.trivertx_t*)(pdaliasframe + 1);
        pframe = zone_c.Hunk_AllocName(numv * sizeof(modelgen_c.trivertx_t), loadname.ToString());

        *pframeindex = (int)((byte*)pframe - (byte*)pheader);

        for (j = 0; j < numv; j++)
        {
            int k;

            pframe[j].lightnormalindex = pinframe[j].lightnormalindex;

            for (k = 0; k < 3; k++)
            {
                pframe[j].v[k] = pinframe[j].v[k];
            }
        }

        pinframe += numv;

        return (void*)pinframe;
    }

    public void* Mod_LoadAliasGroup(void* pin, int* pframeindex, int numv, modelgen_c.trivertx_t* pbboxmin, modelgen_c.trivertx_t* pbboxmax, aliashdr_t* pheader, char* name)
    {
        modelgen_c.daliasgroup_t* pingroup;
        maliasgroup_t* paliasgroup;
        int i, numframes;
        modelgen_c.daliasinterval_t* pin_intervals;
        float* poutintervals;
        void* ptemp;

        pingroup = (modelgen_c.daliasgroup_t*)pin;

        numframes = common_c.LittleLong(pingroup->numframes);

        paliasgroup = (maliasgroup_t*)zone_c.Hunk_AllocName(sizeof(maliasgroup_t) + (numframes - 1) * sizeof(maliasgroup_t), loadname.ToString());

        paliasgroup->numframes = numframes;

        for (i = 0; i < 3; i++)
        {
            pbboxmin->v[i] = pingroup->bboxmin.v[i];
            pbboxmax->v[i] = pingroup->bboxmax.v[i];
        }

        *pframeindex = (int)((byte*)paliasgroup - (byte*)pheader);

        pin_intervals = (modelgen_c.daliasinterval_t*)(pingroup + 1);

        poutintervals = (float*)zone_c.Hunk_AllocName(numframes * sizeof(float), loadname.ToString());

        paliasgroup->intervals = (int)((byte*)poutintervals - (byte*)pheader);

        for (i = 0; i < numframes; i++)
        {
            *poutintervals = common_c.LittleFloat(pin_intervals->interval);

            if (*poutintervals <= 0.0f)
            {
                sys_win_c.Sys_Error("Mod_LoadAliasGroup: interval <= 0");
            }

            poutintervals++;
            pin_intervals++;
        }

        ptemp = pin_intervals;

        for (i = 0; i < numframes; i++)
        {
            ptemp = Mod_LoadAliasFrame(ptemp, &paliasgroup->frames[i].frame, numv, &paliasgroup->frames[i].bboxmin, &paliasgroup->frames[i].bboxmax, pheader, name);
        }

        return ptemp;
    }

    public void* Mod_LoadAliasSkin(void* pin, int* pskinindex, int skinsize, aliashdr_t* pheader)
    {
        int i;
        byte* pskin, pinskin;
        ushort* pusskin;

        pskin = zone_c.Hunk_AllocName(skinsize * r_main_c.r_pixbytes, loadname.ToString());
        pinskin = (byte*)pin;
        *pskinindex = (int)(pskin - (byte*)pheader);

        if (r_main_c.r_pixbytes == 1)
        {
            common_c.Q_memcpy(*pskin, *pinskin, skinsize);
        }
        else if (r_main_c.r_pixbytes == 2)
        {
            pusskin = (ushort*)pskin;

            for (i = 0; i < skinsize; i++)
            {
                pusskin[i] = vid_win_c.d_8to16table[pinskin[i]];
            }
        }
        else
        {
            sys_win_c.Sys_Error($"Mod_LoadAliasSkin: driver set invalid r_pixbytes: {r_main_c.r_pixbytes}\n");
        }

        pinskin += skinsize;

        return pinskin;
    }

    public void* Mod_LoadAliasSkinGroup(void* pin, int* pskinindex, int skinsize, aliashdr_t* pheader)
    {
        modelgen_c.daliasskingroup_t* pinskingroup;
        maliasskingroup_t* paliasskingroup;
        int i, numskins;
        modelgen_c.daliasskininterval_t* pinskinintervals;
        float* poutskinintervals;
        void* ptemp;

        pinskingroup = (modelgen_c.daliasskingroup_t*)pin;

        numskins = common_c.LittleLong(pinskingroup->numskins);

        paliasskingroup = (maliasskingroup_t*)zone_c.Hunk_AllocName(sizeof(maliasskingroup_t) + (numskins - 1) * sizeof(maliasskindesc_t), loadname.ToString());

        paliasskingroup->numskins = numskins;

        *pskinindex = (int)((byte*)paliasskingroup - (byte*)pheader);

        pinskinintervals = (modelgen_c.daliasskininterval_t*)(pinskingroup + 1);

        poutskinintervals = (float*)zone_c.Hunk_AllocName(numskins * sizeof(float), loadname.ToString());

        paliasskingroup->intervals = (int)((byte*)poutskinintervals - (byte*)pheader);

        for (i = 0; i < numskins; i++)
        {
            *poutskinintervals = common_c.LittleFloat(pinskinintervals->interval);

            if (*poutskinintervals <= 0)
            {
                sys_win_c.Sys_Error("Mod_LoadAliasSkinGroup: interval <= 0");
            }

            poutskinintervals++;
            pinskinintervals++;
        }

        ptemp = pinskinintervals;

        for (i = 0; i < numskins; i++)
        {
            ptemp = Mod_LoadAliasSkin(ptemp, &paliasskingroup->skindescs[i].skin, skinsize, pheader);
        }

        return ptemp;
    }

    public void Mod_LoadAliasModel(model_t* mod, void* buffer)
    {
        int i;
        modelgen_c.mdl_t* pmodel, pinmodel;
        modelgen_c.stvert_t* pstverts, pinstverts;
        aliashdr_t* pheader;
        mtriangle_t* ptri;
        modelgen_c.dtriangle_t* pintriangles;
        int version, numframes, numskins;
        int size;
        modelgen_c.daliasframetype_t* pframetype;
        modelgen_c.daliasskintype_t* pskintype;
        maliasskindesc_t* pskindesc;
        int skinsize;
        int start, end, total;

        start = zone_c.Hunk_LowMark();
    }
}