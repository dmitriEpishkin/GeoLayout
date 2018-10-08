using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Nordwest.Wpf.Controls
{
    public class BitmapWriter : BinaryWriter
    {
        private BitmapFileHeader _bitmapFileHeader;
        private BitmapInfoHeader _bitmapInfoHeader;
        private int _width;
        private int _height;

        public BitmapWriter(Stream stream, int width, int height)
            : base(stream)
        {
            _width = width;
            _height = height;
            var fileSize = width * height * 4 + 14 + 40;
            _bitmapFileHeader = new BitmapFileHeader((uint)fileSize, 14 + 40);
            _bitmapInfoHeader = new BitmapInfoHeader(width, -height);
            WriteHeader();
        }

        private void WriteHeader()
        {
            _bitmapFileHeader.Write(this);
            _bitmapInfoHeader.Write(this);
        }

        public void WriteImageLine(List<BitmapSource> images, int imageWidth, int imageHeight)
        {
            for (int i = 0; i < imageHeight; i++)
            {
                for (int j = 0; j < images.Count; j++)
                {
                    var buff = new byte[imageWidth * 4];
                    images[j].CopyPixels(new Int32Rect(0, i, imageWidth, 1), buff, imageWidth * 4, 0);//.Read(buff, 0, imageWidth * 4);
                    Write(buff);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 14)]
        struct BitmapFileHeader
        {
            ushort bfType;        // смещение 0 байт от начала файла
            uint bfSize;        // смещение 2 байта от начала файла, длина 4 байта
            ushort bfReserved1;
            ushort bfReserved2;
            uint bfOffBits;     // смещение 10 байт от начала файла, длина 4 байта

            public BitmapFileHeader(uint fileSize, uint offBits)
            {
                bfType = 0x4d42;
                bfSize = fileSize;
                bfReserved1 = 0;
                bfReserved2 = 0;
                bfOffBits = offBits;
            }

            public void Write(BinaryWriter binaryWriter)
            {
                binaryWriter.Write(bfType);
                binaryWriter.Write(bfSize);
                binaryWriter.Write(bfReserved1);
                binaryWriter.Write(bfReserved2);
                binaryWriter.Write(bfOffBits);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 40)]
        struct BitmapInfoHeader
        {
            uint biSize;
            int biWidth;
            int biHeight;
            ushort biPlanes;
            ushort biBitCount;
            uint biCompression;
            uint biSizeImage;
            int biXPelsPerMeter;
            int biYPelsPerMeter;
            uint biClrUsed;
            uint biClrImportant;

            public BitmapInfoHeader(int width, int height)
            {
                biSize = 40;
                biWidth = width;
                biHeight = height;
                biPlanes = 1;
                biBitCount = 32;
                biCompression = 0;
                biSizeImage = 0;
                biXPelsPerMeter = 3780;
                biYPelsPerMeter = 3780;
                biClrUsed = 0;
                biClrImportant = 0;
            }

            public void Write(BinaryWriter binaryWriter)
            {
                binaryWriter.Write(biSize);
                binaryWriter.Write(biWidth);
                binaryWriter.Write(biHeight);
                binaryWriter.Write(biPlanes);
                binaryWriter.Write(biBitCount);
                binaryWriter.Write(biCompression);
                binaryWriter.Write(biSizeImage);
                binaryWriter.Write(biXPelsPerMeter);
                binaryWriter.Write(biYPelsPerMeter);
                binaryWriter.Write(biClrUsed);
                binaryWriter.Write(biClrImportant);
            }
        }
    }
}