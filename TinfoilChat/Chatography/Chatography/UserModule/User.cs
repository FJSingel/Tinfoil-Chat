using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OTR.Interface;

using fastJSON;

namespace Chatography.UserModule
{
    public class User
    {
        private DSAKeyParams keyParams;
        private LinkedList<String> lastKnownIPAddresses;
        private string fingerprint;

        public User(DSAKeyParams keyp)
        {
            keyParams = keyp;
            lastKnownIPAddresses = new LinkedList<String>();
        }

        public User(string user)
        {

        }

        public DSAKeyParams getKeyParams()
        {
            return keyParams;
        }

        public LinkedList<String> getLastKnownIPAddresses()
        {
            return new LinkedList<String>(lastKnownIPAddresses);
        }

        public void addIPAddress(String IPAddress)
        {
            lastKnownIPAddresses.AddFirst(IPAddress);
        }

        public string getFingerprint()
        {
            return fingerprint;
        }

        public void setFingerprint(string fp)
        {
            //Fingerprint can be gotten using the sessionmanager's getMyBuddyFingerprint method
            fingerprint = fp;
        }

        public override string ToString()
        {
            return "";
        }
    }
}
