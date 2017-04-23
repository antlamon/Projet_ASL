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
        DialogueActions BoutonsActions { get; set; }
        ManagerNetwork NetworkManager { get; set; }
        Player JoueurLocal { get; set; }
        Player JoueurEnnemi { get; set; }
        int CompteurPersonnage { get; set; }

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
            CompteurPersonnage = 0;
            Cr�erBtnClasses();
            base.Initialize();
        }

        private void Cr�erBtnClasses()
        {
            foreach(Personnage p in JoueurLocal.Personnages)
            {
                switch(p.GetType().ToString())
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
            if(BoutonsActions.�tatSorts)
            {
                switch(JoueurLocal.Personnages[CompteurPersonnage].GetType().ToString())
                {
                    case TypePersonnage.ARCHER:
                        BoutonsActions.VoirBoutonsArcher(true);
                        break;
                    case TypePersonnage.GU�RISSEUR:
                        BoutonsActions.VoirBoutonsGu�risseur(true);
                        break;
                    case TypePersonnage.GUERRIER:
                        BoutonsActions.VoirBoutonsGuerrier(true);
                        break;
                    case TypePersonnage.MAGE:
                        BoutonsActions.VoirBoutonsMage(true);
                        break;
                    case TypePersonnage.PALADIN:
                        BoutonsActions.VoirBoutonsPaladin(true);
                        break;
                    case TypePersonnage.VOLEUR:
                        BoutonsActions.VoirBoutonsVoleur(true);
                        break;
                }
            }
            else
            {
                BoutonsActions.BtnPasserTour.Visible = true;

            }
            base.Update(gameTime);
        }
    }
}
