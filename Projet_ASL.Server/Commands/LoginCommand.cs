//------------------------------------------------------
// 
// Copyright - (c) - 2014 - Mille Boström 
//
// Youtube channel - https://www.youtube.com/user/Maloooon
//------------------------------------------------------
using System;
using System.Collections.Generic;
using Projet_ASL;
using Lidgren.Network;
using Microsoft.Xna.Framework;


namespace Projet_ASL.Server.Commands
{
    class LoginCommand : ICommand
    {
        const int POSITION_X_DEPART = 20;
        const int POSITION_Z_DEPART = -15;
        const int VIE_MAX = Personnage.PTS_VIE_MAX;

        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            Console.WriteLine("New connection...");
            var data = inc.ReadByte();
            if (data == (byte)PacketType.Login)
            {
                Console.WriteLine("..connection accepted.");
                player = CreatePlayer(inc, players);
                inc.SenderConnection.Approve();
                var outmsg = server.CreateMessage();
                outmsg.Write((byte)PacketType.Login);
                outmsg.Write(true);
                outmsg.Write(players.Count);
                for (int n = 0; n < players.Count; n++)
                {
                    outmsg.WriteAllProperties(players[n]);
                    foreach (Personnage p in players[n].Personnages)
                    {
                        outmsg.Write(p.GetType().ToString());
                        outmsg.Write(p.Position.X);
                        outmsg.Write(p.Position.Z);
                        outmsg.Write(p.PtsDeVie);
                    }
                }
                server.SendMessage(outmsg, inc.SenderConnection, NetDeliveryMethod.ReliableOrdered, 0);
                var command = new PlayerPositionCommand();
                command.Run(server, inc, player, players);
            }
            else
            {
                inc.SenderConnection.Deny("Didn't send correct information.");
            }
        }

        private Player CreatePlayer(NetIncomingMessage inc, List<Player> players)
        {
            var player = new Player();
            inc.ReadAllProperties(player);
            for (int i = 0; i < player.Personnages.Capacity; ++i)
            {
                var personnage = InstancierPersonnage(inc.ReadString());
                player.Personnages.Add(personnage as Personnage);
            }
            CreatePosition(player, players);
            players.Add(player);
            return player;
        }

        private Personnage InstancierPersonnage(string type)
        {
            Personnage personnage = null;
            if (type == TypePersonnage.ARCHER)
            {
                personnage = new Archer(null, null, 0, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0, VIE_MAX);
            }
            if (type == TypePersonnage.GUÉRISSEUR)
            {
                personnage = new Guérisseur(null, null, 0, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0, VIE_MAX);
            }
            if (type == TypePersonnage.GUERRIER)
            {
                personnage = new Guerrier(null, null, 0, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0, VIE_MAX);
            }
            if (type == TypePersonnage.MAGE)
            {
                personnage = new Mage(null, null, 0, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0, VIE_MAX);
            }
            if (type == TypePersonnage.PALADIN)
            {
                personnage = new Paladin(null, null, 0, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0, VIE_MAX);
            }
            if (type == TypePersonnage.VOLEUR)
            {
                personnage = new Voleur(null, null, 0, Vector3.Zero, Vector3.Zero, 0, 0, 0, 0, VIE_MAX);
            }
            return personnage;
        }


        private void CreatePosition(Player player, List<Player> players)
        {
            for(int i = 0; i < player.Personnages.Count; ++i)
            {
                player.Personnages[i].GérerPositionObjet(new Vector3(players.Count == 0 ? -POSITION_X_DEPART : POSITION_X_DEPART, 0, POSITION_Z_DEPART + 10 * i));
            }
        }

    }
}
