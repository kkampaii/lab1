using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace IGS_LAB2
{

    class LightParameters
    {
        public float ka;
        public float kd;
        public float ks;
        public int n;
        public Point_3d coordinates;

        public Color diffusedColor;
        public Color dottedColor;

        public float Ia = 0;
        public float Id = 0;
        public float Is = 0;

        public LightParameters(float ka, float kd, float ks, int n, Point_3d coordinates, Color diffusedColor, Color dottedColor)
        {
            this.ka = ka;
            this.kd = kd;
            this.ks = ks;
            this.n = n;
            this.coordinates = coordinates;
            this.diffusedColor = diffusedColor;
            this.dottedColor = dottedColor;
        }


    }
}
