using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    public abstract class Personnage : ObjetDeDémo
    {
        #region Constantes Sorts
        #region Constantes Archer
        const int RAYON_PLUIE_DE_FLÈCHES = 10;
        const float DÉGATS_PLUIE_DE_FLÈCHES = 0.5f;
        const float DÉGATS_FLÈCHE_PERCANTE = 0.7f;
        #endregion
        #region Constantes Guérisseur 
        const int RAYON_SOIN_DE_ZONE = 10;
        const float RATIO_SOIN_DE_ZONE = 0.4f;
        const float RATIO_RESURRECT = 0.5f;
        const float RATIO_VOL_DE_VIE = 0.4f;
        #endregion
        #region Constantes Guerrier
        const int RAYON_TORNADE_FURIEUSE = 10;
        const float DÉGATS_TORNADE_FURIEUSE = 0.75f;
        #endregion
        #region Constantes Mage
        const int RAYON_BRASIER = 10;
        const float DÉGATS_FREEZE_DONT_MOVE = 0.25f;
        const float DÉGATS_BRASIER = 0.25f;
        #endregion
        #region Constantes Paladin
        #endregion
        #region Constantes Voleur 
        #endregion
        #endregion
        int ptsDeVie;
        public string Nom { get; protected set; }
        public int PtsDeVie
        {
            get { return ptsDeVie; }
            protected set
            {
                if (value < 0)
                {
                    ptsDeVie = 0;
                }
                else ptsDeVie = value;
            }
        }
        public int Dextérité { get; protected set; }
        public int Force { get; protected set; }
        public int Intelligence { get; protected set; }
        public int Sagesse { get; protected set; }
        public int PtsDéfense
        {
            get { return Armure.GetDéfense(); }
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
            protected set { frozen = value; }
        }
        public bool _EnFeu
        {
            get { return enFeu; }
            protected set { enFeu = value; }
        }
        public bool _BouclierDivin
        {
            get { return bouclierDivin; }
            protected set { bouclierDivin = value; }
        }

        protected Personnage(Game jeu, String nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, string nom, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, 1f/60f)
        {
            Nom = nom;
            Dextérité = dextérité;
            Force = force;
            Intelligence = intelligence;
            Sagesse = sagesse;
            PtsDeVie = ptsDeVie;
            Niveau = 1;
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
        public virtual void MonterDeNiveau()
        {
            ++Niveau;
        }

        public void ModifierVitalité(int dégats)
        {
            int modificationVitalité;
            if(dégats > 0)
            {
                modificationVitalité = (int)Math.Round((double)dégats*(1-PtsDéfense/100));
            }
            else
            {
                modificationVitalité = dégats;
            }
            PtsDeVie -= modificationVitalité;
        }

        #region Sorts 

        #region Archer
        
        #endregion
        #region Guérisseur
        #endregion
        #region Guerrier

        #endregion
        #region Mage
        public void Freeze()
        {
            _Frozen = true;
        }
        #endregion
        #region Paladin
        public void Clarité(Personnage cible)
        {
            EnleverDebuffs();
        }

        public void BouclierDivin(Personnage cible)
        {
            _BouclierDivin = true;
        }
        #endregion

        public virtual void EnleverDebuffs()
        {
            _EnFeu = false;
            _Frozen = false;
            _BouclierDivin = false;
        }

        public void Brasier(List<Personnage> cibles)
        {
            foreach(Personnage cible in cibles)
            {
                cible._EnFeu = true;
            }
        }
        #endregion

        public void ChangerVitalité(int nouvelleVitalité)
        {
            PtsDeVie = nouvelleVitalité;
        }
    }
}
