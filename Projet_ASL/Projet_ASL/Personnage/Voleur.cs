using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    public class Voleur : Personnage
    {
        const int PORTÉE_ATTAQUE = 5;
        public const int PORTÉE_INVISIBILITÉ = 25;
        public const int PORTÉE_LANCER_COUTEAU = 15;
        const float DÉGÂTS_LANCER_COUTEAU = 0.7f;

        public Voleur(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
        }

        public override int GetPortéeAttaque()
        {
            return PORTÉE_ATTAQUE;
        }

        public override int Attaquer()
        {
            return Dextérité + Force;
        }

        public int LancerCouteau()
        {
            return (int)(DÉGÂTS_LANCER_COUTEAU * Attaquer());
        }
    }
}
