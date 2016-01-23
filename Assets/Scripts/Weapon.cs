using UnityEngine;
using System.Collections;


namespace ProcRoom
{
    [System.Serializable]
    public struct WeaponStats
    {
        public int precision;
        public int maxRange;
        public int precisionLossPerTile;
        public int clipSize;
        public int ammo;
        public bool melee;
        public int critChance;

        public WeaponStats(int precision, int maxRange, int precisionLossPerTile, int clipSize)
        {
            this.precision = precision;
            this.maxRange = maxRange;
            this.precisionLossPerTile = precisionLossPerTile;
            this.clipSize = clipSize;
            ammo = clipSize;
            melee = false;
            critChance = 0;
        }

        public WeaponStats(int precision, int critChance, int clipSize)
        {
           
            this.precision = precision;
            this.critChance = critChance;
            maxRange = 1;
            precisionLossPerTile = 0;
            this.clipSize = clipSize;
            ammo = clipSize;
            melee = true;
            
        }

        public static WeaponStats DefaultWeapon
        {
            get
            {
                var stats = new WeaponStats();
                stats.precision = 80;
                stats.maxRange = 1;
                stats.precisionLossPerTile = 70;
                return stats;
            }
        }

        public static bool operator ==(WeaponStats A, WeaponStats B)
        {
            return Object.ReferenceEquals(A, B) || A.precision == B.precision && A.precisionLossPerTile == B.precisionLossPerTile && A.maxRange == B.maxRange;
        }

        public static bool operator !=(WeaponStats A, WeaponStats B)
        {
            return A.precision != B.precision || A.precisionLossPerTile != B.precisionLossPerTile || A.maxRange != B.maxRange;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }

    public static class WeaponStatsHelpers
    {
        public static WeaponStats copy(this WeaponStats stats)
        {
            var ws = new WeaponStats();
            ws.critChance = stats.critChance;
            ws.ammo = stats.ammo;
            ws.clipSize = stats.clipSize;
            ws.maxRange = stats.maxRange;
            ws.melee = stats.melee;
            ws.precision = stats.precision;
            ws.precisionLossPerTile = stats.precisionLossPerTile;
            return ws;
        }
    }

    public delegate void WeaponAmmoChange(Weapon weapon);

    public class Weapon : MonoBehaviour
    {

        public static event WeaponAmmoChange OnAmmoChange;

        [SerializeField]
        WeaponStats _stats = WeaponStats.DefaultWeapon;

        [SerializeField]
        Projectile _bullet;

        bool _isShooting = false;

        public WeaponStats Stats
        {
            get
            {
                return _stats.copy();
            }
        }

        public bool melee
        {
            get
            {
                return _stats.melee;

            }
        }


        public bool isShooting
        {
            get
            {
                return _isShooting;
            }
        }

        public int range
        {
            get {
                return _stats.maxRange;
            }

        }

        public int ammo
        {
            get
            {
                return _stats.ammo;
            }

            set
            {
                var ammo = Mathf.Clamp(value, 0, _stats.clipSize);
                if (_stats.ammo != ammo)
                {
                    _stats.ammo = ammo;
                    if (OnAmmoChange != null)
                        OnAmmoChange(this);
                }
            }
        }

        public int clipSize
        {
            get
            {
                return _stats.clipSize;
            }
        }

        public bool ammoIsFull
        {
            get
            {
                return _stats.ammo == _stats.clipSize;
            }
        }

        public bool hasAmmo
        {
            get
            {
                return _stats.ammo > 0;
            }
        }

        
        public Projectile bullet
        {
            get
            {
                return _bullet;
            }
        }

        public bool Shoot(Coordinate position, Coordinate lookDirection)
        {
            if (!_isShooting && hasAmmo && _bullet.Shoot(position, lookDirection, _stats.precision, _stats.maxRange, _stats.precisionLossPerTile))
            {
                ammo--;
                _isShooting = true;
                return true;
            }
            return false;
        }

        void OnEnable()
        {
            Projectile.OnProjectileHit += HandleShotHit;
        }

        void OnDisable() {
            Projectile.OnProjectileHit -= HandleShotHit;
        }


        private void HandleShotHit(Projectile projectile, Coordinate position)
        {
            if (projectile == _bullet)
                _isShooting = false;
        }

        public void SetStats(WeaponStats stats)
        {
            _stats = stats;
            if (OnAmmoChange == null)
                OnAmmoChange(this);
        }

        public void Reload()
        {
            _stats.ammo = _stats.clipSize;
        }
    }
}
