using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class BuildingData
{
    public string room_id;
    public int number_of_seats_available;
    public string evacuation_map;
    public string objects_present;
}

public class BuildingDatabase : MonoBehaviour
{
    [Header("CSV (Resources)")]
    [Tooltip("Chemin dans Resources sans extension. Ex: room_db pour Resources/room_db.csv")]
    [SerializeField] private string csvResourcePath = "room_db";

    private readonly Dictionary<string, BuildingData> byId = new();

    private void Awake()
    {
        LoadFromResources();
    }

    private void LoadFromResources()
    {
        TextAsset csv = Resources.Load<TextAsset>(csvResourcePath);
        if (csv == null)
        {
            Debug.LogError($"[BuildingDatabase] CSV introuvable: Resources/{csvResourcePath}.csv");
            return;
        }

        using StringReader reader = new StringReader(csv.text);

        string header = reader.ReadLine();
        if (string.IsNullOrEmpty(header))
        {
            Debug.LogError("[BuildingDatabase] CSV vide ou sans header.");
            return;
        }

        // ✅ Détecte automatiquement le séparateur (Excel FR souvent ';')
        char sep = DetectSeparator(header);
        Debug.Log($"[BuildingDatabase] Separator detected: '{sep}'");

        byId.Clear();

        string line;
        int lineIndex = 1;
        while ((line = reader.ReadLine()) != null)
        {
            lineIndex++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Split simple (ton CSV est simple: pas de virgules dans les champs)
            string[] parts = line.Split(sep);
            if (parts.Length < 4)
            {
                Debug.LogWarning($"[BuildingDatabase] Ligne {lineIndex} invalide (colonnes < 4): {line}");
                continue;
            }

            string rid = Normalize(parts[0]);
            if (string.IsNullOrEmpty(rid)) continue;

            BuildingData data = new BuildingData
            {
                room_id = rid,
                number_of_seats_available = SafeInt(parts[1]),
                evacuation_map = parts[2].Trim(),
                objects_present = parts[3].Trim()
            };

            byId[rid] = data;
        }

        Debug.Log($"[BuildingDatabase] Loaded rooms: {byId.Count}");
        // Debug utile: montre 3 clés au hasard
        int shown = 0;
        foreach (var k in byId.Keys)
        {
            Debug.Log($"[BuildingDatabase] Key sample: '{k}'");
            if (++shown >= 3) break;
        }
    }

    private static char DetectSeparator(string header)
    {
        // Si l'un est clairement dominant, on le prend
        int commas = CountChar(header, ',');
        int semis = CountChar(header, ';');

        if (semis > commas) return ';';
        return ',';
    }

    private static int CountChar(string s, char c)
    {
        int n = 0;
        for (int i = 0; i < s.Length; i++)
            if (s[i] == c) n++;
        return n;
    }

    private static string Normalize(string s)
    {
        if (s == null) return "";
        s = s.Replace("\uFEFF", "");
        return s.Trim().ToLowerInvariant();
    }


    private static int SafeInt(string s)
    {
        if (int.TryParse(s.Trim(), out int v)) return v;
        return 0;
    }

    public bool TryGet(string roomId, out BuildingData data)
    {
        roomId = Normalize(roomId);
        bool ok = byId.TryGetValue(roomId, out data);
        Debug.Log($"[BuildingDatabase] TryGet '{roomId}' -> {ok}");
        return ok;
    }
}