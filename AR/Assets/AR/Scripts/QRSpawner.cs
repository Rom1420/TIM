using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class QRSpawner : MonoBehaviour
{
    [Header("AR")]
    public ARTrackedImageManager imageManager;

    [Header("Prefab")]
    public GameObject cubePrefab;

    // Pour gérer plusieurs QR en même temps
    private Dictionary<string, GameObject> spawnedCubes = new Dictionary<string, GameObject>();

    void OnEnable()
    {
        imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // QR détectés
        foreach (ARTrackedImage image in args.added)
        {
            SpawnCube(image);
        }

        // QR mis à jour (position / tracking)
        foreach (ARTrackedImage image in args.updated)
        {
            UpdateCube(image);
        }

        // QR perdus
        foreach (ARTrackedImage image in args.removed)
        {
            RemoveCube(image);
        }
    }

    void SpawnCube(ARTrackedImage image)
    {
        string id = image.referenceImage.name;

        if (spawnedCubes.ContainsKey(id))
            return;

        GameObject cube = Instantiate(
            cubePrefab,
            image.transform.position,
            image.transform.rotation
        );

        cube.transform.SetParent(image.transform);

        // Stocker l'ID du QR dans le cube
        QRData data = cube.GetComponent<QRData>();
        data.qrID = id;

        spawnedCubes.Add(id, cube);
    }

    void UpdateCube(ARTrackedImage image)
    {
        string id = image.referenceImage.name;

        if (!spawnedCubes.ContainsKey(id))
            return;

        GameObject cube = spawnedCubes[id];

        if (image.trackingState == TrackingState.Tracking)
        {
            cube.SetActive(true);
        }
        else
        {
            cube.SetActive(false);
        }
    }

    void RemoveCube(ARTrackedImage image)
    {
        string id = image.referenceImage.name;

        if (!spawnedCubes.ContainsKey(id))
            return;

        Destroy(spawnedCubes[id]);
        spawnedCubes.Remove(id);
    }
}
