using YamlDotNet.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace texmipper
{
    public class ResourcePct
    {
        public ResourcePctHeader header { get; set; }
        public string linkTd { get; set; }
        public string[] mipMaps { get; set; }
        public string texName { get; set; }
        public string texType { get; set; }
        public string pct { get; set; }
        public SM2TextureFormat TextureFormat => (SM2TextureFormat) header.format;

        public class ResourcePctHeader
        {
            public long faceSize { get; set; }
            public int format { get; set; }
            public ResourceicPctMipLevel[] mipLevel { get; set; }
            public int nFaces { get; set; }
            public int nMipMap { get; set; }
            public long sign { get; set; }
            public long size { get; set; }
            public int sx { get; set; }
            public int sy { get; set; }
            public int sz { get; set; }
        }

        public class ResourceicPctMipLevel
        {
            public long offset { get; set; }
            public long size { get; set; }
        }

    }

}
