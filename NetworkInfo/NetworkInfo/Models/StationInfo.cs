namespace NetworkInfo.Models
{
    public struct StationInfo
    {
        public int Id { get; set; }
        public int Lac { get; set; }
        public int Dbm { get; set; }

        public StationInfo(int id, int lac, int dbm)
        {
            Id = id;
            Lac = lac;
            Dbm = dbm;
        }
    }
}
