using System;
using Flux.Examples.Examples;

namespace Flux.Examples
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
                        FluxExample.Run();
                        break;
                    case "FluxClientSimpleExample":
                        FluxClientSimpleExample.Run();
                        break;
                    case "FluxRawExample":
                        FluxRawExample.Run();
                        break;
                    case "FluxClientFactoryExample":
                        FluxClientFactoryExample.Run();
                        break;
                    case "FluxClientPocoExample":
                        FluxClientPocoExample.Run();
                        break;
                    case "PlatformExample":
                        PlatformExample.Run();
                        break;
                }
            }
        }
    }
}