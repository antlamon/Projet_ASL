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
        List<List<BoutonDeCommande>> Boutons { get; set; }
        int ancienIndicePersonnage { get; set; }
        int IndicePersonnage { get; set; }
        float DéplacementRestant { get; set; }
        Vector3 PositionInitiale { get; set; }
        bool PeutAttaquer { get; set; }
        int TempsDepuisDernierUpdate { get; set; }
        public AOE ZoneDEffet { get; private set; }
        public AOE Portée { get; private set; }

        public TourManager(Jeu jeu, ManagerNetwork networkManager)
            : base(jeu)
        {
            NetworkManager = networkManager;
            BoutonsActions = new DialogueActions(jeu, new Vector2(Game.Window.ClientBounds.Width / 3f, Game.Window.ClientBounds.Height / 7f), NetworkManager);
            Game.Components.Add(BoutonsActions);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            JoueurLocal = NetworkManager.JoueurLocal;
            //JoueurEnnemi = NetworkManager.JoueurEnnemi;
            ancienIndicePersonnage = -1;
            IndicePersonnage = 0;
            TempsDepuisDernierUpdate = 0;
            PositionInitiale = JoueurLocal.Personnages[IndicePersonnage].Position;
            DéplacementRestant = DÉPLACEMENT_MAX;
            CréerBtnClasses();
            BoutonsActions.RéinitialiserDialogueActions(JoueurLocal.Personnages[IndicePersonnage]);
            BoutonsActions.VoirBoutonAction(NetworkManager.TourActif);
            ZoneDEffet = new AOE(Game, 1f, Vector3.Zero, Vector3.Zero, new Vector2(20), "AOE", INTERVALLE_MAJ_STANDARD);
            ZoneDEffet.Visible = false;
            ZoneDEffet.DrawOrder = (int)OrdreDraw.ARRIÈRE_PLAN;
            Game.Components.Add(ZoneDEffet);
            Portée = new AOE(Game, 1f, Vector3.Zero, Vector3.Zero, new Vector2(20), "AOE", INTERVALLE_MAJ_STANDARD);
            Portée.Visible = false;
            Portée.DrawOrder = (int)OrdreDraw.ARRIÈRE_PLAN;
            Game.Components.Add(Portée);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            GestionnaireInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            base.LoadContent();
        }

        private void CréerBtnClasses()
        {
            foreach(Personnage p in JoueurLocal.Personnages)
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
            if (ancienIndicePersonnage != IndicePersonnage && gameTime.TotalGameTime.Seconds - TempsDepuisDernierUpdate > 1)
            {
                BoutonsActions.VoirBoutonAction(true);
                ancienIndicePersonnage = IndicePersonnage;
                ZoneDEffet.Visible = true;
                ZoneDEffet.ChangerÉtendueEtPosition(new Vector2(DéplacementRestant * 2), JoueurLocal.Personnages[IndicePersonnage].Position­);
                ZoneDEffet.Update(gameTime);
            }
            if (!BoutonsActions.ÉtatSorts && !BoutonsActions.ÉtatAttaquer)
            {
                if (DéplacementRestant >= 0.5f)
                {
                    GestionnaireInput.DéterminerSélectionPersonnageDéplacement(IndicePersonnage);
                DéplacementRestant = GestionnaireInput.DéterminerMouvementPersonnageSélectionné(DéplacementRestant, IndicePersonnage);
                }

            }
            if (TourFini())
            {
                NetworkManager.SendFinDeTour();
                IndicePersonnage = IndicePersonnage < JoueurLocal.Personnages.Count - 1 ? IndicePersonnage + 1 : 0;
                BoutonsActions.RéinitialiserDialogueActions(JoueurLocal.Personnages[IndicePersonnage]);
                PositionInitiale = JoueurLocal.Personnages[IndicePersonnage].Position;
                DéplacementRestant = DÉPLACEMENT_MAX;
                BoutonsActions.VoirBoutonAction(false);
                TempsDepuisDernierUpdate = gameTime.TotalGameTime.Seconds;
            }
            base.Update(gameTime);
        }

        bool TourFini()
        {
            return BoutonsActions.ÉtatPasserTour || BoutonsActions.ÉtatAttaquer && (int)Math.Round(DéplacementRestant) <= 0;
        }
    }
}
