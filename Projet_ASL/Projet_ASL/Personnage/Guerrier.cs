using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    class Guerrier : Personnage
    {
        const float DÉGATS_TORNADE_FURIEUSE = 0.75f;

        public Guerrier(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, string nom, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, nom, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
        }

        public override int Attaquer()
        {
            return Force;
        }

        public List<Personnage> TornadeFurieuse(Vector2 positionClic)
    }
}
