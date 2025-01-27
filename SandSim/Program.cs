using SandSim.Monogame;

namespace SandSim;

class Program
{
    static void Main(string[] args)
    {
        using MonogameInstance game = new();
        game.Run();
    }
}