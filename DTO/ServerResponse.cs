
namespace DTO
{
    public class ServerResponse
    {
        public string Status{ get; set; }
        public string Message { get; set; }
        public string? Identify { get; set; }
        public List<Mail>? Mails { get; set; }

        public ServerResponse()
        {
            Status = string.Empty;
            Message = string.Empty;
            Identify = string.Empty;
            Mails = new List<Mail>();
        }

        public ServerResponse(string status, string message,  List<Mail> mails)
        {
            Status = status;
            Message = message;
            Mails = mails;
        }

        public ServerResponse(string status, string message, string identify, List<Mail> mails)
        {
            Status = status;
            Message = message;
            Identify = identify;
            Mails = mails;
        }

        public ServerResponse(string status, string message, string identify)
        {
            Status = status;
            Message = message;
            Identify = identify;

        }

        public ServerResponse(string status, string message)
        {
            Status = status;
            Message = message;

        }

        // Covert to Json
        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        // Convert to ServerResponse object from Json
        public static ServerResponse FromJson(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ServerResponse>(json);
        }

    }
}
