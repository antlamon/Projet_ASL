//------------------------------------------------------
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
    class PlayerPositionCommand : ICommand
    {
        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            if (player != null)
            {
                Console.WriteLine("Sending out new player position");
                var outmessage = server.CreateMessage();
                outmessage.Write((byte) PacketType.PlayerPosition);
                outmessage.WriteAllProperties(player);
                server.SendToAll(outmessage, NetDeliveryMethod.ReliableOrdered);
            }
        }
    }
}
