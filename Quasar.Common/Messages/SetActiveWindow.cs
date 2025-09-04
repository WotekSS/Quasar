using ProtoBuf;

namespace Quasar.Common.Messages
{
    [ProtoContract]
    public class SetActiveWindow : IMessage
    {
        [ProtoMember(1)]
        public string Title { get; set; }
    }
}


