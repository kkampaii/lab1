using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace IGS_LAB2
{
    class Triangle
    {
        // список всех вершин фигуры, каждая вершина хранится в одном экземпляре
        // этот список используется для того, чтобы позже рассчитать среднюю нормаль каждой вершины
        private static List<Vertex> points = new List<Vertex>();

        // кол-во вершин
        const int N = 3;

        // реальные 3d координаты треугольника
        private Vertex[] points_3d;

        // преобразованные координаты треугольника
        private Point_3d[] new_points_3d;

        // центральная точка экрана
        private static PointF center;

        // конструктор
        public Triangle(Vertex p1, Vertex p2, Vertex p3)
        {
            p1 = AddVertexToList(points, p1);
            p2 = AddVertexToList(points, p2);
            p3 = AddVertexToList(points, p3);

            points_3d = new Vertex[N];
            points_3d[0] = p1;
            points_3d[1] = p2;
            points_3d[2] = p3;
        }

        public static void SetCenter(PointF centerP)
        {
            center = centerP;
        }

        // поворот координат в зависимости от положения наблюдателя
        public void Projection(float[,] matrix)
        {
            new_points_3d = new Point_3d[N];

            for (int i = 0; i < N; i++)
            {
                float[] newXyz = MatrixConversion.MultMatrixOnVector(matrix, points_3d[i].GetXyz());
                new_points_3d[i] = new Point_3d(center.X + newXyz[0], center.Y + newXyz[1], newXyz[2]);
            }
        }

        // отрисовка каркаса
        public void DrawFrame(System.Windows.Forms.PaintEventArgs e, System.Drawing.Pen pen)
        {
            // соединяем преобразованные вершины линией
            e.Graphics.DrawLine(pen, new_points_3d[0].GetPointF(), new_points_3d[1].GetPointF());
            e.Graphics.DrawLine(pen, new_points_3d[1].GetPointF(), new_points_3d[2].GetPointF());
            e.Graphics.DrawLine(pen, new_points_3d[0].GetPointF(), new_points_3d[2].GetPointF());
        }

        // простая flat закраска
        public void DrawFlatShading(System.Windows.Forms.PaintEventArgs e,
                                    SolidBrush brush,
                                    Point_3d c,
                                    System.Drawing.Color outColor,
                                    System.Drawing.Color inColor,
                                    LightParameters lp,
                                    Point_3d cPoint)
        {
            Point_3d normal = VectorOperations.GetNormal(new_points_3d[1], new_points_3d[0], new_points_3d[2]);
            //Point_3d cc = new Point_3d(c.X - center.X, c.Y - center.Y, c.Z);
            float cosOfAngleBeetweenPointAndSpectator = VectorOperations.ComputeNDotL(c, normal);

            Point_3d intensity = ComputeIntensity2(new_points_3d[1], normal, cPoint, lp, cosOfAngleBeetweenPointAndSpectator);
            brush.Color = DefineColor(cosOfAngleBeetweenPointAndSpectator, 1, outColor, inColor, lp, intensity); ;

            PointF[] points_2d = { new_points_3d[0].GetPointF(), new_points_3d[1].GetPointF(), new_points_3d[2].GetPointF() };
            e.Graphics.FillPolygon(brush, points_2d);
        }

        /* 
         Вычисление интенсивности закрашивания точки
         * point - точка для которой ищется интенсивность
         * normal - нормаль в точке point
         * c - вектор наблюдения
         * lp - параметры освещения
         * cosOfAngleBeetweenPointAndSpectator - косинус угла между между нормалью точки и наблюдателем
         */
        private static float ComputeIntensity(Point_3d point, Point_3d normal, Point_3d c, LightParameters lp, float cosOfAngleBeetweenPointAndSpectator)
        {
            //Point_3d vectorBetweenPointAndLight = VectorOperations.CoordinatesOfVectorBy2Points(point, lp.coordinates);
            Point_3d vectorBetweenPointAndLight = VectorOperations.Normalize(VectorOperations.CoordinatesOfVectorBy2Points(point, lp.coordinates));
            normal = VectorOperations.Normalize(normal);
            if (cosOfAngleBeetweenPointAndSpectator < 0)
                normal = VectorOperations.MultVectorOnScalar(normal, -1);

            float cosTeta = VectorOperations.ComputeNDotL(vectorBetweenPointAndLight, normal);

            //float intensity = lp.ka;
            float Ia = lp.ka;
            float I = Ia;
            lp.Ia = Ia;
            lp.Id = 0;
            lp.Is = 0;

            if (cosTeta > 0)
            {
                float dotNV = VectorOperations.ComputeNDotL(vectorBetweenPointAndLight, normal) * 2;
                Point_3d v1 = VectorOperations.MultVectorOnScalar(normal, dotNV);
                Point_3d minusV = VectorOperations.MultVectorOnScalar(vectorBetweenPointAndLight, -1);
                Point_3d res = VectorOperations.AdditionOfTwoVectors(v1, minusV);
                //Point_3d res1 = VectorOperations.CoordinatesOfVectorBy2Points(point, c);
                Point_3d res1 = VectorOperations.Normalize(VectorOperations.CoordinatesOfVectorBy2Points(point, c));

                float k = VectorOperations.ComputeNDotL(res1, res);
                if (k < 0)
                    k = 0;

                float cosAlpha = (float)Math.Pow(k, lp.n);

                float Id = lp.kd * cosTeta;
                float Is = lp.ks * cosAlpha;


                lp.Id = Id;
                lp.Is = Is;

                I += (Id + Is);
                //intensity += (lp.kd * cosTeta + lp.ks * cosAlpha);
            }

            return Clamp(I);
        }

        private static Point_3d ComputeIntensity2(Point_3d point, Point_3d normal, Point_3d c, LightParameters lp, float cosOfAngleBeetweenPointAndSpectator)
        {
            //Point_3d vectorBetweenPointAndLight = VectorOperations.CoordinatesOfVectorBy2Points(point, lp.coordinates);
            Point_3d vectorBetweenPointAndLight = VectorOperations.Normalize(VectorOperations.CoordinatesOfVectorBy2Points(point, lp.coordinates));
            normal = VectorOperations.Normalize(normal);
            if (cosOfAngleBeetweenPointAndSpectator < 0)
                normal = VectorOperations.MultVectorOnScalar(normal, -1);

            float cosTeta = VectorOperations.ComputeNDotL(vectorBetweenPointAndLight, normal);

            float Ia = lp.ka;
            float Id = 0;
            float Is = 0;

            if (cosTeta > 0)
            {
                float dotNV = VectorOperations.ComputeNDotL(vectorBetweenPointAndLight, normal) * 2;
                Point_3d v1 = VectorOperations.MultVectorOnScalar(normal, dotNV);
                Point_3d minusV = VectorOperations.MultVectorOnScalar(vectorBetweenPointAndLight, -1);
                Point_3d res = VectorOperations.AdditionOfTwoVectors(v1, minusV);
                Point_3d res1 = VectorOperations.CoordinatesOfVectorBy2Points(point, c);
                //Point_3d res1 = VectorOperations.Normalize(VectorOperations.CoordinatesOfVectorBy2Points(point, c));

                float k = VectorOperations.ComputeNDotL(res1, res);
                if (k < 0)
                    k = 0;

                float cosAlpha = (float)Math.Pow(k, lp.n);

                Id = lp.kd * cosTeta;
                Is = lp.ks * cosAlpha;

            }

            return new Point_3d(Ia, Id, Is);
        }

        public void DrawFlatShadingWithZbuffer(System.Windows.Forms.PaintEventArgs e,
                                               Pen pen,
                                               Point_3d c,
                                               Color outColor,
                                               Color inColor,
                                               float[,] z,
                                               LightParameters lp,
                                               Point_3d cPoint)
        {
            Point_3d p1 = new_points_3d[0];
            Point_3d p2 = new_points_3d[1];
            Point_3d p3 = new_points_3d[2];

            Point_3d normal = VectorOperations.GetNormal(new_points_3d[1], new_points_3d[0], new_points_3d[2]);
            //Point_3d normal = VectorOperations.Normalize(VectorOperations.GetNormal(new_points_3d[1], new_points_3d[0], new_points_3d[2]));

            float cosOfAngleBeetweenPointAndSpectator = VectorOperations.ComputeNDotL(c, normal);

            Point_3d intensity = ComputeIntensity2(new_points_3d[1], normal, cPoint, lp, cosOfAngleBeetweenPointAndSpectator);

            pen.Color = DefineColor(cosOfAngleBeetweenPointAndSpectator, 1, outColor, inColor, lp, intensity);

            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;

            }

            if (p2.Y > p3.Y)
            {
                var temp = p2;
                p2 = p3;
                p3 = temp;

            }

            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;

            }

            // inverse slopes
            float dP1P2, dP1P3;

            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            // First case where triangles are like that:
            // P1
            // -
            // -- 
            // - -
            // -  -
            // -   - P2
            // -  -
            // - -
            // -
            // P3
            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        ProcessScanLine(e, y, p1, p3, p1, p2, pen, z, outColor, inColor);
                    }
                    else
                    {
                        ProcessScanLine(e, y, p1, p3, p2, p3, pen, z, outColor, inColor);
                    }
                }
            }
            // First case where triangles are like that:
            //       P1
            //        -
            //       -- 
            //      - -
            //     -  -
            // P2 -   - 
            //     -  -
            //      - -
            //        -
            //       P3
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        ProcessScanLine(e, y, p1, p2, p1, p3, pen, z, outColor, inColor);
                    }
                    else
                    {
                        ProcessScanLine(e, y, p2, p3, p1, p3, pen, z, outColor, inColor);
                    }
                }
            }
            //pen.Color = Color.Black;
            //normal = VectorOperations.MultVectorOnScalar(normal, -1);
            //e.Graphics.DrawLine(pen, new_points_3d[1].X, new_points_3d[1].Y, normal.X, normal.Y);
        }

        public void DrawGouraudShading(System.Windows.Forms.PaintEventArgs e,
                                       Pen pen,
                                       Point_3d c,
                                       Color outColor,
                                       Color inColor,
                                       float[,] z,
                                       LightParameters lp,
                                       Point_3d cPoint)
        {
            Point_3d p1 = new_points_3d[0];
            Point_3d p2 = new_points_3d[1];
            Point_3d p3 = new_points_3d[2];

            Point_3d mainNormal = VectorOperations.GetNormal(new_points_3d[1], new_points_3d[0], new_points_3d[2]);
            float cosOfAngleBeetweenPointAndSpectator = VectorOperations.ComputeNDotL(c, mainNormal);
            //Point_3d secondNormal = VectorOperations.GetNormal(new_points_3d[0], new_points_3d[1], new_points_3d[2]);

            //Point_3d normal1 = VectorOperations.Normalize(points_3d[0].GetNormalOfPoint());
            //Point_3d normal1 = VectorOperations.GetNormal(new_points_3d[1], new_points_3d[0], new_points_3d[2]);
            Point_3d normal1 = points_3d[0].GetNormalOfPoint();
            //float nl11 = ComputeIntensity(new_points_3d[0], normal1, cPoint, lp, cosOfAngleBeetweenPointAndSpectator);
            Point_3d nl1 = ComputeIntensity2(new_points_3d[0], normal1, cPoint, lp, cosOfAngleBeetweenPointAndSpectator);


            //Point_3d normal2 = VectorOperations.GetNormal(new_points_3d[1], new_points_3d[0], new_points_3d[2]);
            //Point_3d normal2 = VectorOperations.Normalize(points_3d[1].GetNormalOfPoint());
            Point_3d normal2 = points_3d[1].GetNormalOfPoint();
            //float nl22 = ComputeIntensity(new_points_3d[1], normal2, cPoint, lp, cosOfAngleBeetweenPointAndSpectator);
            //Point_3d nl2 = new Point_3d(lp.Ia, lp.Id, lp.Is);
            Point_3d nl2 = ComputeIntensity2(new_points_3d[1], normal2, cPoint, lp, cosOfAngleBeetweenPointAndSpectator);


            //Point_3d normal3 = VectorOperations.GetNormal(new_points_3d[1], new_points_3d[0], new_points_3d[2]);
            //Point_3d normal3 = VectorOperations.Normalize(points_3d[2].GetNormalOfPoint());
            Point_3d normal3 = points_3d[2].GetNormalOfPoint();
            //float nl33 = ComputeIntensity(new_points_3d[2], normal3, cPoint, lp, cosOfAngleBeetweenPointAndSpectator);
            //Point_3d nl3 = new Point_3d(lp.Ia, lp.Id, lp.Is);
            Point_3d nl3 = ComputeIntensity2(new_points_3d[2], normal3, cPoint, lp, cosOfAngleBeetweenPointAndSpectator);

            /*Point_3d normal = VectorOperations.GetNormal(new_points_3d[1], new_points_3d[0], new_points_3d[2]);

            float cosOfAngleBeetweenPointAndSpectator = VectorOperations.ComputeNDotL(c, normal);*/
            float intensity = ComputeIntensity(new_points_3d[1], mainNormal, cPoint, lp, cosOfAngleBeetweenPointAndSpectator);


            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;

                Point_3d temp2 = nl2;
                nl2 = nl1;
                nl1 = temp2;
            }

            if (p2.Y > p3.Y)
            {
                var temp = p2;
                p2 = p3;
                p3 = temp;

                Point_3d temp2 = nl2;
                nl2 = nl3;
                nl3 = temp2;
            }

            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;

                Point_3d temp2 = nl2;
                nl2 = nl1;
                nl1 = temp2;
            }


            // inverse slopes
            float dP1P2, dP1P3;

            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            // First case where triangles are like that:
            // P1
            // -
            // -- 
            // - -
            // -  -
            // -   - P2
            // -  -
            // - -
            // -
            // P3
            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        ProcessScanLine(e, y, p1, p3, p1, p2, pen, z, outColor, inColor, nl1, nl3, nl1, nl2, cosOfAngleBeetweenPointAndSpectator, lp);
                    }
                    else
                    {
                        ProcessScanLine(e, y, p1, p3, p2, p3, pen, z, outColor, inColor, nl1, nl3, nl2, nl3, cosOfAngleBeetweenPointAndSpectator, lp);
                    }
                }
            }
            // First case where triangles are like that:
            //       P1
            //        -
            //       -- 
            //      - -
            //     -  -
            // P2 -   - 
            //     -  -
            //      - -
            //        -
            //       P3
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        ProcessScanLine(e, y, p1, p2, p1, p3, pen, z, outColor, inColor, nl1, nl2, nl1, nl3, cosOfAngleBeetweenPointAndSpectator, lp);
                    }
                    else
                    {
                        ProcessScanLine(e, y, p2, p3, p1, p3, pen, z, outColor, inColor, nl2, nl3, nl1, nl3, cosOfAngleBeetweenPointAndSpectator, lp);
                    }
                }
            }
        }

        public void DrawPhongShading(System.Windows.Forms.PaintEventArgs e,
                                     Pen pen,
                                     Point_3d c,
                                     System.Drawing.Color outColor,
                                     System.Drawing.Color inColor,
                                     float[,] z,
                                     LightParameters lp,
                                     Point_3d cPoint)
        {
            Point_3d p1 = new_points_3d[0];
            Point_3d p2 = new_points_3d[1];
            Point_3d p3 = new_points_3d[2];

            //Point_3d normal1 = VectorOperations.GetNormal(points_3d[0], points_3d[1], points_3d[2]);
            //Point_3d normal2 = VectorOperations.GetNormal(points_3d[0], points_3d[1], points_3d[2]);
            //Point_3d normal3 = VectorOperations.GetNormal(points_3d[0], points_3d[1], points_3d[2]);

            /*Point_3d normal = VectorOperations.GetNormal(new_points_3d[1], new_points_3d[0], new_points_3d[2]);

           /I Point_3d normal1 = normal;
            Point_3d normal2 = normal;
            Point_3d normal3 = normal;*/

            Point_3d normal1 = points_3d[0].GetNormalOfPoint();
            Point_3d normal2 = points_3d[1].GetNormalOfPoint();
            Point_3d normal3 = points_3d[2].GetNormalOfPoint();

            Point_3d normal = VectorOperations.GetNormal(new_points_3d[1], new_points_3d[0], new_points_3d[2]);

            float intensity1 = VectorOperations.ComputeNDotL(c, VectorOperations.GetNormal(new_points_3d[1], new_points_3d[0], new_points_3d[2]));
            //Point_3d normal2 = VectorOperations.GetNormal(points_3d[1], points_3d[0], points_3d[2]);
            //Point_3d normal3 = VectorOperations.GetNormal(points_3d[2], points_3d[1], points_3d[0]);

            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;

                Point_3d temp2 = normal2;
                normal2 = normal1;
                normal1 = temp2;
            }

            if (p2.Y > p3.Y)
            {
                var temp = p2;
                p2 = p3;
                p3 = temp;

                Point_3d temp2 = normal2;
                normal2 = normal3;
                normal3 = temp2;
            }

            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;

                Point_3d temp2 = normal2;
                normal2 = normal1;
                normal1 = temp2;
            }

            float dP1P2, dP1P3;

            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            // First case where triangles are like that:
            // P1
            // -
            // -- 
            // - -
            // -  -
            // -   - P2
            // -  -
            // - -
            // -
            // P3
            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        ProcessScanLine(e, y, p1, p3, p1, p2, pen, z, outColor, inColor, normal1, normal3, normal1, normal2, c, lp, intensity1, cPoint);
                    }
                    else
                    {
                        ProcessScanLine(e, y, p1, p3, p2, p3, pen, z, outColor, inColor, normal1, normal3, normal2, normal3, c, lp, intensity1, cPoint);
                    }
                }
            }
            // First case where triangles are like that:
            //       P1
            //        -
            //       -- 
            //      - -
            //     -  -
            // P2 -   - 
            //     -  -
            //      - -
            //        -
            //       P3
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        ProcessScanLine(e, y, p1, p2, p1, p3, pen, z, outColor, inColor, normal1, normal2, normal1, normal3, c, lp, intensity1, cPoint);
                    }
                    else
                    {
                        ProcessScanLine(e, y, p2, p3, p1, p3, pen, z, outColor, inColor, normal2, normal3, normal1, normal3, c, lp, intensity1, cPoint);
                    }
                }
            }
        }

        private static float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        private static float Interpolate(float min, float max, float gradient)
        {

            return min + (max - min) * Clamp(gradient);
        }

        private static Point_3d Interpolate(Point_3d min, Point_3d max, float gradient)
        {
            Point_3d sub1 = VectorOperations.SubtractionOfTwoVectors(max, min);
            Point_3d mult2 = VectorOperations.MultVectorOnScalar(sub1, Clamp(gradient));
            return VectorOperations.AdditionOfTwoVectors(min, mult2);

            //return min + (max - min) * Clamp(gradient);

            /*Point_3d mult1 = VectorOperations.MultVectorOnScalar(min, gradient);
            Point_3d mult2 = VectorOperations.MultVectorOnScalar(max, (1 - gradient));
            return VectorOperations.AdditionOfTwoVectors(mult1, mult2);*/
        }

        private void ProcessScanLine(System.Windows.Forms.PaintEventArgs e, int y, Point_3d pa, Point_3d pb, Point_3d pc, Point_3d pd, Pen pen, float[,] zBuf,
            Color outColor, Color inColor)
        {
            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            var gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);


            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            // drawing a line from left (sx) to right (ex) 
            for (var x = sx; x < ex; x++)
            {
                //DrawPoint(new Vector2(x, y), color);
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                if (zBuf[x, y] < z)
                {
                    e.Graphics.DrawEllipse(pen, x, y, 1, 1);
                    zBuf[x, y] = z;
                }
            }
        }

        private void ProcessScanLine(System.Windows.Forms.PaintEventArgs e, int y, Point_3d pa, Point_3d pb, Point_3d pc, Point_3d pd, Pen pen, float[,] zBuf,
            Color outColor, Color inColor, float ndotla, float ndotlb, float ndotlc, float ndotld, float intensity1)
        {
            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            var gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);


            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            var snl = Interpolate(ndotla, ndotlb, gradient1);
            var enl = Interpolate(ndotlc, ndotld, gradient2);

            // drawing a line from left (sx) to right (ex) 
            for (var x = sx; x < ex; x++)
            {
                //DrawPoint(new Vector2(x, y), color);
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                var ndotl = Interpolate(snl, enl, gradient);
                /*if (Math.Abs(ndotl) > 1)
                {
                    int bf = 0;
                }*/
                pen.Color = DefineColor(intensity1, ndotl, outColor, inColor);

                if (zBuf[x, y] < z)
                {
                    e.Graphics.DrawEllipse(pen, x, y, 1, 1);
                    zBuf[x, y] = z;
                }
            }
        }

        private void ProcessScanLine(System.Windows.Forms.PaintEventArgs e, int y, Point_3d pa, Point_3d pb, Point_3d pc, Point_3d pd, Pen pen, float[,] zBuf,
            Color outColor, Color inColor, Point_3d ndotla, Point_3d ndotlb, Point_3d ndotlc, Point_3d ndotld, float intensity1, LightParameters lp)
        {
            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            var gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);


            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            var snl = Interpolate(ndotla, ndotlb, gradient1);
            var enl = Interpolate(ndotlc, ndotld, gradient2);

            // drawing a line from left (sx) to right (ex) 
            for (var x = sx; x < ex; x++)
            {
                //DrawPoint(new Vector2(x, y), color);
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                var ndotl = Interpolate(snl, enl, gradient);
                /*if (Math.Abs(ndotl) > 1)
                {
                    int bf = 0;
                }*/
                pen.Color = DefineColor(intensity1, 10, outColor, inColor, lp, ndotl);

                if (zBuf[x, y] < z)
                {
                    e.Graphics.DrawEllipse(pen, x, y, 1, 1);
                    zBuf[x, y] = z;
                }
            }
        }

        private void ProcessScanLine(System.Windows.Forms.PaintEventArgs e, int y, Point_3d pa, Point_3d pb, Point_3d pc, Point_3d pd, Pen pen, float[,] zBuf,
            Color outColor, Color inColor, Point_3d normal1, Point_3d normal2, Point_3d normal3, Point_3d normal4, Point_3d c, LightParameters lp, float intensity1, Point_3d cPoint)
        {
            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            var gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);


            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            Point_3d snl = Interpolate(normal1, normal2, gradient1);
            Point_3d enl = Interpolate(normal3, normal4, gradient2);


            /*var snl = Interpolate(ndotla, ndotlb, gradient1);
            var enl = Interpolate(ndotlc, ndotld, gradient2);*/

            // drawing a line from left (sx) to right (ex) 
            for (var x = sx; x < ex; x++)
            {
                //DrawPoint(new Vector2(x, y), color);
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                Point_3d normal = Interpolate(snl, enl, gradient);
                Point_3d point = new Point_3d(x, y, z);
                /*float cosTeta = VectorOperations.ComputeNDotL(VectorOperations.CoordinatesOfVectorBy2Points(point, lp.coordinates), normal);

                //if (cosTeta < 0)
                //cosTeta = Math.Abs(cosTeta);

                float intensity = lp.kd * Clamp(Math.Abs(cosTeta));*/


                float intensity = ComputeIntensity(point, normal, cPoint, lp, intensity1);
                pen.Color = DefineColor(intensity1, intensity, outColor, inColor, lp);
                //pen.Color = DefineColor(intensity1, intensity, outColor, inColor);

                if (zBuf[x, y] < z)
                {
                    e.Graphics.DrawEllipse(pen, x, y, 1, 1);
                    zBuf[x, y] = z;
                }
            }
        }

        public void AddNormalsToVertices()
        {
            Point_3d normal1 = VectorOperations.GetNormal(new_points_3d[1], new_points_3d[0], new_points_3d[2]);

            foreach (Vertex p in points_3d)
            {
                p.AddNormal(normal1);
            }
        }

        private static Color DefineColor(double intensity, Color outColor, Color inColor)
        {

            Color clr;

            if (intensity < 0)
            {
                intensity = Math.Abs(intensity);
                clr = Color.FromArgb((int)(outColor.R * intensity), (int)(outColor.G * intensity), (int)(outColor.B * intensity));
            }
            else
            {
                clr = Color.FromArgb((int)(inColor.R * intensity), (int)(inColor.G * intensity), (int)(inColor.B * intensity));
            }

            return clr;
        }

        private static Color DefineColor(double intensity1, double intensity, Color outColor, Color inColor)
        {
            if (intensity1 > 0)
            {
                return Color.FromArgb((int)(outColor.R * intensity), (int)(outColor.G * intensity), (int)(outColor.B * intensity));
            }
            else
            {
                return Color.FromArgb((int)(inColor.R * intensity), (int)(inColor.G * intensity), (int)(inColor.B * intensity));
            }

        }

        private static Color DefineColor(double intensity1, double intensity, Color outColor, Color inColor, LightParameters lp)
        {
            Color surfaceColor;

            Color diffusedColor = lp.diffusedColor;
            float difR = diffusedColor.R / 255F;
            float difG = diffusedColor.G / 255F;
            float difB = diffusedColor.B / 255F;

            Color dottedColor = lp.dottedColor;
            float dotR = dottedColor.R / 255F;
            float dotG = dottedColor.G / 255F;
            float dotB = dottedColor.B / 255F;

            if (intensity1 > 0)
            {
                surfaceColor = outColor;
                //return Color.FromArgb((int)(outColor.R * intensity), (int)(outColor.G * intensity), (int)(outColor.B * intensity));
            }
            else
            {
                surfaceColor = inColor;
                //return Color.FromArgb((int)(inColor.R * intensity), (int)(inColor.G * intensity), (int)(inColor.B * intensity));
            }

            //Color res = Color.FromArgb((int)(surfaceColor.R * (lp.Ia * difR)), (int)(surfaceColor.G * (lp.Ia * difG)), (int)(surfaceColor.B * (lp.Ia * difB)));

            /*int r = (int)(Clamp(surfaceColor.R * ((lp.Id + lp.Is) * dotR + lp.Ia * dotR) + lp.Is * dotR,0,255));
            int g = (int)(Clamp(surfaceColor.G * ((lp.Id + lp.Is) * dotG + lp.Ia * dotG), 0, 255));
            int b = (int)(Clamp(surfaceColor.B * ((lp.Id + lp.Is) * dotB + lp.Ia * dotB), 0, 255));*/

            int r = (int)(Clamp(surfaceColor.R * (lp.Id * dotR + lp.Ia * difR) + lp.Is * dottedColor.R, 0, 255));
            int g = (int)(Clamp(surfaceColor.G * (lp.Id * dotG + lp.Ia * difG) + lp.Is * dottedColor.G, 0, 255));
            int b = (int)(Clamp(surfaceColor.B * (lp.Id * dotB + lp.Ia * difB) + lp.Is * dottedColor.B, 0, 255));


            //Color res = Color.FromArgb((int)(surfaceColor.R * (lp.Ia * difR)), (int)(surfaceColor.G * (lp.Ia * difG)), (int)(surfaceColor.B * (lp.Ia * difB)));
            Color res = Color.FromArgb(r, g, b);

            return res;
        }

        private static Color DefineColor(double intensity1, double intensity, Color outColor, Color inColor, LightParameters lp, Point_3d parameters)
        {
            Color surfaceColor;

            Color diffusedColor = lp.diffusedColor;
            float difR = diffusedColor.R / 255F;
            float difG = diffusedColor.G / 255F;
            float difB = diffusedColor.B / 255F;

            Color dottedColor = lp.dottedColor;
            float dotR = dottedColor.R / 255F;
            float dotG = dottedColor.G / 255F;
            float dotB = dottedColor.B / 255F;

            if (intensity1 > 0)
            {
                surfaceColor = outColor;
                //return Color.FromArgb((int)(outColor.R * intensity), (int)(outColor.G * intensity), (int)(outColor.B * intensity));
            }
            else
            {
                surfaceColor = inColor;
                //return Color.FromArgb((int)(inColor.R * intensity), (int)(inColor.G * intensity), (int)(inColor.B * intensity));
            }

            /*
            int r = (int)(Clamp(surfaceColor.R * (lp.Id * dotR + lp.Ia * difR) + lp.Is * dottedColor.R, 0, 255));
            int g = (int)(Clamp(surfaceColor.G * (lp.Id * dotG + lp.Ia * difG) + lp.Is * dottedColor.G, 0, 255));
            int b = (int)(Clamp(surfaceColor.B * (lp.Id * dotB + lp.Ia * difB) + lp.Is * dottedColor.B, 0, 255));*/

            int r = (int)(Clamp(surfaceColor.R * (parameters.Y * dotR + parameters.X * difR) + parameters.Z * dottedColor.R, 0, 255));
            int g = (int)(Clamp(surfaceColor.G * (parameters.Y * dotG + parameters.X * difG) + parameters.Z * dottedColor.G, 0, 255));
            int b = (int)(Clamp(surfaceColor.B * (parameters.Y * dotB + parameters.X * difB) + parameters.Z * dottedColor.B, 0, 255));


            //Color res = Color.FromArgb((int)(surfaceColor.R * (lp.Ia * difR)), (int)(surfaceColor.G * (lp.Ia * difG)), (int)(surfaceColor.B * (lp.Ia * difB)));
            Color res = Color.FromArgb(r, g, b);

            return res;
        }

        private static Vertex AddVertexToList(List<Vertex> list, Vertex point)
        {
            // если точно такая же вершина отсутвует в списке вершин, то добавляем её
            if (!list.Contains(point))
            {
                list.Add(point);
                return point;
            }

            // иначе ссылаемся уже на вершину, которая есть в списке
            else
            {
                return list[list.IndexOf(point)];
            }
        }

        // усреднение нормалей в вершинах
        public static void ComputeNormalsInVertices()
        {
            foreach (Vertex p in points)
            {
                //int n = p.getSize();
                p.ComputeNormal();
            }
        }

        public static void ClearNormals()
        {
            foreach (Vertex p in points)
            {
                p.ClearNormals();
            }
        }

        public static void ClearPoints()
        {
            points = new List<Vertex>();
        }
    }
}
