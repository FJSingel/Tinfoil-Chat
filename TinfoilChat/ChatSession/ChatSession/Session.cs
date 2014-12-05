using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using OTR.Interface;

namespace ChatSession
{
    class Session
    {
        public static Session currentSession;

        private static NetworkModule nModule = NetworkModule.networkModule;

        private HashSet<TcpClient> verifiedUsers;
        private HashSet<TcpClient> unverifiedUsers;

        private Dictionary<int, Chat> verificationProcesses;
        private Dictionary<int, Chat> chats;
        public static OTRSessionManager AliceOTRSession;
        public static string AlicesFriendID = "Bob";
        private string AlicesID;

        public Session()
        {
            if (currentSession == null)
            {
                currentSession = this;
            }
            else
            {
                throw new Exception("Attempting to create a second session.");
            }

            NetworkModule.networkModule = new NetworkModule();

            verifiedUsers = new HashSet<TcpClient>();
            unverifiedUsers = new HashSet<TcpClient>();
            chats = new Dictionary<int, Chat>();

            AlicesID = "Alice";
            AlicesFriendID = "Bob";
            AliceOTRSession = new OTRSessionManager(AlicesID);
        }

        /// <summary>
        /// This is the function the UI calls to add a new user
        /// </summary>
        /// <param name="ip">The IP address the user is located at</param>
        public void addUser(String ip)
        {
            TcpClient man = nModule.findUser(ip);
            unverifiedUsers.Add(man);

            Chat verificationProcess = new Chat();
            verificationProcesses.Add(0, verificationProcess);

            verificationProcess.message(new byte[]{});                                                             // Frank some validation process begins here.

        }

        /// <summary>
        /// This is the signaled function that the NetworkModule calls when it receives a new connection (a new user initiates a connection to you).
        /// </summary>
        /// <param name="newUser"></param>
        public void signalNewUser(TcpClient newUser)
        {
            unverifiedUsers.Add(newUser);

            //We need the foreign user's name
            OpenOTRSession(AlicesFriendID);
        }

                                                                                                                    // FRAAAAAAANK FIIIITTZZZZZ Messages are processed here (all types in switch-case)

        /// <summary>
        /// This is the signaled function that the NetworkModule calls when it receives a message.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        public void signalNewMessage(msgType type, byte[] msg)
        {
            ChatMessage message = new ChatMessage(msg);
            Chat chat = null;
            if (chats.TryGetValue(message.getChatID(), out chat))
            {

            }
            switch (type)
            {
                case msgType.Verification:
                    AliceOTRSession.ProcessOTRMessage(AlicesFriendID, msg.ToString());
                    break;
                case msgType.Internal:
                    AliceOTRSession.ProcessOTRMessage(AlicesFriendID, msg.ToString());
                    break;
                case msgType.Chat:
                    AliceOTRSession.ProcessOTRMessage(AlicesFriendID, msg.ToString());                                                                                                // FITZ Here's to you kid. Should signal some function in UI to output message.
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// This class is a container for a chatMessage and the chatID it belongs to.
        /// It can return a byte array containing both information before sending to the stream.
        /// It can also parse a byte array from that same stream to return both values.
        /// </summary>
        public class ChatMessage
        {
            private int chatID;
            private byte[] chatMsg;

            public ChatMessage(int id, byte[] msg)
            {
                chatID = id;
                chatMsg = msg;
            }

            // reads a raw message byte array containing the message and the chatID appended to the end
            public ChatMessage(byte[] msg)
            {
                chatID = BitConverter.ToInt32(msg, msg.Length - 4);
                chatMsg = new byte[msg.Length - 4];
                Array.Copy(msg, chatMsg, msg.Length - 4);
            }

            // returns the chatID the message belongs to
            public int getChatID()
            {
                return chatID;
            }

            // returns actual message
            public byte[] getChatMessage()
            {
                return new byte[]{};
            }

            // returns bytes with chatID appended to the end
            public byte[] toBytes()
            {
                byte[] msg = new byte[chatMsg.Length + 4];

                Array.Copy(chatMsg, msg, chatMsg.Length);

                byte[] id = BitConverter.GetBytes(chatID);
                id.CopyTo(msg, chatMsg.Length - 1);

                return msg;
            }
        }
        
        public class Chat
        {
            private int chatID;
            private HashSet<TcpClient> chatMembers;

            public Chat()                                                                                                 // Modify Constructor to generate "random" chatID
            {
                chatID = 0;
                chatMembers = new HashSet<TcpClient>();
            }

            public void addMember(TcpClient newUser)
            {
                foreach(TcpClient member in chatMembers){

                }
                chatMembers.Add(newUser);
            }

            /// <summary>
            /// For chat messages only
            /// </summary>
            /// <param name="message"></param>
            public void message(String message)
            {
                foreach (TcpClient member in chatMembers)
                {
                    AliceOTRSession.EncryptMessage(AlicesFriendID, message);
                    
                    byte[] msg = encrypt(message);                                                                 // Frank - Encrypt Message here
                    ChatMessage messg = new ChatMessage(chatID, msg);
                    nModule.message(member, msgType.Chat, messg.toBytes());
                }
            }

            /// <summary>
            /// For verification only
            /// </summary>
            /// <param name="message"></param>
            public void message(byte[] message)
            {
                foreach (TcpClient member in chatMembers)
                {
                    ChatMessage messg = new ChatMessage(chatID, message);
                    nModule.message(member, msgType.Chat, messg.toBytes());
                }
            }
        }


        // Dummy Encryption
        public static byte[] encrypt(String message)
        {
            return Encoding.ASCII.GetBytes(message);
        }

            #region otradditions

        public void OpenOTRSession(string buddy_id)
        {
            /* create otr session and request otr session */
    
            AliceOTRSession.OnOTREvent += new OTREventHandler(OnAliceOTRMangerEventHandler);

            AliceOTRSession.CreateOTRSession(buddy_id);

            AliceOTRSession.RequestOTRSession(buddy_id, OTRSessionManager.GetSupportedOTRVersionList()[0]);

        }

        //todo: this manager needs to be updated to use the network traffic
        private void OnAliceOTRMangerEventHandler(object source, OTREventArgs e)
        {

            switch (e.GetOTREvent())
            {
                case OTR_EVENT.MESSAGE:
                    //this event happens when a message is decrypted successfully. Hand the message to the GUI
                    Console.WriteLine("{0}: {1} \n", e.GetSessionID(), e.GetMessage());
                    break;
                case OTR_EVENT.SEND:
                    //this is where you would send the data on the network. next line is just a dummy line. e.getmessage() will contain message to be sent
                    //senddataonnetwork(aliceid, e.getmessage());
                    
                    break;
                case OTR_EVENT.ERROR:
                    //some sort of error occurred. we should use these errors to decide if it is fatal (failure to verify key) or benign (message did not decrypt)
                    Console.WriteLine("alice: otr error: {0} \n", e.GetErrorMessage());
                    Console.WriteLine("alice: otr error verbose: {0} \n", e.GetErrorVerbose());
                    break;
                case OTR_EVENT.READY:
                    //fires when each user is ready for communication. can't communicate prior to this.
                    Console.WriteLine("alice: encrypted otr session with {0} established \n", e.GetSessionID());
                    AliceOTRSession.EncryptMessage(AlicesFriendID, "hi first message");
                    break;
                case OTR_EVENT.DEBUG:
                    //just for debug lines. flagged using a true flag in the session manager construction
                    Console.WriteLine("alice: " + e.GetMessage() + "\n");
                    break;
                case OTR_EVENT.EXTRA_KEY_REQUEST:
                    //allow for symmetric aes key usage. only for otr v3+.
                    //i doubt we need this.
                    break;
                case OTR_EVENT.SMP_MESSAGE:
                    //fires after smp process finishes
                    Console.WriteLine("alice: " + e.GetMessage() + "\n");
                    break;
                case OTR_EVENT.CLOSED:
                    //fires when otr session closes
                    Console.WriteLine("Alice: Encrypted OTR session with {0} closed \n", e.GetSessionID());
                    break;
            }
        }
        #endregion
    }
}
