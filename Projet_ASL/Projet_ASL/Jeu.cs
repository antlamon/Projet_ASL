//------------------------------------------------------
// 
// Copyright - (c) - 2014 - Mille Boström 
//
// Youtube channel - https://www.youtube.com/user/Maloooon
//------------------------------------------------------
//Modifié par ASL©
#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Projet_ASL
{
    enum OrdreDraw { ARRIÈRE_PLAN, MILIEU, AVANT_PLAN };
    enum États { MENU, INVENTAIRE, QUITTER, CONNEXION, JEU, FIN_DE_JEU };

    public class Jeu : Game
    {
        const float INTERVALLE_CALCUL_FPS = 1f;
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;

        GraphicsDeviceManager PériphériqueGraphique { get; set; }
        SpriteBatch GestionSprites { get; set; }
        RessourcesManager<SpriteFont> GestionnaireDeFonts { get; set; }
        RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }
        RessourcesManager<Model> GestionnaireDeModèles { get; set; }
        Caméra CaméraJeu { get; set; }
        InputManager GestionInput { get; set; }
        États ÉtatJeu { get; set; }
        ManagerNetwork _managerNetwork;
        ManagerInput _managerInput;
        TourManager ManagerTour;

        DialogueMenu MenuAccueil { get; set; }
        DialogueInventaire MenuInventaire { get; set; }
        //DialogueActions MenuActions { get; set; }

        private Texture2D _texture; //For test
        private SpriteFont _font; //For test
                                  //private Mage pion2; //for test
                                  //private Color couleur;

        private int CompteurDeTours { get; set; }
        private int CompteurPersonnage { get; set; }

        bool TourLocal { get; set; }

        public Jeu()
            : base()
        {
            PériphériqueGraphique = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _managerNetwork = new ManagerNetwork(this);
            //_managerInput = new ManagerInput(_managerNetwork);
            PériphériqueGraphique.SynchronizeWithVerticalRetrace = false;
            PériphériqueGraphique.PreferredBackBufferHeight = 720;
            PériphériqueGraphique.PreferredBackBufferWidth = 1280;
            IsFixedTimeStep = false;
            IsMouseVisible = true;
        }


        protected override void Initialize()
        {
            //Vector3 positionObjet1 = new Vector3(-2, 0, 0);
            //Vector3 positionObjet2 = new Vector3(0, 1.5f, 0);
            //Vector3 positionObjet3 = new Vector3(0, 0, 0);
            //Vector3 positionObjet4 = new Vector3(0, -1.5f, 0);
            //Vector3 positionObjet5 = new Vector3(2, 0, 0);
            //Vector3 positionLumière = new Vector3(0, 0f, 3f);
            CompteurDeTours = 1;
            CompteurPersonnage = 0;

            Vector3 positionCaméra = new Vector3(0, 80, 20);
            Vector3 cibleCaméra = new Vector3(0, 0, 0);
            Vector2 dimensionDialogueMenu = new Vector2(Window.ClientBounds.Width / 3, Window.ClientBounds.Height);
            Vector2 dimensionDialogueInventaire = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height);
            //a changer les dimensions
            //Vector2 dimensionDialogueSpells = new Vector2(Window.ClientBounds.Width/3f, Window.ClientBounds.Height / 7f);
            CaméraJeu = new CaméraSubjective(this, positionCaméra, cibleCaméra, -Vector3.UnitZ, INTERVALLE_MAJ_STANDARD);
            MenuAccueil = new DialogueMenu(this, dimensionDialogueMenu, _managerNetwork);
            MenuInventaire = new DialogueInventaire(this, dimensionDialogueInventaire);
            //MenuActions = new DialogueActions(this, dimensionDialogueSpells);
            
            CréationDuPanierDeServices();

            //Components.Add(new ArrièrePlanSpatial(this, "CielÉtoilé", INTERVALLE_MAJ_STANDARD));
            AfficheurFPS afficheurFPS = new AfficheurFPS(this, "Arial20", Color.Gold, INTERVALLE_CALCUL_FPS);
            afficheurFPS.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            Components.Add(afficheurFPS);
            Components.Add(new Afficheur3D(this));
            Components.Add(GestionInput);
            Components.Add(CaméraJeu);

            Components.Add(MenuAccueil);
            Components.Add(MenuInventaire);
            //Components.Add(MenuActions);
            base.Initialize();
        }

        private void CréationDuPanierDeServices()
        {
            GestionnaireDeFonts = new RessourcesManager<SpriteFont>(this, "Fonts");
            GestionnaireDeTextures = new RessourcesManager<Texture2D>(this, "Textures");
            GestionnaireDeModèles = new RessourcesManager<Model>(this, "Models");
            //GestionnaireDeShaders = new RessourcesManager<Effect>(this, "Effects");
            GestionInput = new InputManager(this, CaméraJeu, _managerNetwork);
            GestionSprites = new SpriteBatch(GraphicsDevice);

            Services.AddService(typeof(RessourcesManager<SpriteFont>), GestionnaireDeFonts);
            Services.AddService(typeof(RessourcesManager<Texture2D>), GestionnaireDeTextures);
            Services.AddService(typeof(RessourcesManager<Model>), GestionnaireDeModèles);
            //Services.AddService(typeof(RessourcesManager<Effect>), GestionnaireDeShaders);
            Services.AddService(typeof(InputManager), GestionInput);
            Services.AddService(typeof(Caméra), CaméraJeu);
            Services.AddService(typeof(SpriteBatch), GestionSprites);
        }

        protected override void LoadContent()
        {
            _font = GestionnaireDeFonts.Find("Arial20");
            _texture = GestionnaireDeTextures.Find("BoutonRouge");
            base.LoadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            GérerClavier();
            GérerTransition(gameTime);

            base.Update(gameTime);
        }

        private void GérerTransition(GameTime gameTime)
        {
            switch (ÉtatJeu)
            {
                case États.MENU:
                    MenuAccueil.BtnJouer.Enabled = MenuInventaire._player.Personnages.Count == 4;
                    if (MenuAccueil.ÉtatJouer)
                    {
                        ÉtatJeu = États.CONNEXION;
                        MenuAccueil.VoirBoutonMenu(false);
                    }
                    if (MenuAccueil.ÉtatInventaire)
                    {
                        ÉtatJeu = États.INVENTAIRE;
                        MenuAccueil.VoirBoutonMenu(false);
                        MenuInventaire.VoirBoutonInventaire(true);
                    }
                    break;
                case États.CONNEXION:
                    _managerNetwork.Start(MenuInventaire._player);
                    DémarrerPhaseDeJeu();
                    ÉtatJeu = États.JEU;
                     goto case États.JEU;
                case États.JEU:
                    if (PeopleAlive())
                    {
                        _managerNetwork.Update();
                        TourLocal = _managerNetwork.TourActif;
                        Window.Title = TourLocal.ToString();
                        if(TourLocal)
                        {
                            ManagerTour.Update(gameTime);
                        }
                        //_managerInput.Update(gameTime.ElapsedGameTime.Milliseconds);
                    }
                    else
                    {
                        CompteurManches.RéinitialiserCompteur();
                        ÉtatJeu = États.FIN_DE_JEU;
                    }
                    break;
                case États.INVENTAIRE:
                    if (MenuInventaire.ÉtatMenu)
                    {
                        ÉtatJeu = États.MENU;
                        MenuInventaire.VoirBoutonInventaire(false);
                        MenuAccueil.VoirBoutonMenu(true);
                    }

                    break;
                case États.FIN_DE_JEU:
                    ÉtatJeu = États.MENU;
                    break;
            }
        }


        private void DémarrerPhaseDeJeu()
        {
            TourLocal = _managerNetwork.TourActif;
            Carte carte = new Carte(this, 1f, Vector3.Zero, Vector3.Zero, new Vector2(120, 60), new Vector2(24, 16), "hexconcrete", INTERVALLE_MAJ_STANDARD);
            carte.DrawOrder = (int)OrdreDraw.ARRIÈRE_PLAN;
            Components.Add(carte);

            while(_managerNetwork.JoueurLocal == null)
            {
                _managerNetwork.Update();
            }
            ManagerTour = new TourManager(this, _managerNetwork);
            ManagerTour.Initialize();
        }


        private bool PeopleAlive()
        {
            return _managerNetwork.Players.TrueForAll(player => player.Personnages.Exists(perso => !perso.EstMort));
        }


        private void Combat()
        {
            if(TourLocal)
            {
                //Personnage persoLocal = _managerNetwork.JoueurLocal.Personnages[CompteurPersonnage];
                //MenuActions.VoirBouttonAction(true);

                GérerCompteurs(); // à revoir
            }
            //MenuActions.VoirBouttonAction(false);
        }


        private void GérerCompteurs()
        {
            if(CompteurPersonnage < _managerNetwork.JoueurLocal.Personnages.Count)
            { // devrait etre .Count - 1, ex. : Count = 5, si cptPerso = 4, ++cptPerso = 5, alors l'indice de la list va etre OutOfBounds
                ++CompteurPersonnage;
            }
            else
            {
                CompteurPersonnage = 0;
            }
        }


        private void GérerClavier()
        {
            if (GestionInput.EstNouvelleTouche(Keys.Escape))
            {
                MenuAccueil.VoirBoutonMenu(!MenuAccueil.MenuVisible);
            }
            if (GestionInput.EstNouvelleTouche(Keys.F11))
            {
                PériphériqueGraphique.ToggleFullScreen();
            }

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);
        }
    }
}