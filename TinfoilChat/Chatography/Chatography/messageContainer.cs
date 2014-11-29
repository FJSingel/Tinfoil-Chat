using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatography
{
    public class MessageContainer
    {
        enum MessageType {Internal, Chat};

        MessageType messageType;
        string message;

        public MessageContainer(MessageType type, string rawMessage)
        {
            messageType = type;
            message = rawMessage;
        }

        public MessageContainer(string networkMessage)
        {
            switch (networkMessage.ElementAt(0))
            {
                case '@':
                    messageType = MessageType.Chat;
                    break;
                case '&':
                    messageType = MessageType.Internal;
                    break;
                default:
                    //throw an error
                    break;
            }
            message = networkMessage.Substring(1);
        }

        public string ToString()
        {
            if (messageType == MessageType.Chat)
            {
                return '@' + message;
            }
            else if (messageType == MessageType.Internal)
            {
                return '&' + message;
            }
            else return message;
        }
    }
}
