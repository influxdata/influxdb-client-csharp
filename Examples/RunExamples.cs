using System;
using System.Threading.Tasks;

namespace Examples
{
    public static class RunExamples
    {
        /// <summary>
        /// specify name of example in configuration Program arguments e.g. FluxExample
        /// </summary>
        /// <param name="args"></param>
        public static async Task Main(string[] args)
        {
            if (args.Length >= 1 && !string.IsNullOrEmpty(args[0]))
            {
                Console.WriteLine($"Run solution: {args[0]}");
                Console.WriteLine("====================================");

                switch (args[0])
                {
                    case "FluxExample":
                        await FluxExample.Main(args);
                        break;
                    case "FluxClientSimpleExample":
                        await FluxClientSimpleExample.Main(args);
                        break;
                    case "FluxRawExample":
                        await FluxRawExample.Main(args);
                        break;
                    case "FluxClientFactoryExample":
                        await FluxClientFactoryExample.Main(args);
                        break;
                    case "FluxClientPocoExample":
                        await FluxClientPocoExample.Main(args);
                        break;
                    case "PlatformExample":
                        await PlatformExample.Main(args);
                        break;
                    case "WriteApiAsyncExample":
                        await WriteApiAsyncExample.Main(args);
                        break;
                    case "PocoQueryWriteExample":
                        await PocoQueryWriteExample.Main(args);
                        break;
                    case "CustomDomainMappingAndLinq":
                        await CustomDomainMappingAndLinq.Main(args);
                        break;
                    case "InfluxDB18Example":
                        await InfluxDB18Example.Main(args);
                        break;
                    case "SynchronousQuery":
                        SynchronousQuery.Main(args);
                        break;
                    case "CustomDomainMapping":
                        await CustomDomainMapping.Main(args);
                        break;
                    case "QueryLinqCloud":
                        QueryLinqCloud.Main(args);
                        break;
                    case "ManagementExample":
                        await ManagementExample.Main(args);
                        break;
                    case "InvokableScripts":
                        await InvokableScripts.Main(args);
                        break;
                    case "ParametrizedQuery":
                        await ParametrizedQuery.Main(args);
                        break;
                }
            }
            else
            {
                Console.WriteLine("Please specify the name of example. One of: " +
                                  "FluxExample, FluxClientSimpleExample, FluxRawExample, FluxClientFactoryExample, " +
                                  "FluxClientPocoExample, PlatformExample, WriteApiAsyncExample, CustomDomainMapping" +
                                  "PocoQueryWriteExample, CustomDomainMappingAndLinq, SynchronousQuery, InfluxDB18Example, " +
                                  "QueryLinqCloud, ManagementExample, InvokableScripts, ParametrizedQuery");
            }
        }
    }
}