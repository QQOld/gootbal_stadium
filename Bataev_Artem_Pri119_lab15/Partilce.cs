using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bataev_Artem_Pri119_lab15
{
    class Partilce
    {
        private Random _rnd;
        // позиция частицы
        private float[] position = new float[3];
        // поворот частицы
        private float[] rotation = new float[3];
        // размер
        private float _size;
        // время жизни
        private float _lifeTime;

        // вектор гравитации
        private float[] Grav = new float[3];
        // ускорение частицы
        private float[] power = new float[3];
        // коэфицент затухания силы
        private float attenuation;

        // набранная скорость
        private float[] speed = new float[3];

        // временной интервал активации частицы
        private float LastTime = 0;

        // конструктор класса
        public Partilce(float x, float y, float z, float size, float lifeTime, float start_time, Random rnd)
        {
            // записываем все начальные настройки частицы, устанавливаем начальный коэфицент затухания
            // и обнуляем скорость и силу, приложенную к частице
            _size = size;
            _lifeTime = lifeTime;
            _rnd = rnd;

            position[0] = x;
            position[1] = y;
            position[2] = z;

            rotation[0] = _rnd.Next(360);
            rotation[1] = _rnd.Next(360);
            rotation[2] = _rnd.Next(360);

            speed[0] = 0;
            speed[1] = 0;
            speed[2] = 0;

            Grav[0] = 0;
            Grav[1] = -9.8f;
            Grav[2] = 0;

            attenuation = 3.33f;

            power[0] = 0;
            power[1] = 0;
            power[2] = 0;

            LastTime = start_time;

        }

        // функция установка ускорения, действующего на частицу
        public void SetPower(float x, float y, float z)
        {
            power[0] = x;
            power[1] = y;
            power[2] = z;
        }

        // инвертирование скорости частицы по заданной оси с указанным затуханием
        // удобно использовать для простой демонстрации столкновений, например с землей
        public void InvertSpeed(int os, float attenuation)
        {
            speed[os] *= -1 * attenuation;
        }

        // получение размера частицы
        public float GetSize()
        {
            return _size;
        }

        // установка нового значения затухания
        public void setAttenuation(float new_value)
        {
            attenuation = new_value;
        }

        // обновление позиции частицы
        public void UpdatePosition(float timeNow)
        {
            // орпределяем разницу во времени, прошедшую с последнего обновления
            // позиции частицы (ведь таймер может быть не фиксированный)
            float dTime = timeNow - LastTime;
            _lifeTime -= dTime;

            // обновляем последнюю отметку временного интервала
            LastTime = timeNow;

            // перерасчитываем ускорение, движущее частицу, с учетом затухания
            for (int a = 0; a < 3; a++)
            {
                if (power[a] > 0)
                {
                    power[a] -= attenuation * dTime;

                    if (power[a] <= 0)
                        power[a] = 0;
                }

                // перерасчитываем позицию частицы с учетом гравитации, вектора ускорения и прощедшего промежутка времени
                position[a] += (speed[a] * dTime + (Grav[a] + power[a]) * dTime * dTime);

                rotation[a] += _rnd.Next(1, 15) * (_lifeTime / 5);

                // обновляем скорость частицы
                speed[a] += (Grav[a] + power[a]) * dTime;
            }
        }

        // проверка, не закончилось ли время жизни частицы
        public bool isLife()
        {
            if (_lifeTime > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // получение координат частицы
        public float GetPositionX()
        {
            return position[0];
        }
        public float GetPositionY()
        {
            return position[1];
        }
        public float GetPositionZ()
        {
            return position[2];
        }

        public float GetRotationX()
        {
            return rotation[0];
        }
        public float GetRotationY()
        {
            return rotation[1];
        }
        public float GetRotationZ()
        {
            return rotation[2];
        }
    }
}
