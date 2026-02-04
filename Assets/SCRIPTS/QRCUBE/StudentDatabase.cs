using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class StudentDatabase : MonoBehaviour
{
    [Header("CSV (Resources)")]
    [Tooltip("Chemin dans Resources sans extension. Ex: students pour Resources/students.csv")]
    [SerializeField] private string csvResourcePath = "students";

    private List<StudentData> allStudents = new();

    private void Awake()
    {
        LoadFromResources();
    }

    private void LoadFromResources()
    {
        TextAsset csv = Resources.Load<TextAsset>(csvResourcePath);
        if (csv == null)
        {
            Debug.LogError($"[StudentDatabase] CSV introuvable: Resources/{csvResourcePath}.csv");
            return;
        }

        using StringReader reader = new StringReader(csv.text);

        string header = reader.ReadLine();
        if (string.IsNullOrEmpty(header))
        {
            Debug.LogError("[StudentDatabase] CSV vide ou sans header.");
            return;
        }

        char sep = DetectSeparator(header);
        Debug.Log($"[StudentDatabase] Separator detected: '{sep}'");

        allStudents.Clear();
        string line;
        int lineNum = 1;

        while ((line = reader.ReadLine()) != null)
        {
            lineNum++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(sep);
            if (parts.Length < 13)
            {
                Debug.LogWarning($"[StudentDatabase] Ligne {lineNum} ignorée (colonnes insuffisantes): {line}");
                continue;
            }

            try
            {
                StudentData student = new StudentData
                {
                    name = parts[0].Trim(),
                    gender = parts[1].Trim(),
                    room_13h = parts[2].Trim(),
                    room_14h = parts[3].Trim(),
                    room_15h = parts[4].Trim(),
                    room_16h = parts[5].Trim(),
                    room_17h = parts[6].Trim(),
                    hair = parts[7].Trim(),
                    height = ParseInt(parts[8].Trim()),
                    transport = parts[9].Trim(),
                    clothing = parts[10].Trim(),
                    year = ParseInt(parts[11].Trim()),
                    specialization = parts[12].Trim()
                };

                allStudents.Add(student);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[StudentDatabase] Erreur ligne {lineNum}: {ex.Message}");
            }
        }

        Debug.Log($"[StudentDatabase] {allStudents.Count} étudiants chargés.");
    }

    private char DetectSeparator(string headerLine)
    {
        int commas = headerLine.Count(c => c == ',');
        int semicolons = headerLine.Count(c => c == ';');
        return semicolons > commas ? ';' : ',';
    }

    private int ParseInt(string value)
    {
        return int.TryParse(value, out int result) ? result : 0;
    }

    // ============================
    // GETTERS
    // ============================

    public List<StudentData> GetAllStudents()
    {
        return new List<StudentData>(allStudents);
    }

    public StudentData GetStudentByName(string name)
    {
        return allStudents.FirstOrDefault(s => s.name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    // ============================
    // FILTRES
    // ============================

    public List<StudentData> FilterStudents(
        int? year = null,
        string specialization = null,
        string roomProximity = null,
        int? hour = null,
        string transport = null,
        string objectFilter = null
    )
    {
        IEnumerable<StudentData> filtered = allStudents;

        // Filtre par année
        if (year.HasValue && year.Value > 0)
        {
            filtered = filtered.Where(s => s.year == year.Value);
        }

        // Filtre par spécialisation
        if (!string.IsNullOrEmpty(specialization) && specialization != "All")
        {
            filtered = filtered.Where(s => s.specialization.Equals(specialization, StringComparison.OrdinalIgnoreCase));
        }

        // Filtre par proximité de salle
        if (!string.IsNullOrEmpty(roomProximity) && roomProximity != "All")
        {
            if (hour.HasValue)
            {
                // À une heure spécifique
                filtered = filtered.Where(s => s.IsInRoomAtHour(roomProximity, hour.Value));
            }
            else
            {
                // N'importe quand dans la journée
                filtered = filtered.Where(s => s.IsInRoomAnyTime(roomProximity));
            }
        }

        // Filtre par mode de transport
        if (!string.IsNullOrEmpty(transport) && transport != "All")
        {
            filtered = filtered.Where(s => s.transport.Equals(transport, StringComparison.OrdinalIgnoreCase));
        }

        // Filtre par objet (cheveux, vêtements, etc.)
        if (!string.IsNullOrEmpty(objectFilter))
        {
            filtered = filtered.Where(s =>
                s.hair.IndexOf(objectFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                s.clothing.IndexOf(objectFilter, StringComparison.OrdinalIgnoreCase) >= 0
            );
        }

        return filtered.ToList();
    }

    // ============================
    // LISTES POUR UI DROPDOWNS
    // ============================

    public List<int> GetAllYears()
    {
        return allStudents.Select(s => s.year).Distinct().OrderBy(y => y).ToList();
    }

    public List<string> GetAllSpecializations()
    {
        return allStudents.Select(s => s.specialization).Distinct().OrderBy(s => s).ToList();
    }

    public List<string> GetAllTransportModes()
    {
        return allStudents.Select(s => s.transport).Distinct().OrderBy(t => t).ToList();
    }

    public List<string> GetAllRooms()
    {
        HashSet<string> rooms = new HashSet<string>();
        foreach (var student in allStudents)
        {
            rooms.Add(student.room_13h);
            rooms.Add(student.room_14h);
            rooms.Add(student.room_15h);
            rooms.Add(student.room_16h);
            rooms.Add(student.room_17h);
        }
        return rooms.OrderBy(r => r).ToList();
    }
}
