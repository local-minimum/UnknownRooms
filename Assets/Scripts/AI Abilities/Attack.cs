using UnityEngine;


namespace ProcRoom.AI.Abilities
{

    public class Attack : Ability
    {

        public override bool Allowed
        {
            get
            {
                return base.Allowed && monster.canShootPlayer;
            }
        }

        public override bool Enact()
        {
            
            //Debug.Log("Enacting Attack");
            monster.lookDirection = (monster.player.position - monster.position).asDirection;
            SendMessage("Attack", SendMessageOptions.RequireReceiver);

            return base.Enact();
        }
    }
    
}
