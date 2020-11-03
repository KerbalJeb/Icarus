using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace TileSystem
{
    public enum TileRotation : byte
    {
        Up,
        Down,
        Left,
        Right,
        TurnUp,
        TurnDown,
        None,
    }

    public static class TileInfo
    {
        public static readonly ReadOnlyDictionary<TileRotation, Vector2> Directions =
            new ReadOnlyDictionary<TileRotation, Vector2>(new Dictionary<TileRotation, Vector2>
            {
                {TileRotation.Up, Vector2.up},
                {TileRotation.Down, Vector2.down},
                {TileRotation.Left, Vector2.left},
                {TileRotation.Right, Vector2.right},
            });

        public static readonly ReadOnlyDictionary<TileRotation, Matrix4x4> TransformMatrix =
            new ReadOnlyDictionary<TileRotation, Matrix4x4>(new Dictionary<TileRotation, Matrix4x4>
            {
                {TileRotation.Up, Matrix4x4.Rotate(Quaternion.Euler(0,    0, 0))},
                {TileRotation.Down, Matrix4x4.Rotate(Quaternion.Euler(0,  0, 180f))},
                {TileRotation.Left, Matrix4x4.Rotate(Quaternion.Euler(0,  0, 90f))},
                {TileRotation.Right, Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270f))},
            });
    }
}
