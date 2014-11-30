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

        public void initialize()
        {
            List<int> users = new List<int>();
            foreach (int user in users)
            {
                List<string> ipaddresses = new List<string>();
                foreach (string ip in ipaddresses)
                {
                    if (client.findUser(ip))
                    {
                        break;
                    }
                }
            }
        }

        public void addUser()
        {

        }


    }
}
