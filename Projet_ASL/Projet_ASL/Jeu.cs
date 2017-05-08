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
        ManagerNetwork _managerNetwork;
        TourManager ManagerTour;
        public Carte PlancheDeJeu { get; private set; } // HAD TO : voir TourManager
        DialogueMenu MenuAccueil { get; set; }
        DialogueInventaire MenuInventaire { get; set; }
        public AOE AOE1 { get; private set; }
        public AOE AOE2 { get; private set; }
        public AOE AOE3 { get; private set; }
        TexteCentr� TexteConnection { get; set; }

        private Texture2D _texture; //For test
        private SpriteFont _font; //For test
                                  //private Mage pion2; //for test
                                  //private Color couleur

        bool TourLocal { get; set; }

        public Jeu()
            : base()
        {
            P�riph�riqueGraphique = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _managerNetwork = new ManagerNetwork(this);
            P�riph�riqueGraphique.SynchronizeWithVerticalRetrace = false;
            P�riph�riqueGraphique.PreferredBackBufferHeight = 720;
            P�riph�riqueGraphique.PreferredBackBufferWidth = 1280;
            Window.AllowUserResizing = true;
            IsFixedTimeStep = false;
            IsMouseVisible = true;
            Window.Title = "Jeu de Bataille";
            P�riph�riqueGraphique.
        }


        protected override void Initialize()
        {
            //Vector3 positionObjet1 = new Vector3(-2, 0, 0);
            //Vector3 positionObjet2 = new Vector3(0, 1.5f, 0);
            //Vector3 positionObjet3 = new Vector3(0, 0, 0);
            //Vector3 positionObjet4 = new Vector3(0, -1.5f, 0);
            //Vector3 positionObjet5 = new Vector3(2, 0, 0);
            //Vector3 positionLumi�re = new Vector3(0, 0f, 3f);
            Vector3 positionCam�ra = new Vector3(0, 55, 15);
            Vector3 cibleCam�ra = new Vector3(0, 0, 0);
            Vector2 dimensionDialogueMenu = new Vector2(Window.ClientBounds.Width / 3, Window.ClientBounds.Height);
            Vector2 dimensionDialogueInventaire = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height);
            //a changer les dimensions
            //Vector2 dimensionDialogueSpells = new Vector2(Window.ClientBounds.Width/3f, Window.ClientBounds.Height / 7f);
            Cam�raJeu = new Cam�raSubjective(this, positionCam�ra, cibleCam�ra, -Vector3.UnitZ, INTERVALLE_MAJ_STANDARD);
            MenuAccueil = new DialogueMenu(this, dimensionDialogueMenu, _managerNetwork);
            MenuInventaire = new DialogueInventaire(this, dimensionDialogueInventaire);

            Cr�ationDuPanierDeServices();

            AfficheurFPS afficheurFPS = new AfficheurFPS(this, "Arial20", Color.Gold, INTERVALLE_CALCUL_FPS);
            afficheurFPS.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            Components.Add(afficheurFPS);
            Arri�rePlanD�roulant Arri�rePlan = new Arri�rePlanD�roulant(this, "WoodForest", INTERVALLE_MAJ_STANDARD);
            Arri�rePlan.DrawOrder = (int)OrdreDraw.ARRI�RE_PLAN;
            Components.Add(Arri�rePlan);
            PlancheDeJeu = new Carte(this, 1f, Vector3.Zero, Vector3.Zero, new Vector2(120, 60), new Vector2(24, 16), "hexconcrete", INTERVALLE_MAJ_STANDARD);
            PlancheDeJeu.DrawOrder = (int)OrdreDraw.ARRI�RE_PLAN;
            PlancheDeJeu.Visible = false;
            Components.Add(PlancheDeJeu);
            AOE1 = new AOE(this, 1f, Vector3.Zero, Vector3.Zero, new Vector2(20), "AOE", INTERVALLE_MAJ_STANDARD);
            AOE1.Visible = false;
            AOE1.DrawOrder = (int)OrdreDraw.ARRI�RE_PLAN;
            Components.Add(AOE1);
            AOE2 = new AOE(this, 1f, Vector3.Zero, Vector3.Zero, new Vector2(20), "AOE", INTERVALLE_MAJ_STANDARD);
            AOE2.Visible = false;
            AOE2.DrawOrder = (int)OrdreDraw.ARRI�RE_PLAN;
            Components.Add(AOE2);
            AOE3 = new AOE(this, 1f, Vector3.Zero, Vector3.Zero, new Vector2(20), "AOE", INTERVALLE_MAJ_STANDARD);
            AOE3.Visible = false;
            AOE3.DrawOrder = (int)OrdreDraw.ARRI�RE_PLAN;
            Components.Add(AOE3);
            TexteConnection = new TexteCentr�(this, "En attente d'un autre joueur", "Arial20", new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.Red, 0.2f);
            TexteConnection.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            Components.Add(TexteConnection);
            TexteConnection.Visible = false;
            Components.Add(new Afficheur3D(this));
            Components.Add(GestionInput);
            Components.Add(Cam�raJeu);

            Components.Add(MenuAccueil);
            Components.Add(MenuInventaire);

            base.Initialize();
        }

        private void Cr�ationDuPanierDeServices()
        {
            GestionnaireDeFonts = new RessourcesManager<SpriteFont>(this, "Fonts");
            GestionnaireDeTextures = new RessourcesManager<Texture2D>(this, "Textures");
            GestionnaireDeMod�les = new RessourcesManager<Model>(this, "Models");

            GestionInput = new InputManager(this, Cam�raJeu, _managerNetwork);
            GestionSprites = new SpriteBatch(GraphicsDevice);

            Services.AddService(typeof(RessourcesManager<SpriteFont>), GestionnaireDeFonts);
            Services.AddService(typeof(RessourcesManager<Texture2D>), GestionnaireDeTextures);
            Services.AddService(typeof(RessourcesManager<Model>), GestionnaireDeMod�les);

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


        protected override void Update(GameTime gameTime)
        {
            G�rerClavier();
            G�rerTransition(gameTime);
            base.Update(gameTime);
        }

        private void G�rerTransition(GameTime gameTime)
        {
            switch (�tatJeu)
            {
                case �tats.MENU:
                    if (MenuAccueil.�tatJouer)
                    {
                        if(MenuInventaire._player.Personnages.Count != 4)
                        {
                            MenuInventaire.InitialiserPersonnageParD�faut();
                        }
                        �tatJeu = �tats.CONNEXION;
                        MenuAccueil.VoirBoutonMenu(false);
                        _managerNetwork.Start(MenuInventaire._player);
                        D�marrerJeu(gameTime);
                    }
                    if (MenuAccueil.�tatInventaire)
                    {
                        �tatJeu = �tats.INVENTAIRE;
                        MenuAccueil.VoirBoutonMenu(false);
                        MenuInventaire.VoirBoutonInventaire(true);
                    }
                    break;
                case �tats.CONNEXION:
                    _managerNetwork.Update();
                    if (_managerNetwork.Players.Count == 2)
                    {
                        TexteConnection.Visible = false;
                        ManagerTour = new TourManager(this, _managerNetwork);
                        ManagerTour.Initialize();
                        �tatJeu = �tats.JEU;
                    }
                    if (_managerNetwork.Players.Count == 0)
                    {
                        RetourAuMenu();
                    }
                    break;
                case �tats.JEU:
                    if (PeopleAlive())
                    {
                        _managerNetwork.Update();
                        TourLocal = _managerNetwork.TourActif;
                        if (TourLocal && _managerNetwork.JoueurLocal != null)
                        {
                            ManagerTour.Update(gameTime);
                        }
                        if (MenuAccueil.�tatRetourMenu || _managerNetwork.Players.Count == 0)
                        {
                            RetourAuMenu();
                        }
                    }
                    else
                    {
                        CompteurManches.R�initialiserCompteur();
                        �tatJeu = �tats.FIN_DE_JEU;
                        TexteConnection.ModifierTexte(_managerNetwork.JoueurLocal.Personnages.Exists(p => !p.EstMort) ? "VICTOIRE" : "D�FAITE");
                        TexteConnection.Visible = true;
                    }
                    break;
                case �tats.INVENTAIRE:
                    MenuInventaire.BtnOK.EstActif = MenuInventaire._player.Personnages.Count == 4;
                    if (MenuInventaire.�tatMenu)
                    {
                        �tatJeu = �tats.MENU;
                        MenuInventaire.VoirBoutonInventaire(false);
                        MenuAccueil.VoirBoutonMenu(true);
                    }
                    break;
                case �tats.FIN_DE_JEU:
                    _managerNetwork.Update();
                    if (MenuAccueil.�tatRetourMenu || _managerNetwork.Players.Count == 0)
                    {
                        RetourAuMenu();
                    }

                    break;
            }
        }

        private void RetourAuMenu()
        {
            �tatJeu = �tats.MENU;
            MenuAccueil.VoirOptionsMenu(false);
            MenuAccueil.VoirBoutonMenu(true);
            PlancheDeJeu.Visible = false;
            TexteConnection.Visible = false;
            TexteConnection.ModifierTexte("En attente d'un autre joueur");
            AOE1.Visible = false;
            AOE2.Visible = false;
            AOE3.Visible = false;
            if (ManagerTour != null)
            {
                foreach (BoutonDeCommande b in ManagerTour.BoutonsActions.Boutons)
                {
                    Components.Remove(b);
                }
            }

        }

        private void D�marrerJeu(GameTime gameTime)
        {
            TourLocal = _managerNetwork.TourActif;
            PlancheDeJeu.Visible = true;
            TexteConnection.Visible = true;
        }

        private bool PeopleAlive()
        {
            return _managerNetwork.Players.TrueForAll(player => player.Personnages.Exists(perso => !perso.EstMort));
        }

        private void G�rerClavier()
        {
            if (GestionInput.EstNouvelleTouche(Keys.Escape))
            {
                MenuAccueil.VoirOptionsMenu(!MenuAccueil.MenuVisible);
            }
            if (GestionInput.EstNouvelleTouche(Keys.F11))
            {
                P�riph�riqueGraphique.ToggleFullScreen();
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