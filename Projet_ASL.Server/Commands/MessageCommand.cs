using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using Projet_ASL.Library;

namespace Projet_ASL.Server.Commands
{
    class MessageCommand : ICommand
    {
        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            Console.WriteLine("Received new message");
            Console.WriteLine(inc.ReadString());
        }
    }
}
