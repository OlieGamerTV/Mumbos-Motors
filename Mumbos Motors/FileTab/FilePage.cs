using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Data.Common;
using System.Threading;
using Mumbos_Motors.FileTab;

namespace Mumbos_Motors
{
    class FilePage
    {
        /// <summary>
        /// Construct a tabPage for the file and what the file is about and add it to the main_tabControl
        /// </summary>

        public bool error = false;
        public string fileName;
        CAFF caff;
        MULTICAFF multiCAFF;

        //Main
        public TabPage TabPage_mainpage;
        public TabControl tabControl;

        //Page1
        private FileTab.FileInfoPage fileInfoPage;
        

        //Page2
        private FileTab.TagsCAFF tagsPage;


        //Added a Form1 passthough, I dont know any other way to get the ref ¯\_(ツ)_/¯ -Solar
        //You can get the ref by grabbing the first form from Application.OpenForms. -Olie
        public FilePage(string dir)
        {
            Form1 Form = Application.OpenForms[0] as Form1;
            fileName = Path.GetFileName(dir);

            int headerWord = DataMethods.readInt32(dir, 0x0);
            switch (headerWord)
            {
                case 0x43414646: //CAFF
                    {
                        Form.decompAttempts = 0;
                        caff = new CAFF(dir);
                        TabPage_mainpage = new TabPage();
                        TabPage_mainpage.Text = fileName + "        ";

                        tabControl = new TabControl();
                        tabControl.Width = 1063;
                        tabControl.Height = 605;

                        tabControl.TabPages.Add(new FileTab.FileInfo.InfoCAFF(dir, caff, Form).tabPage);
                        tabControl.TabPages.Add(new FileTab.TagsCAFF(caff).tabPage);

                        //spawn tabcontrol in filepage
                        TabPage_mainpage.Controls.Add(tabControl);
                        break;
                    }

                case 0x438CB47C: //MULTICAFF
                    {
                        Form.decompAttempts = 0;
                        multiCAFF = new MULTICAFF(dir);
                        TabPage_mainpage = new TabPage();
                        TabPage_mainpage.Text = fileName + "        ";

                        tabControl = new TabControl();
                        tabControl.Width = 1063;
                        tabControl.Height = 605;

                        tabControl.TabPages.Add(new FileTab.FileInfo.InfoMULTICAFF(dir, multiCAFF, Form).tabPage);
                        tabControl.TabPages.Add(new FileTab.TagsInfo.TagsMULTICAFF(multiCAFF).tabPage);

                        //spawn tabcontrol in filepage
                        TabPage_mainpage.Controls.Add(tabControl);
                        break;
                    }

                case 0xFF512ED: //XB COMPRESSED FILE
                    {
                        string xboxPath = Environment.ExpandEnvironmentVariables("%XEDK%");

                        //Path is not valid, ether path is missing or you need a restart
                        if (xboxPath == "%XEDK%")
                        {
                            error = true; ;

                            if (MessageBox.Show("You need to install the XBOX 360 SDK and restart windows. Would you like to install the SDK?", "SDK ERROR", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                            {
                                System.Diagnostics.Process.Start("https://www.mediafire.com/file/l9786i9endh5w5e/XBOX360+SDK+21256.3.exe");
                            }
                            
                            caff = new CAFF(dir);
                            break;
                        }
                        else //Path is valid in which case we want to decompress the file and force the Form to load it.
                        {
                            Form.decompAttempts += 1;
                            error = true; ;
                            string fullPath = xboxPath + "\\bin\\win32\\xbdecompress.exe";

                            //Starts up xbdecompress and feeds it the dir and the output as dir_decompressed
                            Process p = new Process();
                            p.StartInfo.FileName = fullPath;
                            string oldDir = dir;
                            string newDir = dir.Replace("_recompressed", "");

                            p.StartInfo.Arguments = "/C \"" + oldDir + "\" \"" + newDir + "_decompressed\"";
                            p.Start();

                            /*
                            We wait until the file has been decompressed, 
                            Might want to add a failsafe here but from my testing as long as you close the xbdecompress.exe cmd, Mumbo should be fine.. 
                            */

                            while (!p.HasExited)
                            {
                            }

                            if(p.ExitCode == 0) // An exit code of 0 means it was successful.
                            {
                                Form.decompAttempts = 0;
                                error = true; ;

                                newDir += "_decompressed";
                                Form.ForceLoadFile(newDir);
                            }
                            else if(p.ExitCode == 1) // An exit code of 1 means that something went wrong.
                            {
                                if(Form.decompAttempts > 30)
                                {
                                    MessageBox.Show("Failed to decompress " + Path.GetFileName(fileName) + ".\n" + DataMethods.readString(dir, 0x0, 0x4));
                                    Form.decompAttempts = 0;
                                }
                                else
                                {
                                    Form.ForceLoadFile(newDir);
                                }
                            }
                            //error = true; ;
                            //caff = new CAFF(dir);

                            //Set the new Dir to the decompressed file
                            //newDir += "_decompressed";

                            //If the file Exist aka if xbdecompress.exe did its job, then we load up the file.

                            break;
                        }

                        //Xbox Path was Valid but HUH
                        error = true; ;
                        MessageBox.Show("Could not find xbdecompress at: " + xboxPath);
                        caff = new CAFF(dir);
                        break;

                    }
                case 0x3026B275: // XMV FILE
                    error = true;
                    MessageBox.Show("Error opening: " + Path.GetFileName(fileName) + "\n" + "File supplied is a Xbox Media Video (XMV) file.");
                    caff = new CAFF(dir);
                    break;
                default:
                    {
                        error = true; ;
                        MessageBox.Show("Error opening: " + Path.GetFileName(fileName) + "\n" + DataMethods.readString(dir, 0x0, 0x4));
                        caff = new CAFF(dir);
                        break;
                    }
            }
        }

        public void DisposeCaff()
        {
            if (caff != null) { caff.Dispose(); }
            if (multiCAFF != null) { multiCAFF.Dispose(); }
        }

        public bool getError()
        {
            return error;
        }

        public bool checkCaffFile()
        {
            if (caff.getError())
            {
                MessageBox.Show("There was a problem reading this CAFF file:\n - " + caff.getErrorMessage());
            }
            return caff.getError();
        }
    }
}
