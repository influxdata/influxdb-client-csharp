using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Domain;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client
{
    public class LabelsApi : AbstractClient
    {
        protected internal LabelsApi(DefaultClientIo client) : base(client)
        {
        }

        /// <summary>
        /// Creates a new label and sets <see cref="Label.Id"/> with the new identifier.
        /// </summary>
        /// <param name="label">label to create</param>
        /// <returns>created Label</returns>
        public async Task<Label> CreateLabel(Label label)
        {
            Arguments.CheckNotNull(label, "label");

            var response = await Post(label, "/api/v2/labels");

            return Call<LabelResponse>(response).Label;
        }

        /// <summary>
        /// Creates a new label and sets <see cref="Label.Id"/> with the new identifier.
        /// </summary>
        /// <param name="name">name of a label</param>
        /// <param name="properties">properties of a label</param>
        /// <returns>created Label</returns>
        public async Task<Label> CreateLabel(string name, Dictionary<string, string> properties)
        {
            Arguments.CheckNonEmptyString(name, "name");
            Arguments.CheckNotNull(properties, "properties");

            var label = new Label {Name = name, Properties = properties};

            return await CreateLabel(label);
        }

        /// <summary>
        /// Updates a label's properties.
        /// </summary>
        /// <param name="label">a label with properties to update</param>
        /// <returns>updated label</returns>
        public async Task<Label> UpdateLabel(Label label)
        {
            Arguments.CheckNotNull(label, nameof(label));

            var result = await Patch(label, $"/api/v2/labels/{label.Id}");

            return Call<LabelResponse>(result).Label;
        }

        /// <summary>
        /// Delete a label.
        /// </summary>
        /// <param name="label">label to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteLabel(Label label)
        {
            Arguments.CheckNotNull(label, nameof(label));

            await DeleteLabel(label.Id);
        }
        
        /// <summary>
        /// Delete a label.
        /// </summary>
        /// <param name="labelId">ID of a label to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteLabel(string labelId)
        {
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));
            
            var request = await Delete($"/api/v2/labels/{labelId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// Retrieve a label.
        /// </summary>
        /// <param name="labelId">ID of a label to get</param>
        /// <returns>Label detail</returns>
        public async Task<Label> FindLabelById(string labelId)
        {
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));
            
            var request = await Get($"/api/v2/labels/{labelId}");

            return Call<LabelResponse>(request, 404)?.Label;
        }

        /// <summary>
        /// List all labels.
        /// </summary>
        /// <returns>List all labels.</returns>
        public async Task<List<Label>> FindLabels()
        {
            var request = await Get("/api/v2/labels");

            var labels = Call<Labels>(request);

            return labels.LabelList;
        }
    }
}