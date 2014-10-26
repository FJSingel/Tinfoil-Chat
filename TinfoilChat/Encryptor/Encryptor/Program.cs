using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncryptorModule
{


    class Program
    {
  
        static void Main(string[] args)
        {

          
            Encryptor _otr_tester = new Encryptor();

            _otr_tester.RunOTRDemo();

            Console.ReadLine();
        }
 
    
    }
}
