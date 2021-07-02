using HeroesInfoLibrary;
using System;

namespace HeroesInfoLibraryTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var library = new HeroesLibrarian("../../../../Heroes-talents/heroes-talents-master/hero", "HeroesOfTheStormHeroData.db");
            var input = Console.ReadLine();
            while (input != "q")
            {
                var results = library.GetAbilityAndTalentDataByString(input);
                results.ForEach(r => Console.WriteLine(r.ToString()));
                Console.WriteLine("*********************");
                input = Console.ReadLine();
            }
        }
    }
}
