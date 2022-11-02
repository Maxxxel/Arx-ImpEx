using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Arx_Model_Exporter.Helpers
{
    public class FileWrapper
    {
        private const int bufferSize = 8;
        public string folderPath { get; private set; }
        public string name { get; private set; }
        public string fileExtension { get; private set; }
        public FileStream fileStream;
        private byte[] readBuffer;

        public FileWrapper(string filePath)
        {
            filePath = filePath.Replace('\\', '/');

            folderPath = filePath.Substring(0, filePath.LastIndexOf('/') + 1);
            name = filePath.Substring(filePath.LastIndexOf('/') + 1);
            name = name.Substring(0, name.ToLower().LastIndexOf('.'));
            fileExtension = filePath.Substring(filePath.LastIndexOf('.'));

            FileMode fileMode;
            if (File.Exists(filePath) == true)
            {
                fileMode = FileMode.Open;
            }
            else
            {
                fileMode = FileMode.Create;

                Directory.CreateDirectory(folderPath);
            }

            fileStream = File.Open(filePath, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite);
            readBuffer = new byte[bufferSize];
        }

        public FileWrapper(FileStream fileStream)
        {
            string filePath = fileStream.Name.Replace('\\', '/');

            folderPath = filePath.Substring(0, filePath.LastIndexOf('/') + 1);
            fileStream.Position = 0;
            this.fileStream = fileStream;
            readBuffer = new byte[bufferSize];
        }

        ~FileWrapper()
        {
            this.Close();
        }

        #region misc methods

        /// <summary>
        /// Seeks to the specified offset.
        /// If this method is passed using only default parameters (attempting to seek to offset -1), then it is assumed that the seek was meant to be skipped.
        /// This is because all read/write methods support seeking prior to reading or writing, however the default parameters are used to show that this seek was not requested.
        /// </summary>
        /// <param name="offset">The offset to seek to, relative to seekOrigin.</param>
        /// <param name="seekOrigin">The SeekOrigin setting to use.</param>
        public void Seek(int offset = -1, SeekOrigin seekOrigin = SeekOrigin.Begin)
        {
            if (offset != -1 || seekOrigin != SeekOrigin.Begin)
            {
                fileStream.Seek(offset, seekOrigin);
            }
            else
            {
                // we are only using default parameters, both of which combined are invalid, signalling that the seek was unintended
            }
        }

        public int GetFilePosition()
        {
            return (int)fileStream.Position;
        }

        public void Open()
        {
            fileStream = File.Open(this.GetFullFilePath(), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        public void Close()
        {
            fileStream.Close();
        }

        public string GetFolderPath()
        {
            return folderPath;
        }

        public string GetFileExtension()
        {
            return fileExtension;
        }

        public string GetName()
        {
            return name;
        }

        public string GetFullFilePath()
        {
            return (folderPath + name + fileExtension);  // 'folderPath' already includes a trailing '/', and 'fileExtension' already includes a leading '.'
        }

        public string GetShortFilePath()
        {
            return name + fileExtension;
        }

        public int GetLength()
        {
            return (int)fileStream.Length;
        }

        public bool EndOfFile()
        {
            return fileStream.Position >= fileStream.Length;
        }

        public void Clear()
        {
            fileStream.SetLength(0);
        }

        public void Rewind()
        {
            this.Seek(0);
        }

        #endregion

        #region write methods

        public void WriteString(string s, int offset = -1)
        {
            s += "\x00";
            WriteChars(s.ToCharArray(), offset);
        }

        public void WriteChars(char[] chars, int offset = -1)
        {
            byte[] bytes = new byte[chars.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)chars[i];
            }

            WriteBytes(bytes, offset);
        }

        public void WriteInt(int n, int offset = -1)
        {
            byte[] bytes = new byte[4];

            // as per Riot standards, use little-endian format
            for (int i = 0; i < 4; i++)
            {
                bytes[i] = (byte)(n & 255);
                n = n >> 8;
            }

            WriteBytes(bytes, offset);
        }

        public void WriteFloat(float f, int offset = -1)
        {
            byte[] bytes = System.BitConverter.GetBytes(f);

            // endianness cannot be set directly and is dependent on the computer's system architecture
            if (System.BitConverter.IsLittleEndian == false)
            {
                // bytes were returned in big-endian format, so reverse the array

                byte temp = bytes[3];
                bytes[3] = bytes[0];
                bytes[0] = temp;

                temp = bytes[2];
                bytes[2] = bytes[1];
                bytes[1] = temp;
            }

            WriteBytes(bytes, offset);
        }

        public void WriteShort(int n, int offset = -1)
        {
            byte[] bytes = new byte[2];

            for (int i = 0; i < 2; i++)
            {
                bytes[i] = (byte)(n & 255);
                n = n >> 8;
            }

            WriteBytes(bytes, offset);
        }

        public void WriteByte(int n, int offset = -1)
        {
            byte[] bytes = new byte[] { (byte)n };
            WriteBytes(bytes, offset);
        }

        public void WriteBytes(byte[] bytes, int offset = -1)
        {
            try
            {
                int oldPosition = GetFilePosition();
                Seek(offset);
                fileStream.Write(bytes, 0, bytes.Length);

                if (offset != -1)
                {
                    Seek(oldPosition);
                }
            }
            catch (System.Exception e)
            {
                string errorString = "Error writing to RunePageFile:  ";

                for (int i = 0; i < bytes.Length; i++)
                {
                    errorString += bytes[i] + " ";
                }

                errorString += "\n" + e.GetType() + "\n" + e.StackTrace;

                Console.WriteLine(errorString);
            }
        }

        public void WriteBool(bool b, int offset = -1)
        {
            if (b == true)
            {
                WriteByte(1, offset);
            }
            else
            {
                WriteByte(0, offset);
            }
        }

        public void WriteLine(string s = "", int offset = -1)
        {
            s += "\r\n";  // end-line characters formatted so that they look correct in notepad
            WriteChars(s.ToCharArray(), offset);
        }

        //public void WriteVector3(Vector3 v, int offset = -1)
        //{
        //    WriteFloat(v.x, offset);
        //    WriteFloat(v.y);
        //    WriteFloat(v.z);
        //}

        //public void WriteColor(Color c, int offset = -1)
        //{
        //    WriteByte(c.b, offset);  // little endian colors
        //    WriteByte(c.g);
        //    WriteByte(c.r);
        //}

        #endregion

        #region read methods

        public string ReadString(int length = -1, int offset = -1)
        {
            try
            {
                Seek(offset);

                string s = "";
                if (length < 0)
                {
                    char c = ReadChar();

                    while (c != '\x00')
                    {
                        s += c;
                        c = ReadChar();
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        s += ReadChar();
                    }
                }
                return s;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error reading string:  " + e.GetType() + "\n" + e.StackTrace);
                return "";
            }
        }

        public string ReadAllText()
        {
            Seek(0);
            return ReadString(this.GetLength());
        }

        public char ReadChar(int offset = -1)
        {
            try
            {
                return (char)ReadByte(offset);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error reading char:  " + e.GetType() + "\n" + e.StackTrace);
                return '\x00';
            }
        }

        public char[] ReadChars(int offset = -1, int count = 1)
        {
            try
            {
                char[] ret = new char[count];

                for (int i = 0; i < count; i++)
                    ret[i] = ReadChar();

                return ret;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error reading char:  " + e.GetType() + "\n" + e.StackTrace);
                return new char['\x00'];
            }
        }


        public int ReadInt(int offset = -1)
        {
            try
            {
                Seek(offset);

                fileStream.Read(readBuffer, 0, 4);

                int n = 0;
                for (int i = 0; i < 4; i++)
                {
                    n = n << 8;
                    n += readBuffer[3 - i];
                }

                return n;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error reading int:  " + e.GetType() + "\n" + e.StackTrace);
                return 0;
            }
        }

        public int[] ReadInts(int offset = -1, int count = 1)
        {
            int[] ret = new int[count];

            for (int i = 0; i < count; i++)
            {
                ret[i] = ReadInt(offset);
            }

            return ret;
        }
        public uint ReadUInt(int offset = -1)
        {
            try
            {
                Seek(offset);

                fileStream.Read(readBuffer, 0, 4);

                uint n = 0;
                for (int i = 0; i < 4; i++)
                {
                    n = n << 8;
                    n += readBuffer[3 - i];
                }

                return n;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error reading int:  " + e.GetType() + "\n" + e.StackTrace);
                return 0;
            }
        }
        public Int64 ReadInt64(int offset = -1)
        {
            try
            {
                Seek(offset);

                fileStream.Read(readBuffer, 0, 4);

                Int64 n = 0;
                for (int i = 0; i < 4; i++)
                {
                    n = n << 8;
                    n += readBuffer[3 - i];
                }

                return n;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error reading int:  " + e.GetType() + "\n" + e.StackTrace);
                return 0;
            }
        }

        public float ReadFloat(int offset = -1)
        {
            try
            {
                Seek(offset);

                fileStream.Read(readBuffer, 0, 4);

                // endianness cannot be set directly and is dependent on the computer's system architecture
                if (System.BitConverter.IsLittleEndian == false)
                {
                    // bytes are to be read in big-endian format, so reverse the array

                    byte temp = readBuffer[3];
                    readBuffer[3] = readBuffer[0];
                    readBuffer[0] = temp;

                    temp = readBuffer[2];
                    readBuffer[2] = readBuffer[1];
                    readBuffer[1] = temp;
                }

                float f = System.BitConverter.ToSingle(readBuffer, 0);

                return f;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error reading float:  " + e.GetType() + "\n" + e.StackTrace);
                return 0f;
            }
        }

        public float[] ReadFloats(int offset = -1, int count = 1)
        {
            float[] ret = new float[count];

            for (int i = 0; i < count; i++)
            {
                ret[i] = ReadFloat(offset);
            }

            return ret;
        }

        public short ReadShort(int offset = -1)
        {
            try
            {
                Seek(offset);

                fileStream.Read(readBuffer, 0, 2);

                int n = 0;
                for (int i = 0; i < 2; i++)
                {
                    n = n << 8;
                    n += readBuffer[1 - i];
                }

                return (short)n;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error reading short:  " + e.GetType() + "\n" + e.StackTrace);
                return 0;
            }
        }

        public short[] ReadShorts(int offset = -1, int count = 1)
        {
            short[] ret = new short[count];

            for (int i = 0; i < count; i++)
            {
                ret[i] = ReadShort(offset);
            }

            return ret;
        }
        public byte ReadByte(int offset = -1)
        {
            try
            {
                Seek(offset);
                return (byte)fileStream.ReadByte();
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error reading byte:  " + e.GetType() + "\n" + e.StackTrace);
                return 0;
            }
        }

        public byte[] ReadBytes(int offset = -1, int count = 1)
        {
            try
            {
                Seek(offset);
                byte[] ret = new byte[count];

                for (int i = 0; i < count; i++)
                    ret[i] = ReadByte();

                return ret;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error reading byte:  " + e.GetType() + "\n" + e.StackTrace);
                return null;
            }
        }

        public bool ReadBool(int offset = -1)
        {
            int n = ReadInt(offset);
            if (n != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string ReadLine(int offset = -1)
        {
            try
            {
                Seek(offset);

                string s = "";

                char c = ReadChar();

                while (c != '\r')
                {
                    s += c;
                    c = ReadChar();
                }

                // end-lines are formatted as "\r\n" so that they appear correct in notepad,
                // and this line is needed to read the additional '\n' character that makes up an end-line
                ReadChar();

                return s;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error reading line:  " + e.GetType() + "\n" + e.StackTrace);
                return "";
            }
        }

        public static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public string ReadLineAssignment(int offset = -1)
        {
            string s = ReadLine(offset);
            s = s.Substring(s.IndexOf('=') + 1);
            return s;
        }

        internal ushort[] ReadUShorts(int offset, int count)
        {
            ushort[] ret = new ushort[count];

            for (int i = 0; i < count; i++)
            {
                ret[i] = ReadUShort(offset);
            }

            return ret;
        }

        private ushort ReadUShort(int offset)
        {
            try
            {
                Seek(offset);

                fileStream.Read(readBuffer, 0, 2);

                int n = 0;
                for (int i = 0; i < 2; i++)
                {
                    n = n << 8;
                    n += readBuffer[1 - i];
                }

                return (ushort)n;
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error reading short:  " + e.GetType() + "\n" + e.StackTrace);
                return 0;
            }
        }

        internal Matrix4x4 ReadMatrix()
        {
            Matrix4x4 matrix4X4 = new Matrix4x4();
            matrix4X4.M11 = ReadFloat();
            matrix4X4.M12 = ReadFloat();
            matrix4X4.M13 = ReadFloat();
            matrix4X4.M14 = ReadFloat();
            matrix4X4.M21 = ReadFloat();
            matrix4X4.M22 = ReadFloat();
            matrix4X4.M23 = ReadFloat();
            matrix4X4.M24 = ReadFloat();
            matrix4X4.M31 = ReadFloat();
            matrix4X4.M32 = ReadFloat();
            matrix4X4.M33 = ReadFloat();
            matrix4X4.M34 = ReadFloat();
            matrix4X4.M41 = ReadFloat();
            matrix4X4.M42 = ReadFloat();
            matrix4X4.M43 = ReadFloat();
            matrix4X4.M44 = ReadFloat();
            
            return matrix4X4;
        }

        internal Vector3 ReadVector3()
        {
            Vector3 vector3 = new Vector3();
            vector3.X = ReadFloat();
            vector3.Y = ReadFloat();
            vector3.Z = ReadFloat();
            
            return vector3;
        }

        internal Vector2 ReadVector2()
        {
            Vector2 vector2 = new Vector2();
            vector2.X = ReadFloat();
            vector2.Y = ReadFloat();

            return vector2;
        }

        internal Quaternion ReadArxQuaternion()
        {
            Quaternion quaternion = new Quaternion();
            quaternion.W = ReadFloat();
            quaternion.X = ReadFloat();
            quaternion.Y = ReadFloat();
            quaternion.Z = ReadFloat();

            return quaternion;
        }

        internal Quaternion ReadQuaternion()
        {
            Quaternion quaternion = new Quaternion();
            quaternion.X = ReadFloat();
            quaternion.Y = ReadFloat();
            quaternion.Z = ReadFloat();
            quaternion.W = ReadFloat();

            return quaternion;
        }

        internal ulong ReadUInt64()
        {
            throw new NotImplementedException();
        }

        //public Vector3 ReadVector3(int offset = -1)
        //{
        //    return new Vector3(ReadFloat(offset), ReadFloat(), ReadFloat());
        //}

        //public Color ReadColor(int offset = -1)
        //{
        //    int b = ReadByte(offset);  // little endian colors
        //    int g = ReadByte();
        //    int r = ReadByte();

        //    return new Color(r, g, b);
        //}

        #endregion
    }
}
