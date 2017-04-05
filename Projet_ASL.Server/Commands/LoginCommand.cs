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

namespace Projet_ASL.Server.Commands
{
    class LoginCommand : ICommand
    {
        const int POSITION_X_DEPART = 70;
        const int POSITION_Y_DEPART = -15;
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
            CreatePosition(player, players);
            players.Add(player);
            return player;
        }

        private void CreatePosition(Player player, List<Player> players)
        {
            for(int i = 0; i < player.Personnages.Count; ++i)
            {
                player.Personnages[i].GérerPositionObjet(new Microsoft.Xna.Framework.Vector3(players.Count == 0 ? -POSITION_X_DEPART : POSITION_X_DEPART, 0, POSITION_Y_DEPART + 10 * i));
            }
        }

    }
}
