using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna; 

namespace Projet_ASL
{
    enum NomRareté { COMMUN, RARE, ÉPIQUE, LÉGENDAIRE};
    public class Item
    {
        public int NumeroID { get; private set; }
        public string CatégoriePersonnage { get; private set; }
        public string Nom { get; private set; }
        public int NiveauRequis { get; private set; }
        public int Rareté {get; private set; }
        public string RefImage { get; private set;}

        public Item(int numeroID, string catégoriePersonnage, string nom, int niveauRequis, int rareté, string refImage)
        {
            Nom = nom;
            NiveauRequis = niveauRequis;
            NumeroID = numeroID;
            CatégoriePersonnage = catégoriePersonnage;
            Rareté = rareté;
            RefImage = refImage;
        }
    }
}
