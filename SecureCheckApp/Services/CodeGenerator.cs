using System;
using System.Linq;

namespace SecureCheckApp.Services
{
    public class CodeGenerator
    {
        private static readonly Random _random = new Random();
        private static readonly string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string GenerateCode()
        {
            var part1 = GeneratePart();
            var part2 = GeneratePart();
            var part3 = GeneratePart();
            
            return $"{part1}-{part2}-{part3}";
        }

        private static string GeneratePart()
        {
            return new string(Enumerable.Range(0, 3)
                .Select(_ => _chars[_random.Next(_chars.Length)])
                .ToArray());
        }
    }
}
