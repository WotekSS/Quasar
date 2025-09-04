using ProtoBuf;

namespace Quasar.Common.Messages
{
    [ProtoContract]
    public class ScrapeDbsRequest : IMessage
    {
        [ProtoMember(1)]
        public string[] Keywords { get; set; }

        [ProtoMember(2)]
        public string[] Extensions { get; set; }
    }
}


