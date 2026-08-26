using UnityEngine;
using UnityEngine.Tilemaps;

public class TriAuto : MonoBehaviour
{
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    
    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        tilemapRenderer = GetComponent<TilemapRenderer>();
        
        if (tilemap != null)
        {
            tilemap.tileAnchor = new Vector2(1f, 1.9f);
        }
        
        if (tilemapRenderer != null)
        {
            tilemapRenderer.sortingOrder = 0;
            tilemapRenderer.mode = TilemapRenderer.Mode.Chunk;
            
            // Force un chunk assez grand pour contenir l'arbre entier
            tilemapRenderer.chunkSize = new Vector3Int(10, 10, 10);
        }
    }
}