using System;
using UnityEngine;

[Serializable]
public class StudentData
{
    public string name;
    public string gender;
    public string room_13h;
    public string room_14h;
    public string room_15h;
    public string room_16h;
    public string room_17h;
    public string hair;
    public int height;
    public string transport;
    public string clothing;
    public int year;
    public string specialization;

    /// <summary>
    /// Retourne la salle où se trouve l'étudiant à l'heure donnée (13-17).
    /// </summary>
    public string GetRoomAtHour(int hour)
    {
        return hour switch
        {
            13 => room_13h,
            14 => room_14h,
            15 => room_15h,
            16 => room_16h,
            17 => room_17h,
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Vérifie si l'étudiant est dans une des salles spécifiées à une heure donnée.
    /// </summary>
    public bool IsInRoomAtHour(string roomId, int hour)
    {
        string currentRoom = GetRoomAtHour(hour);
        return currentRoom.Equals(roomId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Vérifie si l'étudiant est dans une des salles à n'importe quelle heure.
    /// </summary>
    public bool IsInRoomAnyTime(string roomId)
    {
        return room_13h.Equals(roomId, StringComparison.OrdinalIgnoreCase) ||
               room_14h.Equals(roomId, StringComparison.OrdinalIgnoreCase) ||
               room_15h.Equals(roomId, StringComparison.OrdinalIgnoreCase) ||
               room_16h.Equals(roomId, StringComparison.OrdinalIgnoreCase) ||
               room_17h.Equals(roomId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Retourne toutes les salles visitées par l'étudiant dans la journée.
    /// </summary>
    public string[] GetAllRooms()
    {
        return new[] { room_13h, room_14h, room_15h, room_16h, room_17h };
    }
}
