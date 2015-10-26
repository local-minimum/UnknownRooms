using UnityEngine;
using System.Collections;

namespace ProcRoom
{
    public enum TileType { None, Wall, Walkable, StairsUp, StairsDown, SpikeTrap };

    [RequireComponent(typeof(SpriteRenderer))]
    public class Tile : MonoBehaviour
    {

        [SerializeField]
        Color32[] debugTileColors;

        public TileType tileType
        {
            set
            {
                var renderer = GetComponent<SpriteRenderer>();
                renderer.color = debugTileColors.Length > (int)value ? debugTileColors[(int)value] : (Color32) Color.grey;
            }
        }
    }
}