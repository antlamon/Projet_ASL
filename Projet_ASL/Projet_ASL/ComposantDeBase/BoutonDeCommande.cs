using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Projet_ASL
{
    public delegate void FonctionÉvénementielle();

    public class BoutonDeCommande : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Color COULEUR_PAR_DÉFAUT = Color.Black;
        Color COULEUR_FOCUS = Color.White;
        Color COULEUR_INACTIF = Color.Gray;
        string Texte { get; set; }
        string NomFont { get; set; }
        string NomImageNormale { get; set; }
        string NomImageEnfoncée { get; set; }
        Vector2 Position { get; set; }
        Vector2 PositionChaîne { get; set; }
        Vector2 OrigineChaîne { get; set; }
        SpriteFont PoliceDeCaractères { get; set; }
        Texture2D ImageNormale { get; set; }
        Texture2D ImageEnfoncée { get; set; }
        Texture2D ImageBouton { get; set; }
        Rectangle RectangleDestination { get; set; }
        Color CouleurTexte { get; set; }
        SpriteBatch GestionSprites { get; set; }
        InputManager GestionInput { get; set; }
        float IntervalleMAJ { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }
        Rectangle DimensionFenêtre { get; set; }
        Vector2 Dimension { get; set; }
        Vector2 DimensionChaîne { get; set; }

        FonctionÉvénementielle OnClick { get; set; }

        bool estActif;

        public bool EstActif
        {
            get { return estActif; }
            set
            {
                estActif = value;
                CouleurTexte = estActif ? COULEUR_PAR_DÉFAUT : COULEUR_INACTIF;
            }
        }

        public BoutonDeCommande(Game jeu, string texte, string nomFont,
                                string nomImageNormale, string nomImageEnfoncée,
                                Vector2 position, bool estActif,
                                FonctionÉvénementielle onClick, float intervalleMAJ)
           : base(jeu)
        {
            Texte = texte;
            NomFont = nomFont;
            NomImageNormale = nomImageNormale;
            NomImageEnfoncée = nomImageEnfoncée;
            Position = position;
            EstActif = estActif;
            OnClick = onClick;
            IntervalleMAJ = intervalleMAJ;
        }

        public void ChangerCouleurActive()
        {
            Texture2D tampon = ImageNormale;
            ImageNormale = ImageEnfoncée;
            ImageEnfoncée = tampon;
        }

        public override void Initialize()
        {
            TempsÉcouléDepuisMAJ = 0;
            DimensionFenêtre = Game.Window.ClientBounds;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            GestionSprites = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;

            PoliceDeCaractères = Game.Content.Load<SpriteFont>("Fonts/" + NomFont);
            ImageNormale = Game.Content.Load<Texture2D>("Textures/" + NomImageNormale);
            ImageEnfoncée = Game.Content.Load<Texture2D>("Textures/" + NomImageEnfoncée);

            ImageBouton = ImageNormale;

            DimensionChaîne = PoliceDeCaractères.MeasureString(Texte);
            Dimension = DimensionChaîne * 1.10f;
            Position = Position - Dimension / 2;
            PositionChaîne = new Vector2(Position.X + Dimension.X / 2, Position.Y + Dimension.Y / 2);
            OrigineChaîne = new Vector2(DimensionChaîne.X / 2, DimensionChaîne.Y / 2);
            RectangleDestination = new Rectangle((int)Position.X, (int)Position.Y, (int)Dimension.X, (int)Dimension.Y);
        }

        private void DéfinirPositionChaîne()
        {
            PositionChaîne = new Vector2(Position.X + Dimension.X / 2, Position.Y + Dimension.Y / 2);
            OrigineChaîne = new Vector2(DimensionChaîne.X / 2, DimensionChaîne.Y / 2);
            RectangleDestination = new Rectangle((int)Position.X, (int)Position.Y, (int)Dimension.X, (int)Dimension.Y);
        }

        public override void Update(GameTime gameTime)
        {
            if (EstActif)
            {
                Point positionSouris = GestionInput.GetPositionSouris();
                if (RectangleDestination.Contains(positionSouris))
                {
                    CouleurTexte = COULEUR_FOCUS;
                    if (GestionInput.EstNouveauClicGauche())
                    {
                        OnClick();
                        ImageBouton = ImageEnfoncée;
                    }
                    else
                    {
                        ImageBouton = ImageNormale;
                        //if (!GestionInput.EstAncienClicGauche())
                        //{
                        //    if (GestionInput.EstAncienClicDroit())
                        //    {
                        //        MiseÀJour(gameTime);
                        //        ImageBouton = ImageEnfoncée;
                        //    }
                        //    else
                        //    {
                        //        ImageBouton = ImageNormale;
                        //    }
                        //}
                    }
                }
                else
                {
                    CouleurTexte = COULEUR_PAR_DÉFAUT;
                    ImageBouton = ImageNormale;
                }
                if(DimensionFenêtre != Game.Window.ClientBounds)
                {
                    Position = new Vector2(Position.X * Game.Window.ClientBounds.Width / DimensionFenêtre.Width, Position.Y * Game.Window.ClientBounds.Height / DimensionFenêtre.Height);
                    DéfinirPositionChaîne();
                    DimensionFenêtre = Game.Window.ClientBounds;
                }
            }
        }

        void MiseÀJour(GameTime gameTime)
        {
            float tempsÉcoulé = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TempsÉcouléDepuisMAJ += tempsÉcoulé;
            if (TempsÉcouléDepuisMAJ >= IntervalleMAJ)
            {
                OnClick();
                TempsÉcouléDepuisMAJ = 0;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            GestionSprites.Begin();
            GestionSprites.Draw(ImageBouton, RectangleDestination, Color.White);
            GestionSprites.DrawString(PoliceDeCaractères, Texte, PositionChaîne, CouleurTexte, 0, OrigineChaîne, 1f, SpriteEffects.None, 1);
            GestionSprites.End();
        }

    }
}
