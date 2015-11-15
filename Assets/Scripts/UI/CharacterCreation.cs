﻿using UnityEngine;
using System.Collections;

namespace ProcRoom.UI
{

    public delegate void NewPoints(int points);

    public class CharacterCreation : MonoBehaviour
    {

        [SerializeField, Range(1, 100)]
        int points;

        static CharacterCreation _instance;

        static CharacterCreation instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<CharacterCreation>();
                return _instance;
            }
        }

        public static event NewPoints OnNewPoints;


        public static void NewTransaction(int cost)
        {
            instance.points -= cost;
            if (OnNewPoints != null)
                OnNewPoints(instance.points);
        }

        public static int Points
        {
            get
            {
                return instance.points;
            }
        }
    }
}