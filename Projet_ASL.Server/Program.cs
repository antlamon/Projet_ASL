﻿//------------------------------------------------------
// 
// Copyright - (c) - 2014 - Mille Boström 
//
// Youtube channel - https://www.youtube.com/user/Maloooon
//------------------------------------------------------
using System;
using System.Collections.Generic;
using Projet_ASL.Library;
using Lidgren.Network;

namespace Projet_ASL.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server(); 
            server.Run();
        }
    }
}