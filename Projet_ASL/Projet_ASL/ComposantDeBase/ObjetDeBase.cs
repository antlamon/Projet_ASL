using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Projet_ASL
{
    public class ObjetDeBase : Microsoft.Xna.Framework.DrawableGameComponent, ICollisionable
    {
        public BoundingSphere Sph�reDeCollision { get; set; }
        string NomMod�le { get; set; }
        RessourcesManager<Model> GestionnaireDeMod�les { get; set; }
        Cam�ra Cam�raJeu { get; set; }
        public float �chelle { get; protected set; }
        public Vector3 Rotation { get; protected set; }
        public Vector3 Position { get; protected set; }

        protected Model Mod�le { get; private set; }
        protected Matrix[] TransformationsMod�le { get; private set; }
        protected Matrix Monde { get; set; }

        public ObjetDeBase(Game jeu, String nomMod�le, float �chelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale)
           : base(jeu)
        {
            NomMod�le = nomMod�le;
            Position = positionInitiale;
            �chelle = �chelleInitiale;
            Rotation = rotationInitiale;
        }

        public override void Initialize()
        {
            Cr�erSph�reDeCollision();
            base.Initialize();
            Monde = Matrix.Identity;
            Monde *= Matrix.CreateScale(�chelle);
            Monde *= Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            Monde *= Matrix.CreateTranslation(Position);
        }

        void Cr�erSph�reDeCollision()
        {
            Sph�reDeCollision = new BoundingSphere(new Vector3(Position.X,0,Position.Z), 1);
        }

        public bool EstEnCollision(object objet)
        {
            ObjetDeD�mo personnage = objet as ObjetDeD�mo;
            return Sph�reDeCollision.Intersects(personnage.Sph�reDeCollision);
        }

        protected override void LoadContent()
        {
            Cam�raJeu = Game.Services.GetService(typeof(Cam�ra)) as Cam�ra;
            GestionnaireDeMod�les = Game.Services.GetService(typeof(RessourcesManager<Model>)) as RessourcesManager<Model>;
            Mod�le = GestionnaireDeMod�les.Find(NomMod�le);
            TransformationsMod�le = new Matrix[Mod�le.Bones.Count];
            Mod�le.CopyAbsoluteBoneTransformsTo(TransformationsMod�le);
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (ModelMesh maille in Mod�le.Meshes)
            {
                Matrix mondeLocal = TransformationsMod�le[maille.ParentBone.Index] * GetMonde();
                foreach (ModelMeshPart portionDeMaillage in maille.MeshParts)
                {
                    BasicEffect effet = (BasicEffect)portionDeMaillage.Effect;
                    effet.EnableDefaultLighting();
                    effet.Projection = Cam�raJeu.Projection;
                    effet.View = Cam�raJeu.Vue;
                    effet.World = mondeLocal;
                }
                maille.Draw();
            }
        }

        public virtual Matrix GetMonde()
        {
            return Monde;
        }
    }
}