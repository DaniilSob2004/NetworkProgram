namespace NetworkProgram
{
    public class ClientRequest
    {
        public string Command { get; set; } = null!;  // что за запрос (Message, Check)
        public ChatMessage ChatMessage { get; set; } = null!;
    }
}
