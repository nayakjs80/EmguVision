using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;

namespace EmguCV
{
    public class EmguVision_DrawTool
    {
        const double dMaxImageScale = 10.0;
        const double dMinImageScale = 1.0;

        public Mat DisImage = null;
        Control CtrlDraw = null;

        int m_nDisStartPosX = 0;
        int m_nDisStartPosY = 0;
        int m_nDisWidth = 0;
        int m_nDisHeight = 0;

        double m_dDrawScale = 1.0;
        double m_dDisScaleX = 1.0;
        double m_dDisScaleY = 1.0;

        bool m_bImageMove = false;

        int m_nPickStartPosX = 0;
        int m_nPickStartPosY = 0;
        int m_nDisTempPosX = 0;
        int m_nDisTempPosY = 0;

        public EmguVision_DrawTool(PictureBox DrawScreen)
        {
            CtrlDraw = DrawScreen;

            m_nDisStartPosX = 0;
            m_nDisStartPosY = 0;
            m_nDisWidth = 0;
            m_nDisHeight = 0;
            
            m_dDrawScale = 1.0;

            m_dDisScaleX = 1.0;
            m_dDisScaleY = 1.0;

            CtrlDraw.MouseDown += CtrlDraw_OnMouseDown;
            CtrlDraw.MouseMove += CtrlDraw_OnMouseMove;
            CtrlDraw.MouseUp += CtrlDraw_OnMouseUp;
            CtrlDraw.MouseWheel += CtrlDraw_OnMouseWheel;
            CtrlDraw.Paint += PaintImage;
        }

        ~EmguVision_DrawTool()
        {
            m_nDisStartPosX = 0;
            m_nDisStartPosY = 0;
            m_nDisWidth = 0;
            m_nDisHeight = 0;
            m_dDrawScale = 1.0;
            m_dDisScaleX = 1.0;
            m_dDisScaleY = 1.0;
        }

        private void CtrlDraw_OnMouseDown(object sender, MouseEventArgs e)
        {
            m_bImageMove = true;

            //m_nDisStartPosX = (int)(e.X / m_dDrawScale);
            //m_nDisStartPosY = (int)(e.Y / m_dDrawScale);

            //m_nDisStartPosX = (int)((e.X * m_dDisScaleX) / m_dDrawScale);
            //m_nDisStartPosY = (int)((e.Y * m_dDisScaleY) / m_dDrawScale);

            m_nPickStartPosX = (int)((e.X * m_dDisScaleX) / m_dDrawScale);
            m_nPickStartPosY = (int)((e.Y * m_dDisScaleY) / m_dDrawScale);
            //m_nPickStartPosX = e.X;
            //m_nPickStartPosY = e.Y;

            m_nDisTempPosX = m_nDisStartPosX;
            m_nDisTempPosY = m_nDisStartPosY;
        }

        private void CtrlDraw_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (true == m_bImageMove)
            {
                int nPickCurPosX = (int)((e.X * m_dDisScaleX) / m_dDrawScale);
                int nPickCurPosY = (int)((e.Y * m_dDisScaleY) / m_dDrawScale);
                //int nPickCurPosX = e.X;
                //int nPickCurPosY = e.Y;

                m_nDisStartPosX = m_nDisTempPosX - (nPickCurPosX - m_nPickStartPosX);
                m_nDisStartPosY = m_nDisTempPosY - (nPickCurPosY - m_nPickStartPosY);

                CtrlDraw.Invalidate();
            }
        }

        private void CtrlDraw_OnMouseUp(object sender, MouseEventArgs e)
        {
            if (true == m_bImageMove)
            {
                m_bImageMove = false;

                int nPickEndPosX = (int)((e.X * m_dDisScaleX) / m_dDrawScale);
                int nPickEndPosY = (int)((e.Y * m_dDisScaleY) / m_dDrawScale);

                m_nDisStartPosX = m_nDisTempPosX - (nPickEndPosX - m_nPickStartPosX);
                m_nDisStartPosY = m_nDisTempPosY - (nPickEndPosY - m_nPickStartPosY);

                m_nDisTempPosX = 0;
                m_nDisTempPosY = 0;

            }

            CtrlDraw.Invalidate();
        }

        private void CtrlDraw_OnMouseWheel(object sender, MouseEventArgs e)
        {
            //m_nCtrlBasePosX = e.X;
            //m_nCtrlBasePosY = e.Y;

            //dSelectPosX = m_nDisStartPosX + (e.X * m_dDisScaleX);
            //dSelectPosY = m_nDisStartPosY + (e.Y * m_dDisScaleY);

            //m_nDisStartPosX = e.X;
            //m_nDisStartPosY = e.Y;

            int nPickPosX = (int)((e.X * m_dDisScaleX) / m_dDrawScale);
            int nPickPosY = (int)((e.Y * m_dDisScaleY) / m_dDrawScale);


            double dMoveImageDisX = (m_nDisWidth * 0.1) / 2;
            double dMoveImageDisY = (m_nDisHeight * 0.1) / 2;

            if (e.Delta > 0)
            {
                // 휠을 위로 스크롤
                m_dDrawScale = Math.Min(dMaxImageScale, m_dDrawScale + 0.1); // 최대 스케일 제한
                //m_dDisScaleY = Math.Min(5.0, m_dDisScaleY + 0.1); // 최대 스케일 제한
                //m_dDrawScale += 0.1;

                //m_nDisStartPosX += (int)dMoveImageDisX;
                //m_nDisStartPosY += (int)dMoveImageDisY;
            }
            else
            {
                // 휠을 아래로 스크롤
                //m_dDrawScale = Math.Max(1.0, m_dDrawScale - 0.1); // 최소 스케일 제한
                m_dDrawScale = Math.Max(dMinImageScale, m_dDrawScale - 0.1); // 최소 스케일 제한
                                                                             //m_dDisScaleY = Math.Max(1.0, m_dDisScaleY - 0.1); // 최소 스케일 제한

                //m_nDisStartPosX -= (int)dMoveImageDisX;
                //m_nDisStartPosY -= (int)dMoveImageDisY;
            }

            int nPickChangePosX = (int)((e.X * m_dDisScaleX) / m_dDrawScale);
            int nPickChangePosY = (int)((e.Y * m_dDisScaleY) / m_dDrawScale);


            m_nDisStartPosX += (nPickPosX - nPickChangePosX);
            m_nDisStartPosY += (nPickPosY - nPickChangePosY);

            //m_nDisStartPosX = (int)(m_nDisCtrlWidth * (m_dDisScaleX - 1));
            //m_nDisStartPosY = (int)(m_nDisCtrlHeight * (m_dDisScaleY - 1));

            // 이미지 다시 그리기
            CtrlDraw.Invalidate();
        }

        Rectangle GetDisRect()
        {
            Rectangle rectRet = new Rectangle(m_nDisStartPosX, m_nDisStartPosY, m_nDisWidth, m_nDisHeight);

            //double dMoveImageDisX = ((m_nDisWidth - (m_nDisWidth / m_dDrawScale)) / m_dDisScaleX) / 2;
            //double dMoveImageDisY = ((m_nDisHeight - (m_nDisHeight / m_dDrawScale)) / m_dDisScaleY) / 2;

            double dMoveImageDisX = ((m_nDisWidth - (m_nDisWidth / m_dDisScaleX)) / m_dDrawScale) / 2;
            double dMoveImageDisY = ((m_nDisHeight - (m_nDisHeight / m_dDisScaleY)) / m_dDrawScale) / 2;

            rectRet.X = m_nDisStartPosX;
            rectRet.Y = m_nDisStartPosY;

            //rectRet.X = m_nDisStartPosX - (int)dMoveImageDisX;
            //rectRet.Y = m_nDisStartPosY - (int)dMoveImageDisY;

            rectRet.Width = (int)((m_nDisWidth / m_dDrawScale));
            rectRet.Height = (int)((m_nDisHeight / m_dDrawScale));

            return rectRet;
        }



        //public void PaintImage(Mat DisImage, PaintEventArgs e)
        public void PaintImage(object sender, PaintEventArgs e)
        {
            if (null != DisImage)
            {
                m_nDisWidth = DisImage.Width;
                m_nDisHeight = DisImage.Height;

                Bitmap TempImage = new Bitmap(m_nDisWidth, m_nDisHeight, DisImage.Step, PixelFormat.Format8bppIndexed, DisImage.DataPointer);
                ColorPalette palette = TempImage.Palette;

                // Grayscale 팔레트를 설정합니다.
                for (int i = 0; i < 256; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                }

                TempImage.Palette = palette;

                m_dDisScaleX = m_nDisWidth / (double)CtrlDraw.Width;
                m_dDisScaleY = m_nDisHeight / (double)CtrlDraw.Height;

                //Rectangle Temp = new Rectangle(0, 0, nImageWidth, nImageHeight);
                //Rectangle rectSrc = new Rectangle(m_nCtrlBasePosX, m_nCtrlBasePosY, nImageWidth, nImageHeight);
                //Rectangle rectSrc = new Rectangle(m_nDisStartPosX, m_nDisStartPosY, m_nDisWidth, m_nDisHeight);
                Rectangle rectSrc = GetDisRect();
                Rectangle rectDest = new Rectangle(0, 0, CtrlDraw.Width, CtrlDraw.Height);
                //e.Graphics.DrawImage(TempImage, Temp);
                e.Graphics.DrawImage(TempImage, rectDest, rectSrc, GraphicsUnit.Pixel);


                //e.Graphics.DrawString()
            }
        }

    }
}
