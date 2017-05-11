using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;


namespace Projet_ASL
{
    public class InputManager : Microsoft.Xna.Framework.GameComponent
    {
        const int DÉPLACEMENT_MAX = 10;

        private ManagerNetwork _managerNetwork;
        Keys[] AnciennesTouches { get; set; }
        Keys[] NouvellesTouches { get; set; }
        KeyboardState ÉtatClavier { get; set; }
        MouseState AncienÉtatSouris { get; set; }
        MouseState NouvelÉtatSouris { get; set; }
        Caméra Cam { get; set; }

        public bool PersonnageSélectionné { get; private set; }
        float? DistanceRayon { get; set; }
        Personnage PersonnageChoisi { get; set; }
        Vector3 PositionInitialePersonnage { get; set; }


        public InputManager(Game game, Caméra caméraJeu, ManagerNetwork managerNetwork)
           : base(game)
        {
            _managerNetwork = managerNetwork;
            Cam = caméraJeu;
        }

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

        public void DéterminerIntersectionPersonnageRay()
        {
            Ray ray = CalculateCursorRay();
            float closestDistance = float.MaxValue;
            foreach (Personnage perso in _managerNetwork.JoueurLocal.Personnages)
            {
                DistanceRayon = perso.SphèreDeCollision.Intersects(ray);
                if (DistanceRayon != null && DistanceRayon < closestDistance)
                {
                    closestDistance = (float)DistanceRayon;
                    PersonnageChoisi = perso;
                }

            }
        }

        #region Méthode sélection personnage à attaquer

        public Personnage DéterminerSélectionPersonnageÀAttaquer(List<Personnage> PersonnagesSélectionnables)
        {
            Personnage personnageÀAttaquer = null;

            if (EstNouveauClicDroit())
            {
                Ray ray = CalculateCursorRay();
                float closestDistance = float.MaxValue;
                foreach (Personnage perso in PersonnagesSélectionnables)
                {
                    DistanceRayon = perso.SphèreDeCollision.Intersects(ray);
                    if (DistanceRayon != null && DistanceRayon < closestDistance)
                    {
                        closestDistance = (float)DistanceRayon;
                        personnageÀAttaquer = perso;
                    }

                }
            }

            return personnageÀAttaquer;
        }

        #endregion

        #region Méthodes déplacement

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

        public float DéterminerMouvementPersonnageSélectionné(float déplacement_maximal, int indice)
        {
            float déplacement_restant = déplacement_maximal;
            if (PersonnageSélectionné)
            {
                if (EstAncienClicGauche())
                {
                    Vector3 positionVouluePersonnage = GetPositionSourisPlan();
                    positionVouluePersonnage = VérifierDéplacementMAX(positionVouluePersonnage, PositionInitialePersonnage, déplacement_maximal);
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

        public Vector3 VérifierDéplacementCollisionPersonnage(Vector3 positionVouluePersonnage, int indice)
        {
            BoundingSphere test = new BoundingSphere(positionVouluePersonnage, 2);
            Vector3 anciennePosition = PersonnageChoisi.Position;
            Vector3 positionVérifiéeFinale = positionVouluePersonnage;

            foreach (Personnage perso in _managerNetwork.JoueurLocal.Personnages)
            {
                PersonnageChoisi.GérerPositionObjet(positionVouluePersonnage);
                if (Vector3.Distance(PersonnageChoisi.Position, perso.Position) < 10)
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
                if (Vector3.Distance(PersonnageChoisi.Position, perso.Position) < 10)
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

        public Vector3 VérifierDéplacementMAX(Vector3 positionVoulue, Vector3 positionInitiale, float déplacementMax)
        {
            Vector3 positionVérifiée;
            if (Vector3.Distance(positionInitiale, positionVoulue) <= déplacementMax)
            {
                positionVérifiée = positionVoulue;
            }
            else
            {
                positionVérifiée = déplacementMax * Vector3.Normalize(positionVoulue - positionInitiale) + positionInitiale;
            }
            return positionVérifiée;
        }
        #endregion

        #region Méthodes vérificationÉtatsSouris et clavier

        public bool EstClavierActivé
        {
            get { return NouvellesTouches.Length > 0; }
        }

        public bool EstEnfoncée(Keys touche)
        {
            return ÉtatClavier.IsKeyDown(touche);
        }

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

        void ActualiserÉtatSouris()
        {
            AncienÉtatSouris = NouvelÉtatSouris;
            NouvelÉtatSouris = Mouse.GetState();
        }

        public bool EstSourisActive
        {
            get { return Game.IsMouseVisible; }
        }

        public bool EstAncienClicDroit()
        {
            return NouvelÉtatSouris.RightButton == ButtonState.Pressed &&
                   AncienÉtatSouris.RightButton == ButtonState.Pressed;
        }

        public bool EstAncienClicGauche()
        {
            return NouvelÉtatSouris.LeftButton == ButtonState.Pressed && AncienÉtatSouris.LeftButton == ButtonState.Pressed;
        }

        public bool EstNouveauClicDroit()
        {
            return NouvelÉtatSouris.RightButton == ButtonState.Pressed && AncienÉtatSouris.RightButton == ButtonState.Released;
        }

        public bool EstNouveauClicGauche()
        {
            return NouvelÉtatSouris.LeftButton == ButtonState.Pressed &&
                   AncienÉtatSouris.LeftButton == ButtonState.Released;
        }

        public bool EstReleasedClicGauche()
        {
            return NouvelÉtatSouris.LeftButton == ButtonState.Released;
        }

        public Point GetPositionSouris()
        {
            return new Point(NouvelÉtatSouris.X, NouvelÉtatSouris.Y);
        }

        public int GetAncienScrollWheelValue()
        {
            return AncienÉtatSouris.ScrollWheelValue;
        }

        public int GetNouveauScrollWheelValue()
        {
            return NouvelÉtatSouris.ScrollWheelValue;
        }
        #endregion

        #region Ray et PositionSouris3D
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
        public Ray CalculateCursorRay()
        {
            Vector3 nearScreenPoint = new Vector3(NouvelÉtatSouris.X, NouvelÉtatSouris.Y, 0);
            Vector3 farScreenPoint = new Vector3(NouvelÉtatSouris.X, NouvelÉtatSouris.Y, 1);
            Vector3 nearWorldPoint = Game.GraphicsDevice.Viewport.Unproject(nearScreenPoint, Cam.Projection, Cam.Vue, Matrix.Identity);
            Vector3 farWorldPoint = Game.GraphicsDevice.Viewport.Unproject(farScreenPoint, Cam.Projection, Cam.Vue, Matrix.Identity);

            Vector3 direction = farWorldPoint - nearWorldPoint;

            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearWorldPoint, direction);
        }
        #endregion
    }
}