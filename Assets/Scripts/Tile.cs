using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom
{
    public enum TileType { None, Wall, Walkable, StairsUp, StairsDown, SpikeTrap, Door };

    public delegate void TileAction(Tile tile, TileType typeOfTile, Coordinate position);

    [RequireComponent(typeof(SpriteRenderer)), RequireComponent(typeof(Animator))]
    public class Tile : MonoBehaviour
    {
        public static event TileAction OnTileAction;

        TileType typeOfTile;

        Coordinate _position;

        Animator anim;

        int cycleStep = -1;

        void Awake() {
            anim = GetComponent<Animator>();
        }

        void OnEnable()
        {
            Room.OnRoomGeneration += HandleNewRoom;
            Player.OnPlayerEnterNewPosition += HandlePlayerMove;
        }

        void OnDisable()
        {
            Room.OnRoomGeneration -= HandleNewRoom;
            Player.OnPlayerEnterNewPosition -= HandlePlayerMove;
        }


        private void HandlePlayerMove(Player player, Coordinate position, TileType tileType)
        {
            if (typeOfTile == TileType.SpikeTrap && position.Equals(_position))
            {
                if (cycleStep == 2)
                {
                    Maim();
                    player.Hurt();
                }
            }

        }

        private void HandleNewRoom(Room room, RoomData data)
        {
            cycleStep = -1;

        }

        public TileType tileType
        {
            set
            {
                cycleStep = -1;
                typeOfTile = value;
            }
        }

        public Coordinate position
        {
            set
            {
                _position = value;
            }
        }

        public void Enact(int cycleStep)
        {
            if (typeOfTile == TileType.SpikeTrap)
            {
                if (this.cycleStep < 0)
                    anim.SetTrigger("SpikesRetrackted");


                if (this.cycleStep < 0 && cycleStep == 0)
                    this.cycleStep = 0;

                if (this.cycleStep >= 0)
                    StepSpikesCycle();
            }
            else if (typeOfTile == TileType.StairsDown)
                anim.SetTrigger("StairsDown");
            else if (typeOfTile == TileType.StairsUp)
                anim.SetTrigger("StairsUp");
            else if (typeOfTile == TileType.Walkable)
                anim.SetTrigger("WalkableArea");
            else if (typeOfTile == TileType.Wall)
                anim.SetTrigger("Wall");
            else if (typeOfTile == TileType.Door)
                anim.SetTrigger("DoorClosed");
            else if (typeOfTile != TileType.None)
                anim.SetTrigger("UnknownTile");
        }

        public bool Unlock()
        {
            if (typeOfTile == TileType.Door)
            {
                StartCoroutine(animateUnlock());
                return true;
            }
            return false;
        }

        IEnumerator<WaitForSeconds> animateUnlock()
        {
            anim.SetTrigger("DoorOpen");
            yield return new WaitForSeconds(1f);
            Tower.ActiveRoom.SetTileType(_position.ToPosition(Tower.ActiveRoom.Width, Tower.ActiveRoom.Height), TileType.Walkable);
        }

        void StepSpikesCycle()
        {
            cycleStep += 1;
            cycleStep %= 3;
            switch (cycleStep)
            {
                case 0:
                    anim.ResetTrigger("SpikesPrepare");
                    anim.ResetTrigger("SpikesOut");
                    anim.ResetTrigger("SpikesMaim");
                    anim.SetTrigger("SpikesRetrackted");
                    break;

                case 1:
                    anim.ResetTrigger("SpikesOut");
                    anim.ResetTrigger("SpikesMaim");
                    anim.SetTrigger("SpikesPrepare");
                    break;

                case 2:
                    anim.ResetTrigger("SpikesPrepare");
                    anim.ResetTrigger("SpikesMaim");
                    anim.SetTrigger("SpikesOut");
                    if (OnTileAction != null)
                        OnTileAction(this, typeOfTile, _position);

                    break;
                default:
                    Debug.Log("Uncaught cycle " + cycleStep);
                    break;
            }
        }

        public void Maim()
        {
            anim.SetTrigger("SpikesMaim");
        }

    }
}