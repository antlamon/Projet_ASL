using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    class Archer : Personnage
    {
        const int RAYON_PLUIE_DE_FLÈCHES = 10;
        const float DÉGAT_PLUIE_DE_FLÈCHES = 0.5f;
        public Archer(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, string nom, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, nom, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
        }

        public override int Attaquer()
        {
            return Dextérité;
        }

        public void PluieDeFlèches(Vector2 positionClic)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere portée = new BoundingSphere(new Vector3(positionClic.X, 0, positionClic.Y), RAYON_PLUIE_DE_FLÈCHES);
            
            foreach(Personnage p in Personnages)
            {
                if(portée.Intersects(p.SphèreDeCollision))
                { cibles.Add(p); }
            }
            foreach(Personnage cible in cibles)
            {
                cible.ModifierVitalité((int)(DÉGAT_PLUIE_DE_FLÈCHES * Attaquer()));
            }
        }
    }
}
