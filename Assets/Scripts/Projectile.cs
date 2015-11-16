using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom
{
    public delegate void ProjectileHit(Projectile projectile, Coordinate position);
    public delegate void ProjectileLaunch(Projectile projectile);

    public class Projectile : MonoBehaviour
    {

        public static event ProjectileHit OnProjectileHit;
        public static event ProjectileLaunch OnProjectileLaunch;

        int accuracyLossPerDistance;
        int _range;
        int speed;
        int attack;
        Coordinate direction;
        Coordinate position;
        LineRenderer rend;
        bool shooting;
        Vector3 headTarget;
        Vector3 headSource;
        Vector3 head;
        Vector3 tail;
        [SerializeField]
        Vector3 offset;
        
        public int power
        {
            get
            {
                return attack;
            }
        }

        void Awake()
        {
            rend = GetComponent<LineRenderer>();
            rend.enabled = false;
        }

      
        public bool Shoot(Coordinate from, Coordinate direction, int attack, int range, int accuracyLossPerDistance)
        {
            if (shooting)
                return false;
            this.accuracyLossPerDistance = accuracyLossPerDistance;
            this._range = range;
            this.attack = attack;
            speed = range;
            this.direction = direction;
            position = from;
            shooting = true;
            if (OnProjectileLaunch != null)
                OnProjectileLaunch(this);
            StartCoroutine(_shoot());
            return true;
        }

        bool SetHead(ref float progress)
        {
            bool newStep = false;
            if (progress >= 1)
            {
                progress %= 1f;
                headSource = headTarget;
                UpdateStats();
                headTarget = Tower.ActiveRoom.GetTileCentre(position) + offset;
                newStep = true;
            }
            head = Vector3.Lerp(headSource, headTarget, progress);
            rend.SetPosition(0, head);
            return newStep;
        }

        void UpdateStats()
        {
            position += direction;
            _range--;
            attack = Mathf.Max(0, attack - accuracyLossPerDistance);
        }

        void SetTail()
        {
            rend.SetPosition(1, Vector3.Lerp(tail, head, 0.8f));
        }

        IEnumerator<WaitForSeconds> _shoot()
        {
            tail = Tower.ActiveRoom.GetTileCentre(position) + offset;
            headTarget = tail;
            float progress = 1.05f;            
            SetHead(ref progress);
            SetTail();
            float animationSpeed = 0.01f;
            //transform.rotation = Quaternion.Euler(0f, 0f, direction.y * -90 + Mathf.Min(direction.x, 0) * -180);
            rend.enabled = true;
            yield return new WaitForSeconds(animationSpeed);
            bool lastTile = false;
            while (_range >= 0)
            {

                if (SetHead(ref progress))
                {
                    lastTile = true;
                }
                SetTail();
                progress += 0.4f;
                if (progress > 1f && (!Tower.ActiveRoom.PassableTile(position) || attack == 0))
                    break;

                yield return new WaitForSeconds(animationSpeed);
            }
            yield return new WaitForSeconds(0.3f);
            rend.enabled = false;
            shooting = false;
            if (OnProjectileHit != null)
                OnProjectileHit(this, position);
        }
    }

}
