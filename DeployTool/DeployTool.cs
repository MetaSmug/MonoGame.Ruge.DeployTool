using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using ZipFile = System.IO.Compression.ZipFile;

namespace RugeDeployTool {
    public partial class DeployTool : Form {

        private string libPath;

        public DeployTool(string[] args) {
            InitializeComponent();
            
            version.Text += FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            var subVersion = version.Text.Substring(version.Text.LastIndexOf(".") + 1);
            if (subVersion == "0") version.Text = version.Text.Substring(0, version.Text.LastIndexOf("."));
            subVersion = version.Text.Substring(version.Text.LastIndexOf(".") + 1);
            if (subVersion == "0") version.Text = version.Text.Substring(0, version.Text.LastIndexOf("."));

            libPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ruge Deploy Tool\\lib";

            if (!Directory.Exists(libPath)) MessageBox.Show(this, "Library Missing from %AppData%");

            groupBox1.Paint += groupBox_Paint;
            groupBox2.Paint += groupBox_Paint;
            groupBox3.Paint += groupBox_Paint;
            groupBox4.Paint += groupBox_Paint;
            groupBox7.Paint += groupBox_Paint;

            if (args.Length > 0) OpenFile(args[0]);
        }




        private void groupBox_Paint(object sender, PaintEventArgs e) {
            GroupBox box = sender as GroupBox;
            DrawGroupBox(box, e.Graphics, Color.OrangeRed, Color.OrangeRed);
        }

        private void DrawGroupBox(GroupBox box, Graphics g, Color textColor, Color borderColor) {
            if (box != null) {
                Brush textBrush = new SolidBrush(textColor);
                Brush borderBrush = new SolidBrush(borderColor);
                Pen borderPen = new Pen(borderBrush);
                SizeF strSize = g.MeasureString(box.Text, box.Font);
                Rectangle rect = new Rectangle(box.ClientRectangle.X,
                                               box.ClientRectangle.Y + (int)(strSize.Height / 2),
                                               box.ClientRectangle.Width - 1,
                                               box.ClientRectangle.Height - (int)(strSize.Height / 2) - 1);

                // Clear text and border
                g.Clear(BackColor);

                // Draw text
                g.DrawString(box.Text, box.Font, textBrush, box.Padding.Left, 0);

                // Drawing Border
                //Left
                g.DrawLine(borderPen, rect.Location, new Point(rect.X, rect.Y + rect.Height));
                //Right
                g.DrawLine(borderPen, new Point(rect.X + rect.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height));
                //Bottom
                g.DrawLine(borderPen, new Point(rect.X, rect.Y + rect.Height), new Point(rect.X + rect.Width, rect.Y + rect.Height));
                //Top1
                g.DrawLine(borderPen, new Point(rect.X, rect.Y), new Point(rect.X + box.Padding.Left, rect.Y));
                //Top2
                g.DrawLine(borderPen, new Point(rect.X + box.Padding.Left + (int)(strSize.Width), rect.Y), new Point(rect.X + rect.Width, rect.Y));

            }
        }

        private void btnProjectFile_Click(object sender, System.EventArgs e) {
            openFileDialog1.Filter = "AssemblyInfo.cs File|AssemblyInfo.cs";
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) {
                textAssemblyFile.Text = openFileDialog1.FileName;
                RefreshMe();
            }
            openFileDialog1.FileName = "";
        }

        private void RefreshMe() {

            var readText = File.ReadAllLines(textAssemblyFile.Text);
            var infoLines = readText.Where(t => t.Contains("[assembly: AssemblyFileVersion"));
            foreach (var item in infoLines) {
                var version = item.Substring(item.IndexOf('(') + 2, item.LastIndexOf(')') - item.IndexOf('(') - 3);
                var subVersion = version.Substring(version.LastIndexOf(".") + 1);
                if (subVersion == "0") version = version.Substring(0, version.LastIndexOf("."));
                subVersion = version.Substring(version.LastIndexOf(".") + 1);
                if (subVersion == "0") version = version.Substring(0, version.LastIndexOf("."));
                textVersion.Text = version;
            }
            infoLines = readText.Where(t => t.Contains("[assembly: AssemblyProduct"));
            foreach (var item in infoLines) {
                textNamespace.Text = item.Substring(item.IndexOf('(') + 2, item.LastIndexOf(')') - item.IndexOf('(') - 3);
            }
            infoLines = readText.Where(t => t.Contains("[assembly: AssemblyTitle"));
            foreach (var item in infoLines) {
                textTitle.Text = item.Substring(item.IndexOf('(') + 2, item.LastIndexOf(')') - item.IndexOf('(') - 3);
            }
            infoLines = readText.Where(t => t.Contains("[assembly: AssemblyCompany"));
            foreach (var item in infoLines) {
                textCompany.Text = item.Substring(item.IndexOf('(') + 2, item.LastIndexOf(')') - item.IndexOf('(') - 3);
            }

        }

        private void OpenFile(string filename) {
            
            ClearData();

            Text = "Ruge Deploy Tool - " + filename;

            var textReader = new XmlTextReader(filename);
            textReader.ReadToFollowing("ShowConsole");
            chkShowConsole.Checked = bool.Parse(textReader.ReadElementContentAsString());
            textReader.ReadToFollowing("Demo");
            chkDemo.Checked = bool.Parse(textReader.ReadElementContentAsString());
            textReader.ReadToFollowing("DemoTag");
            textDemoTag.Text = textReader.ReadElementContentAsString();

            // ProjectSettings
            textReader.ReadToFollowing("AssemblyInfo");
            textAssemblyFile.Text = textReader.ReadElementContentAsString();
            textReader.ReadToFollowing("ReleaseFolder");
            textReleaseFolder.Text = textReader.ReadElementContentAsString();
            textReader.ReadToFollowing("DeployFolder");
            textDeployFolder.Text = textReader.ReadElementContentAsString();

            // SourceSettings
            textReader.ReadToFollowing("SourceFolder");
            textSourceFolder.Text = textReader.ReadElementContentAsString();
            textReader.ReadToFollowing("IgnoreFolders");
            textIgnoreFolders.Text = textReader.ReadElementContentAsString();
            textReader.ReadToFollowing("SourceChecked");
            chkDeploySource.Checked = bool.Parse(textReader.ReadElementContentAsString());

            // WindowsSettings
            textReader.ReadToFollowing("InnoSetupBinary");
            textInnoSetupBinary.Text = textReader.ReadElementContentAsString();
            textReader.ReadToFollowing("SetupGUID");
            textSetupGUID.Text = textReader.ReadElementContentAsString();
            textReader.ReadToFollowing("IconWindows");
            textIconWindows.Text = textReader.ReadElementContentAsString();
            textReader.ReadToFollowing("DeployWinSetup");
            chkDeployWinSetup.Checked = bool.Parse(textReader.ReadElementContentAsString());
            textReader.ReadToFollowing("DeployWinPortable");
            chkDeployWinPortable.Checked = bool.Parse(textReader.ReadElementContentAsString());

            // LinuxSettings
            textReader.ReadToFollowing("DeployLinux");
            chkDeployLinux.Checked = bool.Parse(textReader.ReadElementContentAsString());

            // OSXSettings
            textReader.ReadToFollowing("IconOSX");
            textIconOSX.Text = textReader.ReadElementContentAsString();
            textReader.ReadToFollowing("DeployOSX");
            chkDeployOSX.Checked = bool.Parse(textReader.ReadElementContentAsString());

            textReader.Close();

            RefreshMe();
        }

        private void btnOpen_Click(object sender, System.EventArgs e) {
            openFileDialog1.Filter = "DeployTool Files|*.dt";
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) { 
                OpenFile(openFileDialog1.FileName);
            }
        }

        private void btnSave_Click(object sender, System.EventArgs e) {
            saveFileDialog1.Filter = "DeployTool Files|*.dt";
            var result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK) {

                var sourceChecked = chkDeploySource.Checked ? "true" : "false";
                var showConsole = chkShowConsole.Checked ? "true" : "false";
                var deployWinSetup = chkDeployWinSetup.Checked ? "true" : "false";
                var deployWinPortable = chkDeployWinPortable.Checked ? "true" : "false";
                var deployLinux = chkDeployLinux.Checked ? "true" : "false";
                var deployOSX = chkDeployOSX.Checked ? "true" : "false";
                var demo = chkDemo.Checked ? "true" : "false";

                var textWriter = new XmlTextWriter(saveFileDialog1.FileName, null) {Formatting = Formatting.Indented};
                textWriter.WriteStartDocument();
                textWriter.WriteStartElement("Settings");
                    textWriter.WriteStartElement("ShowConsole");
                        textWriter.WriteString(showConsole);
                    textWriter.WriteEndElement();
                    textWriter.WriteStartElement("Demo");
                        textWriter.WriteString(demo);
                    textWriter.WriteEndElement();
                    textWriter.WriteStartElement("DemoTag");
                        textWriter.WriteString(textDemoTag.Text);
                    textWriter.WriteEndElement();
                    textWriter.WriteStartElement("ProjectSettings");
                        textWriter.WriteStartElement("AssemblyInfo");
                            textWriter.WriteString(textAssemblyFile.Text);
                        textWriter.WriteEndElement();
                        textWriter.WriteStartElement("ReleaseFolder");
                            textWriter.WriteString(textReleaseFolder.Text);
                        textWriter.WriteEndElement();
                        textWriter.WriteStartElement("DeployFolder");
                            textWriter.WriteString(textDeployFolder.Text);
                        textWriter.WriteEndElement();
                    textWriter.WriteEndElement();
                    textWriter.WriteStartElement("SourceSettings");
                        textWriter.WriteStartElement("SourceFolder");
                            textWriter.WriteString(textSourceFolder.Text);
                        textWriter.WriteEndElement();
                        textWriter.WriteStartElement("IgnoreFolders");
                            textWriter.WriteString(textIgnoreFolders.Text);
                        textWriter.WriteEndElement();
                        textWriter.WriteStartElement("SourceChecked");
                            textWriter.WriteString(sourceChecked);
                        textWriter.WriteEndElement();
                    textWriter.WriteEndElement();
                    textWriter.WriteStartElement("WindowsSettings");
                        textWriter.WriteStartElement("InnoSetupBinary");
                            textWriter.WriteString(textInnoSetupBinary.Text);
                        textWriter.WriteEndElement();
                        textWriter.WriteStartElement("SetupGUID");
                            textWriter.WriteString(textSetupGUID.Text);
                        textWriter.WriteEndElement();
                        textWriter.WriteStartElement("IconWindows");
                            textWriter.WriteString(textIconWindows.Text);
                        textWriter.WriteEndElement();
                        textWriter.WriteStartElement("DeployWinSetup");
                            textWriter.WriteString(deployWinSetup);
                        textWriter.WriteEndElement();
                        textWriter.WriteStartElement("DeployWinPortable");
                            textWriter.WriteString(deployWinPortable);
                        textWriter.WriteEndElement();
                    textWriter.WriteEndElement();
                    textWriter.WriteStartElement("LinuxSettings");
                        textWriter.WriteStartElement("DeployLinux");
                            textWriter.WriteString(deployLinux);
                        textWriter.WriteEndElement();
                    textWriter.WriteEndElement();
                    textWriter.WriteStartElement("OSXSettings");
                        textWriter.WriteStartElement("IconOSX");
                            textWriter.WriteString(textIconOSX.Text);
                        textWriter.WriteEndElement();
                        textWriter.WriteStartElement("DeployOSX");
                            textWriter.WriteString(deployOSX);
                        textWriter.WriteEndElement();
                    textWriter.WriteEndElement();
                textWriter.WriteEndElement();
                textWriter.WriteEndDocument();
                textWriter.Close();

            }
        }

        private void ClearData() {
            Text = "Ruge Deploy Tool";
            textAssemblyFile.Text = "";
            textDeployFolder.Text = "";
            textReleaseFolder.Text = "";
            textNamespace.Text = "";
            textVersion.Text = "";
            textTitle.Text = "";
            textCompany.Text = "";
            chkShowConsole.Checked = false;
            textSourceFolder.Text = "";
            textIgnoreFolders.Text = "";
            chkDeploySource.Checked = false;
            textInnoSetupBinary.Text = "";
            textSetupGUID.Text = "";
            chkDeployWinSetup.Checked = false;
            chkDeployWinPortable.Checked = false;
            chkDeployLinux.Checked = false;
        }

        private void btnReleaseFolder_Click(object sender, EventArgs e) {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK) textReleaseFolder.Text = folderBrowserDialog1.SelectedPath;
        }
        private void btnDeployFolder_Click(object sender, EventArgs e) {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK) textDeployFolder.Text = folderBrowserDialog1.SelectedPath;
        }

        private void btnRefresh_Click(object sender, EventArgs e) {
            RefreshMe();
        }

        private void btnDeploy_Click(object sender, EventArgs e) {
            var console = new Console();
            
            console.WriteLine("Library Path:" + libPath);

            if (chkShowConsole.Checked) console.Show();
            if (chkDeploySource.Checked) generateSource(console);
            if (chkDeployWinSetup.Checked) generateWindowsSetup(console);
            if (chkDeployWinPortable.Checked) generateWindowsPortable(console);
            if (chkDeployLinux.Checked) generateLinux(console);
            if (chkDeployOSX.Checked) generateOSX(console);

        }

        private void generateLinux(Console console) {

            console.WriteLine("==================================");
            console.WriteLine("Generating Linux Deploy...");

            var zipPath = textDeployFolder.Text + "\\" + textNamespace.Text + "." + textVersion.Text;
            if (chkDemo.Checked) zipPath += "." + textDemoTag.Text;
            zipPath += ".Linux.tgz";

            var tempPath = textDeployFolder.Text + "\\Linux\\" + textNamespace.Text;

            var files = Directory.GetFiles(libPath + "\\Kick", "*.*", SearchOption.AllDirectories);
            foreach (var file in files) {

                var fileRelative = file;
                fileRelative = fileRelative.Replace(libPath + "\\Kick", "");
                var fileDir = new FileInfo(tempPath + fileRelative);
                fileDir.Directory.Create();

                File.Copy(file, tempPath + fileRelative, true);

                console.WriteLine("Copy To Temp: " + file);
            }

            files = Directory.GetFiles(textReleaseFolder.Text, "*.*", SearchOption.AllDirectories);
            foreach (var file in files) {

                var fileRelative = file;
                fileRelative = fileRelative.Replace(textReleaseFolder.Text, "");
                var fileDir = new FileInfo(tempPath + fileRelative);
                fileDir.Directory.Create();

                console.WriteLine("Copy To Temp: " + file);

                // Kick doesn't like this file and it doesn't seem to break anything to leave it out
                if (!file.Contains(".exe.config")) File.Copy(file, tempPath + fileRelative, true);

            }
            
            var fileDump = File.ReadAllText(tempPath + "\\Kick");
            fileDump = fileDump.Replace("{{filename}}", textNamespace.Text);


            console.WriteLine("Generating Kick Script..." + tempPath + "\\" + textNamespace.Text);
            File.WriteAllText(tempPath + "\\" + textNamespace.Text, fileDump);

            File.Delete(tempPath + "\\Kick");
            File.Move(tempPath + "\\kick.bin.x86", tempPath + "\\" + textNamespace.Text + ".bin.x86");
            File.Move(tempPath + "\\kick.bin.x86_64", tempPath + "\\" + textNamespace.Text + ".bin.x86_64");

            console.WriteLine("Creating TGZ File..." + zipPath);
            if (File.Exists(zipPath)) File.Delete(zipPath);
            CreateTarGZ(zipPath, textDeployFolder.Text + "\\Linux");

            console.Write("Deleting Temp Folder...");
            Directory.Delete(textDeployFolder.Text + "\\Linux", true);
            console.WriteLine("OK");

        }

        private void generateOSX(Console console) {

            console.WriteLine("==================================");
            console.WriteLine("Generating OSX.app Deploy...");

            var zipPath = textDeployFolder.Text + "\\" + textNamespace.Text + "." + textVersion.Text;
            if (chkDemo.Checked) zipPath += "." + textDemoTag.Text;
            zipPath += ".OSX.tgz";
            var tempPath = textDeployFolder.Text + "\\OSX\\" + textTitle.Text + ".app";

            var files = Directory.GetFiles(libPath + "\\Ruge.app", "*.*", SearchOption.AllDirectories);
            foreach (var file in files) {

                var fileRelative = file;
                fileRelative = fileRelative.Replace(libPath + "\\Ruge.app", "");
                var fileDir = new FileInfo(tempPath + fileRelative);
                fileDir.Directory.Create();

                console.WriteLine("Copy To Temp: " + file);
                File.Copy(file, tempPath + fileRelative, true);

            }


            console.WriteLine("Fixing Info.plist...");
            File.Move(tempPath + "\\Contents\\Info.plist", tempPath + "\\Contents\\Info.plist.temp");
            var fileDump = File.ReadAllText(tempPath + "\\Contents\\Info.plist.temp");
            fileDump = fileDump.Replace("{{displayName}}", textTitle.Text);
            File.WriteAllText(tempPath + "\\Contents\\Info.plist", fileDump);
            File.Delete(tempPath + "\\Contents\\Info.plist.temp");

            fileDump = File.ReadAllText(tempPath + "\\Contents\\MacOS\\Kick");
            fileDump = fileDump.Replace("{{filename}}", textNamespace.Text);
            
            console.WriteLine("Generating Kick Script..." + tempPath + "\\Contents\\MacOS\\" + textTitle.Text);
            File.WriteAllText(tempPath + "\\Contents\\MacOS\\" + textTitle.Text, fileDump);
            
            File.Delete(tempPath + "\\Contents\\MacOS\\Kick");
            File.Move(tempPath + "\\Contents\\MacOS\\kick.osx", tempPath + "\\Contents\\MacOS\\" + textNamespace.Text + ".bin.osx");

            console.WriteLine("Setting Icon...");
            File.Copy(textIconOSX.Text, tempPath + "\\Contents\\Resources\\icon.icns", true);

            console.WriteLine("Copying Files...");

            files = Directory.GetFiles(textReleaseFolder.Text, "*.*", SearchOption.AllDirectories);
            foreach (var file in files) {

                var fileRelative = file;
                fileRelative = fileRelative.Replace(textReleaseFolder.Text, "");
                var fileDir = new FileInfo(tempPath + "\\Contents\\MacOS" + fileRelative);
                fileDir.Directory.Create();

                if (!file.Contains(".exe.config")) File.Copy(file, tempPath + "\\Contents\\MacOS" + fileRelative, true);

                console.WriteLine("Copy To Temp: " + file);
            }

            Directory.Move(tempPath + "\\Contents\\MacOS\\Content", tempPath + "\\Contents\\Resources\\Content");

            console.WriteLine("Creating TGZ File..." + zipPath);
            if (File.Exists(zipPath)) File.Delete(zipPath);
            CreateTarGZ(zipPath, textDeployFolder.Text + "\\OSX");

            console.Write("Deleting Temp Folder...");
            Directory.Delete(textDeployFolder.Text + "\\OSX", true);
            console.WriteLine("OK");
        }

        private void generateWindowsPortable(Console console) {

            console.WriteLine("==================================");
            console.WriteLine("Generating Windows Portable Deploy...");
            var zipPath = textDeployFolder.Text + "\\" + textNamespace.Text + "." + textVersion.Text;
            if (chkDemo.Checked) zipPath += "." + textDemoTag.Text;
            zipPath += ".Windows.Portable.zip";
            var tempPath = textDeployFolder.Text + "\\" + textTitle.Text;

            var files = Directory.GetFiles(textReleaseFolder.Text, "*.*", SearchOption.AllDirectories);
            foreach (var file in files) {

                var fileRelative = file;
                fileRelative = fileRelative.Replace(textReleaseFolder.Text, "");
                var fileDir = new FileInfo(tempPath + fileRelative);
                fileDir.Directory.Create();

                File.Copy(file, tempPath + fileRelative, true);

                console.WriteLine("Copy To Temp: " + file);
            }
            console.WriteLine("Creating Zip File..." + zipPath);
            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(tempPath, zipPath, CompressionLevel.Optimal, true);
            console.Write("Deleting Temp Folder...");
            Directory.Delete(tempPath, true);
            console.WriteLine("OK");

        }

        private void generateWindowsSetup(Console console) {

            console.WriteLine("==================================");
            
            var fileDump = File.ReadAllText(libPath + "\\Setup.iss");
            fileDump = fileDump.Replace("{{displayName}}", textTitle.Text);
            fileDump = fileDump.Replace("{{version}}", textVersion.Text);
            fileDump = fileDump.Replace("{{publisher}}", textCompany.Text);
            fileDump = fileDump.Replace("{{namespace}}", textNamespace.Text);
            fileDump = fileDump.Replace("{{releaseDir}}", textReleaseFolder.Text);
            fileDump = fileDump.Replace("{{deployDir}}", textDeployFolder.Text);
            fileDump = fileDump.Replace("{{installGuid}}", textSetupGUID.Text);
            fileDump = fileDump.Replace("{{iconFile}}", textIconWindows.Text);
            fileDump = fileDump.Replace("{{DEMO}}", chkDemo.Checked ? "." + textDemoTag.Text : "");

            console.WriteLine("Generating Inno Setup Script...");
            File.WriteAllText(textDeployFolder.Text + "\\Setup.iss", fileDump);

            console.WriteLine("Generating Windows Setup EXE...");

            System.Diagnostics.Process.Start(textInnoSetupBinary.Text, "/cc \"" + textDeployFolder.Text + "\\Setup.iss\"");
            
            // can't delete Setup.iss b/c there's no logic to determine that the compiler is done with it yet.

        }

        private void generateSource(Console console) {

            console.WriteLine("==================================");
            console.WriteLine("Generating Source Code Archive...");
            var zipPath = textDeployFolder.Text + "\\" + textNamespace.Text + "." + textVersion.Text;
            if (chkDemo.Checked) zipPath += "." + textDemoTag.Text;
            zipPath += ".Source.tgz";
            var tempPath = textDeployFolder.Text + "\\Source\\" + textTitle.Text;
                   
            string[] ignore = textIgnoreFolders.Text.Split(',').Select(sValue => sValue.Trim()).ToArray();

            var files = Directory.GetFiles(textSourceFolder.Text, "*.*", SearchOption.AllDirectories);
            foreach (var file in files) {

                var skipFile = false;

                foreach (var ignoreMe in ignore) {

                    if (file.Contains("\\" + ignoreMe)) skipFile = true;

                }
                if (!skipFile) {

                    var fileRelative = file;
                    fileRelative = fileRelative.Replace(textSourceFolder.Text, "");
                    var fileDir = new FileInfo(tempPath + fileRelative);
                    fileDir.Directory.Create();

                    File.Copy(file, tempPath + fileRelative, true);
                    
                    console.WriteLine("Copy To Temp: " + file);
                }

            }

            console.WriteLine("Creating TGZ File..." + zipPath);
            if (File.Exists(zipPath)) File.Delete(zipPath);
            CreateTarGZ(zipPath, textDeployFolder.Text + "\\Source");

            console.Write("Deleting Temp Folder...");
            Directory.Delete(textDeployFolder.Text + "\\Source", true);
            console.WriteLine("OK");

        }

        private void btnSourceFolder_Click(object sender, EventArgs e) {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK) textSourceFolder.Text = folderBrowserDialog1.SelectedPath;

        }

        private void btnNewGUID_Click(object sender, EventArgs e) {
            textSetupGUID.Text = Guid.NewGuid().ToString();
        }

        private void btnInnoSetupBinary_Click(object sender, EventArgs e) {
            openFileDialog1.Filter = "Inno Setup Compiler|compil32.exe";
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) textInnoSetupBinary.Text = openFileDialog1.FileName;
        }

        private void btnWinIcon_Click(object sender, EventArgs e) {
            openFileDialog1.Filter = "Windows Icon File|*.ico";
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) textIconWindows.Text = openFileDialog1.FileName;
        }

        private void btnIconOSX_Click(object sender, EventArgs e) {
            openFileDialog1.Filter = "OSX Icon File|*.icns";
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) textIconOSX.Text = openFileDialog1.FileName;
        }


        private void CreateTarGZ(string tgzFilename, string sourceDirectory) {

            Stream outStream = File.Create(tgzFilename);
            Stream gzoStream = new GZipOutputStream(outStream);
            TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);
            
            tarArchive.RootPath = sourceDirectory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, true);

            tarArchive.Close();
        }


    }
}
