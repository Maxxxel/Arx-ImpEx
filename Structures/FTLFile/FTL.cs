using Arx_Model_Exporter.Helpers;
using System.IO;
using Path = System.IO.Path;
using Arx_Model_Exporter.Structures.GLTFFile;
using Arx_Model_Exporter.Structures.TEAFile;
using System.Runtime.InteropServices;
using System;
using Arx_Model_Exporter.Structures.EERIE;

namespace Arx_Model_Exporter.Structures.FTLFile
{
    public class FTL
    {
        [DllImport("ArxIO", EntryPoint = "ArxIO_unpack_alloc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnpackAlloc(IntPtr in_, uint inSize, out IntPtr out_, out uint outSize);
        [DllImport("ArxIO", EntryPoint = "ArxIO_unpack_free", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnpackFree(IntPtr buffer);
        public string? DirectoryPath { get; }
        public EERIE_3DOBJ eerieObject { get; private set; }
        public GLTFSkeleton Skeleton { get; private set; }
        public AnimationData[] AnimationData { get; internal set; }

        public FTL(string fileName)
        {
            DirectoryPath = Path.GetDirectoryName(fileName);

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] outBytes;
                long size = fs.Length;
                byte[] bytes = new byte[size];
                fs.Read(bytes, 0, (int)size);
                uint inSize = (uint)bytes.Length;
                var pinnedBytes = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                UnpackAlloc(pinnedBytes.AddrOfPinnedObject(), inSize, out IntPtr outPtr, out uint outSize);
                pinnedBytes.Free();
                outBytes = new byte[outSize];
                Marshal.Copy(outPtr, outBytes, 0, (int)outSize);
                UnpackFree(outPtr);

                MemoryWrapper File = new MemoryWrapper(outBytes, Path.GetDirectoryName(fileName));
                eerieObject = new EERIE_3DOBJ(File);
                Skeleton = new GLTFSkeleton(eerieObject);
            };
        }
    }
}
