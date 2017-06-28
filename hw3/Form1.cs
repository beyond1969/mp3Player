using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using WMPLib;

namespace hw3
{
    public partial class Form1 : Form
    {   
        /* 그래픽 관련 */ 
        float centerX, centerY;
        float DEGREE_TURN = 0f;
        float Ngon = 6f;
        Point[] screenPoint;
        Rectangle fullScreen;
        PointF[] polygon;
        PointF[] player = new PointF[3];
        List<wallClass> list_wall = new List<wallClass>();
        Matrix matrix_rotate = new Matrix();
        Matrix matrix_shirink = new Matrix();
        Matrix matrix_player_right = new Matrix();
        Matrix matrix_player_left = new Matrix();
        Matrix matrix_big = new Matrix();
        Matrix matrix_small = new Matrix();
        int score = 0;

        /* 배경음 */
        WindowsMediaPlayer wmp;

        float turn = 1.0001f;
        float angleAdd = 0f;
        
        /* 기체 회전 제어용 */
        bool b_isRightPressed = false;
        bool b_isLeftPressed = false;

        Random generator;   // 난수 발생기

        /// <summary>
        /// 게임 상태확인 MAIN은 메인 화면, PLAY는 게임 화면
        /// </summary>
        public enum GAME_STATE
        {
            MAIN = 0,
            PLAY
        }

        // 기본적으로 메인 화면
        GAME_STATE state = GAME_STATE.MAIN;

        public Form1()
        {
            InitializeComponent();
        }

        // 각종 변수들의 초기화와 매트릭스 초기화를 담당.
        private void Form1_Load(object sender, EventArgs e)
        {
            centerX = this.Width / 2;
            centerY = this.Height / 2;

            Size screenSize = new Size(this.Width * 2, this.Height * 2);
            screenPoint = new Point[1];
            screenPoint[0].X = -this.Width / 2; screenPoint[0].Y = -this.Height / 2;
            fullScreen = new Rectangle(screenPoint[0], screenSize);

            matrix_rotate.RotateAt(turn,new PointF(centerX, centerY));
            matrix_player_right.RotateAt(turn * 5f, new PointF(centerX, centerY));
            matrix_player_left.RotateAt(-turn * 5f, new PointF(centerX, centerY));
            matrix_big.Scale(3, 3);
            matrix_small.Scale(-3, -3);

            // 배경음 초기화
            string filePath = Application.StartupPath + @"\\bgm.mp3";
            wmp = new WindowsMediaPlayer();
            IWMPMedia bgm = wmp.newMedia(filePath);
            IWMPPlaylist playlist = wmp.newPlaylist("MusicPlayer", "");
            playlist.appendItem(bgm);
            wmp.currentPlaylist = playlist;
            wmp.controls.stop();

            DateTime dtmcurrent = DateTime.Now;
            generator = new Random(dtmcurrent.Millisecond);
        }

        // 실질적으로 화면을 그려주는 메서드
        // 타이머의 틱마다 화면에 그래픽을 표현함.
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen linePen = new Pen(Color.White,1);
           
            SolidBrush greenBrush = new SolidBrush(Color.FromArgb(255,138,255,141));
            SolidBrush secondBrush = new SolidBrush(Color.FromArgb(255,68,255,72));
            SolidBrush thirdBrush = new SolidBrush(Color.FromArgb(255,0,215,5));
            SolidBrush polygonBrush = new SolidBrush(Color.FromArgb(255,207,143,162));
            SolidBrush whiteBrush = new SolidBrush(Color.White);
            SolidBrush blackBrush = new SolidBrush(Color.Black);
            

            /* 메인화면 에서만 (초기화) */
            if (state == GAME_STATE.MAIN)
            {
                polygon = new PointF[(int)Ngon];
                // 도형 그릴 점
                polygon[0].X = centerX + 20; polygon[0].Y = centerY;
                for (int i = 1; i < Ngon; i++)
                {
                    polygon[i] = transformation(centerX, centerY, (float)Math.PI * 2f / Ngon * i, 20);
                }
                // 플레이어 그릴 점
                player[0].X = centerX + 35; player[0].Y = centerY;
                for (int i = 1; i < 3; i++)
                {
                    player[i] = transformation(centerX + 30, centerY, (float)Math.PI * 2f / 3f * i, 5);
                }
            }
            DEGREE_TURN = 360.0000f / Ngon;
            // 구역 나누기
            for(float i = 0; i < Ngon; i++)
            {
                float startAngle = DEGREE_TURN * i + angleAdd;
                if (Ngon % 2 == 0)
                {
                    if (i % 2 == 1)
                    {
                        g.FillPie(greenBrush, fullScreen, startAngle, DEGREE_TURN);
                        g.DrawPie(linePen, fullScreen, startAngle, DEGREE_TURN);
                    }
                    else
                    {
                        g.FillPie(secondBrush, fullScreen, startAngle, DEGREE_TURN);
                        g.DrawPie(linePen, fullScreen, startAngle, DEGREE_TURN);
                    }
                }
                else
                {
                    if (i % 3 == 1)
                    {
                        g.FillPie(greenBrush, fullScreen, startAngle, DEGREE_TURN);
                        g.DrawPie(linePen, fullScreen, startAngle, DEGREE_TURN);
                    }
                    else if (i % 3 == 2)
                    {
                        g.FillPie(secondBrush, fullScreen, startAngle, DEGREE_TURN);
                        g.DrawPie(linePen, fullScreen, startAngle, DEGREE_TURN);
                    }
                    else
                    {
                        g.FillPie(thirdBrush, fullScreen, startAngle, DEGREE_TURN);
                        g.DrawPie(linePen, fullScreen, startAngle, DEGREE_TURN);
                    }
                }
            }
            // 도형 그리기
            g.FillPolygon(polygonBrush, polygon);
            g.DrawPolygon(linePen, polygon);
            // 벽 그리기
            if (state == GAME_STATE.PLAY)
            {
                for (int i = 0; i < list_wall.Count; i++)
                    g.FillPolygon(polygonBrush, list_wall[i].wallPoint);
            }
            // 기체 그리기
            g.FillPolygon(polygonBrush, player);
            g.DrawPolygon(linePen, player);
            // 점수 그리기
            g.DrawString("TIME : " + score, new Font("Verdana", 10), blackBrush, new PointF(centerX+100, centerY-190));
            /* 메인 화면에서만 그려지게 */
            if (state == GAME_STATE.MAIN)
            {
                // 타이틀 그리기
                g.DrawString("Super", new Font("Verdana", 20), whiteBrush, new PointF(centerX - 80, centerY - 100));
                g.DrawString(Ngon + "-Gon", new Font("Verdana", 20), whiteBrush, new PointF(centerX - 40, centerY - 70));
                // 안내문구 그리기
                g.DrawString("Press SPACE key to start", new Font("Verdana", 15), whiteBrush, new PointF(centerX - 130, centerY + 100));
            }
        }

        // 중점 cX cY를 기준으로 rad 만큼 회전하고 length 만큼 떨어진 곳의 점을 리턴함
        private PointF transformation(float cX, float cY, float rad, float length)
        {
            PointF result = new PointF();
            result.X = cX + (length * (float)Math.Cos(rad));
            result.Y = cY + (length * (float)Math.Sin(rad));
            
            return result;
        }

        // 타이머 1틱마다 실행됨
        // 주로 그래픽을 표현하기 전에 각 점들을 업데이트 해줌
        // 순서는 업데이트->충돌판정->그래픽표현
        private void timer1_Tick(object sender, EventArgs e)
        {
            matrix_rotate.TransformPoints(this.polygon);    // 도형 돌리기

            score += 1;

            // 기체 제어 변수 상황에 따라 기체를 회전시킴
            if (b_isRightPressed == true)
            {
                matrix_player_right.TransformPoints(this.player);
            }
            if (b_isLeftPressed == true)
            {
                matrix_player_left.TransformPoints(this.player);
            }

            /* 벽 생성 */
            if (score%100 == 0) 
            {
                // 1초마다 벽 생성 (1~ Ngon-1 개)

                int genWall = generator.Next(1, (int)Ngon - 1); // 생성될 벽 개수
                int[] areaArr = new int[genWall];   // 벽 생성 구역
                /* 구역 정하기 */
                for (int i = 0; i < areaArr.Length; i++)
                    areaArr[i] = -1;
                for (int i = 0; i < genWall; i++)    // 개수 만큼 반복하며 벽 생성
                {
                    bool randomExit = true;
                    while (randomExit)  // 같은게 있으면 계속 반복하며 중복 회피
                    {
                        int areaNum = generator.Next(0, (int)Ngon-1);   // 랜덤 구역
                        for (int j = 0; j < areaArr.Length; j++)             // 구역 배열에서 생성된 구역 번호가 있는지 확인
                        {
                            if(areaArr[j] == areaNum)
                            {
                                break;
                            }
                            else if(areaArr[j] == -1){
                                randomExit = false;
                                areaArr[j] = areaNum;
                                break;
                            }
                        }
                    }
                }
                /* 구역 정하기 end */

                /* 벽 만들기 */
                for(int i = 0; i < genWall; i++)
                {
                    wallClass wall = new wallClass();
                    wall.areaNum = areaArr[i];
                    
                    list_wall.Add(wall);
                }
                /* 벽 만들기 end */
            }
            /* 벽 생성 end */
            
            /* 돌리기 */
            if(angleAdd <= 360f)
            {
                angleAdd += turn;
            }
            else
            {
                angleAdd = angleAdd - 360f + turn;
            }
            /* 벽 움직임 */
            for(int i = 0; i<list_wall.Count; i++)
            {
                list_wall[i].wallMoving(centerX, centerY, angleAdd, Ngon);
                matrix_rotate.TransformPoints(list_wall[i].wallPoint);
                /* 충돌 판정 */
                foreach (wallClass cursor in list_wall)
                {
                    // 충돌 판정 방법은 기체의 세 점 S,T,U가 벽 리스트에 존재하는 벽들 중에 하나의 내부에 진입 했을 경우 발생.
                    float S, T, U;
                    S = calcDistance(cursor.wallPoint[2].X, cursor.wallPoint[2].Y, cursor.wallPoint[3].X, cursor.wallPoint[3].Y, player[0].X, player[0].Y);
                    T = calcDistance(cursor.wallPoint[2].X, cursor.wallPoint[2].Y, cursor.wallPoint[3].X, cursor.wallPoint[3].Y, player[1].X, player[1].Y);
                    U = calcDistance(cursor.wallPoint[2].X, cursor.wallPoint[2].Y, cursor.wallPoint[3].X, cursor.wallPoint[3].Y, player[2].X, player[2].Y);

                    /* 이 부분은 기체가 비교할 벽의 구역 안에 있을 경우만 비교하기 위해 각도 계산*/
                    float dx = player[0].X - centerX;
                    float dy = player[0].Y - centerY;
                    float rad = (float)Math.Atan2(dy, dx);
                    float degree = (rad * 180) / (float)Math.PI;
                    if (degree < 0)
                        degree = 360 + degree;

                    float startAng = angleAdd + DEGREE_TURN * cursor.areaNum;
                    float endAng;
                    if (cursor.areaNum + 1 >= Ngon)
                        endAng = angleAdd + DEGREE_TURN * 0;
                    else
                        endAng = angleAdd + DEGREE_TURN * (cursor.areaNum + 1);
                    if (startAng >= endAng)
                    {
                        endAng += 360f;
                        if (degree + 360f < endAng)
                            degree += 360f;
                    }
                    /* 이곳까지 각도 계산 이후 충돌 판정 */
                    if (degree >= startAng && degree <= endAng)
                        if (S <= cursor.wall_width || T <= cursor.wall_width || U <= cursor.wall_width)
                        {
                            timer1.Enabled = false;
                            state = GAME_STATE.MAIN;
                            MessageBox.Show("Score : " + score);
                            score = 0;
                            angleAdd = 0;
                            list_wall.Clear();
                            wmp.controls.stop();
                            this.Invalidate();
                            return;
                        }
                }
                /* 만약 벽이 중심에 도달하면 리스트에서 제거되어 그려지지 않음 */
                if (list_wall[i].distance <= 0)
                {
                    list_wall.RemoveAt(i);
                }
            }
            /* 벽 움직임 end */
            

            this.Invalidate();
        }

        // 두 점 (x1,y1)(x2,y2) 로 만들어진 선분과 점 (x,y)와의 거리를 리턴해줌
        float calcDistance(float x1, float y1, float x2, float y2, float x, float y)
        {
            float a = (y1 - y2) / (x1 - x2);
            float p = y1 - a * x1;
            
            return Math.Abs(a * x - y + p) / (float)Math.Sqrt(a * a + 1);
        }

        // 키 입력 이벤트를 처리함
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Keys key = e.KeyCode;

            if(key == Keys.Escape)
            {
                if(state == GAME_STATE.PLAY)
                {
                    timer1.Enabled = false;
                    state = GAME_STATE.MAIN;
                    score = 0;
                    angleAdd = 0;
                    list_wall.Clear();
                    wmp.controls.stop();
                    this.Invalidate();
                }
            }
            if(key == Keys.Up)
            {
                // 위쪽 화살표
                // 메인화면 일때만
                if (state == GAME_STATE.MAIN)
                {
                    if (Ngon < 24)
                    {
                        Ngon++;
                        this.Invalidate();
                    }
                }
            }
            if(key == Keys.Down)
            {
                // 아래쪽 화살표
                // 메인화면 일때만
                if (state == GAME_STATE.MAIN)
                {
                    if (Ngon > 3)
                    {
                        Ngon--;
                        this.Invalidate();
                    }
                }
            }
            if(key == Keys.Space)
            {
                // 스페이스바
                // 메인화면 일때만
                if(state == GAME_STATE.MAIN)
                {
                    state = GAME_STATE.PLAY;    // 게임 진행으로 전환
                    // 게임 시작
                    timer1.Interval = 10;
                    timer1.Enabled = true;

                    // 배경음 시작
                    wmp.controls.play();
                }
            }
            if(key == Keys.Right)
            {
                // 오른쪽 화살표
                // 게임 진행중일때만
                if(state == GAME_STATE.PLAY)
                {
                    b_isRightPressed = true;
                }
            }
            if(key == Keys.Left)
            {
                // 왼쪽 화살표
                // 게임 진행중일때만
                if(state == GAME_STATE.PLAY)
                {
                    b_isLeftPressed = true;
                }
            }
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            Keys key = e.KeyCode;

            if(key == Keys.Right)
            {
                b_isRightPressed = false;
            }
            if(key == Keys.Left)
            {
                b_isLeftPressed = false;
            }
        }

    }
    /* 벽 클래스 */
    public class wallClass
    {
        public PointF[] wallPoint = new PointF[4];
        public int areaNum;
        public float distance = 500f;
        public float wall_width = 20f;
        public float wall_move = 1f;

        // 벽 움직임을 담당하는 메서드
        // 현재 벽과 중점 사이의 거리를 점점 줄이게끔 벽을 구성할 4개 점을 이동시킴
        public void wallMoving(float cX, float cY, float angleAdd, float Ngon)
        {
            float transRadian = angleAdd * (float)Math.PI / 180f;

            wallPoint[0] = new PointF(cX + distance * (float)Math.Cos(transRadian + Math.PI * 2 / Ngon * areaNum),
                cY + distance * (float)Math.Sin(transRadian + Math.PI * 2 / Ngon * areaNum));
            wallPoint[1] = new PointF(cX + distance * (float)Math.Cos(transRadian + Math.PI * 2 / Ngon * (areaNum + 1)),
                cY + distance * (float)Math.Sin(transRadian + Math.PI * 2 / Ngon * (areaNum + 1)));
            wallPoint[2] = new PointF(cX + (distance + wall_width) * (float)Math.Cos(transRadian + Math.PI * 2 / Ngon * (areaNum + 1)),
                cY + (distance + wall_width) * (float)Math.Sin(transRadian + Math.PI * 2 / Ngon * (areaNum + 1)));
            wallPoint[3] = new PointF(cX + (distance + wall_width) * (float)Math.Cos(transRadian + Math.PI * 2 / Ngon * areaNum),
                cY + (distance + wall_width) * (float)Math.Sin(transRadian + Math.PI * 2 / Ngon * areaNum));

            distance -= wall_move;
        }
    }
}
