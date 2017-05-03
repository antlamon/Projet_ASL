﻿//------------------------------------------------------
// 
// Copyright - (c) - 2014 - Mille Boström 
//
// Youtube channel - https://www.youtube.com/user/Maloooon
//------------------------------------------------------
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
        public List<Player> Players { get; set; }

        public string Username { get; set; }

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
            return EsablishInfo();
        }

        public string ObtenirType(Personnage p)
        {
            return p.GetType().ToString();
        }

        private bool EsablishInfo()
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
                        ChangerDeTour();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ChangerDeTour()
        {
            TourActif = !TourActif;
        }

        private void ReadPositionPersonnage(NetIncomingMessage inc)
        {
            string username = inc.ReadString();
            Player player = Players.Find(p => p.Username == username);
            player.Personnages[inc.ReadInt32()].GérerPositionObjet(new Vector3(inc.ReadFloat(), 0, inc.ReadFloat()));
        }

        private void RemoveDisconnectedPlayer(NetIncomingMessage inc)
        {
            DéconnectionServeur();
        }

        private void ReceiveAllPlayers(NetIncomingMessage inc)
        {
            var count = inc.ReadInt32();
            TourActif = count == 1;
            for (int n = 0; n < count; n++)
            {
                ReadPlayer(inc);
            }
        }

        private void ReadDégât(NetIncomingMessage inc)
        {
            string name = inc.ReadString();
            int count = inc.ReadInt32();
            Player player = Players.Find(p => p.Username == name);
            for(int i = 0; i < count; ++i)
            {
                player.Personnages[inc.ReadInt32()].ChangerVitalité(inc.ReadInt32());
            }
        }

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
                    AjouterIcôneEffet(Jeu, p);

                }
            }
        }

        const string FEU = "Enfeu";
        const string MORT = "EstMort";
        const string FROZEN = "Frozen";
        const string INVISIBLE = "Invisible";
        const string SATAN = "SatanMode";
        const string FOLIE = "Folie";

        private void AjouterIcôneEffet(Game jeu, Personnage p)
        {
            ClasseQueTonyVeutSupprimer icôneEffetFeu = new ClasseQueTonyVeutSupprimer(Jeu, p, FEU, 1);
            icôneEffetFeu.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            Jeu.Components.Add(icôneEffetFeu);
            //icôneEffetFeu.Visible = false;

            ClasseQueTonyVeutSupprimer icôneEffetMort = new ClasseQueTonyVeutSupprimer(Jeu, p, MORT, 2);
            icôneEffetMort.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            Jeu.Components.Add(icôneEffetMort);
            //icôneEffetMort.Visible = false;

            ClasseQueTonyVeutSupprimer icôneEffetFrozen = new ClasseQueTonyVeutSupprimer(Jeu, p, FROZEN, 3);
            icôneEffetFrozen.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            Jeu.Components.Add(icôneEffetFrozen);
            //icôneEffetFrozen.Visible = false;

            ClasseQueTonyVeutSupprimer icôneEffetInvisible = new ClasseQueTonyVeutSupprimer(Jeu, p, INVISIBLE, 4);
            icôneEffetInvisible.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
            Jeu.Components.Add(icôneEffetInvisible);
            //icôneEffetInvisible.Visible = false;

            if (p.GetType().ToString() == TypePersonnage.GUÉRISSEUR)
            {
                ClasseQueTonyVeutSupprimer icôneEffetSatan = new ClasseQueTonyVeutSupprimer(Jeu, p, SATAN, 5);
                icôneEffetSatan.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
                Jeu.Components.Add(icôneEffetSatan);
                //icôneEffetSatan.Visible = false;
            }

            if (p.GetType().ToString() == TypePersonnage.GUERRIER)
            {
                ClasseQueTonyVeutSupprimer icôneEffetFolie = new ClasseQueTonyVeutSupprimer(Jeu, p, FOLIE, 5);
                icôneEffetFolie.DrawOrder = (int)OrdreDraw.AVANT_PLAN;
                Jeu.Components.Add(icôneEffetFolie);
                //icôneEffetFolie.Visible = false;
            }
        }

        private Personnage ReadPersonnage(NetIncomingMessage inc, string username)
        {
            string type = inc.ReadString();
            float posX = inc.ReadFloat();
            float posZ = inc.ReadFloat();
            int ptsVie = inc.ReadInt32();
            return InstancierPersonnage(type, username == Username, posX, posZ, ptsVie);
        }

        private Personnage InstancierPersonnage(string type, bool allié, float posX, float posZ, int ptsVie)
        {
            Personnage p = null;
            if (type == TypePersonnage.ARCHER)
            {
                p = new Archer(Jeu, allié ? "ArcherB" : "ArcherR", 0.0075f, posX < 0 ? Vector3.Zero : new Vector3(0, MathHelper.Pi, 0), new Vector3(posX, 0, posZ), 10, 30, 10, 10, ptsVie);
            }
            if (type == TypePersonnage.GUÉRISSEUR)
            {
                p = new Guérisseur(Jeu, allié ? "ArcherB" : "ArcherR", 0.0075f, posX < 0 ? Vector3.Zero : new Vector3(0, MathHelper.Pi, 0), new Vector3(posX, 0, posZ), 10, 10, 10, 30, ptsVie);
            }
            if (type == TypePersonnage.GUERRIER)
            {
                p = new Guerrier(Jeu, allié ? "GuerrierB" : "GuerrierR", 0.0075f, posX < 0 ? Vector3.Zero : new Vector3(0, MathHelper.Pi, 0), new Vector3(posX, 0, posZ), 30, 10, 10, 10, ptsVie);
            }
            if (type == TypePersonnage.MAGE)
            {
                p = new Mage(Jeu, allié ? "MageB" : "MageR", 0.0075f, posX < 0 ? Vector3.Zero : new Vector3(0, MathHelper.Pi, 0), new Vector3(posX, 0, posZ), 10, 10, 30, 10, ptsVie);
            }
            if (type == TypePersonnage.PALADIN)
            {
                p = new Paladin(Jeu, allié ? "ArcherB" : "ArcherR", 0.0075f, posX < 0 ? Vector3.Zero : new Vector3(0, MathHelper.Pi, 0), new Vector3(posX, 0, posZ), 15, 10, 10, 15, ptsVie);
            }
            if (type == TypePersonnage.VOLEUR)
            {
                p = new Voleur(Jeu, allié ? "ArcherB" : "ArcherR", 0.0075f, posX < 0 ? Vector3.Zero : new Vector3(0, MathHelper.Pi, 0), new Vector3(posX, 0, posZ), 15, 15, 10, 10, ptsVie);
            }
            p.DrawOrder = (int)OrdreDraw.MILIEU;
            return p;
        }

        public void SendDégât(List<Personnage> PersonnagesTouchés, int dégât, bool allié)
        {
            if(PersonnagesTouchés.Count != 0 || dégât == 0)
            {
                var outMessage = _client.CreateMessage();
                outMessage.Write((byte)PacketType.Dégât);
                outMessage.Write(allié ? Username : JoueurEnnemi.Username);
                outMessage.Write(dégât);
                outMessage.Write(PersonnagesTouchés.Count);
                foreach(Personnage p in PersonnagesTouchés)
                {
                    outMessage.Write(ObtenirType(p));
                }
                _client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered);
            }
        }

        public void SendÉtatsSpéciaux(Personnage cible, bool allié, List<string> nomÉtat, List<bool> estActif)
        {
            var outMessage = _client.CreateMessage();
            outMessage.Write((byte)PacketType.ÉtatSpécial);
            outMessage.Write(allié ? Username : JoueurEnnemi.Username);
            outMessage.Write(ObtenirType(cible));
            outMessage.Write(nomÉtat.Count);
            for(int i = 0; i < nomÉtat.Count; ++i)
            {
                outMessage.Write(nomÉtat[i]);
                outMessage.Write(estActif[i]);
            }
            _client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered);
        }

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
                        break;
                    case ÉtatSpécial.BOUCLIER_DIVIN:
                        personnage.SetBouclierDivin(valeurÉtat);
                        break;
                    case ÉtatSpécial.FREEZE:
                        personnage.SetFreeze(valeurÉtat);
                        break;
                    case ÉtatSpécial.FOLIE:
                        (personnage as Guerrier).SetFolie(valeurÉtat);
                        break;
                    case ÉtatSpécial.SATAN:
                        (personnage as Guérisseur).SetSatan(valeurÉtat);
                        break;
                }
            }
        }

        public void SendInput(Keys key)
        {
            var outmessage = _client.CreateMessage();
            outmessage.Write((byte)PacketType.Input);
            outmessage.Write((byte)key);
            outmessage.Write(Username);
            _client.SendMessage(outmessage, NetDeliveryMethod.ReliableOrdered);
        }

        public void SendFinDeTour()
        {
            var outMessge = _client.CreateMessage();
            outMessge.Write((byte)PacketType.FinDeTour);
            _client.SendMessage(outMessge, NetDeliveryMethod.ReliableOrdered);
        }

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

        public void SendInvisibilité(bool visible)
        {
            var outMessage = _client.CreateMessage();
            outMessage.Write((byte)PacketType.Invisibilité);
            outMessage.Write(Username);
            outMessage.Write(visible);
            _client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered);
        }

        private void ReadInvisibilité(NetIncomingMessage inc)
        {
            string name = inc.ReadString();
            if(Username != name)
            {
                JoueurEnnemi.Personnages.Find(p => p is Voleur).Visible = !inc.ReadBoolean();
            }
        }

        public void SendMesage(string message)
        {
            NetOutgoingMessage outMessage = _client.CreateMessage();
            outMessage.Write((byte)PacketType.Message);
            outMessage.Write(message);
            _client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered);
        }

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

        private void DéconnectionServeur()
        {
            _client.Disconnect("Bye");
            RetirerPersonnages();
        }

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
            RéinitialiserListePlayer();
        }
    }
}
