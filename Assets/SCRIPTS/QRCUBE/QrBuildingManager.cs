using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class QrBuildingManager : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Prefab")]
    [SerializeField] private GameObject buildingCubePrefab;

    [Header("Behavior")]
    [Tooltip("Décalage du cube au-dessus du QR (m).")]
    [SerializeField] private float heightOffset = 0.03f;

    [Tooltip("Si true: on cache le cube quand le QR n'est plus tracké.")]
    [SerializeField] private bool hideWhenNotTracking = true;

    // key = nom de l'image dans la reference library
    private readonly Dictionary<string, GameObject> spawned = new();

    private void Reset()
    {
        // Auto-assign si le script est mis sur AR Session Origin
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var img in args.added)
            Upsert(img);

        foreach (var img in args.updated)
            Upsert(img);

        foreach (var img in args.removed)
            HandleRemoved(img);
    }

    private void Upsert(ARTrackedImage img)
    {
        string id = img.referenceImage.name; // correspond au nom dans ta library

        bool tracking = img.trackingState == TrackingState.Tracking;

        if (!spawned.TryGetValue(id, out GameObject go))
        {
            go = Instantiate(buildingCubePrefab);
            go.name = $"Building_{id}";
            spawned[id] = go;

            // AJOUT: associer QR -> cube -> BDD
            var roomCube = go.GetComponent<BuildingCube>();
            if (roomCube != null)
                roomCube.roomId = id;
        }

        if (hideWhenNotTracking)
            go.SetActive(tracking);

        if (!tracking) return;

        // Place le cube exactement sur le QR
        go.transform.SetPositionAndRotation(img.transform.position, img.transform.rotation);

        // Le fait "sortir" au-dessus du QR
        go.transform.position += img.transform.up * heightOffset;
    }

    private void HandleRemoved(ARTrackedImage img)
    {
        string id = img.referenceImage.name;

        if (spawned.TryGetValue(id, out GameObject go))
        {
            if (hideWhenNotTracking) go.SetActive(false);
        }
    }
}