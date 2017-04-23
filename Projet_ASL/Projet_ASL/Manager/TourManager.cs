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

        DialogueActions BoutonsActions { get; set; }
        ManagerNetwork NetworkManager { get; set; }
        Player JoueurLocal { get; set; }
        Player JoueurEnnemi { get; set; }
        List<List<BoutonDeCommande>> Boutons { get; set; }
        int IndicePersonnage { get; set; }
        float DéplacementRestant { get; set; }
        Vector3 PositionInitiale { get; set; }
        bool PeutAttaquer { get; set; }

        public TourManager(Jeu jeu, ManagerNetwork networkManager)
            : base(jeu)
        {
            NetworkManager = networkManager;
            BoutonsActions = new DialogueActions(jeu, new Vector2(Game.Window.ClientBounds.Width / 2f, Game.Window.ClientBounds.Height / 5f));
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            JoueurLocal = NetworkManager.JoueurLocal;
            JoueurEnnemi = NetworkManager.JoueurEnnemi;
            IndicePersonnage = 0;
            PositionInitiale = JoueurLocal.Personnages[IndicePersonnage].Position;
            DéplacementRestant = DÉPLACEMENT_MAX;
            CréerBtnClasses();
            base.Initialize();
        }

        private void CréerBtnClasses()
        {
            for(int personnage = 0; personnage < JoueurLocal.Personnages.Count - 1; ++personnage) // À vérifer
            {
                switch(JoueurLocal.Personnages[personnage].GetType().ToString())
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
            if(TourFini())
            {
                NetworkManager.SendFinDeTour();
                IndicePersonnage = IndicePersonnage < JoueurLocal.Personnages.Count - 1 ? IndicePersonnage + 1 : 0;
                BoutonsActions.RéinitialiserDialogueActions(JoueurLocal.Personnages[IndicePersonnage]);
                PositionInitiale = JoueurLocal.Personnages[IndicePersonnage].Position;
                DéplacementRestant = DÉPLACEMENT_MAX;
            }
            base.Update(gameTime);
        }

        bool TourFini()
        {
            return BoutonsActions.ÉtatPasserTour || BoutonsActions.ÉtatAttaquer && (int)Math.Round(DéplacementRestant) == 0;
        }
    }
}
