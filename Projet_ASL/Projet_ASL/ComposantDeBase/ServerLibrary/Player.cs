//------------------------------------------------------
// 
// Copyright - (c) - 2014 - Mille Boström 
//
// Youtube channel - https://www.youtube.com/user/Maloooon
//------------------------------------------------------
using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;


namespace Projet_ASL
{
    public class Player
    {
        const int NB_PERSONNAGE = 4;
        public string Username { get; set; }
        public List<Personnage> Personnages { get; set; }

        public Player(string username)
        {
            Username = username;
            Personnages = new List<Personnage>(NB_PERSONNAGE);
        }

        public Player()
        {
            Personnages = new List<Personnage>(NB_PERSONNAGE);
        }

    }
}
