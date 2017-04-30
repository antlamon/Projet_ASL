using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Projet_ASL
{
    static class CompteurManches
    {
        public static int NumeroManche { get; private set; } 

        static CompteurManches()
        {
            NumeroManche = 0;
        }

        public static void ProchaineManche()
        {
            ++NumeroManche;
        }
        
        public static void RéinitialiserCompteur()
        {
            NumeroManche = 0;
        }
    }
}
