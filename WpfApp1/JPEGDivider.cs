using BitMiracle.LibJpeg;
using BitMiracle.LibJpeg.Classic;
using RasterView;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tplan;

namespace JpegDividerApp
{
    class JPEGDivider
    {
        public static void DivideJpeg(string filename, int hcount, int vcount, IProgress<int> progress, string tabname)
        {
//            DivideTab(filename, tabname, hcount, vcount);
            string basename = Path.GetFileNameWithoutExtension(filename);
            string path = Path.GetDirectoryName(filename);
            jpeg_compress_struct[] compressors = new jpeg_compress_struct[hcount];
            FileStream[] deststreams = new FileStream[hcount];
            jpeg_compress_struct compressor = new jpeg_compress_struct();
            jpeg_decompress_struct decompressor = new jpeg_decompress_struct();
            FileStream source = new FileStream(filename, FileMode.Open);
            //FileStream dest = new FileStream(Path.GetDirectoryName(filename) + "\\" +
              //  Path.GetFileNameWithoutExtension(filename) + "_1.jpg", FileMode.Create);

            decompressor.jpeg_stdio_src(source);
            decompressor.jpeg_read_header(true);
            decompressor.jpeg_calc_output_dimensions();
            decompressor.jpeg_start_decompress();
            DivideTab(filename, tabname, hcount, vcount,decompressor.Image_width, decompressor.Image_height);
            //    Debugger.Log(0, "TEST", decompressor.Image_width.ToString());
            //    Debugger.Log(0, "TEST", decompressor.Image_height.ToString());
            int newwidth = decompressor.Image_width / hcount;
            int newheight = decompressor.Image_height / vcount;
            int h = 0;
            InitCompressors(filename, hcount, compressors, deststreams, decompressor, newwidth, newheight, h);

            //compressor.Image_width = decompressor.Image_width / 2;
            //compressor.Image_height = decompressor.Image_height / 2;
            //compressor.In_color_space = decompressor.Out_color_space;
            //compressor.Input_components = decompressor.Output_components;
            //compressor.Density_unit = decompressor.Density_unit;
            //compressor.X_density = decompressor.X_density;
            //compressor.Y_density = decompressor.Y_density;
            //compressor.jpeg_set_defaults();
            //compressor.jpeg_stdio_dest(dest);
            //compressor.jpeg_start_compress(true);
            //            Debug.Write(decompressor.Image_height);
            int linenumber = 0;
            progress.Report(decompressor.Output_height);
            while (decompressor.Output_scanline < decompressor.Output_height)
            {
                byte[][] row = jpeg_common_struct.AllocJpegSamples(decompressor.Output_width * decompressor.Output_components, 1);
                decompressor.jpeg_read_scanlines(row, 1);
                progress.Report(decompressor.Output_scanline);
                if (linenumber < compressors[0].Image_height)
                {
                    WriteScanLines(hcount, compressors, row);
                }
                else
                {
                    //WriteScanLines(hcount, compressors, row);
                    foreach (jpeg_compress_struct comp in compressors) comp.jpeg_finish_compress();
                    foreach (FileStream ss in deststreams) ss.Close();
                    h++;
                    if (h >= hcount) continue;
                    InitCompressors(filename, hcount, compressors, deststreams, decompressor, newwidth, newheight, h);
                    WriteScanLines(hcount, compressors, row);
                    linenumber = 0;
                    
                }
                //compressor.jpeg_write_scanlines(row, 1);
                linenumber++;
                //ProcessPixelsRow(row[0]);
            }
 //           compressor.jpeg_finish_compress();
            decompressor.jpeg_finish_decompress();
            source.Close();

       //     dest.Close();
            //JpegImage image = new JpegImage(filename);
            //JpegImage[,] newfiles = new JpegImage[hcount, vcount];
            //for (int h = 0; h < hcount; h++)
            //    for (int v = 0; v < vcount; v++)
            //    {

            //        int neww = image.Width / hcount;
            //        int newh = image.Height / vcount;
            //        SampleRow[] rows = new SampleRow[newh];
            //        for (int row = 0; row < rows.Length; row++)
            //        {
            //            SampleRow src = image.GetRow(row+h*newh);

            //            byte[] b = src.ToBytes();
            //            int bytesperpixel = b.Length / src.Length;
            //            byte[] nb = new byte[b.Length / hcount + 1];
            //            Array.Copy(b, h * neww * bytesperpixel, nb, 0, neww * bytesperpixel);
            //            rows[row] = new SampleRow(nb, neww, image.BitsPerComponent, image.ComponentsPerSample);
            //        }
            //        string newfname = path+"\\"+ basename + h.ToString() + "_" + v.ToString() + ".jpg";
            //        JpegImage destimage = new JpegImage(rows, image.Colorspace);
            //        FileStream stream = new FileStream(newfname, FileMode.Create);
            //        destimage.WriteJpeg(stream);
            //        stream.Close();
            //        destimage.Dispose();
            //    }

        }

        private static void DivideTab(string filename, string tabname, int hcount, int vcount, int totalw,int totalh)
        {
            if (!File.Exists(tabname)) return;
            TabFile tabFile = new TabFile(tabname);
            Matrix2 smatrix = new Matrix2(tabFile.rx[0], tabFile.ry[0], 1,
                tabFile.rx[1], tabFile.ry[1], 1,
                tabFile.rx[2], tabFile.ry[2], 1);
            Matrix2 dmatrix = new Matrix2(tabFile.wx[0], tabFile.wy[0], 1,
                tabFile.wx[1], tabFile.wy[1], 1,
                tabFile.wx[2], tabFile.wy[2], 1);
            smatrix.Invert();
            smatrix.Mul(dmatrix);
            int rasterw = totalw / hcount;
            int rasterh = totalh / vcount;
            for (int h=0;h<hcount;h++)
                for (int v=0;v<vcount;v++)
                {
                    string newtabname = Path.GetDirectoryName(tabname) + "\\" +
                        Path.GetFileNameWithoutExtension(tabname) +"_"+ h.ToString() + "_" + v.ToString() +
                        ".tab";
                    string rastername=Path.GetFileNameWithoutExtension(tabFile.rastername)+"_"+ h.ToString() + "_" + v.ToString() +
                        ".jpg";
                    StreamWriter fs = new StreamWriter(newtabname,false, Encoding.Default , 10000);

                    WriteTabHeader(fs, rastername);
                    int rx = h * rasterw;
                    int ry = v * rasterh;
                    double wx, wy;
                    smatrix.ConvertPoint(rx, ry, out wx, out wy);
                    WriteTabPoint(fs, 0,0, wx,wy,"Point 0");

                    rx = (h + 1) * rasterw;
                    ry = v * rasterh;
                    smatrix.ConvertPoint(rx, ry, out wx, out wy);
                    WriteTabPoint(fs, rasterw-1, 0, wx, wy, "Point 1");

                  //  rx = (h + 1) * rasterw;
                    ry = (v + 1) * rasterh;
                    smatrix.ConvertPoint(rx, ry, out wx, out wy);
                    WriteTabPoint(fs, rasterw - 1, rasterh-1, wx, wy, "Point 2");

                    WriteTabEnd(fs);
                    fs.Close();

                }




        }

        private static void WriteTabEnd(StreamWriter fs)
        {
            fs.WriteLine("CoordSys NonEarth Units \"m\"");
            fs.WriteLine("Units \"m\"");
            
        }

        private static void WriteTabPoint(StreamWriter fs, int rx, int ry, double wx, double wy, string label)
        {
            fs.WriteLine($"({wy.ToString("F4",CultureInfo.InvariantCulture)},{wx.ToString("F4", CultureInfo.InvariantCulture)}) ({rx},{ry}) Label \"{label}\"");
        }

        private static void WriteTabHeader(StreamWriter fs, string rastername)
        {
            fs.WriteLine("!table");
            fs.WriteLine("!version 300");
            fs.WriteLine("!charset Neutral");
            fs.WriteLine("Definition Table");
            fs.WriteLine("File \""+rastername+"\"");
            fs.WriteLine("Type \"RASTER\"");

        }

        private static void WriteScanLines(int hcount, jpeg_compress_struct[] compressors, byte[][] row)
        {
            for (int ii = 0; ii < hcount; ii++)
            {
                byte[][] rowcopy = new byte[1][];
                rowcopy[0] = new byte[row[0].Length / hcount + 1];
                int bwidth = row[0].Length / hcount;
                Array.Copy(row[0], ii * bwidth, rowcopy[0], 0, bwidth);
                compressors[ii].jpeg_write_scanlines(rowcopy, 1);
            }
        }

        private static void InitCompressors(string filename, int hcount, jpeg_compress_struct[] compressors, FileStream[] deststreams, jpeg_decompress_struct decompressor, int newwidth, int newheight, int h)
        {
            for (int i = 0; i < hcount; i++)
            {
                compressors[i] = new jpeg_compress_struct();
                compressors[i].Image_height = newheight;
                compressors[i].Image_width = newwidth;
                compressors[i].In_color_space = decompressor.Out_color_space;
                compressors[i].Input_components = decompressor.Output_components;// newwidth;
                compressors[i].Density_unit = decompressor.Density_unit;
                compressors[i].X_density = decompressor.X_density;
                compressors[i].Y_density = decompressor.Y_density;
                compressors[i].X_density = decompressor.X_density;
                compressors[i].jpeg_set_defaults();
                string newfname = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename)
                    + "_" + i.ToString() + "_" + h.ToString() + ".jpg";
                deststreams[i] = new FileStream(newfname, FileMode.Create);
                compressors[i].jpeg_stdio_dest(deststreams[i]);
                compressors[i].jpeg_start_compress(true);
            }
        }

        private static void ProcessPixelsRow(byte[] v)
        {
            
        }
    }
}
