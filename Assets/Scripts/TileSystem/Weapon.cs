using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileSystem
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "TileParts/Weapon", order = 0)]
    public class Weapon : BasePart
    {
        /// <summary>
        ///     The image to use for the turret part of this weapon that will track the target
        /// </summary>
        public Sprite turret;

        /// <summary>
        ///     The gameobject to use as a template for the turret, must have a WeaponFX and SpriteRenderer
        /// </summary>
        public GameObject turretTemplate;

        /// <summary>
        ///     How much damage this weapon does
        /// </summary>
        public float baseDamage = 500f;

        /// <summary>
        ///     The range of the weapon
        /// </summary>
        public float range = 50f;

        /// <summary>
        ///     The delay between shots from this weapons (in seconds)
        /// </summary>
        public float firePeriod = 1f;

        /// <summary>
        ///     Creates a tile at the given coordinates
        /// </summary>
        /// <param name="cord">The position of the engine</param>
        /// <param name="tilemap">The tilemap to use</param>
        /// <param name="direction">The direction it is facing</param>
        public override void Instantiate(Vector3Int cord, Tilemap tilemap, Direction direction)
        {
            base.Instantiate(cord, tilemap, direction);
            GameObject gameObject = Instantiate(turretTemplate,
                                                tilemap.CellToLocal(cord) +
                                                new Vector3(0.5f * tilemap.cellSize.x, 0.5f * tilemap.cellSize.y),
                                                TileInfo.TransformMatrix[direction].rotation,
                                                tilemap.transform);

            var spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite       = turret;
            spriteRenderer.sortingOrder = layer + 1;
            tilemap.transform.parent.GetComponent<ShipManager>().WeaponsManager.AddWeapon(cord, this, gameObject);
        }

        /// <summary>
        ///     Sets multiple tiles
        /// </summary>
        /// <param name="cords">The positions of the tile</param>
        /// <param name="tilemap">The tilemap to use</param>
        /// <param name="directions">The directions it is facing</param>
        public override void SetTiles(Vector3Int[] cords, Tilemap tilemap, Direction[] directions)
        {
            for (var i = 0; i < cords.Length; i++) Instantiate(cords[i], tilemap, directions[i]);
        }

        /// <summary>
        ///     Removes tiles at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinates to remove tiles from</param>
        /// <param name="tilemap">The tilemap to use</param>
        public override void RemoveTiles(Vector3Int[] cords, Tilemap tilemap)
        {
            foreach (Vector3Int cord in cords) Remove(cord, tilemap);
        }

        /// <summary>
        ///     Removes a tile at the given coordinates
        /// </summary>
        /// <param name="cords">The coordinate to remove tiles from</param>
        /// <param name="tilemap">The tilemap to use</param>
        public override void Remove(Vector3Int cords, Tilemap tilemap)
        {
            base.Remove(cords, tilemap);
            var shipManager = tilemap.transform.parent.GetComponent<ShipManager>();
            shipManager.WeaponsManager.RemoveWeapon(cords);
        }

        public void Fire(Transform transform, WeaponFx fx, TileManager tileManager)
        {
            Quaternion dir      = transform.rotation;
            Vector3    startPos = transform.position;
            Vector3    endPos   = startPos + dir * Vector3.up * range;
            var        dmg      = new Damage(startPos, endPos, baseDamage);
            (Vector3 hitPos, Vector3 endHitPos) = dmg.ApplyDamage(new[] {tileManager});
            fx.ApplyFX(startPos, endHitPos, hitPos);
        }
    }
}
