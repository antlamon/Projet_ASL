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
            VérifierDébutDeTour(gameTime);
            if (!TourTerminé)
            {
                VérifierDéplacement();
                VérifierSorts(gameTime);
                VérifierFinDeTour(gameTime);
            }
            base.Update(gameTime);
        }

        void VérifierDébutDeTour(GameTime gameTime)
        {
            if (ancienIndicePersonnage != IndicePersonnage && gameTime.TotalGameTime.Seconds - TempsDepuisDernierUpdate > 1)
            {
                TourTerminé = false;
                PersonnageActif = JoueurLocal.Personnages[IndicePersonnage];
                if(PersonnageActif is Voleur)
                {
                    PersonnageActif.Visible = true;
                }
                BoutonsActions.VoirBoutonAction(true);
                PeutAttaquer = true;
                ZoneDéplacement.ChangerÉtendueEtPosition(new Vector2(DéplacementRestant * 2), PersonnageActif.Position­);
                ancienIndicePersonnage = IndicePersonnage;
            }
            else
            {
                ZoneDéplacement.Visible = false;
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

            if (PeutAttaquer)
           {
                if (BoutonsActions.ÉtatSort1)
                {
                    Vector3 positionVérifié;
                    switch (PersonnageActif.GetType().ToString())
                    {
                        case TypePersonnage.ARCHER:
                            GestionnaireInput.Update(gameTime);
                            positionVérifié = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Archer.PORTÉE_PLUIE_DE_FLÈCHES - Archer.RAYON_PLUIE_DE_FLÈCHES);
                            ZoneDEffet.ChangerÉtendueEtPosition(new Vector2(Archer.RAYON_PLUIE_DE_FLÈCHES * 2), positionVérifié);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Archer.PORTÉE_PLUIE_DE_FLÈCHES * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Portée.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                int dégats;
                                Cibles = (PersonnageActif as Archer).PluieDeFlèches(positionVérifié, JoueurEnnemi.Personnages, out dégats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        case TypePersonnage.GUÉRISSEUR:
                            break;
                        case TypePersonnage.GUERRIER:
                            GestionnaireInput.Update(gameTime);
                            positionVérifié = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Guerrier.RAYON_TORNADE_FURIEUSE);
                            ZoneDEffet.ChangerÉtendueEtPosition(new Vector2(Archer.RAYON_PLUIE_DE_FLÈCHES * 2), positionVérifié);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Archer.PORTÉE_PLUIE_DE_FLÈCHES * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Portée.Visible = true;
                            if (GestionnaireInput.EstNouveauClicGauche())
                            {
                                int dégats;
                                Cibles = (PersonnageActif as Archer).PluieDeFlèches(positionVérifié, JoueurEnnemi.Personnages, out dégats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
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
                if (BoutonsActions.ÉtatSort2)
                {
                    switch (PersonnageActif.GetType().ToString())
                    {
                        case TypePersonnage.ARCHER:
                            break;
                        case TypePersonnage.GUÉRISSEUR:
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
