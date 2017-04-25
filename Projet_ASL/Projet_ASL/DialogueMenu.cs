using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projet_ASL
{
    public class DialogueMenu : Microsoft.Xna.Framework.GameComponent
    {
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;
        const int NB_ZONES_DIALOGUE = 3; //Cette constante doit valoir 3 au minimum
        Vector2 DimensionDialogue { get; set; }
        Rectangle RectangleDestination { get; set; }
        public BoutonDeCommande BtnJouer { get; private set; }
        public bool ÉtatJouer { get; set; }
        public BoutonDeCommande BtnInventaire { get; private set; }
        public bool ÉtatInventaire { get; set; }
        public BoutonDeCommande BtnQuitter { get; private set; }
        public BoutonDeCommande BtnRetour { get; set; }
        public bool ÉtatRetourMenu { get; set; }
        public TexteCentré NomJeu { get; private set; }
        SpriteFont Police { get; set; }
        ManagerNetwork _managerNetwork { get; set; }
        public bool MenuVisible { get; private set; }

        public DialogueMenu(Game jeu, Vector2 dimensionDialogue, ManagerNetwork managerNetwork)
           : base(jeu)
        {
            DimensionDialogue = dimensionDialogue;
            RectangleDestination = new Rectangle(Game.Window.ClientBounds.Width - (int)DimensionDialogue.X, 0,
                                                 (int)DimensionDialogue.X, (int)DimensionDialogue.Y);
            ÉtatJouer = false;
            ÉtatInventaire = false;
            MenuVisible = true;
            ÉtatRetourMenu = false;
            _managerNetwork = managerNetwork;
        }

        public override void Initialize()
        {
            int hauteurBouton = RectangleDestination.Height / (NB_ZONES_DIALOGUE + 1);
            Police = Game.Content.Load<SpriteFont>("Fonts/" + "Arial20");

            Vector2 positionBouton = new Vector2(RectangleDestination.X + RectangleDestination.Width / 2f, (NB_ZONES_DIALOGUE - 2) * hauteurBouton);
            BtnJouer = new BoutonDeCommande(Game, "Jouer", "Arial20", "BoutonRouge", "BoutonBleu", positionBouton, true, Jouer, INTERVALLE_MAJ_STANDARD);
            BtnJouer.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            positionBouton = new Vector2(RectangleDestination.X + RectangleDestination.Width / 2f, (NB_ZONES_DIALOGUE - 1) * hauteurBouton);
            BtnInventaire = new BoutonDeCommande(Game, "Inventaire", "Arial20", "BoutonRouge", "BoutonBleu", positionBouton, true, Inventaire, INTERVALLE_MAJ_STANDARD);
            BtnInventaire.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            Vector2 dimensionBouton = Police.MeasureString("Quitter");
            positionBouton = new Vector2(dimensionBouton.X / 2, Game.Window.ClientBounds.Height - dimensionBouton.Y / 2);
            BtnQuitter = new BoutonDeCommande(Game, "Quitter", "Arial20", "BoutonRouge", "BoutonBleu", positionBouton, true, Quitter, INTERVALLE_MAJ_STANDARD);
            BtnQuitter.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            dimensionBouton = Police.MeasureString("Jeu de bataille");
            NomJeu = new TexteCentré(Game, "Jeu de bataille", "Arial20", new Rectangle(100, 100,(int) dimensionBouton.X, (int) dimensionBouton.Y), Color.White, 0);
            NomJeu.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            dimensionBouton = Police.MeasureString("Retour au menu");
            positionBouton = new Vector2(Game.Window.ClientBounds.Width - dimensionBouton.X / 2, Game.Window.ClientBounds.Height - dimensionBouton.Y / 2);
            BtnRetour = new BoutonDeCommande(Game, "Retour au menu", "Arial20", "BoutonRouge", "BoutonBleu", positionBouton, true, Retour, INTERVALLE_MAJ_STANDARD);
            BtnRetour.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            BtnRetour.Enabled = false;
            BtnRetour.Visible = false;

            Game.Components.Add(BtnJouer);
            Game.Components.Add(BtnInventaire);
            Game.Components.Add(BtnQuitter);
            Game.Components.Add(NomJeu);
            Game.Components.Add(BtnRetour);
        }

        private void Retour()
        {
            _managerNetwork.SendLogout();
            ÉtatRetourMenu = true;
        }

        private void Inventaire()
        {
            ÉtatInventaire = true;
        }

        private void Jouer()
        {
            ÉtatJouer = true;
        }

        private void Quitter()
        {
            _managerNetwork.SendLogout();
            Game.Exit();
        }

        public void VoirBoutonMenu(bool x)
        {
            MenuVisible = x;
            BtnJouer.Enabled = x;
            BtnJouer.Visible = x;
            BtnInventaire.Enabled = x;
            BtnInventaire.Visible = x;
            BtnQuitter.Enabled = x;
            BtnQuitter.Visible = x;
            NomJeu.Enabled = x;
            NomJeu.Visible = x;
            ÉtatInventaire = false;
            ÉtatJouer = false;
        }

        public void VoirOptionsMenu(bool x)
        {
            MenuVisible = x;
            BtnQuitter.Enabled = x;
            BtnQuitter.Visible = x;
            BtnRetour.Enabled = x;
            BtnRetour.Visible = x;
            ÉtatRetourMenu = false;
        }
    }
}
