using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Projet_ASL;
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
                case PacketType.Message:
                    return new MessageCommand();
                case PacketType.Logout:
                    return new LogoutCommand();
                case PacketType.InputVector:
                    return new InputVectorCommand();
                case PacketType.PersonnagePosition:
                    return new PersonnagePositionCommand();
                case PacketType.FinDeTour:
                    return new FinDeTourCommand();
                default:
                    throw new ArgumentOutOfRangeException("packetType");
            }
        }
    }
}
