using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Drawing;
using System.Numerics;

namespace NampoSpace
{
    class NamTask
    {
        public NamTask PreTask { get; set; }
        public NamTask NextTask { get; set; }

        public NamTask()
        {
            PreTask = null;
            NextTask = null;
        }

        public NamTask(NamTask pretask, NamTask nexttask)
        {
            this.PreTask = pretask;
            this.NextTask = nexttask;
        }

        public void Add(NamTask addtask)
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
    abstract class GameObject : NamTask
    {
        public GameObject(DrawTool drawTool)
        {
            this.DrawTool = drawTool;
        }
        public DrawTool DrawTool { get; set; }
        public abstract void Run();
        public abstract void Draw();
    }

    abstract class Scene : GameObject
    {
        public virtual SceneStats SceneStats { get; set; }

        protected UserInterface UserInterface;

        public Scene(DrawTool drawTool, UserInterface userInterface) : base(drawTool)
        {
            UserInterface = userInterface;
        }
    }

    abstract class MoveObject : GameObject
    {

        public MoveObject(DrawTool drawTool) : base(drawTool)
        {

        }
        public string Picture { get; set; }
        public string Name { get; set; }
        public Vector2 Point { get; set; }
        public Vector2 Vector { get; set; }

    }

    class DrawTool
    {
        public Form form;
        static int id = 0;

        public DrawTool(Form form)
        {
            this.form = form;

        }

        public string AddLabel(string moji)
        {
            // Create an instance of a Label.
            Label label1 = new Label();

            id++;
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

            label1.Name = moji + id.ToString();

            /* Set the size of the control based on the PreferredHeight and PreferredWidth values. */
            label1.Size = new Size(label1.PreferredWidth, label1.PreferredHeight);

            form.Controls.Add(label1);

            return label1.Name;
        }

        public void RemoveLabel(string moji)
        {
            foreach (var item in this.form.Controls.Find(moji, true))
            {
                form.Controls.Remove(item);
            }
        }

        public void Draw(String moji, Vector2 point)
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
