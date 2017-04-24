using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    public class Guérisseur : Personnage
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

        public Guérisseur(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, force, dextérité, intelligence, sagesse, ptsDeVie)
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
        public List<Personnage> SoinDeZone(Vector2 positionClic, List<Personnage> CiblesPotentielles, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere zone = new BoundingSphere(new Vector3(positionClic.X, 0, positionClic.Y), RAYON_SOIN_DE_ZONE);
            dégats = (int)(RATIO_SOIN_DE_ZONE * Attaquer());

            foreach (Personnage p in CiblesPotentielles)
            {
                if (zone.Intersects(p.SphèreDeCollision) && !p.EstMort)
                { cibles.Add(p); }
            }


            return cibles;
        }

        public int Résurrection(Personnage cible)
        {
            int dégats = 0;

            if (cible.EstMort)
            {
                dégats = (int)(RATIO_RESURRECT * Attaquer());
            }

            return dégats;
        }

        public int VolDeVie(out int vieVolée)
        {
            vieVolée = -(int)(RATIO_VOL_DE_VIE * Attaquer());
            return Attaquer();
        }

        public override void EnleverDebuffs()
        {
            base.EnleverDebuffs();
            _SatanMode = false;
        }
    }
}
