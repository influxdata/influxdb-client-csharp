using System;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;

namespace InfluxData.Platform.Client.Client
{
    public class AbstractPlatformClient : AbstractClient
    {
        protected AbstractPlatformClient()
        {
        }

        protected AbstractPlatformClient(DefaultClientIo client) : base(client)
        {
        }

        protected async Task<Health> GetHealth(string path)
        {
            Arguments.CheckNonEmptyString(path, nameof(path));
            
            try
            {
                var request = await Get(path);
                
                return Call<Health>(request);
            }
            catch (Exception e)
            {
                return new Health {Status = "error", Message = e.Message};
            }
        }
    }
}