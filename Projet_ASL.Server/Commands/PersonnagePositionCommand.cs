using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Projet_ASL.Server.Commands
{
    class PersonnagePositionCommand : ICommand
    {
        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            if (player != null)
            {
                Console.WriteLine("Sending out new personnage position");
                int index = inc.ReadInt32();
                Personnage p = player.Personnages[index];
                var outmsg = server.CreateMessage();
                outmsg.Write((byte)PacketType.PersonnagePosition);
                outmsg.Write(player.Username);
                outmsg.Write(index);
                outmsg.Write(p.Position.X);
                outmsg.Write(p.Position.Z);
                server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
            }
        }
    }
}
