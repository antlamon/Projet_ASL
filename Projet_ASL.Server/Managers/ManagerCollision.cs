﻿//------------------------------------------------------
// 
// Copyright - (c) - 2014 - Mille Boström 
//
// Youtube channel - https://www.youtube.com/user/Maloooon
//------------------------------------------------------
using System.Collections.Generic;
using Projet_ASL.Library;
using Microsoft.Xna.Framework;

namespace Projet_ASL.Server.Managers
{
    class ManagerCollision
    {
        public static bool CheckCollision(Rectangle rec, string username, List<Player> players)
        {
            foreach (var player in players)
            {
                if (player.Username != username)
                {
                    var playerRec = new Rectangle(player.XPosition, player.YPosition, 100, 50);
                    if (playerRec.Intersects(rec))
                    {
                        return true; 
                    }
                }
            }
            return false; 
        }
    }
}
