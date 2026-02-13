using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

[Serializable]
public enum MediaType
{
    Image,
    Video
}

[Serializable]
public class MediaRef
{
    public MediaType type;
    public Texture2D image;
    public VideoClip video;
    public string displayName;
}

[Serializable]
public class MediaRefUnityEvent : UnityEvent<MediaRef> { }
