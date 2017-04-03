using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Projet_ASL
{
    public class Armure : Item
    {
        int Défense { get; set; }

        public Armure(int numeroID, string catégoriePersonnage, string nom, int niveauRequis, int rareté, string refImage, string statistiques)
            :base(numeroID, catégoriePersonnage, nom, niveauRequis, rareté, refImage)
        {
            string[] tableauStats = statistiques.Split('|');
            Défense = int.Parse(tableauStats[0]);
        }

        public int GetDéfense()
        {
            return Défense;
        }
    }
}
