using System.Collections.Generic;

namespace Andead.CameraBot
{
    public class NodeOptions
    {
        public string Name { get; set; }
        public string SnapshotUrl { get; set; }
        public string Url { get; set; }
        public string Website { get; set; }
        public List<NodeOptions> Children { get; set; } = new List<NodeOptions>();
    }
}
