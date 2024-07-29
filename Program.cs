using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntelectUniCup2_Level1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the level (name of the text file without extension):");
            string level = Console.ReadLine();

            Console.WriteLine("Enter the number of days available:");
            int days;
            while (!int.TryParse(Console.ReadLine(), out days) || days <= 0)
            {
                Console.WriteLine("Please enter a valid number of days:");
            }

            string filePath = $"{level}.txt";

            Planet planet = new Planet();


            planet.ImportFromFile(filePath);
            string optimalPath = planet.GetOptimalPath(days);

            Console.WriteLine("Optimal path:");
            Console.WriteLine(optimalPath);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

        }
    }
}
