using UnityEngine;


namespace ProcRoom.AI.Abilities
{

    public class Attack : Ability
    {

        public override bool Enact()
        {
            if (Allowed && base.Enact())
            {
                SendMessage("Attack");
                return true;
            }
            return false;
        }
    }
    
}
