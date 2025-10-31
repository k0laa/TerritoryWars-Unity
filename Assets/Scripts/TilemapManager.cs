using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviourPunCallbacks
{
    public Tilemap tilemap;
    public TileBase[] tiles;
    public Dictionary<Vector2Int, Vector2Int> TilemapValues = new Dictionary<Vector2Int, Vector2Int>();

    // Tilemap Deðer Güncelleme
    [PunRPC]
    public void UpdateTilemapValue(int x, int y, int colorIndex, int pwId, bool isdoubleScore)
    {
        Vector2Int key = new Vector2Int(x, y);
        TilemapValues[key] = new Vector2Int(colorIndex, pwId);

        if (isdoubleScore)
        {
            Vector2Int doubleKey = new Vector2Int(x * 1000, y * 1000);
            TilemapValues[doubleKey] = new Vector2Int(colorIndex, pwId);
        }

    }

    // Grid Boyama
    [PunRPC]
    void RPC_PaintTile(int x, int y, int color)
    {
        Vector3Int cellPos = new Vector3Int(x, y, 0);
        tilemap.SetTile(cellPos, tiles[color]);
    }

    // Tüm Tilemap'i Senkronize Etme
    [PunRPC]
    public void SyncAllTiles()
    {
        foreach (var kvp in TilemapValues)
        {
            Vector3Int cellPos = new Vector3Int(kvp.Key.x, kvp.Key.y, 0);
            FindObjectOfType<Tilemap>().SetTile(cellPos, tiles[kvp.Value.x]);
        }
    }

    // Oyuncu Ayrýldýðýnda Ýlgili Tile'larý Temizleme
    [PunRPC]
    public void ClearTileForLeftPlayer(int leftPwId)
    {
        List<Vector2Int> keysToRemove = new List<Vector2Int>();

        foreach (var kvp in TilemapValues)
        {
            if (kvp.Value.y == leftPwId)
            {
                Vector3Int cellPos = new Vector3Int(kvp.Key.x, kvp.Key.y, 0);
                tilemap.SetTile(cellPos, null);
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (Vector2Int key in keysToRemove)
        {
            TilemapValues.Remove(key);
        }
    }

    // Tüm Tilemap'i Temizleme
    [PunRPC]
    void RPC_ClearAllTilemap()
    {
        tilemap.ClearAllTiles();
        TilemapValues.Clear();
    }
}
