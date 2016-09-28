using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGS_LAB2
{
    class MatrixConversion
    {
        private const int N = 4;

        public static float[] MultMatrixOnVector(float[,] matrix, float[] vector)
        {
            float[] res = new float[N];
            for (int i = 0; i < N; i++)
            {
                res[i] = 0;
                for (int j = 0; j < N; j++)
                    res[i] += vector[j] * matrix[j, i];
            }
            return res;
        }

        public static float[,] MultOfMatrix(float[,] m1, float[,] m2)
        {
            float[,] res = new float[N, N];

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    float sum = 0;
                    for (int k = 0; k < N; k++)
                        sum += m1[i, k] * m2[k, j];
                    res[i, j] = sum;
                }
            }

            return res;
        }

        public static float[,] Rx(float angle)
        {
            float[,] res = {
                    {1, 0, 0, 0},
                    {0, (float) Math.Cos(angle), (float) Math.Sin(angle), 0},
                    {0, (float) (-Math.Sin(angle)), (float) Math.Cos(angle), 0},
                    {0, 0, 0, 1},
            };
            return res;
        }

        public static float[,] Ry(float angle)
        {
            float[,] res = {
                    {(float) Math.Cos(angle), 0, (float) -(Math.Sin(angle)), 0},
		            {0, 1, 0, 0},
		            {(float) Math.Sin(angle), 0, (float) Math.Cos(angle), 0},
		            {0, 0, 0, 1}
            };
            return res;
        }

        public static float[,] Rz(float angle)
        {
            float[,] res = {
                    {(float) Math.Cos(angle), (float) Math.Sin(angle), 0, 0},
		            {(float) -(Math.Sin(angle)), (float) Math.Cos(angle), 0, 0},
		            {0, 0, 1, 0},
		            {0, 0, 0, 1}
            };
            return res;
        }

        public static float[,] Mx()
        {
            float[,] res = {
                    {-1, 0, 0, 0},
		            {0, 1, 0, 0},
		            {0, 0, 1, 0},
		            {0, 0, 0, 1}
            };
            return res;
        }

        public static float[,] My()
        {
            float[,] res = {
                    {1, 0, 0, 0},
		            {0, -1, 0, 0},
		            {0, 0, 1, 0},
		            {0, 0, 0, 1}
            };
            return res;
        }

        public static float[,] Mz()
        {
            float[,] res = {
                    {1, 0, 0, 0},
		            {0, 1, 0, 0},
		            {0, 0, -1, 0},
		            {0, 0, 0, 1}
            };
            return res;
        }

        public static float[,] D(float alpha, float beta, float gamma)
        {
            float[,] res = {
                    {alpha, 0, 0, 0},
		            {0, beta, 0, 0},
		            {0, 0, gamma, 0},
		            {0, 0, 0, 1}
            };
            return res;
        }

        public static float[,] T(float g, float m, float v)
        {
            float[,] res = {
                    {1, 0, 0, 0},
		            {0, 1, 0, 0},
		            {0, 0, 1, 0},
		            {g, m, v, 1}
            };
            return res;
        }

        public static float[,] Px()
        {
            float[,] res = {
                    {0, 0, 0, 0},
		            {0, 1, 0, 0},
		            {0, 0, 1, 0},
		            {0, 0, 0, 1}
            };
            return res;
        }

        public static float[,] Py()
        {
            float[,] res = {
                    {0, 0, 0, 0},
		            {0, 1, 0, 0},
		            {0, 0, 1, 0},
		            {0, 0, 0, 1}
            };
            return res;
        }

        public static float[,] Pz()
        {
            float[,] res = {
                    {1, 0, 0, 0},
		            {0, 1, 0, 0},
		            {0, 0, 0, 0},
		            {0, 0, 0, 1}
            };
            return res;
        }

        public static float[,] Ax(float c)
        {
            float[,] res = {
                    {1, 0, 0, -1/c},
		            {0, 1, 0, 0},
		            {0, 0, 1, 0},
		            {0, 0, 0, 1}
            };
            return res;
        }

        public static float[,] Ay(float c)
        {
            float[,] res = {
                    {1, 0, 0, 0},
		            {0, 1, 0, -1/c},
		            {0, 0, 1, 0},
		            {0, 0, 0, 1}
            };
            return res;
        }

        public static float[,] Az(float c)
        {
            float[,] res = {
                    {1, 0, 0, 0},
		            {0, 1, 0, 0},
		            {0, 0, 1, -1/c},
		            {0, 0, 0, 1}
            };
            return res;
        }
    }
}
