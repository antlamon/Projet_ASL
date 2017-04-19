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
        public TexteCentré NomJeu { get; private set; }
        SpriteFont Police { get; set; }
        ManagerNetwork _managerNetwork { get; set; }
        public bool MenuVisible { get; private set; }

        public DialogueMenu(Game jeu, Vector2 dimensionDialogue, ManagerNetwork managerNetwork)
           : base(jeu)
        {
            DimensionDialogue = dimensionDialogue;
            RectangleDestination = new Rectangle((int)Game.Window.ClientBounds.Width - (int)DimensionDialogue.X, 0,
                                                 (int)DimensionDialogue.X, (int)DimensionDialogue.Y);
            ÉtatJouer = false;
            ÉtatInventaire = false;
            MenuVisible = true;
            _managerNetwork = managerNetwork;
        }

        public override void Initialize()
        {
            int hauteurBouton = RectangleDestination.Height / (NB_ZONES_DIALOGUE + 1);
            Police = Game.Content.Load<SpriteFont>("Fonts/" + "Arial20");

            Vector2 DimensionBouton = Police.MeasureString("Jouer");
            Vector2 PositionBouton = new Vector2(RectangleDestination.X + RectangleDestination.Width / 2f, (NB_ZONES_DIALOGUE - 2) * hauteurBouton);
            BtnJouer = new BoutonDeCommande(Game, "Jouer", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Jouer, INTERVALLE_MAJ_STANDARD);
            BtnJouer.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            DimensionBouton = Police.MeasureString("Inventaire");
            PositionBouton = new Vector2(RectangleDestination.X + RectangleDestination.Width / 2f, (NB_ZONES_DIALOGUE - 1) * hauteurBouton);
            BtnInventaire = new BoutonDeCommande(Game, "Inventaire", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Inventaire, INTERVALLE_MAJ_STANDARD);
            BtnInventaire.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            DimensionBouton = Police.MeasureString("Quitter");
            PositionBouton = new Vector2(DimensionBouton.X / 2, Game.Window.ClientBounds.Height - DimensionBouton.Y / 2);
            BtnQuitter = new BoutonDeCommande(Game, "Quitter", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Quitter, INTERVALLE_MAJ_STANDARD);
            BtnQuitter.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            DimensionBouton = Police.MeasureString("Jeu de bataille");
            NomJeu = new TexteCentré(Game, "Jeu de bataille", "Arial20", new Rectangle(100, 100,(int) DimensionBouton.X, (int) DimensionBouton.Y), Color.White, 0.10f);
            NomJeu.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            Game.Components.Add(BtnJouer);
            Game.Components.Add(BtnInventaire);
            Game.Components.Add(BtnQuitter);
            Game.Components.Add(NomJeu);
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

        public void VoirBouttonMenu(bool x)
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
    }
}
