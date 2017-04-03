using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Projet_ASL
{
    public class Arme : Item
    {
        int Dextérité { get; set; }
        int Force { get; set; }
        int Intelligence { get; set; }
        int Sagesse { get; set; }
        int[] Statistiques { get; set; }

        public Arme(int numeroID, string catégoriePersonnage, string nom, int niveauRequis, int rareté, string refImage, string statistiques)
            :base(numeroID, catégoriePersonnage, nom, niveauRequis, rareté, refImage) 
        {
            string[] tableauStats = statistiques.Split('|');
            InitialiserStats(tableauStats);
        }

        void InitialiserStats(string[] tableauStats)
        {
            Dextérité = int.Parse(tableauStats[0]);
            Force = int.Parse(tableauStats[1]);
            Intelligence = int.Parse(tableauStats[2]);
            Sagesse = int.Parse(tableauStats[3]);
            Statistiques = new int[] { Dextérité, Force, Intelligence, Sagesse };
        }

        public int[] GetStats()
        {
            return Statistiques;
        }
    }
}
