using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileSystem
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "TileParts/Weapon", order = 0)]
    public class Weapon : BasePart
    {
        public Sprite     turret;
        public GameObject turretTemplate;
        public float      baseDamage = 500f;
        public float      range      = 50f;


        public override void Instantiate(Vector3Int cord, Tilemap tilemap, Directions direction)
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

        public override void SetTiles(Vector3Int[] cords, Tilemap tilemap, Directions[] directions)
        {
            for (var i = 0; i < cords.Length; i++) Instantiate(cords[i], tilemap, directions[i]);
        }

        public override void RemoveTiles(Vector3Int[] cords, Tilemap tilemap)
        {
            foreach (Vector3Int cord in cords) Remove(cord, tilemap);
        }

        public override void Remove(Vector3Int cords, Tilemap tilemap)
        {
            base.Remove(cords, tilemap);
            var shipManager = tilemap.transform.parent.GetComponent<ShipManager>();
            shipManager.WeaponsManager.RemoveWeapon(cords);
        }
    }
}
