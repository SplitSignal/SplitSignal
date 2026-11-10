namespace GameServer
{
    public class UpdateRequest
    {
        public string SessionCode { get; set; }
        public string Object {  get; set; }
        public string Status { get; set; }
        public string Notify { get; set; }
    }


    public class CheckRequest
    {
        public string SessionCode {  set; get; }
        public string Object { get; set; } 
    }

    public class UpdateClient
    {
        public string Object { get; set; }

        public string Status { get; set; }
    }
}
