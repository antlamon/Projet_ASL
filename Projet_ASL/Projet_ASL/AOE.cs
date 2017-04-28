using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    public class AOE : PrimitiveDeBaseAnimée
    {
        const int NB_TRIANGLES = 2;
        RessourcesManager<Texture2D> gestionnaireDeTextures;
        protected Vector3[,] PtsSommets { get; private set; }
        Vector3 Origine { get; set; }
        public Vector2 Delta { get; set; }
        protected BasicEffect EffetDeBase { get; private set; }
        Texture2D textureTuile;
        VertexPositionTexture[] Sommets { get; set; }
        Vector2[,] PtsTexture { get; set; }
        string NomTextureTuile { get; set; }
        BlendState GestionAlpha { get; set; }


        public AOE(Game jeu, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, Vector2 étendue, string nomTextureTuile,float intervalleMAJ)
         : base(jeu, homothétieInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            Delta = new Vector2(étendue.X, étendue.Y);
            Origine = new Vector3(-Delta.X / 2, 0, Delta.Y / 2); //pour centrer la primitive au point (0,0,0)
            NomTextureTuile = nomTextureTuile;
        }

        public override void Initialize()
        {
            NbSommets = NB_TRIANGLES + 2;
            PtsSommets = new Vector3[2, 2];
            CréerTableauSommets();
            base.Initialize();
        }

        protected void CréerTableauSommets()
        {
            PtsTexture = new Vector2[2, 2];
            Sommets = new VertexPositionTexture[NbSommets];
        }

        protected override void LoadContent()
        {
            gestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            textureTuile = gestionnaireDeTextures.Find(NomTextureTuile);
            EffetDeBase = new BasicEffect(GraphicsDevice);
            InitialiserParamètresEffetDeBase();
            base.LoadContent();
        }

        protected override void InitialiserSommets()
        {
            Sommets[0] = new VertexPositionTexture(new Vector3(Origine.X, Origine.Y, Origine.Z), new Vector2(0, 1));
            Sommets[1] = new VertexPositionTexture(new Vector3(Origine.X, Origine.Y, Origine.Z - Delta.Y), new Vector2(0, 0));
            Sommets[2] = new VertexPositionTexture(new Vector3(Origine.X + Delta.X, Origine.Y, Origine.Z), new Vector2(1, 1));
            Sommets[3] = new VertexPositionTexture(new Vector3(Origine.X + Delta.X, Origine.Y, Origine.Z - Delta.Y), new Vector2(1, 0));
        }

        public void ChangerÉtendueEtPosition(Vector2 étendue, Vector3 position)
        {
            Delta = new Vector2(étendue.X, étendue.Y);
            Origine = new Vector3(position.X - Delta.X / 2, 0, position.Z + Delta.Y / 2);
            InitialiserSommets();
            Position = position;
        }

        protected void InitialiserParamètresEffetDeBase()
        {
            EffetDeBase.TextureEnabled = true;
            EffetDeBase.Texture = textureTuile;
            GestionAlpha = BlendState.AlphaBlend;
        }

        public override void Draw(GameTime gameTime)
        {
            EffetDeBase.World = GetMonde();
            EffetDeBase.View = CaméraJeu.Vue;
            EffetDeBase.Projection = CaméraJeu.Projection;
            foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
            {
                passeEffet.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, Sommets, 0, NB_TRIANGLES);
            }
            base.Draw(gameTime);
        }
    }
}

