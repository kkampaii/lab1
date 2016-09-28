using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS_LAB2
{
    // реализует функция Equals
    class Vertex : Point_3d, IEquatable<Vertex>
    {
        // список нормалей полигонов, в которые входит точка
        private List<Point_3d> normals;

        // нормаль вершины
        private Point_3d normal;

        public Vertex(float x, float y, float z)
            : base(x, y, z)
        {

        }

        // функция сравнения
        public bool Equals(Vertex other)
        {
            // если три координаты одной точки равны трем координатам другой точки, то точки равны, иначе не равны
            if (this.X == other.X && this.Y == other.Y
                && this.Z == other.Z)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // добавление нормали в список нормалей вершины
        public void AddNormal(Point_3d normal)
        {
            if (normals == null)
            {
                normals = new List<Point_3d>();
            }
            normals.Add(normal);
        }

        // подсчет нормали в вершине, путем усреднения нормалей из списка
        public void ComputeNormal()
        {
            normal = new Point_3d(0, 0, 0);
            foreach (Point_3d n in normals)
                normal = VectorOperations.AdditionOfTwoVectors(normal, n);

            normal = VectorOperations.MultVectorOnScalar(normal, 1.0F / normals.Count);
        }

        // возвращение нормали
        public Point_3d GetNormalOfPoint()
        {
            return normal;
        }

        public void ClearNormals()
        {
            normals = null;
        }
    }
}
