using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RugeDeployTool {
    public partial class Console : Form {
        public Console() {
            InitializeComponent();
        }

        public void WriteLine(string text) { textConsole.AppendText(text + "\r\n"); }
        public void Write(string text) { textConsole.AppendText(text); }
        public void Clear() { textConsole.Text = ""; }

    }
}
