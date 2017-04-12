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
    class AllPlayersCommand : ICommand
    {
        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            Console.WriteLine("Sending full player list");
            var outmsg = server.CreateMessage();
            outmsg.Write((byte)PacketType.AllPlayers);
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
            server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
