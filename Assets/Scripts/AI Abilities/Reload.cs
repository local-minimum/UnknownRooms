﻿using UnityEngine;
using System.Collections;

namespace ProcRoom.AI.Abilities
{
    public class Reload : Ability
    {
        public override bool Allowed
        {
            get
            {
                return base.Allowed && monster.Weapon.ammo == 0;
            }
        }

        public override bool Enact()
        {
            monster.Reload();
            base.Enact();
            return false;
        }
    }
}

