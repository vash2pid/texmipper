
using YamlDotNet.Serialization;
using System.Diagnostics;
using DirectXTexNet;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.IO;
using System.IO.Pipes;

namespace texmipper
{
    internal class TexMipper
    {
        [STAThread]
        static void Main(string[] args)
        {
            string texturefilepath = "";
            
            var texturefiledialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Select texture file to convert...",
                Filter = "Texture File|*.tga"
            };
            using (texturefiledialog)
            {
                if (texturefiledialog.ShowDialog() == DialogResult.OK)
                {
                    texturefilepath = texturefiledialog.FileName;
                }
            }

            Console.WriteLine("Texture file selected:\n" + texturefilepath);

            var resourcefilepath = texturefilepath.Replace(".tga", ".pct.resource");

            if (!File.Exists(resourcefilepath))
            {
                Console.WriteLine("Resource file for selected file texture file not found in the same location." +
                    "\nThe resource file must have the same name as the texture file.");
                Console.WriteLine("\nPress any key to close this window...");
                Console.ReadKey();
                return;

            }

            Console.WriteLine("\nResrouce file selected:\n" + resourcefilepath);

            Console.WriteLine("Converting from *.tga to *.pct_mip files based on the number of mipmaps in resource file" );

            ResourcePct resourcePCT = Deserializer(resourcefilepath);

            ConvertToPCT(resourcePCT, texturefilepath);

            Console.WriteLine("\nPress any key to close this window...");
            Console.ReadKey();
        }

        private static void ConvertToPCT(ResourcePct resourcePCT, String inputtexturefilepath)
        {
            int counter = 1;
            int cursizex = resourcePCT.header.sx;
            int cursizey = resourcePCT.header.sy;

            foreach (String mip in resourcePCT.mipMaps)
            {
                Console.WriteLine("\nConverting " + mip);
                    if (File.Exists(mip))
                        File.Move(mip, mip + "." + DateTime.Now.ToString("yyyyMMddHHmmssffff"));
                try
                {
                    if (counter > 1)
                    {
                        cursizex = cursizex / 2;
                        cursizey = cursizey / 2;
                    }

                    using var tgaimage = TexHelper.Instance.LoadFromTGAFile(inputtexturefilepath);
                    using var resizedimage = tgaimage.Resize(0, cursizex, cursizey, TEX_FILTER_FLAGS.LINEAR);
                    using var compressed = resizedimage.Compress(GetDXGIFormat(resourcePCT.TextureFormat), TEX_COMPRESS_FLAGS.PARALLEL, 0.5f);

                    using var ddsimage = TexHelper.Instance.Initialize2D(GetDXGIFormat(resourcePCT.TextureFormat), cursizex, cursizey, 1, 1, CP_FLAGS.NONE);

                    var srcData = compressed.GetPixels();
                    var destData = ddsimage.GetPixels();

                    if (ddsimage.GetPixelsSize() < compressed.GetPixelsSize()) throw new Exception("Source data will not fit");

                    using var ddsstream = compressed.SaveToDDSMemory(DDS_FLAGS.NONE);
                    using var outputstream = File.Create(mip);
                    using var memstream = new MemoryStream();
                    ddsstream.CopyTo(memstream);
                    long ddsoffset = GetDDSHeaderOffset(memstream);
                    ddsstream.Seek(ddsoffset, SeekOrigin.Begin);
                    ddsstream.CopyTo(outputstream);
                    outputstream.Flush();
                    outputstream.Close();
                    ddsstream.Close();

                    FileCheckResultMip(resourcePCT, mip, counter);
                    Console.WriteLine("\n" + Path.GetFileName(inputtexturefilepath) + " converted to " + mip);
                } 
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
                counter++;
            }
        }

        private static long GetDDSHeaderOffset(MemoryStream memstream)
        {
            memstream.Position = 0;
            using var binaryStream = new BinaryReader(memstream);
            char[] magic4 = binaryStream.ReadChars(4);
            uint headersize = binaryStream.ReadUInt32();
            uint flags = binaryStream.ReadUInt32();
            uint height = binaryStream.ReadUInt32();
            uint width = binaryStream.ReadUInt32();
            uint pitchOrLinearSize = binaryStream.ReadUInt32();
            uint depth = binaryStream.ReadUInt32();
            uint mip = binaryStream.ReadUInt32();
            byte[] reserved = binaryStream.ReadBytes(44);

            uint pixelformatheadersize = binaryStream.ReadUInt32();
            uint pixelformatflags = binaryStream.ReadUInt32();
            char[] fourCC = binaryStream.ReadChars(4);
            uint RGBBitCount = binaryStream.ReadUInt32();
            uint RBitMask = binaryStream.ReadUInt32();
            uint GBitMask = binaryStream.ReadUInt32();
            uint BBitMask = binaryStream.ReadUInt32();
            uint ABitMask = binaryStream.ReadUInt32();
            uint caps = binaryStream.ReadUInt32();
            uint caps2 = binaryStream.ReadUInt32();
            uint caps3 = binaryStream.ReadUInt32();
            uint caps4 = binaryStream.ReadUInt32();
            uint reserved2  = binaryStream.ReadUInt32();
            
            if (new string(fourCC).Equals("DX10"))
            {
                uint dxgiformat = binaryStream.ReadUInt32();
                uint dimension = binaryStream.ReadUInt32();
                uint miscFlash = binaryStream.ReadUInt32();
                uint arraysize = binaryStream.ReadUInt32();
                uint miscflag2 = binaryStream.ReadUInt32();
            }
            return binaryStream.BaseStream.Position;
        }

        private static DXGI_FORMAT GetDXGIFormat(SM2TextureFormat format)
        {
            switch (format)
            {
                case SM2TextureFormat.ARGB8888:
                    return DXGI_FORMAT.B8G8R8A8_UNORM;
                case SM2TextureFormat.ARGB16161616U:
                    return DXGI_FORMAT.R16G16B16A16_UNORM;
                case SM2TextureFormat.BC6U:
                    return DXGI_FORMAT.BC6H_UF16;
                case SM2TextureFormat.BC7:
                case SM2TextureFormat.BC7A:
                    return DXGI_FORMAT.BC7_UNORM;
                case SM2TextureFormat.DXN:
                    return DXGI_FORMAT.BC5_UNORM;
                case SM2TextureFormat.DXT5A:
                    return DXGI_FORMAT.BC4_UNORM;
                case SM2TextureFormat.AXT1:
                case SM2TextureFormat.OXT1:
                    return DXGI_FORMAT.BC1_UNORM;
                case SM2TextureFormat.R8U:
                    return DXGI_FORMAT.R8_UNORM;
                case SM2TextureFormat.R16:
                    return DXGI_FORMAT.R16_SNORM;
                case SM2TextureFormat.R16G16:
                    return DXGI_FORMAT.R16G16_SINT;
                case SM2TextureFormat.RGBA16161616F:
                    return DXGI_FORMAT.R16G16B16A16_FLOAT;
                case SM2TextureFormat.XT5:
                    return DXGI_FORMAT.BC3_UNORM;
                case SM2TextureFormat.XRGB8888:
                    return DXGI_FORMAT.B8G8R8X8_UNORM;
                default:
                    throw new Exception("Invalid SM2 Texture Type: " + format.ToString());
            }

        }

        private static void FileCheckResultMip(ResourcePct resourcePCT, string mipname, int counter)
        {
            if(File.Exists(mipname)) {
                byte[] file = File.ReadAllBytes(mipname);
                long expectedSize = resourcePCT.header.mipLevel[counter - 1].size;

                if (file.Length != expectedSize)
                {
                    throw new Exception("\nCreated mip " + mipname + " is not the expected size. Expected size is " + expectedSize + ".");
                }
            }
        }

        public static ResourcePct Deserializer(String resourcefilepath)
        {
            var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

            using var resourcefilestream = File.Open(resourcefilepath, FileMode.Open, FileAccess.Read);
            using var streamReader = new StreamReader(resourcefilestream, leaveOpen: true);
            var yaml = streamReader.ReadToEnd();

            var pctresource = deserializer.Deserialize<ResourcePct>(yaml);
            return pctresource;
        }
    }
}
