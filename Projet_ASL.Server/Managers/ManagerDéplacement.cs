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
        const int BORNE_HORIZONTALE = 60;
        const int BORNE_VERTICALE = 30;

        public static float CheckDéplacementX(float positionX)
        {
            float positionXExacte;
            if(positionX <= 0)
            {
                positionXExacte = MathHelper.Max(-BORNE_HORIZONTALE, positionX);
            }
            else
            {
                positionXExacte = MathHelper.Min(BORNE_HORIZONTALE, positionX);
            }
            return positionXExacte;
        }
        public static float CheckDéplacementZ(float positionZ)
        {
            float positionZExacte;
            if (positionZ <= 0)
            {
                positionZExacte = MathHelper.Max(-BORNE_VERTICALE, positionZ);
            }
            else
            {
                positionZExacte = MathHelper.Min(BORNE_VERTICALE, positionZ);
            }
            return positionZExacte;
        }
    }
}
