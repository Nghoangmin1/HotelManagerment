using System;

namespace HotelManagement.Shared
{
    public static class ReceptionistLogger
    {
        public static void LogCheckIn(string receptionist, string roomNumber, string customerName)
        {
            Logger.LogInfo($"[RECEPTIONIST] {receptionist} checked in Room P.{roomNumber} for customer '{customerName}'");
        }

        public static void LogCheckOut(string receptionist, string roomNumber, string customerName, decimal totalCharge)
        {
            Logger.LogInfo($"[RECEPTIONIST] {receptionist} checked out Room P.{roomNumber} for customer '{customerName}'. Total Price: {totalCharge:N0}đ");
        }

        public static void LogRoomStatusUpdated(string receptionist, string roomNumber, string oldStatus, string newStatus)
        {
            Logger.LogInfo($"[RECEPTIONIST] {receptionist} updated Room P.{roomNumber} status from '{oldStatus}' to '{newStatus}'");
        }

        public static void LogCustomerUpdated(string receptionist, string customerName, string details)
        {
            Logger.LogInfo($"[RECEPTIONIST] {receptionist} modified customer '{customerName}' info. Details: {details}");
        }
    }
}
