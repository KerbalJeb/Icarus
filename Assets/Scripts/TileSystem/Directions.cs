using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace TileSystem
{
    public enum Directions : byte
    {
        Up,
        Left,
        Down,
        Right,
        ZUp,
        ZDown,
        None,
    }

    public static class TileInfo
    {
        public static readonly ReadOnlyDictionary<Directions, Vector2> Directions =
            new ReadOnlyDictionary<Directions, Vector2>(new Dictionary<Directions, Vector2>
            {
                {TileSystem.Directions.Up, Vector2.up},
                {TileSystem.Directions.Down, Vector2.down},
                {TileSystem.Directions.Left, Vector2.left},
                {TileSystem.Directions.Right, Vector2.right},
            });

        public static readonly ReadOnlyDictionary<Directions, Matrix4x4> TransformMatrix =
            new ReadOnlyDictionary<Directions, Matrix4x4>(new Dictionary<Directions, Matrix4x4>
            {
                {TileSystem.Directions.Up, Matrix4x4.Rotate(Quaternion.Euler(0,    0, 0))},
                {TileSystem.Directions.Down, Matrix4x4.Rotate(Quaternion.Euler(0,  0, 180f))},
                {TileSystem.Directions.Left, Matrix4x4.Rotate(Quaternion.Euler(0,  0, 90f))},
                {TileSystem.Directions.Right, Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270f))},
            });
    }
}
