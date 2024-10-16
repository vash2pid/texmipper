﻿
using YamlDotNet.Serialization;
using System.Diagnostics;
using DirectXTexNet;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using Windows.Devices.Portable;
using System.Reflection;

namespace texmipper
{
    internal class TexMipper
    {

        [STAThread]
        static void Main(string[] args)
        {
            string[] texturefiles = Array.Empty<string>();

            var texturefiledialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Select texture files to convert...",
                Filter = "Texture File|*.tga;"
            };
            using (texturefiledialog)
            {
                if (texturefiledialog.ShowDialog() == DialogResult.OK)
                {
                    texturefiles = texturefiledialog.FileNames;
                }
            }

            int ctr = 1;
            foreach (var texturefilepath in texturefiles)
            {
                Console.WriteLine("Processing file {0} of {1}", ctr, texturefiles.Length);
                Console.WriteLine("Texture file selected:\n" + texturefilepath);

                var resourcefilepath = texturefilepath.Replace(".tga", ".pct.resource");

                if (!File.Exists(resourcefilepath))
                {
                    Console.WriteLine("Resource file for selected file texture file not found in the same location." +
                        "\nThe resource file must have the same name as the texture file.\n\n");

                }
                else
                {
                    Console.WriteLine("\nResrouce file selected:\n" + resourcefilepath);

                    ResourcePct resourcePCT = Deserializer(resourcefilepath);

                    ConvertToPCT(resourcePCT, texturefilepath);

                    Console.WriteLine("\n\nProcessing of {0} completed", texturefilepath);
                }

                ctr++;
            }

            if (texturefiles.Length == 0) { Console.WriteLine("\n\nNo files are selected"); }

            Console.WriteLine("\n\nPress any key to close this window...");
            Console.ReadKey();
        }
        private static void ConvertToPCT(ResourcePct resourcePCT, String inputtexturefilepath)
        {
            Console.WriteLine("Converting from *.tga to *.pct_mip files based on the number of mipmaps in resource file\n\n");

            int counter = 1;
            int cursizex = resourcePCT.header.sx;
            int cursizey = resourcePCT.header.sy;

            try
            {

                SharpDX.DXGI.Factory f = new SharpDX.DXGI.Factory1();
                Device device = null;

                for (int index = 0; index < f.GetAdapterCount(); index++)
                {
                    SharpDX.DXGI.Adapter a = f.GetAdapter(index);

                    if (a.Description.DeviceId == 140) continue;

                    FeatureLevel[] levels = new FeatureLevel[]
                    {
                        FeatureLevel.Level_11_1,
                        FeatureLevel.Level_11_0,
                        FeatureLevel.Level_10_1,
                        FeatureLevel.Level_10_0,
                        FeatureLevel.Level_9_1
                     };

                    DeviceCreationFlags flags = DeviceCreationFlags.BgraSupport;

                    device = new Device(a, flags, levels);

                    if (device.FeatureLevel >= FeatureLevel.Level_10_0)
                    {
                        break;
                    }
                }

                if (device != null && device.FeatureLevel < FeatureLevel.Level_10_0)
                {
                    Console.WriteLine("No device compatible for GPU compute found");
                    Console.WriteLine("Using software conversion instead with multitreading support");
                    Console.WriteLine("CPU usage will be high and converstion time may take longer to complete");
                }

                foreach (String mip in resourcePCT.mipMaps)
                {
                    Console.WriteLine("\nConverting " + mip);
                    if (File.Exists(Path.GetDirectoryName(inputtexturefilepath) + "\\" + mip))
                        File.Move(Path.GetDirectoryName(inputtexturefilepath) + "\\" + mip
                            , Path.GetDirectoryName(inputtexturefilepath) + "\\" + mip + "." + DateTime.Now.ToString("yyyyMMddHHmmssffff"));

                    if (counter > 1)
                    {
                        cursizex = cursizex / 2;
                        cursizey = cursizey / 2;
                    }

                    var ddsimage = TexHelper.Instance.Initialize2D(GetDXGIFormat(resourcePCT.TextureFormat), cursizex, cursizey, 1, 1, CP_FLAGS.NONE);

                    using var tgaimage = TexHelper.Instance.LoadFromTGAFile(inputtexturefilepath);
                    using var resizedimage = tgaimage.Resize(cursizex, cursizey, TEX_FILTER_FLAGS.DEFAULT);
                    if (device != null && device.FeatureLevel >= FeatureLevel.Level_10_0)
                    {
                        ddsimage = resizedimage.Compress(device.NativePointer, GetDXGIFormat(resourcePCT.TextureFormat), TEX_COMPRESS_FLAGS.DEFAULT, 1.0f);
                    }
                    else
                    {
                        ddsimage = resizedimage.Compress(GetDXGIFormat(resourcePCT.TextureFormat), TEX_COMPRESS_FLAGS.PARALLEL, 0.50f);
                    }
                    var srcData = ddsimage.GetPixels();
                    var destData = ddsimage.GetPixels();

                    if (ddsimage.GetPixelsSize() < ddsimage.GetPixelsSize()) throw new Exception("Source data will not fit");

                    using var ddsstream = ddsimage.SaveToDDSMemory(DDS_FLAGS.NONE);
                    using var outputstream = File.Create(Path.GetDirectoryName(inputtexturefilepath) + "\\" + mip);
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
                    counter++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
            uint reserved2 = binaryStream.ReadUInt32();

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
            if (File.Exists(mipname))
            {
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

