using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using Projet_ASL;

namespace Projet_ASL.Server.Commands
{
    class LogoutCommand : ICommand
    {
        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            Console.WriteLine("Logout Command");
            string username = inc.ReadString();
            DeletePlayer(username,players);
            var outMessage = server.CreateMessage();
            outMessage.Write((byte)PacketType.Logout);
            outMessage.Write(username);
            server.SendToAll(outMessage, NetDeliveryMethod.ReliableOrdered);
            Console.WriteLine("Logout complete");
        }

        private void DeletePlayer(string username, List<Player> players)
        {
            players.Remove(players.Find(player => player.Username == username));
        }
    }
}
