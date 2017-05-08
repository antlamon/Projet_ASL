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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TourManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        const int D�PLACEMENT_MAX = 10;
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;

        public DialogueActions BoutonsActions { get; private set; }
        ManagerNetwork NetworkManager { get; set; }
        InputManager GestionnaireInput { get; set; }
        Player JoueurLocal { get; set; }
        Player JoueurEnnemi { get; set; }
        Personnage PersonnageActif { get; set; }
        int ancienIndicePersonnage { get; set; }
        int IndicePersonnage { get; set; }
        float D�placementRestant { get; set; }
        float ancienD�placementRestant { get; set; }
        bool PeutAttaquer { get; set; }
        int TempsDepuisDernierUpdate { get; set; }
        AOE ZoneDEffet { get; set; }
        AOE Port�e { get; set; }
        AOE ZoneD�placement { get; set; }
        Jeu Jeu { get; set; }
        List<Personnage> Cibles { get; set; }
        bool TourTermin� { get; set; }

        public TourManager(Jeu jeu, ManagerNetwork networkManager)
            : base(jeu)
        {
            NetworkManager = networkManager;
            BoutonsActions = new DialogueActions(jeu, new Vector2(Game.Window.ClientBounds.Width / 3f, Game.Window.ClientBounds.Height / 7f), NetworkManager);
            Game.Components.Add(BoutonsActions);
            Jeu = jeu;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            JoueurLocal = NetworkManager.JoueurLocal;
            JoueurEnnemi = NetworkManager.JoueurEnnemi;
            Cibles = new List<Personnage>();
            ancienIndicePersonnage = -1;
            IndicePersonnage = 0;
            TempsDepuisDernierUpdate = 0;
            D�placementRestant = D�PLACEMENT_MAX;
            Cr�erBtnClasses();
            PersonnageActif = JoueurLocal.Personnages[IndicePersonnage];
            BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
            BoutonsActions.VoirBoutonAction(NetworkManager.TourActif);
            ZoneDEffet = Jeu.AOE1;
            Port�e = Jeu.AOE2;
            ZoneD�placement = Jeu.AOE3;
            TourTermin� = false;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            GestionnaireInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            base.LoadContent();
        }

        private void Cr�erBtnClasses()
        {
            foreach (Personnage p in JoueurLocal.Personnages)
            {
                switch (p.GetType().ToString())
                {
                    case TypePersonnage.ARCHER:
                        BoutonsActions.Cr�erBtnArcher();
                        break;
                    case TypePersonnage.GU�RISSEUR:
                        BoutonsActions.Cr�erBtnGu�risseur();
                        break;
                    case TypePersonnage.GUERRIER:
                        BoutonsActions.Cr�erBtnGuerrier();
                        break;
                    case TypePersonnage.MAGE:
                        BoutonsActions.Cr�erBtnMage();
                        break;
                    case TypePersonnage.PALADIN:
                        BoutonsActions.Cr�erBtnPaladin();
                        break;
                    case TypePersonnage.VOLEUR:
                        BoutonsActions.Cr�erBtnVoleur();
                        break;
                }
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
                V�rifierD�butDeTour(gameTime);
                if (!TourTermin�)
                {
                    V�rifierD�placement();
                    V�rifierAttaqueEtSorts();
                    V�rifierFinDeTour(gameTime);
                }
        }

        void V�rifierD�butDeTour(GameTime gameTime)
        {
            if (ancienIndicePersonnage != IndicePersonnage)// && gameTime.TotalGameTime.Seconds - TempsDepuisDernierUpdate > 0.1f)
            {
                PersonnageActif = JoueurLocal.Personnages[IndicePersonnage];
                V�rifier�tatsSp�ciaux();
                if (!JoueurLocal.Personnages[IndicePersonnage].EstMort && !JoueurLocal.Personnages[IndicePersonnage]._Frozen)
                {
                    TourTermin� = false;
                    BoutonsActions.VoirBoutonAction(true);
                    PeutAttaquer = true;
                    ActiverAttaque();
                    ZoneD�placement.Changer�tendueEtPosition(new Vector2(D�placementRestant * 2), PersonnageActif.Position�);
                    ancienIndicePersonnage = IndicePersonnage;
                }
                else
                {

                    if (JoueurLocal.Personnages[IndicePersonnage]._Frozen)
                    {
                        NetworkManager.Send�tatsSp�ciaux(JoueurLocal.Personnages[IndicePersonnage], true, new List<string>() { �tatSp�cial.FREEZE }, new List<bool>() { false });
                        NetworkManager.SendFinDeTour();
                    }
                    IndicePersonnage = IndicePersonnage < JoueurLocal.Personnages.Count - 1 ? IndicePersonnage + 1 : 0;
                    BoutonsActions.R�initialiserDialogueActions(JoueurLocal.Personnages[IndicePersonnage]);
                    BoutonsActions.VoirBoutonAction(false);
                }
            }
        }

        void V�rifier�tatsSp�ciaux()
        {
            if (PersonnageActif._EnFeu)
            {
                NetworkManager.SendD�g�t(new List<Personnage>() { PersonnageActif }, Mage.D�GATS_TICK_BRASIER, true);
                --PersonnageActif.CptEnFeu;
                if (PersonnageActif.CptEnFeu == 0)
                {
                    NetworkManager.Send�tatsSp�ciaux(PersonnageActif, true, new List<string>() { �tatSp�cial.EN_FEU }, new List<bool>() { false });
                }
            }
            if (PersonnageActif is Guerrier && (PersonnageActif as Guerrier)._Folie)
            {
                --(PersonnageActif as Guerrier).CptFolie;
                if ((PersonnageActif as Guerrier).CptFolie == 0)
                {
                    NetworkManager.Send�tatsSp�ciaux(PersonnageActif, true, new List<string>() { �tatSp�cial.FOLIE }, new List<bool>() { false });
                    NetworkManager.SendD�g�t(new List<Personnage>() { PersonnageActif }, PersonnageActif.PtsDeVie, true);
                    PersonnageActif.ChangerVitalit�(0);
                }
            }
            if (PersonnageActif is Voleur)
            {
                NetworkManager.SendInvisibilit�(false);
            }
        }

        void V�rifierD�placement()
        {
            if (!BoutonsActions.�tatSort1 && !BoutonsActions.�tatSort2 && !BoutonsActions.�tatAttaquer && D�placementRestant >= 0.5f)
            {
                ZoneD�placement.Visible = true;
                GestionnaireInput.D�terminerS�lectionPersonnageD�placement(IndicePersonnage);
                D�placementRestant = GestionnaireInput.D�terminerMouvementPersonnageS�lectionn�(D�placementRestant, IndicePersonnage);
                D�placerZoneMouvement();
            }
            else
            {
                ZoneD�placement.Visible = false;
            }
        }

        void D�placerZoneMouvement()
        {
            if (ancienD�placementRestant != D�placementRestant)
            {
                ZoneD�placement.Changer�tendueEtPosition(new Vector2(D�placementRestant * 2), PersonnageActif.Position�);
                ancienD�placementRestant = D�placementRestant;
            }
        }

        void V�rifierAttaqueEtSorts()
        {
            if (PeutAttaquer)
            {
                Vector3 positionV�rifi�e;
                Personnage singleTarget�Attaquer = null;
                int d�gats = 0;

                if (BoutonsActions.�tatSort1)
                {
                    bool ciblealli�e = false;
                    //GestionnaireInput.Update(gameTime);
                    switch (PersonnageActif.GetType().ToString())
                    {
                        case TypePersonnage.ARCHER:
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Archer.PORT�E_PLUIE_DE_FL�CHES - Archer.RAYON_PLUIE_DE_FL�CHES);
                            ZoneDEffet.Changer�tendueEtPosition(new Vector2(Archer.RAYON_PLUIE_DE_FL�CHES * 2), positionV�rifi�e);
                            Port�e.Changer�tendueEtPosition(new Vector2(Archer.PORT�E_PLUIE_DE_FL�CHES * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Port�e.Visible = true;
                            if (GestionnaireInput.EstNouveauClicDroit())
                            {
                                Cibles = (PersonnageActif as Archer).PluieDeFl�ches(positionV�rifi�e, JoueurEnnemi.Personnages, out d�gats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                goto default;
                            }
                            break;
                        case TypePersonnage.GU�RISSEUR:
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Gu�risseur.PORT�E_SOIN_DE_ZONE - Gu�risseur.RAYON_SOIN_DE_ZONE);
                            ZoneDEffet.Changer�tendueEtPosition(new Vector2(Gu�risseur.RAYON_SOIN_DE_ZONE * 2), positionV�rifi�e);
                            Port�e.Changer�tendueEtPosition(new Vector2(Gu�risseur.PORT�E_SOIN_DE_ZONE * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Port�e.Visible = true;
                            if (GestionnaireInput.EstNouveauClicDroit())
                            {
                                if((PersonnageActif as Gu�risseur)._SatanMode)
                                {
                                    Cibles = (PersonnageActif as Gu�risseur).SoinDeZone(positionV�rifi�e, JoueurEnnemi.Personnages, out d�gats);
                                    ciblealli�e = false;
                                }
                                else
                                {
                                    Cibles = (PersonnageActif as Gu�risseur).SoinDeZone(positionV�rifi�e, JoueurLocal.Personnages, out d�gats);
                                    ciblealli�e = true;
                                }
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                goto default;
                            }
                            break;
                        case TypePersonnage.GUERRIER:
                            Port�e.Changer�tendueEtPosition(new Vector2(Guerrier.PORT�E_TORNADE_FURIEUSE * 2), PersonnageActif.Position);
                            Port�e.Visible = true;
                            if (GestionnaireInput.EstNouveauClicDroit())
                            {
                                Cibles = (PersonnageActif as Guerrier).TornadeFurieuse(PersonnageActif.Position, JoueurEnnemi.Personnages, out d�gats);
                                PeutAttaquer = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                goto default;
                            }
                            break;
                        case TypePersonnage.MAGE:
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Mage.PORT�E_BRASIER - Mage.RAYON_BRASIER);
                            ZoneDEffet.Changer�tendueEtPosition(new Vector2(Mage.RAYON_BRASIER * 2), positionV�rifi�e);
                            Port�e.Changer�tendueEtPosition(new Vector2(Mage.PORT�E_BRASIER * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Port�e.Visible = true;
                            if (GestionnaireInput.EstNouveauClicDroit())
                            {
                                Cibles = (PersonnageActif as Mage).Brasier(positionV�rifi�e, JoueurEnnemi.Personnages, out d�gats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                foreach (Personnage cible in Cibles)
                                {
                                    NetworkManager.Send�tatsSp�ciaux(cible, ciblealli�e, new List<string>() { �tatSp�cial.EN_FEU }, new List<bool> { true });
                                }
                                goto default;
                            }
                            break;
                        case TypePersonnage.PALADIN:
                            Port�e.Changer�tendueEtPosition(new Vector2(Paladin.PORT�E_CLARIT� * 2), PersonnageActif.Position);
                            Port�e.Visible = true;
                            singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer(JoueurLocal.Personnages);
                            if (singleTarget�Attaquer != null && (int)(singleTarget�Attaquer.Position - PersonnageActif.Position).Length() <= Paladin.PORT�E_CLARIT�)
                            {
                                ciblealli�e = true;
                                PeutAttaquer = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                List<string> noms�tats = new List<string>() { �tatSp�cial.EN_FEU, �tatSp�cial.FREEZE };
                                List<bool> valeurs�tats = new List<bool>() { false, false };
                                if (singleTarget�Attaquer is Guerrier)
                                {
                                    noms�tats.Add(�tatSp�cial.FOLIE);
                                    valeurs�tats.Add(false);
                                }
                                NetworkManager.Send�tatsSp�ciaux(singleTarget�Attaquer, ciblealli�e, noms�tats, valeurs�tats);
                            }
                            break;
                        case TypePersonnage.VOLEUR:
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Voleur.PORT�E_INVISIBILIT�);
                            ZoneDEffet.Changer�tendueEtPosition(new Vector2(2), positionV�rifi�e);
                            Port�e.Changer�tendueEtPosition(new Vector2(Voleur.PORT�E_INVISIBILIT� * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Port�e.Visible = true;
                            if (GestionnaireInput.EstNouveauClicDroit())
                            {
                                NetworkManager.SendNewPosition(positionV�rifi�e, JoueurLocal.Personnages.FindIndex(p => p is Voleur));
                                ZoneD�placement.Changer�tendueEtPosition(new Vector2(D�placementRestant * 2), positionV�rifi�e);
                                NetworkManager.SendInvisibilit�(true);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        default:
                            NetworkManager.SendD�g�t(Cibles.FindAll(cible => !cible.EstMort), d�gats, ciblealli�e);
                            break;
                    }
                }
                if (BoutonsActions.�tatSort2)
                {
                    bool ciblealli�e = false;
                    //GestionnaireInput.Update(gameTime);
                    switch (PersonnageActif.GetType().ToString())
                    {
                        case TypePersonnage.ARCHER:
                            positionV�rifi�e = GestionnaireInput.V�rifierD�placementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Archer.PORT�E_FL�CHE_REBONDISSANTE);
                            Port�e.Changer�tendueEtPosition(new Vector2(Archer.PORT�E_FL�CHE_REBONDISSANTE * 2), PersonnageActif.Position);
                            ZoneDEffet.Changer�tendueEtPosition(new Vector2(Archer.RAYON_FL�CHE_REBONDISSANTE * 2), positionV�rifi�e);
                            Port�e.Visible = true;
                            ZoneDEffet.Visible = true;
                            singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer(JoueurEnnemi.Personnages);
                            if (singleTarget�Attaquer != null && (int)(singleTarget�Attaquer.Position - PersonnageActif.Position).Length() <= Archer.PORT�E_FL�CHE_REBONDISSANTE)
                            {
                                Cibles.Add(singleTarget�Attaquer);
                                Personnage deuxi�meCible = (PersonnageActif as Archer).Fl�cheRebondissante(singleTarget�Attaquer, JoueurEnnemi.Personnages.FindAll(p => p != singleTarget�Attaquer && !p.EstMort), out d�gats);
                                if (deuxi�meCible != null)
                                {
                                    Cibles.Add(deuxi�meCible);
                                }
                                PeutAttaquer = false;
                                Port�e.Visible = false;
                                ZoneDEffet.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                goto default;
                            }
                            break;
                        case TypePersonnage.GU�RISSEUR:
                            Port�e.Changer�tendueEtPosition(new Vector2(Gu�risseur.PORT�E_RESURRECT * 2), PersonnageActif.Position);
                            Port�e.Visible = true;
                            if ((PersonnageActif as Gu�risseur)._SatanMode)
                            {
                                singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer(JoueurEnnemi.Personnages);
                                if (singleTarget�Attaquer != null && (int)(singleTarget�Attaquer.Position - PersonnageActif.Position).Length() <= Gu�risseur.PORT�E_RESURRECT)
                                {
                                    int vieVol�e;
                                    d�gats = (PersonnageActif as Gu�risseur).VolDeVie(out vieVol�e);
                                    ciblealli�e = true;
                                    Cibles.Add(PersonnageActif);
                                    NetworkManager.SendD�g�t(Cibles, vieVol�e, ciblealli�e);
                                    Cibles.Clear();

                                    Cibles.Add(singleTarget�Attaquer);
                                    ciblealli�e = false;
                                    PeutAttaquer = false;
                                    Port�e.Visible = false;
                                    BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                    goto default;
                                }
                            }
                            else
                            {
                                singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer(JoueurLocal.Personnages);
                                if (singleTarget�Attaquer != null && (int)(singleTarget�Attaquer.Position - PersonnageActif.Position).Length() <= Gu�risseur.PORT�E_RESURRECT && singleTarget�Attaquer.EstMort)
                                {
                                    d�gats = (PersonnageActif as Gu�risseur).R�surrection(singleTarget�Attaquer);
                                    Cibles.Add(singleTarget�Attaquer);
                                    ciblealli�e = true;
                                    PeutAttaquer = false;
                                    Port�e.Visible = false;
                                    BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                    NetworkManager.Send�tatsSp�ciaux(PersonnageActif, ciblealli�e, new List<string>() { �tatSp�cial.SATAN }, new List<bool>() { true });
                                    NetworkManager.SendD�g�t(Cibles, d�gats, ciblealli�e);
                                }
                            }
                            break;
                        case TypePersonnage.GUERRIER:
                            if (GestionnaireInput.EstNouveauClicDroit())
                            {
                                ciblealli�e = true;
                                PeutAttaquer = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                NetworkManager.Send�tatsSp�ciaux(PersonnageActif, ciblealli�e, new List<string>() { �tatSp�cial.FOLIE }, new List<bool>() { true });
                            }
                            break;
                        case TypePersonnage.MAGE:
                            Port�e.Changer�tendueEtPosition(new Vector2(Mage.PORT�E_FREEZE_DONT_MOVE * 2), PersonnageActif.Position);
                            Port�e.Visible = true;
                            singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer(JoueurEnnemi.Personnages);
                            if (singleTarget�Attaquer != null && (int)(singleTarget�Attaquer.Position - PersonnageActif.Position).Length() <= Mage.PORT�E_FREEZE_DONT_MOVE)
                            {
                                d�gats = (PersonnageActif as Mage).FreezeDontMove(singleTarget�Attaquer);
                                Cibles.Add(singleTarget�Attaquer);
                                PeutAttaquer = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                NetworkManager.Send�tatsSp�ciaux(singleTarget�Attaquer, ciblealli�e, new List<string>() { �tatSp�cial.FREEZE }, new List<bool>() { true });
                                goto default;
                            }
                            break;
                        case TypePersonnage.PALADIN:
                            Port�e.Changer�tendueEtPosition(new Vector2(Paladin.PORT�E_BOUCLIER_DIVIN * 2), PersonnageActif.Position);
                            Port�e.Visible = true;
                            singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer(JoueurLocal.Personnages);
                            if (singleTarget�Attaquer != null && (int)(singleTarget�Attaquer.Position - PersonnageActif.Position).Length() <= Paladin.PORT�E_BOUCLIER_DIVIN)
                            {
                                ciblealli�e = true;
                                PeutAttaquer = false;
                                Port�e.Visible = false;
                                BoutonsActions.R�initialiserDialogueActions(PersonnageActif);
                                NetworkManager.Send�tatsSp�ciaux(singleTarget�Attaquer, ciblealli�e, new List<string>() { �tatSp�cial.BOUCLIER_DIVIN }, new List<bool>() { true });
                            }
                            break;
                        case TypePersonnage.VOLEUR:
                            Port�e.Changer�tendueEtPosition(new Vector2(Voleur.PORT�E_LANCER_COUTEAU * 2), PersonnageActif.Position);
                            Port�e.Visible = true;
                            singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer(JoueurEnnemi.Personnages);
                            if (singleTarget�Attaquer != null && (int)(singleTarget�Attaquer.Position - PersonnageActif.Position).Length() <= Voleur.PORT�E_LANCER_COUTEAU)
                            {
                                d�gats = (PersonnageActif as Voleur).LancerCouteau();
                                Cibles.Add(singleTarget�Attaquer);
                                ciblealli�e = false;
                                Port�e.Visible = false;
                                PeutAttaquer = false;
                                goto default;
                            }
                            break;
                        default:
                            NetworkManager.SendD�g�t(Cibles.FindAll(cible => !cible.EstMort), d�gats, ciblealli�e);
                            break;
                    }
                }
                if (BoutonsActions.�tatAttaquer)
                {
                    //GestionnaireInput.Update(gameTime);  plus n�cessaire puisqu'on utilise de clic droit

                    bool ciblealli�e = false;
                    Port�e.Changer�tendueEtPosition(new Vector2(PersonnageActif.GetPort�eAttaque() * 2), PersonnageActif.Position);
                    Port�e.Visible = true;
                    if (PersonnageActif is Gu�risseur && !(PersonnageActif as Gu�risseur)._SatanMode)
                    {
                        singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer(JoueurLocal.Personnages);
                        ciblealli�e = true;
                    }
                    else
                    {
                        singleTarget�Attaquer = GestionnaireInput.D�terminerS�lectionPersonnage�Attaquer(JoueurEnnemi.Personnages);
                    }
                    if (singleTarget�Attaquer != null && (int)(singleTarget�Attaquer.Position - PersonnageActif.Position).Length() <= PersonnageActif.GetPort�eAttaque())
                    {
                        Cibles.Add(singleTarget�Attaquer);
                        Port�e.Visible = false;
                        PeutAttaquer = false;
                        BoutonsActions.Attaquer();
                        d�gats = PersonnageActif.Attaquer();
                        NetworkManager.SendD�g�t(Cibles.FindAll(cible => !cible.EstMort), d�gats, ciblealli�e);
                    }
                }
                if (!BoutonsActions.�tatAttaquer && !BoutonsActions.�tatSort1 && !BoutonsActions.�tatSort2)
                {
                    ZoneDEffet.Visible = false;
                    Port�e.Visible = false;
                }
                ActiverAttaque();
            }
        }

        void ActiverAttaque()
        {
            BoutonsActions.BtnSorts.EstActif = PeutAttaquer;
            BoutonsActions.BtnAttaquer.EstActif = PeutAttaquer;
        }

        void V�rifierFinDeTour(GameTime gameTime)
        {
            if (TourFini())
            {
                TerminerLeTour();
                TempsDepuisDernierUpdate = gameTime.TotalGameTime.Seconds;
            }
        }

        bool TourFini()
        {
            return BoutonsActions.�tatPasserTour || !PeutAttaquer && (int)Math.Round(D�placementRestant) <= 0;
        }

        void TerminerLeTour()
        {
            NetworkManager.SendFinDeTour();
            IndicePersonnage = IndicePersonnage < JoueurLocal.Personnages.Count - 1 ? IndicePersonnage + 1 : 0;
            // Voir avec PersonnageActif...
            BoutonsActions.R�initialiserDialogueActions(JoueurLocal.Personnages[IndicePersonnage]);
            Cibles.Clear();
            D�placementRestant = D�PLACEMENT_MAX;
            BoutonsActions.VoirBoutonAction(false);
            ZoneDEffet.Visible = false;
            Port�e.Visible = false;
            ZoneD�placement.Visible = false;
            TourTermin� = true;
        }
    }
}
