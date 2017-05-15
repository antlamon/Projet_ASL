// Classe créée et écrite par Simon Talbot dans le cadre du Projet d'Intégration du DEC
// Sciences Informatiques et Mathématiques. 
// Avec la contribution de Laugane Patry et Antoine Lamontagne.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    /// <summary>
    /// Classe qui gère le tour du combattant actif local.
    /// </summary>
    public class TourManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        const int DÉPLACEMENT_MAX = 10;
        const float INTERVALLE_MAJ_STANDARD = 1 / 60f;

        Jeu Jeu { get; set; }
        ManagerNetwork NetworkManager { get; set; }
        InputManager GestionnaireInput { get; set; }
        List<Personnage> Cibles { get; set; }
        Player JoueurLocal { get; set; }
        Player JoueurEnnemi { get; set; }
        Personnage PersonnageActif { get; set; }
        int ancienIndicePersonnage { get; set; }
        int IndicePersonnage { get; set; }
        float DéplacementRestant { get; set; }
        float ancienDéplacementRestant { get; set; }
        AOE ZoneDEffet { get; set; }
        AOE Portée { get; set; }
        AOE ZoneDéplacement { get; set; }
        public DialogueActions BoutonsActions { get; private set; }
        bool PeutAttaquer { get; set; }
        bool TourTerminé { get; set; }
        bool DernierSurvivant { get; set; }

        public TourManager(Jeu jeu, ManagerNetwork networkManager)
            : base(jeu)
        {
            NetworkManager = networkManager;
            BoutonsActions = new DialogueActions(jeu, new Vector2(Game.Window.ClientBounds.Width / 3f, Game.Window.ClientBounds.Height / 7f), NetworkManager);
            Game.Components.Add(BoutonsActions);
            Jeu = jeu;
        }

        /// <summary>
        /// Initialise les propriétés et les composants qui doivent être assignés 
        /// au début du jeu pour le bon fonctionnement de la classe.
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
            TourTerminé = true;
            DernierSurvivant = false;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            GestionnaireInput = Game.Services.GetService(typeof(InputManager)) as InputManager;
            base.LoadContent();
        }

        /// <summary>
        /// Crée les boutons d'attaque pour chaque personnage inclu dans l'équipe du joueur.
        /// </summary>
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
                    VérifierFinDeTour();
                }
        }

        /// <summary>
        /// Vérifie si un nouveau personnage est en jeu et gère les propriétés de celui-ci si c'est le cas.
        /// </summary>
        /// <param name="gameTime"></param>
        void VérifierDébutDeTour(GameTime gameTime)
        {
            if (ancienIndicePersonnage != IndicePersonnage || DernierSurvivant)
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
                    DéplacementRestant = DÉPLACEMENT_MAX;
                    DernierSurvivant = false;
                }
                else
                {
                    DernierSurvivant = JoueurLocal.Personnages.FindAll(perso => !perso.EstMort).Count == 1;
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

        /// <summary>
        /// Gère le compteur et le dégat des états spéciaux que peut avoir un personnage.
        /// </summary>
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

        /// <summary>
        /// Vérifie si le personnage en jeu se déplace.
        /// </summary>
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

        /// <summary>
        /// S'occupe de déplacer la portée du mouvement avec le personnage et de modifier la grandeur de celle-ci selon le déplacement restant.
        /// </summary>
        void DéplacerZoneMouvement()
        {
            if (ancienDéplacementRestant != DéplacementRestant)
            {
                ZoneDéplacement.ChangerÉtendueEtPosition(new Vector2(DéplacementRestant * 2), PersonnageActif.Position­);
                ancienDéplacementRestant = DéplacementRestant;
            }
        }

        /// <summary>
        /// Vérifie et gère les attaques et les sorts des personnages selon leur classe.
        /// </summary>
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

        /// <summary>
        /// Rend les boutons d'attaque et de sorts disponibles ou indisponibles selon la valeur de PeutAttaquer
        /// </summary>
        void ActiverAttaque()
        {
            BoutonsActions.BtnSorts.EstActif = PeutAttaquer;
            BoutonsActions.BtnAttaquer.EstActif = PeutAttaquer;
        }

        /// <summary>
        /// Est appelé par la méthode Update pour vérifier si le tour du personnage est terminé.
        /// </summary>
        void VérifierFinDeTour()
        {
            if (TourFini())
            {
                TerminerLeTour();
            }
        }

        /// <returns>Vrai si le personnage en jeu meurt ou utilise tous ses points de déplacement et d'attaque.</returns>
        bool TourFini()
        {
            return PersonnageActif.EstMort || BoutonsActions.ÉtatPasserTour || !PeutAttaquer && (int)Math.Round(DéplacementRestant) <= 0;
        }

        /// <summary>
        /// Lorsque qu'un personnage finit son tour, en mourrant ou en utilisant tous ses points de déplacement et d'attaque,
        /// cette méthode est appelée. Elle met fin au tour local et réinitialise les composants et les propriétés spécifiques
        /// au personnage qui était en jeu.
        /// </summary>
        void TerminerLeTour()
        {
            NetworkManager.SendFinDeTour(); // Envoie au serveur le message que le tour de ce client est terminé
            IndicePersonnage = IndicePersonnage < JoueurLocal.Personnages.Count - 1 ? IndicePersonnage + 1 : 0;
            BoutonsActions.RéinitialiserDialogueActions(JoueurLocal.Personnages[IndicePersonnage]);
            Cibles.Clear();
            BoutonsActions.VoirBoutonAction(false);
            ZoneDEffet.Visible = false;
            Portée.Visible = false;
            ZoneDéplacement.Visible = false;
            TourTerminé = true;
        }
    }
}
