using UnityEngine;
using System.Collections;

namespace ProcRoom.AI.Abilities
{
    public class Reload : Ability
    {
        public override bool Allowed
        {
            get
            {
                return base.Allowed && monster.ammo == 0;
            }
        }

        public override bool Enact()
        {
            monster.Reload();
            return base.Enact();
        }
    }
}

