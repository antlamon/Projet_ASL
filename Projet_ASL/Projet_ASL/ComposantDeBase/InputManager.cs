using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;


namespace Projet_ASL
{
    //Déplacement max (cercle, plus petit rayon chaque fois)
    //distance parcourue max 
    //juste bouger personnage sélectionné (tourmanager a index)
    //méthode mettre dans tourmanager
    // déplacement se fait dans tour manager

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
            //if (_managerNetwork.Players.Count != 0)
            //{
            //    if (!PersonnageSélectionné)
            //    {
            //        DéterminerSélectionPersonnage();
            //        DéterminerMouvementPersonnageSélectionné();
            //    }
            //    else
            //    {
            //        DéterminerMouvementPersonnageSélectionné();
            //    }
            //}
        }
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
            Personnage perso = _managerNetwork.JoueurLocal.Personnages[indice];
            DistanceRayon = perso.SphèreDeCollision.Intersects(ray);
            if (DistanceRayon != null)
            {
                PersonnageChoisi = perso;
                rep = true;
            }
            return rep;
        }

        public void DéterminerSélectionPersonnage()
        {
            if (EstNouveauClicGauche())
            {
                DéterminerIntersectionPersonnageRay();
                if (PersonnageChoisi != null)
                {
                    PersonnageSélectionné = true;
                    PositionInitialePersonnage = PersonnageChoisi.Position;
                }
            }
        }
        public void DéterminerIntersectionPersonnageRay()
        {
            Ray ray = CalculateCursorRay();
            float closestDistance = float.MaxValue;
            //foreach(Player p in _managerNetwork.Players)
            //{
            foreach (Personnage perso in _managerNetwork.JoueurLocal.Personnages)
            {
                DistanceRayon = perso.SphèreDeCollision.Intersects(ray);
                if (DistanceRayon != null && DistanceRayon < closestDistance)
                {
                    closestDistance = (float)DistanceRayon;
                    PersonnageChoisi = perso;
                }

            }
            //}
        }

        public float DéterminerMouvementPersonnageSélectionné(float déplacement_maximal, int indice)
        {
            float déplacement_restant = déplacement_maximal;
            if (PersonnageSélectionné)
            {
                if (EstAncienClicGauche())
                {
                    Vector3 positionVouluePersonnage = GetPositionSourisPlan();
                    //Vector3 Déplacement = Vector3.Subtract(PositionVouluePersonnage, PersonnageChoisi.Position);
                    //PersonnageChoisi.Bouger(Déplacement);
                    //envoyer nouvelle position au serveur
                    positionVouluePersonnage = VérifierDéplacementMAX(positionVouluePersonnage, PositionInitialePersonnage, déplacement_maximal);
                    positionVouluePersonnage = VérifierDéplacementCollisionPersonnage(positionVouluePersonnage, indice);
                    //positionVouluePersonnage = VérifierDéplacementMAX(positionVouluePersonnage, PositionInitialePersonnage, déplacement_maximal);
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
            //position future est ce que va intersect averc sphere
            // si oui bouge pu
            Vector3 positionVérifiée = positionVouluePersonnage;
            Vector3 positionDuPersonnageAvant = PersonnageChoisi.Position;
            foreach (Player p in _managerNetwork.Players)
            {
                foreach (Personnage perso in _managerNetwork.JoueurLocal.Personnages)
                {
                    if (Vector3.Distance(PersonnageChoisi.Position, perso.Position) < 3)
                    {
                        PersonnageChoisi.GérerPositionObjet(positionVouluePersonnage);
                        if (PersonnageChoisi.SphèreDeCollision.Contains(perso.SphèreDeCollision) != ContainmentType.Disjoint)
                        {
                            if (perso != _managerNetwork.JoueurLocal.Personnages[indice])
                            {
                                //une sorte
                                //CorrectCollisions(ref PersonnageChoisi, ref perso);

                                //i try
                                //positionVérifiée = CorrigerCollision(PersonnageChoisi, perso, positionVouluePersonnage);

                                //ancien code perso bouge pu
                                positionVérifiée = positionDuPersonnageAvant;

                                //une autre sorte
                                //positionVérifiée = new Vector3(
                                //CheckCollision(PersonnageChoisi.Position + new Vector3(positionVouluePersonnage.X, 0, 0), perso) ? PersonnageChoisi.Position.X : positionVouluePersonnage.X,
                                //CheckCollision(PersonnageChoisi.Position + new Vector3(0, positionVouluePersonnage.Y, 0), perso) ? PersonnageChoisi.Position.Y : positionVouluePersonnage.Y,
                                //CheckCollision(PersonnageChoisi.Position + new Vector3(0, 0, positionVouluePersonnage.Z), perso) ? PersonnageChoisi.Position.X : positionVouluePersonnage.Z);
                            }
                        }
                    }
                }
            }
            PersonnageChoisi.GérerPositionObjet(positionDuPersonnageAvant);
            return positionVérifiée;
        }





        private static Vector3 CorrigerCollision(Personnage personnageBougé, Personnage personnageImmobile, Vector3 positionVoulue)
        {
            Vector3 positionVérifiée;
            float distanceEntreLesDeux = Vector3.Distance(personnageImmobile.SphèreDeCollision.Center, personnageBougé.SphèreDeCollision.Center);
            float distanceMinimale = personnageImmobile.SphèreDeCollision.Radius + personnageBougé.SphèreDeCollision.Radius;
            Vector3 directionRenvoi = personnageBougé.SphèreDeCollision.Center - personnageImmobile.SphèreDeCollision.Center;
            directionRenvoi.Normalize();
            positionVérifiée = personnageBougé.Position + directionRenvoi;
            //for (int i = 0; i < c1.Modèle.Meshes.Count; i++)
            //{
            //    // Check whether the bounding boxes of the two cubes intersect.
            //    BoundingSphere c1BoundingSphere = c1.Modèle.Meshes[i].BoundingSphere;

            //    c1BoundingSphere.Center += c1.Position + new Vector3(2, 0, 2);
            //    c1BoundingSphere.Radius = c1BoundingSphere.Radius / 1.5f;

            //    for (int j = 0; j < c2.Modèle.Meshes.Count; j++)
            //    {
            //        BoundingSphere c2BoundingSphere = c2.Modèle.Meshes[j].BoundingSphere;
            //        c2BoundingSphere.Center += c2.Position;

            //        Vector3 dir = c2BoundingSphere.Center - c1BoundingSphere.Center;
            //        float center_dist_sq = Vector3.Dot(dir).dot(dir);
            //        float min_dist = c2BoundingSphere.Radius + c1BoundingSphere.Radius;
            //        if (center_dist_sq < min_dist * min_dist)
            //        {
            //            dir.Normalize();
            //            positionVérifiée = c1.Position + dir * (float)(min_dist - Math.Sqrt(center_dist_sq));
            //        }
            //    }
            //}
            return positionVérifiée;
        }


        //private static bool CorrectCollisions(ref Personnage c1, ref Personnage c2)
        //{
        //    for (int i = 0; i < c1.Modèle.Meshes.Count; i++)
        //    {
        //        // Check whether the bounding boxes of the two cubes intersect.
        //        BoundingSphere c1BoundingSphere = c1.Modèle.Meshes[i].BoundingSphere;

        //        c1BoundingSphere.Center += c1.Position + new Vector3(2, 0, 2);
        //        c1BoundingSphere.Radius = c1BoundingSphere.Radius / 1.5f;

        //        for (int j = 0; j < c2.Modèle.Meshes.Count; j++)
        //        {
        //            BoundingSphere c2BoundingSphere = c2.Modèle.Meshes[j].BoundingSphere;
        //            c2BoundingSphere.Center += c2.Position;

        //            Vector3 dir = c2BoundingSphere.Center - c1BoundingSphere.Center;
        //            float center_dist_sq = Vector3.Dot(dir).dot(dir);
        //            float min_dist = c2BoundingSphere.Radius + c1BoundingSphere.Radius;
        //            if (center_dist_sq < min_dist * min_dist)
        //            {
        //                dir.Normalize();
        //                c2.Position += dir * (float) (min_dist - Math.Sqrt(center_dist_sq));
        //            }
        //        }
        //    }
        //    return false;
        //}


        public Boolean CheckCollision(Vector3 positionVoulue, Personnage persoCollisioné)
        {
            return persoCollisioné.SphèreDeCollision.Contains(PersonnageChoisi.SphèreDeCollision) != ContainmentType.Disjoint;
        }

        //private Vector3 previewMove(Vector3 amount)
        //{
        //    // Create a rotate matrix
        //    Matrix rotate = Matrix.CreateRotationY(CameraRotation.Y);
        //    // Create a movement vector
        //    Vector3 movement = new Vector3(amount.X, amount.Y, amount.Z);
        //    movement = Vector3.Transform(movement, rotate);
        //    // Return the value of camera position + movement vector

        //    return CameraPosition + new Vector3(
        //        Collision.CheckCollision(CameraPosition + new Vector3(movement.X, 0, 0)) ? 0 : movement.X,
        //        Collision.CheckCollision(CameraPosition + new Vector3(0, movement.Y, 0)) ? 0 : movement.Y,
        //        Collision.CheckCollision(CameraPosition + new Vector3(0, 0, movement.Z)) ? 0 : movement.Z);

        //}


        public Vector3 VérifierDéplacementMAX(Vector3 positionVouluePersonnage, Vector3 positionInitiale, float déplacementMax)
        {
            Vector3 positionVérifiée;
            if (Vector3.Distance(positionInitiale, positionVouluePersonnage) <= déplacementMax)
            {
                positionVérifiée = positionVouluePersonnage;
            }
            else
            {
                positionVérifiée = déplacementMax * Vector3.Normalize(positionVouluePersonnage - positionInitiale) + positionInitiale;
            }
            return positionVérifiée;
        }

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
    }
}