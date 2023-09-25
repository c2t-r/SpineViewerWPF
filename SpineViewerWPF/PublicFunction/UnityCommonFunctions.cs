using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SpineViewerWPF.PublicFunction
{
    public static class UnityCommonFunctions
    {
        private static byte[] chars = new byte[32];
        private static byte[] bytesBigEndian = new byte[4];

        public static sbyte ReadSByte(this Stream input)
        {
            int value = input.ReadByte();
            if (value == -1) throw new EndOfStreamException();
            return (sbyte)value;
        }

        public static bool ReadBoolean(this Stream input)
        {
            return input.ReadByte() != 0;
        }

        public static float ReadFloat(this Stream input)
        {
            input.Read(bytesBigEndian, 0, 4);
            chars[3] = bytesBigEndian[0];
            chars[2] = bytesBigEndian[1];
            chars[1] = bytesBigEndian[2];
            chars[0] = bytesBigEndian[3];
            return BitConverter.ToSingle(chars, 0);
        }

        public static int ReadInt(this Stream input)
        {
            input.Read(bytesBigEndian, 0, 4);
            return (bytesBigEndian[0] << 24)
                + (bytesBigEndian[1] << 16)
                + (bytesBigEndian[2] << 8)
                + bytesBigEndian[3];
        }

        public static int ReadInt(this Stream input, bool optimizePositive)
        {
            int b = input.ReadByte();
            int result = b & 0x7F;
            if ((b & 0x80) != 0)
            {
                b = input.ReadByte();
                result |= (b & 0x7F) << 7;
                if ((b & 0x80) != 0)
                {
                    b = input.ReadByte();
                    result |= (b & 0x7F) << 14;
                    if ((b & 0x80) != 0)
                    {
                        b = input.ReadByte();
                        result |= (b & 0x7F) << 21;
                        if ((b & 0x80) != 0) result |= (input.ReadByte() & 0x7F) << 28;
                    }
                }
            }
            return optimizePositive ? result : ((result >> 1) ^ -(result & 1));
        }

        public static string ReadString(this Stream input)
        {
            int byteCount = ReadInt(input, true);
            switch (byteCount)
            {
                case 0:
                    return null;
                case 1:
                    return "";
            }
            byteCount--;
            byte[] buffer = chars;
            if (buffer.Length < byteCount) buffer = new byte[byteCount];
            ReadFully(input, buffer, 0, byteCount);
            return System.Text.Encoding.UTF8.GetString(buffer, 0, byteCount);
        }

        public static void ReadFully(this Stream input, byte[] buffer, int offset, int length)
        {
            while (length > 0)
            {
                int count = input.Read(buffer, offset, length);
                if (count <= 0) throw new EndOfStreamException();
                offset += count;
                length -= count;
            }
        }
    }
}
