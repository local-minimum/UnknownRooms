using UnityEngine;


namespace ProcRoom.AI.Abilities
{

    public class Attack : Ability
    {

        public override bool Allowed
        {
            get
            {
                return base.Allowed && monster.weaponRange >= RoomMath.GetManhattanDistance(monster.player.position, monster.position);
            }
        }

        public override bool Enact()
        {
            
            Debug.Log("Enacting Attack");
            SendMessage("Attack", SendMessageOptions.RequireReceiver);

            return base.Enact();
        }
    }
    
}
