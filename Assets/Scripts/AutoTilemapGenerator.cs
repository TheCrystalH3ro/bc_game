using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class AutoTilemapGenerator : MonoBehaviour
{
    public enum TileMatchMode { ByNameShift, ByPalettePosition }

    public Tilemap bottomTilemap;
    public Tilemap midTilemap;
    public Tilemap topTilemap;

    public Tile[] tilePalette;
    public TileMatchMode matchMode = TileMatchMode.ByNameShift;

    public int tileOffset = 1;

    public int paletteWidth = 8; // Ak používaš ByPalettePosition, nastav počet stĺpcov v palete

    public void GenerateAll()
    {
        GenerateLayer(bottomTilemap, midTilemap, shift: -3);
        GenerateLayer(midTilemap, topTilemap, shift: -3);
    }

    private void GenerateLayer(Tilemap source, Tilemap target, int shift)
    {
        if (source == null || target == null) return;

        BoundsInt bounds = source.cellBounds;
        TileBase[] allTiles = source.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(bounds.x + x, bounds.y + y, 0);
                TileBase sourceTile = allTiles[x + y * bounds.size.x];

                if (sourceTile == null) continue;

                TileBase matchedTile = null;

                if (matchMode == TileMatchMode.ByNameShift)
                {
                    matchedTile = GetTileByNameShift(sourceTile, shift);
                }
                else if (matchMode == TileMatchMode.ByPalettePosition)
                {
                    matchedTile = GetTileByPaletteShift(sourceTile, 0, 1);
                }

                if (matchedTile != null)
                {
                    Vector3Int newPos = new Vector3Int(tilePos.x, tilePos.y + tileOffset, tilePos.z);
                    Debug.Log($"From {tilePos} to {newPos} - setting tile: {matchedTile.name}");
                    target.SetTile(newPos, matchedTile);
                }
            }
        }
    }

    private TileBase GetTileByNameShift(TileBase tile, int shift)
    {
        string name = tile.name;
        if (!name.Contains("_")) return null;

        string prefix = name.Substring(0, name.IndexOf('_') + 1);
        if (int.TryParse(name.Substring(name.IndexOf('_') + 1), out int num))
        {
            int newNum = num + shift;
            string newName = prefix + newNum;

            foreach (Tile t in tilePalette)
            {
                if (t != null && t.name == newName)
                    return t;
            }
        }

        return null;
    }

    private TileBase GetTileByPaletteShift(TileBase tile, int deltaX, int deltaY)
    {
        int index = System.Array.IndexOf(tilePalette, tile);
        if (index == -1) return null;

        int x = index % paletteWidth;
        int y = index / paletteWidth;

        int newX = x + deltaX;
        int newY = y + deltaY;
        int newIndex = newY * paletteWidth + newX;

        if (newIndex >= 0 && newIndex < tilePalette.Length)
        {
            return tilePalette[newIndex];
        }

        return null;
    }
}
