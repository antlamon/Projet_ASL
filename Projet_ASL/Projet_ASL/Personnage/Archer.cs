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
        const float DÉGATS_PLUIE_DE_FLÈCHES = 0.5f;
        const float DÉGATS_FLÈCHE_PERCANTE = 0.7f;
        
        public Archer(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, string nom, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, nom, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
        }

        public override int Attaquer()
        {
            return Dextérité;
        }

        public List<Personnage> PluieDeFlèches(Vector2 positionClic, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere portée = new BoundingSphere(new Vector3(positionClic.X, 0, positionClic.Y), RAYON_PLUIE_DE_FLÈCHES);
            
            //foreach(Personnage p in Personnages)
            //{
            //    if(portée.Intersects(p.SphèreDeCollision))
            //    { cibles.Add(p); }
            //}

            dégats = (int)(DÉGATS_PLUIE_DE_FLÈCHES * Attaquer());

            return cibles;
        }

        public List<Personnage> FlèchePercante(Vector2 positionClic, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            Ray portée = new Ray(Position, new Vector3(positionClic.X - Position.X, 0, positionClic.Y - Position.Z)); 
            // positionClic est un vecteur2 et Position un vecteur3, ce qui explique Y - Z

            //foreach(Personnage p in Personnages)
            //{
            //    if(portée.Intersects(p.SphèreDeCollision) != null) // À vérifier
            //    { cibles.Add(p); } 
            //}

            cibles.OrderBy(cible => cible.Position - Position); // À vérifier pour les distances
            cibles.RemoveRange(2, cibles.Count - 2);

            dégats = (int)(DÉGATS_FLÈCHE_PERCANTE * Attaquer());

            return cibles;
        }

        public override void EnleverDebuffs()
        {
            _EnFeu = false;
            _Frozen = false;
            _BouclierDivin = false;
        }
    }
}
