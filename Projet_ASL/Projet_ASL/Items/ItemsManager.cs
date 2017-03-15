using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Projet_ASL
{
    static class ItemsManager
    {
        public static List<Item> Items { get; set; }

        static ItemsManager()
        {
            Items = new List<Item>();
        }

        static void CréerItem(string description)
        {
            char séparateur = ';';

            // Extraction du nom et de la catégorie de l'item du string description
            string[] tableauStatistiques = description.Split(séparateur);
            string catégorieItem = tableauStatistiques[0];
            string catégoriePersonnage = tableauStatistiques[1];
            int numeroID = int.Parse(tableauStatistiques[2]);
            string nom = tableauStatistiques[3];
            int niveauRequis = int.Parse(tableauStatistiques[4]);
            int rareté = int.Parse(tableauStatistiques[5]);
            string refImage = tableauStatistiques[6];
            string statistiques = tableauStatistiques[7];

            // Normaliser le nom de la catégorie pour calquer le nom des classes (Exemple : (J) + (eu) = Jeu)
            catégorieItem = Char.ToUpper(catégorieItem[0]) + catégorieItem.Substring(1).ToLower();

            // Ajouter le nom du Namespace pour qualifier entièrement
            catégorieItem = typeof(Program).Namespace + "." + catégorieItem;

            // Détermination d'un type en fonction de la chaine 'catégorie'
            Type typeVoulu = Type.GetType(catégorieItem);

            // Tentative d'instanciation : le type de la valeur de retour est 'Object'
            var objetCréé = Activator.CreateInstance(typeVoulu, numeroID, catégoriePersonnage, nom, niveauRequis, rareté, refImage, statistiques);

            Items.Add(objetCréé as Item);
        }
    }
}
