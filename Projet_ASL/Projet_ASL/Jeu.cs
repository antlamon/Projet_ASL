//------------------------------------------------------
// 
// Copyright - (c) - 2014 - Mille Bostr�m 
//
// Youtube channel - https://www.youtube.com/user/Maloooon
//------------------------------------------------------
//Modifi� par ASL�
#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Projet_ASL
{
    enum OrdreDraw { ARRI�RE_PLAN, MILIEU, AVANT_PLAN };
    enum �tats { MENU, INVENTAIRE, QUITTER, CONNEXION, JEU, FIN_DE_JEU };
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Jeu : Game
    {
        const float INTERVALLE_CALCUL_FPS = 1f;
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;

        GraphicsDeviceManager P�riph�riqueGraphique { get; set; }
        SpriteBatch GestionSprites { get; set; }
        RessourcesManager<SpriteFont> GestionnaireDeFonts { get; set; }
        RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }
        RessourcesManager<Model> GestionnaireDeMod�les { get; set; }
        Cam�ra Cam�raJeu { get; set; }
        InputManager GestionInput { get; set; }
        �tats �tatJeu { get; set; }
        private ManagerNetwork _managerNetwork;
        private ManagerInput _managerInput;

        DialogueMenu MenuAccueil { get; set; }

        private Texture2D _texture; //For test
        private SpriteFont _font; //For test
        private Mage pion2; //for test

        public Jeu()
            : base()
        {
            P�riph�riqueGraphique = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _managerNetwork = new ManagerNetwork();
            _managerInput = new ManagerInput(_managerNetwork);
            P�riph�riqueGraphique.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = false;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Vector3 positionObjet1 = new Vector3(-2, 0, 0);
            //Vector3 positionObjet2 = new Vector3(0, 1.5f, 0);
            //Vector3 positionObjet3 = new Vector3(0, 0, 0);
            //Vector3 positionObjet4 = new Vector3(0, -1.5f, 0);
            //Vector3 positionObjet5 = new Vector3(2, 0, 0);
            //Vector3 positionLumi�re = new Vector3(0, 0f, 3f);
            Vector3 positionCam�ra = new Vector3(0, 0, 5);
            Vector3 cibleCam�ra = new Vector3(0, 0, 0);
            Vector2 dimensionDialogue = new Vector2(Window.ClientBounds.Width / 3, Window.ClientBounds.Height);
            Cam�raJeu = new Cam�raSubjective(this, positionCam�ra, cibleCam�ra, Vector3.Up, INTERVALLE_MAJ_STANDARD);
            MenuAccueil = new DialogueMenu(this, dimensionDialogue);

            //if (_managerNetwork.Start())
            //{
                _managerNetwork.Start();
                Cr�ationDuPanierDeServices();

                Components.Add(new Arri�rePlanSpatial(this, "Ciel�toil�", INTERVALLE_MAJ_STANDARD));
                Components.Add(new Afficheur3D(this));
                Components.Add(new AfficheurFPS(this, "Arial20", Color.Gold, INTERVALLE_CALCUL_FPS));
                Components.Add(GestionInput);
                Components.Add(Cam�raJeu);
                Components.Add(MenuAccueil);
            //}
            //else
            //{
            //    Cr�ationDuPanierDeServices();
            //    TexteCentr� texte = new TexteCentr�(this, "nope", "Arial20", Window.ClientBounds, Color.Red, 0.2f);
            //    texte.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            //    Components.Add(texte);
            //}

            base.Initialize();
        }

        private void Cr�ationDuPanierDeServices()
        {
            GestionnaireDeFonts = new RessourcesManager<SpriteFont>(this, "Fonts");
            GestionnaireDeTextures = new RessourcesManager<Texture2D>(this, "Textures");
            GestionnaireDeMod�les = new RessourcesManager<Model>(this, "Models");
            //GestionnaireDeShaders = new RessourcesManager<Effect>(this, "Effects");
            GestionInput = new InputManager(this);
            GestionSprites = new SpriteBatch(GraphicsDevice);

            Services.AddService(typeof(RessourcesManager<SpriteFont>), GestionnaireDeFonts);
            Services.AddService(typeof(RessourcesManager<Texture2D>), GestionnaireDeTextures);
            Services.AddService(typeof(RessourcesManager<Model>), GestionnaireDeMod�les);
            //Services.AddService(typeof(RessourcesManager<Effect>), GestionnaireDeShaders);
            Services.AddService(typeof(InputManager), GestionInput);
            Services.AddService(typeof(Cam�ra), Cam�raJeu);
            Services.AddService(typeof(SpriteBatch), GestionSprites);
        }

        protected override void LoadContent()
        {
            _font = GestionnaireDeFonts.Find("Arial20");
            _texture = GestionnaireDeTextures.Find("BoutonRouge");
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            G�rerClavier();
            G�rerTransition();
            if (�tatJeu == �tats.CONNEXION || �tatJeu == �tats.JEU)
            {
                _managerNetwork.Update();
                _managerInput.Update(gameTime.ElapsedGameTime.Milliseconds);
            }

            base.Update(gameTime);
        }

        private void G�rerTransition()
        {
            switch (�tatJeu)
            {
                case �tats.MENU:
                    if (MenuAccueil.�tatJouer)
                    {
                        �tatJeu = �tats.JEU;
                        D�marrerPhaseDeJeu();
                        MenuAccueil.VoirBouttonMenu(false);
                    }
                    if (MenuAccueil.�tatInventaire)
                    {
                        �tatJeu = �tats.INVENTAIRE;
                        MenuAccueil.VoirBouttonMenu(false);
                    }
                    break;
                case �tats.JEU:
                    break;
            }
        }

        private void D�marrerPhaseDeJeu()
        {
            Guerrier pion = new Guerrier(this, "GuerrierB", 0.03f, Vector3.Zero, Vector3.Zero, "bob", 0, 0, 0, 0, 1);
            pion2 = new Mage(this, "Mage", 0.03f, Vector3.Zero, new Vector3(0, 0, 5), "ok", 0, 0, 0, 0, 1);
            pion2.Visible = false;
            pion.DrawOrder = (int)OrdreDraw.MILIEU;
            pion2.DrawOrder = (int)OrdreDraw.MILIEU;
            Components.Add(pion);
            Components.Add(pion2);
        }

        private void G�rerClavier()
        {
            if (GestionInput.EstEnfonc�e(Keys.Escape))
            {
                MenuAccueil.VoirBouttonMenu(true);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            if (_managerNetwork.Active)
            {
                if (_managerNetwork.Players.Count > 1)
                {
                    pion2.Visible = true;
                }
            }
            base.Draw(gameTime);
        }
    }
}