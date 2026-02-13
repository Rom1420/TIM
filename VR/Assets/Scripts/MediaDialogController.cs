using System.Collections.Generic;
using UnityEngine;

public class MediaDialogController : MonoBehaviour
{
    [SerializeField] private MediaLibrary library;
    [SerializeField] private MediaTileView tilePrefab;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private bool allowDebugSpawn = true;
    [SerializeField] private bool enableDebugLogs = true;

    public MediaRefUnityEvent OnMediaSelected = new MediaRefUnityEvent();

    private readonly List<MediaTileView> _tiles = new List<MediaTileView>();
    private MediaTileView _currentSelection;
    private Texture _lastSelectedThumbnail;

    public Texture LastSelectedThumbnail => _lastSelectedThumbnail;

    [ContextMenu("DEBUG / Spawn 10 Tiles")]
    private void DebugSpawn10()
    {
        ClearTiles();

        if (tilePrefab == null || contentRoot == null)
        {
            Debug.LogWarning("[MediaDialogController] Missing tilePrefab or contentRoot.");
            return;
        }

        for (int i = 0; i < 10; i++)
        {
            var t = Instantiate(tilePrefab, contentRoot);
            t.name = $"Tile_{i}";
            // Si tu veux voir du texte:
            // t.SetDebugTitle($"Tile {i}");
            _tiles.Add(t);
        }

        Debug.Log("[MediaDialogController] Spawned 10 debug tiles.");
    }


    private void Start()
    {
        Populate();
    }

    public void Populate()
    {
        ClearTiles();

        if (library == null || tilePrefab == null || contentRoot == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[MediaDialogController] Populate missing refs. Library set: {library != null}, TilePrefab set: {tilePrefab != null}, ContentRoot set: {contentRoot != null}", this);
            }

            if (allowDebugSpawn)
            {
                DebugSpawn10();
            }
            return;
        }

        List<MediaRef> media = library.BuildLibrary();
        if (enableDebugLogs)
        {
            Debug.Log($"[MediaDialogController] Populate. Media count: {media.Count}", this);
        }
        foreach (MediaRef mediaRef in media)
        {
            MediaTileView tile = Instantiate(tilePrefab, contentRoot);
            tile.Initialize(mediaRef);
            tile.Selected += HandleTileSelected;
            _tiles.Add(tile);
            if (enableDebugLogs)
            {
                Debug.Log($"[MediaDialogController] Tile created: {tile.name} for media: {mediaRef.displayName}", tile);
            }
        }
    }

    private void ClearTiles()
    {
        foreach (MediaTileView tile in _tiles)
        {
            if (tile != null)
            {
                tile.Selected -= HandleTileSelected;
                Destroy(tile.gameObject);
            }
        }

        _tiles.Clear();
        _currentSelection = null;
        _lastSelectedThumbnail = null;
    }

    private void HandleTileSelected(MediaTileView tile)
    {
        if (tile == null)
        {
            return;
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[MediaDialogController] Tile selected: {tile.name}. Media set: {tile.Media != null}", tile);
        }
        SelectTile(tile);
    }

    public void SelectTile(MediaTileView tile)
    {
        if (tile == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("[MediaDialogController] SelectTile called with null tile.");
            }
            return;
        }

        if (_currentSelection == tile)
        {
            return;
        }

        if (_currentSelection != null)
        {
            _currentSelection.SetSelected(false);
        }

        _currentSelection = tile;
        _currentSelection.SetSelected(true);
        _lastSelectedThumbnail = GetThumbnailForTile(_currentSelection);
        if (enableDebugLogs && _lastSelectedThumbnail == null)
        {
            Debug.LogWarning($"[MediaDialogController] No thumbnail found for media: {_currentSelection.Media?.displayName}", this);
        }
        OnMediaSelected.Invoke(_currentSelection.Media);
    }

    private Texture GetThumbnailForTile(MediaTileView tile)
    {
        if (tile == null)
        {
            return null;
        }

        Texture thumbnail = tile.ThumbnailTexture;
        if (thumbnail is RenderTexture renderTexture)
        {
            return CopyRenderTexture(renderTexture);
        }

        return thumbnail;
    }

    private Texture2D CopyRenderTexture(RenderTexture source)
    {
        if (source == null || source.width <= 0 || source.height <= 0)
        {
            return null;
        }

        RenderTexture previous = RenderTexture.active;
        try
        {
            RenderTexture.active = source;
            Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            texture.Apply();
            return texture;
        }
        finally
        {
            RenderTexture.active = previous;
        }
    }
}
