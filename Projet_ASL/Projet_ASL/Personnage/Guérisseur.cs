using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    public class Guérisseur : Personnage
    {
        const int PORTÉE_ATTAQUE = 20;
        public const int RAYON_SOIN_DE_ZONE = 10;
        public const int PORTÉE_SOIN_DE_ZONE = 40;
        const float RATIO_SOIN_DE_ZONE = 0.4f;
        public const int PORTÉE_RESURRECT = 20;
        const float RATIO_RESURRECT = 1f;
        const float RATIO_VOL_DE_VIE = 0.4f;
        const float DÉGATS_SATAN_MODE = 0.7f;

        public bool _SatanMode { get; set; }

        public Guérisseur(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
            _SatanMode = false;
        }

        public override int Attaquer()
        {
            int attaque;
            if (_SatanMode)
            {
                attaque = (int)(DÉGATS_SATAN_MODE * Sagesse);
            }
            else
            {
                attaque = -Sagesse;
            }
            return attaque;
        }

        public override int GetPortéeAttaque()
        {
            return PORTÉE_ATTAQUE;
        }

        public List<Personnage> SoinDeZone(Vector3 positionClic, List<Personnage> CiblesPotentielles, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere zone = new BoundingSphere(positionClic, RAYON_SOIN_DE_ZONE);
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
            dégats = (int)(RATIO_RESURRECT * Attaquer());
            _SatanMode = true;
            return dégats;
        }

        public void SetSatan(bool estSatan)
        {
            _SatanMode = estSatan;
        }

        public int VolDeVie(out int vieVolée)
        {
            vieVolée = -(int)(RATIO_VOL_DE_VIE * Attaquer());
            return Attaquer();
        }
    }
}
