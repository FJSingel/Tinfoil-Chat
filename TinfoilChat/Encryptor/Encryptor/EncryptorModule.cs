using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OTR.Interface;

namespace EncryptorModule
{
    public class Encryptor
    {
        DSAKeyParams AliceKeyObject = null;
        DSAKeyParams BobKeyObject = null;

        //Should probably be set to public key fingerprints. Should be UNIQUE
        String AliceID = "alice";
        String BobID = "bob";

        //IDs of who they attempt to talk with
        String AlicesFriendID = String.Empty;
        String BobsFriendID = String.Empty;

        //The OTR Sessions
        OTRSessionManager AliceSessionManager = null;
        OTRSessionManager BobSessionManager = null;

        const int NUMOFMSGS = 3;
        String[] aliceMSGArray = { "Hi.", "How are you?", "Bye!" };
        String[] bobMSGArray = { "Hello.", "I am fine.", "Good bye!" };

        //Keeps track of which messages have already been sent
        int aliceConvPos = 0;
        int bobConvPos = 0;

        //String aliceMSG = string.Empty;
        //String bobMSG = string.Empty;

        internal void RunOTRDemo()
        {
            //Negotiate Keys
            SetPublicKeys();

            //Set friend's IDs
            AlicesFriendID = BobID;
            BobsFriendID = AliceID;

            //Let user designate some text to send
            //for (int x = 0; x < NUMOFMSGS; x++)
            //{
            //    Console.WriteLine("Alice's input:");
            //    aliceMSGArray[x] = Console.ReadLine();
            //    Console.WriteLine("Bob's input:");
            //    bobMSGArray[x] = Console.ReadLine();
            //}

            //Open Sessions for each user. IDs should be UNIQUE (Hashes?)
            AliceSessionManager = new OTRSessionManager(AliceID);
            BobSessionManager = new OTRSessionManager(BobID);

            /*
             //If Alice and Bob want to use their DSA keys
             AliceSessionManager.CreateOTRSession(AlicesFriendID, AliceKeyObject);
             BobSessionManager.CreateOTRSession(BobsFriendID, BobKeyObject);

             //If you want to run in debug mode
             AliceSessionManager.CreateOTRSession(AlicesFriendID, true);
             BobSessionManager.CreateOTRSession(BobsFriendID, true);
             
             //If you want to run in debug mode
             AliceSessionManager.CreateOTRSession(AlicesFriendID, 100);
             BobSessionManager.CreateOTRSession(BobsFriendID, 100);
             */


            AliceSessionManager.OnOTREvent += new OTREventHandler(OnAliceOTRManagerEventHandler);
            BobSessionManager.OnOTREvent += new OTREventHandler(OnBobOTRManagerEventHandler);


            //If they don't have DSA Keys, make some random ones.
            AliceSessionManager.CreateOTRSession(AlicesFriendID, true);
            BobSessionManager.CreateOTRSession(BobsFriendID, true);

            /*
             Alice requests an OTR session with Bob  using OTR version 2
             To use version 3 set OTRSessionManager.GetSupportedOTRVersionList()[1]
             or OTRSessionManager.GetSupportedOTRVersionList()[2]
             */

            AliceSessionManager.RequestOTRSession(AlicesFriendID, OTRSessionManager.GetSupportedOTRVersionList()[0]);
            //AliceSessionManager.StartSMP(AlicesFriendID);
        }

        //Testing method
        internal void SetPublicKeys()
        {
            String AliceP = "00e24d61e1c20661e7514e594cc959859c62eeade72893a4772d3efd246abeb5a2848fb5e4b05a9c5b4edb5b67e53cdeb8337a8e4e44b26a6c1927be024695c83d";
            String AliceQ = "00c873b36de07d9ebea48ee96bcc259b94304c65d9";
            String AliceG = "008028acddbd4e51ba344c7e5bacdaf68ecfce35beead41cf12b3d1093c479c24148d817645a4123604774a8824be2a47e19016e1b4247ea40413cf478fff9009c";

            String BobP = "00e5cc7a4d6cd189b8d176086aec944c2db1e7f8bf13e6b20a3456d8bb9a33d7bb8960de8d1eb4fdf1fbd4ccbb3ecd7ca927169247d2cabff935a70ddbccae6d69";
            String BobQ = "00c9fefa6732d392a57ecebdf6e990887ea52d2835";
            String BobG = "0e9135dc3bd3479de6aae872781ad95703a107915689e655f3ddb2bf99c79af5cec4df5bafe5e502ceb0ca26bac67eefcace2e9f42dc972af4ab0033eeeb583e";

            // 0 < X < Q 
            //(and X should normally be random)
            String AliceX = "333715606eebd0925e79da44e02bdfd0cdba5a";
            String BobX = "00a150c9bec477f9713768f6fc1dfd784702c4ffcd";

            AliceKeyObject = new DSAKeyParams(AliceP, AliceQ, AliceG, AliceX);
            BobKeyObject = new DSAKeyParams(BobP, BobQ, BobG, BobX);
        }

        private void OnAliceOTRManagerEventHandler(object source, OTREventArgs e)
        {

            switch (e.GetOTREvent())
            {
                case OTR_EVENT.MESSAGE:

                    Console.WriteLine("{0}: {1} \n", e.GetSessionID(), e.GetMessage());
                    if (aliceConvPos < aliceMSGArray.Length)
                    {
                        aliceConvPos++;
                        AliceSessionManager.EncryptMessage(AlicesFriendID, aliceMSGArray[aliceConvPos-1]);
                    }
                    break;
                case OTR_EVENT.SEND:    
                    SendDataOnNetwork(AliceID, e.GetMessage());
                    break;
                case OTR_EVENT.ERROR:
                    Console.WriteLine("Alice: OTR Error: {0} \n", e.GetErrorMessage());
                    Console.WriteLine("Alice: OTR Error Verbose: {0} \n", e.GetErrorVerbose());
                    break;
                case OTR_EVENT.READY:
                    Console.WriteLine("Alice: Encrypted OTR session with {0} established \n", e.GetSessionID());
                    aliceConvPos++;
                    AliceSessionManager.EncryptMessage(AlicesFriendID, aliceMSGArray[aliceConvPos-1], true);
                    break;
                case OTR_EVENT.DEBUG:
                    Console.WriteLine("Alice: " + e.GetMessage() + "\n");
                    break;
                case OTR_EVENT.EXTRA_KEY_REQUEST:
                    break;
                case OTR_EVENT.SMP_MESSAGE:
                    Console.WriteLine("Alice: " + e.GetMessage() + "\n");
                    break;
                case OTR_EVENT.CLOSED:
                    Console.WriteLine("Alice: Encrypted OTR session with {0} closed \n", e.GetSessionID());
                    break;
            }
        }

        private void OnBobOTRManagerEventHandler(object source, OTREventArgs e)
        {

            switch (e.GetOTREvent())
            {
                case OTR_EVENT.MESSAGE:

                    Console.WriteLine("{0}: {1} \n", e.GetSessionID(), e.GetMessage());
                    if (bobConvPos < bobMSGArray.Length)
                    {
                        bobConvPos++;
                        BobSessionManager.EncryptMessage(BobsFriendID, bobMSGArray[bobConvPos-1]);
                    }
                    break;
                case OTR_EVENT.SEND:
                    SendDataOnNetwork(BobID, e.GetMessage());
                    break;
                case OTR_EVENT.ERROR:
                    Console.WriteLine("Bob: OTR Error: {0} \n", e.GetErrorMessage());
                    Console.WriteLine("Bob: OTR Error Verbose: {0} \n", e.GetErrorVerbose());
                    break;
                case OTR_EVENT.READY:
                    Console.WriteLine("Bob: Encrypted OTR session with {0} established \n", e.GetSessionID());
                    //bobConvPos++;
                    //BobSessionManager.EncryptMessage(BobsFriendID, bobMSGArray[bobConvPos-1]);
                    break;
                case OTR_EVENT.DEBUG:
                    Console.WriteLine("Bob: " + e.GetMessage() + "\n");
                    break;
                case OTR_EVENT.EXTRA_KEY_REQUEST:
                    break;
                case OTR_EVENT.SMP_MESSAGE:
                    Console.WriteLine("Bob: " + e.GetMessage() + "\n");
                    break;
                case OTR_EVENT.CLOSED:
                    Console.WriteLine("Bob: Encrypted OTR session with {0} closed \n", e.GetSessionID());
                    break;
            }
        }

        private void SendDataOnNetwork(string my_unique_id, string otr_data)
        {
            //This needs to be replaced with actual network code. 
            if (my_unique_id == AliceID)
            {
                //Console.WriteLine("Sending to Bob(" + BobSessionManager.GetMyBuddyFingerPrint(BobID) + "): ");
                BobSessionManager.ProcessOTRMessage(BobsFriendID, otr_data);
            }
            else if (my_unique_id == BobID)
            {
                //Console.WriteLine("Sending to Alice(" + AliceSessionManager.GetMyBuddyFingerPrint(AliceID) + "): ");
                AliceSessionManager.ProcessOTRMessage(AlicesFriendID, otr_data);
            }
        }
    }
}