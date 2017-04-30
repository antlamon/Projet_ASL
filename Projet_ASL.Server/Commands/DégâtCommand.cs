﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Projet_ASL.Server.Commands
{
    class DégâtCommand : ICommand
    {
        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            var name = inc.ReadString();
            player = players.FirstOrDefault(p => p.Username == name);
            int dégât = inc.ReadInt32();
            int nbTouchés = inc.ReadInt32();
            var outMessage = server.CreateMessage();
            outMessage.Write((byte)PacketType.Dégât);
            outMessage.Write(player.Username);
            outMessage.Write(nbTouchés);
            for (int i = 0; i < nbTouchés; ++i)
            {
                string type = inc.ReadString();
                int index = player.Personnages.FindIndex(p => p.GetType().ToString() == type);
                player.Personnages[index].ModifierVitalité(dégât);
                outMessage.Write(index);
                outMessage.Write(player.Personnages[index].PtsDeVie);
            }
            server.SendToAll(outMessage, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
