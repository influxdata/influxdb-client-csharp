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
                        await FluxExample.Main();
                        break;
                    case "FluxClientSimpleExample":
                        await FluxClientSimpleExample.Main();
                        break;
                    case "FluxRawExample":
                        await FluxRawExample.Main();
                        break;
                    case "FluxClientFactoryExample":
                        await FluxClientExample.Main();
                        break;
                    case "FluxClientPocoExample":
                        await FluxClientPocoExample.Main();
                        break;
                    case "PlatformExample":
                        await PlatformExample.Main();
                        break;
                    case "WriteEventHandlerExample":
                        await WriteEventHandlerExample.Main();
                        break;
                    case "WriteApiAsyncExample":
                        await WriteApiAsyncExample.Main();
                        break;
                    case "PocoQueryWriteExample":
                        await PocoQueryWriteExample.Main();
                        break;
                    case "CustomDomainMappingAndLinq":
                        await CustomDomainMappingAndLinq.Main();
                        break;
                    case "InfluxDB18Example":
                        await InfluxDB18Example.Main();
                        break;
                    case "SynchronousQuery":
                        SynchronousQuery.Main();
                        break;
                    case "CustomDomainMapping":
                        await CustomDomainMapping.Main();
                        break;
                    case "QueryLinqCloud":
                        QueryLinqCloud.Main();
                        break;
                    case "ManagementExample":
                        await ManagementExample.Main();
                        break;
                    case "InvokableScripts":
                        await InvokableScripts.Main();
                        break;
                    case "ParametrizedQuery":
                        await ParametrizedQuery.Main();
                        break;
                    case "RecordRowExample":
                        await RecordRowExample.Main();
                        break;
                    case "HttpErrorHandling":
                        await HttpErrorHandling.Main();
                        break;
                }
            }
            else
            {
                Console.WriteLine("Please specify the name of example. One of: " +
                                  "FluxExample, FluxClientSimpleExample, FluxRawExample, FluxClientFactoryExample, " +
                                  "FluxClientPocoExample, PlatformExample, WriteEventHandlerExample, WriteApiAsyncExample, " +
                                  "CustomDomainMapping, PocoQueryWriteExample, CustomDomainMappingAndLinq, " +
                                  "SynchronousQuery, InfluxDB18Example, QueryLinqCloud, ManagementExample, " +
                                  " InvokableScripts, ParametrizedQuery, RecordRowExample");
            }
        }
    }
}