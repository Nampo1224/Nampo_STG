using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nampo_STG
{
    public partial class Form1 : Form
    {
        NampoSpace.GameMaster gm;
        Form1 fm;

        public Form1()
        {
            InitializeComponent();
            fm = this;
            gm = new NampoSpace.GameMaster(new NampoSpace.DrawTool(fm));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void Clock_Tick(object sender, EventArgs e)
        {
            gm.Run();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            gm.UserInterface.KeyDown(e);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            gm.UserInterface.KeyUp(e);
        }
    }
}
