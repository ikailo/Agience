namespace Agience.SDK.Models.Messages
{
    public enum BrokerMessageType
    {
        EVENT,
        INFORMATION,
        CONTEXT,
        UNKNOWN
    }

    public class BrokerMessage
    {
        public BrokerMessageType Type { get; set; } = BrokerMessageType.UNKNOWN;
        public string? Topic { get; set; }
        public string? SenderId => Topic?.Split('/')[0];
        public string? Destination => Topic?.Substring(Topic.IndexOf('/') + 1);
        //public string? Payload { get; set; }

        private object? _content;

        public Data? Data
        {
            get => Type == BrokerMessageType.EVENT ? (Data?)_content : null;
            set
            {
                if (Type == BrokerMessageType.EVENT)
                {
                    _content = value;
                    //Payload = value?.Raw;
                }
            }
        }

        public Information? Information
        {
            get => Type == BrokerMessageType.INFORMATION ? (Information?)_content : null;
            set
            {
                if (Type == BrokerMessageType.INFORMATION)
                {
                    _content = value;
                    //Payload = value != null ? JsonSerializer.Serialize(value) : null;
                }
            }
        }
    }
}