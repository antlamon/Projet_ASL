using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Projet_ASL
{
    public class ObjetDeBase : Microsoft.Xna.Framework.DrawableGameComponent
    {
        string NomModèle { get; set; }
        RessourcesManager<Model> GestionnaireDeModèles { get; set; }
        Caméra CaméraJeu { get; set; }
        public float Échelle { get; protected set; }
        public Vector3 Rotation { get; protected set; }
        public Vector3 Position { get; protected set; }

        protected Model Modèle { get; private set; }
        protected Matrix[] TransformationsModèle { get; private set; }
        protected Matrix Monde { get; set; }

        public ObjetDeBase(Game jeu, String nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale)
           : base(jeu)
        {
            NomModèle = nomModèle;
            Position = positionInitiale;
            Échelle = échelleInitiale;
            Rotation = rotationInitiale;
        }

        public override void Initialize()
        {
            Monde = Matrix.Identity;
            Monde *= Matrix.CreateScale(Échelle);
            Monde *= Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            Monde *= Matrix.CreateTranslation(Position);
            base.Initialize();
        }
        protected override void LoadContent()
        {
            CaméraJeu = Game.Services.GetService(typeof(Caméra)) as Caméra;
            GestionnaireDeModèles = Game.Services.GetService(typeof(RessourcesManager<Model>)) as RessourcesManager<Model>;
            Modèle = GestionnaireDeModèles.Find(NomModèle);
            TransformationsModèle = new Matrix[Modèle.Bones.Count];
            Modèle.CopyAbsoluteBoneTransformsTo(TransformationsModèle);
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (ModelMesh maille in Modèle.Meshes)
            {
                Matrix mondeLocal = TransformationsModèle[maille.ParentBone.Index] * GetMonde();
                foreach (BasicEffect effect in maille.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = CaméraJeu.Projection;
                    effect.View = CaméraJeu.Vue;
                    effect.World = mondeLocal;
                }
                maille.Draw();
            }
            base.Draw(gameTime);
        }

        public virtual Matrix GetMonde()
        {
            return Monde;
        }
    }
}
