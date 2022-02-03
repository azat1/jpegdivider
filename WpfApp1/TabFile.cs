using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace RasterView
{
    internal class TabFile
    {
        private string fileName;
        public string rastername;
        internal int[] rx, ry;
        internal double[] wx, wy;

        public TabFile(string fileName)
        {
            int pcount = 0;
            this.fileName = fileName;
            rx = new int[3];
            ry = new int[3];
            wx = new double[3];
            wy = new double[3];
            Regex fnregex = new Regex("File \"(.+)\"");
            Regex pointregex = new Regex(
                @"\(\s*(\d+\.\d+)\s*,\s*(\d+\.\d+)\s*\)\s*\(\s*(\d+)\s*,\s*(\d+)\s*\)");
            if (File.Exists(fileName))
            {
                StreamReader rs = new StreamReader(fileName,System.Text.Encoding.Default);
                while (!rs.EndOfStream)
                {
                    string s=rs.ReadLine();
                    if (fnregex.IsMatch(s))
                    {
                        var r = fnregex.Match(s);
                        rastername = r.Groups[1].Value;
                    }
                    if (pointregex.IsMatch(s))
                    {
                        if (pcount > 3) continue;
                        var r = pointregex.Match(s);
                        wx[pcount] = double.Parse( r.Groups[2].Value,CultureInfo.InvariantCulture);
                        wy[pcount] = double.Parse(r.Groups[1].Value, CultureInfo.InvariantCulture);
                        rx[pcount] = int.Parse(r.Groups[3].Value);
                        ry[pcount] = int.Parse(r.Groups[4].Value);
                        pcount++;
                    }
                }
            }

        }
    }
}