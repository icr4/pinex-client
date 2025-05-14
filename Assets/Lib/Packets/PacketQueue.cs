using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phoenix;

namespace Packets {
    public abstract class PacketQueue
    {
        public Channel channel;
        public string topic;
        public Reply reply;
        public Message message;
    }

    public class ReplyQueue : PacketQueue
    {
        public ReplyQueue(Channel buildChannel, string buildTopic, Reply buildReply)
        {
            this.channel = buildChannel;
            this.topic = buildTopic;
            this.reply = buildReply;
        }
    }

    public class MessageQueue : PacketQueue
    {
        public MessageQueue(Channel buildChannel, string buildTopic, Message buildMessage)
        {
            this.channel = buildChannel;
            this.topic = buildTopic;
            this.message = buildMessage;
        }
    }
}
