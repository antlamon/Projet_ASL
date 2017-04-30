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
            V�rifierD�butDeTour(gameTime);
            if (!TourTermin�)
            {
                V�rifierD�placement();
                V�rifierSorts(gameTime);
                V�rifierFinDeTour(gameTime);
            }
            base.Update(gameTime);
        }

        void V�rifierD�butDeTour(GameTime gameTime)
        {
            if (ancienIndicePersonnage != IndicePersonnage && gameTime.TotalGameTime.Seconds - TempsDepuisDernierUpdate > 1)
            {
                TourTermin� = false;
                PersonnageActif = JoueurLocal.Personnages[IndicePersonnage];
                if(PersonnageActif is Voleur)
                {
                    PersonnageActif.Visible = true;
                }
                BoutonsActions.VoirBoutonAction(true);
                PeutAttaquer = true;
                ZoneD�placement.Changer�tendueEtPosition(new Vector2(D�placementRestant * 2), PersonnageActif.Position�);
                ancienIndicePersonnage = IndicePersonnage;
            }
            else
            {
                ZoneD�placement.Visible = false;
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

            if (PeutAttaquer)
           {
                if (BoutonsActions.�tatSort1)
                {
                    Vector3 positionV�rifi�;
                    switch (PersonnageActif.GetType().ToString())
                    {
                        case TypePersonnage.ARCHER:
                            GestionnaireInput.Update(gameTime);
                            positionV�rifi� = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Archer.PORT�E_PLUIE_DE_FL�CHES - Archer.RAYON_PLUIE_DE_FL�CHES);
                            ZoneDEffet.Changer�tendueEtPosition(new Vector2(Archer.RAYON_PLUIE_DE_FL�CHES * 2), positionV�rifi�);
                            Port�e.Changer�tendueEtPosition(new Vector2(Archer.PORT�E_PLUIE_DE_FL�CHES * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Port�e.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                int d�gats;
                                Cibles = (PersonnageActif as Archer).PluieDeFl�ches(positionV�rifi�, JoueurEnnemi.Personnages, out d�gats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.GU�RISSEUR:
                            break;
                        case TypePersonnage.GUERRIER:
                            GestionnaireInput.Update(gameTime);
                            positionV�rifi� = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Guerrier.RAYON_TORNADE_FURIEUSE);
                            ZoneDEffet.Changer�tendueEtPosition(new Vector2(Archer.RAYON_PLUIE_DE_FL�CHES * 2), positionV�rifi�);
                            Port�e.Changer�tendueEtPosition(new Vector2(Archer.PORT�E_PLUIE_DE_FL�CHES * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Port�e.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                int d�gats;
                                Cibles = (PersonnageActif as Archer).PluieDeFl�ches(positionV�rifi�, JoueurEnnemi.Personnages, out d�gats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.MAGE:
                            break;
                        case TypePersonnage.PALADIN:
                            break;
                        case TypePersonnage.VOLEUR:

                            break;
                    }
                }
                if (BoutonsActions.�tatSort2)
                {
                    switch (PersonnageActif.GetType().ToString())
                    {
                        case TypePersonnage.ARCHER:
                            break;
                        case TypePersonnage.GU�RISSEUR:
                            break;
                        case TypePersonnage.GUERRIER:
                            break;
                        case TypePersonnage.MAGE:
                            break;
                        case TypePersonnage.PALADIN:
                            break;
                        case TypePersonnage.VOLEUR:

                            break;
                    }
                }
            }
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
