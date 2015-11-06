﻿using UnityEngine;
using System.Collections;


namespace ProcRoom
{
    public struct WeaponStats
    {
        public int attack;
        public int maxRange;
        public int accuracyLossPerTile;

        public WeaponStats(int attack, int maxRange, int accuracyLossPerTile)
        {
            this.attack = attack;
            this.maxRange = maxRange;
            this.accuracyLossPerTile = accuracyLossPerTile;
        }

        public static WeaponStats DefaultWeapon
        {
            get
            {
                var stats = new WeaponStats();
                stats.attack = 80;
                stats.maxRange = 1;
                stats.accuracyLossPerTile = 70;
                return stats;
            }
        }
    }

    public class Weapon : MonoBehaviour
    {

        WeaponStats _stats = WeaponStats.DefaultWeapon;

        [SerializeField]
        Projectile bullet;

        bool _isShooting = false;

        public WeaponStats stats
        {
            set
            {
                _stats = value;
            }
        }

        public bool isShooting
        {
            get
            {
                return _isShooting;
            }
        }

        public bool Shoot(Coordinate position, Coordinate lookDirection)
        {
            if (!_isShooting && bullet.Shoot(position, lookDirection, _stats.attack, _stats.maxRange, _stats.accuracyLossPerTile))
            {
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
            if (projectile == bullet)
                _isShooting = false;
        }

    }
}
