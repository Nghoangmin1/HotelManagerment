using System;

namespace HotelManagement.Shared
{
    public static class BookingLogger
    {
        public static void LogBookingCreated(int bookingId, string roomNumber, string customerName, string user)
        {
            Logger.LogInfo($"[BOOKING] Booking ID #{bookingId} for Room P.{roomNumber} (Guest: {customerName}) was created by User: {user}");
        }

        public static void LogBookingUpdated(int bookingId, string user, string details)
        {
            Logger.LogInfo($"[BOOKING] Booking ID #{bookingId} was updated by User: {user}. Details: {details}");
        }

        public static void LogBookingDeleted(int bookingId, string user)
        {
            Logger.LogInfo($"[BOOKING] Booking ID #{bookingId} was deleted by User: {user}");
        }

        public static void LogBookingStatusChanged(int bookingId, string oldStatus, string newStatus, string user)
        {
            Logger.LogInfo($"[BOOKING] Booking ID #{bookingId} status changed from '{oldStatus}' to '{newStatus}' by User: {user}");
        }

        public static void LogServiceAdded(int bookingId, string serviceName, decimal price, int quantity, string user)
        {
            Logger.LogInfo($"[BOOKING] Service '{serviceName}' (x{quantity}, Price: {price}) was added to Booking ID #{bookingId} by User: {user}");
        }

        public static void LogServiceRemoved(int bookingId, string serviceName, string user)
        {
            Logger.LogInfo($"[BOOKING] Service '{serviceName}' was removed from Booking ID #{bookingId} by User: {user}");
        }
    }
}
