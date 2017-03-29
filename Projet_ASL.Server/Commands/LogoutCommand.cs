using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;
using Projet_ASL.Library;

namespace Projet_ASL.Server.Commands
{
    class LogoutCommand : ICommand
    {
        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            Console.WriteLine("Logout Command");
            DeletePlayer(inc,players);
        }

        private void DeletePlayer(NetIncomingMessage inc, List<Player> players)
        {
            string username = inc.ReadString();
            players.Remove(players.Find(player => player.Username == username));
        }
    }
}
