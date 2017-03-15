﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet_ASL.Library;
using Projet_ASL.Server.Commands;

namespace Projet_ASL.Server
{
    class PacketFactory
    {
        public static ICommand GetCommand(PacketType packetType)
        {
            switch (packetType)
            {
                case PacketType.Login:
                    return new LoginCommand();
                case PacketType.PlayerPosition:
                    return new PlayerPositionCommand();
                case PacketType.AllPlayers:
                    return new AllPlayersCommand();
                case PacketType.Input:
                    return new InputCommand();
                default:
                    throw new ArgumentOutOfRangeException("packetType");
            }
        }
    }
}
