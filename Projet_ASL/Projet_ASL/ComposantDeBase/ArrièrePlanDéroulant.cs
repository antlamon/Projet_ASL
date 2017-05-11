using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Projet_ASL
{
    public class ArriËrePlanDÈroulant : ArriËrePlan
    {
        const float D…PLACEMENT_HORIZONTAL = 0.2f;
        float IntervalMAJ { get; set; }
        float Temps…coulÈDepuisMAJ { get; set; }
        protected float …chelle { get; private set; }
        protected Vector2 Position…cran { get; set; }
        protected Vector2 TailleImage { get; set; }
        protected Rectangle Taille…cran { get; set; }

        public ArriËrePlanDÈroulant(Game jeu, string nomImage, float intervalMAJ)
           : base(jeu, nomImage)
        {
            IntervalMAJ = intervalMAJ;
        }

        public override void Initialize()
        {
            Temps…coulÈDepuisMAJ = 0;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            Taille…cran = Game.Window.ClientBounds;
            Position…cran = new Vector2(Game.Window.ClientBounds.Width / 2, 0);
            DÈfinirTailleImage();
        }
        public override void Update(GameTime gameTime)
        {
            float Temps…coulÈ = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Temps…coulÈDepuisMAJ += Temps…coulÈ;
            if (Temps…coulÈDepuisMAJ >= IntervalMAJ)
            {
                EffectuerMise¿Jour();
                Temps…coulÈDepuisMAJ = 0;
            }
        }

        private void DÈfinirTailleImage()
        {
            …chelle = MathHelper.Max(Game.Window.ClientBounds.Width / (float)ImageDeFond.Width,
                                     Game.Window.ClientBounds.Height / (float)ImageDeFond.Height);
            TailleImage = new Vector2(ImageDeFond.Width * …chelle, 0);
        }

        protected virtual void EffectuerMise¿Jour()
        {
            if(Taille…cran != Game.Window.ClientBounds)
            {
                DÈfinirTailleImage();
                Taille…cran = Game.Window.ClientBounds;
            }
            Position…cran = new Vector2((Position…cran.X + D…PLACEMENT_HORIZONTAL) % TailleImage.X, 0);
        }

        protected override void Afficher(GameTime gameTime)
        {
            GestionSprites.Draw(ImageDeFond, Position…cran, null, Color.White, 0, Vector2.Zero, …chelle, SpriteEffects.None, ARRI»RE_PLAN);
            GestionSprites.Draw(ImageDeFond, Position…cran - TailleImage, null, Color.White, 0, Vector2.Zero, …chelle, SpriteEffects.None, ARRI»RE_PLAN);
        }
    }
}