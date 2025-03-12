using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mumbos_Motors.TextureInfo
{
    public class DXT3
    {
        static byte[][] tempData;

        public static Color[][] getDynamicBlocksDXT3(byte[][] imageData, int Width, int Height)
        {
            tempData = imageData;

            int p = 0;
            int newWidth = Width;
            int newHeight = Height;
            if (Width % 128 != 0) newWidth = Width + (128 - (Width % 128));
            if (Height % 128 != 0) newHeight = Height + (128 - (Height % 128));

            Color[][] start = new Color[newHeight][];
            for (int i = 0; i < start.Length; i++) start[i] = new Color[newWidth];

            for (int i = 0; i < (newWidth / 128) * (newHeight / 128); i++)
            {
                //MessageBox.Show("ColorCopy(getTile(" + p + "), " + start + ", " + ((i % (newWidth / 128)) * 128) + ", " + ((i / (newWidth / 128)) * 128) + ");");
                start = ColorUtils.ColorCopy(getTile0x53(p), start, (i % (newWidth / 128)) * 128, (i / (newWidth / 128)) * 128);
                p += 0x4000;
            }
            Color[][] corrected = new Color[Height][];
            for (int i = 0; i < corrected.Length; i++) corrected[i] = new Color[Width];

            return ColorUtils.ColorCopy(start, 0, 0, Width, Height, corrected, 0, 0);
        }

        public static Color[][] getTile0x53(int p)
        {
            Color[][] ret = new Color[128][];
            for (int i = 0; i < 128; i++) ret[i] = new Color[128];

            for (int i = 0; i < 8; i++)
            {
                switch (i / 4)
                {
                    case 0:
                        {
                            //MessageBox.Show("Y: " + ((((i % 2) * 64) + ((i / 2) * 16))));
                            ret = ColorUtils.ColorCopy(getPanel0x53((i * 0x800) + p), ret, 0, (((i % 2) * 64) + ((i / 2) * 16)));
                            break;
                        }
                    case 1:
                        {
                            //MessageBox.Show("Y: " + ((((i % 2) * 64) + ((i / 2) * 16))));
                            ret = ColorUtils.ColorCopy(ColorUtils.swapPanel(getPanel0x53((i * 0x800) + p)), ret, 0, (((i % 2) * 64) + ((i / 2) * 16)));
                            break;
                        }
                }
            }
            return ret;
        }

        public static Color[][] getPanel0x53(int p)//0x800
        {
            Color[][] ret = new Color[16][];
            for (int i = 0; i < ret.Length; i++) ret[i] = new Color[128];

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    Color[][] bigTexel = getBigTexel0x53(((j * 0x40) + (i * 0x400)) + p);
                    //MessageBox.Show("X: " + (((j * 0x20) % 128) + ((j / 4) * 8)) + " Y: " + (i * 8));
                    ret = ColorUtils.ColorCopy(bigTexel, ret, ((j * 0x20) % 128) + ((j / 4) * 8), i * 8);
                }
            }
            return ret;
        }

        public static Color[][] getBigTexel0x53(int p)//0x40
        {
            Color[][] ret = new Color[8][];
            for (int i = 0; i < ret.Length; i++) ret[i] = new Color[8];
            for (int i = 0; i < 4; i++)
            {
                Color[] texel = getTexel(DataMethods.readInt16(tempData[1], p + (i * 0x10) + 0x0), DataMethods.readInt16(tempData[1], p + (i * 0x10) + 0x2), DataMethods.readInt16(tempData[1], p + (i * 0x10) + 0x4), DataMethods.readInt16(tempData[1], p + (i * 0x10) + 0x6), DataMethods.readInt16(tempData[1], p + (i * 0x10) + 0x8), DataMethods.readInt16(tempData[1], p + (i * 0x10) + 0xA), DataMethods.readInt16(tempData[1], p + (i * 0x10) + 0xC), DataMethods.readInt16(tempData[1], p + (i * 0x10) + 0xE));
                for (int j = 0; j < texel.Length; j++)
                {
                    ret[((i * 4) % 8) + (j / 4)][(j % 4) + ((i / 2) * 4)] = texel[j];
                }
            }
            //int change = (p - last);
            //if (change != (0x40))
            //{
            //    MessageBox.Show("change: " + (p.ToString("X")) + " - " + last.ToString("X") + " = " + change.ToString("X") + "\nFormat: " + format.ToString("X2"));
            //}
            //else
            //{
            //    MessageBox.Show("OK");
            //}
            //last = p;

            return ret;
        }

        public static Color[] getTexel(int a1, int a2, int a3, int a4, int color1, int color2, int lay1, int lay2) //dxt3
        {
            int[] alphas = new int[] { a1, a2, a3, a4 };
            Color[] colors = { ColorUtils.GetColor565(color1), ColorUtils.GetColor565(color2) };
            Color[] c = new Color[16];

            for (int i = 0; i < 16; i++)
            {
                if (i < 8)
                {
                    c[i] = ColorUtils.GetColorLayout(i, colors[0], colors[1], lay1);
                }
                else
                {
                    c[i] = ColorUtils.GetColorLayout(i, colors[0], colors[1], lay2);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int alpha = (int)(((15 & (alphas[i] >> (j * 4))) / 15.0) * 255);
                    c[(i * 4) + j] = Color.FromArgb(alpha, c[(i * 4) + j].R, c[(i * 4) + j].G, c[(i * 4) + j].B);
                }
            }


            return c;
        }
    }
}
