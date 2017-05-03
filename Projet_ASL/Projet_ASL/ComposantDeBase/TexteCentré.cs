//
// Auteur : Vincent Echelard
// Date : Création - Septembre 2014
//        Modification - Novembre 2016
//
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Projet_ASL
{
    public class TexteCentré : Microsoft.Xna.Framework.DrawableGameComponent
    {
        float PourcentageZoneAffichable;
        string TexteÀAfficher { get; set; }
        string NomFont { get; set; }
        Rectangle ZoneAffichage { get; set; }
        Vector2 PositionAffichage { get; set; }
        Color CouleurTexte { get; set; }
        Vector2 Origine { get; set; }
        float Échelle { get; set; }
        SpriteFont PoliceDeCaractères { get; set; }
        SpriteBatch GestionSprites { get; set; }
        RessourcesManager<SpriteFont> GestionnaireDeFonts { get; set; }
        Rectangle DimensionFenêtre { get; set; }

        public TexteCentré(Game jeu, string texteÀAfficher, string nomFont, Rectangle zoneAffichage,
                           Color couleurTexte, float marge)
           : base(jeu)
        {
            TexteÀAfficher = texteÀAfficher;
            NomFont = nomFont;
            CouleurTexte = couleurTexte;
            ZoneAffichage = zoneAffichage;
            PourcentageZoneAffichable = 1f - marge;
            PositionAffichage = new Vector2(zoneAffichage.X + zoneAffichage.Width / 2,
                                            zoneAffichage.Y + zoneAffichage.Height / 2);
        }

        protected override void LoadContent()
        {
            GestionSprites = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
            GestionnaireDeFonts = Game.Services.GetService(typeof(RessourcesManager<SpriteFont>)) as RessourcesManager<SpriteFont>;
            PoliceDeCaractères = GestionnaireDeFonts.Find(NomFont);
            ModifierTexte(TexteÀAfficher);
            DimensionFenêtre = Game.Window.ClientBounds;
        }

        public void ModifierTexte(string texteÀAfficher)
        {
            TexteÀAfficher = texteÀAfficher;
            Vector2 dimensionTexte = PoliceDeCaractères.MeasureString(TexteÀAfficher);
            float échelleHorizontale = MathHelper.Max(MathHelper.Min(ZoneAffichage.Width * PourcentageZoneAffichable, dimensionTexte.X), ZoneAffichage.Width * PourcentageZoneAffichable) / dimensionTexte.X;
            float échelleVerticale = MathHelper.Max(MathHelper.Min(ZoneAffichage.Height * PourcentageZoneAffichable, dimensionTexte.Y), ZoneAffichage.Height * PourcentageZoneAffichable) / dimensionTexte.Y;
            Échelle = MathHelper.Min(échelleHorizontale, échelleVerticale);
            Origine = dimensionTexte / 2;
        }

        public override void Update(GameTime gameTime)
        {
            if (DimensionFenêtre != Game.Window.ClientBounds)
            {
                PositionAffichage = new Vector2(PositionAffichage.X * Game.Window.ClientBounds.X / DimensionFenêtre.X, PositionAffichage.Y * Game.Window.ClientBounds.Y / DimensionFenêtre.Y);
                DimensionFenêtre = Game.Window.ClientBounds;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            GestionSprites.Begin();
            GestionSprites.DrawString(PoliceDeCaractères, TexteÀAfficher, PositionAffichage, CouleurTexte, 0, Origine, Échelle, SpriteEffects.None, 0);
            GestionSprites.End();
        }
    }
}