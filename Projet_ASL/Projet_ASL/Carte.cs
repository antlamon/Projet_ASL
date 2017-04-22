using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Projet_ASL
{
    public class Carte : PrimitiveDeBase
    {
        const int NB_TRIANGLES = 2;

        Vector3 Origine { get; set; }  //Le coin inf�rieur gauche du plan en tenant compte que la primitive est centr�e au point (0,0,0)
        Vector2 Delta { get; set; } // un vecteur contenant l'espacement entre deux colonnes (en X) et entre deux rang�es (en Y)
        Vector3[,] PtsSommets { get; set; }
        int NbColonnes { get; set; }
        int NbRang�es { get; set; }
        BasicEffect EffetDeBase { get; set; }
        VertexPositionTexture[] Sommets { get; set; }
        RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }
        Texture2D TextureCarte { get; set; }
        Vector2[] PtsTexture { get; set; }
        string NomTextureCarte { get; set; }
        BlendState GestionAlpha { get; set; }

        public Carte(Game jeu, float homoth�tieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, Vector2 �tendue, Vector2 charpente, string nomTexture, float intervalleMAJ)
           : base(jeu, homoth�tieInitiale, rotationInitiale, positionInitiale)
        {
            NbColonnes = (int)charpente.X;
            NbRang�es = (int)charpente.Y;
            Delta = new Vector2(�tendue.X / NbColonnes, �tendue.Y / NbRang�es);
            Origine = new Vector3(-�tendue.X / 2, 0, �tendue.Y / 2);
            NomTextureCarte = nomTexture;
        }

        public override void Initialize()
        {
            NbTriangles = NbRang�es * NbColonnes * NB_TRIANGLES;
            NbSommets = NbTriangles * 3;
            PtsSommets = new Vector3[NbColonnes + 1, NbRang�es + 1];
            Cr�erTableauSommets();
            Cr�erTableauxPoints();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            EffetDeBase = new BasicEffect(GraphicsDevice);
            GestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            TextureCarte = GestionnaireDeTextures.Find(NomTextureCarte);
            InitialiserParam�tresEffetDeBase();
            base.LoadContent();
        }


        void InitialiserParam�tresEffetDeBase()
        {
            EffetDeBase.TextureEnabled = true;
            EffetDeBase.Texture = TextureCarte;
            GestionAlpha = BlendState.AlphaBlend;
        }

        void Cr�erTableauSommets()
        {
            PtsTexture = new Vector2[4];
            Sommets = new VertexPositionTexture[NbSommets];
        }

        void Cr�erTableauxPoints()
        {
            for (int j = 0; j < PtsSommets.GetLength(1); ++j)
            {
                for (int i = 0; i < PtsSommets.GetLength(0); ++i)
                {
                    PtsSommets[i, j] = new Vector3(Origine.X + i * Delta.X, Origine.Y, Origine.Z - j * Delta.Y);
                }
            }

            PtsTexture[0] = new Vector2(0, 1);
            PtsTexture[1] = new Vector2(1, 1);
            PtsTexture[2] = new Vector2(0, 0);
            PtsTexture[3] = new Vector2(1, 0);
        }

        protected override void InitialiserSommets()
        {
            int NoSommet = -1;
            for (int j = 0; j < PtsSommets.GetLength(1) - 1; ++j)
            {
                for (int i = 0; i < PtsSommets.GetLength(0) - 1; ++i)
                {
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i, j], new Vector2(0, 1));
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i, j + 1], new Vector2(0, 0));
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i + 1, j], new Vector2(1, 1));
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i, j + 1], new Vector2(0, 0));
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i + 1, j + 1], new Vector2(1, 0));
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i + 1, j], new Vector2(1, 1));
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            EffetDeBase.World = GetMonde();
            EffetDeBase.View = Cam�raJeu.Vue;
            EffetDeBase.Projection = Cam�raJeu.Projection;
            foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
            {
                passeEffet.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, Sommets, 0, NbTriangles);
            }
            base.Draw(gameTime);
        }
    }
}
