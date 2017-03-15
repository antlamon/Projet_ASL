﻿//------------------------------------------------------
// 
// Copyright - (c) - 2014 - Mille Boström 
//
// Youtube channel - https://www.youtube.com/user/Maloooon
//------------------------------------------------------
using System;
using System.Collections.Generic;
using Projet_ASL.Library;
using Lidgren.Network;

namespace Projet_ASL.Server.Commands
{
    class LoginCommand : ICommand
    {
        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            Console.WriteLine("New connection...");
            var data = inc.ReadByte();
            if (data == (byte)PacketType.Login)
            {
                Console.WriteLine("..connection accepted.");
                player = CreatePlayer(inc,players);
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
                command.Run(server,inc,player,players);
            }
            else
            {
                inc.SenderConnection.Deny("Didn't send correct information.");
            }
        }

        private Player CreatePlayer(NetIncomingMessage inc, List<Player> players)
        {
            var random = new Random();
            var player = new Player
            {
                Username = inc.ReadString(),
                XPosition = random.Next(0, 750),
                YPosition = random.Next(0, 420)
            };
            players.Add(player);
            return player;
        }

    }
}
