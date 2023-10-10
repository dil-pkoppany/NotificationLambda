namespace NotificationTablePopulationLambda.UnitTests.Helpers
{
    public static class RandomIntGenerator
    {
        /// <summary>
        /// Generates a new integer between <paramref name="min"/> inclusive
        /// and <paramref name="max"/> exclusive
        public static int Generate(int min, int max)
        {
            Random random = new();
            return random.Next(min, max);
        }

        /// <summary>
        /// Generates random positive integer
        /// </summary>
        public static int GeneratePositive() => Generate(0, int.MaxValue);
    }
}
