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
    class PlayerPositionCommand : ICommand
    {
        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            if (player != null)
            {
                Console.WriteLine("Sending out new player position");
                var outmsg = server.CreateMessage();
                outmsg.Write((byte) PacketType.PlayerPosition);
                outmsg.WriteAllProperties(player);
                foreach (Personnage p in player.Personnages)
                {
                    outmsg.Write(p.GetType().ToString());
                    outmsg.Write(p.Position.X);
                    outmsg.Write(p.Position.Z);
                    outmsg.Write(p.PtsDeVie);
                }
                server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
            }
        }
    }
}
