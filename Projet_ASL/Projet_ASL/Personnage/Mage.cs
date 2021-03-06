﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    public class Mage : Personnage
    {
        const int PORTÉE_ATTAQUE = 20;
        public const int RAYON_BRASIER = 10;
        public const int PORTÉE_BRASIER = 40;
        const float DÉGATS_BRASIER = 0.20f;
        public const int DÉGATS_TICK_BRASIER = 5;
        public const int PORTÉE_FREEZE_DONT_MOVE = 40;
        const float DÉGATS_FREEZE_DONT_MOVE = 0.25f;

        public Mage(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
        }

        public override int Attaquer()
        {
            return Intelligence;
        }

        public override int GetPortéeAttaque()
        {
            return PORTÉE_ATTAQUE;
        }

        public List<Personnage> Brasier(Vector3 positionClic, List<Personnage> CiblesPotentielles, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere portée = new BoundingSphere(positionClic, RAYON_BRASIER);
            dégats = (int)(DÉGATS_BRASIER * Attaquer());

            if (this is Mage)
            {
                foreach (Personnage p in CiblesPotentielles)
                {
                    if (portée.Intersects(p.SphèreDeCollision))
                    {
                        cibles.Add(p);
                    }
                }
            }

            return cibles;
        }

        public int FreezeDontMove(Personnage cible)
        {
            return (int)(DÉGATS_FREEZE_DONT_MOVE * Attaquer());
        }
    }
}
