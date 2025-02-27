using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mumbos_Motors
{
    struct VehiclePart
    {
        public int[] pos;
        public int mod;
        public int ident;
        public float[] rot;
        public int[] RGBA;
    }
    public class vehicle : ModdingTab
    {
        int[] partIdents;
        string[] partNames;
        VehiclePart[] parts;
        int section = 0;
        int nameStart = 0x20;
        string name;
        int dataStart = 0x7C;
        int dataStartSave = 0x84;
        int numParts;
        int numPartsSave;
        int partSize = 0x24;

        public vehicle(CAFF caff, int fileID) : base(caff, fileID)
        {
            buildMetaPage();
        }

        public vehicle(MULTICAFF multiCaff, int caffIndex, int symbolID) : base(multiCaff, caffIndex, symbolID)
        {
            buildMetaPage();
        }

        public override void buildMetaPage()
        {
            VehicleParts.readPartStore(out partIdents, out partNames);
            readVehicle();
            string nodeName = "Vehicle";
            createNode(nodeName, numParts);

            int p = dataStart;
            for (int i = 0; i < parts.Length; i++)
            {
                int ident = DataMethods.readInt32(hxd.sectionData[section], p + 0x8);
                string name = DataMethods.readString(hxd.sectionData[section], 0x20, 0x40);
                MetaBlock_String("Vehicle Name:", 1, 0, 0, nodeName, i);
                EditLastStringBox(name, 410);
                MetaBlock_Text("LEFT dist (X):", 1, p + 0x1, 0x1, 0, nodeName, i);
                MetaBlock_Text("FRONT dist (Y):", 1, p + 0x2, 0x1, 0, nodeName, i);
                MetaBlock_Text("TOP dist (Z):", 1, p + 0x3, 0x1, 0, nodeName, i);
                MetaBlock_Text("Modifier:", 1, p + 0x5, 0x2, 0, nodeName, i);
                MetaBlock_Combo_Custom("Ident:", 1, p + 0x8, 0x4, nodeName, i);
                metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - 1].comboBox = createIdentList(metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - 1].comboBox, ident, i);
                metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - 1].comboBox.DropDownClosed += new EventHandler(comboBoxIdents_update);
                metaTab.metaTagRefs_Custom[metaTab.metaTagRefs_Custom.Count - 1].comboBox.SelectedIndexChanged += new EventHandler(comboBoxIdents_update);
                MetaBlock_Text("Yaw:", 1, p + 0xC, 0x4, 2, nodeName, i);
                MetaBlock_Text("Pitch:", 1, p + 0x10, 0x4, 2, nodeName, i);
                MetaBlock_Text("Roll:", 1, p + 0x14, 0x4, 2, nodeName, i);
                MetaBlock_Text("Red:", 1, p + 0x18, 0x1, 0, nodeName, i);
                MetaBlock_Text("Green:", 1, p + 0x19, 0x1, 0, nodeName, i);
                MetaBlock_Text("Blue:", 1, p + 0x20, 0x1, 0, nodeName, i);
                MetaBlock_Text("Alpha:", 1, p + 0x21, 0x1, 0, nodeName, i);
                p += partSize;
            }
            bottomToolBar.addButton("Extract Vehicle");
            bottomToolBar.buttons[bottomToolBar.buttons.Count - 1].Click += new EventHandler(export_vehicle);
            bottomToolBar.addButton("Import Vehicle");
            bottomToolBar.buttons[bottomToolBar.buttons.Count - 1].Click += new EventHandler(import_vehicle);
        }

        public void readVehicle()
        {
            name = DataMethods.readString(hxd.sectionData[0], 0x20);
            numParts = DataMethods.readInt16(hxd.sectionData[section], 0);
            //MessageBox.Show(numParts + "\n" + hxd.sectionData[section][0] + " " + hxd.sectionData[section][1]);
            parts = new VehiclePart[numParts];
            int p = dataStart;
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i].pos = new int[3];
                parts[i].rot = new float[3];
                parts[i].RGBA = new int[4];
                parts[i].pos[0] = hxd.sectionData[section][p];
                parts[i].pos[1] = hxd.sectionData[section][p + 0x1];
                parts[i].pos[2] = hxd.sectionData[section][p + 0x2];
                parts[i].mod = DataMethods.readInt16(hxd.sectionData[section], p + 0x5);
                parts[i].ident = DataMethods.readInt32(hxd.sectionData[section], p + 0x8);
                parts[i].rot[0] = DataMethods.readFloat32(hxd.sectionData[section], p + 0xC);
                parts[i].rot[1] = DataMethods.readFloat32(hxd.sectionData[section], p + 0x10);
                parts[i].rot[2] = DataMethods.readFloat32(hxd.sectionData[section], p + 0x14);
                parts[i].RGBA[0] = hxd.sectionData[section][p + 0x18]; //red
                parts[i].RGBA[1] = hxd.sectionData[section][p + 0x19]; //green
                parts[i].RGBA[2] = hxd.sectionData[section][p + 0x20]; //blue
                parts[i].RGBA[3] = hxd.sectionData[section][p + 0x21]; //alpha
                p += partSize;
            }
        }

        public ComboBox createIdentList(ComboBox box, int ident, int blockID)
        {
            box.Name = blockID + "";
            box.Items.Clear();
            for (int i = 0; i < partNames.Length; i++)
            {
                box.Items.Add(partNames[i]);
            }
            box.Text = getPartNameFromIdent(ident);
            return box;
        }

        public string getPartNameFromIdent(int ident)
        {
            for (int i = 0; i < partIdents.Length; i++)
            {
                if (partIdents[i] == ident)
                {
                    return partNames[i];
                }
            }
            return "unknown";
        }

        public int getIdentFromPartName(string name)
        {
            for (int i = 0; i < partIdents.Length; i++)
            {
                if (partNames[i] == name)
                {
                    return i;
                }
            }
            return 0;
        }

        void comboBoxIdents_update(object sender, EventArgs e)
        {
            ComboBox box = sender as ComboBox;
            int index = getIdentFromPartName(box.Text);
            int blockID = Convert.ToInt32(box.Name);
            metaTab.metaTagRefs_Custom[blockID].textBox.Text = partIdents[index].ToString("X8");
        }

        void calcNumPartsSave(byte[] saveData)
        {
            numPartsSave = (saveData.Length - 0x84) / partSize;
        }

        void export_vehicle(object sender, EventArgs e)
        {
            byte[] exp = {
                0x3F, 0x9A, 0xE1, 0x48, 0x40, 0x54, 0x7A, 0xE1, 0x00, 0x00, 0x01, 0x00, 0x43, 0x3E, 0x00, 0x00,
                0x45, 0x88, 0xE2, 0x2A, 0x44, 0xC8, 0x00, 0x00, 0x45, 0x2F, 0x50, 0x01, 0x43, 0x5D, 0x8E, 0x39,
                0x00, 0x00, 0x0D, 0xD8, 0x00, 0x00, 0x00, 0x01
            };
            byte[] Exp = new byte[exp.Length + 0x5C + (parts.Length * partSize)];
            exp.CopyTo(Exp, 0);
            Exp[0x80] = 1;

            //name
            int p = 0x28;
            for (int i = nameStart; i < nameStart + name.Length; i++)
            {
                Exp = DataMethods.writeInt16(Exp, p, Convert.ToInt32(name[i - nameStart]));
                p += 2;
            }

            Exp = DataMethods.writeInt16(Exp, 0x8, numParts);
            //Copy remaining data;
            p = dataStartSave;
            for (int i = dataStart; i < hxd.sectionData[0].Length; i++)
            {
                Exp[p] = hxd.sectionData[0][i];
                p++;
            }
            DataMethods.saveFileDialog(Exp, "00000000", "Content of package");
        }
        void import_vehicle(object sender, EventArgs e)
        {
            byte[] imp = DataMethods.openFileDialog("Vehicle save content");
            try
            {
                calcNumPartsSave(imp);
                byte[] Imp = new byte[dataStart + (numPartsSave * partSize)];
                Array.Copy(hxd.sectionData[section], Imp, dataStart);

                Imp = DataMethods.writeInt16(Imp, 0, numPartsSave);
                int p = dataStartSave;
                for (int i = dataStart; i < Imp.Length; i++)
                {
                    Imp[i] = imp[p];
                    p++;
                }
                hxd.sectionData[section] = new byte[Imp.Length];
                Imp.CopyTo(hxd.sectionData[section], 0);

                hxd.updateHexTextBox_partial(section);
            }


            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
