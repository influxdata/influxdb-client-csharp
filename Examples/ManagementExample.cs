using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Task = System.Threading.Tasks.Task;

namespace Examples
{
    public static class ManagementExample
    {
        public static async Task Main(string[] args)
        {
            const string url = "http://localhost:8086";
            const string token = "my-token";
            const string org = "my-org";

            using var client = InfluxDBClientFactory.Create(url, token);

            // Find ID of Organization with specified name (PermissionAPI requires ID of Organization).
            var orgId = (await client.GetOrganizationsApi().FindOrganizationsAsync(org: org)).First().Id;

            //
            // Create bucket "iot_bucket" with data retention set to 3,600 seconds
            //
            var retention = new BucketRetentionRules(BucketRetentionRules.TypeEnum.Expire, 3600);

            var bucket = await client.GetBucketsApi().CreateBucketAsync("iot_bucket", retention, orgId);

            //
            // Create access token to "iot_bucket"
            //
            var resource = new PermissionResource(PermissionResource.TypeBuckets, bucket.Id, null,
                orgId);

            // Read permission
            var read = new Permission(Permission.ActionEnum.Read, resource);

            // Write permission
            var write = new Permission(Permission.ActionEnum.Write, resource);

            var authorization = await client.GetAuthorizationsApi()
                .CreateAuthorizationAsync(orgId, new List<Permission> { read, write });

            //
            // Created token that can be use for writes to "iot_bucket"
            //
            Console.WriteLine($"Authorized token to write into iot_bucket: {authorization.Token}");
        }
    }
}