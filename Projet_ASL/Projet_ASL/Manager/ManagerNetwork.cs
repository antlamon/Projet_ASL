
using System;
using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Projet_ASL
{
    public class ManagerNetwork
    {
        private NetClient _client;
        public List<Player> Players { get; private set; }

        public string Username { get; private set; }

        public bool Active { get; set; }
        public bool TourActif { get; set; }
        Game Jeu { get; set; }

        public Player JoueurLocal
        {
            get
            {
                return Players.Find(p => p.Username == Username);
            }
        }
        public Player JoueurEnnemi
        {
            get
            {
                return Players.FirstOrDefault(p => p.Username != Username);
            }
        }

        public ManagerNetwork(Game jeu)
        {
            Players = new List<Player>();
            Jeu = jeu;
        }

        private void RéinitialiserListePlayer()
        {
            Players.Clear();
        }

        /// <summary>
        /// Établir la connection avec le serveur
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool Start(Player player)
        {
            var random = new Random();
            _client = new NetClient(new NetPeerConfiguration("networkGame"));
            _client.Start();
            Username = "name_" + random.Next(0, 100);
            player.Username = Username;
            var outmsg = _client.CreateMessage();
            outmsg.Write((byte)PacketType.Login);
            outmsg.WriteAllProperties(player);
            foreach (Personnage p in player.Personnages)
            {
                outmsg.Write(ObtenirType(p));
            }
            _client.Connect("localhost", 5013, outmsg);
            return SeConnecterServeur();
        }

        /// <summary>
        /// Obtenir le type d'un personnage sous forme de string
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public string ObtenirType(Personnage p)
        {
            return p.GetType().ToString();
        }

        /// <summary>
        /// Permet de se connecter au serveur et de récupérer les personnages connectés
        /// </summary>
        /// <returns>Connection établit</returns>
        private bool SeConnecterServeur()
        {
            var time = DateTime.Now;
            NetIncomingMessage inc;
            while (true)
            {
                if (DateTime.Now.Subtract(time).Seconds > 4)
                {
                    return false;
                }

                if ((inc = _client.ReadMessage()) == null) continue;

                switch (inc.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        var data = inc.ReadByte();
                        if (data == (byte)PacketType.Login)
                        {
                            Active = inc.ReadBoolean();
                            if (Active)
                            {
                                ReceiveAllPlayers(inc);
                                return true;
                            }

                            return false;
                        }
                        return false;
                }
            }
        }

        /// <summary>
        /// Vérifie les messages envoyés par le serveur
        /// </summary>
        public void Update()
        {
            NetIncomingMessage inc;
            while ((inc = _client.ReadMessage()) != null)
            {
                if (inc.MessageType != NetIncomingMessageType.Data) continue;
                var packageType = (PacketType)inc.ReadByte();
                switch (packageType)
                {
                    case PacketType.PlayerPosition:
                        ReadPlayer(inc);
                        break;
                    case PacketType.PersonnagePosition:
                        ReadPositionPersonnage(inc);
                        break;
                    case PacketType.Dégât:
                        ReadDégât(inc);
                        break;
                    case PacketType.Invisibilité:
                        ReadInvisibilité(inc);
                        break;
                    case PacketType.ÉtatSpécial:
                        ReadÉtatsSpéciaux(inc);
                        break;
                    case PacketType.AllPlayers:
                        ReceiveAllPlayers(inc);
                        break;
                    case PacketType.Logout:
                        RemoveDisconnectedPlayer(inc);
                        break;
                    case PacketType.FinDeTour:
                        ChangerDeTour(inc);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Détermine si c'est le tour du client ou non
        /// </summary>
        /// <param name="inc"></param>
        private void ChangerDeTour(NetIncomingMessage inc)
        {
            if (inc.ReadString() != Username)
            {
                TourActif = !TourActif;
            }
        }

        /// <summary>
        /// Lit et change la position d'un personnage
        /// </summary>
        /// <param name="inc">message du serveur</param>
        private void ReadPositionPersonnage(NetIncomingMessage inc)
        {
            string username = inc.ReadString();
            Player player = Players.Find(p => p.Username == username);
            player.Personnages[inc.ReadInt32()].GérerPositionObjet(new Vector3(inc.ReadFloat(), 0, inc.ReadFloat()));
        }

        /// <summary>
        /// Déconnecte le client du serveur en supprimant les personnages des game components
        /// </summary>
        /// <param name="inc">message du serveur</param>
        private void RemoveDisconnectedPlayer(NetIncomingMessage inc)
        {
            DéconnectionServeur();
        }

        /// <summary>
        /// Récupère tous les personnages présents sur le serveur
        /// </summary>
        /// <param name="inc">message du serveur</param>
        private void ReceiveAllPlayers(NetIncomingMessage inc)
        {
            var count = inc.ReadInt32();
            TourActif = count == 1;
            for (int n = 0; n < count; n++)
            {
                ReadPlayer(inc);
            }
        }

        /// <summary>
        /// Lit et modifie la vitalité des personnages touchés par une attaque ou un soin
        /// </summary>
        /// <param name="inc">message du serveur</param>
        private void ReadDégât(NetIncomingMessage inc)
        {
            string name = inc.ReadString();
            int count = inc.ReadInt32();
            Player player = Players.Find(p => p.Username == name);
            for (int i = 0; i < count; ++i)
            {
                int index = inc.ReadInt32();
                bool ancienMort = player.Personnages[index].EstMort;
                player.Personnages[index].ChangerVitalité(inc.ReadInt32());
                if (ancienMort != player.Personnages[index].EstMort)
                {
                    (Jeu.Components.First(c => c is IdentificateurEffet && (c as IdentificateurEffet).PersonnageÀIdentifier == player.Personnages[index] && (c as IdentificateurEffet).NomImage == ÉtatSpécial.MORT) as DrawableGameComponent).Visible = player.Personnages[index].EstMort;

                }
            }
        }

        /// <summary>
        /// Récupère les données d'un joueur connecté au serveur, soit les personnages et leur position et leur vitalité 
        /// er instancie le joueur s'il n'est pas dans la liste de joueur du client
        /// </summary>
        /// <param name="inc"></param>
        private void ReadPlayer(NetIncomingMessage inc)
        {
            var player = new Player();
            inc.ReadAllProperties(player);
            for (int i = 0; i < player.Personnages.Capacity; ++i)
            {
                player.Personnages.Add(ReadPersonnage(inc, player.Username));
            }
            if (Players.Any(p => p.Username == player.Username))
            {
                var oldPlayer = Players.FirstOrDefault(p => p.Username == player.Username);
                for (int i = 0; i < player.Personnages.Count; ++i)
                {
                    if (player.Personnages[i].Position != oldPlayer.Personnages[i].Position)
                    {
                        oldPlayer.Personnages[i].GérerPositionObjet(player.Personnages[i].Position);
                    }
                    if (player.Personnages[i].PtsDeVie != oldPlayer.Personnages[i].PtsDeVie)
                    {
                        oldPlayer.Personnages[i].ChangerVitalité(player.Personnages[i].PtsDeVie);
                    }
                }
            }
            else
            {
                Players.Add(player);
                foreach (Personnage p in player.Personnages)
                {
                    Jeu.Components.Add(p);
                    IdentificateurPersonnage identificateur = new IdentificateurPersonnage(Jeu, p);
                    identificateur.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
                    Jeu.Components.Add(identificateur);
                    AjouterIcôneEffet(Jeu, p, identificateur);

                }
            }
        }

        /// <summary>
        /// Ajoute les icônes d'effets spéciaux selon un certain personnage dans les Game.Components
        /// </summary>
        /// <param name="jeu"></param>
        /// <param name="p">Personnage à identifier</param>
        /// <param name="identificateur">Identificateur du personnage</param>
        private void AjouterIcôneEffet(Game jeu, Personnage p, IdentificateurPersonnage identificateur)
        {
            IdentificateurEffet icôneEffetFeu = new IdentificateurEffet(Jeu, p, ÉtatSpécial.EN_FEU, identificateur.Position, 0);
            icôneEffetFeu.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            Jeu.Components.Add(icôneEffetFeu);
            icôneEffetFeu.Visible = false;

            IdentificateurEffet icôneEffetMort = new IdentificateurEffet(Jeu, p, ÉtatSpécial.MORT, identificateur.Position, 1);
            icôneEffetMort.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            Jeu.Components.Add(icôneEffetMort);
            icôneEffetMort.Visible = false;

            IdentificateurEffet icôneEffetFrozen = new IdentificateurEffet(Jeu, p, ÉtatSpécial.FREEZE, identificateur.Position, 2);
            icôneEffetFrozen.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            Jeu.Components.Add(icôneEffetFrozen);
            icôneEffetFrozen.Visible = false;

            IdentificateurEffet icôneEffetBouclierDivin = new IdentificateurEffet(Jeu, p, ÉtatSpécial.BOUCLIER_DIVIN, identificateur.Position, 3);
            icôneEffetBouclierDivin.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            Jeu.Components.Add(icôneEffetBouclierDivin);
            icôneEffetBouclierDivin.Visible = false;

            if (ObtenirType(p) == TypePersonnage.VOLEUR)
            {
                IdentificateurEffet icôneEffetInvisible = new IdentificateurEffet(Jeu, p, ÉtatSpécial.INVISIBLE, identificateur.Position, 4);
                icôneEffetInvisible.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
                Jeu.Components.Add(icôneEffetInvisible);
                icôneEffetInvisible.Visible = false;
            }

            if (ObtenirType(p) == TypePersonnage.GUÉRISSEUR)
            {
                IdentificateurEffet icôneEffetSatan = new IdentificateurEffet(Jeu, p, ÉtatSpécial.SATAN, identificateur.Position, 4);
                icôneEffetSatan.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
                Jeu.Components.Add(icôneEffetSatan);
                icôneEffetSatan.Visible = false;
            }

            if (ObtenirType(p) == TypePersonnage.GUERRIER)
            {
                IdentificateurEffet icôneEffetFolie = new IdentificateurEffet(Jeu, p, ÉtatSpécial.FOLIE, identificateur.Position, 4);
                icôneEffetFolie.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
                Jeu.Components.Add(icôneEffetFolie);
                icôneEffetFolie.Visible = false;
            }
        }

        /// <summary>
        /// Lis un personnage provenant du serveur
        /// </summary>
        /// <param name="inc">message du serveur</param>
        /// <param name="username">Username du joueur ayant ce personnage</param>
        /// <returns></returns>
        private Personnage ReadPersonnage(NetIncomingMessage inc, string username)
        {
            string type = inc.ReadString();
            float posX = inc.ReadFloat();
            float posZ = inc.ReadFloat();
            int ptsVie = inc.ReadInt32();
            return InstancierPersonnage(type, username == Username, posX, posZ, ptsVie);
        }

        /// <summary>
        /// Instancie un personnage selon son type, sa position et sa vitalité et du joueur d'où il vient
        /// </summary>
        /// <param name="type">Type du personnage</param>
        /// <param name="allié">Personnage du joueur local?</param>
        /// <param name="posX">position en X</param>
        /// <param name="posZ">position en Z</param>
        /// <param name="ptsVie">Points de vie du personnage</param>
        /// <returns>Personnage créé</returns>
        private Personnage InstancierPersonnage(string type, bool allié, float posX, float posZ, int ptsVie)
        {
            Personnage p = null;
            if (type == TypePersonnage.ARCHER)
            {
                p = new Archer(Jeu, allié ? "ArcherAllié" : "ArcherEnnemi", 0.04f, posX < 0 ? new Vector3(0, MathHelper.Pi / 2, 0) : new Vector3(0, -MathHelper.Pi / 2, 0), new Vector3(posX, 0, posZ), 10, 30, 10, 10, ptsVie);
            }
            if (type == TypePersonnage.GUÉRISSEUR)
            {
                p = new Guérisseur(Jeu, allié ? "GuérisseurAllié" : "GuérisseurEnnemi", 0.04f, posX < 0 ? new Vector3(0, MathHelper.Pi / 2, 0) : new Vector3(0, -MathHelper.Pi / 2, 0), new Vector3(posX, 0, posZ), 10, 10, 10, 30, ptsVie);
            }
            if (type == TypePersonnage.GUERRIER)
            {
                p = new Guerrier(Jeu, allié ? "GuerrierAllié" : "GuerrierEnnemi", 0.04f, posX < 0 ? new Vector3(0, MathHelper.Pi / 2, 0) : new Vector3(0, -MathHelper.Pi / 2, 0), new Vector3(posX, 0, posZ), 30, 10, 10, 10, ptsVie);
            }
            if (type == TypePersonnage.MAGE)
            {
                p = new Mage(Jeu, allié ? "MageAllié" : "MageEnnemi", 0.04f, posX < 0 ? new Vector3(0, MathHelper.Pi / 2, 0) : new Vector3(0, -MathHelper.Pi / 2, 0), new Vector3(posX, 0, posZ), 10, 10, 30, 10, ptsVie);
            }
            if (type == TypePersonnage.PALADIN)
            {
                p = new Paladin(Jeu, allié ? "PaladinAllié" : "PaladinEnnemi", 0.04f, posX < 0 ? new Vector3(0, MathHelper.Pi / 2, 0) : new Vector3(0, -MathHelper.Pi / 2, 0), new Vector3(posX, 0, posZ), 15, 10, 10, 15, ptsVie);
            }
            if (type == TypePersonnage.VOLEUR)
            {
                p = new Voleur(Jeu, allié ? "VoleurAllié" : "VoleurEnnemi", 0.04f, posX < 0 ? new Vector3(0, MathHelper.Pi / 2, 0) : new Vector3(0, -MathHelper.Pi / 2, 0), new Vector3(posX, 0, posZ), 15, 15, 10, 10, ptsVie);
            }
            p.DrawOrder = (int)OrdreDraw.MILIEU;
            return p;
        }

        /// <summary>
        /// Envoie du dégâts au serveur selon une liste de personnages touchées et si le personnage est local
        /// </summary>
        /// <param name="PersonnagesTouchés">Personnages touchés par le dégât</param>
        /// <param name="dégât"></param>
        /// <param name="allié">Personnage appartient au joueur local?</param>
        public void SendDégât(List<Personnage> PersonnagesTouchés, int dégât, bool allié)
        {
            if (PersonnagesTouchés.Count != 0 || dégât == 0)
            {
                var outMessage = _client.CreateMessage();
                outMessage.Write((byte)PacketType.Dégât);
                outMessage.Write(allié ? Username : JoueurEnnemi.Username);
                outMessage.Write(dégât);
                outMessage.Write(PersonnagesTouchés.Count);
                foreach (Personnage p in PersonnagesTouchés)
                {
                    outMessage.Write(ObtenirType(p));
                }
                _client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered);
            }
        }

        /// <summary>
        /// Envoie une liste d'éffets spéciaux à appliquer sur une cible
        /// </summary>
        /// <param name="cible">personnage touché</param>
        /// <param name="allié">Personnage du joueur local?</param>
        /// <param name="nomÉtat">Nom des états à appliquer</param>
        /// <param name="estActif">États des états à appliquer</param>
        public void SendÉtatsSpéciaux(Personnage cible, bool allié, List<string> nomÉtat, List<bool> estActif)
        {
            var outMessage = _client.CreateMessage();
            outMessage.Write((byte)PacketType.ÉtatSpécial);
            outMessage.Write(allié ? Username : JoueurEnnemi.Username);
            outMessage.Write(ObtenirType(cible));
            outMessage.Write(nomÉtat.Count);
            for (int i = 0; i < nomÉtat.Count; ++i)
            {
                outMessage.Write(nomÉtat[i]);
                outMessage.Write(estActif[i]);
            }
            _client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Lit les états spéciaux à appliquer sur un personnage à partir d'un message du serveur
        /// </summary>
        /// <param name="inc">Message du serveur</param>
        private void ReadÉtatsSpéciaux(NetIncomingMessage inc)
        {
            string name = inc.ReadString();
            int index = inc.ReadInt32();
            Personnage personnage = Players.Find(p => p.Username == name).Personnages[index];
            int compteur = inc.ReadInt32();
            for (int i = 0; i < compteur; ++i)
            {
                string nomÉtat = inc.ReadString();
                bool valeurÉtat = inc.ReadBoolean();
                switch (nomÉtat)
                {
                    case ÉtatSpécial.EN_FEU:
                        personnage.SetEnFeu(valeurÉtat);
                        (Jeu.Components.First(c => c is IdentificateurEffet && (c as IdentificateurEffet).PersonnageÀIdentifier == personnage && (c as IdentificateurEffet).NomImage == ÉtatSpécial.EN_FEU) as DrawableGameComponent).Visible = valeurÉtat;
                        break;
                    case ÉtatSpécial.BOUCLIER_DIVIN:
                        personnage.SetBouclierDivin(valeurÉtat);
                        (Jeu.Components.First(c => c is IdentificateurEffet && (c as IdentificateurEffet).PersonnageÀIdentifier == personnage && (c as IdentificateurEffet).NomImage == ÉtatSpécial.BOUCLIER_DIVIN) as DrawableGameComponent).Visible = valeurÉtat;
                        break;
                    case ÉtatSpécial.FREEZE:
                        personnage.SetFreeze(valeurÉtat);
                        (Jeu.Components.First(c => c is IdentificateurEffet && (c as IdentificateurEffet).PersonnageÀIdentifier == personnage && (c as IdentificateurEffet).NomImage == ÉtatSpécial.FREEZE) as DrawableGameComponent).Visible = valeurÉtat;
                        break;
                    case ÉtatSpécial.FOLIE:
                        (personnage as Guerrier).SetFolie(valeurÉtat);
                        (Jeu.Components.First(c => c is IdentificateurEffet && (c as IdentificateurEffet).PersonnageÀIdentifier == personnage && (c as IdentificateurEffet).NomImage == ÉtatSpécial.FOLIE) as DrawableGameComponent).Visible = valeurÉtat;
                        break;
                    case ÉtatSpécial.SATAN:
                        (personnage as Guérisseur).SetSatan(valeurÉtat);
                        (Jeu.Components.First(c => c is IdentificateurEffet && (c as IdentificateurEffet).PersonnageÀIdentifier == personnage && (c as IdentificateurEffet).NomImage == ÉtatSpécial.SATAN) as DrawableGameComponent).Visible = valeurÉtat;
                        break;
                }
            }
        }

        //public void SendInput(Keys key)
        //{
        //    var outmessage = _client.CreateMessage();
        //    outmessage.Write((byte)PacketType.Input);
        //    outmessage.Write((byte)key);
        //    outmessage.Write(Username);
        //    _client.SendMessage(outmessage, NetDeliveryMethod.ReliableOrdered);
        //}

        /// <summary>
        /// Envoie la fin d'un tour local au serveur
        /// </summary>
        public void SendFinDeTour()
        {
            TourActif = false;
            var outMessge = _client.CreateMessage();
            outMessge.Write((byte)PacketType.FinDeTour);
            outMessge.Write(Username);
            _client.SendMessage(outMessge, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Envoie la nouvelle position d'un personnage
        /// </summary>
        /// <param name="position">Nouvelle position</param>
        /// <param name="indexPersonnage">Index du personnage local à déplacer</param>
        public void SendNewPosition(Vector3 position, int indexPersonnage)
        {
            var outMessage = _client.CreateMessage();
            outMessage.Write((byte)PacketType.InputVector);
            outMessage.Write(position.X);
            outMessage.Write(position.Z);
            outMessage.Write(Username);
            outMessage.Write(indexPersonnage);
            _client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered);

        }

        /// <summary>
        /// Envoie l'état d'invisibilité du voleur
        /// </summary>
        /// <param name="visible">personnage visible?</param>
        public void SendInvisibilité(bool visible)
        {
            var outMessage = _client.CreateMessage();
            outMessage.Write((byte)PacketType.Invisibilité);
            outMessage.Write(Username);
            outMessage.Write(visible);
            _client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Lit l'invisibilité d'un voleur selon le joueur impliqué
        /// </summary>
        /// <param name="inc">message du serveur</param>
        private void ReadInvisibilité(NetIncomingMessage inc)
        {
            string name = inc.ReadString();
            if (Username != name)
            {
                Personnage personnage = JoueurEnnemi.Personnages.Find(p => p is Voleur);
                personnage.Visible = !inc.ReadBoolean();
                (Jeu.Components.First(c => c is IdentificateurEffet && (c as IdentificateurEffet).PersonnageÀIdentifier == personnage && (c as IdentificateurEffet).NomImage == ÉtatSpécial.INVISIBLE) as DrawableGameComponent).Visible = !personnage.Visible;
            }
        }

        /// <summary>
        /// Envoie un message à afficher sur la console du serveur, utilisé pour le deboguage
        /// </summary>
        /// <param name="message">message à envoyer</param>
        public void SendMesage(string message)
        {
            NetOutgoingMessage outMessage = _client.CreateMessage();
            outMessage.Write((byte)PacketType.Message);
            outMessage.Write(message);
            _client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Envoie une commande de déconnection au serveur
        /// </summary>
        public void SendLogout()
        {
            if (Active)
            {
                NetOutgoingMessage outMessage = _client.CreateMessage();
                outMessage.Write((byte)PacketType.Logout);
                _client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered);
                DéconnectionServeur();
            }
        }

        /// <summary>
        /// Déconnecte le client du serveur
        /// </summary>
        private void DéconnectionServeur()
        {
            _client.Disconnect("Bye");
            RetirerPersonnages();
        }

        /// <summary>
        /// Retire les personnages et leurs identificateurs de la liste des game components
        /// </summary>
        private void RetirerPersonnages()
        {
            foreach (Player player in Players)
            {
                foreach (Personnage p in player.Personnages)
                {
                    Jeu.Components.Remove(p);
                    IdentificateurPersonnage identificateur = Jeu.Components.First(id => id is IdentificateurPersonnage && (id as IdentificateurPersonnage).PersonnageÀIdentifier == p) as IdentificateurPersonnage;
                    Jeu.Components.Remove(identificateur);
                    Jeu.Components.Remove(identificateur.AfficheurPtsVie);

                }
            }
            List<IdentificateurEffet> effetÀSupprimer = new List<IdentificateurEffet>();
            foreach (GameComponent c in Jeu.Components)
            {
                if (c is IdentificateurEffet) { effetÀSupprimer.Add(c as IdentificateurEffet); }
            }
            foreach (IdentificateurEffet c in effetÀSupprimer)
            {
                Jeu.Components.Remove(c);
            }
            RéinitialiserListePlayer();
        }
    }
}
