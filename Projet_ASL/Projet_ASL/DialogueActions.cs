using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projet_ASL
{
    public class DialogueActions : Microsoft.Xna.Framework.GameComponent
    {
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;
        const int NB_ZONES_DIALOGUE = 3; //Cette constante doit valoir 3 au minimum
        Vector2 DimensionDialogue { get; set; }
        Rectangle RectangleDestination { get; set; }
        public BoutonDeCommande BtnD�placement { get; private set; }
        public bool �tatD�placement { get; set; }
        public BoutonDeCommande BtnAttaquer { get; private set; }
        public bool �tatAttaquer { get; set; }
        public BoutonDeCommande BtnPasserTour { get; private set; }
        public TexteCentr� NomJeu { get; private set; }
        SpriteFont Police { get; set; }
        public bool MenuActionVisible { get; private set; }

        public DialogueActions(Game jeu, Vector2 dimensionDialogue)
           : base(jeu)
        {
            DimensionDialogue = dimensionDialogue;
            //zone occup�e par dialogue, comprend position
            RectangleDestination = new Rectangle(0, 0, (int)DimensionDialogue.X, (int)DimensionDialogue.Y);
            �tatD�placement = false;
            �tatAttaquer = false;
            MenuActionVisible = true;
        }

        public override void Initialize()
        {
            int hauteurBouton = RectangleDestination.Height / (NB_ZONES_DIALOGUE + 1);
            Police = Game.Content.Load<SpriteFont>("Fonts/" + "Arial20");

            Vector2 DimensionBouton = Police.MeasureString("Jouer");
            Vector2 PositionBouton = new Vector2(RectangleDestination.X + RectangleDestination.Width / 2f, (NB_ZONES_DIALOGUE - 2) * hauteurBouton);
            BtnD�placement = new BoutonDeCommande(Game, "Jouer", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Jouer, INTERVALLE_MAJ_STANDARD);
            BtnD�placement.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            DimensionBouton = Police.MeasureString("Inventaire");
            PositionBouton = new Vector2(RectangleDestination.X + RectangleDestination.Width / 2f, (NB_ZONES_DIALOGUE - 1) * hauteurBouton);
            BtnAttaquer = new BoutonDeCommande(Game, "Inventaire", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Inventaire, INTERVALLE_MAJ_STANDARD);
            BtnAttaquer.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            DimensionBouton = Police.MeasureString("Quitter");
            PositionBouton = new Vector2(DimensionBouton.X / 2, Game.Window.ClientBounds.Height - DimensionBouton.Y / 2);
            BtnPasserTour = new BoutonDeCommande(Game, "Quitter", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Quitter, INTERVALLE_MAJ_STANDARD);
            BtnPasserTour.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            DimensionBouton = Police.MeasureString("Jeu de bataille");
            NomJeu = new TexteCentr�(Game, "Jeu de bataille", "Arial20", new Rectangle(100, 100, (int)DimensionBouton.X, (int)DimensionBouton.Y), Color.White, 0.10f);
            NomJeu.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            Game.Components.Add(BtnD�placement);
            Game.Components.Add(BtnAttaquer);
            Game.Components.Add(BtnPasserTour);
            Game.Components.Add(NomJeu);

            VoirBouttonAction(false);
        }

        private void Inventaire()
        {
            �tatAttaquer = true;
        }

        private void Jouer()
        {
            �tatD�placement = true;
        }

        public void VoirBouttonAction(bool x)
        {
            MenuActionVisible = x;
            BtnD�placement.Enabled = x;
            BtnD�placement.Visible = x;
            BtnAttaquer.Enabled = x;
            BtnAttaquer.Visible = x;
            BtnPasserTour.Enabled = x;
            BtnPasserTour.Visible = x;
            NomJeu.Enabled = x;
            NomJeu.Visible = x;
            �tatAttaquer = false;
            �tatD�placement = false;
        }
    }
}
