using UnityEngine;
using System.Collections;

namespace ProcRoom
{
    public class Projectile : MonoBehaviour
    {

        int accuracyLossPerDistance;
        int range;
        int speed;
        Coordinate direction;
        Coordinate position;


        void Shoot(Coordinate from, Coordinate direction, int range, int accuracyLossPerDistance)
        {
            this.accuracyLossPerDistance = accuracyLossPerDistance;
            this.range = range;
            speed = range;
            this.direction = direction;
            position = from;

            PositionShot();
        }

        void PositionShot()
        {
            transform.position = Tower.ActiveRoom.GetTileCentre(position);
            transform.rotation = Quaternion.Euler(0f, 0f, direction.y * -90 + Mathf.Min(direction.x, 0) * -180);
        }
    }

}
