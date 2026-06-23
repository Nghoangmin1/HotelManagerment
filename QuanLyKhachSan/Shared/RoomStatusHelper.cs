namespace HotelManagement.Shared
{
    public static class RoomStatusHelper
    {
        public static string GetStatusLabel(string statusCode)
        {
            return statusCode.ToLower() switch
            {
                "available" => "Còn trống",
                "occupied" => "Đang ở",
                "dirty" => "Cần dọn dẹp",
                "reserved" => "Đã đặt trước",
                _ => "Không xác định"
            };
        }

        public static string GetStatusClass(string statusCode)
        {
            return statusCode.ToLower() switch
            {
                "available" => "available",
                "occupied" => "occupied",
                "dirty" => "dirty",
                "reserved" => "reserved",
                _ => ""
            };
        }
    }
}
