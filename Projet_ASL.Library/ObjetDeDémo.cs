using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Projet_ASL
{
    public class ObjetDeDémo : ObjetDeBase
    {
        const float INCRÉMENTATION_ROTATION = MathHelper.Pi / 120f;
        const float INCRÉMENTATION_ÉCHELLE = 0.00001f;
        const float ÉCHELLE_MAX = 1f;
        const float ÉCHELLE_MIN = 0.005f;

        float IntervalleMAJ { get; set; }
        float TempsÉcouléDepuisMAJ { get; set; }
        InputManager GestionInput { get; set; }
        float ÉchelleDémo { get; set; }
        float AncienneÉchelle { get; set; }
        Vector3 RotationDémo { get; set; }
        bool RotationX { get; set; }
        bool RotationY { get; set; }
        bool RotationZ { get; set; }
        Matrix MondeRefait { get; set; }
        Vector3 VectorX { get; set; }
        Vector3 VectorY { get; set; }
        Vector3 VectorZ { get; set; }


        public ObjetDeDémo(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, float intervalleMAJ)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale)
        {
            IntervalleMAJ = intervalleMAJ;
        }

        public override void Initialize()
        {
            base.Initialize();
            MondeRefait = Monde;
            RotationDémo = Rotation;
            ÉchelleDémo = Échelle;
            VectorX = new Vector3(INCRÉMENTATION_ROTATION, 0, 0);
            VectorY = new Vector3(0, INCRÉMENTATION_ROTATION, 0);
            VectorZ = new Vector3(0, 0, INCRÉMENTATION_ROTATION);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            GestionInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
        }

        public override void Update(GameTime gameTime)
        {
            float tempsÉcoulé = (float)gameTime.ElapsedGameTime.TotalSeconds;
            TempsÉcouléDepuisMAJ += tempsÉcoulé;
            AncienneÉchelle = ÉchelleDémo;
            GérerClavier();

            if (TempsÉcouléDepuisMAJ >= IntervalleMAJ)
            {
                TempsÉcouléDepuisMAJ = 0;
                GérerRotationDémo();
                GérerPositionObjet();
                SphèreDeCollision = new BoundingSphere(new Vector3(Position.X,0,Position.Z), 1);
            }
        }

        private void GérerClavier()
        {
            RotationY = GestionInput.EstNouvelleTouche(Keys.NumPad1) ? !RotationY : RotationY;
            RotationX = GestionInput.EstNouvelleTouche(Keys.NumPad2) ? !RotationX : RotationX;
            RotationZ = GestionInput.EstNouvelleTouche(Keys.NumPad3) ? !RotationZ : RotationZ;

            ÉchelleDémo = GestionInput.EstEnfoncée(Keys.Add) ? MathHelper.Min(ÉchelleDémo + INCRÉMENTATION_ÉCHELLE, ÉCHELLE_MAX) : ÉchelleDémo;
            ÉchelleDémo = GestionInput.EstEnfoncée(Keys.Subtract) ? MathHelper.Max(ÉchelleDémo - INCRÉMENTATION_ÉCHELLE, ÉCHELLE_MIN) : ÉchelleDémo;

            if (GestionInput.EstNouvelleTouche(Keys.Space))
            {
                RéinitialiserObjet();
            }
        }

        void RéinitialiserObjet()
        {
            ÉchelleDémo = Échelle;
            RotationDémo = Rotation;
            MondeRefait = Monde;
        }

        void GérerRotationDémo()
        {
            RotationDémo += RotationX ? VectorX : Vector3.Zero;
            RotationDémo += RotationY ? VectorY : Vector3.Zero;
            RotationDémo += RotationZ ? VectorZ  : Vector3.Zero;
        }

        public override Matrix GetMonde()
        {
            if (RotationX || RotationY || RotationZ || AncienneÉchelle != ÉchelleDémo)
            {
                MondeRefait = Matrix.Identity;
                MondeRefait *= Matrix.CreateScale(ÉchelleDémo);
                MondeRefait *= Matrix.CreateFromYawPitchRoll(RotationDémo.Y, RotationDémo.X, RotationDémo.Z);
                MondeRefait *= Matrix.CreateTranslation(Position);
            }
            return MondeRefait;
        }
    }
}
