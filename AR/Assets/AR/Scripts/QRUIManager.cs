using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class QRUIManager : MonoBehaviour
{
    [Header("AR")]
    public ARTrackedImageManager imageManager;

    [Header("UI")]
    public GameObject qrPanel;
    public TextMeshProUGUI qrText;

    // On garde les QR actuellement suivis (tracking)
    private readonly HashSet<string> trackingQrs = new HashSet<string>();

    void OnEnable()
    {
        imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void Start()
    {
        qrPanel.SetActive(false);
        qrText.text = "";
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // added + updated : on met à jour l'état tracking
        foreach (var img in args.added)
            UpdateTrackingSet(img);

        foreach (var img in args.updated)
            UpdateTrackingSet(img);

        // removed : on retire
        foreach (var img in args.removed)
            trackingQrs.Remove(img.referenceImage.name);

        RefreshUI();
    }

    void UpdateTrackingSet(ARTrackedImage img)
    {
        string name = img.referenceImage.name;

        if (img.trackingState == TrackingState.Tracking)
            trackingQrs.Add(name);
        else
            trackingQrs.Remove(name);
    }

    void RefreshUI()
    {
        if (trackingQrs.Count == 0)
        {
            qrPanel.SetActive(false);
            qrText.text = "";
            return;
        }

        qrPanel.SetActive(true);

        // Afficher une liste propre
        var sb = new StringBuilder();
        sb.AppendLine($"QR détectés : {trackingQrs.Count}");
        foreach (var n in trackingQrs)
            sb.AppendLine("- " + n);

        qrText.text = sb.ToString();
    }
}