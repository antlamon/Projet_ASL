using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projet_ASL
{
    class IdentificateurPersonnage : Microsoft.Xna.Framework.DrawableGameComponent
    {
        const int NB_CASES = 20;
        string NomImage { get; set; }
        public Vector2 Position { get; protected set; }        // En prévision d'une spécialisation vers un sprite dynamique
        protected Texture2D Image { get; set; } // En prévision d'une spécialisation vers un sprite animé
        SpriteBatch GestionSprites { get; set; } // En prévision d'une spécialisation vers un sprite animé
        protected RessourcesManager<Texture2D> GestionnaireDeTextures { get; private set; }
        protected Rectangle ZoneAffichage { get; private set; }
        protected float Échelle { get; set; }
        protected Rectangle RectangleSource { get; set; }
        private Personnage PersonnageÀIdentifier { get; set; }
        private int PtsViePersonnage { get; set; }
        private int PtsVieMax { get; set; }
        private TexteCentré AfficheurPtsVie { get; set; }


        public IdentificateurPersonnage(Game jeu, Personnage personnage)
           : base(jeu)
        {
            PersonnageÀIdentifier = personnage;
            PtsVieMax = PersonnageÀIdentifier.PtsDeVie;
            PtsViePersonnage = PersonnageÀIdentifier.PtsDeVie;
            DéterminerNomImage();
            DéterminerPosition();
            ZoneAffichage = new Rectangle(0,0,Game.Window.ClientBounds.Width,Game.Window.ClientBounds.Height/15);
            AfficheurPtsVie = new TexteCentré(Game, ":" + PtsViePersonnage.ToString() + "/" + PtsVieMax.ToString(), "Arial20",
                new Rectangle((int)Position.X + 2 * Game.Window.ClientBounds.Width / (3 * NB_CASES), (int)Position.Y, Game.Window.ClientBounds.Width / NB_CASES, Game.Window.ClientBounds.Height / 15),
                Color.Red, 0);
            AfficheurPtsVie.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            Game.Components.Add(AfficheurPtsVie);
        }

        private void DéterminerPosition()
        {
            if(PersonnageÀIdentifier.Position.X < 0)
            {
                if(PersonnageÀIdentifier.Position.Z < 0)
                {
                    if(PersonnageÀIdentifier.Position.Z < -10)
                    {
                        Position = new Vector2(Game.Window.ClientBounds.Width/NB_CASES, 0);
                    }
                    else
                    {
                        Position = new Vector2(3*Game.Window.ClientBounds.Width / NB_CASES, 0);
                    }
                }
                else
                {
                    if (PersonnageÀIdentifier.Position.Z < 10)
                    {
                        Position = new Vector2(5*Game.Window.ClientBounds.Width / NB_CASES, 0);
                    }
                    else
                    {
                        Position = new Vector2(7 * Game.Window.ClientBounds.Width / NB_CASES, 0);
                    }
                }
            }
            else
            {
                if (PersonnageÀIdentifier.Position.Z < 0)
                {
                    if (PersonnageÀIdentifier.Position.Z < -10)
                    {
                        Position = new Vector2(12 * Game.Window.ClientBounds.Width / NB_CASES, 0);
                    }
                    else
                    {
                        Position = new Vector2(14 * Game.Window.ClientBounds.Width / NB_CASES, 0);
                    }
                }
                else
                {
                    if (PersonnageÀIdentifier.Position.Z < 10)
                    {
                        Position = new Vector2(16 * Game.Window.ClientBounds.Width / NB_CASES, 0);
                    }
                    else
                    {
                        Position = new Vector2(18 * Game.Window.ClientBounds.Width / NB_CASES, 0);
                    }
                }
            }
        }

        private void DéterminerNomImage()
        {
            switch (PersonnageÀIdentifier.GetType().ToString())
            {
                case TypePersonnage.ARCHER:
                    NomImage = "Copper_Bow";
                    break;
                case TypePersonnage.GUERRIER:
                    NomImage = "Copper_Broadsword";
                    break;
                case TypePersonnage.GUÉRISSEUR:
                    NomImage = "Lightning_Aura_Cane";
                    break;
                case TypePersonnage.MAGE:
                    NomImage = "Amethyst_Staff";
                    break;
                case TypePersonnage.PALADIN:
                    NomImage = "Paladin's_Hammer";
                    break;
                case TypePersonnage.VOLEUR:
                    NomImage = "Throwing_Knife";
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if(PtsViePersonnage != PersonnageÀIdentifier.PtsDeVie)
            {
                PtsViePersonnage = PersonnageÀIdentifier.PtsDeVie;
                AfficheurPtsVie.ModifierTexte(":" + PtsViePersonnage.ToString() + "/" + PtsVieMax.ToString());
            }
        }

        protected override void LoadContent()
        {
            GestionSprites = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
            GestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            Image = GestionnaireDeTextures.Find(NomImage);
            RectangleSource = new Rectangle(0, 0, Image.Width, Image.Height);
            CalculerÉchelle();
        }

        public override void Draw(GameTime gameTime)
        {
            GestionSprites.Begin();
            GestionSprites.Draw(Image, Position, RectangleSource, Color.White, 0, Vector2.Zero, Échelle, SpriteEffects.None, 0);
            GestionSprites.End();
        }

        protected virtual void CalculerÉchelle()
        {
            Échelle = MathHelper.Min((float)ZoneAffichage.Width / (float)Image.Width, (float)ZoneAffichage.Height / (float)Image.Height);
        }
    }
}
