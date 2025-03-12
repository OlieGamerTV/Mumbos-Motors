using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Mumbos_Motors.TextureInfo;
using System.Drawing.Imaging;
using System.IO;
using KUtility;
using static System.Net.Mime.MediaTypeNames;

namespace Mumbos_Motors
{

    public class texture : ModdingTab
    {
        Bitmap btm;
        Random ran = new Random();
        PictureBox pbx;
        int oldwidth;
        int oldheight;
        int width;
        int height;
        int type;
        int format;
        int modifier = 1;
        int last = 0;
        bool isDDS;
        byte[][] tempSectionData;
        public texture(CAFF caff, int fileID) : base(caff, fileID)
        {
            buildMetaPage();
        }

        public texture(MULTICAFF multiCaff, int caffIndex, int symbolID) : base(multiCaff, caffIndex, symbolID)
        {
            buildMetaPage();
        }

        public override void buildMetaPage()
        {
            tempSectionData = hxd.sectionData;
            width = DataMethods.readInt16(hxd.sectionData[0], 0x24);
            height = DataMethods.readInt16(hxd.sectionData[0], 0x26);
            pbx = new PictureBox();
            pbx.Location = new Point(metaTab.sideSpacing, metaTab.sideSpacing + 25);

            solveBitmap();
            metaTab.metaSpacing += pbx.Height + metaTab.sideSpacing + 25;
            if (pbx.Width > metaTab.Background.Width)
            {
                metaTab.Background.Width = pbx.Width + (2 * metaTab.sideSpacing);
            }

            if (isDDS)
            {
                MetaBlock_Text("Width: ", 1, 0, 0, 0);
                EditLastTextBox(width + "", 100);

                MetaBlock_Text("Height: ", 1, 0, 0, 0);
                EditLastTextBox(height + "", 100);
            }
            else
            {
                MetaBlock_Text("Format: ", 1, 0x1B, 0x1, 1);
                MetaBlock_Text("Type: ", 1, 0x30, 0x1, 1);
                MetaBlock_Text("LOD Specifier: ", 1, 0x2C, 0x4, 1);
                MetaBlock_Text("Width: ", 1, 0x24, 0x2, 0);
                MetaBlock_Text("Height: ", 1, 0x26, 0x2, 0);

                metaTab.buildMetaFunPanel();
                buildMetaFunPanelButton("Randomize Colors");
                latestFunButton().Click += new EventHandler(RandomizeColors);
                buildMetaFunPanelButton("Checkerboard Colors");
                latestFunButton().Click += new EventHandler(CheckerboardColors);
                buildMetaFunPanelButton("10", "Amplify Colors");
                latestFunButton().Click += new EventHandler(AmplifyColors);
            }

            bottomToolBar.addButton("Export Texture");
            bottomToolBar.buttons[bottomToolBar.buttons.Count - 1].Click += new EventHandler(export_texture);

            if (!isDDS)
            {
                bottomToolBar.addButton("Export ALL Textures");
                bottomToolBar.buttons[bottomToolBar.buttons.Count - 1].Click += new EventHandler(export_textures);
            }
        }

        /// <summary>
        /// Randomizes the colors for bkn&b's .rtx format
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RandomizeColors(object sender, EventArgs e) //section 2
        {
            for (int i = 0; i < hxd.sectionData[1].Length; i += 0x8)
            {
                //color 1
                hxd.sectionData[1][i] = (byte)ran.Next(0, 256);
                hxd.sectionData[1][i + 1] = (byte)ran.Next(0, 256);
                //color 2
                hxd.sectionData[1][i + 2] = (byte)ran.Next(0, 256);
                hxd.sectionData[1][i + 3] = (byte)ran.Next(0, 256);
            }
            metaTab.metaChanged[1] = true;
            MessageBox.Show("Color data randomized successfully. Don't forget to save it :)");
        }
        void CheckerboardColors(object sender, EventArgs e) //section 2
        {
            for (int i = 0; i < hxd.sectionData[1].Length; i += 0x8)
            {
                //layout 1
                hxd.sectionData[1][i + 4] = 0x11;
                hxd.sectionData[1][i + 5] = 0x44;
                //layout 2
                hxd.sectionData[1][i + 6] = 0x11;
                hxd.sectionData[1][i + 7] = 0x44;
            }
            metaTab.metaChanged[1] = true;
            MessageBox.Show("Color data checkerboarded successfully. Don't forget to save it :)");
        }
        void AmplifyColors(object sender, EventArgs e) //section 2
        {
            Button addButton = sender as Button;
            int add = Convert.ToInt32(metaTab.metaFunPanel.funTextBoxes[Convert.ToInt32(addButton.Name)].Text);
            for (int i = 0; i < hxd.sectionData[1].Length; i += 0x8)
            {
                //layout 1
                hxd.sectionData[1][i] = (byte)((hxd.sectionData[1][i] + add) % 256);
                hxd.sectionData[1][i + 1] = (byte)((hxd.sectionData[1][i + 1] + (add / 2)) % 256);
                //layout 2
                hxd.sectionData[1][i + 2] = (byte)((hxd.sectionData[1][i + 2] + (add / 2)) % 256);
                hxd.sectionData[1][i + 3] = (byte)((hxd.sectionData[1][i + 3] + add) % 256);
            }
            metaTab.metaChanged[1] = true;


            MessageBox.Show("Colors data amplified. Don't forget to save it :)");
        }

        void export_texture(object sender, EventArgs e)
        {
            if (isDDS)
            {
                btm.Save(DataMethods.saveFileDialogDDSTexture(hxd.symbol) + ".dds");
            }
            else
            {
                btm.Save(DataMethods.saveFileDialogTexture(DataMethods.readTextureSymbol(hxd.symbol)) + ".png");
            }
        }

        void export_textures(object sender, EventArgs e)
        {
            string[] symbols = DataMethods.getStringsBySearch(caff.getSymbols(), "aid_texture");
            for (int i = 0; i < symbols.Length; i++)
            {
                if (symbols[i].Contains("colour") || symbols[i].Contains("Color") || symbols[i].Contains("sky") || symbols[i].Contains("cubemaps"))
                {
                    tempSectionData = caff.readSectionsData(DataMethods.getIndexBySearch(caff.getSymbols(), symbols[i]));
                    RipRTX(DataMethods.readInt16(tempSectionData[0], 0x24), DataMethods.readInt16(tempSectionData[0], 0x26), DataMethods.readTextureSymbol(symbols[i]));
                }
            }
            tempSectionData = hxd.sectionData;
        }

        private void RipRTX(int Width, int Height, string name)
        {
            if (Width >= 512) Width /= 2;
            if (Height >= 512) Height /= 2;
            Color[][] test = new Color[0][];
            //MessageBox.Show(name);
            Bitmap btm;
            if (Width >= 128 && Height >= 128)
            {
                btm = colorToBitmap(test);
                btm.Save(@"C:\Users\Alex Weight\Desktop\textures\" + name + ".png");
            }
        }
        public void solveBitmap()
        {
            string magic = DataMethods.readString(hxd.sectionData[0], 0x0, 4);
            if (magic.Contains("DDS"))
            {
                BinaryReader reader = new BinaryReader(new MemoryStream(hxd.sectionData[0]));

                reader.BaseStream.Seek(0xA, SeekOrigin.Begin);
                byte[] wrongWidth = reader.ReadBytes(2);
                byte[] wrongHeight = reader.ReadBytes(2);
                width = DataMethods.readInt(DataMethods.swapEndianness(wrongWidth, 2), 0, 2);
                height = DataMethods.readInt(DataMethods.swapEndianness(wrongHeight, 2), 0, 2);

                magic = DataMethods.readString(hxd.sectionData[0], 0x54, 4);

                if (magic.Contains("DXT1"))
                {
                    reader.BaseStream.Seek(0x80, SeekOrigin.Begin);
                    byte[] bytes = reader.ReadBytes(hxd.sectionData[0].Length - 0x80);
                    btm = DDSImage.UncompressDXT1(bytes, width, height);
                }
                else if (magic.Contains("DXT5"))
                {
                    reader.BaseStream.Seek(0x80, SeekOrigin.Begin);
                    byte[] bytes = reader.ReadBytes(hxd.sectionData[0].Length - 0x80);
                    btm = DDSImage.UncompressDXT5(bytes, width, height);
                }
                reader.Dispose();
                reader.Close();
                pbx.Size = new Size(width, height);
                pbx.Visible = true;
                pbx.Image = btm;
                metaTab.Background.Controls.Add(pbx);
                isDDS = true;
                return; 
            }

            type = DataMethods.readInt(hxd.sectionData[0], 0x30, 0x1);
            format = DataMethods.readInt(hxd.sectionData[0], 0x1B, 0x1);
            bool halfed = DataMethods.readInt16(hxd.sectionData[0], 0x2C) != 0xFFFF;

            oldwidth = width;
            oldheight = height;

            if (width >= 512) width /= (int)Math.Pow(2, Convert.ToInt32(halfed));
            if (height >= 512) height /= (int)Math.Pow(2, Convert.ToInt32(halfed));

            switch (format)
            {
                case 0x52:
                    {
                        modifier = 1;
                        break;
                    }
                case 0x53:
                    {
                        modifier = 2;
                        break;
                    }
            }
            //MessageBox.Show(modifier + " (mod)");
            switch (format)
            {
                case 0x52:
                    {
                        btm = colorToBitmap(DXT1.getDynamicBlocksDXT1(tempSectionData, width, height, modifier));
                        break;
                    }
                case 0x53:
                    {
                        btm = colorToBitmap(DXT3.getDynamicBlocksDXT3(tempSectionData, width, height));
                        break;
                    }
            }


            pbx.Size = new Size(width, height);
            pbx.Visible = true;
            pbx.Image = btm;
            metaTab.Background.Controls.Add(pbx);
        }

        public Bitmap colorToBitmap(Color[][] colors)
        {
            Bitmap btm = new Bitmap(colors[0].Length, colors.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                for (int j = 0; j < colors[i].Length; j++)
                {
                    btm.SetPixel(j, i, colors[i][j]);
                }
            }
            return btm;
        }

        public int texelCalc(int i)
        {
            int ret = i;
            ret = DataMethods.swapBits(ret, 1, 3);
            ret = DataMethods.swapBits(ret, 2, 1);
            return ret;
        }
    }
}