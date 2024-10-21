using System;
using System.Threading.Tasks;

namespace DDPM.CMA.Tester
{
    internal class Program
    {

        private static async Task Main(string[] args)
        {

            Console.WriteLine("CMA SubAgent");
            // CMA call DDPM
            CMAAgent agent = new CMAAgent(Guid.NewGuid(), Guid.NewGuid());

            await agent.StartAsync(args).ConfigureAwait(false);
        }
    }
}