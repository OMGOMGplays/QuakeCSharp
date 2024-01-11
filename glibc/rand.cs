namespace stdlib;

public class rand_c
{
    public static int rand()
    {
        Random random = new();
        return random.Next();
    }
}