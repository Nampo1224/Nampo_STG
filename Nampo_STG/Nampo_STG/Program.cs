﻿using System;
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

            /* ToDo:
             *      ・衝突判定の最適化（4分木空間）
             *      ・衝突判定クラス（＋衝突タイプクラス）
             *      ・アニメーションクラス
             *      ・敵の生成・弾の生成(Factory)クラス
             *      ・エフェクトクラス（※→∵→×→・）
             *      ・サウンドツールクラス
             *      ・C#スクリプトによる弾・敵・アニメーションの作成
             *      ・C# + OpenGLによる描画(DrawToolクラス改善)
             *      ・デリゲート・イベントをいい感じで使いたい（インターフェースも）
             */
        }
    }
}

namespace NampoSpace
{
    enum SceneStats
    {
        start,game,end
    }

    class GameMaster {

        DrawTool DrawTool;
        public UserInterface UserInterface;
        Scene CurrentScene;
        SceneStats SceneStats,preSceneStats;

        public GameMaster(DrawTool drawTool)
        {
            this.DrawTool = drawTool;
            UserInterface = new UserInterface();

            SceneStats = SceneStats.start;
            preSceneStats = SceneStats.start;

            CurrentScene = new StartScene(DrawTool, UserInterface);

        }

        public void Run()
        {
            preSceneStats = SceneStats;

            CurrentScene.Run();

            SceneStats = CurrentScene.SceneStats;

            if (SceneStats != preSceneStats)
            {
                switch (SceneStats)
                {
                    case SceneStats.start:
                        CurrentScene = new StartScene(DrawTool, UserInterface);
                        break;
                    case SceneStats.game:
                        CurrentScene = new GameScene(DrawTool, UserInterface);
                        break;
                    case SceneStats.end:
                        break;
                    default:
                        break;
                }
            }
        }

        public void Draw()
        {
            CurrentScene.Draw();
        }
    }

    class ManageMoveObject : GameObject
    {
        public MoveObject MoveObjectGroup{get;set;}

        public ManageMoveObject(DrawTool drawTool) : base(drawTool)
        {
            MoveObjectGroup = new Character(DrawTool);
        }

        public override void Run()
        {
            GameObject temp = (GameObject)MoveObjectGroup.NextTask;
            while (temp != null)
            {
                temp.Run();
                temp = (GameObject)temp.NextTask;
            }
        }

        public override void Draw()
        {
            GameObject temp = (GameObject)MoveObjectGroup.NextTask;
            while (temp != null)
            {
                temp.Draw();
                temp = (GameObject)temp.NextTask;
            }
        }

        public void Clear()
        {
            MoveObject temp = (MoveObject)MoveObjectGroup.NextTask;
            while (temp != null)
            {
                temp.Remove();
                temp.DrawTool.RemoveLabel(temp.Name);
                temp = (MoveObject)temp.NextTask;
            }
        }

        public void Add(MoveObject AddMoveObject)
        {
            MoveObjectGroup.Add(AddMoveObject);
        }

        public static ManageMoveObject operator+ (ManageMoveObject manage,MoveObject MoveObject)
        {
            manage.Add(MoveObject);
            return manage;
        }
    }

    class Character : MoveObject
    {
        public int Hp { get; set; }
        public bool IsShot { get; set; }
        public int IntervalShot { get; set; }
        public int ShotTimer { get; set; }
        public ManageMoveObject BelletGroup;

        public Character(DrawTool drawTool) : base(drawTool) { }
        public Character(DrawTool drawTool, string pic, Vector2 point) : this(drawTool)
        {
            this.Hp = 10;
            this.DrawTool = drawTool;
            this.Picture = pic;
            this.Point = point;
            this.Vector = new Vector2(0, 0);

            IsShot = false;
            IntervalShot = 6;
            ShotTimer = 0;

            Name = DrawTool.AddLabel(this.Picture);
        }
        public Character(DrawTool drawTool, string pic, Vector2 point, Vector2 vec) : this(drawTool, pic, point)
        {
            this.Vector = vec;
        }
        public Character(DrawTool drawTool, string pic, Vector2 point, ManageMoveObject managebellet) : this(drawTool,pic,point)
        {
            this.BelletGroup = managebellet;
        }
        public Character(DrawTool drawTool, string pic, Vector2 point, Vector2 vec, ManageMoveObject managebellet) : this(drawTool,pic,point,managebellet)
        {
            this.Vector = vec;
        }

        public override void Run()
        {
            Point = Point + Vector;
            ShotTimer++;
            if (ShotTimer > IntervalShot)
            {
                IsShot = false;
                ShotTimer = 0;
            }
        }
        public override void Draw()
        {
            DrawTool.Draw(Name, Point);
        }

        public void Shot()
        {
            if (IsShot == false)
            {
                //3Way
                BelletGroup += (MoveObject)new Bellet1(DrawTool, Point);
                BelletGroup += (MoveObject)new Bellet1(DrawTool, Point, new Vector2(-2, -3));
                BelletGroup += (MoveObject)new Bellet1(DrawTool, Point, new Vector2(+2, -3));
                IsShot = true;
                ShotTimer = 0;
            }
        }
    }

    class teki : Character
    {
        MoveObject target;
        public teki(DrawTool drawTool, string pic, Vector2 point, Vector2 vec,MoveObject target,ManageMoveObject bulletGroup) : base(drawTool, pic, point,vec,bulletGroup)
        {
            IntervalShot = 150;
            this.target = target;

        }

        public override void Run()
        {
            Point = Point + Vector;
            ShotTimer++;
            if (ShotTimer > IntervalShot)
            {
                IsShot = false;
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
            if (IsShot == false)
            {
                BelletGroup.Add(new Bellet2(DrawTool, Point,target));
                IsShot = true;
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
    //普通の弾
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

        public Bellet1(DrawTool drawTool, Vector2 point, Vector2 vec) : this(drawTool, point)
        {
            Vector = vec;
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
    
    //誘導するの弾?
    class Bellet2 : MoveObject
    {
        public int damege;
        public int LimitCount;//移動距離制限、一定距離過ぎたら消す
        public int Count;
        public MoveObject target;
        public float MaxDegree;

        /*
        public Bellet2(DrawTool drawTool, Vector2 point) : base(drawTool)
        {
            damege = 5;
            Picture = "・";
            Point = point;
            Vector = new Vector2(0, -6);
            Count = 0;
            LimitCount = 600;

            Name = drawTool.AddLabel(Picture);
        }

        public Bellet2(DrawTool drawTool, Vector2 point, bool teki) : base(drawTool)
        {
            damege = 5;
            Picture = "・";
            Point = point;
            Vector = new Vector2(0, 6);
            Count = 0;
            LimitCount = 600;

            Name = drawTool.AddLabel(Picture);
        }*/

        
        public Bellet2(DrawTool drawTool, Vector2 point,MoveObject moveObject) : base(drawTool)
        {
            damege = 5;
            Picture = "◎";
            Point = point;
            Vector = new Vector2(0, 0);
            Count = 0;
            LimitCount = 600;

            Name = drawTool.AddLabel(Picture);

            target = moveObject;
            MaxDegree = (float)(((Math.PI * 10) / 180));//約10度がマックス
        }


        public override void Run()
        {
            //throw new NotImplementedException();
            if (Count % 100 < 70)
            {
                Vector2 preVector = new Vector2(Vector.X, Vector.Y);
                preVector = Vector2.Normalize(preVector);

                Vector = target.Point - this.Point;
                Vector = Vector2.Normalize(Vector);

                if (preVector.X * Vector.X + preVector.Y * Vector.Y > Math.Cos(MaxDegree))
                {
                    Vector = new Vector2((float)((Math.Cos(MaxDegree) - Math.Sin(MaxDegree)) * preVector.X),
                                         (float)((Math.Sin(MaxDegree) + Math.Cos(MaxDegree)) * preVector.Y));
                }

                Vector = Vector * 3;
            }

            Point = Point + Vector;

            Count += (int)Vector.Length();

            if (Count > LimitCount)
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

    class StartScene : Scene
    {
        Character Title,start,end,select;
        public override SceneStats SceneStats { set; get; }

        public StartScene(DrawTool drawTool,UserInterface userInterface):base(drawTool,userInterface)
        {
            Title = new Character(DrawTool, "Nampo Shooting !!(仮)", new Vector2(600, 300), new Vector2(0, 0));
            start = new Character(DrawTool, "スタート", new Vector2(700, 500), new Vector2(0, 0));
            end = new Character(DrawTool, "おわり", new Vector2(700, 600), new Vector2(0, 0));
            select = new Character(DrawTool, "→", new Vector2(650, 500), new Vector2(0, 0));

            this.SceneStats = SceneStats.start;
        }

        public override void Run()
        {
            switch (UserInterface.command)
            {
                case 0x20://↑
                    select.Point =  new Vector2(650, 500);
                    break;

                case 0x10://↓
                    select.Point = new Vector2(650, 600);
                    break;

                case 0x01://Space//Aボタン
                    SceneStats = SceneStats.game;
                    Title.DrawTool.RemoveLabel(Title.Name);
                    start.DrawTool.RemoveLabel(start.Name);
                    end.DrawTool.RemoveLabel(end.Name);
                    select.DrawTool.RemoveLabel(select.Name);
                    break;
            }
        }
        public override void Draw()
        {
            Title.Draw();
            start.Draw();
            end.Draw();
            select.Draw();
        }
    }

    class GameScene : Scene
    {
        public override SceneStats SceneStats { set; get; }

        //testキャラ
        Character nampo;

        //キャラクターマネージャー
        ManageMoveObject Mikata, Teki, MikataBellet, TekiBellet;

        public GameScene(DrawTool drawTool, UserInterface userInterface) : base(drawTool,userInterface)
        {
            this.SceneStats = SceneStats.game;
                        
            Mikata = new ManageMoveObject(drawTool);
            Teki = new ManageMoveObject(drawTool);
            MikataBellet = new ManageMoveObject(drawTool);
            TekiBellet = new ManageMoveObject(drawTool);

            nampo = new Character(DrawTool, "@", new Vector2(500, 600), new Vector2(0, 0), MikataBellet);

            Mikata += nampo;

            for (int i = 0; i < 10; i++)
            {
                Teki.Add(new teki(DrawTool, "P", new Vector2(10 + 10 * i, 10 + 30 * i), new Vector2(1, 0), nampo,TekiBellet));
            }
        }

        public override void Run()
        {
            switch (UserInterface.command & 0xF0)
            {
                case 0x20://↑
                    nampo.Point = nampo.Point + new Vector2(0, -5);
                    break;

                case 0xA0://←↑
                    nampo.Point = nampo.Point + new Vector2(-5, -5);
                    break;

                case 0x80://←
                    nampo.Point = nampo.Point + new Vector2(-5, 0);
                    break;

                case 0x90://←↓
                    nampo.Point = nampo.Point + new Vector2(-5, +5);
                    break;

                case 0x10://↓
                    nampo.Point = nampo.Point + new Vector2(0, +5);
                    break;

                case 0x50://↓→
                    nampo.Point = nampo.Point + new Vector2(+5, +5);
                    break;

                case 0x40://→
                    nampo.Point = nampo.Point + new Vector2(+5, 0);
                    break;

                case 0x60://→↑
                    nampo.Point = nampo.Point + new Vector2(+5, -5);
                    break;
            }

            //Aボタン
            if((UserInterface.command & 0x01) == 0x01)
            {
                nampo.Shot();
            }

            Mikata.Run();
            MikataBellet.Run();
            Teki.Run();
            TekiBellet.Run();

            HitCheck((MoveObject)Mikata.MoveObjectGroup.NextTask, (MoveObject)Teki.MoveObjectGroup.NextTask);

            HitCheck((MoveObject)MikataBellet.MoveObjectGroup.NextTask, (MoveObject)Teki.MoveObjectGroup.NextTask);
            HitCheck((MoveObject)Mikata.MoveObjectGroup.NextTask, (MoveObject)TekiBellet.MoveObjectGroup.NextTask);

            if (RessultCheck())
            {
                //ゲーム終了処理
                Mikata.Clear();
                MikataBellet.Clear();
                Teki.Clear();
                TekiBellet.Clear();
            }
        }
        public override void Draw()
        {
            Mikata.Draw();
            MikataBellet.Draw();
            Teki.Draw();
            TekiBellet.Draw();
        }

        void HitCheck(MoveObject move1G, MoveObject move2G)
        {
            var temp1 = move1G;
            var temp2 = move2G;

            while (temp1 != null)
            {
                while (temp2 != null)
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

        //ゲーム終了ならTrue
        bool RessultCheck()
        {
            int Count = 0;

            GameObject temp = (GameObject)Teki.MoveObjectGroup.NextTask;
            while (temp != null)
            {
                Count++;
                temp = (GameObject)temp.NextTask;
            }

            if(Count < 2)
            {
                SceneStats = SceneStats.start;
                return true;
            }

            return false;
        }

    }

    class RessultScene : GameObject
    {
        public RessultScene(DrawTool drawTool) : base(drawTool)
        {

        }

        public override void Run()
        {

        }
        public override void Draw()
        {

        }
    }

}