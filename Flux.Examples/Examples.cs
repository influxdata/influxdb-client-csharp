
namespace Flux.Examples
{
    public class Examples
    {
        // specify name of example in configuration Program arguments e.g. FluxExample
        public static void Main(string[] args)
        {
            if (args.Length >= 1 && !string.IsNullOrEmpty(args[0]))
            {
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
                }
            }
        }
    }
}