using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    public class Guerrier : Personnage
    {
        const int PORTÉE_ATTAQUE = 5;
        public const int PORTÉE_TORNADE_FURIEUSE = 10;
        const float DÉGATS_TORNADE_FURIEUSE = 0.75f;
        const int PTS_VITALITÉ_FOLIE = 1;
        const int NB_TOURS_FOLIE = 2;
        public int CptFolie { get; set; }
        public bool _Folie { get; private set; }

        public Guerrier(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
            CptFolie = 0;
        }

        public override int Attaquer()
        {
            return Force;
        }


        public override int GetPortéeAttaque()
        {
            return PORTÉE_ATTAQUE;
        }

        public List<Personnage> TornadeFurieuse(Vector3 position, List<Personnage> CiblesPotentielles, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere portée = new BoundingSphere(position, PORTÉE_TORNADE_FURIEUSE);
            dégats = (int)(DÉGATS_TORNADE_FURIEUSE * Attaquer());

            if (this is Guerrier)
            {
                foreach (Personnage p in CiblesPotentielles)
                {
                    if (portée.Intersects(p.SphèreDeCollision))
                    { cibles.Add(p); }
                }
            }

            return cibles;
        }

        public override void ModifierVitalité(int dégats)
        {
            base.ModifierVitalité(dégats);
            if (_Folie && PtsDeVie == 0)
            {
                PtsDeVie = PTS_VITALITÉ_FOLIE;
            }
        }

        public void SetFolie(bool estFou)
        {
            _Folie = estFou;
            if (_Folie)
            {
                CptFolie = NB_TOURS_FOLIE;
                ChangerVitalité(PTS_VITALITÉ_FOLIE);
            }
        }

        public override void EnleverDebuffs(Personnage caster)
        {
            base.EnleverDebuffs(caster);
            if (caster is Paladin)
            {
                _Folie = false;
            }
        }
    }
}
