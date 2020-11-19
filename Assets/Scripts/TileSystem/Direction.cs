using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace TileSystem
{
    /// <summary>
    ///     Direction of tiles
    /// </summary>
    public enum Direction : byte
    {
        Up,
        Left,
        Down,
        Right,
        ZUp,
        ZDown,
        None,
    }

    /// <summary>
    ///     Some useful mappings of the Direction Enum
    /// </summary>
    public static class TileInfo
    {
        /// <value>
        ///     Maps the direction to a vector2 in the same direction
        /// </value>
        public static readonly ReadOnlyDictionary<Direction, Vector2> Directions =
            new ReadOnlyDictionary<Direction, Vector2>(new Dictionary<Direction, Vector2>
            {
                {Direction.Up, Vector2.up},
                {Direction.Down, Vector2.down},
                {Direction.Left, Vector2.left},
                {Direction.Right, Vector2.right},
            });

        /// <value>
        ///     Maps the direction to a rotation matrix
        /// </value>
        public static readonly ReadOnlyDictionary<Direction, Matrix4x4> TransformMatrix =
            new ReadOnlyDictionary<Direction, Matrix4x4>(new Dictionary<Direction, Matrix4x4>
            {
                {Direction.Up, Matrix4x4.Rotate(Quaternion.Euler(0,    0, 0))},
                {Direction.Down, Matrix4x4.Rotate(Quaternion.Euler(0,  0, 180f))},
                {Direction.Left, Matrix4x4.Rotate(Quaternion.Euler(0,  0, 90f))},
                {Direction.Right, Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270f))},
            });
    }
}
