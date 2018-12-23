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
        testCharacter nampo;
        MoveObject mikataG, tekiG;

        public GameMaster(DrawTool drawTool)
        {
            this.DrawTool = drawTool;
            UserInterface = new UserInterface();

            //DrawTool.AddLabel("@");
            //DrawTool.Draw("@", new Vector2(200, 100));
            nampo = new testCharacter(DrawTool, "@", new Vector2(500, 600), new Vector2(0, 0));

            //改善の余地あり
            mikataG = new MoveObject(drawTool);
            tekiG = new MoveObject(drawTool);

            mikataG.Add(nampo);

            for (int i = 0; i < 10; i++)
            {
                tekiG.Add(new teki(DrawTool, "P", new Vector2(10 + 10 * i, 10 + 30 * i), new Vector2(1, 0)));
            }
        }

        public void Run()
        {
            switch (UserInterface.command)
            {
                case 0x20://↑
                    nampo.Point = nampo.Point + new Vector2(0, -5);
                    break;

                case 0x80://←
                    nampo.Point = nampo.Point + new Vector2(-5, 0);
                    break;

                case 0x10://↓
                    nampo.Point = nampo.Point + new Vector2(0, +5);
                    break;

                case 0x40://→
                    nampo.Point = nampo.Point + new Vector2(+5, 0);
                    break;

                case 0x01://Space//Aボタン
                    nampo.Shot();
                    break;
            }

            RunTask(mikataG);
            RunTask(tekiG);

            HitCheck((MoveObject)mikataG.NextTask, (MoveObject)tekiG.NextTask);

        }

        public void HitCheck(MoveObject move1G,MoveObject move2G)
        {
            var temp1 = move1G;
            var temp2 = move2G;

            while(temp1 != null)
            {
                while(temp2 != null)
                {
                    if (
                        (
                         (temp1.Point.X >= temp2.Point.X && temp1.Point.X <= temp2.Point.X + 30) ||
                         (temp2.Point.X >= temp1.Point.X && temp2.Point.X <= temp1.Point.X + 30)
                         ) &&
                        (
                         (temp1.Point.Y >= temp2.Point.Y && temp1.Point.Y <= temp2.Point.Y + 30) ||
                         (temp2.Point.Y >= temp1.Point.Y && temp2.Point.Y <= temp1.Point.Y + 30)
                        )
                       )
                    {
                        //あたった
                        temp1.Remove();
                        temp2.Remove();
                        temp1.DrawTool.RemoveLabel(temp1.Name);
                        temp2.DrawTool.RemoveLabel(temp2.Name);
                    }


                        temp2 = (MoveObject)temp2.NextTask;
                }
                temp1 = (MoveObject)temp1.NextTask;
                temp2 = move2G;
            }
        }

        public void RunTask(GameObject gameobjectG)
        {
            GameObject temp = (GameObject)gameobjectG.NextTask;
            while (temp != null)
            {
                temp.Run();
                temp.Draw();
                temp = (GameObject)temp.NextTask;
            }
        }
    }

    abstract class NamTask {
        public NamTask PreTask { get; set; }
        public NamTask NextTask { get; set; }

        public NamTask()
        {
            PreTask = null;
            NextTask = null;
        }

        public NamTask(NamTask pretask,NamTask nexttask)
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
    abstract class GameObject : NamTask {
        public abstract void Run();
        public abstract void Draw();
    }
    class MoveObject : GameObject {

        public MoveObject(DrawTool drawTool)
        {
            this.DrawTool = drawTool;
        }
        public DrawTool DrawTool { get; set; }
        public string Picture { get; set; }
        public string Name { get; set; }
        public Vector2 Point { get; set; }
        public Vector2 Vector { get; set; }
        public override void Run()
        {

        }
        public override void Draw()
        {

        }

    }

    //テスト用キャラクター
    class testCharacter : MoveObject
    {
        public int Hp { get; set; }
        public bool isShot { get; set; }
        public int IntervalShot { get; set; }
        public int ShotTimer { get; set; }

        public testCharacter(DrawTool drawTool,string pic,Vector2 point,Vector2 vec):base(drawTool)
        {
            this.Hp = 10;
            this.DrawTool = drawTool;
            this.Picture = pic;
            this.Point = point;
            this.Vector = vec;

            isShot = false;
            IntervalShot = 6;
            ShotTimer = 0;

            Name = DrawTool.AddLabel(this.Picture);
        }

        public override void Run()
        {
            Point = Point + Vector;
            ShotTimer++;
            if(ShotTimer > IntervalShot)
            {
                isShot = false;
                ShotTimer = 0;
            }
        }

        public override void Draw()
        {
            DrawTool.Draw(Name, Point);
        }

        public void Shot()
        {
            if(isShot == false)
            {
                this.Add(new Bellet1(DrawTool, Point));
                isShot = true;
                ShotTimer = 0;
            }
        }
    }

    class teki : testCharacter
    {
        public teki(DrawTool drawTool, string pic, Vector2 point, Vector2 vec) : base(drawTool, pic, point, vec)
        {
            IntervalShot = 36;
        }

        public override void Run()
        {
            Point = Point + Vector;
            ShotTimer++;
            if (ShotTimer > IntervalShot)
            {
                isShot = false;
                ShotTimer = 0;
            }
            Shot();
        }

        public override void Draw()
        {
            DrawTool.Draw(Name, Point);
        }

        new public void Shot()
        {
            if (isShot == false)
            {
                this.Add(new Bellet1(DrawTool, Point,true));
                isShot = true;
                ShotTimer = 0;
            }
        }
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

    class Bellet1 : MoveObject
    {
        public int damege;
        public int LimitCount;//移動距離制限、一定距離過ぎたら消す
        public int Count;

        public Bellet1(DrawTool drawTool, Vector2 point):base(drawTool)
        {
            damege = 5;
            Picture = "・";
            Point = point;
            Vector = new Vector2(0, -6);
            Count = 0;
            LimitCount = 600;

            Name = drawTool.AddLabel(Picture);
        }

        public Bellet1(DrawTool drawTool, Vector2 point,bool teki) : base(drawTool)
        {
            damege = 5;
            Picture = "・";
            Point = point;
            Vector = new Vector2(0, 6);
            Count = 0;
            LimitCount = 600;

            Name = drawTool.AddLabel(Picture);
        }

        public override void Run()
        {
            //throw new NotImplementedException();
            Point = Point + Vector;

            Count += (int)Vector.Length();

            if(Count > LimitCount)
            {
                Remove();
                DrawTool.RemoveLabel(Name);
            }
        }

        public override void Draw()
        {
            //throw new NotImplementedException();
            DrawTool.Draw(Name, Point);
        }


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
    class Scene : GameObject
    {
        public override void Run()
        {

        }
        public override void Draw()
        {

        }
    }


}