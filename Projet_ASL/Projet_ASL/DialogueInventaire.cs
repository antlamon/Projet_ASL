using System;
using System.Linq;
using Microsoft.Xna.Framework;


namespace Projet_ASL
{
    public class DialogueInventaire : Microsoft.Xna.Framework.GameComponent
    {
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;
        const int NB_ZONES_DIALOGUE = 6; //Cette constante doit valoir 3 au minimum
        public Player _player { get; private set; }
        Vector2 DimensionDialogue { get; set; }
        Rectangle RectangleDestination { get; set; }
        BoutonDeCommande BtnGuerrier { get; set; }
        bool ClickGuerrier { get; set; }
        BoutonDeCommande BtnArcher { get; set; }
        bool ClickArcher { get; set; }
        BoutonDeCommande BtnMage { get; set; }
        bool ClickMage { get; set; }
        BoutonDeCommande BtnGuérisseur { get; set; }
        bool ClickGuérisseur { get; set; }
        BoutonDeCommande BtnVoleur { get; set; }
        bool ClickVoleur { get; set; }
        BoutonDeCommande BtnPaladin { get; set; }
        bool ClickPaladin { get; set; }
        public BoutonDeCommande BtnOK { get; private set; }
        public bool ÉtatMenu { get; private set; }

        public DialogueInventaire(Game jeu, Vector2 dimensionDialogue)
           : base(jeu)
        {
            DimensionDialogue = dimensionDialogue;
            RectangleDestination = new Rectangle((int)Game.Window.ClientBounds.Width - (int)DimensionDialogue.X, 0,
                                                 (int)DimensionDialogue.X, (int)DimensionDialogue.Y);
            _player = new Player();
            ClickArcher = false;
            ClickGuerrier = false;
            ClickGuérisseur = false;
            ClickMage = false;
            ClickPaladin = false;
            ClickVoleur = false;
            ÉtatMenu = false;
        }

        public override void Initialize()
        {
            int positionXBouton = RectangleDestination.Width / (NB_ZONES_DIALOGUE + 1);

            Vector2 PositionBouton = new Vector2((NB_ZONES_DIALOGUE - 5) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnArcher = new BoutonDeCommande(Game, "Archer", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Archer, INTERVALLE_MAJ_STANDARD);
            BtnArcher.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2((NB_ZONES_DIALOGUE - 4) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnGuérisseur = new BoutonDeCommande(Game, "Guérisseur", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Guérisseur, INTERVALLE_MAJ_STANDARD);
            BtnGuérisseur.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2((NB_ZONES_DIALOGUE - 3) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnGuerrier = new BoutonDeCommande(Game, "Guerrier", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Guerrier, INTERVALLE_MAJ_STANDARD);
            BtnGuerrier.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2((NB_ZONES_DIALOGUE - 2) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnMage = new BoutonDeCommande(Game, "Mage", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Mage, INTERVALLE_MAJ_STANDARD);
            BtnMage.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2((NB_ZONES_DIALOGUE - 1) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnPaladin = new BoutonDeCommande(Game, "Paladin", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Paladin, INTERVALLE_MAJ_STANDARD);
            BtnPaladin.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2((NB_ZONES_DIALOGUE) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnVoleur = new BoutonDeCommande(Game, "Voleur", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Voleur, INTERVALLE_MAJ_STANDARD);
            BtnVoleur.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2(Game.Window.ClientBounds.Width - 20, Game.Window.ClientBounds.Height - 20);
            BtnOK = new BoutonDeCommande(Game, "Ok", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, OK, INTERVALLE_MAJ_STANDARD);
            BtnOK.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            Game.Components.Add(BtnArcher);
            Game.Components.Add(BtnGuérisseur);
            Game.Components.Add(BtnGuerrier);
            Game.Components.Add(BtnMage);
            Game.Components.Add(BtnPaladin);
            Game.Components.Add(BtnVoleur);
            Game.Components.Add(BtnOK);

            VoirBoutonInventaire(false);
        }

        private void OK()
        {
            if(_player.Personnages.Count == 4)
            {
                ÉtatMenu = true;
            }
        }

        private void Archer()
        {
            ClickArcher = !ClickArcher;
            BtnArcher.ChangerCouleurActive();
            if (ClickArcher)
            {
                Archer archer = new Archer(Game, "ArcherB", 0.03f, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0, 1);
                archer.DrawOrder = (int)OrdreDraw.MILIEU;
                _player.Personnages.Add(archer);
            }
            else
            {
                _player.Personnages.Remove(_player.Personnages.Find(p => p is Archer));
            }
        }

        private void Guerrier()
        {
            ClickGuerrier = !ClickGuerrier;
            BtnGuerrier.ChangerCouleurActive();
            if (ClickGuerrier)
            {
                Guerrier guerrier = new Guerrier(Game, "GuerrierB", 0.03f, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0, 1);
                guerrier.DrawOrder = (int)OrdreDraw.MILIEU;
                _player.Personnages.Add(guerrier);
            }
            else
            {
                _player.Personnages.Remove(_player.Personnages.Find(p => p is Guerrier));
            }
        }
        private void Guérisseur()
        {
            ClickGuérisseur = !ClickGuérisseur;
            BtnGuérisseur.ChangerCouleurActive();
            if (ClickGuérisseur)
            {
                Guérisseur guérisseur = new Guérisseur(Game, "ArcherB", 0.03f, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0, 1);
                guérisseur.DrawOrder = (int)OrdreDraw.MILIEU;
                _player.Personnages.Add(guérisseur);
            }
            else
            {
                _player.Personnages.Remove(_player.Personnages.Find(p => p is Guérisseur));
            }
        }
        private void Mage()
        {
            ClickMage = !ClickMage;
            BtnMage.ChangerCouleurActive();
            if (ClickMage)
            {
                Mage mage = new Mage(Game, "Mage", 0.03f, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0, 1);
                mage.DrawOrder = (int)OrdreDraw.MILIEU;
                _player.Personnages.Add(mage);
            }
            else
            {
                _player.Personnages.Remove(_player.Personnages.Find(p => p is Mage));
            }
        }
        private void Paladin()
        {
            ClickPaladin = !ClickPaladin;
            BtnPaladin.ChangerCouleurActive();
            if (ClickPaladin)
            {
                Paladin paladin = new Paladin(Game, "ArcherB", 0.03f, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0, 1);
                paladin.DrawOrder = (int)OrdreDraw.MILIEU;
                _player.Personnages.Add(paladin);
            }
            else
            {
                _player.Personnages.Remove(_player.Personnages.Find(p => p is Paladin));
            }
        }
        private void Voleur()
        {
            ClickVoleur = !ClickVoleur;
            BtnVoleur.ChangerCouleurActive();
            if (ClickVoleur)
            {
                Voleur voleur = new Voleur(Game, "ArcherB", 0.03f, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0, 1);
                voleur.DrawOrder = (int)OrdreDraw.MILIEU;
                _player.Personnages.Add(voleur);
            }
            else
            {
                _player.Personnages.Remove(_player.Personnages.Find(p => p is Voleur));
            }
        }


        public void VoirBoutonInventaire(bool v)
        {
            BtnArcher.Visible = v;
            BtnArcher.Enabled = v;
            BtnGuerrier.Visible = v;
            BtnGuerrier.Enabled = v;
            BtnGuérisseur.Visible = v;
            BtnGuérisseur.Enabled = v;
            BtnMage.Enabled = v;
            BtnMage.Visible = v;
            BtnPaladin.Visible = v;
            BtnPaladin.Enabled = v;
            BtnVoleur.Visible = v;
            BtnVoleur.Enabled = v;
            BtnOK.Visible = v;
            BtnOK.Enabled = v;
        }
    }
}
