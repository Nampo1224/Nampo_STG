using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Drawing;
using System.Numerics;


namespace Nampo_STG
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}

namespace NampoSpace
{
    class GameMaster {

        DrawTool DrawTool;
        public UserInterface UserInterface;

        //testキャラ
        testCharacter nampo,nampo1,nampo2;

        public GameMaster(DrawTool drawTool)
        {
            this.DrawTool = drawTool;
            UserInterface = new UserInterface();

            //DrawTool.AddLabel("@");
            //DrawTool.Draw("@", new Vector2(200, 100));
            nampo = new testCharacter(DrawTool, "@", new Vector2(0, 0), new Vector2(1, 1));
            nampo1 = new testCharacter(DrawTool, "#", new Vector2(10, 10), new Vector2(2, 1));
            nampo2 = new testCharacter(DrawTool, "P", new Vector2(20, 20), new Vector2(1, 2));
        }

        public void Run()
        {
            nampo.Run(UserInterface);
            nampo.Draw();
            nampo1.Run();
            nampo1.Draw();
            nampo2.Run();
            nampo2.Draw();
        }
    }
    abstract class NumTask {
        public NumTask PreTask { get; set; }
        public NumTask NextTask { get; set; }

        public NumTask()
        {
            PreTask = null;
            NextTask = null;
        }

        public NumTask(NumTask pretask,NumTask nexttask)
        {
            this.PreTask = pretask;
            this.NextTask = nexttask;
        }

        public void Add(NumTask addtask)
        {
            addtask.NextTask = this.NextTask;
            addtask.PreTask = this;
            if (this.NextTask != null)
            {
                this.NextTask.PreTask = addtask;
            }
            this.NextTask = addtask;           

        }

        public void Remove()
        {
            this.PreTask.NextTask = this.NextTask;
            if (this.NextTask != null)
            {
                this.NextTask.PreTask = this.PreTask;
            }
        }

    }
    abstract class GameObject : NumTask {
        public abstract void Run();
        public abstract void Draw();
    }

    //NumTaskのテスト用
    /*
    class Test : GameObject
    {
        string moji;

        public Test (string moji)
        {
            this.moji = moji;
        }

        public override void Run()
        {
            for(int i = 0; i < 4; i++)
            {
                this.Add(new Test(i.ToString()));
            }
        }

        public override void Draw()
        {
            MessageBox.Show("moji = " + moji);
        }
    }*/

    class Scene : GameObject {
        public override void Run() {
            
        }
        public override void Draw() {

        }
    }
    abstract class MoveObject : GameObject {
        public DrawTool DrawTool { get; set; }
        public String Picture { get; set; }
        public Vector2 Point { get; set; }
        public Vector2 Vector { get; set; }

    }

    //テスト用キャラクター
    class testCharacter : MoveObject
    {

        public testCharacter(DrawTool drawTool,String pic,Vector2 point,Vector2 vec)
        {
            this.DrawTool = drawTool;
            this.Picture = pic;
            this.Point = point;
            this.Vector = vec;

            DrawTool.AddLabel(this.Picture);
            DrawTool.Draw(this.Picture, this.Point);
        }

        public override void Run()
        {
            Point = Point + Vector;
        }
        public void Run(UserInterface ui)
        {
            switch (ui.command)
            {
                case 0x20://↑
                    Point = Point + new Vector2(0,-2);
                    break;

                case 0x80://←
                    Point = Point + new Vector2(-2,0);
                    break;

                case 0x10://↓
                    Point = Point + new Vector2(0, +2);
                    break;

                case 0x40://→
                    Point = Point + new Vector2(+2, 0);
                    break;
            }
        }
        public override void Draw()
        {
            DrawTool.Draw(Picture, Point);
        }

        public void Shot() { }
    }
    /*
    class Character : MoveObject
    {
       
        public override void Run()
        {

        }
        public override void Draw()
        {

        }
    }
    */

    class DrawTool
    {
        public Form form;

        public DrawTool(Form form)
        {
            this.form = form;

        }

        public void AddLabel(string moji)
        {
            // Create an instance of a Label.
            Label label1 = new Label();

            // Set the border to a three-dimensional border.
            label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            // Set the ImageList to use for displaying an image.
            //label1.ImageList = imageList1;
            // Use the second image in imageList1.
            //label1.ImageIndex = 1;
            // Align the image to the top left corner.
            label1.ImageAlign = ContentAlignment.TopLeft;

            // Specify that the text can display mnemonic characters.
            label1.UseMnemonic = true;
            // Set the text of the control and specify a mnemonic character.
            label1.Text = moji;
            //文字のフォントとフォントサイズ変更
            label1.Font = new Font(label1.Font.FontFamily, 20);

            label1.Name = moji;

            /* Set the size of the control based on the PreferredHeight and PreferredWidth values. */
            label1.Size = new Size(label1.PreferredWidth, label1.PreferredHeight);

            form.Controls.Add(label1);
        }

        public void Draw(String moji,Vector2 point)
        {
            foreach (var item in this.form.Controls.Find(moji, true))
            {
                item.Location = new Point((int)point.X, (int)point.Y);
            }
        }

    }

    //UserInterface.Command は１バイトでボタンをあらわす。
    //左から「←」「→」「↑」「↓」
    //      「Xボタン」「Yボタン」「Bボタン」「Aボタン」
    //0000 0000
    //
    // ex: 0110 0011の場合は
    //                  「→」「↑」 「Bボタン」「Aボタン」が押されている。
    class UserInterface
    {
        public byte command { get; set; }

        public UserInterface()
        {
            command = 0x00;
        }

        public void KeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                    command = (byte)(command | 0x20);//↑
                    break;

                case Keys.A:
                    command = (byte)(command | 0x80);//←
                    break;

                case Keys.S:
                    command = (byte)(command | 0x10);//↓
                    break;

                case Keys.D:
                    command = (byte)(command | 0x40);//→
                    break;

                case Keys.Space:
                    command = (byte)(command | 0x01);//Aボタン
                    break;

                case Keys.C:
                    command = (byte)(command | 0x02);//Bボタン
                    break;
            }
        }
        public void KeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {

                case Keys.W:
                    command = (byte)(command & ~0x20);//↑
                    break;

                case Keys.A:
                    command = (byte)(command & ~0x80);//←
                    break;

                case Keys.S:
                    command = (byte)(command & ~0x10);//↓
                    break;

                case Keys.D:
                    command = (byte)(command & ~0x40);//→
                    break;

                case Keys.Space:
                    command = (byte)(command & ~0x01);//Aボタン
                    break;

                case Keys.C:
                    command = (byte)(command & ~0x02);//Bボタン
                    break;
            }
        }

    }



}