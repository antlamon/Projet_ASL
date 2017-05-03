using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projet_ASL
{
    public class ClasseQueTonyVeutSupprimer : Microsoft.Xna.Framework.DrawableGameComponent
    {
        List<string> ListeNomsEffets = new List<string> { "Enfeu", "EstMort", "Frozen", "Invisible", "SatanMode"};

        const string FEU = "Enfeu";
        const string MORT = "EstMort";
        const string FROID = "Frozen";
        const string INVISIBLE = "Invisible";
        const string SATAN = "SatanMode";

        const int NB_CASES = 90;

        const int NB_EFFETS_NORMAL = 4;
        const int NB_EFFETS_SPÉCIAL = 5;
         
        string NomImage { get; set; }
        public Vector2 Position { get; protected set; } 
        protected Texture2D Image { get; set; } 
        SpriteBatch GestionSprites { get; set; } 
        protected RessourcesManager<Texture2D> GestionnaireDeTextures { get; private set; }
        protected float Échelle { get; set; }
        protected Rectangle RectangleSource { get; set; }
        public Personnage PersonnageÀIdentifier { get; private set; }

        protected Rectangle ZoneAffichageIcôneEffet { get; private set; }

        private int NbEffets { get; set; }
        private int Multiplicateur { get; set; }


        public ClasseQueTonyVeutSupprimer(Game jeu, Personnage personnage, string nomImage, int multiplicateur)
           : base(jeu)
        {
            PersonnageÀIdentifier = personnage;
            Multiplicateur = multiplicateur;
            if (personnage.GetType().ToString() == TypePersonnage.GUÉRISSEUR || personnage.GetType().ToString() == TypePersonnage.GUERRIER)
            {
                NbEffets = NB_EFFETS_SPÉCIAL;
            }
            else
            {
                NbEffets = NB_EFFETS_NORMAL;
            }
            NomImage = nomImage;
            DéterminerPositionGénérale();
            ZoneAffichageIcôneEffet = new Rectangle(0, 0, Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height / 40);

        }

        private void DéterminerPositionGénérale()
        {
            if (PersonnageÀIdentifier.Position.X < 0)
            {
                if (PersonnageÀIdentifier.Position.Z < 0)
                {
                    if (PersonnageÀIdentifier.Position.Z < -10)
                    {
                        Position = new Vector2(100*Multiplicateur + ( Game.Window.ClientBounds.Width / NB_CASES), Game.Window.ClientBounds.Height / 15);
                    }
                    else
                    {
                        Position = new Vector2(100* Multiplicateur + (3 * Game.Window.ClientBounds.Width / NB_CASES), Game.Window.ClientBounds.Height / 15);
                    }
                }
                else
                {
                    if (PersonnageÀIdentifier.Position.Z < 10)
                    {
                        Position = new Vector2(100*Multiplicateur + (5 * Game.Window.ClientBounds.Width / NB_CASES), Game.Window.ClientBounds.Height / 15);
                    }
                    else
                    {
                        Position = new Vector2(100*Multiplicateur + (7 * Game.Window.ClientBounds.Width / NB_CASES), Game.Window.ClientBounds.Height / 15);
                    }
                }
            }
            else
            {
                if (PersonnageÀIdentifier.Position.Z < 0)
                {
                    if (PersonnageÀIdentifier.Position.Z < -10)
                    {
                        Position = new Vector2(100* Multiplicateur + (12 * Game.Window.ClientBounds.Width / NB_CASES), Game.Window.ClientBounds.Height / 15);
                    }
                    else
                    {
                        Position = new Vector2(100*Multiplicateur + (14 * Game.Window.ClientBounds.Width / NB_CASES), Game.Window.ClientBounds.Height / 15);
                    }
                }
                else
                {
                    if (PersonnageÀIdentifier.Position.Z < 10)
                    {
                        Position = new Vector2(100*Multiplicateur + (16 * Game.Window.ClientBounds.Width / NB_CASES), Game.Window.ClientBounds.Height / 15);
                    }
                    else
                    {
                        Position = new Vector2(100* Multiplicateur + (18 * Game.Window.ClientBounds.Width / NB_CASES), Game.Window.ClientBounds.Height / 15);
                    }
                }
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
            Échelle = MathHelper.Min((float)ZoneAffichageIcôneEffet.Width / (float)Image.Width, (float)ZoneAffichageIcôneEffet.Height / (float)Image.Height);
        }
    }
}