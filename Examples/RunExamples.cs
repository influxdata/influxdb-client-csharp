using System;
using System.Threading.Tasks;

namespace Examples
{
    public class RunExamples
    {
        /// <summary>
        /// specify name of example in configuration Program arguments e.g. FluxExample
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            if (args.Length >= 1 && !string.IsNullOrEmpty(args[0]))
            {
                Console.WriteLine("Run solution: " + args[0]);

                switch (args[0])
                {
                    case "FluxExample":
                        FluxExample.Run().Wait();
                        break;
                    case "FluxClientSimpleExample":
                        FluxClientSimpleExample.Run().Wait();
                        break;
                    case "FluxRawExample":
                        FluxRawExample.Run().Wait();
                        break;
                    case "FluxClientFactoryExample":
                        FluxClientFactoryExample.Run().Wait();
                        break;
                    case "FluxClientPocoExample":
                        FluxClientPocoExample.Run().Wait();
                        break;
                    case "PlatformExample":
                        PlatformExample.Run().Wait();
                        break;
                    case "WriteApiAsyncExample":
                        WriteApiAsyncExample.Run().Wait();
                        break;
                }
            }
        }
    }
}