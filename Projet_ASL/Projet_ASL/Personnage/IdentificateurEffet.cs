using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projet_ASL
{
    public class IdentificateurEffet : Microsoft.Xna.Framework.DrawableGameComponent
    {
        const int NB_CASES = 90;

        const int NB_EFFETS_NORMAL = 4;
        const int NB_EFFETS_SP�CIAL = 5;
         
        public string NomImage { get; private set; }
        public Vector2 Position { get; protected set; } 
        protected Texture2D Image { get; set; } 
        SpriteBatch GestionSprites { get; set; } 
        protected RessourcesManager<Texture2D> GestionnaireDeTextures { get; private set; }
        protected float �chelle { get; set; }
        protected Rectangle RectangleSource { get; set; }
        public Personnage Personnage�Identifier { get; private set; }

        protected Rectangle ZoneAffichageIc�neEffet { get; private set; }

        private int NbEffets { get; set; }
        private int Multiplicateur { get; set; }
        Rectangle DimensionFen�tre { get; set; }


        public IdentificateurEffet(Game jeu, Personnage personnage, string nomImage, Vector2 positionIdentificateur, int multiplicateur)
           : base(jeu)
        {
            Personnage�Identifier = personnage;
            Multiplicateur = multiplicateur;
            NbEffets = NB_EFFETS_SP�CIAL;
            NomImage = nomImage;
            D�terminerPositionG�n�rale(positionIdentificateur.X);
            ZoneAffichageIc�neEffet = new Rectangle(0, 0, Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height / 40);

        }

        private void D�terminerPositionG�n�rale(float positionXIdentificateur)
        {
            Position = new Vector2(positionXIdentificateur+Multiplicateur*25, Game.Window.ClientBounds.Height / 15);
        }

        protected override void LoadContent()
        {
            GestionSprites = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
            GestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            Image = GestionnaireDeTextures.Find(NomImage);
            RectangleSource = new Rectangle(0, 0, Image.Width, Image.Height);
            DimensionFen�tre = Game.Window.ClientBounds;
            Calculer�chelle();
        }

        public override void Update(GameTime gameTime)
        {
            if (DimensionFen�tre != Game.Window.ClientBounds)
            {
                Position = new Vector2(Position.X * Game.Window.ClientBounds.Width / DimensionFen�tre.Width, Position.Y * Game.Window.ClientBounds.Height / DimensionFen�tre.Height);
                ZoneAffichageIc�neEffet = new Rectangle(0, 0, Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height / 40);
                Calculer�chelle();
                DimensionFen�tre = Game.Window.ClientBounds;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            GestionSprites.Begin();
            GestionSprites.Draw(Image, Position, RectangleSource, Color.White, 0, Vector2.Zero, �chelle, SpriteEffects.None, 0);
            GestionSprites.End();
        }

        protected virtual void Calculer�chelle()
        {
            �chelle = MathHelper.Min((float)ZoneAffichageIc�neEffet.Width / (float)Image.Width, (float)ZoneAffichageIc�neEffet.Height / (float)Image.Height);
        }
    }
}