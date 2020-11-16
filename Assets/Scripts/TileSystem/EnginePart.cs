using UnityEngine;

namespace TileSystem
{
    /// <summary>
    ///     The engine tile variant type
    /// </summary>
    [CreateAssetMenu(fileName = "Engine", menuName = "TileParts/Engine", order = 1)]
    public class EnginePart : BasePart
    {
        public float thrust;
    }
}
