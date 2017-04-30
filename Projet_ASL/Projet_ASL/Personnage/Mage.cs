using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    public class Mage : Personnage
    {
        public const int RAYON_BRASIER = 10;
        public const float DÉGATS_FREEZE_DONT_MOVE = 0.25f;
        public const float DÉGATS_BRASIER = 0.25f;

        public Mage(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
        }

        public override int Attaquer()
        {
            return Intelligence;
        }

        public List<Personnage> Brasier(Vector3 positionClic, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere portée = new BoundingSphere(positionClic, RAYON_BRASIER);
            dégats = (int)(DÉGATS_BRASIER * Attaquer());

            if (this is Mage)
            {
                //foreach(Personnage p in Personnages)
                //{
                //    if(portée.Intersects(p.SphèreDeCollision))
                //    { cibles.Add(p); }
                //}
            }

            return cibles;
        }

        public int FreezeDontMove()
        {
            int dégats = (int)(DÉGATS_FREEZE_DONT_MOVE * Attaquer());
            return dégats;
        }
    }
}
