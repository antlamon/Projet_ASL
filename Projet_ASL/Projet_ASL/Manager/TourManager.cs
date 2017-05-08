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
        const int DÉPLACEMENT_MAX = 10;
        const float INTERVALLE_MAJ_STANDARD = 1f / 60f;

        public DialogueActions BoutonsActions { get; private set; }
        ManagerNetwork NetworkManager { get; set; }
        InputManager GestionnaireInput { get; set; }
        Player JoueurLocal { get; set; }
        Player JoueurEnnemi { get; set; }
        Personnage PersonnageActif { get; set; }
        int ancienIndicePersonnage { get; set; }
        int IndicePersonnage { get; set; }
        float DéplacementRestant { get; set; }
        float ancienDéplacementRestant { get; set; }
        bool PeutAttaquer { get; set; }
        int TempsDepuisDernierUpdate { get; set; }
        AOE ZoneDEffet { get; set; }
        AOE Portée { get; set; }
        AOE ZoneDéplacement { get; set; }
        Jeu Jeu { get; set; }
        List<Personnage> Cibles { get; set; }
        bool TourTerminé { get; set; }

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
            DéplacementRestant = DÉPLACEMENT_MAX;
            CréerBtnClasses();
            PersonnageActif = JoueurLocal.Personnages[IndicePersonnage];
            BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
            BoutonsActions.VoirBoutonAction(NetworkManager.TourActif);
            ZoneDEffet = Jeu.AOE1;
            Portée = Jeu.AOE2;
            ZoneDéplacement = Jeu.AOE3;
            TourTerminé = false;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            GestionnaireInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            base.LoadContent();
        }

        private void CréerBtnClasses()
        {
            foreach (Personnage p in JoueurLocal.Personnages)
            {
                switch (p.GetType().ToString())
                {
                    case TypePersonnage.ARCHER:
                        BoutonsActions.CréerBtnArcher();
                        break;
                    case TypePersonnage.GUÉRISSEUR:
                        BoutonsActions.CréerBtnGuérisseur();
                        break;
                    case TypePersonnage.GUERRIER:
                        BoutonsActions.CréerBtnGuerrier();
                        break;
                    case TypePersonnage.MAGE:
                        BoutonsActions.CréerBtnMage();
                        break;
                    case TypePersonnage.PALADIN:
                        BoutonsActions.CréerBtnPaladin();
                        break;
                    case TypePersonnage.VOLEUR:
                        BoutonsActions.CréerBtnVoleur();
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
                VérifierDébutDeTour(gameTime);
                if (!TourTerminé)
                {
                    VérifierDéplacement();
                    VérifierAttaqueEtSorts();
                    VérifierFinDeTour(gameTime);
                }
        }

        void VérifierDébutDeTour(GameTime gameTime)
        {
            if (ancienIndicePersonnage != IndicePersonnage)// && gameTime.TotalGameTime.Seconds - TempsDepuisDernierUpdate > 0.1f)
            {
                PersonnageActif = JoueurLocal.Personnages[IndicePersonnage];
                VérifierÉtatsSpéciaux();
                if (!JoueurLocal.Personnages[IndicePersonnage].EstMort && !JoueurLocal.Personnages[IndicePersonnage]._Frozen)
                {
                    TourTerminé = false;
                    BoutonsActions.VoirBoutonAction(true);
                    PeutAttaquer = true;
                    ActiverAttaque();
                    ZoneDéplacement.ChangerÉtendueEtPosition(new Vector2(DéplacementRestant * 2), PersonnageActif.Position­);
                    ancienIndicePersonnage = IndicePersonnage;
                }
                else
                {

                    if (JoueurLocal.Personnages[IndicePersonnage]._Frozen)
                    {
                        NetworkManager.SendÉtatsSpéciaux(JoueurLocal.Personnages[IndicePersonnage], true, new List<string>() { ÉtatSpécial.FREEZE }, new List<bool>() { false });
                        NetworkManager.SendFinDeTour();
                    }
                    IndicePersonnage = IndicePersonnage < JoueurLocal.Personnages.Count - 1 ? IndicePersonnage + 1 : 0;
                    BoutonsActions.RéinitialiserDialogueActions(JoueurLocal.Personnages[IndicePersonnage]);
                    BoutonsActions.VoirBoutonAction(false);
                }
            }
        }

        void VérifierÉtatsSpéciaux()
        {
            if (PersonnageActif._EnFeu)
            {
                NetworkManager.SendDégât(new List<Personnage>() { PersonnageActif }, Mage.DÉGATS_TICK_BRASIER, true);
                --PersonnageActif.CptEnFeu;
                if (PersonnageActif.CptEnFeu == 0)
                {
                    NetworkManager.SendÉtatsSpéciaux(PersonnageActif, true, new List<string>() { ÉtatSpécial.EN_FEU }, new List<bool>() { false });
                }
            }
            if (PersonnageActif is Guerrier && (PersonnageActif as Guerrier)._Folie)
            {
                --(PersonnageActif as Guerrier).CptFolie;
                if ((PersonnageActif as Guerrier).CptFolie == 0)
                {
                    NetworkManager.SendÉtatsSpéciaux(PersonnageActif, true, new List<string>() { ÉtatSpécial.FOLIE }, new List<bool>() { false });
                    NetworkManager.SendDégât(new List<Personnage>() { PersonnageActif }, PersonnageActif.PtsDeVie, true);
                    PersonnageActif.ChangerVitalité(0);
                }
            }
            if (PersonnageActif is Voleur)
            {
                NetworkManager.SendInvisibilité(false);
            }
        }

        void VérifierDéplacement()
        {
            if (!BoutonsActions.ÉtatSort1 && !BoutonsActions.ÉtatSort2 && !BoutonsActions.ÉtatAttaquer && DéplacementRestant >= 0.5f)
            {
                ZoneDéplacement.Visible = true;
                GestionnaireInput.DéterminerSélectionPersonnageDéplacement(IndicePersonnage);
                DéplacementRestant = GestionnaireInput.DéterminerMouvementPersonnageSélectionné(DéplacementRestant, IndicePersonnage);
                DéplacerZoneMouvement();
            }
            else
            {
                ZoneDéplacement.Visible = false;
            }
        }

        void DéplacerZoneMouvement()
        {
            if (ancienDéplacementRestant != DéplacementRestant)
            {
                ZoneDéplacement.ChangerÉtendueEtPosition(new Vector2(DéplacementRestant * 2), PersonnageActif.Position­);
                ancienDéplacementRestant = DéplacementRestant;
            }
        }

        void VérifierAttaqueEtSorts()
        {
            if (PeutAttaquer)
            {
                Vector3 positionVérifiée;
                Personnage singleTargetÀAttaquer = null;
                int dégats = 0;

                if (BoutonsActions.ÉtatSort1)
                {
                    bool ciblealliée = false;
                    //GestionnaireInput.Update(gameTime);
                    switch (PersonnageActif.GetType().ToString())
                    {
                        case TypePersonnage.ARCHER:
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Archer.PORTÉE_PLUIE_DE_FLÈCHES - Archer.RAYON_PLUIE_DE_FLÈCHES);
                            ZoneDEffet.ChangerÉtendueEtPosition(new Vector2(Archer.RAYON_PLUIE_DE_FLÈCHES * 2), positionVérifiée);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Archer.PORTÉE_PLUIE_DE_FLÈCHES * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Portée.Visible = true;
                            if (GestionnaireInput.EstNouveauClicDroit())
                            {
                                Cibles = (PersonnageActif as Archer).PluieDeFlèches(positionVérifiée, JoueurEnnemi.Personnages, out dégats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                goto default;
                            }
                            break;
                        case TypePersonnage.GUÉRISSEUR:
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Guérisseur.PORTÉE_SOIN_DE_ZONE - Guérisseur.RAYON_SOIN_DE_ZONE);
                            ZoneDEffet.ChangerÉtendueEtPosition(new Vector2(Guérisseur.RAYON_SOIN_DE_ZONE * 2), positionVérifiée);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Guérisseur.PORTÉE_SOIN_DE_ZONE * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Portée.Visible = true;
                            if (GestionnaireInput.EstNouveauClicDroit())
                            {
                                if((PersonnageActif as Guérisseur)._SatanMode)
                                {
                                    Cibles = (PersonnageActif as Guérisseur).SoinDeZone(positionVérifiée, JoueurEnnemi.Personnages, out dégats);
                                    ciblealliée = false;
                                }
                                else
                                {
                                    Cibles = (PersonnageActif as Guérisseur).SoinDeZone(positionVérifiée, JoueurLocal.Personnages, out dégats);
                                    ciblealliée = true;
                                }
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                goto default;
                            }
                            break;
                        case TypePersonnage.GUERRIER:
                            Portée.ChangerÉtendueEtPosition(new Vector2(Guerrier.PORTÉE_TORNADE_FURIEUSE * 2), PersonnageActif.Position);
                            Portée.Visible = true;
                            if (GestionnaireInput.EstNouveauClicDroit())
                            {
                                Cibles = (PersonnageActif as Guerrier).TornadeFurieuse(PersonnageActif.Position, JoueurEnnemi.Personnages, out dégats);
                                PeutAttaquer = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                goto default;
                            }
                            break;
                        case TypePersonnage.MAGE:
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Mage.PORTÉE_BRASIER - Mage.RAYON_BRASIER);
                            ZoneDEffet.ChangerÉtendueEtPosition(new Vector2(Mage.RAYON_BRASIER * 2), positionVérifiée);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Mage.PORTÉE_BRASIER * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Portée.Visible = true;
                            if (GestionnaireInput.EstNouveauClicDroit())
                            {
                                Cibles = (PersonnageActif as Mage).Brasier(positionVérifiée, JoueurEnnemi.Personnages, out dégats);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                foreach (Personnage cible in Cibles)
                                {
                                    NetworkManager.SendÉtatsSpéciaux(cible, ciblealliée, new List<string>() { ÉtatSpécial.EN_FEU }, new List<bool> { true });
                                }
                                goto default;
                            }
                            break;
                        case TypePersonnage.PALADIN:
                            Portée.ChangerÉtendueEtPosition(new Vector2(Paladin.PORTÉE_CLARITÉ * 2), PersonnageActif.Position);
                            Portée.Visible = true;
                            singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer(JoueurLocal.Personnages);
                            if (singleTargetÀAttaquer != null && (int)(singleTargetÀAttaquer.Position - PersonnageActif.Position).Length() <= Paladin.PORTÉE_CLARITÉ)
                            {
                                ciblealliée = true;
                                PeutAttaquer = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                List<string> nomsÉtats = new List<string>() { ÉtatSpécial.EN_FEU, ÉtatSpécial.FREEZE };
                                List<bool> valeursÉtats = new List<bool>() { false, false };
                                if (singleTargetÀAttaquer is Guerrier)
                                {
                                    nomsÉtats.Add(ÉtatSpécial.FOLIE);
                                    valeursÉtats.Add(false);
                                }
                                NetworkManager.SendÉtatsSpéciaux(singleTargetÀAttaquer, ciblealliée, nomsÉtats, valeursÉtats);
                            }
                            break;
                        case TypePersonnage.VOLEUR:
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Voleur.PORTÉE_INVISIBILITÉ);
                            ZoneDEffet.ChangerÉtendueEtPosition(new Vector2(2), positionVérifiée);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Voleur.PORTÉE_INVISIBILITÉ * 2), PersonnageActif.Position);
                            ZoneDEffet.Visible = true;
                            Portée.Visible = true;
                            if (GestionnaireInput.EstNouveauClicDroit())
                            {
                                NetworkManager.SendNewPosition(positionVérifiée, JoueurLocal.Personnages.FindIndex(p => p is Voleur));
                                ZoneDéplacement.ChangerÉtendueEtPosition(new Vector2(DéplacementRestant * 2), positionVérifiée);
                                NetworkManager.SendInvisibilité(true);
                                PeutAttaquer = false;
                                ZoneDEffet.Visible = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                            }
                            break;
                        default:
                            NetworkManager.SendDégât(Cibles.FindAll(cible => !cible.EstMort), dégats, ciblealliée);
                            break;
                    }
                }
                if (BoutonsActions.ÉtatSort2)
                {
                    bool ciblealliée = false;
                    //GestionnaireInput.Update(gameTime);
                    switch (PersonnageActif.GetType().ToString())
                    {
                        case TypePersonnage.ARCHER:
                            positionVérifiée = GestionnaireInput.VérifierDéplacementMAX(GestionnaireInput.GetPositionSourisPlan(), PersonnageActif.Position, Archer.PORTÉE_FLÈCHE_REBONDISSANTE);
                            Portée.ChangerÉtendueEtPosition(new Vector2(Archer.PORTÉE_FLÈCHE_REBONDISSANTE * 2), PersonnageActif.Position);
                            ZoneDEffet.ChangerÉtendueEtPosition(new Vector2(Archer.RAYON_FLÈCHE_REBONDISSANTE * 2), positionVérifiée);
                            Portée.Visible = true;
                            ZoneDEffet.Visible = true;
                            singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer(JoueurEnnemi.Personnages);
                            if (singleTargetÀAttaquer != null && (int)(singleTargetÀAttaquer.Position - PersonnageActif.Position).Length() <= Archer.PORTÉE_FLÈCHE_REBONDISSANTE)
                            {
                                Cibles.Add(singleTargetÀAttaquer);
                                Personnage deuxièmeCible = (PersonnageActif as Archer).FlècheRebondissante(singleTargetÀAttaquer, JoueurEnnemi.Personnages.FindAll(p => p != singleTargetÀAttaquer && !p.EstMort), out dégats);
                                if (deuxièmeCible != null)
                                {
                                    Cibles.Add(deuxièmeCible);
                                }
                                PeutAttaquer = false;
                                Portée.Visible = false;
                                ZoneDEffet.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                goto default;
                            }
                            break;
                        case TypePersonnage.GUÉRISSEUR:
                            Portée.ChangerÉtendueEtPosition(new Vector2(Guérisseur.PORTÉE_RESURRECT * 2), PersonnageActif.Position);
                            Portée.Visible = true;
                            if ((PersonnageActif as Guérisseur)._SatanMode)
                            {
                                singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer(JoueurEnnemi.Personnages);
                                if (singleTargetÀAttaquer != null && (int)(singleTargetÀAttaquer.Position - PersonnageActif.Position).Length() <= Guérisseur.PORTÉE_RESURRECT)
                                {
                                    int vieVolée;
                                    dégats = (PersonnageActif as Guérisseur).VolDeVie(out vieVolée);
                                    ciblealliée = true;
                                    Cibles.Add(PersonnageActif);
                                    NetworkManager.SendDégât(Cibles, vieVolée, ciblealliée);
                                    Cibles.Clear();

                                    Cibles.Add(singleTargetÀAttaquer);
                                    ciblealliée = false;
                                    PeutAttaquer = false;
                                    Portée.Visible = false;
                                    BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                    goto default;
                                }
                            }
                            else
                            {
                                singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer(JoueurLocal.Personnages);
                                if (singleTargetÀAttaquer != null && (int)(singleTargetÀAttaquer.Position - PersonnageActif.Position).Length() <= Guérisseur.PORTÉE_RESURRECT && singleTargetÀAttaquer.EstMort)
                                {
                                    dégats = (PersonnageActif as Guérisseur).Résurrection(singleTargetÀAttaquer);
                                    Cibles.Add(singleTargetÀAttaquer);
                                    ciblealliée = true;
                                    PeutAttaquer = false;
                                    Portée.Visible = false;
                                    BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                    NetworkManager.SendÉtatsSpéciaux(PersonnageActif, ciblealliée, new List<string>() { ÉtatSpécial.SATAN }, new List<bool>() { true });
                                    NetworkManager.SendDégât(Cibles, dégats, ciblealliée);
                                }
                            }
                            break;
                        case TypePersonnage.GUERRIER:
                            if (GestionnaireInput.EstNouveauClicDroit())
                            {
                                ciblealliée = true;
                                PeutAttaquer = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                NetworkManager.SendÉtatsSpéciaux(PersonnageActif, ciblealliée, new List<string>() { ÉtatSpécial.FOLIE }, new List<bool>() { true });
                            }
                            break;
                        case TypePersonnage.MAGE:
                            Portée.ChangerÉtendueEtPosition(new Vector2(Mage.PORTÉE_FREEZE_DONT_MOVE * 2), PersonnageActif.Position);
                            Portée.Visible = true;
                            singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer(JoueurEnnemi.Personnages);
                            if (singleTargetÀAttaquer != null && (int)(singleTargetÀAttaquer.Position - PersonnageActif.Position).Length() <= Mage.PORTÉE_FREEZE_DONT_MOVE)
                            {
                                dégats = (PersonnageActif as Mage).FreezeDontMove(singleTargetÀAttaquer);
                                Cibles.Add(singleTargetÀAttaquer);
                                PeutAttaquer = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                NetworkManager.SendÉtatsSpéciaux(singleTargetÀAttaquer, ciblealliée, new List<string>() { ÉtatSpécial.FREEZE }, new List<bool>() { true });
                                goto default;
                            }
                            break;
                        case TypePersonnage.PALADIN:
                            Portée.ChangerÉtendueEtPosition(new Vector2(Paladin.PORTÉE_BOUCLIER_DIVIN * 2), PersonnageActif.Position);
                            Portée.Visible = true;
                            singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer(JoueurLocal.Personnages);
                            if (singleTargetÀAttaquer != null && (int)(singleTargetÀAttaquer.Position - PersonnageActif.Position).Length() <= Paladin.PORTÉE_BOUCLIER_DIVIN)
                            {
                                ciblealliée = true;
                                PeutAttaquer = false;
                                Portée.Visible = false;
                                BoutonsActions.RéinitialiserDialogueActions(PersonnageActif);
                                NetworkManager.SendÉtatsSpéciaux(singleTargetÀAttaquer, ciblealliée, new List<string>() { ÉtatSpécial.BOUCLIER_DIVIN }, new List<bool>() { true });
                            }
                            break;
                        case TypePersonnage.VOLEUR:
                            Portée.ChangerÉtendueEtPosition(new Vector2(Voleur.PORTÉE_LANCER_COUTEAU * 2), PersonnageActif.Position);
                            Portée.Visible = true;
                            singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer(JoueurEnnemi.Personnages);
                            if (singleTargetÀAttaquer != null && (int)(singleTargetÀAttaquer.Position - PersonnageActif.Position).Length() <= Voleur.PORTÉE_LANCER_COUTEAU)
                            {
                                dégats = (PersonnageActif as Voleur).LancerCouteau();
                                Cibles.Add(singleTargetÀAttaquer);
                                ciblealliée = false;
                                Portée.Visible = false;
                                PeutAttaquer = false;
                                goto default;
                            }
                            break;
                        default:
                            NetworkManager.SendDégât(Cibles.FindAll(cible => !cible.EstMort), dégats, ciblealliée);
                            break;
                    }
                }
                if (BoutonsActions.ÉtatAttaquer)
                {
                    //GestionnaireInput.Update(gameTime);  plus nécessaire puisqu'on utilise de clic droit

                    bool ciblealliée = false;
                    Portée.ChangerÉtendueEtPosition(new Vector2(PersonnageActif.GetPortéeAttaque() * 2), PersonnageActif.Position);
                    Portée.Visible = true;
                    if (PersonnageActif is Guérisseur && !(PersonnageActif as Guérisseur)._SatanMode)
                    {
                        singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer(JoueurLocal.Personnages);
                        ciblealliée = true;
                    }
                    else
                    {
                        singleTargetÀAttaquer = GestionnaireInput.DéterminerSélectionPersonnageÀAttaquer(JoueurEnnemi.Personnages);
                    }
                    if (singleTargetÀAttaquer != null && (int)(singleTargetÀAttaquer.Position - PersonnageActif.Position).Length() <= PersonnageActif.GetPortéeAttaque())
                    {
                        Cibles.Add(singleTargetÀAttaquer);
                        Portée.Visible = false;
                        PeutAttaquer = false;
                        BoutonsActions.Attaquer();
                        dégats = PersonnageActif.Attaquer();
                        NetworkManager.SendDégât(Cibles.FindAll(cible => !cible.EstMort), dégats, ciblealliée);
                    }
                }
                if (!BoutonsActions.ÉtatAttaquer && !BoutonsActions.ÉtatSort1 && !BoutonsActions.ÉtatSort2)
                {
                    ZoneDEffet.Visible = false;
                    Portée.Visible = false;
                }
                ActiverAttaque();
            }
        }

        void ActiverAttaque()
        {
            BoutonsActions.BtnSorts.EstActif = PeutAttaquer;
            BoutonsActions.BtnAttaquer.EstActif = PeutAttaquer;
        }

        void VérifierFinDeTour(GameTime gameTime)
        {
            if (TourFini())
            {
                TerminerLeTour();
                TempsDepuisDernierUpdate = gameTime.TotalGameTime.Seconds;
            }
        }

        bool TourFini()
        {
            return BoutonsActions.ÉtatPasserTour || !PeutAttaquer && (int)Math.Round(DéplacementRestant) <= 0;
        }

        void TerminerLeTour()
        {
            NetworkManager.SendFinDeTour();
            IndicePersonnage = IndicePersonnage < JoueurLocal.Personnages.Count - 1 ? IndicePersonnage + 1 : 0;
            // Voir avec PersonnageActif...
            BoutonsActions.RéinitialiserDialogueActions(JoueurLocal.Personnages[IndicePersonnage]);
            Cibles.Clear();
            DéplacementRestant = DÉPLACEMENT_MAX;
            BoutonsActions.VoirBoutonAction(false);
            ZoneDEffet.Visible = false;
            Portée.Visible = false;
            ZoneDéplacement.Visible = false;
            TourTerminé = true;
        }
    }
}
