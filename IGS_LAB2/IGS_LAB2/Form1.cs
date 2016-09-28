using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IGS_LAB2
{
    public partial class Form1 : Form
    {
        private List<Triangle> triangles;

        private float[,] z;//z-буфер

        public Form1()
        {
            InitializeComponent();
            //trToNum();
        }

        private void ConvertPolygonsTo2d()
        {
            float[,] matrix = MatrixOfOrtog(GetHi(), GetGam());
            Triangle.SetCenter(GetO());

            foreach (Triangle polygon in triangles)
            {
                polygon.Projection(matrix);
            }
        }

        private void DrawPolygons(System.Windows.Forms.PaintEventArgs e)
        {
            Point_3d c = GetC();

            Pen pen = new Pen(button8.BackColor);

            Color outsideColor = GetOutsideColor();
            Color insideColor = GetInsideColor();

            PointF o = GetO();

            float[] newLightCoordinates = MatrixConversion.MultMatrixOnVector(MatrixOfOrtog(GetHi(), GetGam()), GetLightCoordinates().GetXyz());
            Point_3d newLightPoint = new Point_3d(o.X + newLightCoordinates[0], o.Y + newLightCoordinates[1], newLightCoordinates[2]);
            LightParameters lp = new LightParameters(GetKa(), GetKd(), GetKs(), GetN(), newLightPoint, GetDiffusedColor(), GetDottedColor());

            float[] newSpectatorCoordinates = MatrixConversion.MultMatrixOnVector(MatrixOfOrtog(GetHi(), GetGam()), c.GetXyz());
            Point_3d newSpectatorPoint = new Point_3d(o.X + newSpectatorCoordinates[0], o.Y + newSpectatorCoordinates[1], newSpectatorCoordinates[2]);

            Point_3d vectorBetweenSpectatorAndCenter = new Point_3d(newSpectatorPoint.X - o.X, newSpectatorPoint.Y - o.Y, newSpectatorPoint.Z);

            if (checkBox1.Checked)
            {
                DrawFlatShading(e, vectorBetweenSpectatorAndCenter, outsideColor, insideColor, lp, newSpectatorPoint);
            }

            else if (checkBox2.Checked)
            {
                DrawFrame(e, pen);
            }

            else if (checkBox4.Checked)
            {
                DrawFlatShadingWithZbuffer(e, pen, vectorBetweenSpectatorAndCenter, outsideColor, insideColor, z, lp, newSpectatorPoint);
                ClearZbuffer();
            }

            else if (checkBox7.Checked)
            {
                DrawGouraudShading(e, pen, vectorBetweenSpectatorAndCenter, outsideColor, insideColor, z, lp, newSpectatorPoint); 
                ClearZbuffer();
            }
            else if (checkBox8.Checked)
            {
                DrawPhongShading(e, pen, vectorBetweenSpectatorAndCenter, outsideColor, insideColor, z, lp, newSpectatorPoint);
                ClearZbuffer();
            }

            pen.Color = Color.Black;
            e.Graphics.DrawEllipse(pen, newLightPoint.X - 10, newLightPoint.Y - 10, 20, 20);
        }

        private float[,] MatrixOfOrtog(float hi, float gam)
        {
            float[,] first = MatrixConversion.MultOfMatrix(MatrixConversion.Rz(hi), MatrixConversion.Rx(gam));
            float[,] second = MatrixConversion.MultOfMatrix(first, MatrixConversion.Mx());
            return second;
        }

        private void ClearZbuffer()
        {
            z = new float[pictureBox1.Height*2, pictureBox1.Width*2];
            for (int i = 0; i < pictureBox1.Height*2; i++)
                for (int j = 0; j < pictureBox1.Width*2; j++)
                    z[i, j] = float.MinValue;
        }

        private void countPolygons()
        {
            Triangle.ClearPoints();

            float u_min = trackBar7.Value;
            float v_min = (float)(trackBar4.Value * Math.PI / 180);

            float u_max = trackBar2.Value;
            float v_max = (float)(trackBar5.Value * Math.PI / 180);


            float du = (u_max - u_min) / trackBar3.Value;
            float dv = (v_max - v_min) / trackBar6.Value;

            if (du <= 0 || dv <= 0)
                return;

            float param1 = trackBar11.Value;
            float param2 = trackBar12.Value;

            this.triangles = Function.CountPolygons(u_min, u_max, v_min, v_max, du, dv, param1, param2);
        }

        #region shading
        private void DrawFrame(System.Windows.Forms.PaintEventArgs e, Pen pen)
        {
            foreach (Triangle polygon in triangles)
            {
                polygon.DrawFrame(e, pen);
            }
        }

        private void DrawFlatShading(System.Windows.Forms.PaintEventArgs e,
                                     Point_3d vectorBetweenSpectatorAndCenter,
                                     Color outsideColor,
                                     Color insideColor,
                                     LightParameters lp,
                                     Point_3d cPoint)
        {
            SolidBrush brush = new SolidBrush(Color.Black);
            foreach (Triangle polygon in triangles)
            {
                polygon.DrawFlatShading(e, brush, vectorBetweenSpectatorAndCenter, outsideColor, insideColor, lp, cPoint);
            }
        }

        private void DrawFlatShadingWithZbuffer(System.Windows.Forms.PaintEventArgs e,
                                               Pen pen,
                                               Point_3d vectorBetweenSpectatorAndCenter,
                                               Color outColor,
                                               Color inColor,
                                               float[,] z,
                                               LightParameters lp,
                                               Point_3d newSpectatorPoint)
        {
            foreach (Triangle polygon in triangles)
            {
                polygon.DrawFlatShadingWithZbuffer(e, pen, vectorBetweenSpectatorAndCenter, outColor, inColor, z, lp, newSpectatorPoint);
            }
        }

        private void DrawGouraudShading(System.Windows.Forms.PaintEventArgs e,
                                        Pen pen,
                                        Point_3d vectorBetweenSpectatorAndCenter,
                                        Color outColor,
                                        Color inColor,
                                        float[,] z,
                                        LightParameters lp,
                                        Point_3d newSpectatorPoint)
        {
            foreach (Triangle polygon in triangles)
            {
                polygon.AddNormalsToVertices();
            }

            Triangle.ComputeNormalsInVertices();

            foreach (Triangle polygon in triangles)
            {
                polygon.DrawGouraudShading(e, pen, vectorBetweenSpectatorAndCenter, outColor, inColor, z, lp, newSpectatorPoint);
            }
        }

        private void DrawPhongShading(System.Windows.Forms.PaintEventArgs e,
                                      Pen pen,
                                      Point_3d vectorBetweenSpectatorAndCenter,
                                      Color outColor,
                                      Color inColor,
                                      float[,] z,
                                      LightParameters lp,
                                      Point_3d newSpectatorPoint)
        {
            foreach (Triangle polygon in triangles)
            {
                polygon.AddNormalsToVertices();
            }

            Triangle.ComputeNormalsInVertices();

            foreach (Triangle polygon in triangles)
            {
                polygon.DrawPhongShading(e, pen, vectorBetweenSpectatorAndCenter, outColor, inColor, z, lp, newSpectatorPoint);
            }
        }
        #endregion

        #region get
        private Point_3d GetC()
        {
            float x = trackBar10.Value;
            float y = trackBar8.Value;
            float z = trackBar9.Value;

            return new Point_3d(x, y, z);
        }

        private PointF GetO()
        {
            return new PointF(pictureBox1.Width / 2, pictureBox1.Height / 2);
        }

        private float GetKa()
        {
            return trackBar18.Value / 100F;
        }

        private float GetKd()
        {
            return trackBar17.Value / 100F;
        }

        private float GetKs()
        {
            return trackBar16.Value / 100F;
        }

        private int GetN()
        {
            return trackBar1.Value;
        }

        private Point_3d GetLightCoordinates()
        {
            return new Point_3d(trackBar13.Value, trackBar14.Value, trackBar15.Value);
        }

        private Color GetOutsideColor()
        {
            return button3.BackColor;
        }

        private Color GetInsideColor()
        {
            return button4.BackColor;
        }

        private Color GetDottedColor()
        {
            return button1.BackColor;
        }

        private Color GetDiffusedColor()
        {
            return button1.BackColor;
        }

        private float GetHi()
        {
            Point_3d c = GetC();
            float hi = (float)(Math.Acos(c.Y / Math.Pow((c.X * c.X + c.Y * c.Y), 0.5)));
            return hi;
        }

        private float GetGam()
        {
            Point_3d c = GetC();
            float gam = (float)(Math.Acos(c.Z / Math.Pow((c.X * c.X + c.Z * c.Z + c.Y * c.Y), 0.5)));
            return gam;
        }
        #endregion

        #region colors
        private void button3_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog1 = new ColorDialog();
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                button3.BackColor = colorDialog1.Color;
            }
            pictureBox1.Invalidate();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog1 = new ColorDialog();
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                button4.BackColor = colorDialog1.Color;
            }
            pictureBox1.Invalidate();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog1 = new ColorDialog();
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                button8.BackColor = colorDialog1.Color;
            }
            pictureBox1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog1 = new ColorDialog();
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                button1.BackColor = colorDialog1.Color;
            }
            pictureBox1.Invalidate();
        }
        #endregion

        #region checkboxes
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                checkBox4.Checked = false;
                checkBox1.Checked = false;
                checkBox7.Checked = false;
                checkBox8.Checked = false;
                pictureBox1.Invalidate();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                checkBox4.Checked = false;
                checkBox2.Checked = false;
                checkBox7.Checked = false;
                checkBox8.Checked = false;
                pictureBox1.Invalidate();
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked == true)
            {
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox7.Checked = false;
                checkBox8.Checked = false;
                pictureBox1.Invalidate();
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked == true)
            {
                checkBox4.Checked = false;
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox8.Checked = false;
                pictureBox1.Invalidate();
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked == true)
            {
                checkBox4.Checked = false;
                checkBox1.Checked = false;
                checkBox7.Checked = false;
                checkBox2.Checked = false;
                pictureBox1.Invalidate();
            }
        }

        #endregion

        private void equalizeNumToTr()
        {
            numericUpDown12.Value = trackBar13.Value;
            numericUpDown13.Value = trackBar14.Value;
            numericUpDown14.Value = trackBar15.Value;

            numericUpDown17.Value = trackBar18.Value;
            numericUpDown16.Value = trackBar17.Value;
            numericUpDown15.Value = trackBar16.Value;

            numericUpDown18.Value = trackBar1.Value;

            numericUpDown7.Value = trackBar10.Value;
            numericUpDown8.Value = trackBar8.Value;
            numericUpDown9.Value = trackBar9.Value;

            numericUpDown1.Value = trackBar7.Value;
            numericUpDown2.Value = trackBar2.Value;
            numericUpDown3.Value = trackBar3.Value;

            numericUpDown4.Value = trackBar4.Value;
            numericUpDown5.Value = trackBar5.Value;
            numericUpDown6.Value = trackBar6.Value;

            numericUpDown10.Value = trackBar11.Value;
            numericUpDown11.Value = trackBar12.Value;
        }

        private void trToNum()
        {
            trackBar13.Value = Convert.ToInt32(numericUpDown12.Value);
            trackBar14.Value = Convert.ToInt32(numericUpDown13.Value);
            trackBar15.Value = Convert.ToInt32(numericUpDown14.Value);

            trackBar18.Value = Convert.ToInt32(numericUpDown17.Value);
            trackBar17.Value = Convert.ToInt32(numericUpDown16.Value);
            trackBar16.Value = Convert.ToInt32(numericUpDown15.Value);

            trackBar1.Value = Convert.ToInt32(numericUpDown18.Value);

            trackBar10.Value = Convert.ToInt32(numericUpDown7.Value);
            trackBar8.Value = Convert.ToInt32(numericUpDown8.Value);
            trackBar9.Value = Convert.ToInt32(numericUpDown9.Value);

            trackBar7.Value = Convert.ToInt32(numericUpDown1.Value);
            trackBar2.Value = Convert.ToInt32(numericUpDown2.Value);
            trackBar3.Value = Convert.ToInt32(numericUpDown3.Value);

            trackBar4.Value = Convert.ToInt32(numericUpDown4.Value);
            trackBar5.Value = Convert.ToInt32(numericUpDown5.Value);
            trackBar6.Value = Convert.ToInt32(numericUpDown6.Value);

            trackBar11.Value = Convert.ToInt32(numericUpDown10.Value);
            trackBar12.Value = Convert.ToInt32(numericUpDown11.Value);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            countPolygons();
            Triangle.ClearNormals();
            ConvertPolygonsTo2d();
            ClearZbuffer();
            DrawPolygons(e);
        }

        #region numerics
        private void numericUpDown12_ValueChanged(object sender, EventArgs e)
        {
            trackBar13.Value = Convert.ToInt32(numericUpDown12.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown13_ValueChanged(object sender, EventArgs e)
        {
            trackBar14.Value = Convert.ToInt32(numericUpDown13.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown14_ValueChanged(object sender, EventArgs e)
        {
            trackBar15.Value = Convert.ToInt32(numericUpDown14.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown17_ValueChanged(object sender, EventArgs e)
        {
            trackBar18.Value = Convert.ToInt32(numericUpDown17.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown16_ValueChanged(object sender, EventArgs e)
        {
            trackBar17.Value = Convert.ToInt32(numericUpDown16.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown15_ValueChanged(object sender, EventArgs e)
        {
            trackBar16.Value = Convert.ToInt32(numericUpDown15.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown18_ValueChanged(object sender, EventArgs e)
        {
            trackBar1.Value = Convert.ToInt32(numericUpDown18.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            trackBar10.Value = Convert.ToInt32(numericUpDown7.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            trackBar8.Value = Convert.ToInt32(numericUpDown8.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            trackBar9.Value = Convert.ToInt32(numericUpDown9.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            trackBar7.Value = Convert.ToInt32(numericUpDown1.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            trackBar2.Value = Convert.ToInt32(numericUpDown2.Value);
            pictureBox1.Invalidate();

        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            trackBar3.Value = Convert.ToInt32(numericUpDown3.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            trackBar4.Value = Convert.ToInt32(numericUpDown4.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            trackBar5.Value = Convert.ToInt32(numericUpDown5.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            trackBar6.Value = Convert.ToInt32(numericUpDown6.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            trackBar11.Value = Convert.ToInt32(numericUpDown10.Value);
            pictureBox1.Invalidate();
        }

        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            trackBar12.Value = Convert.ToInt32(numericUpDown11.Value);
            pictureBox1.Invalidate();
        }
#endregion

        #region scrolls
        private void trackBar13_Scroll(object sender, EventArgs e)
        {
            numericUpDown12.Value = trackBar13.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar14_Scroll(object sender, EventArgs e)
        {
            numericUpDown13.Value = trackBar14.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar15_Scroll(object sender, EventArgs e)
        {
            numericUpDown14.Value = trackBar15.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar18_Scroll(object sender, EventArgs e)
        {
            numericUpDown17.Value = trackBar18.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar17_Scroll(object sender, EventArgs e)
        {
            numericUpDown16.Value = trackBar17.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar16_Scroll(object sender, EventArgs e)
        {
            numericUpDown15.Value = trackBar16.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            numericUpDown18.Value = trackBar1.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar10_Scroll(object sender, EventArgs e)
        {
            numericUpDown7.Value = trackBar10.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar8_Scroll(object sender, EventArgs e)
        {
            numericUpDown8.Value = trackBar8.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar9_Scroll(object sender, EventArgs e)
        {
            numericUpDown9.Value = trackBar9.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar7_Scroll(object sender, EventArgs e)
        {
            numericUpDown1.Value = trackBar7.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            numericUpDown2.Value = trackBar2.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            numericUpDown3.Value = trackBar3.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            numericUpDown4.Value = trackBar4.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            numericUpDown5.Value = trackBar5.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            numericUpDown6.Value = trackBar6.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar11_Scroll(object sender, EventArgs e)
        {
            numericUpDown10.Value = trackBar11.Value;
            pictureBox1.Invalidate();
        }

        private void trackBar12_Scroll(object sender, EventArgs e)
        {
            numericUpDown11.Value = trackBar12.Value;
            pictureBox1.Invalidate();
        }
        #endregion
    }
}
