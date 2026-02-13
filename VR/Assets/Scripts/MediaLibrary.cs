using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class MediaLibrary : MonoBehaviour
{
    [SerializeField] private string imagesResourcesPath = "Images";
    [SerializeField] private string videosResourcesPath = "Videos";
    [SerializeField] private bool enableDebugLogs = true;

    public List<MediaRef> BuildLibrary()
    {
        var media = new List<MediaRef>();

        Texture2D[] images = Resources.LoadAll<Texture2D>(imagesResourcesPath);
        if (enableDebugLogs)
        {
            Debug.Log($"[MediaLibrary] Loaded {images.Length} images from Resources/{imagesResourcesPath}");
        }

        foreach (Texture2D texture in images)
        {
            if (texture == null)
            {
                continue;
            }

            media.Add(new MediaRef
            {
                type = MediaType.Image,
                image = texture,
                video = null,
                displayName = texture.name
            });
        }

        VideoClip[] videos = Resources.LoadAll<VideoClip>(videosResourcesPath);
        if (enableDebugLogs)
        {
            Debug.Log($"[MediaLibrary] Loaded {videos.Length} videos from Resources/{videosResourcesPath}");
        }

        foreach (VideoClip clip in videos)
        {
            if (clip == null)
            {
                continue;
            }

            media.Add(new MediaRef
            {
                type = MediaType.Video,
                image = null,
                video = clip,
                displayName = clip.name
            });
        }

        return media;
    }
}
