using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Chatography.NetworkModule;

using OTR.Interface;

namespace Chatography.UserModule
{
    public class UserManager
    {
        Client client;

        public UserManager()
        {
            client = new Client();
        }
    }
}
