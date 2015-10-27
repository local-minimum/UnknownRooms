using UnityEngine;
using System.Collections;

namespace ProcRoom
{
    public enum TileType { None, Wall, Walkable, StairsUp, StairsDown, SpikeTrap };

    public delegate void TileAction(Tile tile, TileType typeOfTile, Coordinate position);

    [RequireComponent(typeof(SpriteRenderer)), RequireComponent(typeof(Animator))]
    public class Tile : MonoBehaviour
    {
        public static event TileAction OnTileAction;

        TileType typeOfTile;

        Coordinate _position;

        Animator anim;

        int previousCycle = -1;

        void Awake() {
            anim = GetComponent<Animator>();
        }

        void OnEnable()
        {
            Room.OnRoomGeneration += HandleNewRoom;
        }

        void OnDisable()
        {
            Room.OnRoomGeneration -= HandleNewRoom;
        }

        private void HandleNewRoom(Room room, RoomData data)
        {
            previousCycle = -1;

        }

        public TileType tileType
        {
            set
            {
                previousCycle = -1;
                typeOfTile = value;
                if (value == TileType.SpikeTrap)
                    anim.SetTrigger("SpikesRetrackted");
                else if (value == TileType.StairsDown)
                    anim.SetTrigger("StairsDown");
                else if (value == TileType.StairsUp)
                    anim.SetTrigger("StairsUp");
                else if (value == TileType.Walkable)
                    anim.SetTrigger("WalkableArea");
                else if (value == TileType.Wall)
                    anim.SetTrigger("Wall");
                else if (value != TileType.None)
                    anim.SetTrigger("UnknownTile");
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
                if (previousCycle < 0)
                    anim.SetTrigger("SpikesRetrackted");


                if (previousCycle < 0 && cycleStep == 0)
                    previousCycle = 0;
               
                if (previousCycle >= 0)
                    StepSpikesCycle();
            }
        }

        void StepSpikesCycle()
        {
            previousCycle += 1;
            previousCycle %= 3;
            switch (previousCycle)
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
                    Debug.Log("Uncaught cycle " + previousCycle);
                    break;
            }
        }

        public void Maim()
        {
            anim.SetTrigger("SpikesMaim");
        }

    }
}