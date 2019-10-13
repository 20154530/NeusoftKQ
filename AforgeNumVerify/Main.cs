using AforgeNumVerify.AForge.Core;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace AforgeNumVerify {
    public class Main {

        /// <summary>
        /// 从文件读入位图并预处理
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Bitmap LoadTestImg(string path) {

            Bitmap bori = null;
            using (FileStream fs = File.OpenRead(path)) {
                Image result = Image.FromStream(fs);
                bori = new Bitmap(result);
            }
            return PreProcess(bori);
        }

        /// <summary>
        /// 预处理位图
        /// </summary>
        /// <param name="bori"></param>
        /// <returns></returns>
        public static Bitmap PreProcess(Bitmap bori) {
            var bnew = new Bitmap(bori.Width, bori.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bnew)) {
                g.DrawImage(bori, 0, 0);
            }

            using (bnew = new Grayscale(0.2125, 0.7154, 0.0721).Apply(bnew)) {
                using (bnew = new Threshold(50).Apply(bnew)) {
                    return new BlobsFiltering(1, 1, bnew.Width, bnew.Height).Apply(bnew);
                }
            }
        }

        /// <summary>
        /// 按照 Y 轴线 切割
        /// (丢弃等于号)
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public static List<Bitmap> Crop_Y(Bitmap b) {
            var list = new List<Bitmap>();

            int[] cols = new int[b.Width];

            for (int x = 0; x < b.Width; x++) {
                for (int y = 0; y < b.Height; y++) {
                    //获取当前像素点像素
                    var pixel = b.GetPixel(x, y);

                    //说明是黑色点
                    if (pixel.R == 0) {
                        cols[x] = ++cols[x];
                    }
                }
            }

            int left = 0, right = 0;

            for (int i = 0; i < cols.Length; i++) {
                //说明该列有像素值（为了防止像素干扰，去噪后出现空白的问题，所以多判断一下，防止切割成多个)
                if (cols[i] > 0 || (i + 1 < cols.Length && cols[i + 1] > 0)) {
                    if (left == 0) {
                        //切下来图片的横坐标left
                        left = i;
                    } else {
                        //切下来图片的横坐标right
                        right = i;
                    }
                } else {
                    //说明已经有切割图了，下面我们进行切割处理
                    if ((left > 0 || right > 0)) {
                        Crop corp = new Crop(new Rectangle(left, 0, right - left + 1, b.Height));

                        var small = corp.Apply(b);

                        //居中，将图片放在20*50的像素里面

                        list.Add(small);
                    }

                    left = right = 0;
                }
            }

            return list;
        }

        /// <summary>
        /// 按照 X 轴线 切割
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static List<Bitmap> Crop_X(List<Bitmap> list) {
            var corplist = new List<Bitmap>();

            //再对分割的图进行上下切割，取出上下的白边
            foreach (var segb in list) {
                //统计每一行的“1”的个数，方便切除
                int[] rows = new int[segb.Height];

                /*
                 *  横向切割
                 */
                for (int y = 0; y < segb.Height; y++) {
                    for (int x = 0; x < segb.Width; x++) {
                        //获取当前像素点像素
                        var pixel = segb.GetPixel(x, y);

                        //说明是黑色点
                        if (pixel.R == 0) {
                            rows[y] = ++rows[y];
                        }
                    }
                }

                int bottom = 0, top = 0;

                for (int y = 0; y < rows.Length; y++) {
                    //说明该行有像素值（为了防止像素干扰，去噪后出现空白的问题，所以多判断一下，防止切割成多个)
                    if (rows[y] > 0 || (y + 1 < rows.Length && rows[y + 1] > 0)) {
                        if (top == 0) {
                            //切下来图片的top坐标
                            top = y;
                        } else {
                            //切下来图片的bottom坐标
                            bottom = y;
                        }
                    } else {
                        //说明已经有切割图了，下面我们进行切割处理
                        if ((top > 0 || bottom > 0) && bottom - top > 0) {
                            Crop corp = new Crop(new Rectangle(0, top, segb.Width, bottom - top + 1));

                            var small = corp.Apply(segb);

                            corplist.Add(small);
                        }

                        top = bottom = 0;
                    }
                }
            }

            return corplist;
        }


        /// <summary>
        /// 重置图片的指定大小并且居中
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<Bitmap> ToResizeAndCenterIt(List<Bitmap> list, int w = 20, int h = 20) {
            List<Bitmap> resizeList = new List<Bitmap>();


            for (int i = 0; i < list.Count; i++) {
                //反转一下图片
                list[i] = new Invert().Apply(list[i]);

                int sw = list[i].Width;
                int sh = list[i].Height;

                Crop corpFilter = new Crop(new Rectangle(0, 0, w, h));

                list[i] = corpFilter.Apply(list[i]);

                //再反转回去
                list[i] = new Invert().Apply(list[i]);

                //计算中心位置
                int centerX = (w - sw) / 2;
                int centerY = (h - sh) / 2;

                list[i] = new CanvasMove(new IntPoint(centerX, centerY), Color.White).Apply(list[i]);

                resizeList.Add(list[i]);
            }

            return resizeList;
        }
    }
}
