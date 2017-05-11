using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;


namespace Projet_ASL
{
    //Classe qui gère les entrées au clavier et à la souris.
    //Elle s'occupe aussi des vérifications de sélection de personnages et
    //des vérifications du déplacement des personnages

    public class InputManager : Microsoft.Xna.Framework.GameComponent
    {
        //constantes de la classe
        const int DÉPLACEMENT_MAX = 10;
        const int RAYON_SPHÈRE_COLLISION_PERSO = 2;

        //Propriétés de la classe
        private ManagerNetwork _managerNetwork;
        Keys[] AnciennesTouches { get; set; }
        Keys[] NouvellesTouches { get; set; }
        KeyboardState ÉtatClavier { get; set; }
        MouseState AncienÉtatSouris { get; set; }
        MouseState NouvelÉtatSouris { get; set; }
        Caméra Cam { get; set; }

        //Propriétés en lien direct avec le personnage
        public bool PersonnageSélectionné { get; private set; }
        float? DistanceRayon { get; set; }
        Personnage PersonnageChoisi { get; set; }
        Vector3 PositionInitialePersonnage { get; set; }

        /// <summary>
        /// Constructeur de la classe InputManager
        /// </summary>
        /// <param name="game">le jeu</param>
        /// <param name="caméraJeu">la caméra utilisée par le jeu</param>
        /// <param name="managerNetwork">l'instance de la classe qui gère la partie réseau du jeu</param>
        public InputManager(Game game, Caméra caméraJeu, ManagerNetwork managerNetwork)
           : base(game)
        {
            _managerNetwork = managerNetwork;
            Cam = caméraJeu;
        }

        /// <summary>
        /// La méthode qui permet d'initialiser différents paramètres de la classe
        /// </summary>
        public override void Initialize()
        {
            NouvellesTouches = new Keys[0];
            AnciennesTouches = NouvellesTouches;
            NouvelÉtatSouris = Mouse.GetState();
            AncienÉtatSouris = NouvelÉtatSouris;
            PersonnageSélectionné = false;
            DistanceRayon = null;
            PersonnageChoisi = null;
            base.Initialize();
        }

        /// <summary>
        /// la méthode update de la classe
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            AnciennesTouches = NouvellesTouches;
            ÉtatClavier = Keyboard.GetState();
            NouvellesTouches = ÉtatClavier.GetPressedKeys();
            if (EstSourisActive)
            {
                ActualiserÉtatSouris();
            }
        }

        #region Méthodes pour la sélection du personnage à attaquer

        /// <summary>
        /// Méthode qui permet de déterminer quel personnage à été sélectionné pour être attaqué
        /// </summary>
        /// <param name="PersonnagesSélectionnables">La liste des personnages qui peuvent être attaqués</param>
        /// <returns>Le personnage à attaquer</returns>
        public Personnage DéterminerSélectionPersonnageÀAttaquer(List<Personnage> PersonnagesSélectionnables)
        {
            Personnage personnageÀAttaquer = null;

            if (EstNouveauClicDroit())
            {
                Ray ray = CalculateCursorRay();
                float DistanceLaPlusPrès = float.MaxValue;
                foreach (Personnage perso in PersonnagesSélectionnables)
                {
                    DistanceRayon = perso.SphèreDeCollision.Intersects(ray);
                    if (DistanceRayon != null && DistanceRayon < DistanceLaPlusPrès)
                    {
                        DistanceLaPlusPrès = (float)DistanceRayon;
                        personnageÀAttaquer = perso;
                    }

                }
            }

            return personnageÀAttaquer;
        }

        #endregion

        #region Méthodes pour déplacer les personnages et les vérifications de leur déplacement

        /// <summary>
        /// Méthode qui permet de déterminer si un certain personnage est sélectionné pour le déplacement
        /// </summary>
        /// <param name="indice">L'indice du personnage dans la liste des personnages du joueur local</param>
        public void DéterminerSélectionPersonnageDéplacement(int indice)
        {
            if (EstNouveauClicGauche())
            {
                if (DéterminerIntersectionPersonnageRayDéplacement(indice))
                {
                    PersonnageSélectionné = true;
                    PositionInitialePersonnage = PersonnageChoisi.Position;
                }
            }
        }

        /// <summary>
        /// Méthode appelé par DéterminerSélectionPersonnageDéplacement(int indice) qui permet
        /// de déterminer s'il y a une intersection entre le rayon crée à partir du curseur et un 
        /// certain personnage.
        /// </summary>
        /// <param name="indice">L'indice du personnage dans la liste des personnages du joueur local</param>
        /// <returns>un bool qui dit si oui ou non si ce personnage est sélectionné</returns>
        public bool DéterminerIntersectionPersonnageRayDéplacement(int indice)
        {
            bool rep = false;
            Ray ray = CalculateCursorRay();
            DistanceRayon = _managerNetwork.JoueurLocal.Personnages[indice].SphèreDeCollision.Intersects(ray);
            if (DistanceRayon != null)
            {
                PersonnageChoisi = _managerNetwork.JoueurLocal.Personnages[indice];
                rep = true;
            }
            return rep;
        }

        /// <summary>
        /// Méthode qui permet de déterminer ce qui se passe lorsqu'un personnage est sélectionné ou s'il
        /// vient d'être relâché. Elle indique le déplacement restant au personnage.
        /// </summary>
        /// <param name="déplacement_maximal">Le déplacement maximal permis pour le personnage</param>
        /// <param name="indice">L'indice du personnage</param>
        /// <returns>un float qui indique le déplacement restant</returns>
        public float DéterminerMouvementPersonnageSélectionné(float déplacement_maximal, int indice)
        {
            float déplacement_restant = déplacement_maximal;
            if (PersonnageSélectionné)
            {
                if (EstAncienClicGauche())
                {
                    Vector3 positionVouluePersonnage = GetPositionSourisPlan();
                    positionVouluePersonnage = VérifierDéplacementMAX(positionVouluePersonnage, déplacement_maximal);
                    positionVouluePersonnage = VérifierDéplacementCollisionPersonnage(positionVouluePersonnage, indice);
                    _managerNetwork.SendNewPosition(positionVouluePersonnage, _managerNetwork.JoueurLocal.Personnages.FindIndex(p => p.GetType() == PersonnageChoisi.GetType()));
                }
                if (EstReleasedClicGauche())
                {
                    déplacement_restant = déplacement_maximal - Vector3.Distance(PositionInitialePersonnage, PersonnageChoisi.Position);
                    PersonnageSélectionné = false;
                    PersonnageChoisi = null;
                }
            }
            return déplacement_restant;
        }

        /// <summary>
        /// Méthode appelée pour vérifier si, avec sa position voulue, le personnage déplacé entre en collision
        /// avec un autre personnage
        /// </summary>
        /// <param name="positionVouluePersonnage"></param>
        /// <param name="indice"></param>
        /// <returns>La position du personnage qui n'est pas en collision avec un autre personnage</returns>
        public Vector3 VérifierDéplacementCollisionPersonnage(Vector3 positionVouluePersonnage, int indice)
        {
            BoundingSphere test = new BoundingSphere(positionVouluePersonnage, RAYON_SPHÈRE_COLLISION_PERSO);
            Vector3 anciennePosition = PersonnageChoisi.Position;
            Vector3 positionVérifiéeFinale = positionVouluePersonnage;

            foreach (Personnage perso in _managerNetwork.JoueurLocal.Personnages)
            {
                PersonnageChoisi.GérerPositionObjet(positionVouluePersonnage);
                if (Vector3.Distance(PersonnageChoisi.Position, perso.Position) < DÉPLACEMENT_MAX)
                {
                    if (test.Intersects(perso.SphèreDeCollision))
                    {
                        if (perso != _managerNetwork.JoueurLocal.Personnages[indice])
                        {
                            positionVérifiéeFinale = anciennePosition;
                        }
                    }
                }
            }
            foreach (Personnage perso in _managerNetwork.JoueurEnnemi.Personnages)
            {
                PersonnageChoisi.GérerPositionObjet(positionVouluePersonnage);
                if (Vector3.Distance(PersonnageChoisi.Position, perso.Position) < DÉPLACEMENT_MAX)
                {
                    if (test.Intersects(perso.SphèreDeCollision))
                    {
                        positionVérifiéeFinale = anciennePosition;
                    }
                }
            }
            PersonnageChoisi.GérerPositionObjet(positionVérifiéeFinale);
            return positionVérifiéeFinale;
        }

        /// <summary>
        /// Méthode qui permet de vérifier si un personnage atteint le déplacement maximal 
        /// et qui change comment est décrite sa position s'il a atteint sa limite
        /// </summary>
        /// <param name="positionVoulue">La position voulue du personnage</param>
        /// <param name="déplacementMax"></param>
        /// <returns>la positon du personnage qui respecte son déplacement maximal</returns>
        public Vector3 VérifierDéplacementMAX(Vector3 positionVoulue, float déplacementMax)
        {
            Vector3 positionVérifiée;
            if (Vector3.Distance(PositionInitialePersonnage, positionVoulue) <= déplacementMax)
            {
                positionVérifiée = positionVoulue;
            }
            else
            {
                positionVérifiée = déplacementMax * Vector3.Normalize(positionVoulue - PositionInitialePersonnage) + PositionInitialePersonnage;
            }
            return positionVérifiée;
        }
        #endregion


        #region Méthodes pour la vérification des états de la souris et du clavier

        /// <summary>
        /// Méthode qui permet de vérifier si une touche est enfoncée ou non
        /// </summary>
        /// <param name="touche">La touche que l'on veut vérifier</param>
        /// <returns>Un bool qui dit si oui ou non la touche est enfoncée</returns>
        public bool EstEnfoncée(Keys touche)
        {
            return ÉtatClavier.IsKeyDown(touche);
        }

        /// <summary>
        /// Méthode qui permet de vérifier si une touche vient d'être enfoncée ou non
        /// </summary>
        /// <param name="touche">La touche que l'on veut vérifier</param>
        /// <returns>Un bool qui dit si oui ou non la touche vient d'être enfoncée</returns>
        public bool EstNouvelleTouche(Keys touche)
        {
            int nbTouches = AnciennesTouches.Length;
            bool estNouvelleTouche = ÉtatClavier.IsKeyDown(touche);
            int i = 0;
            while (i < nbTouches && estNouvelleTouche)
            {
                estNouvelleTouche = AnciennesTouches[i] != touche;
                ++i;
            }
            return estNouvelleTouche;
        }

        /// <summary>
        /// Méthode qui permet d'aller chercher le nouvel état de la souris et de mettre à jour l'ancien
        /// </summary>
        void ActualiserÉtatSouris()
        {
            AncienÉtatSouris = NouvelÉtatSouris;
            NouvelÉtatSouris = Mouse.GetState();
        }

        /// <summary>
        /// Méthode qui permet de vérifier si la souris est active ou non
        /// </summary>
        /// <returns>Un bool qui dit si oui ou non la souris est active</returns>
        public bool EstSourisActive
        {
            get { return Game.IsMouseVisible; }
        }

        /// <summary>
        /// Méthode qui permet de vérifier si le clic gauche est encore enfoncé ou non
        /// </summary>
        /// <returns>Un bool qui dit si oui ou non le clic gauche est encore enfoncé</returns>
        public bool EstAncienClicGauche()
        {
            return NouvelÉtatSouris.LeftButton == ButtonState.Pressed && AncienÉtatSouris.LeftButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Méthode qui permet de vérifier si le clic droit vient d'être enfoncé ou non
        /// </summary>
        /// <returns>Un bool qui dit si oui ou non le clic droit vient d'être enfoncé</returns>
        public bool EstNouveauClicDroit()
        {
            return NouvelÉtatSouris.RightButton == ButtonState.Pressed && AncienÉtatSouris.RightButton == ButtonState.Released;
        }

        /// <summary>
        /// Méthode qui permet de vérifier si le clic gauche vient d'être enfoncé ou non
        /// </summary>
        /// <returns>Un bool qui dit si oui ou non le clic gauche vient d'être enfoncé</returns>
        public bool EstNouveauClicGauche()
        {
            return NouvelÉtatSouris.LeftButton == ButtonState.Pressed &&
                   AncienÉtatSouris.LeftButton == ButtonState.Released;
        }

        /// <summary>
        /// Méthode qui permet de vérifier si le clic gauche vient d'être relâché ou non
        /// </summary>
        /// <returns>Un bool qui dit si oui ou non le clic droit vient d'être enfoncé</returns>
        public bool EstReleasedClicGauche()
        {
            return NouvelÉtatSouris.LeftButton == ButtonState.Released;
        }

        /// <summary>
        /// Méthode qui permet de retourner la position de la souris à l'écran en 2D
        /// </summary>
        /// <returns>Un Point représentat la position de la souris à l'écran en 2D</returns>
        public Point GetPositionSouris()
        {
            return new Point(NouvelÉtatSouris.X, NouvelÉtatSouris.Y);
        }

        /// <summary>
        /// Méthode qui permet d'obtenir la valeur de l'ancien défilement
        /// </summary>
        /// <returns>la valeur de l'ancien défilement</returns>
        public int GetAncienScrollWheelValue()
        {
            return AncienÉtatSouris.ScrollWheelValue;
        }

        /// <summary>
        /// Méthode qui permet d'obtenir la valeur du nouveau défilement
        /// </summary>
        /// <returns>la valeur du nouveau défilement</returns>
        public int GetNouveauScrollWheelValue()
        {
            return NouvelÉtatSouris.ScrollWheelValue;
        }
        #endregion


        #region Ray et PositionSouris3D

        /// <summary>
        /// Méthode trouvée sur le site de Microsoft qui permet de retourner la position de la souris dans l'espace 3D
        /// </summary>
        /// <returns>La position de la souris dans l'espace 3D</returns>
        public Vector3 GetPositionSourisPlan()
        {
            Vector3 nearScreenPoint = new Vector3(NouvelÉtatSouris.X, NouvelÉtatSouris.Y, 0);
            Vector3 farScreenPoint = new Vector3(NouvelÉtatSouris.X, NouvelÉtatSouris.Y, 1);
            Vector3 nearWorldPoint = Game.GraphicsDevice.Viewport.Unproject(nearScreenPoint, Cam.Projection, Cam.Vue, Matrix.Identity);
            Vector3 farWorldPoint = Game.GraphicsDevice.Viewport.Unproject(farScreenPoint, Cam.Projection, Cam.Vue, Matrix.Identity);

            Vector3 direction = farWorldPoint - nearWorldPoint;

            float zFactor = -nearWorldPoint.Y / direction.Y;
            Vector3 zeroWorldPoint = nearWorldPoint + direction * zFactor;

            return zeroWorldPoint;
        }

        /// <summary>
        /// Méthode trouvé sur le site gamedev.stackexchange.com qui permet de retourner un rayon qui part de la position de la souris et qui va aller dans l'espace 3D
        /// </summary>
        /// <returns>Le rayon qui part de la souris dans l'espace 3D</returns>
        public Ray CalculateCursorRay()
        {
            Vector3 nearScreenPoint = new Vector3(NouvelÉtatSouris.X, NouvelÉtatSouris.Y, 0);
            Vector3 farScreenPoint = new Vector3(NouvelÉtatSouris.X, NouvelÉtatSouris.Y, 1);
            Vector3 nearWorldPoint = Game.GraphicsDevice.Viewport.Unproject(nearScreenPoint, Cam.Projection, Cam.Vue, Matrix.Identity);
            Vector3 farWorldPoint = Game.GraphicsDevice.Viewport.Unproject(farScreenPoint, Cam.Projection, Cam.Vue, Matrix.Identity);

            Vector3 direction = farWorldPoint - nearWorldPoint;

            direction.Normalize();

            return new Ray(nearWorldPoint, direction);
        }
        #endregion
    }
}