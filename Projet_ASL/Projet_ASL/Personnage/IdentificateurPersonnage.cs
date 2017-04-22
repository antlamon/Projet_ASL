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
        string NomImage { get; set; }
        public Vector2 Position { get; protected set; }        // En prévision d'une spécialisation vers un sprite dynamique
        protected Texture2D Image { get; set; } // En prévision d'une spécialisation vers un sprite animé
        SpriteBatch GestionSprites { get; set; } // En prévision d'une spécialisation vers un sprite animé
        protected RessourcesManager<Texture2D> GestionnaireDeTextures { get; private set; }
        protected Rectangle ZoneAffichage { get; private set; }
        protected float Échelle { get; set; }
        protected Rectangle RectangleSource { get; set; }
        private Personnage PersonnageÀIdentifier { get; set; }


        public IdentificateurPersonnage(Game jeu, Personnage personnage)
           : base(jeu)
        {
            PersonnageÀIdentifier = personnage;
            DéterminerNomImage();
            DéterminerPosition();
            ZoneAffichage = new Rectangle(0,0,Game.Window.ClientBounds.Width,Game.Window.ClientBounds.Height/15);

        }

        private void DéterminerPosition()
        {
            if(PersonnageÀIdentifier.Position.X < 0)
            {
                if(PersonnageÀIdentifier.Position.Z < 0)
                {
                    if(PersonnageÀIdentifier.Position.Z < -10)
                    {
                        Position = new Vector2(Game.Window.ClientBounds.Width/11, 0);
                    }
                    else
                    {
                        Position = new Vector2(2*Game.Window.ClientBounds.Width / 11, 0);
                    }
                }
                else
                {
                    if (PersonnageÀIdentifier.Position.Z < 10)
                    {
                        Position = new Vector2(3*Game.Window.ClientBounds.Width / 11, 0);
                    }
                    else
                    {
                        Position = new Vector2(4 * Game.Window.ClientBounds.Width / 11, 0);
                    }
                }
            }
            else
            {
                if (PersonnageÀIdentifier.Position.Z < 0)
                {
                    if (PersonnageÀIdentifier.Position.Z < -10)
                    {
                        Position = new Vector2(6 * Game.Window.ClientBounds.Width / 11, 0);
                    }
                    else
                    {
                        Position = new Vector2(7 * Game.Window.ClientBounds.Width / 11, 0);
                    }
                }
                else
                {
                    if (PersonnageÀIdentifier.Position.Z < 10)
                    {
                        Position = new Vector2(8 * Game.Window.ClientBounds.Width / 11, 0);
                    }
                    else
                    {
                        Position = new Vector2(9 * Game.Window.ClientBounds.Width / 11, 0);
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
