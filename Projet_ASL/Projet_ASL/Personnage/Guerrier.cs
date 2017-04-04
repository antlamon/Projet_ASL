using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    class Guerrier : Personnage
    {
        const int RAYON_TORNADE_FURIEUSE = 10;
        const float DÉGATS_TORNADE_FURIEUSE = 0.75f;
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

        public List<Personnage> TornadeFurieuse(Vector2 positionClic, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere portée = new BoundingSphere(new Vector3(positionClic.X, 0, positionClic.Y), RAYON_TORNADE_FURIEUSE);

            //foreach(Personnage p in Personnages)
            //{
            //    if(portée.Intersects(p.SphèreDeCollision))
            //    { cibles.Add(p); }
            //}

            dégats = (int)(DÉGATS_TORNADE_FURIEUSE * Attaquer());

            return cibles;
        }

        public int Folie(out int nbToursFolie)
        {
            _Folie = true;
            nbToursFolie = NB_TOURS_FOLIE;
            return PTS_VITALITÉ_FOLIE;
        }

        public override void EnleverDebuffs()
        {
            _EnFeu = false;
            _Frozen = false;
            _BouclierDivin = false;
            _Folie = false;
        }
    }
}
