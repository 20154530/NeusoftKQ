

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AforgeNumVerify.AForge.Core {
    public class UnsupportedImageFormatException : ArgumentException {
        public UnsupportedImageFormatException() { }
        public UnsupportedImageFormatException(string message) :
            base(message) { }
        public UnsupportedImageFormatException(string message, string paramName) :
            base(message, paramName) { }
    }
    public class InvalidImagePropertiesException : ArgumentException {
        public InvalidImagePropertiesException() { }
        public InvalidImagePropertiesException(string message) :
            base(message) { }
        public InvalidImagePropertiesException(string message, string paramName) :
            base(message, paramName) { }
    }
    public class RGB {
        public const short R = 2;
        public const short G = 1;
        public const short B = 0;
        public const short A = 3;
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Alpha;
        public System.Drawing.Color Color {
            get { return Color.FromArgb(Alpha, Red, Green, Blue); }
            set {
                Red = value.R;
                Green = value.G;
                Blue = value.B;
                Alpha = value.A;
            }
        }
        public RGB() {
            Red = 0;
            Green = 0;
            Blue = 0;
            Alpha = 255;
        }
        public RGB(byte red, byte green, byte blue) {
            this.Red = red;
            this.Green = green;
            this.Blue = blue;
            this.Alpha = 255;
        }
        public RGB(byte red, byte green, byte blue, byte alpha) {
            this.Red = red;
            this.Green = green;
            this.Blue = blue;
            this.Alpha = alpha;
        }
        public RGB(System.Drawing.Color color) {
            this.Red = color.R;
            this.Green = color.G;
            this.Blue = color.B;
            this.Alpha = color.A;
        }
    }
    public enum ObjectsOrder {
        None,
        Size,
        Area,
        YX,
        XY
    }
    public class UnmanagedImage : IDisposable {
        private IntPtr imageData;
        private int width, height;
        private int stride;
        private PixelFormat pixelFormat;
        private bool mustBeDisposed = false;
        public IntPtr ImageData {
            get { return imageData; }
        }
        public int Width {
            get { return width; }
        }
        public int Height {
            get { return height; }
        }
        public int Stride {
            get { return stride; }
        }
        public PixelFormat PixelFormat {
            get { return pixelFormat; }
        }
        public UnmanagedImage(IntPtr imageData, int width, int height, int stride, PixelFormat pixelFormat) {
            this.imageData = imageData;
            this.width = width;
            this.height = height;
            this.stride = stride;
            this.pixelFormat = pixelFormat;
        }
        public UnmanagedImage(BitmapData bitmapData) {
            this.imageData = bitmapData.Scan0;
            this.width = bitmapData.Width;
            this.height = bitmapData.Height;
            this.stride = bitmapData.Stride;
            this.pixelFormat = bitmapData.PixelFormat;
        }
        ~UnmanagedImage() {
            Dispose(false);
        }
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
            }
            if ((mustBeDisposed) && (imageData != IntPtr.Zero)) {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(imageData);
                System.GC.RemoveMemoryPressure(stride * height);
                imageData = IntPtr.Zero;
            }
        }
        public UnmanagedImage Clone() {
            IntPtr newImageData = System.Runtime.InteropServices.Marshal.AllocHGlobal(stride * height);
            System.GC.AddMemoryPressure(stride * height);
            UnmanagedImage newImage = new UnmanagedImage(newImageData, width, height, stride, pixelFormat);
            newImage.mustBeDisposed = true;
            SystemTools.CopyUnmanagedMemory(newImageData, imageData, stride * height);
            return newImage;
        }
        public void Copy(UnmanagedImage destImage) {
            if (
                (width != destImage.width) || (height != destImage.height) ||
                (pixelFormat != destImage.pixelFormat)) {
                throw new InvalidImagePropertiesException("Destination image has different size or pixel format.");
            }
            if (stride == destImage.stride) {
                SystemTools.CopyUnmanagedMemory(destImage.imageData, imageData, stride * height);
            } else {
                unsafe {
                    int dstStride = destImage.stride;
                    int copyLength = (stride < dstStride) ? stride : dstStride;
                    byte* src = (byte*)imageData.ToPointer();
                    byte* dst = (byte*)destImage.imageData.ToPointer();
                    for (int i = 0; i < height; i++) {
                        SystemTools.CopyUnmanagedMemory(dst, src, copyLength);
                        dst += dstStride;
                        src += stride;
                    }
                }
            }
        }
        public static UnmanagedImage Create(int width, int height, PixelFormat pixelFormat) {
            int bytesPerPixel = 0;
            switch (pixelFormat) {
                case PixelFormat.Format8bppIndexed:
                    bytesPerPixel = 1;
                    break;
                case PixelFormat.Format16bppGrayScale:
                    bytesPerPixel = 2;
                    break;
                case PixelFormat.Format24bppRgb:
                    bytesPerPixel = 3;
                    break;
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    bytesPerPixel = 4;
                    break;
                case PixelFormat.Format48bppRgb:
                    bytesPerPixel = 6;
                    break;
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    bytesPerPixel = 8;
                    break;
                default:
                    throw new UnsupportedImageFormatException("Can not create image with specified pixel format.");
            }
            if ((width <= 0) || (height <= 0)) {
                throw new InvalidImagePropertiesException("Invalid image size specified.");
            }
            int stride = width * bytesPerPixel;
            if (stride % 4 != 0) {
                stride += (4 - (stride % 4));
            }
            IntPtr imageData = System.Runtime.InteropServices.Marshal.AllocHGlobal(stride * height);
            SystemTools.SetUnmanagedMemory(imageData, 0, stride * height);
            System.GC.AddMemoryPressure(stride * height);
            UnmanagedImage image = new UnmanagedImage(imageData, width, height, stride, pixelFormat);
            image.mustBeDisposed = true;
            return image;
        }
        public Bitmap ToManagedImage() {
            return ToManagedImage(true);
        }
        public Bitmap ToManagedImage(bool makeCopy) {
            Bitmap dstImage = null;
            try {
                if (!makeCopy) {
                    dstImage = new Bitmap(width, height, stride, pixelFormat, imageData);
                    if (pixelFormat == PixelFormat.Format8bppIndexed) {
                        Imaging.SetGrayscalePalette(dstImage);
                    }
                } else {
                    dstImage = (pixelFormat == PixelFormat.Format8bppIndexed) ?
    Imaging.CreateGrayscaleImage(width, height) :
    new Bitmap(width, height, pixelFormat);
                    BitmapData dstData = dstImage.LockBits(
    new Rectangle(0, 0, width, height),
    ImageLockMode.ReadWrite, pixelFormat);
                    int dstStride = dstData.Stride;
                    int lineSize = Math.Min(stride, dstStride);
                    unsafe {
                        byte* dst = (byte*)dstData.Scan0.ToPointer();
                        byte* src = (byte*)imageData.ToPointer();
                        if (stride != dstStride) {
                            for (int y = 0; y < height; y++) {
                                SystemTools.CopyUnmanagedMemory(dst, src, lineSize);
                                dst += dstStride;
                                src += stride;
                            }
                        } else {
                            SystemTools.CopyUnmanagedMemory(dst, src, stride * height);
                        }
                    }
                    dstImage.UnlockBits(dstData);
                }
                return dstImage;
            } catch (Exception) {
                if (dstImage != null) {
                    dstImage.Dispose();
                }
                throw new InvalidImagePropertiesException("The unmanaged image has some invalid properties, which results in failure of converting it to managed image.");
            }
        }
        public static UnmanagedImage FromManagedImage(Bitmap image) {
            UnmanagedImage dstImage = null;
            BitmapData sourceData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);
            try {
                dstImage = FromManagedImage(sourceData);
            } finally {
                image.UnlockBits(sourceData);
            }
            return dstImage;
        }
        public static UnmanagedImage FromManagedImage(BitmapData imageData) {
            PixelFormat pixelFormat = imageData.PixelFormat;
            if (
    (pixelFormat != PixelFormat.Format8bppIndexed) &&
    (pixelFormat != PixelFormat.Format16bppGrayScale) &&
    (pixelFormat != PixelFormat.Format24bppRgb) &&
    (pixelFormat != PixelFormat.Format32bppRgb) &&
    (pixelFormat != PixelFormat.Format32bppArgb) &&
    (pixelFormat != PixelFormat.Format32bppPArgb) &&
    (pixelFormat != PixelFormat.Format48bppRgb) &&
    (pixelFormat != PixelFormat.Format64bppArgb) &&
    (pixelFormat != PixelFormat.Format64bppPArgb)) {
                throw new UnsupportedImageFormatException("Unsupported pixel format of the source image.");
            }
            IntPtr dstImageData = System.Runtime.InteropServices.Marshal.AllocHGlobal(imageData.Stride * imageData.Height);
            System.GC.AddMemoryPressure(imageData.Stride * imageData.Height);
            UnmanagedImage image = new UnmanagedImage(dstImageData, imageData.Width, imageData.Height, imageData.Stride, pixelFormat);
            SystemTools.CopyUnmanagedMemory(dstImageData, imageData.Scan0, imageData.Stride * imageData.Height);
            image.mustBeDisposed = true;
            return image;
        }
        public byte[] Collect8bppPixelValues(List<IntPoint> points) {
            int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;
            if ((pixelFormat == PixelFormat.Format16bppGrayScale) || (pixelSize > 4)) {
                throw new UnsupportedImageFormatException("Unsupported pixel format of the source image. Use Collect16bppPixelValues() method for it.");
            }
            byte[] pixelValues = new byte[points.Count * ((pixelFormat == PixelFormat.Format8bppIndexed) ? 1 : 3)];
            unsafe {
                byte* basePtr = (byte*)imageData.ToPointer();
                byte* ptr;
                if (pixelFormat == PixelFormat.Format8bppIndexed) {
                    int i = 0;
                    foreach (IntPoint point in points) {
                        ptr = basePtr + stride * point.Y + point.X;
                        pixelValues[i++] = *ptr;
                    }
                } else {
                    int i = 0;
                    foreach (IntPoint point in points) {
                        ptr = basePtr + stride * point.Y + point.X * pixelSize;
                        pixelValues[i++] = ptr[RGB.R];
                        pixelValues[i++] = ptr[RGB.G];
                        pixelValues[i++] = ptr[RGB.B];
                    }
                }
            }
            return pixelValues;
        }
        public List<IntPoint> CollectActivePixels() {
            return CollectActivePixels(new Rectangle(0, 0, width, height));
        }
        public List<IntPoint> CollectActivePixels(Rectangle rect) {
            List<IntPoint> pixels = new List<IntPoint>();
            int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;
            rect.Intersect(new Rectangle(0, 0, width, height));
            int startX = rect.X;
            int startY = rect.Y;
            int stopX = rect.Right;
            int stopY = rect.Bottom;
            unsafe {
                byte* basePtr = (byte*)imageData.ToPointer();
                if ((pixelFormat == PixelFormat.Format16bppGrayScale) || (pixelSize > 4)) {
                    int pixelWords = pixelSize >> 1;
                    for (int y = startY; y < stopY; y++) {
                        ushort* ptr = (ushort*)(basePtr + y * stride + startX * pixelSize);
                        if (pixelWords == 1) {
                            for (int x = startX; x < stopX; x++, ptr++) {
                                if (*ptr != 0) {
                                    pixels.Add(new IntPoint(x, y));
                                }
                            }
                        } else {
                            for (int x = startX; x < stopX; x++, ptr += pixelWords) {
                                if ((ptr[RGB.R] != 0) || (ptr[RGB.G] != 0) || (ptr[RGB.B] != 0)) {
                                    pixels.Add(new IntPoint(x, y));
                                }
                            }
                        }
                    }
                } else {
                    for (int y = startY; y < stopY; y++) {
                        byte* ptr = basePtr + y * stride + startX * pixelSize;
                        if (pixelSize == 1) {
                            for (int x = startX; x < stopX; x++, ptr++) {
                                if (*ptr != 0) {
                                    pixels.Add(new IntPoint(x, y));
                                }
                            }
                        } else {
                            for (int x = startX; x < stopX; x++, ptr += pixelSize) {
                                if ((ptr[RGB.R] != 0) || (ptr[RGB.G] != 0) || (ptr[RGB.B] != 0)) {
                                    pixels.Add(new IntPoint(x, y));
                                }
                            }
                        }
                    }
                }
            }
            return pixels;
        }
        public void SetPixels(List<IntPoint> coordinates, Color color) {
            unsafe {
                int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;
                byte* basePtr = (byte*)imageData.ToPointer();
                byte red = color.R;
                byte green = color.G;
                byte blue = color.B;
                byte alpha = color.A;
                switch (pixelFormat) {
                    case PixelFormat.Format8bppIndexed: {
                        byte grayValue = (byte)(0.2125 * red + 0.7154 * green + 0.0721 * blue);
                        foreach (IntPoint point in coordinates) {
                            if ((point.X >= 0) && (point.Y >= 0) && (point.X < width) && (point.Y < height)) {
                                byte* ptr = basePtr + point.Y * stride + point.X;
                                *ptr = grayValue;
                            }
                        }
                    }
                    break;
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb: {
                        foreach (IntPoint point in coordinates) {
                            if ((point.X >= 0) && (point.Y >= 0) && (point.X < width) && (point.Y < height)) {
                                byte* ptr = basePtr + point.Y * stride + point.X * pixelSize;
                                ptr[RGB.R] = red;
                                ptr[RGB.G] = green;
                                ptr[RGB.B] = blue;
                            }
                        }
                    }
                    break;
                    case PixelFormat.Format32bppArgb: {
                        foreach (IntPoint point in coordinates) {
                            if ((point.X >= 0) && (point.Y >= 0) && (point.X < width) && (point.Y < height)) {
                                byte* ptr = basePtr + point.Y * stride + point.X * pixelSize;
                                ptr[RGB.R] = red;
                                ptr[RGB.G] = green;
                                ptr[RGB.B] = blue;
                                ptr[RGB.A] = alpha;
                            }
                        }
                    }
                    break;
                    case PixelFormat.Format16bppGrayScale: {
                        ushort grayValue = (ushort)((ushort)(0.2125 * red + 0.7154 * green + 0.0721 * blue) << 8);
                        foreach (IntPoint point in coordinates) {
                            if ((point.X >= 0) && (point.Y >= 0) && (point.X < width) && (point.Y < height)) {
                                ushort* ptr = (ushort*)(basePtr + point.Y * stride) + point.X;
                                *ptr = grayValue;
                            }
                        }
                    }
                    break;
                    case PixelFormat.Format48bppRgb: {
                        ushort red16 = (ushort)(red << 8);
                        ushort green16 = (ushort)(green << 8);
                        ushort blue16 = (ushort)(blue << 8);
                        foreach (IntPoint point in coordinates) {
                            if ((point.X >= 0) && (point.Y >= 0) && (point.X < width) && (point.Y < height)) {
                                ushort* ptr = (ushort*)(basePtr + point.Y * stride + point.X * pixelSize);
                                ptr[RGB.R] = red16;
                                ptr[RGB.G] = green16;
                                ptr[RGB.B] = blue16;
                            }
                        }
                    }
                    break;
                    case PixelFormat.Format64bppArgb: {
                        ushort red16 = (ushort)(red << 8);
                        ushort green16 = (ushort)(green << 8);
                        ushort blue16 = (ushort)(blue << 8);
                        ushort alpha16 = (ushort)(alpha << 8);
                        foreach (IntPoint point in coordinates) {
                            if ((point.X >= 0) && (point.Y >= 0) && (point.X < width) && (point.Y < height)) {
                                ushort* ptr = (ushort*)(basePtr + point.Y * stride + point.X * pixelSize);
                                ptr[RGB.R] = red16;
                                ptr[RGB.G] = green16;
                                ptr[RGB.B] = blue16;
                                ptr[RGB.A] = alpha16;
                            }
                        }
                    }
                    break;
                    default:
                        throw new UnsupportedImageFormatException("The pixel format is not supported: " + pixelFormat);
                }
            }
        }
        public void SetPixel(IntPoint point, Color color) {
            SetPixel(point.X, point.Y, color);
        }
        public void SetPixel(int x, int y, Color color) {
            SetPixel(x, y, color.R, color.G, color.B, color.A);
        }
        public void SetPixel(int x, int y, byte value) {
            SetPixel(x, y, value, value, value, 255);
        }
        private void SetPixel(int x, int y, byte r, byte g, byte b, byte a) {
            if ((x >= 0) && (y >= 0) && (x < width) && (y < height)) {
                unsafe {
                    int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;
                    byte* ptr = (byte*)imageData.ToPointer() + y * stride + x * pixelSize;
                    ushort* ptr2 = (ushort*)ptr;
                    switch (pixelFormat) {
                        case PixelFormat.Format8bppIndexed:
                            *ptr = (byte)(0.2125 * r + 0.7154 * g + 0.0721 * b);
                            break;
                        case PixelFormat.Format24bppRgb:
                        case PixelFormat.Format32bppRgb:
                            ptr[RGB.R] = r;
                            ptr[RGB.G] = g;
                            ptr[RGB.B] = b;
                            break;
                        case PixelFormat.Format32bppArgb:
                            ptr[RGB.R] = r;
                            ptr[RGB.G] = g;
                            ptr[RGB.B] = b;
                            ptr[RGB.A] = a;
                            break;
                        case PixelFormat.Format16bppGrayScale:
                            *ptr2 = (ushort)((ushort)(0.2125 * r + 0.7154 * g + 0.0721 * b) << 8);
                            break;
                        case PixelFormat.Format48bppRgb:
                            ptr2[RGB.R] = (ushort)(r << 8);
                            ptr2[RGB.G] = (ushort)(g << 8);
                            ptr2[RGB.B] = (ushort)(b << 8);
                            break;
                        case PixelFormat.Format64bppArgb:
                            ptr2[RGB.R] = (ushort)(r << 8);
                            ptr2[RGB.G] = (ushort)(g << 8);
                            ptr2[RGB.B] = (ushort)(b << 8);
                            ptr2[RGB.A] = (ushort)(a << 8);
                            break;
                        default:
                            throw new UnsupportedImageFormatException("The pixel format is not supported: " + pixelFormat);
                    }
                }
            }
        }
        public Color GetPixel(IntPoint point) {
            return GetPixel(point.X, point.Y);
        }
        public Color GetPixel(int x, int y) {
            if ((x < 0) || (y < 0)) {
                throw new ArgumentOutOfRangeException("x", "The specified pixel coordinate is out of image's bounds.");
            }
            if ((x >= width) || (y >= height)) {
                throw new ArgumentOutOfRangeException("y", "The specified pixel coordinate is out of image's bounds.");
            }
            Color color = new Color();
            unsafe {
                int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;
                byte* ptr = (byte*)imageData.ToPointer() + y * stride + x * pixelSize;
                switch (pixelFormat) {
                    case PixelFormat.Format8bppIndexed:
                        color = Color.FromArgb(*ptr, *ptr, *ptr);
                        break;
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb:
                        color = Color.FromArgb(ptr[RGB.R], ptr[RGB.G], ptr[RGB.B]);
                        break;
                    case PixelFormat.Format32bppArgb:
                        color = Color.FromArgb(ptr[RGB.A], ptr[RGB.R], ptr[RGB.G], ptr[RGB.B]);
                        break;
                    default:
                        throw new UnsupportedImageFormatException("The pixel format is not supported: " + pixelFormat);
                }
            }
            return color;
        }
        public ushort[] Collect16bppPixelValues(List<IntPoint> points) {
            int pixelSize = Bitmap.GetPixelFormatSize(pixelFormat) / 8;
            if ((pixelFormat == PixelFormat.Format8bppIndexed) || (pixelSize == 3) || (pixelSize == 4)) {
                throw new UnsupportedImageFormatException("Unsupported pixel format of the source image. Use Collect8bppPixelValues() method for it.");
            }
            ushort[] pixelValues = new ushort[points.Count * ((pixelFormat == PixelFormat.Format16bppGrayScale) ? 1 : 3)];
            unsafe {
                byte* basePtr = (byte*)imageData.ToPointer();
                ushort* ptr;
                if (pixelFormat == PixelFormat.Format16bppGrayScale) {
                    int i = 0;
                    foreach (IntPoint point in points) {
                        ptr = (ushort*)(basePtr + stride * point.Y + point.X * pixelSize);
                        pixelValues[i++] = *ptr;
                    }
                } else {
                    int i = 0;
                    foreach (IntPoint point in points) {
                        ptr = (ushort*)(basePtr + stride * point.Y + point.X * pixelSize);
                        pixelValues[i++] = ptr[RGB.R];
                        pixelValues[i++] = ptr[RGB.G];
                        pixelValues[i++] = ptr[RGB.B];
                    }
                }
            }
            return pixelValues;
        }
    }
    [Serializable]
    public struct Point {
        public float X;
        public float Y;
        public Point(float x, float y) {
            this.X = x;
            this.Y = y;
        }
        public float DistanceTo(Point anotherPoint) {
            float dx = X - anotherPoint.X;
            float dy = Y - anotherPoint.Y;
            return (float)System.Math.Sqrt(dx * dx + dy * dy);
        }
        public float SquaredDistanceTo(Point anotherPoint) {
            float dx = X - anotherPoint.X;
            float dy = Y - anotherPoint.Y;
            return dx * dx + dy * dy;
        }
        public static Point operator +(Point point1, Point point2) {
            return new Point(point1.X + point2.X, point1.Y + point2.Y);
        }
        public static Point Add(Point point1, Point point2) {
            return new Point(point1.X + point2.X, point1.Y + point2.Y);
        }
        public static Point operator -(Point point1, Point point2) {
            return new Point(point1.X - point2.X, point1.Y - point2.Y);
        }
        public static Point Subtract(Point point1, Point point2) {
            return new Point(point1.X - point2.X, point1.Y - point2.Y);
        }
        public static Point operator +(Point point, float valueToAdd) {
            return new Point(point.X + valueToAdd, point.Y + valueToAdd);
        }
        public static Point Add(Point point, float valueToAdd) {
            return new Point(point.X + valueToAdd, point.Y + valueToAdd);
        }
        public static Point operator -(Point point, float valueToSubtract) {
            return new Point(point.X - valueToSubtract, point.Y - valueToSubtract);
        }
        public static Point Subtract(Point point, float valueToSubtract) {
            return new Point(point.X - valueToSubtract, point.Y - valueToSubtract);
        }
        public static Point operator *(Point point, float factor) {
            return new Point(point.X * factor, point.Y * factor);
        }
        public static Point Multiply(Point point, float factor) {
            return new Point(point.X * factor, point.Y * factor);
        }
        public static Point operator /(Point point, float factor) {
            return new Point(point.X / factor, point.Y / factor);
        }
        public static Point Divide(Point point, float factor) {
            return new Point(point.X / factor, point.Y / factor);
        }
        public static bool operator ==(Point point1, Point point2) {
            return ((point1.X == point2.X) && (point1.Y == point2.Y));
        }
        public static bool operator !=(Point point1, Point point2) {
            return ((point1.X != point2.X) || (point1.Y != point2.Y));
        }
        public override bool Equals(object obj) {
            return (obj is Point) ? (this == (Point)obj) : false;
        }
        public override int GetHashCode() {
            return X.GetHashCode() + Y.GetHashCode();
        }
        public static explicit operator IntPoint(Point point) {
            return new IntPoint((int)point.X, (int)point.Y);
        }
        public IntPoint Round() {
            return new IntPoint((int)Math.Round(X), (int)Math.Round(Y));
        }
        public override string ToString() {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, {1}", X, Y);
        }
        public float EuclideanNorm() {
            return (float)System.Math.Sqrt(X * X + Y * Y);
        }
    }
    public class Blob {
        private UnmanagedImage image;
        private bool originalSize = false;
        private Rectangle rect;
        private int id;
        private int area;
        private Point cog;
        private double fullness;
        private Color colorMean = Color.Black;
        private Color colorStdDev = Color.Black;
        [Browsable(false)]
        public UnmanagedImage Image {
            get { return image; }
            internal set { image = value; }
        }
        [Browsable(false)]
        public bool OriginalSize {
            get { return originalSize; }
            internal set { originalSize = value; }
        }
        public Rectangle Rectangle {
            get { return rect; }
        }
        [Browsable(false)]
        public int ID {
            get { return id; }
            internal set { id = value; }
        }
        public int Area {
            get { return area; }
            internal set { area = value; }
        }
        public double Fullness {
            get { return fullness; }
            internal set { fullness = value; }
        }
        public Point CenterOfGravity {
            get { return cog; }
            internal set { cog = value; }
        }
        public Color ColorMean {
            get { return colorMean; }
            internal set { colorMean = value; }
        }
        public Color ColorStdDev {
            get { return colorStdDev; }
            internal set { colorStdDev = value; }
        }
        public Blob(int id, Rectangle rect) {
            this.id = id;
            this.rect = rect;
        }
        public Blob(Blob source) {
            id = source.id;
            rect = source.rect;
            cog = source.cog;
            area = source.area;
            fullness = source.fullness;
            colorMean = source.colorMean;
            colorStdDev = source.colorStdDev;
        }
    }
    public interface IBlobsFilter {
        bool Check(Blob blob);
    }
    public abstract class BlobCounterBase {
        List<Blob> blobs = new List<Blob>();
        private ObjectsOrder objectsOrder = ObjectsOrder.None;
        private bool filterBlobs = false;
        private IBlobsFilter filter = null;
        private bool coupledSizeFiltering = false;
        private int minWidth = 1;
        private int minHeight = 1;
        private int maxWidth = int.MaxValue;
        private int maxHeight = int.MaxValue;
        protected int objectsCount;
        protected int[] objectLabels;
        protected int imageWidth;
        protected int imageHeight;
        public int ObjectsCount {
            get { return objectsCount; }
        }
        public int[] ObjectLabels {
            get { return objectLabels; }
        }
        public ObjectsOrder ObjectsOrder {
            get { return objectsOrder; }
            set { objectsOrder = value; }
        }
        public bool FilterBlobs {
            get { return filterBlobs; }
            set { filterBlobs = value; }
        }
        public bool CoupledSizeFiltering {
            get { return coupledSizeFiltering; }
            set { coupledSizeFiltering = value; }
        }
        public int MinWidth {
            get { return minWidth; }
            set { minWidth = value; }
        }
        public int MinHeight {
            get { return minHeight; }
            set { minHeight = value; }
        }
        public int MaxWidth {
            get { return maxWidth; }
            set { maxWidth = value; }
        }
        public int MaxHeight {
            get { return maxHeight; }
            set { maxHeight = value; }
        }
        public IBlobsFilter BlobsFilter {
            get { return filter; }
            set { filter = value; }
        }
        public BlobCounterBase() { }
        public BlobCounterBase(Bitmap image) {
            ProcessImage(image);
        }
        public BlobCounterBase(BitmapData imageData) {
            ProcessImage(imageData);
        }
        public BlobCounterBase(UnmanagedImage image) {
            ProcessImage(image);
        }
        public void ProcessImage(Bitmap image) {
            BitmapData imageData = image.LockBits(
    new Rectangle(0, 0, image.Width, image.Height),
    ImageLockMode.ReadOnly, image.PixelFormat);
            try {
                ProcessImage(imageData);
            } finally {
                image.UnlockBits(imageData);
            }
        }
        public void ProcessImage(BitmapData imageData) {
            ProcessImage(new UnmanagedImage(imageData));
        }
        public void ProcessImage(UnmanagedImage image) {
            imageWidth = image.Width;
            imageHeight = image.Height;
            BuildObjectsMap(image);
            CollectObjectsInfo(image);
            if (filterBlobs) {
                int[] labelsMap = new int[objectsCount + 1];
                for (int i = 1; i <= objectsCount; i++) {
                    labelsMap[i] = i;
                }
                int objectsToRemove = 0;
                if (filter == null) {
                    for (int i = objectsCount - 1; i >= 0; i--) {
                        int blobWidth = blobs[i].Rectangle.Width;
                        int blobHeight = blobs[i].Rectangle.Height;
                        if (coupledSizeFiltering == false) {
                            if (
    (blobWidth < minWidth) || (blobHeight < minHeight) ||
    (blobWidth > maxWidth) || (blobHeight > maxHeight)) {
                                labelsMap[i + 1] = 0;
                                objectsToRemove++;
                                blobs.RemoveAt(i);
                            }
                        } else {
                            if (
    ((blobWidth < minWidth) && (blobHeight < minHeight)) ||
    ((blobWidth > maxWidth) && (blobHeight > maxHeight))) {
                                labelsMap[i + 1] = 0;
                                objectsToRemove++;
                                blobs.RemoveAt(i);
                            }
                        }
                    }
                } else {
                    for (int i = objectsCount - 1; i >= 0; i--) {
                        if (!filter.Check(blobs[i])) {
                            labelsMap[i + 1] = 0;
                            objectsToRemove++;
                            blobs.RemoveAt(i);
                        }
                    }
                }
                int label = 0;
                for (int i = 1; i <= objectsCount; i++) {
                    if (labelsMap[i] != 0) {
                        label++;
                        labelsMap[i] = label;
                    }
                }
                for (int i = 0, n = objectLabels.Length; i < n; i++) {
                    objectLabels[i] = labelsMap[objectLabels[i]];
                }
                objectsCount -= objectsToRemove;
                for (int i = 0, n = blobs.Count; i < n; i++) {
                    blobs[i].ID = i + 1;
                }
            }
            if (objectsOrder != ObjectsOrder.None) {
                blobs.Sort(new BlobsSorter(objectsOrder));
            }
        }
        public Rectangle[] GetObjectsRectangles() {
            if (objectLabels == null)
                throw new ApplicationException("Image should be processed before to collect objects map.");
            Rectangle[] rects = new Rectangle[objectsCount];
            for (int i = 0; i < objectsCount; i++) {
                rects[i] = blobs[i].Rectangle;
            }
            return rects;
        }
        public Blob[] GetObjectsInformation() {
            if (objectLabels == null)
                throw new ApplicationException("Image should be processed before to collect objects map.");
            Blob[] blobsToReturn = new Blob[objectsCount];
            for (int k = 0; k < objectsCount; k++) {
                blobsToReturn[k] = new Blob(blobs[k]);
            }
            return blobsToReturn;
        }
        public Blob[] GetObjects(Bitmap image, bool extractInOriginalSize) {
            Blob[] blobs = null;
            BitmapData imageData = image.LockBits(
    new Rectangle(0, 0, image.Width, image.Height),
    ImageLockMode.ReadOnly, image.PixelFormat);
            try {
                blobs = GetObjects(new UnmanagedImage(imageData), extractInOriginalSize);
            } finally {
                image.UnlockBits(imageData);
            }
            return blobs;
        }
        public Blob[] GetObjects(UnmanagedImage image, bool extractInOriginalSize) {
            if (objectLabels == null)
                throw new ApplicationException("Image should be processed before to collect objects map.");
            if (
                (image.PixelFormat != PixelFormat.Format24bppRgb) &&
                (image.PixelFormat != PixelFormat.Format8bppIndexed) &&
                (image.PixelFormat != PixelFormat.Format32bppRgb) &&
                (image.PixelFormat != PixelFormat.Format32bppArgb) &&
                (image.PixelFormat != PixelFormat.Format32bppRgb) &&
                (image.PixelFormat != PixelFormat.Format32bppPArgb)
                )
                throw new UnsupportedImageFormatException("Unsupported pixel format of the provided image.");
            int width = image.Width;
            int height = image.Height;
            int srcStride = image.Stride;
            int pixelSize = Bitmap.GetPixelFormatSize(image.PixelFormat) / 8;
            Blob[] objects = new Blob[objectsCount];
            for (int k = 0; k < objectsCount; k++) {
                int objectWidth = blobs[k].Rectangle.Width;
                int objectHeight = blobs[k].Rectangle.Height;
                int blobImageWidth = (extractInOriginalSize) ? width : objectWidth;
                int blobImageHeight = (extractInOriginalSize) ? height : objectHeight;
                int xmin = blobs[k].Rectangle.X;
                int xmax = xmin + objectWidth - 1;
                int ymin = blobs[k].Rectangle.Y;
                int ymax = ymin + objectHeight - 1;
                int label = blobs[k].ID;
                UnmanagedImage dstImage = UnmanagedImage.Create(blobImageWidth, blobImageHeight, image.PixelFormat);
                unsafe {
                    byte* src = (byte*)image.ImageData.ToPointer() + ymin * srcStride + xmin * pixelSize;
                    byte* dst = (byte*)dstImage.ImageData.ToPointer();
                    int p = ymin * width + xmin;
                    if (extractInOriginalSize) {
                        dst += ymin * dstImage.Stride + xmin * pixelSize;
                    }
                    int srcOffset = srcStride - objectWidth * pixelSize;
                    int dstOffset = dstImage.Stride - objectWidth * pixelSize;
                    int labelsOffset = width - objectWidth;
                    for (int y = ymin; y <= ymax; y++) {
                        for (int x = xmin; x <= xmax; x++, p++, dst += pixelSize, src += pixelSize) {
                            if (objectLabels[p] == label) {
                                *dst = *src;
                                if (pixelSize > 1) {
                                    dst[1] = src[1];
                                    dst[2] = src[2];
                                    if (pixelSize > 3) {
                                        dst[3] = src[3];
                                    }
                                }
                            }
                        }
                        src += srcOffset;
                        dst += dstOffset;
                        p += labelsOffset;
                    }
                }
                objects[k] = new Blob(blobs[k]);
                objects[k].Image = dstImage;
                objects[k].OriginalSize = extractInOriginalSize;
            }
            return objects;
        }
        public void ExtractBlobsImage(Bitmap image, Blob blob, bool extractInOriginalSize) {
            BitmapData imageData = image.LockBits(
    new Rectangle(0, 0, image.Width, image.Height),
    ImageLockMode.ReadOnly, image.PixelFormat);
            try {
                ExtractBlobsImage(new UnmanagedImage(imageData), blob, extractInOriginalSize);
            } finally {
                image.UnlockBits(imageData);
            }
        }
        public void ExtractBlobsImage(UnmanagedImage image, Blob blob, bool extractInOriginalSize) {
            if (objectLabels == null)
                throw new ApplicationException("Image should be processed before to collect objects map.");
            if (
                (image.PixelFormat != PixelFormat.Format24bppRgb) &&
                (image.PixelFormat != PixelFormat.Format8bppIndexed) &&
                (image.PixelFormat != PixelFormat.Format32bppRgb) &&
                (image.PixelFormat != PixelFormat.Format32bppArgb) &&
                (image.PixelFormat != PixelFormat.Format32bppRgb) &&
                (image.PixelFormat != PixelFormat.Format32bppPArgb)
                )
                throw new UnsupportedImageFormatException("Unsupported pixel format of the provided image.");
            int width = image.Width;
            int height = image.Height;
            int srcStride = image.Stride;
            int pixelSize = Bitmap.GetPixelFormatSize(image.PixelFormat) / 8;
            int objectWidth = blob.Rectangle.Width;
            int objectHeight = blob.Rectangle.Height;
            int blobImageWidth = (extractInOriginalSize) ? width : objectWidth;
            int blobImageHeight = (extractInOriginalSize) ? height : objectHeight;
            int xmin = blob.Rectangle.Left;
            int xmax = xmin + objectWidth - 1;
            int ymin = blob.Rectangle.Top;
            int ymax = ymin + objectHeight - 1;
            int label = blob.ID;
            blob.Image = UnmanagedImage.Create(blobImageWidth, blobImageHeight, image.PixelFormat);
            blob.OriginalSize = extractInOriginalSize;
            unsafe {
                byte* src = (byte*)image.ImageData.ToPointer() + ymin * srcStride + xmin * pixelSize;
                byte* dst = (byte*)blob.Image.ImageData.ToPointer();
                int p = ymin * width + xmin;
                if (extractInOriginalSize) {
                    dst += ymin * blob.Image.Stride + xmin * pixelSize;
                }
                int srcOffset = srcStride - objectWidth * pixelSize;
                int dstOffset = blob.Image.Stride - objectWidth * pixelSize;
                int labelsOffset = width - objectWidth;
                for (int y = ymin; y <= ymax; y++) {
                    for (int x = xmin; x <= xmax; x++, p++, dst += pixelSize, src += pixelSize) {
                        if (objectLabels[p] == label) {
                            *dst = *src;
                            if (pixelSize > 1) {
                                dst[1] = src[1];
                                dst[2] = src[2];
                                if (pixelSize > 3) {
                                    dst[3] = src[3];
                                }
                            }
                        }
                    }
                    src += srcOffset;
                    dst += dstOffset;
                    p += labelsOffset;
                }
            }
        }
        public void GetBlobsLeftAndRightEdges(Blob blob, out List<IntPoint> leftEdge, out List<IntPoint> rightEdge) {
            if (objectLabels == null)
                throw new ApplicationException("Image should be processed before to collect objects map.");
            leftEdge = new List<IntPoint>();
            rightEdge = new List<IntPoint>();
            int xmin = blob.Rectangle.Left;
            int xmax = xmin + blob.Rectangle.Width - 1;
            int ymin = blob.Rectangle.Top;
            int ymax = ymin + blob.Rectangle.Height - 1;
            int label = blob.ID;
            for (int y = ymin; y <= ymax; y++) {
                int p = y * imageWidth + xmin;
                for (int x = xmin; x <= xmax; x++, p++) {
                    if (objectLabels[p] == label) {
                        leftEdge.Add(new IntPoint(x, y));
                        break;
                    }
                }
                p = y * imageWidth + xmax;
                for (int x = xmax; x >= xmin; x--, p--) {
                    if (objectLabels[p] == label) {
                        rightEdge.Add(new IntPoint(x, y));
                        break;
                    }
                }
            }
        }
        public void GetBlobsTopAndBottomEdges(Blob blob, out List<IntPoint> topEdge, out List<IntPoint> bottomEdge) {
            if (objectLabels == null)
                throw new ApplicationException("Image should be processed before to collect objects map.");
            topEdge = new List<IntPoint>();
            bottomEdge = new List<IntPoint>();
            int xmin = blob.Rectangle.Left;
            int xmax = xmin + blob.Rectangle.Width - 1;
            int ymin = blob.Rectangle.Top;
            int ymax = ymin + blob.Rectangle.Height - 1;
            int label = blob.ID;
            for (int x = xmin; x <= xmax; x++) {
                int p = ymin * imageWidth + x;
                for (int y = ymin; y <= ymax; y++, p += imageWidth) {
                    if (objectLabels[p] == label) {
                        topEdge.Add(new IntPoint(x, y));
                        break;
                    }
                }
                p = ymax * imageWidth + x;
                for (int y = ymax; y >= ymin; y--, p -= imageWidth) {
                    if (objectLabels[p] == label) {
                        bottomEdge.Add(new IntPoint(x, y));
                        break;
                    }
                }
            }
        }
        public List<IntPoint> GetBlobsEdgePoints(Blob blob) {
            if (objectLabels == null)
                throw new ApplicationException("Image should be processed before to collect objects map.");
            List<IntPoint> edgePoints = new List<IntPoint>();
            int xmin = blob.Rectangle.Left;
            int xmax = xmin + blob.Rectangle.Width - 1;
            int ymin = blob.Rectangle.Top;
            int ymax = ymin + blob.Rectangle.Height - 1;
            int label = blob.ID;
            int[] leftProcessedPoints = new int[blob.Rectangle.Height];
            int[] rightProcessedPoints = new int[blob.Rectangle.Height];
            for (int y = ymin; y <= ymax; y++) {
                int p = y * imageWidth + xmin;
                for (int x = xmin; x <= xmax; x++, p++) {
                    if (objectLabels[p] == label) {
                        edgePoints.Add(new IntPoint(x, y));
                        leftProcessedPoints[y - ymin] = x;
                        break;
                    }
                }
                p = y * imageWidth + xmax;
                for (int x = xmax; x >= xmin; x--, p--) {
                    if (objectLabels[p] == label) {
                        if (leftProcessedPoints[y - ymin] != x) {
                            edgePoints.Add(new IntPoint(x, y));
                        }
                        rightProcessedPoints[y - ymin] = x;
                        break;
                    }
                }
            }
            for (int x = xmin; x <= xmax; x++) {
                int p = ymin * imageWidth + x;
                for (int y = ymin, y0 = 0; y <= ymax; y++, y0++, p += imageWidth) {
                    if (objectLabels[p] == label) {
                        if ((leftProcessedPoints[y0] != x) &&
     (rightProcessedPoints[y0] != x)) {
                            edgePoints.Add(new IntPoint(x, y));
                        }
                        break;
                    }
                }
                p = ymax * imageWidth + x;
                for (int y = ymax, y0 = ymax - ymin; y >= ymin; y--, y0--, p -= imageWidth) {
                    if (objectLabels[p] == label) {
                        if ((leftProcessedPoints[y0] != x) &&
     (rightProcessedPoints[y0] != x)) {
                            edgePoints.Add(new IntPoint(x, y));
                        }
                        break;
                    }
                }
            }
            return edgePoints;
        }
        protected abstract void BuildObjectsMap(UnmanagedImage image);
        #region Private Methods - Collecting objects' rectangles
        private unsafe void CollectObjectsInfo(UnmanagedImage image) {
            int i = 0, label;
            int[] x1 = new int[objectsCount + 1];
            int[] y1 = new int[objectsCount + 1];
            int[] x2 = new int[objectsCount + 1];
            int[] y2 = new int[objectsCount + 1];
            int[] area = new int[objectsCount + 1];
            long[] xc = new long[objectsCount + 1];
            long[] yc = new long[objectsCount + 1];
            long[] meanR = new long[objectsCount + 1];
            long[] meanG = new long[objectsCount + 1];
            long[] meanB = new long[objectsCount + 1];
            long[] stdDevR = new long[objectsCount + 1];
            long[] stdDevG = new long[objectsCount + 1];
            long[] stdDevB = new long[objectsCount + 1];
            for (int j = 1; j <= objectsCount; j++) {
                x1[j] = imageWidth;
                y1[j] = imageHeight;
            }
            byte* src = (byte*)image.ImageData.ToPointer();
            if (image.PixelFormat == PixelFormat.Format8bppIndexed) {
                int offset = image.Stride - imageWidth;
                byte g;
                for (int y = 0; y < imageHeight; y++) {
                    for (int x = 0; x < imageWidth; x++, i++, src++) {
                        label = objectLabels[i];
                        if (label == 0)
                            continue;
                        if (x < x1[label]) {
                            x1[label] = x;
                        }
                        if (x > x2[label]) {
                            x2[label] = x;
                        }
                        if (y < y1[label]) {
                            y1[label] = y;
                        }
                        if (y > y2[label]) {
                            y2[label] = y;
                        }
                        area[label]++;
                        xc[label] += x;
                        yc[label] += y;
                        g = *src;
                        meanG[label] += g;
                        stdDevG[label] += g * g;
                    }
                    src += offset;
                }
                for (int j = 1; j <= objectsCount; j++) {
                    meanR[j] = meanB[j] = meanG[j];
                    stdDevR[j] = stdDevB[j] = stdDevG[j];
                }
            } else {
                int pixelSize = Bitmap.GetPixelFormatSize(image.PixelFormat) / 8;
                int offset = image.Stride - imageWidth * pixelSize;
                byte r, g, b;
                for (int y = 0; y < imageHeight; y++) {
                    for (int x = 0; x < imageWidth; x++, i++, src += pixelSize) {
                        label = objectLabels[i];
                        if (label == 0)
                            continue;
                        if (x < x1[label]) {
                            x1[label] = x;
                        }
                        if (x > x2[label]) {
                            x2[label] = x;
                        }
                        if (y < y1[label]) {
                            y1[label] = y;
                        }
                        if (y > y2[label]) {
                            y2[label] = y;
                        }
                        area[label]++;
                        xc[label] += x;
                        yc[label] += y;
                        r = src[RGB.R];
                        g = src[RGB.G];
                        b = src[RGB.B];
                        meanR[label] += r;
                        meanG[label] += g;
                        meanB[label] += b;
                        stdDevR[label] += r * r;
                        stdDevG[label] += g * g;
                        stdDevB[label] += b * b;
                    }
                    src += offset;
                }
            }
            blobs.Clear();
            for (int j = 1; j <= objectsCount; j++) {
                int blobArea = area[j];
                Blob blob = new Blob(j, new Rectangle(x1[j], y1[j], x2[j] - x1[j] + 1, y2[j] - y1[j] + 1));
                blob.Area = blobArea;
                blob.Fullness = (double)blobArea / ((x2[j] - x1[j] + 1) * (y2[j] - y1[j] + 1));
                blob.CenterOfGravity = new Point((float)xc[j] / blobArea, (float)yc[j] / blobArea);
                blob.ColorMean = Color.FromArgb((byte)(meanR[j] / blobArea), (byte)(meanG[j] / blobArea), (byte)(meanB[j] / blobArea));
                blob.ColorStdDev = Color.FromArgb(
                    (byte)(Math.Sqrt(stdDevR[j] / blobArea - blob.ColorMean.R * blob.ColorMean.R)),
                    (byte)(Math.Sqrt(stdDevG[j] / blobArea - blob.ColorMean.G * blob.ColorMean.G)),
                    (byte)(Math.Sqrt(stdDevB[j] / blobArea - blob.ColorMean.B * blob.ColorMean.B)));
                blobs.Add(blob);
            }
        }
        private class BlobsSorter : System.Collections.Generic.IComparer<Blob> {
            private ObjectsOrder order;
            public BlobsSorter(ObjectsOrder order) {
                this.order = order;
            }
            public int Compare(Blob a, Blob b) {
                Rectangle aRect = a.Rectangle;
                Rectangle bRect = b.Rectangle;
                switch (order) {
                    case ObjectsOrder.Size:
                        return (bRect.Width * bRect.Height - aRect.Width * aRect.Height);
                    case ObjectsOrder.Area: return b.Area - a.Area;
                    case ObjectsOrder.YX:
                        return ((aRect.Y * 100000 + aRect.X) - (bRect.Y * 100000 + bRect.X));
                    case ObjectsOrder.XY:
                        return ((aRect.X * 100000 + aRect.Y) - (bRect.X * 100000 + bRect.Y));
                }
                return 0;
            }
        }
        #endregion
    }
    public class BlobCounter : BlobCounterBase {
        private byte backgroundThresholdR = 0;
        private byte backgroundThresholdG = 0;
        private byte backgroundThresholdB = 0;
        public Color BackgroundThreshold {
            get { return Color.FromArgb(backgroundThresholdR, backgroundThresholdG, backgroundThresholdB); }
            set {
                backgroundThresholdR = value.R;
                backgroundThresholdG = value.G;
                backgroundThresholdB = value.B;
            }
        }
        public BlobCounter() { }
        public BlobCounter(Bitmap image) : base(image) { }
        public BlobCounter(BitmapData imageData) : base(imageData) { }
        public BlobCounter(UnmanagedImage image) : base(image) { }
        protected override void BuildObjectsMap(UnmanagedImage image) {
            int stride = image.Stride;
            if ((image.PixelFormat != PixelFormat.Format8bppIndexed) &&
     (image.PixelFormat != PixelFormat.Format24bppRgb) &&
     (image.PixelFormat != PixelFormat.Format32bppRgb) &&
     (image.PixelFormat != PixelFormat.Format32bppArgb) &&
     (image.PixelFormat != PixelFormat.Format32bppPArgb)) {
                throw new UnsupportedImageFormatException("Unsupported pixel format of the source image.");
            }
            if (imageWidth == 1) {
                throw new InvalidImagePropertiesException("BlobCounter cannot process images that are one pixel wide. Rotate the image or use RecursiveBlobCounter.");
            }
            int imageWidthM1 = imageWidth - 1;
            objectLabels = new int[imageWidth * imageHeight];
            int labelsCount = 0;
            int maxObjects = ((imageWidth / 2) + 1) * ((imageHeight / 2) + 1) + 1;
            int[] map = new int[maxObjects];
            for (int i = 0; i < maxObjects; i++) {
                map[i] = i;
            }
            unsafe {
                byte* src = (byte*)image.ImageData.ToPointer();
                int p = 0;
                if (image.PixelFormat == PixelFormat.Format8bppIndexed) {
                    int offset = stride - imageWidth;
                    if (*src > backgroundThresholdG) {
                        objectLabels[p] = ++labelsCount;
                    }
                    ++src;
                    ++p;
                    for (int x = 1; x < imageWidth; x++, src++, p++) {
                        if (*src > backgroundThresholdG) {
                            if (src[-1] > backgroundThresholdG) {
                                objectLabels[p] = objectLabels[p - 1];
                            } else {
                                objectLabels[p] = ++labelsCount;
                            }
                        }
                    }
                    src += offset;
                    for (int y = 1; y < imageHeight; y++) {
                        if (*src > backgroundThresholdG) {
                            if (src[-stride] > backgroundThresholdG) {
                                objectLabels[p] = objectLabels[p - imageWidth];
                            } else if (src[1 - stride] > backgroundThresholdG) {
                                objectLabels[p] = objectLabels[p + 1 - imageWidth];
                            } else {
                                objectLabels[p] = ++labelsCount;
                            }
                        }
                        ++src;
                        ++p;
                        for (int x = 1; x < imageWidthM1; x++, src++, p++) {
                            if (*src > backgroundThresholdG) {
                                if (src[-1] > backgroundThresholdG) {
                                    objectLabels[p] = objectLabels[p - 1];
                                } else if (src[-1 - stride] > backgroundThresholdG) {
                                    objectLabels[p] = objectLabels[p - 1 - imageWidth];
                                } else if (src[-stride] > backgroundThresholdG) {
                                    objectLabels[p] = objectLabels[p - imageWidth];
                                }
                                if (src[1 - stride] > backgroundThresholdG) {
                                    if (objectLabels[p] == 0) {
                                        objectLabels[p] = objectLabels[p + 1 - imageWidth];
                                    } else {
                                        int l1 = objectLabels[p];
                                        int l2 = objectLabels[p + 1 - imageWidth];
                                        if ((l1 != l2) && (map[l1] != map[l2])) {
                                            if (map[l1] == l1) {
                                                map[l1] = map[l2];
                                            } else if (map[l2] == l2) {
                                                map[l2] = map[l1];
                                            } else {
                                                map[map[l1]] = map[l2];
                                                map[l1] = map[l2];
                                            }
                                            for (int i = 1; i <= labelsCount; i++) {
                                                if (map[i] != i) {
                                                    int j = map[i];
                                                    while (j != map[j]) {
                                                        j = map[j];
                                                    }
                                                    map[i] = j;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (objectLabels[p] == 0) {
                                    objectLabels[p] = ++labelsCount;
                                }
                            }
                        }
                        if (*src > backgroundThresholdG) {
                            if (src[-1] > backgroundThresholdG) {
                                objectLabels[p] = objectLabels[p - 1];
                            } else if (src[-1 - stride] > backgroundThresholdG) {
                                objectLabels[p] = objectLabels[p - 1 - imageWidth];
                            } else if (src[-stride] > backgroundThresholdG) {
                                objectLabels[p] = objectLabels[p - imageWidth];
                            } else {
                                objectLabels[p] = ++labelsCount;
                            }
                        }
                        ++src;
                        ++p;
                        src += offset;
                    }
                } else {
                    int pixelSize = Bitmap.GetPixelFormatSize(image.PixelFormat) / 8;
                    int offset = stride - imageWidth * pixelSize;
                    int strideM1 = stride - pixelSize;
                    int strideP1 = stride + pixelSize;
                    if ((src[RGB.R] | src[RGB.G] | src[RGB.B]) != 0) {
                        objectLabels[p] = ++labelsCount;
                    }
                    src += pixelSize;
                    ++p;
                    for (int x = 1; x < imageWidth; x++, src += pixelSize, p++) {
                        if ((src[RGB.R] > backgroundThresholdR) ||
     (src[RGB.G] > backgroundThresholdG) ||
     (src[RGB.B] > backgroundThresholdB)) {
                            if ((src[RGB.R - pixelSize] > backgroundThresholdR) ||
     (src[RGB.G - pixelSize] > backgroundThresholdG) ||
     (src[RGB.B - pixelSize] > backgroundThresholdB)) {
                                objectLabels[p] = objectLabels[p - 1];
                            } else {
                                objectLabels[p] = ++labelsCount;
                            }
                        }
                    }
                    src += offset;
                    for (int y = 1; y < imageHeight; y++) {
                        if ((src[RGB.R] > backgroundThresholdR) ||
(src[RGB.G] > backgroundThresholdG) ||
(src[RGB.B] > backgroundThresholdB)) {
                            if ((src[RGB.R - stride] > backgroundThresholdR) ||
     (src[RGB.G - stride] > backgroundThresholdG) ||
     (src[RGB.B - stride] > backgroundThresholdB)) {
                                objectLabels[p] = objectLabels[p - imageWidth];
                            } else if ((src[RGB.R - strideM1] > backgroundThresholdR) ||
                                        (src[RGB.G - strideM1] > backgroundThresholdG) ||
                                        (src[RGB.B - strideM1] > backgroundThresholdB)) {
                                objectLabels[p] = objectLabels[p + 1 - imageWidth];
                            } else {
                                objectLabels[p] = ++labelsCount;
                            }
                        }
                        src += pixelSize;
                        ++p;
                        for (int x = 1; x < imageWidth - 1; x++, src += pixelSize, p++) {
                            if ((src[RGB.R] > backgroundThresholdR) ||
                                 (src[RGB.G] > backgroundThresholdG) ||
                                 (src[RGB.B] > backgroundThresholdB)) {
                                if ((src[RGB.R - pixelSize] > backgroundThresholdR) ||
     (src[RGB.G - pixelSize] > backgroundThresholdG) ||
     (src[RGB.B - pixelSize] > backgroundThresholdB)) {
                                    objectLabels[p] = objectLabels[p - 1];
                                } else if ((src[RGB.R - strideP1] > backgroundThresholdR) ||
                                            (src[RGB.G - strideP1] > backgroundThresholdG) ||
                                            (src[RGB.B - strideP1] > backgroundThresholdB)) {
                                    objectLabels[p] = objectLabels[p - 1 - imageWidth];
                                } else if ((src[RGB.R - stride] > backgroundThresholdR) ||
                                            (src[RGB.G - stride] > backgroundThresholdG) ||
                                            (src[RGB.B - stride] > backgroundThresholdB)) {
                                    objectLabels[p] = objectLabels[p - imageWidth];
                                }
                                if ((src[RGB.R - strideM1] > backgroundThresholdR) ||
                                     (src[RGB.G - strideM1] > backgroundThresholdG) ||
                                     (src[RGB.B - strideM1] > backgroundThresholdB)) {
                                    if (objectLabels[p] == 0) {
                                        objectLabels[p] = objectLabels[p + 1 - imageWidth];
                                    } else {
                                        int l1 = objectLabels[p];
                                        int l2 = objectLabels[p + 1 - imageWidth];
                                        if ((l1 != l2) && (map[l1] != map[l2])) {
                                            if (map[l1] == l1) {
                                                map[l1] = map[l2];
                                            } else if (map[l2] == l2) {
                                                map[l2] = map[l1];
                                            } else {
                                                map[map[l1]] = map[l2];
                                                map[l1] = map[l2];
                                            }
                                            for (int i = 1; i <= labelsCount; i++) {
                                                if (map[i] != i) {
                                                    int j = map[i];
                                                    while (j != map[j]) {
                                                        j = map[j];
                                                    }
                                                    map[i] = j;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (objectLabels[p] == 0) {
                                    objectLabels[p] = ++labelsCount;
                                }
                            }
                        }
                        if ((src[RGB.R] > backgroundThresholdR) ||
(src[RGB.G] > backgroundThresholdG) ||
(src[RGB.B] > backgroundThresholdB)) {
                            if ((src[RGB.R - pixelSize] > backgroundThresholdR) ||
     (src[RGB.G - pixelSize] > backgroundThresholdG) ||
     (src[RGB.B - pixelSize] > backgroundThresholdB)) {
                                objectLabels[p] = objectLabels[p - 1];
                            } else if ((src[RGB.R - strideP1] > backgroundThresholdR) ||
                                        (src[RGB.G - strideP1] > backgroundThresholdG) ||
                                        (src[RGB.B - strideP1] > backgroundThresholdB)) {
                                objectLabels[p] = objectLabels[p - 1 - imageWidth];
                            } else if ((src[RGB.R - stride] > backgroundThresholdR) ||
                                        (src[RGB.G - stride] > backgroundThresholdG) ||
                                        (src[RGB.B - stride] > backgroundThresholdB)) {
                                objectLabels[p] = objectLabels[p - imageWidth];
                            } else {
                                objectLabels[p] = ++labelsCount;
                            }
                        }
                        src += pixelSize;
                        ++p;
                        src += offset;
                    }
                }
            }
            int[] reMap = new int[map.Length];
            objectsCount = 0;
            for (int i = 1; i <= labelsCount; i++) {
                if (map[i] == i) {
                    reMap[i] = ++objectsCount;
                }
            }
            for (int i = 1; i <= labelsCount; i++) {
                if (map[i] != i) {
                    reMap[i] = reMap[map[i]];
                }
            }
            for (int i = 0, n = objectLabels.Length; i < n; i++) {
                objectLabels[i] = reMap[objectLabels[i]];
            }
        }
    }
    public interface IFilter {
        Bitmap Apply(Bitmap image);
        Bitmap Apply(BitmapData imageData);
        UnmanagedImage Apply(UnmanagedImage image);
        void Apply(UnmanagedImage sourceImage, UnmanagedImage destinationImage);
    }
    public interface IInPlaceFilter {
        void ApplyInPlace(Bitmap image);
        void ApplyInPlace(BitmapData imageData);
        void ApplyInPlace(UnmanagedImage image);
    }
    public interface IInPlacePartialFilter {
        void ApplyInPlace(Bitmap image, Rectangle rect);
        void ApplyInPlace(BitmapData imageData, Rectangle rect);
        void ApplyInPlace(UnmanagedImage image, Rectangle rect);
    }
    public interface IFilterInformation {
        Dictionary<PixelFormat, PixelFormat> FormatTranslations { get; }
    }
    public class Grayscale : BaseFilter {
        public static class CommonAlgorithms {
            public static readonly Grayscale BT709 = new Grayscale(0.2125, 0.7154, 0.0721);
            public static readonly Grayscale RMY = new Grayscale(0.5000, 0.4190, 0.0810);
            public static readonly Grayscale Y = new Grayscale(0.2990, 0.5870, 0.1140);
        }
        public readonly double RedCoefficient;
        public readonly double GreenCoefficient;
        public readonly double BlueCoefficient;
        private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();
        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations {
            get { return formatTranslations; }
        }
        public Grayscale(double cr, double cg, double cb) {
            RedCoefficient = cr;
            GreenCoefficient = cg;
            BlueCoefficient = cb;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format32bppRgb] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format32bppArgb] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format48bppRgb] = PixelFormat.Format16bppGrayScale;
            formatTranslations[PixelFormat.Format64bppArgb] = PixelFormat.Format16bppGrayScale;
        }
        protected override unsafe void ProcessFilter(UnmanagedImage sourceData, UnmanagedImage destinationData) {
            int width = sourceData.Width;
            int height = sourceData.Height;
            PixelFormat srcPixelFormat = sourceData.PixelFormat;
            if (
                (srcPixelFormat == PixelFormat.Format24bppRgb) ||
                (srcPixelFormat == PixelFormat.Format32bppRgb) ||
                (srcPixelFormat == PixelFormat.Format32bppArgb)) {
                int pixelSize = (srcPixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                int srcOffset = sourceData.Stride - width * pixelSize;
                int dstOffset = destinationData.Stride - width;
                int rc = (int)(0x10000 * RedCoefficient);
                int gc = (int)(0x10000 * GreenCoefficient);
                int bc = (int)(0x10000 * BlueCoefficient);
                byte* src = (byte*)sourceData.ImageData.ToPointer();
                byte* dst = (byte*)destinationData.ImageData.ToPointer();
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++, src += pixelSize, dst++) {
                        *dst = (byte)((rc * src[RGB.R] + gc * src[RGB.G] + bc * src[RGB.B]) >> 16);
                    }
                    src += srcOffset;
                    dst += dstOffset;
                }
            } else {
                int pixelSize = (srcPixelFormat == PixelFormat.Format48bppRgb) ? 3 : 4;
                byte* srcBase = (byte*)sourceData.ImageData.ToPointer();
                byte* dstBase = (byte*)destinationData.ImageData.ToPointer();
                int srcStride = sourceData.Stride;
                int dstStride = destinationData.Stride;
                for (int y = 0; y < height; y++) {
                    ushort* src = (ushort*)(srcBase + y * srcStride);
                    ushort* dst = (ushort*)(dstBase + y * dstStride);
                    for (int x = 0; x < width; x++, src += pixelSize, dst++) {
                        *dst = (ushort)(RedCoefficient * src[RGB.R] + GreenCoefficient * src[RGB.G] + BlueCoefficient * src[RGB.B]);
                    }
                }
            }
        }
    }
    public class Threshold : BaseInPlacePartialFilter {
        protected int threshold = 128;
        private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();
        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations {
            get { return formatTranslations; }
        }
        public int ThresholdValue {
            get { return threshold; }
            set { threshold = value; }
        }
        public Threshold() {
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format16bppGrayScale] = PixelFormat.Format16bppGrayScale;
        }
        public Threshold(int threshold)
            : this() {
            this.threshold = threshold;
        }
        protected override unsafe void ProcessFilter(UnmanagedImage image, Rectangle rect) {
            int startX = rect.Left;
            int startY = rect.Top;
            int stopX = startX + rect.Width;
            int stopY = startY + rect.Height;
            if (image.PixelFormat == PixelFormat.Format8bppIndexed) {
                int offset = image.Stride - rect.Width;
                byte* ptr = (byte*)image.ImageData.ToPointer();
                ptr += (startY * image.Stride + startX);
                for (int y = startY; y < stopY; y++) {
                    for (int x = startX; x < stopX; x++, ptr++) {
                        *ptr = (byte)((*ptr >= threshold) ? 255 : 0);
                    }
                    ptr += offset;
                }
            } else {
                byte* basePtr = (byte*)image.ImageData.ToPointer() + startX * 2;
                int stride = image.Stride;
                for (int y = startY; y < stopY; y++) {
                    ushort* ptr = (ushort*)(basePtr + stride * y);
                    for (int x = startX; x < stopX; x++, ptr++) {
                        *ptr = (ushort)((*ptr >= threshold) ? 65535 : 0);
                    }
                }
            }
        }
    }
    public abstract class BaseInPlacePartialFilter : IFilter, IInPlaceFilter, IInPlacePartialFilter, IFilterInformation {
        public abstract Dictionary<PixelFormat, PixelFormat> FormatTranslations { get; }
        public Bitmap Apply(Bitmap image) {
            BitmapData srcData = image.LockBits(
    new Rectangle(0, 0, image.Width, image.Height),
    ImageLockMode.ReadOnly, image.PixelFormat);
            Bitmap dstImage = null;
            try {
                dstImage = Apply(srcData);
                if ((image.HorizontalResolution > 0) && (image.VerticalResolution > 0)) {
                    dstImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                }
            } finally {
                image.UnlockBits(srcData);
            }
            return dstImage;
        }
        public Bitmap Apply(BitmapData imageData) {
            PixelFormat dstPixelFormat = imageData.PixelFormat;
            CheckSourceFormat(dstPixelFormat);
            int width = imageData.Width;
            int height = imageData.Height;
            Bitmap dstImage = (dstPixelFormat == PixelFormat.Format8bppIndexed) ?
    Imaging.CreateGrayscaleImage(width, height) :
    new Bitmap(width, height, dstPixelFormat);
            BitmapData dstData = dstImage.LockBits(
    new Rectangle(0, 0, width, height),
    ImageLockMode.ReadWrite, dstPixelFormat);
            SystemTools.CopyUnmanagedMemory(dstData.Scan0, imageData.Scan0, imageData.Stride * height);
            try {
                ProcessFilter(new UnmanagedImage(dstData), new Rectangle(0, 0, width, height));
            } finally {
                dstImage.UnlockBits(dstData);
            }
            return dstImage;
        }
        public UnmanagedImage Apply(UnmanagedImage image) {
            CheckSourceFormat(image.PixelFormat);
            UnmanagedImage dstImage = UnmanagedImage.Create(image.Width, image.Height, image.PixelFormat);
            Apply(image, dstImage);
            return dstImage;
        }
        public void Apply(UnmanagedImage sourceImage, UnmanagedImage destinationImage) {
            CheckSourceFormat(sourceImage.PixelFormat);
            if (destinationImage.PixelFormat != sourceImage.PixelFormat) {
                throw new InvalidImagePropertiesException("Destination pixel format must be the same as pixel format of source image.");
            }
            if ((destinationImage.Width != sourceImage.Width) || (destinationImage.Height != sourceImage.Height)) {
                throw new InvalidImagePropertiesException("Destination image must have the same width and height as source image.");
            }
            int dstStride = destinationImage.Stride;
            int srcStride = sourceImage.Stride;
            int lineSize = Math.Min(srcStride, dstStride);
            unsafe {
                byte* dst = (byte*)destinationImage.ImageData.ToPointer();
                byte* src = (byte*)sourceImage.ImageData.ToPointer();
                for (int y = 0, height = sourceImage.Height; y < height; y++) {
                    SystemTools.CopyUnmanagedMemory(dst, src, lineSize);
                    dst += dstStride;
                    src += srcStride;
                }
            }
            ProcessFilter(destinationImage, new Rectangle(0, 0, destinationImage.Width, destinationImage.Height));
        }
        public void ApplyInPlace(Bitmap image) {
            ApplyInPlace(image, new Rectangle(0, 0, image.Width, image.Height));
        }
        public void ApplyInPlace(BitmapData imageData) {
            CheckSourceFormat(imageData.PixelFormat);
            ProcessFilter(new UnmanagedImage(imageData), new Rectangle(0, 0, imageData.Width, imageData.Height));
        }
        public void ApplyInPlace(UnmanagedImage image) {
            CheckSourceFormat(image.PixelFormat);
            ProcessFilter(image, new Rectangle(0, 0, image.Width, image.Height));
        }
        public void ApplyInPlace(Bitmap image, Rectangle rect) {
            BitmapData data = image.LockBits(
    new Rectangle(0, 0, image.Width, image.Height),
    ImageLockMode.ReadWrite, image.PixelFormat);
            try {
                ApplyInPlace(new UnmanagedImage(data), rect);
            } finally {
                image.UnlockBits(data);
            }
        }
        public void ApplyInPlace(BitmapData imageData, Rectangle rect) {
            ApplyInPlace(new UnmanagedImage(imageData), rect);
        }
        public void ApplyInPlace(UnmanagedImage image, Rectangle rect) {
            CheckSourceFormat(image.PixelFormat);
            rect.Intersect(new Rectangle(0, 0, image.Width, image.Height));
            if ((rect.Width | rect.Height) != 0)
                ProcessFilter(image, rect);
        }
        protected abstract unsafe void ProcessFilter(UnmanagedImage image, Rectangle rect);
        private void CheckSourceFormat(PixelFormat pixelFormat) {
            if (!FormatTranslations.ContainsKey(pixelFormat))
                throw new UnsupportedImageFormatException("Source pixel format is not supported by the filter.");
        }
    }
    public abstract class BaseFilter : IFilter, IFilterInformation {
        public abstract Dictionary<PixelFormat, PixelFormat> FormatTranslations { get; }
        public Bitmap Apply(Bitmap image) {
            BitmapData srcData = image.LockBits(
    new Rectangle(0, 0, image.Width, image.Height),
    ImageLockMode.ReadOnly, image.PixelFormat);
            Bitmap dstImage = null;
            try {
                dstImage = Apply(srcData);
                if ((image.HorizontalResolution > 0) && (image.VerticalResolution > 0)) {
                    dstImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                }
            } finally {
                image.UnlockBits(srcData);
            }
            return dstImage;
        }
        public Bitmap Apply(BitmapData imageData) {
            CheckSourceFormat(imageData.PixelFormat);
            int width = imageData.Width;
            int height = imageData.Height;
            PixelFormat dstPixelFormat = FormatTranslations[imageData.PixelFormat];
            Bitmap dstImage = (dstPixelFormat == PixelFormat.Format8bppIndexed) ?
    Imaging.CreateGrayscaleImage(width, height) :
    new Bitmap(width, height, dstPixelFormat);
            BitmapData dstData = dstImage.LockBits(
    new Rectangle(0, 0, width, height),
    ImageLockMode.ReadWrite, dstPixelFormat);
            try {
                ProcessFilter(new UnmanagedImage(imageData), new UnmanagedImage(dstData));
            } finally {
                dstImage.UnlockBits(dstData);
            }
            return dstImage;
        }
        public UnmanagedImage Apply(UnmanagedImage image) {
            CheckSourceFormat(image.PixelFormat);
            UnmanagedImage dstImage = UnmanagedImage.Create(image.Width, image.Height, FormatTranslations[image.PixelFormat]);
            ProcessFilter(image, dstImage);
            return dstImage;
        }
        public void Apply(UnmanagedImage sourceImage, UnmanagedImage destinationImage) {
            CheckSourceFormat(sourceImage.PixelFormat);
            if (destinationImage.PixelFormat != FormatTranslations[sourceImage.PixelFormat]) {
                throw new InvalidImagePropertiesException("Destination pixel format is specified incorrectly.");
            }
            if ((destinationImage.Width != sourceImage.Width) || (destinationImage.Height != sourceImage.Height)) {
                throw new InvalidImagePropertiesException("Destination image must have the same width and height as source image.");
            }
            ProcessFilter(sourceImage, destinationImage);
        }
        protected abstract unsafe void ProcessFilter(UnmanagedImage sourceData, UnmanagedImage destinationData);
        private void CheckSourceFormat(PixelFormat pixelFormat) {
            if (!FormatTranslations.ContainsKey(pixelFormat))
                throw new UnsupportedImageFormatException("Source pixel format is not supported by the filter.");
        }
    }
    public abstract class BaseInPlaceFilter : IFilter, IInPlaceFilter, IFilterInformation {
        public abstract Dictionary<PixelFormat, PixelFormat> FormatTranslations { get; }
        public Bitmap Apply(Bitmap image) {
            BitmapData srcData = image.LockBits(
    new Rectangle(0, 0, image.Width, image.Height),
    ImageLockMode.ReadOnly, image.PixelFormat);
            Bitmap dstImage = null;
            try {
                dstImage = Apply(srcData);
                if ((image.HorizontalResolution > 0) && (image.VerticalResolution > 0)) {
                    dstImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                }
            } finally {
                image.UnlockBits(srcData);
            }
            return dstImage;
        }
        public Bitmap Apply(BitmapData imageData) {
            PixelFormat dstPixelFormat = imageData.PixelFormat;
            CheckSourceFormat(dstPixelFormat);
            int width = imageData.Width;
            int height = imageData.Height;
            Bitmap dstImage = (dstPixelFormat == PixelFormat.Format8bppIndexed) ?
    Imaging.CreateGrayscaleImage(width, height) :
    new Bitmap(width, height, dstPixelFormat);
            BitmapData dstData = dstImage.LockBits(
    new Rectangle(0, 0, width, height),
    ImageLockMode.ReadWrite, dstPixelFormat);
            SystemTools.CopyUnmanagedMemory(dstData.Scan0, imageData.Scan0, imageData.Stride * height);
            try {
                ProcessFilter(new UnmanagedImage(dstData));
            } finally {
                dstImage.UnlockBits(dstData);
            }
            return dstImage;
        }
        public UnmanagedImage Apply(UnmanagedImage image) {
            CheckSourceFormat(image.PixelFormat);
            UnmanagedImage dstImage = UnmanagedImage.Create(image.Width, image.Height, image.PixelFormat);
            Apply(image, dstImage);
            return dstImage;
        }
        public void Apply(UnmanagedImage sourceImage, UnmanagedImage destinationImage) {
            CheckSourceFormat(sourceImage.PixelFormat);
            if (destinationImage.PixelFormat != sourceImage.PixelFormat) {
                throw new InvalidImagePropertiesException("Destination pixel format must be the same as pixel format of source image.");
            }
            if ((destinationImage.Width != sourceImage.Width) || (destinationImage.Height != sourceImage.Height)) {
                throw new InvalidImagePropertiesException("Destination image must have the same width and height as source image.");
            }
            int dstStride = destinationImage.Stride;
            int srcStride = sourceImage.Stride;
            int lineSize = Math.Min(srcStride, dstStride);
            unsafe {
                byte* dst = (byte*)destinationImage.ImageData.ToPointer();
                byte* src = (byte*)sourceImage.ImageData.ToPointer();
                for (int y = 0, height = sourceImage.Height; y < height; y++) {
                    SystemTools.CopyUnmanagedMemory(dst, src, lineSize);
                    dst += dstStride;
                    src += srcStride;
                }
            }
            ProcessFilter(destinationImage);
        }
        public void ApplyInPlace(Bitmap image) {
            CheckSourceFormat(image.PixelFormat);
            BitmapData data = image.LockBits(
    new Rectangle(0, 0, image.Width, image.Height),
    ImageLockMode.ReadWrite, image.PixelFormat);
            try {
                ProcessFilter(new UnmanagedImage(data));
            } finally {
                image.UnlockBits(data);
            }
        }
        public void ApplyInPlace(BitmapData imageData) {
            CheckSourceFormat(imageData.PixelFormat);
            ProcessFilter(new UnmanagedImage(imageData));
        }
        public void ApplyInPlace(UnmanagedImage image) {
            CheckSourceFormat(image.PixelFormat);
            ProcessFilter(image);
        }
        protected abstract unsafe void ProcessFilter(UnmanagedImage image);
        private void CheckSourceFormat(PixelFormat pixelFormat) {
            if (!FormatTranslations.ContainsKey(pixelFormat))
                throw new UnsupportedImageFormatException("Source pixel format is not supported by the filter.");
        }
    }
    public abstract class BaseTransformationFilter : IFilter, IFilterInformation {
        public abstract Dictionary<PixelFormat, PixelFormat> FormatTranslations { get; }
        public Bitmap Apply(Bitmap image) {
            BitmapData srcData = image.LockBits(
    new Rectangle(0, 0, image.Width, image.Height),
    ImageLockMode.ReadOnly, image.PixelFormat);
            Bitmap dstImage = null;
            try {
                dstImage = Apply(srcData);
                if ((image.HorizontalResolution > 0) && (image.VerticalResolution > 0)) {
                    dstImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                }
            } finally {
                image.UnlockBits(srcData);
            }
            return dstImage;
        }
        public Bitmap Apply(BitmapData imageData) {
            CheckSourceFormat(imageData.PixelFormat);
            PixelFormat dstPixelFormat = FormatTranslations[imageData.PixelFormat];
            Size newSize = CalculateNewImageSize(new UnmanagedImage(imageData));
            Bitmap dstImage = (dstPixelFormat == PixelFormat.Format8bppIndexed) ?
   Imaging.CreateGrayscaleImage(newSize.Width, newSize.Height) :
    new Bitmap(newSize.Width, newSize.Height, dstPixelFormat);
            BitmapData dstData = dstImage.LockBits(
    new Rectangle(0, 0, newSize.Width, newSize.Height),
    ImageLockMode.ReadWrite, dstPixelFormat);
            try {
                ProcessFilter(new UnmanagedImage(imageData), new UnmanagedImage(dstData));
            } finally {
                dstImage.UnlockBits(dstData);
            }
            return dstImage;
        }
        public UnmanagedImage Apply(UnmanagedImage image) {
            CheckSourceFormat(image.PixelFormat);
            Size newSize = CalculateNewImageSize(image);
            UnmanagedImage dstImage = UnmanagedImage.Create(newSize.Width, newSize.Height, FormatTranslations[image.PixelFormat]);
            ProcessFilter(image, dstImage);
            return dstImage;
        }
        public void Apply(UnmanagedImage sourceImage, UnmanagedImage destinationImage) {
            CheckSourceFormat(sourceImage.PixelFormat);
            if (destinationImage.PixelFormat != FormatTranslations[sourceImage.PixelFormat]) {
                throw new InvalidImagePropertiesException("Destination pixel format is specified incorrectly.");
            }
            Size newSize = CalculateNewImageSize(sourceImage);
            if ((destinationImage.Width != newSize.Width) || (destinationImage.Height != newSize.Height)) {
                throw new InvalidImagePropertiesException("Destination image must have the size expected by the filter.");
            }
            ProcessFilter(sourceImage, destinationImage);
        }
        protected abstract System.Drawing.Size CalculateNewImageSize(UnmanagedImage sourceData);
        protected abstract unsafe void ProcessFilter(UnmanagedImage sourceData, UnmanagedImage destinationData);
        private void CheckSourceFormat(PixelFormat pixelFormat) {
            if (!FormatTranslations.ContainsKey(pixelFormat))
                throw new UnsupportedImageFormatException("Source pixel format is not supported by the filter.");
        }
    }
    public class BlobsFiltering : BaseInPlaceFilter {
        private BlobCounter blobCounter = new BlobCounter();
        private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();
        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations {
            get { return formatTranslations; }
        }
        public bool CoupledSizeFiltering {
            get { return blobCounter.CoupledSizeFiltering; }
            set { blobCounter.CoupledSizeFiltering = value; }
        }
        public int MinWidth {
            get { return blobCounter.MinWidth; }
            set { blobCounter.MinWidth = value; }
        }
        public int MinHeight {
            get { return blobCounter.MinHeight; }
            set { blobCounter.MinHeight = value; }
        }
        public int MaxWidth {
            get { return blobCounter.MaxWidth; }
            set { blobCounter.MaxWidth = value; }
        }
        public int MaxHeight {
            get { return blobCounter.MaxHeight; }
            set { blobCounter.MaxHeight = value; }
        }
        public IBlobsFilter BlobsFilter {
            get { return blobCounter.BlobsFilter; }
            set { blobCounter.BlobsFilter = value; }
        }
        public BlobsFiltering() {
            blobCounter.FilterBlobs = true;
            blobCounter.MinWidth = 1;
            blobCounter.MinHeight = 1;
            blobCounter.MaxWidth = int.MaxValue;
            blobCounter.MaxHeight = int.MaxValue;
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format24bppRgb;
            formatTranslations[PixelFormat.Format32bppArgb] = PixelFormat.Format32bppArgb;
            formatTranslations[PixelFormat.Format32bppPArgb] = PixelFormat.Format32bppPArgb;
        }
        public BlobsFiltering(int minWidth, int minHeight, int maxWidth, int maxHeight)
            : this(minWidth, minHeight, maxWidth, maxHeight, false) { }
        public BlobsFiltering(int minWidth, int minHeight, int maxWidth, int maxHeight, bool coupledSizeFiltering)
            : this() {
            blobCounter.MinWidth = minWidth;
            blobCounter.MinHeight = minHeight;
            blobCounter.MaxWidth = maxWidth;
            blobCounter.MaxHeight = maxHeight;
            blobCounter.CoupledSizeFiltering = coupledSizeFiltering;
        }
        public BlobsFiltering(IBlobsFilter blobsFilter) : this() {
            blobCounter.BlobsFilter = blobsFilter;
        }
        protected override unsafe void ProcessFilter(UnmanagedImage image) {
            blobCounter.ProcessImage(image);
            int[] objectsMap = blobCounter.ObjectLabels;
            int width = image.Width;
            int height = image.Height;
            byte* ptr = (byte*)image.ImageData.ToPointer();
            if (image.PixelFormat == PixelFormat.Format8bppIndexed) {
                int offset = image.Stride - width;
                for (int y = 0, p = 0; y < height; y++) {
                    for (int x = 0; x < width; x++, ptr++, p++) {
                        if (objectsMap[p] == 0) {
                            *ptr = 0;
                        }
                    }
                    ptr += offset;
                }
            } else {
                int pixelSize = Bitmap.GetPixelFormatSize(image.PixelFormat) / 8;
                int offset = image.Stride - width * pixelSize;
                for (int y = 0, p = 0; y < height; y++) {
                    for (int x = 0; x < width; x++, ptr += pixelSize, p++) {
                        if (objectsMap[p] == 0) {
                            ptr[RGB.R] = ptr[RGB.G] = ptr[RGB.B] = 0;
                        }
                    }
                    ptr += offset;
                }
            }
        }
    }
    public static class SystemTools {
#if !MONO
        [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern byte* memcpy(
    byte* dst,
    byte* src,
    int count);
        [DllImport("ntdll.dll", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern byte* memset(
    byte* dst,
    int filler,
    int count);
#endif
        public static IntPtr CopyUnmanagedMemory(IntPtr dst, IntPtr src, int count) {
            unsafe {
                CopyUnmanagedMemory((byte*)dst.ToPointer(), (byte*)src.ToPointer(), count);
            }
            return dst;
        }
        public static unsafe byte* CopyUnmanagedMemory(byte* dst, byte* src, int count) {
#if !MONO
            return memcpy(dst, src, count);
#else
            int countUint = count >> 2;
            int countByte = count & 3;
            uint* dstUint = (uint*) dst;
            uint* srcUint = (uint*) src;
            while ( countUint-- != 0 )
            {
                *dstUint++ = *srcUint++;
            }
            byte* dstByte = (byte*) dstUint;
            byte* srcByte = (byte*) srcUint;
            while ( countByte-- != 0 )
            {
                *dstByte++ = *srcByte++;
            }
            return dst;
#endif
        }
        public static IntPtr SetUnmanagedMemory(IntPtr dst, int filler, int count) {
            unsafe {
                SetUnmanagedMemory((byte*)dst.ToPointer(), filler, count);
            }
            return dst;
        }
        public static unsafe byte* SetUnmanagedMemory(byte* dst, int filler, int count) {
#if !MONO
            return memset(dst, filler, count);
#else
            int countUint = count >> 2;
            int countByte = count & 3;
            byte fillerByte = (byte) filler;
            uint fiilerUint = (uint) filler | ( (uint) filler << 8 ) |
                                              ( (uint) filler << 16 );                                              
            uint* dstUint = (uint*) dst;
            while ( countUint-- != 0 )
            {
                *dstUint++ = fiilerUint;
            }
            byte* dstByte = (byte*) dstUint;
            while ( countByte-- != 0 )
            {
                *dstByte++ = fillerByte;
            }
            return dst;
#endif
        }
    }
    public class Crop : BaseTransformationFilter {
        private Rectangle rect;
        private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();
        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations {
            get { return formatTranslations; }
        }
        public Rectangle Rectangle {
            get { return rect; }
            set { rect = value; }
        }
        public Crop(Rectangle rect) {
            this.rect = rect;
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format24bppRgb;
            formatTranslations[PixelFormat.Format32bppRgb] = PixelFormat.Format32bppRgb;
            formatTranslations[PixelFormat.Format32bppArgb] = PixelFormat.Format32bppArgb;
            formatTranslations[PixelFormat.Format16bppGrayScale] = PixelFormat.Format16bppGrayScale;
            formatTranslations[PixelFormat.Format48bppRgb] = PixelFormat.Format48bppRgb;
            formatTranslations[PixelFormat.Format64bppArgb] = PixelFormat.Format64bppArgb;
        }
        protected override System.Drawing.Size CalculateNewImageSize(UnmanagedImage sourceData) {
            return new Size(rect.Width, rect.Height);
        }
        protected override unsafe void ProcessFilter(UnmanagedImage sourceData, UnmanagedImage destinationData) {
            Rectangle srcRect = rect;
            srcRect.Intersect(new Rectangle(0, 0, sourceData.Width, sourceData.Height));
            int xmin = srcRect.Left;
            int ymin = srcRect.Top;
            int ymax = srcRect.Bottom - 1;
            int copyWidth = srcRect.Width;
            int srcStride = sourceData.Stride;
            int dstStride = destinationData.Stride;
            int pixelSize = System.Drawing.Image.GetPixelFormatSize(sourceData.PixelFormat) / 8;
            int copySize = copyWidth * pixelSize;
            byte* src = (byte*)sourceData.ImageData.ToPointer() + ymin * srcStride + xmin * pixelSize;
            byte* dst = (byte*)destinationData.ImageData.ToPointer();
            if (rect.Top < 0) {
                dst -= dstStride * rect.Top;
            }
            if (rect.Left < 0) {
                dst -= pixelSize * rect.Left;
            }
            for (int y = ymin; y <= ymax; y++) {
                SystemTools.CopyUnmanagedMemory(dst, src, copySize);
                src += srcStride;
                dst += dstStride;
            }
        }
    }
    public sealed class Invert : BaseInPlacePartialFilter {
        private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();
        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations {
            get { return formatTranslations; }
        }
        public Invert() {
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format24bppRgb;
            formatTranslations[PixelFormat.Format16bppGrayScale] = PixelFormat.Format16bppGrayScale;
            formatTranslations[PixelFormat.Format48bppRgb] = PixelFormat.Format48bppRgb;
        }
        protected override unsafe void ProcessFilter(UnmanagedImage image, Rectangle rect) {
            int pixelSize = ((image.PixelFormat == PixelFormat.Format8bppIndexed) ||
                              (image.PixelFormat == PixelFormat.Format16bppGrayScale)) ? 1 : 3;
            int startY = rect.Top;
            int stopY = startY + rect.Height;
            int startX = rect.Left * pixelSize;
            int stopX = startX + rect.Width * pixelSize;
            byte* basePtr = (byte*)image.ImageData.ToPointer();
            if (
                (image.PixelFormat == PixelFormat.Format8bppIndexed) ||
                (image.PixelFormat == PixelFormat.Format24bppRgb)) {
                int offset = image.Stride - (stopX - startX);
                byte* ptr = basePtr + (startY * image.Stride + rect.Left * pixelSize);
                for (int y = startY; y < stopY; y++) {
                    for (int x = startX; x < stopX; x++, ptr++) {
                        *ptr = (byte)(255 - *ptr);
                    }
                    ptr += offset;
                }
            } else {
                int stride = image.Stride;
                basePtr += (startY * image.Stride + rect.Left * pixelSize * 2);
                for (int y = startY; y < stopY; y++) {
                    ushort* ptr = (ushort*)(basePtr);
                    for (int x = startX; x < stopX; x++, ptr++) {
                        *ptr = (ushort)(65535 - *ptr);
                    }
                    basePtr += stride;
                }
            }
        }
    }
    [Serializable]
    public struct IntPoint {
        public int X;
        public int Y;
        public IntPoint(int x, int y) {
            this.X = x;
            this.Y = y;
        }
        public float DistanceTo(IntPoint anotherPoint) {
            int dx = X - anotherPoint.X;
            int dy = Y - anotherPoint.Y;
            return (float)System.Math.Sqrt(dx * dx + dy * dy);
        }
        public float SquaredDistanceTo(Point anotherPoint) {
            float dx = X - anotherPoint.X;
            float dy = Y - anotherPoint.Y;
            return dx * dx + dy * dy;
        }
        public static IntPoint operator +(IntPoint point1, IntPoint point2) {
            return new IntPoint(point1.X + point2.X, point1.Y + point2.Y);
        }
        public static IntPoint Add(IntPoint point1, IntPoint point2) {
            return new IntPoint(point1.X + point2.X, point1.Y + point2.Y);
        }
        public static IntPoint operator -(IntPoint point1, IntPoint point2) {
            return new IntPoint(point1.X - point2.X, point1.Y - point2.Y);
        }
        public static IntPoint Subtract(IntPoint point1, IntPoint point2) {
            return new IntPoint(point1.X - point2.X, point1.Y - point2.Y);
        }
        public static IntPoint operator +(IntPoint point, int valueToAdd) {
            return new IntPoint(point.X + valueToAdd, point.Y + valueToAdd);
        }
        public static IntPoint Add(IntPoint point, int valueToAdd) {
            return new IntPoint(point.X + valueToAdd, point.Y + valueToAdd);
        }
        public static IntPoint operator -(IntPoint point, int valueToSubtract) {
            return new IntPoint(point.X - valueToSubtract, point.Y - valueToSubtract);
        }
        public static IntPoint Subtract(IntPoint point, int valueToSubtract) {
            return new IntPoint(point.X - valueToSubtract, point.Y - valueToSubtract);
        }
        public static IntPoint operator *(IntPoint point, int factor) {
            return new IntPoint(point.X * factor, point.Y * factor);
        }
        public static IntPoint Multiply(IntPoint point, int factor) {
            return new IntPoint(point.X * factor, point.Y * factor);
        }
        public static IntPoint operator /(IntPoint point, int factor) {
            return new IntPoint(point.X / factor, point.Y / factor);
        }
        public static IntPoint Divide(IntPoint point, int factor) {
            return new IntPoint(point.X / factor, point.Y / factor);
        }
        public static bool operator ==(IntPoint point1, IntPoint point2) {
            return ((point1.X == point2.X) && (point1.Y == point2.Y));
        }
        public static bool operator !=(IntPoint point1, IntPoint point2) {
            return ((point1.X != point2.X) || (point1.Y != point2.Y));
        }
        public override bool Equals(object obj) {
            return (obj is IntPoint) ? (this == (IntPoint)obj) : false;
        }
        public override int GetHashCode() {
            return X.GetHashCode() + Y.GetHashCode();
        }
        public static implicit operator Point(IntPoint point) {
            return new Point(point.X, point.Y);
        }
        public override string ToString() {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, {1}", X, Y);
        }
        public float EuclideanNorm() {
            return (float)System.Math.Sqrt(X * X + Y * Y);
        }
    }
    public class CanvasMove : BaseInPlaceFilter {
        private byte fillRed = 255;
        private byte fillGreen = 255;
        private byte fillBlue = 255;
        private byte fillAlpha = 255;
        private byte fillGray = 255;
        private IntPoint movePoint;
        private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();
        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations {
            get { return formatTranslations; }
        }
        public Color FillColorRGB {
            get { return Color.FromArgb(fillAlpha, fillRed, fillGreen, fillBlue); }
            set {
                fillRed = value.R;
                fillGreen = value.G;
                fillBlue = value.B;
                fillAlpha = value.A;
            }
        }
        public byte FillColorGray {
            get { return fillGray; }
            set { fillGray = value; }
        }
        public IntPoint MovePoint {
            get { return movePoint; }
            set { movePoint = value; }
        }
        private CanvasMove() {
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format16bppGrayScale] = PixelFormat.Format16bppGrayScale;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format24bppRgb;
            formatTranslations[PixelFormat.Format32bppArgb] = PixelFormat.Format32bppArgb;
            formatTranslations[PixelFormat.Format32bppRgb] = PixelFormat.Format32bppRgb;
            formatTranslations[PixelFormat.Format48bppRgb] = PixelFormat.Format48bppRgb;
            formatTranslations[PixelFormat.Format64bppArgb] = PixelFormat.Format64bppArgb;
        }
        public CanvasMove(IntPoint movePoint)
            : this() {
            this.movePoint = movePoint;
        }
        public CanvasMove(IntPoint movePoint, Color fillColorRGB)
            : this() {
            this.movePoint = movePoint;
            this.fillRed = fillColorRGB.R;
            this.fillGreen = fillColorRGB.G;
            this.fillBlue = fillColorRGB.B;
            this.fillAlpha = fillColorRGB.A;
        }
        public CanvasMove(IntPoint movePoint, byte fillColorGray)
            : this() {
            this.movePoint = movePoint;
            this.fillGray = fillColorGray;
        }
        public CanvasMove(IntPoint movePoint, Color fillColorRGB, byte fillColorGray)
            : this() {
            this.movePoint = movePoint;
            this.fillRed = fillColorRGB.R;
            this.fillGreen = fillColorRGB.G;
            this.fillBlue = fillColorRGB.B;
            this.fillAlpha = fillColorRGB.A;
            this.fillGray = fillColorGray;
        }
        protected override void ProcessFilter(UnmanagedImage image) {
            int pixelSize = System.Drawing.Image.GetPixelFormatSize(image.PixelFormat) / 8;
            switch (pixelSize) {
                case 1:
                case 3:
                case 4:
                    ProcessFilter8bpc(image);
                    break;
                case 2:
                case 6:
                case 8:
                    ProcessFilter16bpc(image);
                    break;
            }
        }
        private unsafe void ProcessFilter8bpc(UnmanagedImage image) {
            int pixelSize = System.Drawing.Image.GetPixelFormatSize(image.PixelFormat) / 8;
            bool is32bpp = (pixelSize == 4);
            int width = image.Width;
            int height = image.Height;
            int stride = image.Stride;
            int movePointX = movePoint.X;
            int movePointY = movePoint.Y;
            Rectangle intersect = Rectangle.Intersect(
    new Rectangle(0, 0, width, height),
    new Rectangle(movePointX, movePointY, width, height));
            int yStart = 0;
            int yStop = height;
            int yStep = 1;
            int xStart = 0;
            int xStop = width;
            int xStep = 1;
            if (movePointY > 0) {
                yStart = height - 1;
                yStop = -1;
                yStep = -1;
            }
            if (movePointX > 0) {
                xStart = width - 1;
                xStop = -1;
                xStep = -1;
            }
            byte* src = (byte*)image.ImageData.ToPointer();
            byte* pixel, moved;
            if (image.PixelFormat == PixelFormat.Format8bppIndexed) {
                for (int y = yStart; y != yStop; y += yStep) {
                    for (int x = xStart; x != xStop; x += xStep) {
                        pixel = src + y * stride + x;
                        if (intersect.Contains(x, y)) {
                            moved = src + (y - movePointY) * stride + (x - movePointX);
                            *pixel = *moved;
                        } else {
                            *pixel = fillGray;
                        }
                    }
                }
            } else {
                for (int y = yStart; y != yStop; y += yStep) {
                    for (int x = xStart; x != xStop; x += xStep) {
                        pixel = src + y * stride + x * pixelSize;
                        if (intersect.Contains(x, y)) {
                            moved = src + (y - movePointY) * stride + (x - movePointX) * pixelSize;
                            pixel[RGB.R] = moved[RGB.R];
                            pixel[RGB.G] = moved[RGB.G];
                            pixel[RGB.B] = moved[RGB.B];
                            if (is32bpp) {
                                pixel[RGB.A] = moved[RGB.A];
                            }
                        } else {
                            pixel[RGB.R] = fillRed;
                            pixel[RGB.G] = fillGreen;
                            pixel[RGB.B] = fillBlue;
                            if (is32bpp) {
                                pixel[RGB.A] = fillAlpha;
                            }
                        }
                    }
                }
            }
        }
        private unsafe void ProcessFilter16bpc(UnmanagedImage image) {
            int pixelSize = System.Drawing.Image.GetPixelFormatSize(image.PixelFormat) / 8;
            bool is64bpp = (pixelSize == 8);
            ushort fillRed = (ushort)(this.fillRed << 8);
            ushort fillGreen = (ushort)(this.fillGreen << 8);
            ushort fillBlue = (ushort)(this.fillBlue << 8);
            ushort fillAlpha = (ushort)(this.fillAlpha << 8);
            int width = image.Width;
            int height = image.Height;
            int stride = image.Stride;
            int movePointX = movePoint.X;
            int movePointY = movePoint.Y;
            Rectangle intersect = Rectangle.Intersect(
    new Rectangle(0, 0, width, height),
    new Rectangle(movePointX, movePointY, width, height));
            int yStart = 0;
            int yStop = height;
            int yStep = 1;
            int xStart = 0;
            int xStop = width;
            int xStep = 1;
            if (movePointY > 0) {
                yStart = height - 1;
                yStop = -1;
                yStep = -1;
            }
            if (movePointX > 0) {
                xStart = width - 1;
                xStop = -1;
                xStep = -1;
            }
            byte* src = (byte*)image.ImageData.ToPointer();
            ushort* pixel, moved;
            if (image.PixelFormat == PixelFormat.Format16bppGrayScale) {
                for (int y = yStart; y != yStop; y += yStep) {
                    for (int x = xStart; x != xStop; x += xStep) {
                        pixel = (ushort*)(src + y * stride + x * 2);
                        if (intersect.Contains(x, y)) {
                            moved = (ushort*)(src + (y - movePointY) * stride + (x - movePointX) * 2);
                            *pixel = *moved;
                        } else {
                            *pixel = fillGray;
                        }
                    }
                }
            } else {
                for (int y = yStart; y != yStop; y += yStep) {
                    for (int x = xStart; x != xStop; x += xStep) {
                        pixel = (ushort*)(src + y * stride + x * pixelSize);
                        if (intersect.Contains(x, y)) {
                            moved = (ushort*)(src + (y - movePointY) * stride + (x - movePointX) * pixelSize);
                            pixel[RGB.R] = moved[RGB.R];
                            pixel[RGB.G] = moved[RGB.G];
                            pixel[RGB.B] = moved[RGB.B];
                            if (is64bpp) {
                                pixel[RGB.A] = moved[RGB.A];
                            }
                        } else {
                            pixel[RGB.R] = fillRed;
                            pixel[RGB.G] = fillGreen;
                            pixel[RGB.B] = fillBlue;
                            if (is64bpp) {
                                pixel[RGB.A] = fillAlpha;
                            }
                        }
                    }
                }
            }
        }
    }
    public class Imaging {
        public static Bitmap CreateGrayscaleImage(int width, int height) {
            Bitmap image = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            SetGrayscalePalette(image);
            return image;
        }
        public static void SetGrayscalePalette(Bitmap image) {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new UnsupportedImageFormatException("Source image is not 8 bpp image.");
            ColorPalette cp = image.Palette;
            for (int i = 0; i < 256; i++) {
                cp.Entries[i] = Color.FromArgb(i, i, i);
            }
            image.Palette = cp;
        }
    }
    public class TemplateMatch {
        private Rectangle rect;
        private float similarity;

        public Rectangle Rectangle {
            get { return rect; }
        }

        public float Similarity {
            get { return similarity; }
        }

        public TemplateMatch(Rectangle rect, float similarity) {
            this.rect = rect;
            this.similarity = similarity;
        }
    }
    public interface ITemplateMatching {

        TemplateMatch[] ProcessImage(Bitmap image, Bitmap template, Rectangle searchZone);


        TemplateMatch[] ProcessImage(BitmapData imageData, BitmapData templateData, Rectangle searchZone);


        TemplateMatch[] ProcessImage(UnmanagedImage image, UnmanagedImage template, Rectangle searchZone);
    }
    public class ExhaustiveTemplateMatching : ITemplateMatching {
        private float similarityThreshold = 0.9f;


        public float SimilarityThreshold {
            get { return similarityThreshold; }
            set { similarityThreshold = Math.Min(1, Math.Max(0, value)); }
        }


        public ExhaustiveTemplateMatching() { }


        public ExhaustiveTemplateMatching(float similarityThreshold) {
            this.similarityThreshold = similarityThreshold;
        }

        public TemplateMatch[] ProcessImage(Bitmap image, Bitmap template) {
            return ProcessImage(image, template, new Rectangle(0, 0, image.Width, image.Height));
        }


        public TemplateMatch[] ProcessImage(Bitmap image, Bitmap template, Rectangle searchZone) {

            if (
                ((image.PixelFormat != PixelFormat.Format8bppIndexed) &&
                  (image.PixelFormat != PixelFormat.Format24bppRgb)) ||
                (image.PixelFormat != template.PixelFormat)) {
                throw new UnsupportedImageFormatException("Unsupported pixel format of the source or template image.");
            }

            if ((template.Width > image.Width) || (template.Height > image.Height)) {
                throw new InvalidImagePropertiesException("Template's size should be smaller or equal to source image's size.");
            }

            BitmapData imageData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);
            BitmapData templateData = template.LockBits(
                new Rectangle(0, 0, template.Width, template.Height),
                ImageLockMode.ReadOnly, template.PixelFormat);
            TemplateMatch[] matchings;
            try {

                matchings = ProcessImage(
                    new UnmanagedImage(imageData),
                    new UnmanagedImage(templateData),
                    searchZone);
            } finally {

                image.UnlockBits(imageData);
                template.UnlockBits(templateData);
            }
            return matchings;
        }

        public TemplateMatch[] ProcessImage(BitmapData imageData, BitmapData templateData) {
            return ProcessImage(new UnmanagedImage(imageData), new UnmanagedImage(templateData),
                new Rectangle(0, 0, imageData.Width, imageData.Height));
        }


        public TemplateMatch[] ProcessImage(BitmapData imageData, BitmapData templateData, Rectangle searchZone) {
            return ProcessImage(new UnmanagedImage(imageData), new UnmanagedImage(templateData), searchZone);
        }

        public TemplateMatch[] ProcessImage(UnmanagedImage image, UnmanagedImage template) {
            return ProcessImage(image, template, new Rectangle(0, 0, image.Width, image.Height));
        }


        public TemplateMatch[] ProcessImage(UnmanagedImage image, UnmanagedImage template, Rectangle searchZone) {

            if (
                ((image.PixelFormat != PixelFormat.Format8bppIndexed) &&
                  (image.PixelFormat != PixelFormat.Format24bppRgb)) ||
                (image.PixelFormat != template.PixelFormat)) {
                throw new UnsupportedImageFormatException("Unsupported pixel format of the source or template image.");
            }

            Rectangle zone = searchZone;
            zone.Intersect(new Rectangle(0, 0, image.Width, image.Height));

            int startX = zone.X;
            int startY = zone.Y;

            int sourceWidth = zone.Width;
            int sourceHeight = zone.Height;
            int templateWidth = template.Width;
            int templateHeight = template.Height;

            if ((templateWidth > sourceWidth) || (templateHeight > sourceHeight)) {
                throw new InvalidImagePropertiesException("Template's size should be smaller or equal to search zone.");
            }
            int pixelSize = (image.PixelFormat == PixelFormat.Format8bppIndexed) ? 1 : 3;
            int sourceStride = image.Stride;


            int mapWidth = sourceWidth - templateWidth + 1;
            int mapHeight = sourceHeight - templateHeight + 1;
            int[,] map = new int[mapHeight + 4, mapWidth + 4];

            int maxDiff = templateWidth * templateHeight * pixelSize * 255;

            int threshold = (int)(similarityThreshold * maxDiff);

            int templateWidthInBytes = templateWidth * pixelSize;

            unsafe {
                byte* baseSrc = (byte*)image.ImageData.ToPointer();
                byte* baseTpl = (byte*)template.ImageData.ToPointer();
                int sourceOffset = image.Stride - templateWidth * pixelSize;
                int templateOffset = template.Stride - templateWidth * pixelSize;

                for (int y = 0; y < mapHeight; y++) {

                    for (int x = 0; x < mapWidth; x++) {
                        byte* src = baseSrc + sourceStride * (y + startY) + pixelSize * (x + startX);
                        byte* tpl = baseTpl;

                        int dif = 0;

                        for (int i = 0; i < templateHeight; i++) {

                            for (int j = 0; j < templateWidthInBytes; j++, src++, tpl++) {
                                int d = *src - *tpl;
                                if (d > 0) {
                                    dif += d;
                                } else {
                                    dif -= d;
                                }
                            }
                            src += sourceOffset;
                            tpl += templateOffset;
                        }

                        int sim = maxDiff - dif;
                        if (sim >= threshold)
                            map[y + 2, x + 2] = sim;
                    }
                }
            }

            List<TemplateMatch> matchingsList = new List<TemplateMatch>();

            for (int y = 2, maxY = mapHeight + 2; y < maxY; y++) {

                for (int x = 2, maxX = mapWidth + 2; x < maxX; x++) {
                    int currentValue = map[y, x];

                    for (int i = -2; (currentValue != 0) && (i <= 2); i++) {

                        for (int j = -2; j <= 2; j++) {
                            if (map[y + i, x + j] > currentValue) {
                                currentValue = 0;
                                break;
                            }
                        }
                    }

                    if (currentValue != 0) {
                        matchingsList.Add(new TemplateMatch(
                            new Rectangle(x - 2 + startX, y - 2 + startY, templateWidth, templateHeight),
                            (float)currentValue / maxDiff));
                    }
                }
            }

            TemplateMatch[] matchings = new TemplateMatch[matchingsList.Count];
            matchingsList.CopyTo(matchings);

            Array.Sort(matchings, new MatchingsSorter());
            return matchings;
        }

        private class MatchingsSorter : System.Collections.IComparer {
            public int Compare(Object x, Object y) {
                float diff = ((TemplateMatch)y).Similarity - ((TemplateMatch)x).Similarity;
                return (diff > 0) ? 1 : (diff < 0) ? -1 : 0;
            }
        }
    }

}