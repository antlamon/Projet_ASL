//------------------------------------------------------
// 
// Copyright - (c) - 2014 - Mille Boström 
//
// Youtube channel - https://www.youtube.com/user/Maloooon
//------------------------------------------------------
using System.Collections.Generic;
using Projet_ASL;
using Lidgren.Network;

namespace Projet_ASL.Server.Commands
{
    interface ICommand
    {
        void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players);
    }
}
