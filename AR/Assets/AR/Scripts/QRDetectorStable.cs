using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class QRMultiDetectUI : MonoBehaviour
{
    [Header("AR")]
    public ARTrackedImageManager imageManager;

    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI text;

    // QR en Tracking (noms uniques)
    private readonly HashSet<string> trackingQrs = new HashSet<string>();

    void Reset()
    {
        imageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        if (imageManager != null)
            imageManager.trackedImagesChanged += OnChanged;
    }

    void OnDisable()
    {
        if (imageManager != null)
            imageManager.trackedImagesChanged -= OnChanged;
    }

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (text != null) text.text = "";
    }

    void OnChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var img in args.added)
            UpdateOne(img);

        foreach (var img in args.updated)
            UpdateOne(img);

        foreach (var img in args.removed)
            trackingQrs.Remove(img.referenceImage.name);

        RefreshUI();
    }

    void UpdateOne(ARTrackedImage img)
    {
        string id = img.referenceImage.name;

        if (img.trackingState == TrackingState.Tracking)
            trackingQrs.Add(id);
        else
            trackingQrs.Remove(id);
    }

    void RefreshUI()
    {
        if (panel == null || text == null) return;

        if (trackingQrs.Count == 0)
        {
            panel.SetActive(false);
            text.text = "";
            return;
        }

        panel.SetActive(true);

        var sb = new StringBuilder();
        sb.AppendLine($"QR détectés : {trackingQrs.Count}");
        foreach (var id in trackingQrs)
            sb.AppendLine("- " + id);

        text.text = sb.ToString();
    }
}