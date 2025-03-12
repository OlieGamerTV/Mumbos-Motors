using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mumbos_Motors.TextureInfo
{
    public class DXT1
    {
        static byte[][] tempData;
        static int modifier = 1;

        public static Color[][] getDynamicBlocksDXT1(byte[][] imageData, int Width, int Height, int modifierType)
        {
            tempData = imageData;
            modifier = modifierType;
            int p = 0;
            int newWidth = Width;
            int newHeight = Height;
            if (Width % 128 != 0) newWidth = Width + (128 - (Width % 128));
            if (Height % 128 != 0) newHeight = Height + (128 - (Height % 128));

            Color[][] start = new Color[newHeight][];
            for (int i = 0; i < start.Length; i++) start[i] = new Color[newWidth];
            Color[][] corrected = new Color[Height][];
            for (int i = 0; i < corrected.Length; i++) corrected[i] = new Color[Width];

            for (int i = 0; i < (newWidth / 128) * (newHeight / 128); i++)
            {
                //MessageBox.Show("ColorCopy(getTile(" + p + "), " + start + ", " + ((i % (newWidth / 128)) * 128) + ", " + ((i / (newWidth / 128)) * 128) + ");");
                start = ColorUtils.ColorCopy(getTile(p), start, (i % (newWidth / 128)) * 128, (i / (newWidth / 128)) * 128);
                p += 0x2000;
            }

            corrected = ColorUtils.ColorCopy(start, 0, 0, Width, Height, corrected, 0, 0);
            return corrected;
        }
        public static Color[][] getTile(int p)
        {
            int duoPanelLen = 0x800 * modifier;
            Color[][][] duoPanels = new Color[4][][];

            duoPanels[0] = getDuoPanel(p);
            duoPanels[2] = getDuoPanel((p) + duoPanelLen);
            duoPanels[1] = ColorUtils.swapPanel(getDuoPanel((p) + (duoPanelLen * 2)));
            duoPanels[3] = ColorUtils.swapPanel(getDuoPanel((p) + (duoPanelLen * 3)));

            Color[][] tile = new Color[duoPanels.Length * 32][];
            for (int i = 0; i < tile.Length; i++) //height
            {
                tile[i] = new Color[128];
                for (int j = 0; j < duoPanels[(i / 32)][i % 32].Length; j++) //length
                {
                    tile[i][j] = duoPanels[(i / 32)][i % 32][j];
                    //if (i >= 95 && j >= 110)
                    //{
                    //    MessageBox.Show("tile[" + i + "][" + j + "] = duoPanels[" + (i / 32) + "][" + (i % 32) + "][" + j + "]");
                    //}
                }
            }
            return tile;
        } //128
        public static Color[][] getDuoPanel(int p)
        {
            Color[][] duoPanel = new Color[32][];
            Color[][][] panels = new Color[2][][];
            panels[0] = getPanel(p);
            panels[1] = getPanel(p + (0x400 * modifier));

            for (int i = 0; i < duoPanel.Length; i++)
            {
                duoPanel[i] = new Color[128];
                for (int j = 0; j < panels[i / 16][i % 16].Length; j++)
                {
                    Color towrite = panels[i / 16][i % 16][j];
                    duoPanel[i][j] = towrite;
                }
            }


            return duoPanel;
        }
        public static Color[][] getPanel(int p) //height, width
        {
            Color[][][] bigTexels = new Color[32][][];
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    bigTexels[((i * 16) + (((j / 2) * 4) - ((j / 8) * 14)) + (j % 2))] = getBigTexel(p);
                    p += 0x20 * modifier;
                }
            }
            Color[][] panel = new Color[16][];
            for (int i = 0; i < panel.Length; i++) //fill in the rows (downward)
            {
                panel[i] = new Color[128];
                for (int j = 0; j < 16; j++) //var for bigTexels across
                {
                    for (int k = 0; k < 8; k++) //traverse bigTexel width
                    {
                        try
                        {

                            panel[i][(j * 8) + k] = bigTexels[((i / 8) * 16) + j][k][i % 8];
                        }
                        catch
                        {
                            MessageBox.Show("panel[" + i + "][" + ((j * 8) + k) + "] = bigTexels[" + (((i / 8) * 16) + j) + "][" + k + "][" + (i + ((i / 8) * 8)) + "]");
                        }

                    }
                }
            }
            return panel;
        }
        public static Color[][] getBigTexel(int p)
        {
            //MessageBox.Show("current pos: " + p.ToString("X8"));
            Color[][] bigTexel = new Color[4][];
            for (int j = 0; j < 4; j++)
            {
                bigTexel[j] = getTexel(DataMethods.readInt16(tempData[1], p), DataMethods.readInt16(tempData[1], p + 0x2), DataMethods.readInt16(tempData[1], p + 0x4), DataMethods.readInt16(tempData[1], p + 0x6));

                //int change = (p - last);
                //if (change != (0x8 * modifier))
                //{
                //    MessageBox.Show("change: " + (p.ToString("X")) + " - " + last.ToString("X") + " = " + change.ToString("X") + "\nFormat: " + format.ToString("X2"));
                //}
                //last = p;

                p += 0x8 * modifier;
            }

            Color[][] Ordered = new Color[8][];
            for (int i = 0; i < Ordered.Length; i++)
            {
                Ordered[i] = new Color[8];
            }
            for (int i = 0; i < bigTexel.Length; i++)
            {
                for (int j = 0; j < bigTexel[i].Length; j++)
                {
                    //MessageBox.Show("x: " + (((i % 2) * 4) + (j % 4)) + " y: " + ((j / 4) + ((i / 2) * 4)));
                    Ordered[((i % 2) * 4) + (j % 4)][(j / 4) + ((i / 2) * 4)] = bigTexel[i][j];
                }
            }

            return Ordered;
        }
        public static Color[] getTexel(int color1, int color2, int lay1, int lay2) //dxt1
        {
            Color[] colors = { ColorUtils.GetColor565(color1), ColorUtils.GetColor565(color2) };
            Color[] c = new Color[16];

            //for (int i = 0; i < 16; i++)
            //{
            //    c[i] = colors[(i + ((i / 4) % 2)) % 2];
            //}

            for (int i = 0; i < 16; i++)
            {
                if (i < 8)
                {
                    c[i] = ColorUtils.GetColorLayout(i, colors[0], colors[1], lay1);
                }
                else
                {
                    c[i] = ColorUtils.GetColorLayout(i, colors[0], colors[1], lay2);
                    //MessageBox.Show(c[i].ToString());
                }
            }
            return c;
        }

        public static Color[] getTexelOld(int color1, int color2, int lay1, int lay2)
        {
            Color[] colors = { ColorUtils.GetColor565(color1), ColorUtils.GetColor565(color2) };
            Color[] c = new Color[16];

            for (int i = 0; i < 16; i++)
            {
                c[i] = colors[(i + ((i / 4) % 2)) % 2];
            }

            return c;
        }//dxt1
    }
}
