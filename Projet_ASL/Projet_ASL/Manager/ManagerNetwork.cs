//------------------------------------------------------
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
        public bool PremierTour { get; set; }
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
                    case PacketType.AllPlayers:
                        ReceiveAllPlayers(inc);
                        break;
                    case PacketType.Logout:
                        RemoveDisconnectedPlayer(inc);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ReadPositionPersonnage(NetIncomingMessage inc)
        {
            string username = inc.ReadString();
            Player player = Players.Find(p => p.Username == username);
            player.Personnages[inc.ReadInt32()].GérerPositionObjet(new Vector3(inc.ReadFloat(), 0, inc.ReadFloat()));
        }

        private void RemoveDisconnectedPlayer(NetIncomingMessage inc)
        {
            string username = inc.ReadString();
            Player player = Players.Find(p => p.Username == username);
            foreach (Personnage p in player.Personnages)
            {
                Jeu.Components.Remove(p);
            }
            Players.Remove(player);
        }

        private void ReceiveAllPlayers(NetIncomingMessage inc)
        {
            var count = inc.ReadInt32();
            PremierTour = count == 1;
            for (int n = 0; n < count; n++)
            {
                ReadPlayer(inc);
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
                }
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
                p = new Archer(Jeu, allié ? "ArcherB" : "ArcherR", 0.0075f, posX == -40 ? Vector3.Zero : new Vector3(0, MathHelper.Pi, 0), new Vector3(posX, 0, posZ), 0, 0, 0, 0, ptsVie);
            }
            if (type == TypePersonnage.GUÉRISSEUR)
            {
                p = new Guérisseur(Jeu, allié ? "ArcherB" : "ArcherR", 0.0075f, posX == -40 ? Vector3.Zero : new Vector3(0, MathHelper.Pi, 0), new Vector3(posX, 0, posZ), 0, 0, 0, 0, ptsVie);
            }
            if (type == TypePersonnage.GUERRIER)
            {
                p = new Guerrier(Jeu, allié ? "GuerrierB" : "GuerrierR", 0.0075f, posX == -40 ? Vector3.Zero : new Vector3(0, MathHelper.Pi, 0), new Vector3(posX, 0, posZ), 0, 0, 0, 0, ptsVie);
            }
            if (type == TypePersonnage.MAGE)
            {
                p = new Mage(Jeu, allié ? "MageB" : "MageR", 0.0075f, posX == -40 ? Vector3.Zero : new Vector3(0, MathHelper.Pi, 0), new Vector3(posX, 0, posZ), 0, 0, 0, 0, ptsVie);
            }
            if (type == TypePersonnage.PALADIN)
            {
                p = new Paladin(Jeu, allié ? "ArcherB" : "ArcherR", 0.0075f, posX == -40 ? Vector3.Zero : new Vector3(0, MathHelper.Pi, 0), new Vector3(posX, 0, posZ), 0, 0, 0, 0, ptsVie);
            }
            if (type == TypePersonnage.VOLEUR)
            {
                p = new Voleur(Jeu, allié ? "ArcherB" : "ArcherR", 0.0075f, posX == -40 ? Vector3.Zero : new Vector3(0, MathHelper.Pi, 0), new Vector3(posX, 0, posZ), 0, 0, 0, 0, ptsVie);
            }
            p.DrawOrder = (int)OrdreDraw.MILIEU;
            return p;
        }

        public void SendInput(Keys key)
        {
            var outmessage = _client.CreateMessage();
            outmessage.Write((byte)PacketType.Input);
            outmessage.Write((byte)key);
            outmessage.Write(Username);
            _client.SendMessage(outmessage, NetDeliveryMethod.ReliableOrdered);
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
                outMessage.Write(Username);
                _client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered);
                _client.Disconnect("Bye");
            }
        }
    }
}
