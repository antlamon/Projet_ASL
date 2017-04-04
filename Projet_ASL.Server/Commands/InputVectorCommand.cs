﻿using System;
using System.Collections.Generic;
using System.Linq;
using Projet_ASL;
using Projet_ASL.Server.Managers;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Projet_ASL.Server.Commands
{
    class InputVectorCommand : ICommand
    {
        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            Console.WriteLine("Received new inputVector");
            Vector3 déplacement = new Vector3(inc.ReadFloat(), 0, inc.ReadFloat());
            var name = inc.ReadString();
            player = players.FirstOrDefault(p => p.Username == name);
            if (player == null)
            {
                Console.WriteLine("Could not find player with name {0}", name);
                return;
            }
            Personnage pion = player.Personnages[inc.ReadInt32()];

            if (ManagerCollisionDéplacement.CheckCollisionDéplacement(pion.Position,déplacement))
            {
                pion.GérerPositionObjet(déplacement);

                var command = new PlayerPositionCommand();
                command.Run(server, inc, player, players);
            }

        }
    }
}