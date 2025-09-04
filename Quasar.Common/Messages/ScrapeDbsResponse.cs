using ProtoBuf;

namespace Quasar.Common.Messages
{
    [ProtoContract]
    public class ScrapeDbsResponse : IMessage
    {
        [ProtoMember(1)]
        public string[] FilePaths { get; set; }
    }
}


