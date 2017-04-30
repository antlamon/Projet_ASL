using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    public class Paladin : Personnage
    {
        public const int PORTÉE_CLARITÉ = 15;
        public const int PORTÉE_BOUCLIER_DIVIN = 15;

        public Paladin(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
        }
        public override int Attaquer()
        {
            return Force + Sagesse;
        }

        public void Clarité(Personnage cible)
        {
            cible.EnleverDebuffs(this);
        }

        public void BouclierDivin(Personnage cible)
        {
            cible.SetBouclierDivin(this);
        }
    }
}
