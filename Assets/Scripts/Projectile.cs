using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom
{
    public delegate void ProjectileHit(Projectile projectile, Coordinate position);

    public class Projectile : MonoBehaviour
    {

        public static event ProjectileHit OnProjectileHit;

        int accuracyLossPerDistance;
        int range;
        int speed;
        int attack;
        Coordinate direction;
        Coordinate position;
        Renderer rend;
        bool shooting;

        void Awake()
        {
            rend = GetComponent<Renderer>();
            rend.enabled = false;
        }

      
        public bool Shoot(Coordinate from, Coordinate direction, int attack, int range, int accuracyLossPerDistance)
        {
            if (shooting)
                return false;
            this.accuracyLossPerDistance = accuracyLossPerDistance;
            this.range = range;
            this.attack = attack;
            speed = range;
            this.direction = direction;
            position = from;
            shooting = true;
            StartCoroutine(_shoot());
            return true;
        }

        void PositionShot()
        {
            transform.position = Tower.ActiveRoom.GetTileCentre(position);
            transform.rotation = Quaternion.Euler(0f, 0f, direction.y * -90 + Mathf.Min(direction.x, 0) * -180);
        }

        IEnumerator<WaitForSeconds> _shoot()
        {
            //TODO: Add transition animations
            float animationSpeed = 0.25f;
            PositionShot();
            rend.enabled = true;
            yield return new WaitForSeconds(animationSpeed);
            while (range > 0)
            {
                position += direction;
                PositionShot();
                range--;
                attack = Mathf.Max(0, attack - accuracyLossPerDistance);
                if (!Tower.ActiveRoom.PassableTile(position))
                    break;
                yield return new WaitForSeconds(animationSpeed);
            }
            rend.enabled = false;
            shooting = false;
            if (OnProjectileHit != null)
                OnProjectileHit(this, position);
        }
    }

}
