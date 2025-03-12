using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mumbos_Motors.ModdingInfo
{
    public class sound : ModdingTab
    {
        string soundName;
        int DNBWIndex;
        byte[][] sectionData;

        public sound(string soundName, MULTICAFF multiCAFF, byte[][] sectionData) : base (multiCAFF.dnbws[multiCAFF.getDNBWIndexByName(soundName)], soundName + ".xwb", sectionData, multiCAFF.getDNBWIndexByName(soundName))
        {
            this.multiCaff = multiCAFF;
            dnbwIndex = multiCAFF.getDNBWIndexByName(soundName);
            this.soundName = soundName;
            this.sectionData = sectionData;

            buildMetaPage();
        }
        public override void buildMetaPage()
        {
            bottomToolBar.addButton("Import Sound");
            bottomToolBar.buttons[bottomToolBar.buttons.Count - 1].Click += new EventHandler(import_sound);
            bottomToolBar.buttons[0].Click += new EventHandler(save);
        }

        void import_sound(object sender, EventArgs e)
        {
            byte[] data = DataMethods.openFileDialog(".wav files (16 bit PCM)");
            if (data.Length != 0)
            {
                byte[] samples = new byte[data.Length - 0x104];
                for (int i = 0x40; i < data.Length - 0xC4; i++)
                {
                    samples[i - 0x40] = data[i];
                }
                if (samples.Length < multiCaff.dnbws[DNBWIndex].len && samples.Length % 2 == 0)
                {
                    samples = DataMethods.swapEndianness(samples, 2);
                    for (int i = 0x9C0; i < (samples.Length - 0x9C0); i++)
                    {
                        hxd.sectionData[0][i] = samples[i - 0x9C0];
                    }
                    metaTab.metaChanged[0] = true;
                    MessageBox.Show("Sound imported successfully. Don't forget to save it :)\n\n");
                    //Status: samples.Length + " < " + multiCAFF.dnbws[DNBWIndex].name + "   " + DNBWIndex
                }
                else
                {
                    MessageBox.Show(".wav too big");
                }
            }
        }

        void save(object sender, EventArgs e)
        {
            DataMethods.writeDataSection(multiCaff.path, multiCaff.dnbws[DNBWIndex].offs, hxd.sectionData[0].Length, hxd.sectionData[0]);
            HexInfo.HexEditor.SaySaved();
        }
    }
}
