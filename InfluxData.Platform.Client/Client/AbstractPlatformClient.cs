using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;
using Task = System.Threading.Tasks.Task;

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

        protected async Task<List<Label>> GetLabels(string resourceId, string resourcePath)
        {
            Arguments.CheckNonEmptyString(resourceId, nameof(resourceId));
            Arguments.CheckNonEmptyString(resourcePath, nameof(resourcePath));

            var request = await Get($"/api/v2/{resourcePath}/{resourceId}/labels");

            return Call<Labels>(request)?.LabelList;
        }

        protected async Task<Label> AddLabel(string labelId, string resourceId, string resourcePath,
            ResourceType resourceType)

        {
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));
            Arguments.CheckNonEmptyString(resourceId, nameof(resourceId));
            Arguments.CheckNonEmptyString(resourcePath, nameof(resourcePath));
            Arguments.CheckNotNull(resourceType, nameof(resourceType));

            var labelMapping = new LabelMapping {LabelId = labelId, ResourceType = resourceType};

            var request = await Post(labelMapping, $"/api/v2/{resourcePath}/{resourceId}/labels");

            return Call<LabelResponse>(request)?.Label;
        }

        protected async Task DeleteLabel(string labelId, string resourceId, string resourcePath)
        {
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));
            Arguments.CheckNonEmptyString(resourceId, nameof(resourceId));
            Arguments.CheckNonEmptyString(resourcePath, nameof(resourcePath));

            var request = await Delete($"/api/v2/{resourcePath}/{resourceId}/labels/{labelId}");

            RaiseForInfluxError(request);
        }

        protected OperationLogEntries GetOperationLogEntries(RequestResult request)
        {
            Arguments.CheckNotNull(request, nameof(request));

            //TODO https://github.com/influxdata/influxdb/issues/11632
            var entries = Call<OperationLogEntries>(request, "oplog not found");

            return entries ?? new OperationLogEntries();
        }

        protected string CreateQueryString(FindOptions findOptions)
        {
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return $"{FindOptions.LimitKey}={findOptions.Limit}&" +
                   $"{FindOptions.OffsetKey}={findOptions.Offset}&" +
                   $"{FindOptions.SortByKey}={findOptions.SortBy}&" +
                   $"{FindOptions.DescendingKey}={findOptions.Descending}";
        }
    }
}