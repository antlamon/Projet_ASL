using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace Projet_ASL.Server.Commands
{
    class ÉtatSpécialCommand : ICommand
    {
        public void Run(NetServer server, NetIncomingMessage inc, Player player, List<Player> players)
        {
            var outMessage = server.CreateMessage();
            string name = inc.ReadString();
            outMessage.Write((byte)PacketType.ÉtatSpécial);
            outMessage.Write(name);
            player = players.Find(p => p.Username == name);
            string type = inc.ReadString();
            int index = player.Personnages.FindIndex(p => p.GetType().ToString() == type);
            Personnage personnage = player.Personnages[index];
            outMessage.Write(index);
            int compteur = inc.ReadInt32();
            outMessage.Write(compteur);
            for(int i = 0; i < compteur; ++i)
            {
                string nomÉtat = inc.ReadString();
                switch (nomÉtat)
                {
                    case ÉtatSpécial.EN_FEU:
                        personnage.SetEnFeu(inc.ReadBoolean());
                        outMessage.Write(ÉtatSpécial.EN_FEU);
                        outMessage.Write(personnage._EnFeu);
                        break;
                    case ÉtatSpécial.BOUCLIER_DIVIN:
                        personnage.SetBouclierDivin(inc.ReadBoolean());
                        outMessage.Write(ÉtatSpécial.BOUCLIER_DIVIN);
                        outMessage.Write(personnage._BouclierDivin);
                        break;
                    case ÉtatSpécial.FREEZE:
                        personnage.SetFreeze(inc.ReadBoolean());
                        outMessage.Write(ÉtatSpécial.FREEZE);
                        outMessage.Write(personnage._Frozen);
                        break;
                    case ÉtatSpécial.FOLIE:
                        (personnage as Guerrier).SetFolie(inc.ReadBoolean());
                        outMessage.Write(ÉtatSpécial.FOLIE);
                        outMessage.Write((personnage as Guerrier)._Folie);
                        break;
                    case ÉtatSpécial.SATAN:
                        (personnage as Guérisseur).SetSatan(inc.ReadBoolean());
                        outMessage.Write(ÉtatSpécial.FOLIE);
                        outMessage.Write((personnage as Guérisseur)._SatanMode);
                        break;
                }
            }
            server.SendToAll(outMessage, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
