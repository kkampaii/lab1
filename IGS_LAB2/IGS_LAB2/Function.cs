using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS_LAB2
{
    static class Function
    {

        public static List<Triangle> CountPolygons(float u_min, float u_max, float v_min, float v_max, float du, float dv, float param1, float param2)
        {
            List<Triangle> triangles = new List<Triangle>();
            for (float u = u_min; u <= u_max; u += du)
            {
                for (float v = v_min; v <= v_max; v += dv)
                {
                    Vertex p1 = R(u, v, param1, param2);
                    Vertex p2 = R(u + du, v, param1, param2);
                    Vertex p3 = R(u + du, v + dv, param1, param2);
                    Vertex p4 = R(u, v + dv, param1, param2);

                    Triangle t1 = new Triangle(p1, p2, p3);
                    Triangle t2 = new Triangle(p3, p4, p1);
                    triangles.Add(t1);
                    triangles.Add(t2);
                }
            }
            return triangles;
        }

        private static Vertex R(float u, float v, float a, float b)
        {
            float x = (float)(Math.Cos(u) * (b + a * Math.Cos(v))*2);
            float y = (float)(Math.Sin(u) * (b + a * Math.Cos(v))*2);
            float z = (float)(a * Math.Sin(v)*2);
            return new Vertex(x, y, z);
        }
    }
}
