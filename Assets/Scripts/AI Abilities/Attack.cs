using UnityEngine;


namespace ProcRoom.AI.Abilities
{

    public class Attack : Ability
    {

        public override bool Enact()
        {

            Debug.Log("Enacting Attack");
            SendMessage("Attack", SendMessageOptions.RequireReceiver);

            return base.Enact();
        }
    }
    
}
