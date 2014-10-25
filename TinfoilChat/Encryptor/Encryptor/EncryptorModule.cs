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

        //Should probably be set to public key fingerprints
        String AliceID = "alice";
        String BobID = "bob";

        internal void RunOTRTest()
        {
            //Negotiate Keys
            SetPublicKeys();

            Console.WriteLine("Alice's input:");
            String aliceMSG = Console.ReadLine();
            Console.WriteLine("Bob's input:");
            String bobMSG = Console.ReadLine();

            
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
    }
}