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
        public List<Personnage> PluieDeFlèches(Vector2 positionClic, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere portée = new BoundingSphere(new Vector3(positionClic.X, 0, positionClic.Y), RAYON_PLUIE_DE_FLÈCHES);
            dégats = (int)(DÉGATS_PLUIE_DE_FLÈCHES * Attaquer());

            if (this is Archer)
            {
                //foreach(Personnage p in Personnages)
                //{
                //    if(portée.Intersects(p.SphèreDeCollision))
                //    { cibles.Add(p); }
                //}
            }

            return cibles;
        }

        public List<Personnage> FlèchePercante(Vector2 positionClic, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            Ray portée = new Ray(Position, new Vector3(positionClic.X - Position.X, 0, positionClic.Y - Position.Z));
            // positionClic est un vecteur2 et Position un vecteur3, ce qui explique Y - Z
            dégats = (int)(DÉGATS_FLÈCHE_PERCANTE * Attaquer());

            if (this is Archer)
            {
                //foreach(Personnage p in Personnages)
                //{
                //    if(portée.Intersects(p.SphèreDeCollision) != null) // À vérifier
                //    { cibles.Add(p); } 
                //}

                cibles.OrderBy(cible => cible.Position - Position); // À vérifier pour les distances
                cibles.RemoveRange(2, cibles.Count - 2);
            }

            return cibles;
        }
        #endregion
        #region Guérisseur
        public List<Personnage> SoinDeZone(Vector2 positionClic, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere portée = new BoundingSphere(new Vector3(positionClic.X, 0, positionClic.Y), RAYON_SOIN_DE_ZONE);
            dégats = (int)(RATIO_SOIN_DE_ZONE * Attaquer());

            if (this is Guérisseur)
            {
                //foreach (Personnage p in Personnages)
                //{
                //    if (portée.Intersects(p.SphèreDeCollision) && !p.EstMort)
                //    { cibles.Add(p); }
                //}
            }

            return cibles;
        }

        public int Résurrection(Personnage cible)
        {
            int dégats = 0;

            if(this is Guérisseur && cible.EstMort)
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
        #endregion
        #region Guerrier
        public List<Personnage> TornadeFurieuse(Vector2 positionClic, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere portée = new BoundingSphere(new Vector3(positionClic.X, 0, positionClic.Y), RAYON_TORNADE_FURIEUSE);
            dégats = (int)(DÉGATS_TORNADE_FURIEUSE * Attaquer());

            if (this is Guerrier)
            {
                //foreach(Personnage p in Personnages)
                //{
                //    if(portée.Intersects(p.SphèreDeCollision))
                //    { cibles.Add(p); }
                //}
            }

            return cibles;
        }
        #endregion
        #region Mage
        public int FreezeDontMove()
        {
            int dégats = (int)(DÉGATS_FREEZE_DONT_MOVE * Attaquer());
            return dégats;
        }

        public List<Personnage> Brasier(Vector2 positionClic, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere portée = new BoundingSphere(new Vector3(positionClic.X, 0, positionClic.Y), RAYON_BRASIER);
            dégats = (int)(DÉGATS_BRASIER * Attaquer());

            if (this is Mage)
            {
                //foreach(Personnage p in Personnages)
                //{
                //    if(portée.Intersects(p.SphèreDeCollision))
                //    { cibles.Add(p); }
                //}
            }

            return cibles;
        }
        #endregion

        #endregion
        public virtual void EnleverDebuffs()
        {
            _EnFeu = false;
            _Frozen = false;
            _BouclierDivin = false;
        }

        public void Clarité(Personnage cible)
        {
            cible.EnleverDebuffs();
        }

        public void BouclierDivin(Personnage cible)
        {
            cible._BouclierDivin = true;
        }

        public void FreezeDontMove(Personnage cible)
        {
            cible._Frozen = true;
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
