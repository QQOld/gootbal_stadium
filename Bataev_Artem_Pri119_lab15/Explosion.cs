using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;

namespace Bataev_Artem_Pri119_lab15
{
    class Explosion
    {
        // позиция взрыва
        private float[] position = new float[3];
        // мощность
        private float _power;
        // максимальное количество частиц
        private int MAX_PARTICLES = 1000;
        // текущее установленное количество частиц
        private int _particles_now;

        // активирован
        private bool isStart = false;

        // массив частиц на основе созданного ранее класса
        private Partilce[] PartilceArray;

        // дисплейный список для рисования частицы создан
        private bool isDisplayList = false;
        // номер дисплейного списка для отрисовки
        private int DisplayListNom = 0;

        private Random rnd = new Random();

        // конструктор класса; в него передаются координаты, где должен произойти взрыв, мощность и количество чатиц
        public Explosion(float x, float y, float z, float power, int particle_count)
        {
            position[0] = x;
            position[1] = y;
            position[2] = z;

            _particles_now = particle_count;
            _power = power;

            // если число частиц превышает максимально разрешенное
            if (particle_count > MAX_PARTICLES)
            {
                particle_count = MAX_PARTICLES;
            }

            // создаем массив частиц необходимого размера
            PartilceArray = new Partilce[particle_count];
        }

        // функция обновления позиции взрыва
        public void SetNewPosition(float x, float y, float z)
        {
            position[0] = x;
            position[1] = y;
            position[2] = z;
        }

        // установка нового значения мощности взрыва
        public void SetNewPower(float new_power)
        {
            _power = new_power;
        }

        // создания дисплейного списка для отрисовки частицы (т.к. отрисовывать даже небольшой полигон такое количество раз очень накладно)
        private void CreateDisplayList()
        {
            // генерация дисплейного списка
            DisplayListNom = Gl.glGenLists(1);

            // начало создания списка
            Gl.glNewList(DisplayListNom, Gl.GL_COMPILE);

            // режим отрисовки треугольника
            Gl.glBegin(Gl.GL_TRIANGLES);

            Gl.glColor3d(0.7, 0.2, 0.4);

            // задаем форму частицы
            Gl.glVertex3d(0, 0, 0);
            Gl.glVertex3d(0.02f, 0.02f, 0);
            Gl.glVertex3d(0.02f, 0, -0.02f);

            Gl.glEnd();

            // завершаем отрисовку частицы
            Gl.glEndList();

            // флаг - дисплейный список создан
            isDisplayList = true;
        }

        // функция, реализующая взрыв
        public void Boooom(float time_start, float? dx, float? dy, float? dz)
        {
            // инициализируем экземпляр класса Random
            Random rnd = new Random();

            // если дисплейный список не создан, надо его создать
            if (!isDisplayList)
            {
                CreateDisplayList();
            }

            // по всем частицам
            for (int ax = 0; ax < _particles_now; ax++)
            {
                // создаем частицу
                PartilceArray[ax] = new Partilce(position[0] + rnd.Next(-2, 3), position[1] + rnd.Next(-2, 3), position[2] + rnd.Next(-2, 3), 15.0f, 5, time_start, rnd);

                // случайным образом генериуем ориентацию вектора ускорения для данной частицы

                float direction_x;
                float direction_y;
                float direction_z;

                if (dx != null)
                {
                    direction_x = (float)dx;
                } else
                {
                    direction_x = rnd.Next(1, 3);
                    if (direction_x == 2) direction_x = -1;
                }

                if (dy != null)
                {
                    direction_y = (float)dy;
                }
                else
                {
                    direction_y = rnd.Next(1, 3);
                    if (direction_y == 2) direction_y = -1;
                }

                if (dz != null)
                {
                    direction_z = (float)dz;
                }
                else
                {
                    direction_z = rnd.Next(1, 3);
                    if (direction_z == 2) direction_z = -1;
                }

                // задаем мощность в промежутке от 5 до 100% от указанной (чтобы частицы имели разное ускорение)
                float _power_rnd = rnd.Next((int)_power / 20, (int)_power);
                // устанавливаем затухание, равное 50% от мощности
                PartilceArray[ax].setAttenuation(_power / 2.0f);
                // устанавливаем ускорение частицы, еще раз генерируя случайное число
                // таким образом мощность определится от 10 - до 100% полученной
                // Здесь же применяем ориентацию для векторов ускорения
                PartilceArray[ax].SetPower(_power_rnd * ((float)rnd.Next(100, 1000) / 1000.0f) * direction_x, _power_rnd * ((float)rnd.Next(100, 1000) / 1000.0f) * direction_y, _power_rnd * ((float)rnd.Next(100, 1000) / 1000.0f) * direction_z);
            }

            // взрыв активирован
            isStart = true;
        }

        // калькуляция текущего взрыва
        public void Calculate(float time)
        {
            Random rnd = new Random();
            // только в том случае, если взрыв уже активирован
            if (isStart)
            {
                // проходим циклом по всем частицам
                for (int ax = 0; ax < _particles_now; ax++)
                {
                    // если время жизни частицы еще не вышло
                    if (PartilceArray[ax].isLife())
                    {
                        // обновляем позицию частицы
                        PartilceArray[ax].UpdatePosition(time);

                        // сохраняем текущую матрицу
                        Gl.glPushMatrix();
                        // получаем размер частицы
                        float size = PartilceArray[ax].GetSize();

                        // выполняем перемещение частицы в необходимую позицию
                        Gl.glTranslated(PartilceArray[ax].GetPositionX(), PartilceArray[ax].GetPositionY(), PartilceArray[ax].GetPositionZ());

                        Gl.glRotated(PartilceArray[ax].GetRotationX(), 1, 0, 0);
                        Gl.glRotated(PartilceArray[ax].GetRotationY(), 0, 1, 0);
                        Gl.glRotated(PartilceArray[ax].GetRotationZ(), 0, 0, 1);

                        // масштабируем ее в соотвествии с ее размером
                        Gl.glScalef(size, size, size);
                        // вызываем дисплейный список для отрисовки частицы из кеша видеоадаптера
                        Gl.glCallList(DisplayListNom);
                        // возвращаем матрицу
                        Gl.glPopMatrix();

                        // отражение от "земли"
                        // если координата Y стала меньше нуля (удар о землю)
                        /*if (PartilceArray[ax].GetPositionY() < 0)
                        {
                            // инвертируем проекцию скорости на ось Y, как будто частица ударилась и отскочила от земли
                            // причем скорость затухает на 40%
                            PartilceArray[ax].InvertSpeed(1, 0.6f);
                        }*/
                    }

                }
            }
        }
    }
}
