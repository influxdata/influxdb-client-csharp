using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Generated.Domain;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Internal
{
    public class AbstractInfluxDBClient : AbstractClient
    {
        protected AbstractInfluxDBClient()
        {
        }

        protected AbstractInfluxDBClient(DefaultClientIo client) : base(client)
        {
        }

        protected Check GetHealth(Task<Check> task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            try
            {
                return task.Result;
            }
            catch (Exception e)
            {
                return new Check("influxdb", e.GetBaseException().Message, default(List<Check>), Check.StatusEnum.Fail);
            }
        }

        protected async Task<List<Label>> GetLabels(string resourceId, string resourcePath)
        {
            Arguments.CheckNonEmptyString(resourceId, nameof(resourceId));
            Arguments.CheckNonEmptyString(resourcePath, nameof(resourcePath));

            var request = await Get($"/api/v2/{resourcePath}/{resourceId}/labels");

            return Call<LabelsResponse>(request)?.Labels;
        }

        protected async Task<Label> AddLabel(string labelId, string resourceId, string resourcePath,
            ResourceType resourceType)

        {
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));
            Arguments.CheckNonEmptyString(resourceId, nameof(resourceId));
            Arguments.CheckNonEmptyString(resourcePath, nameof(resourcePath));
            Arguments.CheckNotNull(resourceType, nameof(resourceType));

            var labelMapping = new LabelMapping {LabelID = labelId};

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