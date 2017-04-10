using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    class Guerrier : Personnage
    {
        const int PTS_VITALITÉ_FOLIE = 1;
        const int NB_TOURS_FOLIE = 2;
        bool folie; 

        public bool _Folie
        {
            get { return folie; }
            private set { folie = value; }
        }

        public Guerrier(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, string nom, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, nom, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
        }

        public override int Attaquer()
        {
            return Force;
        }

        public int Folie(out int nbToursFolie)
        {
            _Folie = true;
            nbToursFolie = NB_TOURS_FOLIE;
            return PTS_VITALITÉ_FOLIE;
        }

        public override void EnleverDebuffs()
        {
            base.EnleverDebuffs();
            _Folie = false;
        }
    }
}
