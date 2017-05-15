using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    public abstract class Personnage : ObjetDeDémo
    {
        public const int PTS_VIE_MAX = 75;
        const int NB_TOURS_EN_FEU = 3;
        int ptsDeVie;
        public int PtsDeVie
        {
            get { return ptsDeVie; }
            protected set
            {
                ptsDeVie = value > 0 ? value < PTS_VIE_MAX ? value : PTS_VIE_MAX : 0;
            }
        }
        public int Dextérité { get; protected set; }
        public int Force { get; protected set; }
        public int Intelligence { get; protected set; }
        public int Sagesse { get; protected set; }
        public int PtsDéfense
        {
            get { return 0; }
        }
        public bool EstMort
        {
            get { return PtsDeVie == 0; }
        }
        //public Expérience PtsExp { get; private set; }
        public int Niveau { get; private set; }
        public Arme Arme { get; private set; }
        public Armure Armure { get; private set; }
        bool frozen;
        bool enFeu;
        bool bouclierDivin;
        public bool _Frozen
        {
            get { return frozen; }
            private set { frozen = value; }
        }
        public bool _EnFeu
        {
            get { return enFeu; }
            private set { enFeu = value; }
        }
        public int CptEnFeu { get; set; }
        public bool _BouclierDivin
        {
            get { return bouclierDivin; }
            private set { bouclierDivin = value; }
        }

        protected Personnage(Game jeu, String nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, 1f / 60f)
        {
            Dextérité = dextérité;
            Force = force;
            Intelligence = intelligence;
            Sagesse = sagesse;
            PtsDeVie = ptsDeVie;
            Niveau = 1;
            CptEnFeu = 0;
        }

        public Arme ModifierArme(Arme nouvelleArme)
        {
            Arme ancienArme = Arme;
            Arme = nouvelleArme;
            return ancienArme;
        }

        public Armure ModifierArmure(Armure nouvelleArmure)
        {
            Armure ancienArmure = Armure;
            Armure = nouvelleArmure;
            return ancienArmure;
        }

        public abstract int Attaquer();
        public abstract int GetPortéeAttaque();
        public virtual void MonterDeNiveau()
        {
            ++Niveau;
        }

        public virtual void ModifierVitalité(int dégats)
        {
            int modificationVitalité;
            if (dégats > 0)
            {
                if (!_BouclierDivin)
                {
                    modificationVitalité = (int)Math.Round((double)dégats * (1 - PtsDéfense / 100));
                }
                else
                {
                    _BouclierDivin = false;
                    modificationVitalité = 0;
                }
            }
            else
            {
                modificationVitalité = dégats;
            }
            PtsDeVie -= modificationVitalité;
        }

        #region Mage

        public void SetEnFeu(bool persoEnFeu)
        {
            _EnFeu = persoEnFeu;
            if (_EnFeu) { CptEnFeu = NB_TOURS_EN_FEU; }
        }

        public void SetFreeze(bool persoFrozen)
        {
            _Frozen = persoFrozen;
        }
        #endregion
        #region Paladin
        public void SetBouclierDivin(bool bouclierActif)
        {
            _BouclierDivin = bouclierActif;
        }
        #endregion

        public void ChangerVitalité(int nouvelleVitalité)
        {
            PtsDeVie = nouvelleVitalité;
        }

    }
}
