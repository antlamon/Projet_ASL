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
        const int D�PLACEMENT_MAX = 10;
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;

        public DialogueActions BoutonsActions { get; private set; }
        ManagerNetwork NetworkManager { get; set; }
        InputManager GestionnaireInput { get; set; }
        Player JoueurLocal { get; set; }
        Player JoueurEnnemi { get; set; }
        Personnage PersonnageActif { get; set; }
        int ancienIndicePersonnage { get; set; }
        int IndicePersonnage { get; set; }
        float D�placementRestant { get; set; }
        float ancienD�placementRestant { get; set; }
        bool PeutAttaquer { get; set; }
        int TempsDepuisDernierUpdate { get; set; }
        AOE ZoneDEffet { get; set; }
        AOE Port�e { get; set; }
        AOE ZoneD�placement { get; set; }
        Jeu Jeu { get; set; }
        List<Personnage> Cibles { get; set; }
        bool TourTermin� { get; set; }

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
            D�placementRestant = D�PLACEMENT_MAX;
            Cr�erBtnClasses();
            PersonnageActif = JoueurLocal.Personnages[IndicePersonnage];
            BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
            BoutonsActions.VoirBoutonAction(NetworkManager.TourActif);
            ZoneDEffet = Jeu.AOE1;
            Port�e = Jeu.AOE2;
            ZoneD�placement = Jeu.AOE3;
            TourTermin� = false;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            GestionnaireInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            base.LoadContent();
        }

        private void Cr�erBtnClasses()
        {
            foreach (Personnage p in JoueurLocal.Personnages)
            {
                switch (p.GetType().ToString())
                {
                    case TypePersonnage.ARCHER:
                        BoutonsActions.Cr�erBtnArcher();
                        break;
                    case TypePersonnage.GU�RISSEUR:
                        BoutonsActions.Cr�erBtnGu�risseur();
                        break;
                    case TypePersonnage.GUERRIER:
                        BoutonsActions.Cr�erBtnGuerrier();
                        break;
                    case TypePersonnage.MAGE:
                        BoutonsActions.Cr�erBtnMage();
                        break;
                    case TypePersonnage.PALADIN:
                        BoutonsActions.Cr�erBtnPaladin();
                        break;
                    case TypePersonnage.VOLEUR:
                        BoutonsActions.Cr�erBtnVoleur();
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
                V�rifierD�butDeTour(gameTime);
                if (!TourTermin�)
                {
                    V�rifierD�placement();
                    V�rifierSorts(gameTime);
                    V�rifierFinDeTour(gameTime);
                }
            }
            else
            {
                IndicePersonnage = IndicePersonnage < JoueurLocal.Personnages.Count - 1 ? IndicePersonnage + 1 : 0;
            }
        }

        void V�rifierD�butDeTour(GameTime gameTime)
        {
            if (ancienIndicePersonnage != IndicePersonnage && gameTime.TotalGameTime.Seconds - TempsDepuisDernierUpdate > 1)
            {
                TourTermin� = false;
                PersonnageActif = JoueurLocal.Personnages[IndicePersonnage];
                if (PersonnageActif is Voleur)
                {
                    NetworkManager.SendInvisibilit�(false);
                }
                BoutonsActions.VoirBoutonAction(true);
                PeutAttaquer = true;
                ActiverAttaque();
                ZoneD�placement.Changer�tendueEtPosition(new Vector2(D�placementRestant * 2), PersonnageActif.Position�);
                ancienIndicePersonnage = IndicePersonnage;
            }
        }

        void V�rifierD�placement()
        {
            if (!BoutonsActions.�tatSort1 && !BoutonsActions.�tatSort2 && !BoutonsActions.�tatAttaquer && D�placementRestant >= 0.5f)
            {
                ZoneD�placement.Visible = true;
                GestionnaireInput.D�terminerS�lectionPersonnageD�placement(IndicePersonnage);
                D�placementRestant = GestionnaireInput.D�terminerMouvementPersonnageS�lectionn�(D�placementRestant, IndicePersonnage);
                D�placerZoneMouvement();
            }
            else
            {
                ZoneD�placement.Visible = false;
            }
        }

        void D�placerZoneMouvement()
        {
            if (ancienD�placementRestant != D�placementRestant)
            {
                ZoneD�placement.Changer�tendueEtPosition(new Vector2(D�placementRestant * 2), PersonnageActif.Position�);
                ancienD�placementRestant = D�placementRestant;
            }
        }

        void V�rifierSorts(GameTime gameTime)
        {
            Vector3 positionV�rifi�e;
            Personnage singleTarget�Attaquer = null;
            int d�gats = 0;

            if (PeutAttaquer)
            {
                if (BoutonsActions.�tatSort1)
                {
                    switch (PersonnageActif.GetType().ToString())
                    {
                        case TypePersonnage.ARCHER:
                            GestionnaireInput.Update(gameTime);
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Archer.PORT�E_PLUIE_DE_FL�CHES - Archer.RAYON_PLUIE_DE_FL�CHES);
                            ZoneDEffet.Changer�tendueEtPosition(new Vector2(Archer.RAYON_PLUIE_DE_FL�CHES * 2), positionV�rifi�e);
                            Port�e.Changer�tendueEtPosition(new Vector2(Archer.PORT�E_PLUIE_DE_FL�CHES * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Port�e.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                Cibles = (PersonnageActif as Archer).PluieDeFl�ches(positionV�rifi�e, JoueurEnnemi.Personnages, out d�gats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                ActiverAttaque();
                                goto default;
                            }
                            break;
                        case TypePersonnage.GU�RISSEUR:
                            GestionnaireInput.Update(gameTime);
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Gu�risseur.PORT�E_SOIN_DE_ZONE - Gu�risseur.RAYON_SOIN_DE_ZONE);
                            ZoneDEffet.Changer�tendueEtPosition(new Vector2(Gu�risseur.RAYON_SOIN_DE_ZONE * 2), positionV�rifi�e);
                            Port�e.Changer�tendueEtPosition(new Vector2(Gu�risseur.PORT�E_SOIN_DE_ZONE * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Port�e.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                Cibles = (PersonnageActif as Gu�risseur).SoinDeZone(positionV�rifi�e, JoueurLocal.Personnages, out d�gats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                goto default;
                            }
                            break;
                        case TypePersonnage.GUERRIER:
                            GestionnaireInput.Update(gameTime);
                            //positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Guerrier.PORT�E_TORNADE_FURIEUSE);
                            //ZoneDEffet.Changer�tendueEtPosition(new Vector2(Guerrier.PORT�E_TORNADE_FURIEUSE * 2), positionV�rifi�e);
                            Port�e.Changer�tendueEtPosition(new Vector2(Guerrier.PORT�E_TORNADE_FURIEUSE * 2), PersonnageActif.Position);
                            //ZoneDEffet.Visible = true;
                            Port�e.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                Cibles = (PersonnageActif as Guerrier).TornadeFurieuse(PersonnageActif.Position, JoueurEnnemi.Personnages, out d�gats);
                                PeutAttaquer = false;
                                //ZoneDEffet.Visible = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.MAGE:
                            GestionnaireInput.Update(gameTime);
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Mage.PORT�E_BRASIER - Mage.RAYON_BRASIER);
                            ZoneDEffet.Changer�tendueEtPosition(new Vector2(Mage.RAYON_BRASIER * 2), positionV�rifi�e);
                            Port�e.Changer�tendueEtPosition(new Vector2(Mage.PORT�E_BRASIER * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Port�e.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                Cibles = (PersonnageActif as Mage).Brasier(positionV�rifi�e, JoueurEnnemi.Personnages, out d�gats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.PALADIN:
                            GestionnaireInput.Update(gameTime);
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Paladin.PORT�E_CLARIT�);
                            Port�e.Changer�tendueEtPosition(new Vector2(Paladin.PORT�E_CLARIT� * 2), PersonnageActif.Position);
                            Port�e.Visible = true;
                            singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer();
                            if (singleTarget�Attaquer != null)
                            {
                                (PersonnageActif as Paladin).Clarit�(singleTarget�Attaquer);
                                PeutAttaquer = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.VOLEUR:
                            GestionnaireInput.Update(gameTime);
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Voleur.PORT�E_INVISIBILIT�);
                            ZoneDEffet.Changer�tendueEtPosition(new Vector2(2), positionV�rifi�e);
                            Port�e.Changer�tendueEtPosition(new Vector2(Voleur.PORT�E_INVISIBILIT�* 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Port�e.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                NetworkManager.SendNewPosition(positionV�rifi�e, JoueurLocal.Personnages.FindIndex(p => p is Voleur));
                                ZoneD�placement.Changer�tendueEtPosition(new Vector2(D�placementRestant * 2), positionV�rifi�e);
                                NetworkManager.SendInvisibilit�(true);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                ActiverAttaque();
                            }
                            break;
                        default:
                            NetworkManager.SendD�g�t(Cibles.FindAll(cible => !cible.EstMort), d�gats);
                            break;
                    }
                }
                if (BoutonsActions.�tatSort2)
                {
                    switch (PersonnageActif.GetType().ToString())
                    {
                        case TypePersonnage.ARCHER:
                            GestionnaireInput.Update(gameTime);
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Archer.PORT�E_FL�CHE_PERCANTE);
                            Port�e.Changer�tendueEtPosition(new Vector2(Archer.PORT�E_FL�CHE_PERCANTE * 2), PersonnageActif.Position);
                            Port�e.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                Cibles = (PersonnageActif as Archer).Fl�chePercante(positionV�rifi�e, JoueurEnnemi.Personnages, out d�gats);
                                PeutAttaquer = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                            }
                            goto default;
                        case TypePersonnage.GU�RISSEUR:
                            GestionnaireInput.Update(gameTime);
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Gu�risseur.PORT�E_RESURRECT);
                            Port�e.Changer�tendueEtPosition(new Vector2(Gu�risseur.PORT�E_RESURRECT * 2), PersonnageActif.Position);
                            Port�e.Visible = true;
                            singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer();
                            if (singleTarget�Attaquer != null)
                            {
                                d�gats = (PersonnageActif as Gu�risseur).R�surrection(singleTarget�Attaquer);
                                Cibles.Add(singleTarget�Attaquer);
                                PeutAttaquer = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.GUERRIER:
                            GestionnaireInput.Update(gameTime);
                            singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer();
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                (PersonnageActif as Guerrier).Folie();
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.MAGE:
                            GestionnaireInput.Update(gameTime);
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Mage.PORT�E_FREEZE_DONT_MOVE);
                            Port�e.Changer�tendueEtPosition(new Vector2(Mage.PORT�E_FREEZE_DONT_MOVE * 2), PersonnageActif.Position);
                            Port�e.Visible = true;
                            singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer();
                            if (singleTarget�Attaquer != null)
                            {
                                (PersonnageActif as Mage).FreezeDontMove(singleTarget�Attaquer);
                                PeutAttaquer = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.PALADIN:
                            GestionnaireInput.Update(gameTime);
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Paladin.PORT�E_BOUCLIER_DIVIN);
                            Port�e.Changer�tendueEtPosition(new Vector2(Paladin.PORT�E_BOUCLIER_DIVIN * 2), PersonnageActif.Position);
                            Port�e.Visible = true;
                            singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer();
                            if (singleTarget�Attaquer != null)
                            {
                                (PersonnageActif as Paladin).BouclierDivin(singleTarget�Attaquer);
                                PeutAttaquer = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.VOLEUR:
                            break;
                        default:
                            NetworkManager.SendD�g�t(Cibles.FindAll(cible => !cible.EstMort), d�gats);
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

        void V�rifierFinDeTour(GameTime gameTime)
        {
            if (TourFini())
            {
                TerminerLeTour();
                TempsDepuisDernierUpdate = gameTime.TotalGameTime.Seconds;
            }
        }

        bool TourFini()
        {
            return BoutonsActions.�tatPasserTour || !PeutAttaquer && (int)Math.Round(D�placementRestant) <= 0;
        }

        void TerminerLeTour()
        {
            NetworkManager.SendFinDeTour();
            IndicePersonnage = IndicePersonnage < JoueurLocal.Personnages.Count - 1 ? IndicePersonnage + 1 : 0;
            // Voir avec PersonnageActif...
            BoutonsActions.R�initialiserDialogueActions(JoueurLocal.Personnages[IndicePersonnage]);
            Cibles.Clear();
            D�placementRestant = D�PLACEMENT_MAX;
            BoutonsActions.VoirBoutonAction(false);
            ZoneDEffet.Visible = false;
            Port�e.Visible = false;
            ZoneD�placement.Visible = false;
            TourTermin� = true;
        }
    }
}
