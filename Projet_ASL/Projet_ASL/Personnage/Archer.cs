using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projet_ASL
{
    public class Archer : Personnage
    {
        const int PORTÉE_ATTAQUE = 100;
        public const int RAYON_PLUIE_DE_FLÈCHES = 10;
        public const int PORTÉE_PLUIE_DE_FLÈCHES = 100;
        const float DÉGATS_PLUIE_DE_FLÈCHES = 0.5f;
        //public const int PORTÉE_FLÈCHE_PERCANTE = 100;
        //const float DÉGATS_FLÈCHE_PERCANTE = 0.7f;
        public const int PORTÉE_FLÈCHE_REBONDISSANTE = 20;
        public const int RAYON_FLÈCHE_REBONDISSANTE = 15;
        const float DÉGÂTS_FLÈCHE_REBONDISSANTE = 0.65f;

        public Archer(Game jeu, string nomModèle, float échelleInitiale, Vector3 rotationInitiale, Vector3 positionInitiale, int force, int dextérité, int intelligence, int sagesse, int ptsDeVie)
            : base(jeu, nomModèle, échelleInitiale, rotationInitiale, positionInitiale, force, dextérité, intelligence, sagesse, ptsDeVie)
        {
        }

        public override int Attaquer()
        {
            return Dextérité;
        }

        public override int GetPortéeAttaque()
        {
            return PORTÉE_ATTAQUE;
        }

        public List<Personnage> PluieDeFlèches(Vector3 positionClic, List<Personnage> CiblesPotentielles, out int dégats)
        {
            List<Personnage> cibles = new List<Personnage>();
            BoundingSphere zone = new BoundingSphere(positionClic, RAYON_PLUIE_DE_FLÈCHES);
            dégats = (int)(DÉGATS_PLUIE_DE_FLÈCHES * Attaquer());

            if (this is Archer)
            {
                foreach (Personnage p in CiblesPotentielles)
                {
                    if (zone.Intersects(p.SphèreDeCollision))
                    { cibles.Add(p); }
                }
            }

            return cibles;
        }

        public Personnage FlècheRebondissante(Personnage cible, List<Personnage> CiblesPotentielles, out int dégâts)
        {
            dégâts = (int)(DÉGÂTS_FLÈCHE_REBONDISSANTE*Attaquer());
            float min = RAYON_FLÈCHE_REBONDISSANTE;
            foreach(Personnage p in CiblesPotentielles)
            {
                float distance = Vector3.Distance(p.Position, cible.Position);
                if (distance < min)
                {
                    min = distance;
                }
            }
            return CiblesPotentielles.FirstOrDefault(p => Vector3.Distance(p.Position, cible.Position) == min);
        }

        //public List<Personnage> FlèchePercante(Vector3 positionClic, List<Personnage> CiblesPotentielles, out int dégats)
        //{
        //    List<Personnage> cibles = new List<Personnage>();
        //    Vector3 direction = positionClic - Position;
        //    Vector3 directionPerpNormaliséeMin = Vector3.Normalize(new Vector3(direction.Z, 0, -direction.X));
        //    Vector3 directionPerpNormaliséeMax = Vector3.Normalize(new Vector3(-direction.Z, 0, direction.X));

        //    Vector3 pointMin = new Vector3(positionClic.X + directionPerpNormaliséeMin.X, 0, positionClic.Z + directionPerpNormaliséeMin.Z);
        //    Vector3 pointMax = new Vector3(Position.X + directionPerpNormaliséeMax.X, 1, Position.Z + directionPerpNormaliséeMax.Z);
        //    BoundingBox portée = new BoundingBox(pointMin, pointMax);
        //    //Ray portée = new Ray(Position, positionClic - Position);
        //    dégats = (int)(DÉGATS_FLÈCHE_PERCANTE * Attaquer());
        //    bool intersection = false;

        //    //SphèreDeCollision.Intersects()
        //    foreach (Personnage p in CiblesPotentielles)
        //    {
        //        intersection = portée.Intersects(p.SphèreDeCollision);
        //        if(intersection)
        //        {
        //            cibles.Add(p);
        //        }
        //    }

        //    if (cibles.Count != 0)
        //    {
        //        cibles.OrderBy(cible => (cible.Position - Position).Length());
        //        cibles.RemoveRange(2, cibles.Count - 2);
        //        int allo = cibles.Capacity;
        //    }

        //    return cibles;
        //}
    }
}
