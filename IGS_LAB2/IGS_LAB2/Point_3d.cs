using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace IGS_LAB2
{
    class Point_3d
    {
        // координаты 3d точки или вектора
        public float X;
        public float Y;
        public float Z;

        // конструктор
        public Point_3d(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        // возвращение массива для проведения матричных преобразований
        public float[] GetXyz()
        {
            return new float[] { X, Y, Z, 1 };
        }

        // отбрасывание z координаты (для отрисовки)
        public PointF GetPointF()
        {
            return new PointF(X, Y);
        }

        private void test()
        {
        }
    }
}
