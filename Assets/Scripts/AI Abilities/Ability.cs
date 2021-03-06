﻿using UnityEngine;
using System.Collections;

namespace ProcRoom.AI.Abilities
{
    public enum RequirementTypes { None, Required, NotAllowed};

    public abstract class Ability : MonoBehaviour
    {
        [SerializeField, Range(0, 100)]
        int buildCost;

        [SerializeField, Range(0, 10)]
        int allowancePerTurn;

        protected Monster monster;

        [SerializeField]
        RequirementTypes playerInSight = RequirementTypes.None;

        [SerializeField]
        RequirementTypes playerTracking = RequirementTypes.None;

        [SerializeField, Range(1, 100)]
        int priority = 1;

        public int Priority
        {
            get { return priority; }
        }
        protected int usageCurrentTurn;

        public virtual bool Allowed
        {
            get
            {
                return PlayerInSightValid && PlayerTrackingValid && usageCurrentTurn <= allowancePerTurn;
            }
        }

        bool PlayerInSightValid
        {
            get
            {
                return playerInSight == RequirementTypes.None || playerInSight == (monster.PlayerInSight ? RequirementTypes.Required : RequirementTypes.NotAllowed);
            }
        }

        bool PlayerTrackingValid
        {
            get
            {
                return playerTracking == RequirementTypes.None || playerTracking == (monster.trackingPlayer ? RequirementTypes.Required : RequirementTypes.NotAllowed);
            }
        }

        void OnEnable()
        {
            if (monster == null)
                monster = GetComponentInParent<Monster>();
        }

        virtual public void NewTurn()
        {
            usageCurrentTurn = 0;
        }

        virtual public bool Enact()
        {
            usageCurrentTurn++;
            return true;
        }
    }
}