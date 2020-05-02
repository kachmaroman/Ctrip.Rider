namespace Ctrip.Rider.DataModels
{
    public class UserData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int Logintype { get; set; }
        public bool IsLinked { get; set; }
        public string ProfileId { get; set; }
    }
}