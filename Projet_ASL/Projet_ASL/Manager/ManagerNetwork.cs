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
        Game Jeu { get; set; }


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
            _client.Connect("localHost", 5013, outmsg);
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

        private void RemoveDisconnectedPlayer(NetIncomingMessage inc)
        {
            string username = inc.ReadString();
            Players.Remove(Players.Find(p => p.Username == username));
        }

        private void ReceiveAllPlayers(NetIncomingMessage inc)
        {
            var count = inc.ReadInt32();
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
                player.Personnages.Add(ReadPersonnage(inc));
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
                }
            }
        }

        private Personnage ReadPersonnage(NetIncomingMessage inc)
        {
            string type = inc.ReadString();
            float posX = inc.ReadFloat();
            float posZ = inc.ReadFloat();
            int ptsVie = inc.ReadInt32();
            return InstancierPersonnage(inc.ReadString(), posX, posZ, ptsVie);
        }

        private Personnage InstancierPersonnage(string type, float posX, float posZ, int ptsVie)
        {
            Personnage p = null;
            if (type == TypePersonnage.ARCHER)
            {
                p = new Archer(Jeu, "ArcherB", 0.03f, Vector3.Zero, new Vector3(posX, 0, posZ), 0, 0, 0, 0, ptsVie);
            }
            if (type == TypePersonnage.GUÉRISSEUR)
            {
                p = new Guérisseur(Jeu, "Guerrier", 0.03f, Vector3.Zero, new Vector3(posX, 0, posZ), 0, 0, 0, 0, ptsVie);
            }
            if (type == TypePersonnage.GUERRIER)
            {
                p = new Guerrier(Jeu, "GuerrierB", 0.03f, Vector3.Zero, new Vector3(posX, 0, posZ), 0, 0, 0, 0, ptsVie);
            }
            if (type == TypePersonnage.MAGE)
            {
                p = new Mage(Jeu, "Mage", 0.03f, Vector3.Zero, new Vector3(posX, 0, posZ), 0, 0, 0, 0, ptsVie);
            }
            if (type == TypePersonnage.PALADIN)
            {
                p = new Paladin(Jeu, "ArcherR", 0.03f, Vector3.Zero, new Vector3(posX, 0, posZ), 0, 0, 0, 0, ptsVie);
            }
            if (type == TypePersonnage.VOLEUR)
            {
                p = new Voleur(Jeu, "Mage", 0.03f, Vector3.Zero, new Vector3(posX, 0, posZ), 0, 0, 0, 0, ptsVie);
            }
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
            NetOutgoingMessage outMessage = _client.CreateMessage();
            outMessage.Write((byte)PacketType.Logout);
            outMessage.Write(Username);
            _client.SendMessage(outMessage, NetDeliveryMethod.ReliableOrdered);
            _client.Disconnect("Thank you, come again");
        }
    }
}
