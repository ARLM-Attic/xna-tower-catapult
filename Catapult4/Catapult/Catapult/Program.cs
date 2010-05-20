using System;

namespace CatapultMiniGame
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (CatapultGame game = new CatapultGame())
            {
                game.Run();
            }
        }
    }
#endif
}

