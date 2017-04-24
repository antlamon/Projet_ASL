using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Projet_ASL
{
    public class DialogueActions : Microsoft.Xna.Framework.GameComponent
    {
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;
        const int NB_ZONES_DIALOGUE = 3; //Cette constante doit valoir 3 au minimum
        Vector2 DimensionDialogue { get; set; }
        Rectangle RectangleDestination { get; set; }
        public BoutonDeCommande BtnSorts { get; private set; }
        public bool �tatSorts { get; set; }
        public BoutonDeCommande BtnAttaquer { get; private set; }
        public bool �tatAttaquer { get; private set; }
        public BoutonDeCommande BtnPasserTour { get; private set; }
        public bool �tatPasserTour { get; private set; }
        SpriteFont Police { get; set; }
        public bool MenuActionVisible { get; private set; }
        Personnage PersonnageActif { get; set; }

        #region Btn Sorts
        BoutonDeCommande BtnPluieDeFl�ches { get; set; }
        BoutonDeCommande BtnFl�chePercante { get; set; }
        BoutonDeCommande BtnSoinDeZone { get; set; }
        BoutonDeCommande BtnR�ssurection { get; set; }
        BoutonDeCommande BtnVolDeVie { get; set; }
        BoutonDeCommande BtnTornadeFurieuse { get; set; }
        BoutonDeCommande BtnFolie { get; set; }
        BoutonDeCommande BtnBrazzer { get; set; }
        BoutonDeCommande BtnFreezeDontMove { get; set; }
        BoutonDeCommande BtnBouclierDivin { get; set; }
        BoutonDeCommande BtnClarit� { get; set; }
        BoutonDeCommande BtnInvisibilit� { get; set; }
        BoutonDeCommande BtnLancerCouteau { get; set; }
        BoutonDeCommande BtnRetour { get; set; }
        #endregion

        public DialogueActions(Game jeu, Vector2 dimensionDialogue, ManagerNetwork managerNetwork)
           : base(jeu)
        {
            DimensionDialogue = dimensionDialogue;
            //zone occup�e par dialogue, comprend position
            RectangleDestination = new Rectangle(Game.Window.ClientBounds.Width - 2*(int)DimensionDialogue.X, Game.Window.ClientBounds.Height - (int)DimensionDialogue.Y,
                                                 (int)DimensionDialogue.X, (int)DimensionDialogue.Y);
            �tatAttaquer = false;
            MenuActionVisible = true;
        }

        public void R�initialiserDialogueActions(Personnage personnage)
        {
            Retour();
            PersonnageActif = personnage;
        }

        public override void Initialize()
        {
            int positionXBouton = RectangleDestination.Width / (NB_ZONES_DIALOGUE + 1);

            Vector2 PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 2) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnAttaquer = new BoutonDeCommande(Game, "Attaquer", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnAttaquer.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 1) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnSorts = new BoutonDeCommande(Game, "Sorts", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Sorts, INTERVALLE_MAJ_STANDARD);
            BtnSorts.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnPasserTour = new BoutonDeCommande(Game, "Passer", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, PasserTour, INTERVALLE_MAJ_STANDARD);
            BtnPasserTour.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            BtnRetour = new BoutonDeCommande(Game, "retour", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Retour, INTERVALLE_MAJ_STANDARD);
            BtnRetour.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            Game.Components.Add(BtnSorts);
            Game.Components.Add(BtnAttaquer);
            Game.Components.Add(BtnPasserTour);
            Game.Components.Add(BtnRetour);

            VoirBoutonRetour(false);
            VoirBoutonAction(false);
        }

        public void VoirBoutonRetour(bool x)
        {
            BtnRetour.Enabled = x;
            BtnRetour.Visible = x;
        }

        #region Archer
        public void Cr�erBtnArcher()
        {
            int positionXBouton = RectangleDestination.Width / (NB_ZONES_DIALOGUE + 1);

            Vector2 PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 2) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnPluieDeFl�ches = new BoutonDeCommande(Game, "Pluie de fl�ches", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnPluieDeFl�ches.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 1) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnFl�chePercante = new BoutonDeCommande(Game, "Fl�che per�ante", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnFl�chePercante.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            Game.Components.Add(BtnPluieDeFl�ches);
            Game.Components.Add(BtnFl�chePercante);

            VoirBoutonsArcher(false);
        }

        public void VoirBoutonsArcher(bool v)
        {
            BtnPluieDeFl�ches.Enabled = v;
            BtnPluieDeFl�ches.Visible = v;
            BtnFl�chePercante.Enabled = v;
            BtnFl�chePercante.Visible = v;
            VoirBoutonRetour(v);
        }
        #endregion

        #region Gu�risseur
        public void Cr�erBtnGu�risseur()
        {
            int positionXBouton = RectangleDestination.Width / (NB_ZONES_DIALOGUE + 1);

            Vector2 PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 2) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnSoinDeZone = new BoutonDeCommande(Game, "Soin de zone", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnSoinDeZone.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 1) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnR�ssurection = new BoutonDeCommande(Game, "R�surrection", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnR�ssurection.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            BtnVolDeVie = new BoutonDeCommande(Game, "Vol de vie", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnVolDeVie.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            Game.Components.Add(BtnSoinDeZone);
            Game.Components.Add(BtnR�ssurection);
            Game.Components.Add(BtnVolDeVie);


            VoirBoutonsGu�risseur(false);
            VoirBoutonsSatan(false);
        }

        public void VoirBoutonsSatan(bool v)
        {
            BtnSoinDeZone.Enabled = v;
            BtnSoinDeZone.Visible = v;
            BtnVolDeVie.Enabled = v;
            BtnVolDeVie.Visible = v;
            VoirBoutonRetour(v);
        }

        public void VoirBoutonsGu�risseur(bool v)
        {
            BtnSoinDeZone.Enabled = v;
            BtnSoinDeZone.Visible = v;
            BtnR�ssurection.Enabled = v;
            BtnR�ssurection.Visible = v;
            VoirBoutonRetour(v);
        }
        #endregion

        #region Guerrier
        public void Cr�erBtnGuerrier()
        {
            int positionXBouton = RectangleDestination.Width / (NB_ZONES_DIALOGUE + 1);

            Vector2 PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 2) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnTornadeFurieuse = new BoutonDeCommande(Game, "Tornade furieuse", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnTornadeFurieuse.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 1) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnFolie = new BoutonDeCommande(Game, "Folie", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnFolie.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            Game.Components.Add(BtnTornadeFurieuse);
            Game.Components.Add(BtnFolie);

            VoirBoutonsGuerrier(false);
        }

        public void VoirBoutonsGuerrier(bool v)
        {
            BtnTornadeFurieuse.Enabled = v;
            BtnTornadeFurieuse.Visible = v;
            BtnFolie.Enabled = v;
            BtnFolie.Visible = v;
            VoirBoutonRetour(v);
        }
        #endregion

        #region Mage
        public void Cr�erBtnMage()
        {
            int positionXBouton = RectangleDestination.Width / (NB_ZONES_DIALOGUE + 1);

            Vector2 PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 2) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnBrazzer = new BoutonDeCommande(Game, "Brasier", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnBrazzer.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 1) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnFreezeDontMove = new BoutonDeCommande(Game, "Freeze", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnFreezeDontMove.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            Game.Components.Add(BtnBrazzer);
            Game.Components.Add(BtnFreezeDontMove);

            VoirBoutonsMage(false);
        }

        public void VoirBoutonsMage(bool v)
        {
            BtnBrazzer.Enabled = v;
            BtnBrazzer.Visible = v;
            BtnFreezeDontMove.Enabled = v;
            BtnFreezeDontMove.Visible = v;
            VoirBoutonRetour(v);
        }
        #endregion

        #region Paladin
        public void Cr�erBtnPaladin()
        {
            int positionXBouton = RectangleDestination.Width / (NB_ZONES_DIALOGUE + 1);

            Vector2 PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 2) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnClarit� = new BoutonDeCommande(Game, "Clarit�", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnClarit�.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 1) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnBouclierDivin = new BoutonDeCommande(Game, "Bouclier divin", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnBouclierDivin.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            Game.Components.Add(BtnClarit�);
            Game.Components.Add(BtnBouclierDivin);

            VoirBoutonsPaladin(false);
        }

        public void VoirBoutonsPaladin(bool v)
        {
            BtnClarit�.Enabled = v;
            BtnClarit�.Visible = v;
            BtnBouclierDivin.Enabled = v;
            BtnBouclierDivin.Visible = v;
            VoirBoutonRetour(v);
        }
        #endregion

        #region Voleur
        public void Cr�erBtnVoleur()
        {
            int positionXBouton = RectangleDestination.Width / (NB_ZONES_DIALOGUE + 1);

            Vector2 PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 2) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnInvisibilit� = new BoutonDeCommande(Game, "Invisibilit�", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnInvisibilit�.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            PositionBouton = new Vector2(RectangleDestination.X + (NB_ZONES_DIALOGUE - 1) * positionXBouton, RectangleDestination.Y + RectangleDestination.Height / 2f);
            BtnLancerCouteau = new BoutonDeCommande(Game, "Lancer du couteau", "Arial20", "BoutonRouge", "BoutonBleu", PositionBouton, true, Attaquer, INTERVALLE_MAJ_STANDARD);
            BtnLancerCouteau.DrawOrder = (int)OrdreDraw.AVANT_PLAN;

            Game.Components.Add(BtnInvisibilit�);
            Game.Components.Add(BtnLancerCouteau);

            VoirBoutonsVoleur(false);
        }

        public void VoirBoutonsVoleur(bool v)
        {
            BtnInvisibilit�.Enabled = v;
            BtnInvisibilit�.Visible = v;
            BtnLancerCouteau.Enabled = v;
            BtnLancerCouteau.Visible = v;
            VoirBoutonRetour(v);
        }
        #endregion

        private void Attaquer()
        {
            �tatAttaquer = true;
        }

        private void Sorts()
        {
            �tatSorts = true;
            switch (PersonnageActif.GetType().ToString())
            {
                case TypePersonnage.ARCHER:
                    VoirBoutonsArcher(true);
                    break;
                case TypePersonnage.GU�RISSEUR:
                    VoirBoutonsGu�risseur(true);
                    break;
                case TypePersonnage.GUERRIER:
                    VoirBoutonsGuerrier(true);
                    break;
                case TypePersonnage.MAGE:
                    VoirBoutonsMage(true);
                    break;
                case TypePersonnage.PALADIN:
                    VoirBoutonsPaladin(true);
                    break;
                case TypePersonnage.VOLEUR:
                    VoirBoutonsVoleur(true);
                    break;
            }
            VoirBoutonAction(false);

        }

        private void PasserTour()
        {
            �tatPasserTour = true;
        }

        private void Retour()
        {
            �tatSorts = false;
            switch (PersonnageActif.GetType().ToString())
            {
                case TypePersonnage.ARCHER:
                    VoirBoutonsArcher(false);
                    break;
                case TypePersonnage.GU�RISSEUR:
                    VoirBoutonsGu�risseur(false);
                    break;
                case TypePersonnage.GUERRIER:
                    VoirBoutonsGuerrier(false);
                    break;
                case TypePersonnage.MAGE:
                    VoirBoutonsMage(false);
                    break;
                case TypePersonnage.PALADIN:
                    VoirBoutonsPaladin(false);
                    break;
                case TypePersonnage.VOLEUR:
                    VoirBoutonsVoleur(false);
                    break;
            }
            VoirBoutonAction(true);
        }

        public void VoirBoutonAction(bool x)
        {
            MenuActionVisible = x;
            BtnSorts.Enabled = x;
            BtnSorts.Visible = x;
            BtnAttaquer.Enabled = x;
            BtnAttaquer.Visible = x;
            BtnPasserTour.Enabled = x;
            BtnPasserTour.Visible = x;
            �tatAttaquer = false;
            �tatPasserTour = false;
        }

    }
}
