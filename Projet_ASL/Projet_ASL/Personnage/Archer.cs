using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    class Archer : Personnage
    {
        public Archer(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, string nom, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, nom, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
        }

        public override int Attaquer()
        {
            return Dextérité;
        }

        public void PluieDeFlèches(Tuile cible)
        {
            List<Personnage> cibles = new List<Personnage>();
            List<Tuile> tuilesCibles = new List<Tuile>();
            //Vector2 ciblePosition = cible.Position;

            foreach(Tuile t in Tuiles)
            {
                if (t.Position == new Vector2(cible.Position.X + 1, cible.Position.Y + 1))  { tuilesCibles.Add(t); }
                if (t.Position == new Vector2(cible.Position.X + 1, cible.Position.Y))      { tuilesCibles.Add(t); }
                if (t.Position == new Vector2(cible.Position.X, cible.Position.Y + 1))      { tuilesCibles.Add(t); }
                if (t.Position == new Vector2(cible.Position.X - 1, cible.Position.Y - 1))  { tuilesCibles.Add(t); }
                if (t.Position == new Vector2(cible.Position.X - 1, cible.Position.Y))      { tuilesCibles.Add(t); }
                if (t.Position == new Vector2(cible.Position.X, cible.Position.Y - 1))      { tuilesCibles.Add(t); }
                if (t.Position == new Vector2(cible.Position.X + 1, cible.Position.Y - 1))  { tuilesCibles.Add(t); }
                if (t.Position == new Vector2(cible.Position.X - 1, cible.Position.Y + 1))  { tuilesCibles.Add(t); }
            }
            
            foreach()
            {
                if (t.)
            }
        }
    }
}
