using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    class Mage : Personnage
    {

        public Mage(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, string nom, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, nom, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
        }
        public Mage() { }

        public override int Attaquer()
        {
            return Intelligence;
        }

        public List<Personnage> Brasier(Vector2 positionClic, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere portée = new BoundingSphere(new Vector3(positionClic.X, 0, positionClic.Y), RAYON_BRASIER);
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
