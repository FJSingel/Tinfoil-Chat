using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChatUIBackend
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ChatBackend : IChatBackend
    {
        #region Receiving Messages

        DisplyMessageDelegate _displayMessageDelegate = null;

        /// <summary>
        /// The default constructore is only here for testing
        /// </summary>

        private ChatBackend() { }

        /// <summary>
        /// ChatBackend constructor should be called with a delegate is capabale 
        /// of displaying messages
        /// </summary>

        public ChatBackend(DisplyMessageDelegate dmd)
        {
            _displayMessageDelegate = dmd;
            StartService();
        }

        /// <summary>
        /// This method gets called by our friend when they wan tto display a message
        /// on our screen.
        /// 
        /// We really only returning a string for demonstration purposes.
        /// Might be cleaner to return void and make this a one-way comm channel
        /// </summary>
        /// <param name="composite"></param>
        public void DisplayMessage(CompositeType composite) 
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (_displayMessageDelegate != null)
            {
                _displayMessageDelegate(composite);
            }
        }
        #endregion

        #region Sending messages

        private string _myUserName = "Anonymous";
        private ServiceHost host = null;
        private ChannelFactory<IChatBackend> channelFactory = null;
        private IChatBackend _channel;

        /// <summary>
        /// The front-end calls the SendMessage method in order to broadcast 
        /// a message to our friends
        /// </summary>
        /// <param name="text"</param>
        public void SendMessage(string text)
        {
            if(text.StartsWith("setname:", StringComparison.OrdinalIgnoreCase))
            {
                _myUserName = text.Substring("setname:".Length).Trim();
                _displayMessageDelegate(new CompositeType("Event", "Setting your name to "
                    + _myUserName));
            }
            else
            {
                // In order to send a message, we call our friends' DisplayMessage method
                _channel.DisplayMessage(new CompositeType(_myUserName, text));
            }
        }

        private void StartService()
        {
            host = new ServiceHost(this);
            host.Open();
            channelFactory = new ChannelFactory<IChatBackend>("ChatEndpoint");
            _channel = channelFactory.CreateChannel();

            // Information to send to the channel
            _channel = channelFactory.CreateChannel();

            // Information to display locall
            _displayMessageDelegate(new CompositeType("Info", "To change your name, type \"setname: NEW_NAME\""));
        }

        private void StopService()
        {
            if (host != null)
            {
                _channel.DisplayMessage(new CompositeType("Event", _myUserName + " is leaving the conversation."));
                if(host.State != CommunicationState.Closed)
                {
                    channelFactory.Close();
                    host.Close();
                }
            }
        }

        #endregion // Sending messages

    }
}
