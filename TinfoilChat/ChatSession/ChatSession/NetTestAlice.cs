using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatSession
{
    class NetTestAlice
    {    
        public static void main(string[] args)
        {
            NetworkModule network = new NetworkModule();
            Session test = new Session();
            test.addUser("127.0.0.1");

            
        }
    }

    class NetTestBob
    {
        public static void main(string[] args)
        {
            NetworkModule network = new NetworkModule();
            Session test = new Session();

        }
    }
}
