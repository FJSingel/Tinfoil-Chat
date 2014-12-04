using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatUI
{
    class Session
    {
        public static Session currentSession;

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
        }

        public void verifyNewUser()
        {
            // More stuff here
        }
    }
}
