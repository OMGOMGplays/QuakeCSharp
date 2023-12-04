namespace Quake;

public unsafe class r_edge_c
{
    public static r_shared_c.edge_t* auxedges;
    public static r_shared_c.edge_t* r_edges, edge_p, edge_max;

    public static r_shared_c.surf_t* surfaces, surface_p, surf_max;

    public static r_shared_c.edge_t* newedges;
    public static r_shared_c.edge_t* removeedges;

    public static r_shared_c.espan_t* span_p, max_span_p;

    public static int r_currentkey;

    public static int screenwidth;

    public static int current_iv;

    public static int edge_head_u_shift20, edge_tail_u_shift20;

    public static r_shared_c.edge_t edge_head;
    public static r_shared_c.edge_t edge_tail;
    public static r_shared_c.edge_t edge_aftertail;
    public static r_shared_c.edge_t edge_sentinel;

    public static float fv;

    public static void R_DrawCulledPolys()
    {
        r_shared_c.surf_t* s;
        model_c.msurface_t* pface;

        r_shared_c.currententity = &cl_main_c.cl_entities[0];

        if (r_main_c.r_worldpolysbacktofront)
        {
            for (s = surface_p - 1; s > &surfaces[1]; s--)
            {
                if (s->spans == null)
                {
                    continue;
                }

                if ((s->flags & model_c.SURF_DRAWBACKGROUND) == 0)
                {
                    pface = (model_c.msurface_t*)s->data;
                    r_draw_c.R_RenderPoly(pface, 15);
                }
            }
        }
    }
}