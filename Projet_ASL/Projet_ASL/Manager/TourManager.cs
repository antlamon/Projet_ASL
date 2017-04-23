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

        DialogueActions BoutonsActions { get; set; }
        ManagerNetwork NetworkManager { get; set; }
        Player JoueurLocal { get; set; }
        Player JoueurEnnemi { get; set; }
        List<List<BoutonDeCommande>> Boutons { get; set; }
        int IndicePersonnage { get; set; }
        float D�placementRestant { get; set; }
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
            D�placementRestant = D�PLACEMENT_MAX;
            Cr�erBtnClasses();
            base.Initialize();
        }

        private void Cr�erBtnClasses()
        {
            for(int personnage = 0; personnage < JoueurLocal.Personnages.Count - 1; ++personnage) // � v�rifer
            {
                switch(JoueurLocal.Personnages[personnage].GetType().ToString())
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
            if(TourFini())
            {
                NetworkManager.SendFinDeTour();
                IndicePersonnage = IndicePersonnage < JoueurLocal.Personnages.Count - 1 ? IndicePersonnage + 1 : 0;
                BoutonsActions.R�initialiserDialogueActions(JoueurLocal.Personnages[IndicePersonnage]);
                PositionInitiale = JoueurLocal.Personnages[IndicePersonnage].Position;
                D�placementRestant = D�PLACEMENT_MAX;
            }
            base.Update(gameTime);
        }

        bool TourFini()
        {
            return BoutonsActions.�tatPasserTour || BoutonsActions.�tatAttaquer && (int)Math.Round(D�placementRestant) == 0;
        }
    }
}
