using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    class Guérisseur : Personnage
    {
        const float DÉGATS_SATAN_MODE = 0.7f;

        bool satanMode;

        public bool _SatanMode
        {
            get { return satanMode; }
            private set
            {
                satanMode = value;
            }
        }

        public Guérisseur(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, string nom, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, nom, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
            _SatanMode = false;
        }

        public override int Attaquer()
        {
            int attaque = -Sagesse;
            if(_SatanMode)
            {
                attaque = (int)(DÉGATS_SATAN_MODE * Sagesse);
            }
            return attaque;
        }

        public override void EnleverDebuffs()
        {
            base.EnleverDebuffs();
            _SatanMode = false;
        }
    }
}
