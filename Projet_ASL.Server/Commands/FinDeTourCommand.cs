using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Projet_ASL.Server.Commands
{
    class FinDeTourCommand : ICommand
    {
        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            Console.WriteLine("Fin de Tour");
            var outMessage = server.CreateMessage();
            outMessage.Write((byte)PacketType.FinDeTour);
            server.SendToAll(outMessage, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
