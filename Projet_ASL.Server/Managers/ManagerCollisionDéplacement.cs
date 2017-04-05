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
    static class ManagerDéplacement
    {
        const int BORNE_HORIZONTALE = 80;
        const int BORNE_VERTICALE = 50;
        const int DÉPLACEMENT_MAX = 10;
        public static bool CheckDéplacement(Vector3 position, Vector3 déplacement)
        {
            return Vector3.Distance(position, déplacement) <= DÉPLACEMENT_MAX && déplacement.X > -BORNE_HORIZONTALE && déplacement.X < BORNE_HORIZONTALE && déplacement.Z > -BORNE_VERTICALE && déplacement.Z < BORNE_VERTICALE;
        }
    }
}
