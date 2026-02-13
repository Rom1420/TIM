using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Meta.WitAi;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif


public class QuestVoiceLightController : MonoBehaviour
{
    [Header("Voice")]
    [SerializeField] private VoiceService voiceService;
    [SerializeField] private bool autoListen = true;
    [SerializeField] private bool activateImmediately = true;
    [SerializeField] private float listenRetryDelay = 0.25f;

    [Header("Lights")]
    [SerializeField] private bool autoDiscoverLights = true;
    [SerializeField] private List<Light> interiorLightsOverride = new List<Light>();
    [SerializeField] private Light directionalLight;
    [SerializeField] private float intensityStep = 0.2f;
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float directionalIntensityMultiplier = 0.3f;
    [SerializeField] private float defaultDirectionalIntensity = 0f;

    private readonly List<Light> interiorLights = new List<Light>();
    private float nextListenTime;

    private void Awake()
    {
        if (!voiceService)
        {
            voiceService = FindAnyObjectByType<VoiceService>();
        }

        if (autoDiscoverLights)
        {
            RefreshLights();
        }
    }

    private void Start()
    {
        if (autoDiscoverLights)
        {
            RefreshLights();
        }

        InitializeDefaultDirectionalIntensity();
    }

    private void OnEnable()
    {
        if (voiceService)
        {
            voiceService.VoiceEvents.OnFullTranscription.AddListener(HandleTranscription);
        }

        RequestMicrophonePermission();
    }

    private void OnDisable()
    {
        if (voiceService)
        {
            voiceService.VoiceEvents.OnFullTranscription.RemoveListener(HandleTranscription);
        }
    }

    private void Update()
    {
        if (!autoListen || !voiceService || voiceService.Active)
        {
            return;
        }

        if (Time.unscaledTime >= nextListenTime)
        {
            StartListening();
        }
    }

    private void StartListening()
    {
        if (!voiceService)
        {
            return;
        }

        if (activateImmediately)
        {
            voiceService.ActivateImmediately();
        }
        else
        {
            voiceService.Activate();
        }

        nextListenTime = Time.unscaledTime + listenRetryDelay;
    }

    private void HandleTranscription(string transcription)
    {
        if (string.IsNullOrWhiteSpace(transcription))
        {
            return;
        }

        Debug.Log($"Voice transcription: {transcription}", this);

        string normalized = Normalize(transcription);

        if (TryHandleAmbienceCommand(normalized))
        {
            return;
        }

        if (TryHandleColorCommand(normalized))
        {
            return;
        }

        if (TryHandleIntensityCommand(normalized))
        {
            return;
        }

        Debug.Log("Voice action: no match", this);
    }

    [ContextMenu("Refresh Lights")]
    public void RefreshLights()
    {
        interiorLights.Clear();

        if (interiorLightsOverride != null && interiorLightsOverride.Count > 0)
        {
            AddValidLights(interiorLightsOverride, interiorLights);
        }
        else
        {
            DiscoverInteriorLightsFromScene();
        }

        EnsureDirectionalLightFound();

        Debug.Log(
            $"Lights refreshed: interior={interiorLights.Count}, directional={(directionalLight ? "assigned" : "none")}",
            this);
    }

    private void DiscoverInteriorLightsFromScene()
    {
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);

        for (int i = 0; i < allLights.Length; i++)
        {
            Light light = allLights[i];
            if (!light)
            {
                continue;
            }

            if (IsInteriorLightType(light.type))
            {
                interiorLights.Add(light);
            }
        }
    }

    private void EnsureDirectionalLightFound()
    {
        if (directionalLight && directionalLight.type != LightType.Directional)
        {
            Debug.LogWarning("Directional light override is not Directional. Auto-discovering.", this);
            directionalLight = null;
        }

        if (directionalLight)
        {
            return;
        }

        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < allLights.Length; i++)
        {
            if (allLights[i] && allLights[i].type == LightType.Directional)
            {
                directionalLight = allLights[i];
                break;
            }
        }
    }

    private void InitializeDefaultDirectionalIntensity()
    {
        if (!Mathf.Approximately(defaultDirectionalIntensity, 0f))
        {
            return;
        }

        if (directionalLight)
        {
            defaultDirectionalIntensity = directionalLight.intensity;
        }
        else
        {
            defaultDirectionalIntensity = 0.3f;
        }
    }

    private bool TryHandleColorCommand(string normalized)
    {
        if (!TryGetColor(normalized, out Color color, out string colorName))
        {
            return false;
        }

        int interiorCount = ApplyColorToInteriorLights(color);
        Debug.Log($"Voice action: set color {colorName} (interior={interiorCount})", this);
        return true;
    }

    private bool TryHandleAmbienceCommand(string normalized)
    {
        if (!ContainsAny(normalized, "ambiance", "anbiance"))
        {
            return false;
        }

        if (ContainsToken(normalized, "chill"))
        {
            int interiorCount = ApplyAmbiencePreset(
                new Color32(30, 60, 120, 255),
                0.5f,
                new Color32(80, 170, 255, 255),
                0.3f,
                new Color32(180, 120, 255, 255));
            Debug.Log($"Voice action: ambience chill (interior={interiorCount})", this);
            return true;
        }

        if (ContainsAny(normalized, "ocean", "mer"))
        {
            int interiorCount = ApplyAmbiencePreset(
                new Color32(60, 120, 200, 255),
                0.5f,
                new Color32(80, 170, 255, 255),
                0.25f,
                new Color32(180, 120, 255, 255));
            Debug.Log($"Voice action: ambience ocean (interior={interiorCount})", this);
            return true;
        }

        if (ContainsToken(normalized, "neon"))
        {
            int interiorCount = ApplyAmbiencePreset(
                new Color32(180, 120, 255, 255),
                0.4f,
                new Color32(255, 130, 200, 255),
                0.3f,
                new Color32(80, 170, 255, 255));
            Debug.Log($"Voice action: ambience neon (interior={interiorCount})", this);
            return true;
        }

        if (ContainsToken(normalized, "sunset"))
        {
            int interiorCount = ApplyAmbiencePreset(
                new Color32(255, 160, 90, 255),
                0.4f,
                new Color32(255, 220, 120, 255),
                0.3f,
                new Color32(255, 130, 200, 255));
            Debug.Log($"Voice action: ambience sunset (interior={interiorCount})", this);
            return true;
        }

        if (ContainsAny(normalized, "cosy", "cozy"))
        {
            int interiorCount = ApplyAmbiencePreset(
                new Color32(255, 244, 220, 255),
                0.6f,
                new Color32(255, 220, 120, 255),
                0.2f,
                new Color32(255, 160, 90, 255));
            Debug.Log($"Voice action: ambience cosy (interior={interiorCount})", this);
            return true;
        }

        if (ContainsAny(normalized, "nuit", "night"))
        {
            int interiorCount = ApplyAmbiencePreset(
                new Color32(30, 60, 120, 255),
                0.6f,
                new Color32(180, 120, 255, 255),
                0.25f,
                new Color32(255, 130, 200, 255));
            Debug.Log($"Voice action: ambience nuit (interior={interiorCount})", this);
            return true;
        }

        return false;
    }

    private bool TryHandleIntensityCommand(string normalized)
    {
        bool hasEteins = ContainsToken(normalized, "eteins");
        bool hasAllume = ContainsToken(normalized, "allume");
        bool hasLumiere = ContainsToken(normalized, "lumiere");
        int interiorCount;

        if (hasEteins && hasLumiere)
        {
            interiorCount = SetInteriorIntensity(minIntensity);
            SetDirectionalIntensity(0f);
            Debug.Log($"Voice action: eteins lumiere (interior={interiorCount})", this);
            return true;
        }

        if (hasAllume && hasLumiere)
        {
            interiorCount = SetInteriorIntensity(maxIntensity);
            SetDirectionalIntensity(defaultDirectionalIntensity);
            Debug.Log($"Voice action: allume lumiere (interior={interiorCount})", this);
            return true;
        }

        if (hasEteins)
        {
            interiorCount = SetInteriorIntensity(minIntensity);
            SetDirectionalIntensity(0f);
            Debug.Log($"Voice action: eteins (interior={interiorCount})", this);
            return true;
        }

        if (hasAllume)
        {
            interiorCount = SetInteriorIntensity(maxIntensity);
            SetDirectionalIntensity(defaultDirectionalIntensity);
            Debug.Log($"Voice action: allume (interior={interiorCount})", this);
            return true;
        }

        bool wantsIntensity = ContainsAny(normalized, "baisse", "diminue", "augmente", "plus sombre", "plus clair", "intensite", "lumineux");
        if (!wantsIntensity)
        {
            return false;
        }

        float delta = 0f;
        if (ContainsAny(normalized, "baisse", "diminue", "plus sombre"))
        {
            delta = -intensityStep;
        }
        else if (ContainsAny(normalized, "augmente", "plus clair"))
        {
            delta = intensityStep;
        }
        else
        {
            delta = intensityStep;
        }

        interiorCount = AdjustInteriorIntensity(delta);
        AdjustDirectionalIntensity(delta);
        Debug.Log($"Voice action: adjust intensity {delta} (interior={interiorCount})", this);
        return true;
    }

    private static bool ContainsAny(string text, params string[] tokens)
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            if (ContainsToken(text, tokens[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsToken(string text, string token)
    {
        return text.IndexOf(token, StringComparison.Ordinal) >= 0;
    }

    private static bool TryGetColor(string normalized, out Color color, out string colorName)
    {
        if (ContainsToken(normalized, "bleu"))
        {
            color = new Color32(80, 170, 255, 255);
            colorName = "bleu";
            return true;
        }

        if (ContainsToken(normalized, "rouge"))
        {
            color = new Color32(255, 90, 90, 255);
            colorName = "rouge";
            return true;
        }

        if (ContainsToken(normalized, "vert"))
        {
            color = new Color32(120, 255, 160, 255);
            colorName = "vert";
            return true;
        }

        if (ContainsToken(normalized, "violet"))
        {
            color = new Color32(180, 120, 255, 255);
            colorName = "violet";
            return true;
        }

        if (ContainsToken(normalized, "jaune"))
        {
            color = new Color32(255, 220, 120, 255);
            colorName = "jaune";
            return true;
        }

        if (ContainsToken(normalized, "blanc"))
        {
            color = new Color32(255, 244, 220, 255);
            colorName = "blanc";
            return true;
        }

        if (ContainsToken(normalized, "orange"))
        {
            color = new Color32(255, 160, 90, 255);
            colorName = "orange";
            return true;
        }

        if (ContainsToken(normalized, "rose"))
        {
            color = new Color32(255, 130, 200, 255);
            colorName = "rose";
            return true;
        }

        color = new Color32(255, 244, 220, 255);
        colorName = string.Empty;
        return false;
    }

    private int ApplyColorToInteriorLights(Color color)
    {
        int affected = 0;
        for (int i = 0; i < interiorLights.Count; i++)
        {
            if (interiorLights[i])
            {
                interiorLights[i].color = color;
                affected++;
            }
        }

        return affected;
    }

    private int ApplyAmbiencePreset(Color colorA, float ratioA, Color colorB, float ratioB, Color colorC)
    {
        List<Light> validLights = new List<Light>(interiorLights.Count);
        for (int i = 0; i < interiorLights.Count; i++)
        {
            if (interiorLights[i])
            {
                validLights.Add(interiorLights[i]);
            }
        }

        int total = validLights.Count;
        if (total == 0)
        {
            return 0;
        }

        int countA = Mathf.Clamp(Mathf.RoundToInt(total * ratioA), 0, total);
        int countB = Mathf.Clamp(Mathf.RoundToInt(total * ratioB), 0, total - countA);
        int countC = total - countA - countB;

        for (int i = 0; i < countA; i++)
        {
            validLights[i].color = colorA;
        }

        for (int i = countA; i < countA + countB; i++)
        {
            validLights[i].color = colorB;
        }

        for (int i = countA + countB; i < countA + countB + countC; i++)
        {
            validLights[i].color = colorC;
        }

        return total;
    }

    private int AdjustInteriorIntensity(float delta)
    {
        int affected = 0;
        for (int i = 0; i < interiorLights.Count; i++)
        {
            if (!interiorLights[i])
            {
                continue;
            }

            float target = Mathf.Clamp(interiorLights[i].intensity + delta, minIntensity, maxIntensity);
            interiorLights[i].intensity = target;
            affected++;
        }

        return affected;
    }

    private int SetInteriorIntensity(float intensityValue)
    {
        int affected = 0;
        float clampedIntensity = Mathf.Clamp(intensityValue, minIntensity, maxIntensity);
        for (int i = 0; i < interiorLights.Count; i++)
        {
            if (interiorLights[i])
            {
                interiorLights[i].intensity = clampedIntensity;
                affected++;
            }
        }

        return affected;
    }

    private void AdjustDirectionalIntensity(float delta)
    {
        if (!directionalLight)
        {
            return;
        }

        float target = Mathf.Max(0f, directionalLight.intensity + delta * directionalIntensityMultiplier);
        directionalLight.intensity = target;
    }

    private void SetDirectionalIntensity(float intensity)
    {
        if (!directionalLight)
        {
            return;
        }

        directionalLight.intensity = Mathf.Max(0f, intensity);
    }

    private static string Normalize(string input)
    {
        string lower = input.ToLowerInvariant().Trim();
        string formD = lower.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);

        foreach (char c in formD)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) ? c : ' ');
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private void RequestMicrophonePermission()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
    }

    private static void AddValidLights(List<Light> source, List<Light> destination)
    {
        if (source == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            Light light = source[i];
            if (light && IsInteriorLightType(light.type) && !destination.Contains(light))
            {
                destination.Add(light);
            }
        }
    }

    private static bool IsInteriorLightType(LightType type)
    {
        return type == LightType.Point || type == LightType.Spot || type == LightType.Rectangle;
    }
}
