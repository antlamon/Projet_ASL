using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Projet_ASL.Server.Commands
{
    class InvisibleCommand : ICommand
    {
        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            var outMessage = server.CreateMessage();
            outMessage.Write((byte)PacketType.Invisibilité);
            outMessage.Write(inc.ReadString());
            outMessage.Write(inc.ReadBoolean());
            server.SendToAll(outMessage, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
