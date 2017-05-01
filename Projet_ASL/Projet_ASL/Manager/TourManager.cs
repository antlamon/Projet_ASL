using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Projet_ASL
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TourManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        const int DÉPLACEMENT_MAX = 10;
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;

        public DialogueActions BoutonsActions { get; private set; }
        ManagerNetwork NetworkManager { get; set; }
        InputManager GestionnaireInput { get; set; }
        Player JoueurLocal { get; set; }
        Player JoueurEnnemi { get; set; }
        Personnage PersonnageActif { get; set; }
        int ancienIndicePersonnage { get; set; }
        int IndicePersonnage { get; set; }
        float DéplacementRestant { get; set; }
        float ancienDéplacementRestant { get; set; }
        bool PeutAttaquer { get; set; }
        int TempsDepuisDernierUpdate { get; set; }
        AOE ZoneDEffet { get; set; }
        AOE Portée { get; set; }
        AOE ZoneDéplacement { get; set; }
        Jeu Jeu { get; set; }
        List<Personnage> Cibles { get; set; }
        bool TourTerminé { get; set; }

        public TourManager(Jeu jeu, ManagerNetwork networkManager)
            : base(jeu)
        {
            NetworkManager = networkManager;
            BoutonsActions = new DialogueActions(jeu, new Vector2(Game.Window.ClientBounds.Width / 3f, Game.Window.ClientBounds.Height / 7f), NetworkManager);
            Game.Components.Add(BoutonsActions);
            Jeu = jeu;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            JoueurLocal = NetworkManager.JoueurLocal;
            JoueurEnnemi = NetworkManager.JoueurEnnemi;
            Cibles = new List<Personnage>();
            ancienIndicePersonnage = -1;
            IndicePersonnage = 0;
            TempsDepuisDernierUpdate = 0;
            DéplacementRestant = DÉPLACEMENT_MAX;
            CréerBtnClasses();
            PersonnageActif = JoueurLocal.Personnages[IndicePersonnage];
            BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
            BoutonsActions.VoirBoutonAction(NetworkManager.TourActif);
            ZoneDEffet = Jeu.AOE1;
            Portée = Jeu.AOE2;
            ZoneDéplacement = Jeu.AOE3;
            TourTerminé = false;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            GestionnaireInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            base.LoadContent();
        }

        private void CréerBtnClasses()
        {
            foreach (Personnage p in JoueurLocal.Personnages)
            {
                switch (p.GetType().ToString())
                {
                    case TypePersonnage.ARCHER:
                        BoutonsActions.CréerBtnArcher();
                        break;
                    case TypePersonnage.GUÉRISSEUR:
                        BoutonsActions.CréerBtnGuérisseur();
                        break;
                    case TypePersonnage.GUERRIER:
                        BoutonsActions.CréerBtnGuerrier();
                        break;
                    case TypePersonnage.MAGE:
                        BoutonsActions.CréerBtnMage();
                        break;
                    case TypePersonnage.PALADIN:
                        BoutonsActions.CréerBtnPaladin();
                        break;
                    case TypePersonnage.VOLEUR:
                        BoutonsActions.CréerBtnVoleur();
                        break;
                }
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (!JoueurLocal.Personnages[IndicePersonnage].EstMort)
            {
                VérifierDébutDeTour(gameTime);
                if (!TourTerminé)
                {
                    VérifierDéplacement();
                    VérifierSorts(gameTime);
                    VérifierFinDeTour(gameTime);
                }
            }
            else
            {
                IndicePersonnage = IndicePersonnage < JoueurLocal.Personnages.Count - 1 ? IndicePersonnage + 1 : 0;
            }
        }

        void VérifierDébutDeTour(GameTime gameTime)
        {
            if (ancienIndicePersonnage != IndicePersonnage && gameTime.TotalGameTime.Seconds - TempsDepuisDernierUpdate > 1)
            {
                TourTerminé = false;
                PersonnageActif = JoueurLocal.Personnages[IndicePersonnage];
                if (PersonnageActif is Voleur)
                {
                    NetworkManager.SendInvisibilité(false);
                }
                BoutonsActions.VoirBoutonAction(true);
                PeutAttaquer = true;
                ActiverAttaque();
                ZoneDéplacement.ChangerÉtendueEtPosition(new Vector2(DéplacementRestant * 2), PersonnageActif.Position­);
                ancienIndicePersonnage = IndicePersonnage;
            }
        }

        void VérifierDéplacement()
        {
            if (!BoutonsActions.ÉtatSort1 && !BoutonsActions.ÉtatSort2 && !BoutonsActions.ÉtatAttaquer && DéplacementRestant >= 0.5f)
            {
                ZoneDéplacement.Visible = true;
                GestionnaireInput.DéterminerSélectionPersonnageDéplacement(IndicePersonnage);
                DéplacementRestant = GestionnaireInput.DéterminerMouvementPersonnageSélectionné(DéplacementRestant, IndicePersonnage);
                DéplacerZoneMouvement();
            }
            else
            {
                ZoneDéplacement.Visible = false;
            }
        }

        void DéplacerZoneMouvement()
        {
            if (ancienDéplacementRestant != DéplacementRestant)
            {
                ZoneDéplacement.ChangerÉtendueEtPosition(new Vector2(DéplacementRestant * 2), PersonnageActif.Position­);
                ancienDéplacementRestant = DéplacementRestant;
            }
        }

        void VérifierSorts(GameTime gameTime)
        {
            Vector3 positionVérifiée;
            Personnage singleTargetÀAttaquer = null;
            int dégats = 0;

            if (PeutAttaquer)
            {
                if (BoutonsActions.ÉtatSort1)
                {
                    switch (PersonnageActif.GetType().ToString())
                    {
                        case TypePersonnage.ARCHER:
                            GestionnaireInput.Update(gameTime);
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Archer.PORTÉE_PLUIE_DE_FLÈCHES - Archer.RAYON_PLUIE_DE_FLÈCHES);
                            ZoneDEffet.ChangerÉtendueEtPosition(new Vector2(Archer.RAYON_PLUIE_DE_FLÈCHES * 2), positionVérifiée);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Archer.PORTÉE_PLUIE_DE_FLÈCHES * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Portée.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                Cibles = (PersonnageActif as Archer).PluieDeFlèches(positionVérifiée, JoueurEnnemi.Personnages, out dégats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                ActiverAttaque();
                                goto default;
                            }
                            break;
                        case TypePersonnage.GUÉRISSEUR:
                            GestionnaireInput.Update(gameTime);
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Guérisseur.PORTÉE_SOIN_DE_ZONE - Guérisseur.RAYON_SOIN_DE_ZONE);
                            ZoneDEffet.ChangerÉtendueEtPosition(new Vector2(Guérisseur.RAYON_SOIN_DE_ZONE * 2), positionVérifiée);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Guérisseur.PORTÉE_SOIN_DE_ZONE * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Portée.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                Cibles = (PersonnageActif as Guérisseur).SoinDeZone(positionVérifiée, JoueurLocal.Personnages, out dégats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                goto default;
                            }
                            break;
                        case TypePersonnage.GUERRIER:
                            GestionnaireInput.Update(gameTime);
                            //positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Guerrier.PORTÉE_TORNADE_FURIEUSE);
                            //ZoneDEffet.ChangerÉtendueEtPosition(new Vector2(Guerrier.PORTÉE_TORNADE_FURIEUSE * 2), positionVérifiée);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Guerrier.PORTÉE_TORNADE_FURIEUSE * 2), PersonnageActif.Position);
                            //ZoneDEffet.Visible = true;
                            Portée.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                Cibles = (PersonnageActif as Guerrier).TornadeFurieuse(PersonnageActif.Position, JoueurEnnemi.Personnages, out dégats);
                                PeutAttaquer = false;
                                //ZoneDEffet.Visible = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.MAGE:
                            GestionnaireInput.Update(gameTime);
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Mage.PORTÉE_BRASIER - Mage.RAYON_BRASIER);
                            ZoneDEffet.ChangerÉtendueEtPosition(new Vector2(Mage.RAYON_BRASIER * 2), positionVérifiée);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Mage.PORTÉE_BRASIER * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Portée.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                Cibles = (PersonnageActif as Mage).Brasier(positionVérifiée, JoueurEnnemi.Personnages, out dégats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.PALADIN:
                            GestionnaireInput.Update(gameTime);
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Paladin.PORTÉE_CLARITÉ);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Paladin.PORTÉE_CLARITÉ * 2), PersonnageActif.Position);
                            Portée.Visible = true;
                            singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer();
                            if (singleTargetÀAttaquer != null)
                            {
                                (PersonnageActif as Paladin).Clarité(singleTargetÀAttaquer);
                                PeutAttaquer = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.VOLEUR:
                            GestionnaireInput.Update(gameTime);
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Voleur.PORTÉE_INVISIBILITÉ);
                            ZoneDEffet.ChangerÉtendueEtPosition(new Vector2(2), positionVérifiée);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Voleur.PORTÉE_INVISIBILITÉ* 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Portée.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                NetworkManager.SendNewPosition(positionVérifiée, JoueurLocal.Personnages.FindIndex(p => p is Voleur));
                                ZoneDéplacement.ChangerÉtendueEtPosition(new Vector2(DéplacementRestant * 2), positionVérifiée);
                                NetworkManager.SendInvisibilité(true);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                ActiverAttaque();
                            }
                            break;
                        default:
                            NetworkManager.SendDégât(Cibles.FindAll(cible => !cible.EstMort), dégats);
                            break;
                    }
                }
                if (BoutonsActions.ÉtatSort2)
                {
                    switch (PersonnageActif.GetType().ToString())
                    {
                        case TypePersonnage.ARCHER:
                            GestionnaireInput.Update(gameTime);
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Archer.PORTÉE_FLÈCHE_PERCANTE);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Archer.PORTÉE_FLÈCHE_PERCANTE * 2), PersonnageActif.Position);
                            Portée.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                Cibles = (PersonnageActif as Archer).FlèchePercante(positionVérifiée, JoueurEnnemi.Personnages, out dégats);
                                PeutAttaquer = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                            }
                            goto default;
                        case TypePersonnage.GUÉRISSEUR:
                            GestionnaireInput.Update(gameTime);
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Guérisseur.PORTÉE_RESURRECT);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Guérisseur.PORTÉE_RESURRECT * 2), PersonnageActif.Position);
                            Portée.Visible = true;
                            singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer();
                            if (singleTargetÀAttaquer != null)
                            {
                                dégats = (PersonnageActif as Guérisseur).Résurrection(singleTargetÀAttaquer);
                                Cibles.Add(singleTargetÀAttaquer);
                                PeutAttaquer = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.GUERRIER:
                            GestionnaireInput.Update(gameTime);
                            singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer();
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                (PersonnageActif as Guerrier).Folie();
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.MAGE:
                            GestionnaireInput.Update(gameTime);
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Mage.PORTÉE_FREEZE_DONT_MOVE);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Mage.PORTÉE_FREEZE_DONT_MOVE * 2), PersonnageActif.Position);
                            Portée.Visible = true;
                            singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer();
                            if (singleTargetÀAttaquer != null)
                            {
                                (PersonnageActif as Mage).FreezeDontMove(singleTargetÀAttaquer);
                                PeutAttaquer = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.PALADIN:
                            GestionnaireInput.Update(gameTime);
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Paladin.PORTÉE_BOUCLIER_DIVIN);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Paladin.PORTÉE_BOUCLIER_DIVIN * 2), PersonnageActif.Position);
                            Portée.Visible = true;
                            singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer();
                            if (singleTargetÀAttaquer != null)
                            {
                                (PersonnageActif as Paladin).BouclierDivin(singleTargetÀAttaquer);
                                PeutAttaquer = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.VOLEUR:
                            break;
                        default:
                            NetworkManager.SendDégât(Cibles.FindAll(cible => !cible.EstMort), dégats);
                            break;
                    }
                }
            }
        }

        void ActiverAttaque()
        {
            BoutonsActions.BtnSorts.EstActif = PeutAttaquer;
            BoutonsActions.BtnAttaquer.EstActif = PeutAttaquer;
        }

        void VérifierFinDeTour(GameTime gameTime)
        {
            if (TourFini())
            {
                TerminerLeTour();
                TempsDepuisDernierUpdate = gameTime.TotalGameTime.Seconds;
            }
        }

        bool TourFini()
        {
            return BoutonsActions.ÉtatPasserTour || !PeutAttaquer && (int)Math.Round(DéplacementRestant) <= 0;
        }

        void TerminerLeTour()
        {
            NetworkManager.SendFinDeTour();
            IndicePersonnage = IndicePersonnage < JoueurLocal.Personnages.Count - 1 ? IndicePersonnage + 1 : 0;
            // Voir avec PersonnageActif...
            BoutonsActions.RéinitialiserDialogueActions(JoueurLocal.Personnages[IndicePersonnage]);
            Cibles.Clear();
            DéplacementRestant = DÉPLACEMENT_MAX;
            BoutonsActions.VoirBoutonAction(false);
            ZoneDEffet.Visible = false;
            Portée.Visible = false;
            ZoneDéplacement.Visible = false;
            TourTerminé = true;
        }
    }
}
