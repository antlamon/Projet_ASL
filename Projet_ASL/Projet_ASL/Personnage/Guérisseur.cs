using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    class Guérisseur : Personnage
    {
        const int RAYON_SOIN_DE_ZONE = 10;
        const float RATIO_SOIN_DE_ZONE = 0.4f;
        const float DÉGATS_SATAN_MODE = 0.7f;
        const float RATIO_RESURRECT = 0.5f;
        const float RATIO_VOL_DE_VIE = 0.4f;

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

        public List<Personnage> SoinDeZone(Vector2 positionClic, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere portée = new BoundingSphere(new Vector3(positionClic.X, 0, positionClic.Y), RAYON_SOIN_DE_ZONE);

            //foreach (Personnage p in Personnages)
            //{
            //    if (portée.Intersects(p.SphèreDeCollision) && !p.EstMort)
            //    { cibles.Add(p); }
            //}

            dégats = (int)(RATIO_SOIN_DE_ZONE * Attaquer());

            return cibles;
        }

        public int Résurrection(Personnage cible)
        {
            int dégats;
            if (cible.EstMort)
            {
                dégats = (int)(RATIO_RESURRECT * Attaquer());
            }
            else
            {
                dégats = 0;
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
            _EnFeu = false;
            _Frozen = false;
            _BouclierDivin = false;
            _SatanMode = false;
        }
    }
}
