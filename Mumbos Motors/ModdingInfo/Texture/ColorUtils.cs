using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mumbos_Motors.TextureInfo
{
    public class ColorUtils
    {
        public static Color[][] ColorCopy(Color[][] sourceArray, Color[][] destArray, int x, int y)
        {
            for (int i = x; i < x + sourceArray[0].Length; i++)
            {
                for (int j = y; j < y + sourceArray.Length; j++)
                {
                    destArray[j][i] = sourceArray[j - y][i - x];
                }
            }
            return destArray;
        }
        public static Color[][] ColorCopy(Color[][] sourceArray, int sourceX, int sourceY, int w, int h, Color[][] destArray, int destX, int destY)
        {
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    //MessageBox.Show("destArray[" + (destY + i) + "][" + (destX + j) + "] = sourceArray[" + (sourceY) + " " + (i) + "][" + (sourceX + j) + "]");
                    destArray[destY + i][destX + j] = sourceArray[sourceY + i][sourceX + j];
                }
            }
            return destArray;
        }

        public static Color Average(Color c, Color c1, Color c2)
        {
            return Color.FromArgb((c.R + c1.R + c2.R) / 3, (c.G + c1.G + c2.G) / 3, (c.B + c1.B + c2.B) / 3);
        }

        public static Color HalfColor(Color c)
        {
            return Color.FromArgb(c.R / 2, c.G / 2, c.B / 2);
        }

        public static Color GetColor565(int word)
        {
            //16bits to to rgb565
            Color c = new Color();
            int blue = (int)(255.0 * ((word & 0x1F) / (double)0x1F));
            int green = (int)(255.0 * (((word >> 5) & 0x3F) / (double)0x3F));
            int red = (int)(255.0 * (((word >> 11) & 0x1F) / (double)0x1F));
            //int blue = (word & 0x1F) << 3;
            //int green = (word & 0x1E0) >> 3;
            //int red = (word & 0xF800) >> 8;
            c = Color.FromArgb(red, green, blue);
            return c;
        }

        public static Color GetColorLayout(int p, Color c1, Color c2, int layout)
        {
            p %= 8;
            Color ret = new Color();
            int andIndex = (int)Math.Pow(2, p * 2);

            //MessageBox.Show(p + "");
            int andIndex2 = (int)Math.Pow(2, (p * 2) + 1);
            //MessageBox.Show("if ((" + layout.ToString("X8") + " & " + andIndex + ") == 0");
            if ((layout & andIndex) == 0)
            {
                ret = c1;
            }
            else
            {
                ret = c2;
            }
            if ((layout & andIndex2) != 0) //average
            {
                ret = Average(ret, c1, c2);
            }
            return ret;
        } //dxt1

        public static Color GetColorLayout(int layout, int avg, int alphaLayout, Color c1, Color c2, int[] alphas) //dxt3
        {
            Color preColor = new Color[] { c1, c2 }[layout];
            if (avg == 1)
            {
                preColor = Average(preColor, c1, c2);
            }
            return Color.FromArgb(alphas[alphaLayout], preColor.R, preColor.G, preColor.B);
        }

        public static Color[][] swapPanel(Color[][] duoPanel)
        {
            Color[][] ret = new Color[duoPanel.Length][];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new Color[duoPanel[i].Length];
                for (int j = 0; j < ret[i].Length / 2; j++)
                {
                    ret[i][j] = duoPanel[i][j + 64];
                }
                for (int j = 0; j < ret[i].Length / 2; j++)
                {
                    ret[i][j + 64] = duoPanel[i][j];
                }
            }
            return ret;
        }
    }
}
