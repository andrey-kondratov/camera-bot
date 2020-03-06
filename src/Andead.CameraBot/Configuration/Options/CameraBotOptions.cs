namespace Andead.CameraBot
{
    public class CameraBotOptions
    {
        public NodeOptions Root { get; set; } = new NodeOptions();
        public int RetryCount { get; set; } = 3;
        public int TimeoutMilliseconds { get; set; } = 1000;
    }
}
