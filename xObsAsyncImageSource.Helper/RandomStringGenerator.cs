using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace xObsAsyncImageSource.Helper
{
    public static class RandomStringGenerator
    {
        // Thread-local Random object to ensure thread safety
        private static readonly ThreadLocal<Random> random = new(() => new Random());

        // Use ConcurrentDictionary to store generated strings in a thread-safe manner
        private static ConcurrentDictionary<string, bool> generatedStrings = new ConcurrentDictionary<string, bool>();

        public static string GetRandomString(int length)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder stringBuilder = new StringBuilder();
            string newString;

            do
            {
                stringBuilder.Clear();
                for (int i = 0; i < length; i++)
                {
                    int index = random.Value!.Next(chars.Length);
                    stringBuilder.Append(chars[index]);
                }
                newString = stringBuilder.ToString();
            } while (!generatedStrings.TryAdd(newString, true));

            return newString;
        }
    }
}
