//------------------------------------------------------
// 
// Copyright - (c) - 2014 - Mille Boström 
//
// Youtube channel - https://www.youtube.com/user/Maloooon
//------------------------------------------------------
using System.Collections.Generic;
using Projet_ASL;
using Microsoft.Xna.Framework;
using System;

namespace Projet_ASL.Server.Managers
{
    static class ManagerCollisionDéplacement
    {
        const int DÉPLACEMENT_MAX = 10;
        public static bool CheckCollisionDéplacement(Vector3 position, Vector3 déplacement)
        {
            return Vector3.Distance(position, déplacement) <= DÉPLACEMENT_MAX;
        }
    }
}
