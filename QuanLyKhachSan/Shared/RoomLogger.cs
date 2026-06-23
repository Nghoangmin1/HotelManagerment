using System;

namespace HotelManagement.Shared
{
    public static class RoomLogger
    {
        public static void LogRoomCreated(string roomNumber, string user)
        {
            Logger.LogInfo($"[ROOM] Room P.{roomNumber} was created by User: {user}");
        }

        public static void LogRoomUpdated(string roomNumber, string user, string details)
        {
            Logger.LogInfo($"[ROOM] Room P.{roomNumber} was updated by User: {user}. Details: {details}");
        }

        public static void LogRoomDeleted(string roomNumber, string user)
        {
            Logger.LogInfo($"[ROOM] Room P.{roomNumber} was deleted by User: {user}");
        }
    }
}
