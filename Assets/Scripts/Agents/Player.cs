﻿using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom
{
    public delegate void PlayerEnterPosition(Player player, Coordinate position, TileType tileType);

    public enum PlayerActions {None, MoveLeft, MoveRight, MoveUp, MoveDown, Shoot, Reload, Shield, EndTurn};

    [RequireComponent(typeof(Animator))]
    public class Player : Agent
    {

        public static event PlayerEnterPosition OnPlayerEnterNewPosition;

        [SerializeField]
        int minDistanceSpawnInFirstLevel = 8;

        [SerializeField]
        RectTransform HUDtransform;

        float cancelRequest = 0;
        
        void Start()
        {
            alive = false;
        
        }

        protected override void HandleNewRoom(Room room, RoomData data)
        {
            base.HandleNewRoom(room, data);
            UpdatePosition(Coordinate.FromPosition(PlayerSpawnPosition(data), roomWidth));
        }

        int PlayerSpawnPosition(RoomData data)
        {
            if (RoomSearch.HasAnyOfType(TileType.StairsDown, data.tileTypeMap))
            {
                return RoomSearch.GetFirstOccurance(data.tileTypeMap, TileType.StairsDown);
            }
            else
            {

                var candidates = RoomSearch.GetPositionsAtDistance(data.tileTypeMap,
                    RoomSearch.GetFirstOccurance(data.tileTypeMap, TileType.StairsUp),
                    new Range(minDistanceSpawnInFirstLevel), TileType.Walkable, false, data.width);

                return candidates[Random.Range(0, candidates.Count)];
            }

        }

        void Update()
        {
            ActOnInput(InputAsAction());
            if (Input.GetKey(KeyCode.Escape))
            {
                if (Time.realtimeSinceStartup - cancelRequest < 1f)
                    Application.Quit();
                else
                    cancelRequest = Time.realtimeSinceStartup;
            }
        }

        PlayerActions InputAsAction()
        {
            if (Input.GetButton("right"))
                return PlayerActions.MoveRight;
            else if (Input.GetButton("left"))
                return PlayerActions.MoveLeft;
            else if (Input.GetButton("up"))
                return PlayerActions.MoveUp;
            else if (Input.GetButton("down"))
                return PlayerActions.MoveDown;
            else if (Input.GetButton("reload"))
                return PlayerActions.Reload;
            else if (Input.GetButton("shoot"))
                return PlayerActions.Shoot;
            else if (Input.GetButton("endTurn"))
                return PlayerActions.EndTurn;
            return PlayerActions.None;
        }

        void ActOnInput(PlayerActions action) { 
            if (!myTurn || action == PlayerActions.None || !actionAllowed)            
                return;


            if (action == PlayerActions.MoveRight)
            {
                if (_stats.lookDirection.Equals(Coordinate.Right))
                    AttemptMoveTo(_stats.position.RightSide());
                else
                {
                    lookDirection = Coordinate.Right;

                }
            }
            else if (action == PlayerActions.MoveLeft)
            {
                if (_stats.lookDirection.Equals(Coordinate.Left))
                    AttemptMoveTo(_stats.position.LeftSide());
                else
                {
                    lookDirection = Coordinate.Left;

                }
            }
            else if (action == PlayerActions.MoveUp)
            {
                if (_stats.lookDirection.Equals(Coordinate.Up))
                    AttemptMoveTo(_stats.position.UpSide());
                else
                {
                    lookDirection = Coordinate.Up;
                }
            }
            else if (action == PlayerActions.MoveDown)
            {
                if (_stats.lookDirection.Equals(Coordinate.Down))
                    AttemptMoveTo(_stats.position.DownSide());
                else
                {
                    lookDirection = Coordinate.Down;
                }
            }

            else if (action == PlayerActions.EndTurn)
                actionPoints = 0;
            else if (action == PlayerActions.Reload)
                Reload();
            else if (action == PlayerActions.Shoot)
                Attack();
        }

        public void MoveTo(Coordinate newPosition) {
            if (actionPoints > 0  && (newPosition - position).Distance == 1)
                AttemptMoveTo(newPosition);
        }

        void AttemptMoveTo(Coordinate newPosition)
        {
            var tileType = room.GetTileTypeAt(newPosition);

            if (room.PassableTile(newPosition) || tileType == TileType.StairsUp) {

                actionTick();
                if (tileType == TileType.StairsUp)
                    actionPoints = 0;
                else
                    actionPoints--;
                UpdatePosition(newPosition);
                if (OnPlayerEnterNewPosition != null)
                    OnPlayerEnterNewPosition(this, newPosition, tileType);

                _stats.steps++;
            } else if (_stats.hasKey && room.PassableTile(newPosition, false, TileType.Door) && room.GetTile(newPosition).Unlock())
            {
                ConsumeKey();
            }

        }

        protected override void Death()
        {
            Game.ReportScore(_stats);
        }

        protected override void OnEnable()
        {
            Tower.OnNewLevel += SetDeepestLevel;
            base.OnEnable();
            if (Input.touchSupported)
                TouchControls.OnTouchEvent += HandleTouch;
        }

        protected override void OnDisable()
        {
            Tower.OnNewLevel -= SetDeepestLevel;
            base.OnDisable();
            if (Input.touchSupported)
                TouchControls.OnTouchEvent -= HandleTouch;
        }

        private void SetDeepestLevel(int level)
        {
            _stats.maxLevel = Mathf.Max(level, _stats.maxLevel);
        }

        private void HandleTouch(TouchAction action, Vector2 position)
        {
            if (!Tower.Alive || RectTransformUtility.RectangleContainsScreenPoint(HUDtransform, position))
                return;
            
            if (action == TouchAction.SwipeDown)
                ActOnInput(PlayerActions.MoveUp);
            else if (action == TouchAction.SwipeUp)
                ActOnInput(PlayerActions.MoveDown);
            else if (action == TouchAction.SwipeLeft)
                ActOnInput(PlayerActions.MoveLeft);
            else if (action == TouchAction.SwipeRight)
                ActOnInput(PlayerActions.MoveRight);
            else if (action == TouchAction.DoubleTap)
            {
                //Dash
            }
            else if (action == TouchAction.Tap)
            {
                //TODO: Add if tapping end turn button
                ActOnInput(weapon.hasAmmo ? PlayerActions.Shoot : PlayerActions.Reload);
            }
                

        }

    }
}