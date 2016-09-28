using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS_LAB2
{
    class VectorOperations
    {
        public static Point_3d CoordinatesOfVectorBy2Points(Point_3d p1, Point_3d p2)
        {
            float x = p2.X - p1.X;
            float y = p2.Y - p1.Y;
            float z = p2.Z - p1.Z;

            return new Point_3d(x, y, z);
        }

        public static Point_3d CrossProduct(Point_3d a, Point_3d b)
        {
            float x = a.Y * b.Z - a.Z * b.Y;
            float y = a.Z * b.X - a.X * b.Z;
            float z = a.X * b.Y - a.Y * b.X;

            return new Point_3d(x, y, z);
        }

        public static float LengthOfVector(Point_3d a)
        {
            double sum = a.X * a.X + a.Y * a.Y + a.Z * a.Z;
            return (float)Math.Sqrt(sum);
        }

        public static float DotProduct(Point_3d a, Point_3d b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static float DistanceBetweenTwoPoints(Point_3d a, Point_3d b)
        {
            return (float)(Math.Sqrt(Math.Pow((b.X - a.X), 2) + Math.Pow((b.Y - a.Y), 2) + Math.Pow((b.Z - a.Z), 2)));
        }

        public static Point_3d AdditionOfTwoVectors(Point_3d a, Point_3d b)
        {
            return new Point_3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Point_3d SubtractionOfTwoVectors(Point_3d a, Point_3d b)
        {
            return new Point_3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Point_3d MultVectorOnScalar(Point_3d v, float scalar)
        {
            return new Point_3d(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        public static float ComputeNDotL(Point_3d c, Point_3d normal)
        {
            float l = LengthOfVector(normal);

            float value;
            if (l != 0)
                value = (DotProduct(normal, c)
                            / (l * LengthOfVector(c)));
            else
                value = 1;

            return value;
        }

        public static Point_3d GetNormal(Point_3d a, Point_3d b, Point_3d c)
        {
            Point_3d vector1 = CoordinatesOfVectorBy2Points(a, b);
            Point_3d vector2 = CoordinatesOfVectorBy2Points(a, c);

            Point_3d crossProduct = CrossProduct(vector1, vector2);
            return crossProduct;
        }

        public static Point_3d Normalize(Point_3d a)
        {
            float l = LengthOfVector(a);
            if (l == 0)
                return a;
            float invL = 1 / l;
            float x = a.X * invL;
            float y = a.Y * invL;
            float z = a.Z * invL;
            return new Point_3d(x, y, z);
        }
    }
}
