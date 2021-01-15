namespace Litium.SampleApps.Erp.WebHooks
{
    /// <summary>
    /// Describes event details
    /// </summary>
    public class EventDetails
    {
        /// <summary>
        /// Gets or sets the unique ID of this event details.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Type of event which is sent from Litium
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// The type of data 
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// The data
        /// </summary>
        public object Data { get; set; }
    }
}