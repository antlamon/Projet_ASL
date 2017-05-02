using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    public class Archer : Personnage
    {
        const int PORTÉE_ATTAQUE = 20;
        public const int RAYON_PLUIE_DE_FLÈCHES = 10;
        public const int PORTÉE_PLUIE_DE_FLÈCHES = 40;
        const float DÉGATS_PLUIE_DE_FLÈCHES = 0.5f;
        public const int PORTÉE_FLÈCHE_PERCANTE = 50;
        const float DÉGATS_FLÈCHE_PERCANTE = 0.7f;

        public Archer(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
        }

        public override int Attaquer()
        {
            return Dextérité;
        }

        public override int GetPortéeAttaque()
        {
            return PORTÉE_ATTAQUE;
        }

        public List<Personnage> PluieDeFlèches(Vector3 positionClic, List<Personnage> CiblesPotentielles, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere zone = new BoundingSphere(positionClic, RAYON_PLUIE_DE_FLÈCHES);
            dégats = (int)(DÉGATS_PLUIE_DE_FLÈCHES * Attaquer());

            if (this is Archer)
            {
                foreach (Personnage p in CiblesPotentielles)
                {
                    if (zone.Intersects(p.SphèreDeCollision))
                    { cibles.Add(p); }
                }
            }

            return cibles;
        }

        public List<Personnage> FlèchePercante(Vector3 positionClic, List<Personnage> CiblesPotentielles, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            Ray portée = new Ray(Position, Position - positionClic); // positionClic - Position?
            dégats = (int)(DÉGATS_FLÈCHE_PERCANTE * Attaquer());

            if (this is Archer)
            {
                foreach (Personnage p in CiblesPotentielles)
                {
                    if (portée.Intersects(p.SphèreDeCollision) <= PORTÉE_FLÈCHE_PERCANTE) 
                    { cibles.Add(p); }
                }

                cibles.OrderBy(cible => cible.Position - Position); // À vérifier pour les distances
                cibles.RemoveRange(2, cibles.Count - 2);
            }

            return cibles;
        }
    }
}
