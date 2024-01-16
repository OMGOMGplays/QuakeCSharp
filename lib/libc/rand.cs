namespace lib.libc;

public unsafe class rand_c
{
    static uint randseed = 12345;

    public static void srand(uint seed)
    {
        randseed = seed;
    }

    //public static void rand_add_entropy(Action* buf, )

    public static int rand()
    {
        return (int)(randseed = randseed * 1664525 + 1013904223);
    }
}