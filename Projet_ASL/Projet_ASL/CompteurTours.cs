using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Projet_ASL
{
    static class CompteurTours
    {
        public static int NumeroTour { get; private set; } 

        static CompteurTours()
        {
            NumeroTour = 0;
        }

        public static void ProchainTour()
        {
            ++NumeroTour;
        }
        
        public static void RedémarrerCompteur()
        {
            NumeroTour = 0;
        }
    }
}
