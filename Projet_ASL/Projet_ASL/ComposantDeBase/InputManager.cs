﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;


namespace Projet_ASL
{
    public class InputManager : Microsoft.Xna.Framework.GameComponent
    {
        private ManagerNetwork _managerNetwork;
        Keys[] AnciennesTouches { get; set; }
        Keys[] NouvellesTouches { get; set; }
        KeyboardState ÉtatClavier { get; set; }
        MouseState AncienÉtatSouris { get; set; }
        MouseState NouvelÉtatSouris { get; set; }
        Caméra Cam { get; set; }

        bool PersonnageSélectionné { get; set; }
        float? DistanceRayon { get; set; }
        Personnage PersonnageChoisi { get; set; }

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
            DéterminerSélectionPersonnage();
            DéterminerMouvementPersonnageSélectionné();
        }

        public void DéterminerSélectionPersonnage()
        {
            if (EstNouveauClicGauche())
            {
                DéterminerIntersectionPersonnageRay();
                if (PersonnageChoisi != null)
                {
                    //envoyer au serveur quel personnage est sélectionné?
                    PersonnageSélectionné = true;
                    Console.WriteLine("{0} sélectionné", PersonnageChoisi);
                }
            }
        }
        public void DéterminerIntersectionPersonnageRay()
        {

            Ray ray = CalculateCursorRay();
            float closestDistance = float.MaxValue;
            foreach(Player p in _managerNetwork.Players)
            {
                foreach(Personnage perso in p.Personnages)
                {
                    DistanceRayon = perso.SphèreDeCollision.Intersects(ray);
                    if (DistanceRayon != null && DistanceRayon < closestDistance)
                    {
                        closestDistance = (float)DistanceRayon;
                        PersonnageChoisi = perso;
                    }
                }
            }
        }

        public void DéterminerMouvementPersonnageSélectionné()
        {
            if(PersonnageSélectionné)
            {
                if(EstAncienClicGauche())
                {
                    Vector3 PositionVouluePersonnage = GetPositionSourisPlan();
                    Vector3 Déplacement = Vector3.Subtract(PositionVouluePersonnage, PersonnageChoisi.Position);
                    PersonnageChoisi.BougerPersonnage(Déplacement);
                }
            }
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