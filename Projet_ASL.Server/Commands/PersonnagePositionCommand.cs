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
                //Console.WriteLine("Sending out new personnage position");
                //var outmsg = server.CreateMessage();
                //outmsg.Write((byte)PacketType.PersonnagePosition);
                //outmsg.Write(player.Username);
                //outmsg.Write(p.GetType().ToString());
                //outmsg.Write(p.Position.X);
                //outmsg.Write(p.Position.Z);
                //outmsg.Write(p.PtsDeVie);
                //server.SendToAll(outmsg, NetDeliveryMethod.ReliableOrdered);
            }
        }
    }
}
