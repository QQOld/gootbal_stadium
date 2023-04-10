using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tao.DevIl;
using Tao.FreeGlut;
using Tao.OpenGl;

namespace Bataev_Artem_Pri119_lab15
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            AnT.InitializeContexts();
        }

        // отсчет времени
        float global_time = 0; // массив с параметрами установки камеры
        private float[,] camera_date = new float[5, 7];

        // экземпляра класса Explosion
        private Explosion BOOOOM_1 = new Explosion(1, 10, 1, 300, 300);

        // генератор случайны чисел
        Random rnd = new Random();

        double translateX = -5, translateY = 5, translateZ = -60, d = 45, zoom = 1;
        double os_x = 1, os_y = 0, os_z = 0;

        double ballTranslateX = 5, ballTranslateY = 5, ballTranslateZ = -1.7;
        double ballRotateX = 0, ballRotateY = 0, ballRotateZ = 0;
        double ballRadius = 1.5;

        double cameraSpeed = 1;

        private int imageId;
        private uint pictureTexture;
        string picture = "picture.jpg";

        private void Form1_Load(object sender, EventArgs e)
        {
            // инициализация OpenGL
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE);
            Il.ilInit();    
            Il.ilEnable(Il.IL_ORIGIN_SET);
            Gl.glClearColor(133, 255, 255, 0.5f);
            Gl.glViewport(0, 0, AnT.Width, AnT.Height);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluPerspective(45, (float)AnT.Width / (float)AnT.Height, 0.1, 800);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();

            Gl.glTranslated(camera_date[1, 0], camera_date[1, 1], camera_date[1, 2]);
            Gl.glRotated(camera_date[1, 3], camera_date[1, 4], camera_date[1, 5], camera_date[1, 6]);

            Gl.glEnable(Gl.GL_DEPTH_TEST);

            Il.ilGenImages(1, out imageId);
            Il.ilBindImage(imageId);

            if (Il.ilLoadImage(picture))
            {
                int width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
                int height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
                int bitspp = Il.ilGetInteger(Il.IL_IMAGE_BITS_PER_PIXEL);
                switch (bitspp)
                {
                    case 24:
                        pictureTexture = MakeGlTexture(Gl.GL_RGB, Il.ilGetData(), width, height);
                        break;
                    case 32:
                        pictureTexture = MakeGlTexture(Gl.GL_RGBA, Il.ilGetData(), width, height);
                        break;
                }
            }
            Il.ilDeleteImages(1, ref imageId);


            // позиция камеры 1:
            camera_date[0, 0] = -5;
            camera_date[0, 1] = 0;
            camera_date[0, 2] = -60;
            camera_date[0, 3] = 0;
            camera_date[0, 4] = 1;
            camera_date[0, 5] = 0;
            camera_date[0, 6] = 0;

            // позиция камеры 2:
            camera_date[1, 0] = -5;
            camera_date[1, 1] = 5;
            camera_date[1, 2] = -60;
            camera_date[1, 3] = 45;
            camera_date[1, 4] = 1;
            camera_date[1, 5] = 0;
            camera_date[1, 6] = 0;

            // позиция камеры 3:
            camera_date[2, 0] = -4;
            camera_date[2, 1] = -2;
            camera_date[2, 2] = -50;
            camera_date[2, 3] = 60;
            camera_date[2, 4] = 1;
            camera_date[2, 5] = 1;
            camera_date[2, 6] = 0.5f;

            // позиция камеры 4:
            camera_date[3, 0] = 5;
            camera_date[3, 1] = -6;
            camera_date[3, 2] = -22;
            camera_date[3, 3] = 180;
            camera_date[3, 4] = 0;
            camera_date[3, 5] = 1;
            camera_date[3, 6] = 0.4f;

            // позиция камеры 5:
            camera_date[4, 0] = -5;
            camera_date[4, 1] = 4;
            camera_date[4, 2] = -52;
            camera_date[4, 3] = 90;
            camera_date[4, 4] = 1;
            camera_date[4, 5] = 0;
            camera_date[4, 6] = 0;

            comboBox1.SelectedIndex = 1;
            numericUpDown1.Value = (decimal)cameraSpeed;
            timer1.Enabled = false;

            // опиции для загрузки файла
            openFileDialog1.Filter = "ase files (*.ase)|*.ase|All files (*.*)|*.*";

            // активация таймера
            RenderTimer.Start();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value > 0)
                cameraSpeed = (double)numericUpDown1.Value;
            AnT.Focus();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = comboBox1.SelectedIndex;
            translateX = camera_date[index, 0];
            translateY = camera_date[index, 1];
            translateZ = camera_date[index, 2];
            d = camera_date[index, 3];
            os_x = camera_date[index, 4];
            os_y = camera_date[index, 5];
            os_z = camera_date[index, 6];
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            // отсчитываем время
            global_time += (float)RenderTimer.Interval / 1000;
            // вызываем функцию отрисовки
            Draw();
        }

        double goalTimer = 0;
        double currentballX = 0;
        double currentballY = 0;
        double currentballZ = 0;

        private void timer1_Tick(object sender, EventArgs e)
        {
            goalTimer += timer1.Interval;
            double t = goalTimer/1200;
            if (t > 1)
            {
                doBooom(22.6f, 5f, -2f, -1, null, -1);
                timer1.Enabled = false;
                goalTimer = 0;
                ballTranslateX = 5;
                ballTranslateY = 5;
                ballTranslateZ = -1.7;
            } else
            {
                ballTranslateX = (1 - t) * (1 - t) * currentballX + 2 * (1 - t) * t * 10 + t * t * 23;
                ballTranslateY = (1 - t) * (1 - t) * currentballY + 2 * (1 - t) * t * -5 + t * t * 4;
                ballTranslateZ = (1 - t) * (1 - t) * currentballZ + 2 * (1 - t) * t * -7 + t * t * -4;
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            currentballX = ballTranslateX;
            currentballY = ballTranslateY;
            currentballZ = ballTranslateZ;
            timer1.Enabled = true;
        }

        private void AnT_KeyDown(object sender, KeyEventArgs e)
        {  

            if (e.KeyCode == Keys.Q)
            {
                translateZ -= cameraSpeed;
            }
            if (e.KeyCode == Keys.E)
            {
                translateZ += cameraSpeed;
            }
            if (e.KeyCode == Keys.A)
            {
                translateX += cameraSpeed;
            }
            if (e.KeyCode == Keys.D)
            {
                translateX -= cameraSpeed;
            }
            if (e.KeyCode == Keys.W)
            {
                translateY -= cameraSpeed;
            }
            if (e.KeyCode == Keys.S)
            {
                translateY += cameraSpeed;
            }

            if (e.KeyCode == Keys.NumPad6)
            {
                if(ballTranslateX <= 23)
                {
                    if (a % 2 == 0)
                    {
                        ballRotateY -= 2;
                    }
                    else
                    {
                        ballRotateZ -= 2;
                    }
                    ballTranslateX += 0.2;

                    if (isGoalpostHit(22.6, 8) || isGoalpostHit(22.6, 2))
                    {
                        ballTranslateX -= 0.2;
                        if (a % 2 == 0)
                        {
                            ballRotateY += 2;
                        }
                        else
                        {
                            ballRotateZ += 2;
                        }
                    }
                    if (ballTranslateX >= 22.4 && ballTranslateY >= 3.59 && ballTranslateY <= 6.41)
                    {
                        doBooom(22.6f, 5f, -2f, -1, null, -1);
                        ballTranslateX = 5;
                        ballTranslateY = 5;
                    }
                }
            }

            if (e.KeyCode == Keys.NumPad4)
            {
                if (ballTranslateX >= -13.2)
                {
                    if (a % 2 == 0)
                    {
                        ballRotateY += 2;
                    }
                    else
                    {
                        ballRotateZ += 2;
                    }
                    ballTranslateX -= 0.2;

                    if (isGoalpostHit(-12.4, 8) || isGoalpostHit(-12.4, 2))
                    {
                        ballTranslateX += 0.2;
                        if (a % 2 == 0)
                        {
                            ballRotateY -= 2;
                        }
                        else
                        {
                            ballRotateZ -= 2;
                        }
                    }

                    if (ballTranslateX <= -12.4 && ballTranslateY >= 3.59 && ballTranslateY <= 6.41)
                    {
                        doBooom(-12.2f, 5f, -2f, 1, null, -1);
                        ballTranslateX = 5;
                        ballTranslateY = 5;
                    }
                }
            }

            if (e.KeyCode == Keys.NumPad5)
            {
                if (ballTranslateY <= 13.6)
                {
                    ballRotateX += 2;
                    ballTranslateY += 0.2;
                    if (isGoalpostHit(-12.4, 8) || isGoalpostHit(-12.4, 2) || isGoalpostHit(22.6, 8) || isGoalpostHit(22.6, 2))
                    {
                        ballRotateX -= 2;
                        ballTranslateY -= 0.2;
                    }
                }
            }

            if (e.KeyCode == Keys.NumPad8)
            {
                if (ballTranslateY >= -4.2)
                {
                    ballRotateX -= 2;
                    ballTranslateY -= 0.2;
                    if (isGoalpostHit(-12.4, 8) || isGoalpostHit(-12.4, 2) || isGoalpostHit(22.6, 8) || isGoalpostHit(22.6, 2))
                    {
                        ballRotateX += 2;
                        ballTranslateY += 0.2;
                    }
                }
            }
        }

        // функция отрисовки сцены
        private void Draw()
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glClearColor(0.2f, 0.9f, 1f, 1);
            Gl.glLoadIdentity();

            // определяем установленную камеру
            int camera = comboBox1.SelectedIndex;

            Gl.glPushMatrix();

            Gl.glTranslated(translateX, translateY, translateZ);
            Gl.glRotated(d, os_x, os_y, os_z);
            Gl.glScaled(zoom, zoom, zoom);
            Gl.glPushMatrix();

            DrawStadium();  

            // выполняем просчет взрыва
            BOOOOM_1.Calculate(global_time);

            Gl.glPopMatrix();

            Gl.glPopMatrix();
            Gl.glFlush();

            // обновляем окно
            AnT.Invalidate();
        }

        // функция для отрисовки матрицы
        private void DrawStadium()
        {
            //Земля
            Gl.glPushMatrix();
            Gl.glColor3f(0.5f, 0.5f, 0.5f);
            Gl.glTranslated(5, -10, 6);
            Gl.glScaled(3, 0.5, 2);
            Glut.glutSolidCube(40);
            Gl.glPopMatrix();

            Gl.glRotated(90, 1, 0, 0);
         
            //Поле
            Gl.glPushMatrix();    
            Gl.glTranslated(5, 5, 0);
            Gl.glScaled(2.5, 1.3, 0.05);
            Gl.glColor3f(0, 0.8f, 0);
            Glut.glutSolidCube(16);
            Gl.glColor3f(0, 0, 0);
            Gl.glLineWidth(2f);
            Glut.glutWireCube(16);
            Gl.glPopMatrix();

            //Контур поля
            Gl.glPushMatrix();     
            Gl.glTranslated(5, 5, -0.45);
            Gl.glScaled(2.5, 1.3, 0.0);
            Gl.glColor3f(1, 1, 1);
            Gl.glLineWidth(3f);
            Glut.glutWireCube(14);
            Gl.glPopMatrix();

            //Круг в центре
            Gl.glPushMatrix();          
            Gl.glTranslated(5, 5, -0.45);
            Gl.glScaled(1, 1, 0.0);
            Gl.glColor3f(1, 1, 1);
            Gl.glLineWidth(3f);
            Glut.glutWireCylinder(3.5, 2, 20, 1);
            Gl.glPopMatrix();

            //Линии ворот
            Gl.glPushMatrix();
            Gl.glTranslated(19.5, 5, -0.45);
            Gl.glScaled(0.5, 0.85, 0.0);
            Gl.glColor3f(1, 1, 1);
            Gl.glLineWidth(2f);
            Glut.glutWireCube(12);

            Gl.glTranslated(-58, 0, 0);
            Glut.glutWireCube(12);
            Gl.glPopMatrix();

            //Центральная линия
            Gl.glPushMatrix();
            Gl.glColor3f(1, 1, 1);
            Gl.glLineWidth(2f);
            Gl.glBegin(Gl.GL_LINES);
            Gl.glVertex3d(5, -4, -0.45);
            Gl.glVertex3d(5, 14, -0.45);
            Gl.glEnd();       
            Gl.glPopMatrix();

            //Ворота
            Gl.glPushMatrix();
            Gl.glTranslated(22.5, 5, -0.45);
            Gl.glScaled(0, 0.5, 1);
            Gl.glColor3f(0.92f, 0.92f, 0.92f);
            Gl.glLineWidth(3f);
            Glut.glutWireCube(12);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslated(-12.5, 5, -0.45);
            Gl.glScaled(0, 0.5, 1);
            Glut.glutWireCube(12);
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslated(22.5, 5, -0.45);
            Gl.glBegin(Gl.GL_LINE_STRIP);
            Gl.glVertex3d(-0.01, 3, -6);
            Gl.glVertex3d(2.2, 3, 0);
            Gl.glVertex3d(2.2, -3.05, 0);
            Gl.glEnd();

            Gl.glColor3f(0, 0, 0);
            Gl.glLineWidth(1f);
            Gl.glBegin(Gl.GL_LINES);
            for (double i = 0.5; i < 6; i += 0.5)
            {
                Gl.glVertex3d(-0.01, 3 - i, -6);
                Gl.glVertex3d(2.2, 3- i, 0);

                Gl.glVertex3d(2.2 - 0.367 * i, 3, 0);
                Gl.glVertex3d(2.2 - 0.367 * i, 3, 0 - i);

                Gl.glVertex3d(2.2 - 0.367 * i, -3, 0);
                Gl.glVertex3d(2.2 - 0.367 * i, -3, 0 - i);
            }
            for (double i = 0.5; i < 6; i += 0.5)
            {
                Gl.glVertex3d(2.2 - 0.367 * i, 3, -i);
                Gl.glVertex3d(2.2 - 0.367 * i, -3, -i);

                Gl.glVertex3d(2.2 - 0.367 * i, 3, -i);
                Gl.glVertex3d(0, 3, -i);

                Gl.glVertex3d(2.2 - 0.367 * i, -3, -i);
                Gl.glVertex3d(0, -3, -i);
            }
            Gl.glEnd();

            Gl.glColor3f(0.92f, 0.92f, 0.92f);
            Gl.glLineWidth(3f);

            Gl.glTranslated(0, -6, 0);
            Gl.glBegin(Gl.GL_LINE_STRIP);
            Gl.glVertex3d(-0.01, 3, -6);
            Gl.glVertex3d(2.2, 3, 0);
            Gl.glEnd();

          
            Gl.glTranslated(-36.5, 6, 0);
            Gl.glBegin(Gl.GL_LINE_STRIP);
            Gl.glVertex3d(1.5, 3, -6);
            Gl.glVertex3d(-0.7, 3, 0);
            Gl.glVertex3d(-0.7, -3, 0);
            Gl.glEnd();

            Gl.glColor3f(0, 0, 0);
            Gl.glLineWidth(1f);
            Gl.glBegin(Gl.GL_LINES);
            for (double i = 0.5; i < 6; i += 0.5)
            {
                Gl.glVertex3d(1.5, 3 - i, -6);
                Gl.glVertex3d(-0.7, 3 - i, 0);

                Gl.glVertex3d(-0.7 + 0.367 * i, 3, 0);
                Gl.glVertex3d(-0.7 + 0.367 * i, 3, 0 - i);

                Gl.glVertex3d(-0.7 + 0.367 * i, -3, 0);
                Gl.glVertex3d(-0.7 + 0.367 * i, -3, 0 - i);
            }
            for (double i = 0.5; i < 6; i += 0.5)
            {
                Gl.glVertex3d(-0.7 + 0.367 * i, 3, -i);
                Gl.glVertex3d(-0.7 + 0.367 * i, -3.2, -i);

                Gl.glVertex3d(-0.7 + 0.367 * i, 3, -i);
                Gl.glVertex3d(1.5, 3, -i);

                Gl.glVertex3d(-0.7 + 0.367 * i, -3, -i);
                Gl.glVertex3d(1.5, -3, -i);
            }
            Gl.glEnd();

            Gl.glColor3f(0.92f, 0.92f, 0.92f);
            Gl.glLineWidth(3f);

            Gl.glTranslated(0, -6, 0);
            Gl.glBegin(Gl.GL_LINE_STRIP);
            Gl.glVertex3d(-0.7, 3, 0);
            Gl.glVertex3d(1.5, 3, -6);
            Gl.glEnd();
            Gl.glPopMatrix();

            DrawMainTribune(-15, 25, -6);
            DrawSideTribune(15, -5, -15, false);
            DrawSideTribune(15, -5, -15, true);

            DrawBall();

            //Банер
            Gl.glPushMatrix();
            Gl.glTranslated(2, -15, -12);
            Gl.glColor3f(0.4f, 0.4f, 0.4f);
            Gl.glScaled(0.1, 0.1, 1);
            Glut.glutSolidCylinder(5, 3, 10, 10);
            Gl.glScaled(10, 10, 1);
            Gl.glTranslated(5, 0, 0);
            Gl.glScaled(0.1, 0.1, 1);
            Glut.glutSolidCylinder(5, 3, 10, 10);
            Gl.glScaled(10, 1, 1);
            Gl.glTranslated(-2.5, 0, -4);
            Gl.glScaled(1.5, 1, 1);
            Gl.glColor3f(0.1f, 0.1f, 0.1f);
            Glut.glutSolidCube(8);

            Gl.glScaled(1.5, 1, 1);
            Gl.glEnable(Gl.GL_TEXTURE_2D);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, pictureTexture);
            Gl.glPushMatrix();
            Gl.glScaled(1 / 1.5, 1, 1);
            Gl.glBegin(Gl.GL_QUADS);
            
            
            Gl.glVertex3d(-3.5, 8, -3);
            Gl.glTexCoord2f(0, 0);

            Gl.glVertex3d(-3.5, 8, 3);
            Gl.glTexCoord2f(1, 0);

            Gl.glVertex3d(3.5, 8, 3);
            Gl.glTexCoord2f(1, 1);

            
            Gl.glVertex3d(3.5, 8, -3);
            Gl.glTexCoord2f(0, 1);

            Gl.glEnd();
            Gl.glDisable(Gl.GL_TEXTURE_2D);

            Gl.glPopMatrix();
        }

        private void DrawMainTribune(float x1, float x2, float y)
        {
            Gl.glPushMatrix();
            Gl.glColor3f(1f, 0.9f, 0.4f);
           
            Gl.glBegin(Gl.GL_QUADS);
            for (int i = 0; i <= 4; i++)
            {
                Gl.glVertex3d(x1, y + i * -2, i * -2);
                Gl.glVertex3d(x2, y + i * -2, i * -2);
                Gl.glVertex3d(x2, y + i * -2, i * -2 - 2);
                Gl.glVertex3d(x1, y + i * -2, i * -2 - 2);
                                 
                Gl.glVertex3d(x2, y + i * -2, i * -2 - 2);
                Gl.glVertex3d(x1, y + i * -2, i * -2 - 2);
                Gl.glVertex3d(x1, y + (i + 1) * -2, (i + 1) * -2);
                Gl.glVertex3d(x2, y + (i + 1) * -2, (i + 1) * -2);
            }
            Gl.glEnd();

            Gl.glColor3f(0, 0, 0);
            Gl.glLineWidth(1f);
            Gl.glBegin(Gl.GL_LINE_LOOP);
            for (int i = 0; i <= 4; i++)
            {
                Gl.glVertex3d(x1 - 0.1, y + i * -2, i * -2);
                Gl.glVertex3d(x2 + 0.1, y + i * -2, i * -2);
                Gl.glVertex3d(x2 + 0.1, y + i * -2, i * -2 - 2);
                Gl.glVertex3d(x1 - 0.1, y + i * -2, i * -2 - 2);
                Gl.glVertex3d(x1 - 0.1, (y + 0.1) + (i + 1) * -2, (i + 1) * -2 - 0.1);
                Gl.glVertex3d(x2 + 0.1, (y + 0.1) + (i + 1) * -2, (i + 1) * -2 - 0.1);

                Gl.glVertex3d(x1, y + i * -2, i * -2);
                Gl.glVertex3d(x1 -0.1, y + i * -2, i * -2 - 2);
                Gl.glVertex3d(x1 -0.1, y + (i + 1) * -2, (i + 1) * -2);
            }
            Gl.glEnd();

            Gl.glPopMatrix();
        }

        private void DrawSideTribune(float y1, float y2, float x, bool right)
        {
            
            Gl.glPushMatrix();      
            Gl.glColor3f(1f, 0.9f, 0.4f);
            if(right) {
                Gl.glRotated(180, 0, 0, 1);
                Gl.glTranslated(-10, -10, 0);
            }
            Gl.glBegin(Gl.GL_QUADS);
            for (int i = 0; i <= 4; i++)
            {
                Gl.glVertex3d(x + i * -2, y1, i * -2);
                Gl.glVertex3d(x + i * -2, y2, i * -2);
                Gl.glVertex3d(x + i * -2, y2, i * -2 - 2);
                Gl.glVertex3d(x + i * -2, y1, i * -2 - 2);
                                          
                Gl.glVertex3d(x + i * -2, y2, i * -2 - 2);
                Gl.glVertex3d(x + i * -2, y1, i * -2 - 2);
                Gl.glVertex3d(x + (i + 1) * -2, y1, (i + 1) * -2);
                Gl.glVertex3d(x + (i + 1) * -2, y2, (i + 1) * -2);
            }
            Gl.glEnd();

            Gl.glColor3f(0, 0, 0);
            Gl.glLineWidth(1f);
            Gl.glBegin(Gl.GL_LINE_LOOP);
            for (int i = 0; i <= 4; i++)
            {
                Gl.glVertex3d(x + i * -2, y1 - 0.1, i * -2);
                Gl.glVertex3d(x + i * -2, y2 + 0.1, i * -2);
                Gl.glVertex3d(x + i * -2, y2 + 0.1, i * -2 - 2);
                Gl.glVertex3d(x + i * -2, y1 - 0.1, i * -2 - 2);
                Gl.glVertex3d(x + 0.1 + (i + 1) * -2, y1, (i + 1) * -2 - 0.1);
                Gl.glVertex3d(x + 0.1 + (i + 1) * -2, y2, (i + 1) * -2 - 0.1);

                Gl.glVertex3d(x + i * -2, y1, i * -2);
                Gl.glVertex3d(x + i * -2 - 0.1, y1, i * -2 - 2);
                Gl.glVertex3d(x + (i + 1) * -2 - 0.1, y1, (i + 1) * -2);
            }
            Gl.glEnd();

            Gl.glColor3f(1f, 0.9f, 0.4f);
            Gl.glBegin(Gl.GL_TRIANGLES);
            if(right)
            {
                Gl.glVertex3d(x + 2, y2 + 0.1, 0);
                Gl.glVertex3d(x - 8, y2 + 0.1, 0);
                Gl.glVertex3d(x - 8, y2 + 0.1, -12);
            } else
            {
                Gl.glVertex3d(x + 2, y1 - 0.1, 0);
                Gl.glVertex3d(x - 8, y1 - 0.1, 0);
                Gl.glVertex3d(x - 8, y1 - 0.1, -12);
            }
            Gl.glEnd();

            Gl.glPopMatrix();
        }

        double a;

        private void DrawBall()
        {
            Gl.glPushMatrix();
            Gl.glColor3f(0.98f, 0.98f, 0.98f);
            Gl.glTranslated(ballTranslateX, ballTranslateY, ballTranslateZ);

            Gl.glRotated(ballRotateX, 1, 0, 0);

            a = Math.Round(ballRotateX / 90);

            if (a % 2 == 0)
            {
                if (a == 2 || a == 6 || a == 8)
                {
                    Gl.glRotated(ballRotateY, 0, -1, 0);
                    Gl.glRotated(ballRotateZ, 0, 0, 1);
                }
                 else
                {
                    Gl.glRotated(ballRotateY, 0, 1, 0);
                    Gl.glRotated(ballRotateZ, 0, 0, -1);
                }
            }
            else
            {
                if (a == 1 || a == 5 || a == 9)
                {
                    Gl.glRotated(ballRotateZ, 0, 0, -1);
                    Gl.glRotated(ballRotateY, 0, 1, 0);
                }
                else
                {
                    Gl.glRotated(ballRotateZ, 0, 0, 1);
                    Gl.glRotated(ballRotateY, 0, -1, 0);
                }     
            }

            

            Glut.glutSolidSphere(1.5, 15, 15);

            Gl.glColor3f(0.1f, 0.1f, 0.1f);
            /*Glut.glutWireSphere(2, 20, 20);*/

            Gl.glPopMatrix();
        }

        private bool isGoalpostHit(double x, double y)
        {
            return Math.Sqrt(Math.Pow(ballTranslateX - x, 2) + Math.Pow(ballTranslateY - y, 2)) <= ballRadius;
        }

        private void doBooom(float x, float y, float z, float? dx, float? dy, float? dz)
        {
            Random rnd = new Random();
            // устанавливаем новые координаты взрыва
            BOOOOM_1.SetNewPosition(x, y, z);
            // случайную силу
            BOOOOM_1.SetNewPower(rnd.Next(50, 80));
            // и активируем сам взрыв
            BOOOOM_1.Boooom(global_time, dx, dy, dz);
        }

        private static uint MakeGlTexture(int Format, IntPtr pixels, int w, int h)
        {

            uint texObject;
            Gl.glGenTextures(1, out texObject);
            Gl.glPixelStorei(Gl.GL_UNPACK_ALIGNMENT, 1);
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, texObject);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
            Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_REPLACE);

            switch (Format)
            {

                case Gl.GL_RGB:
                    Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, w, h, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, pixels);
                    break;

                case Gl.GL_RGBA:
                    Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, w, h, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, pixels);
                    break;

            }
            return texObject;
        }
    }
}
