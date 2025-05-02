using Emgu.CV.CvEnum;
using Emgu.CV;
using EmguCV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Reg;
using Emgu.CV.Structure;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing.Drawing2D;

namespace LaserWaterJet.Vision_EmguCV
{
    public class EmguCVSearchEdgeCircle : EmguVisionFunc
    {
        int nExecuteIndex = 0;

        public EmguCVSearchEdgeCircle()
        {
            // 중심점 설정
            EmguVisionParamter paramCenterX = new EmguVisionParamter
            {
                sName = "CenterX",
                sValue = "0" // 기본값
            };
            listParameters.Add(paramCenterX);

            EmguVisionParamter paramCenterY = new EmguVisionParamter
            {
                sName = "CenterY",
                sValue = "0" // 기본값
            };
            listParameters.Add(paramCenterY);

            // 시작 각도와 종료 각도
            EmguVisionParamter paramStartAngle = new EmguVisionParamter
            {
                sName = "StartAngle",
                sValue = "0" // 기본값
            };
            listParameters.Add(paramStartAngle);

            EmguVisionParamter paramEndAngle = new EmguVisionParamter
            {
                sName = "EndAngle",
                sValue = "180" // 기본값
            };
            listParameters.Add(paramEndAngle);

            // 선 개수
            EmguVisionParamter paramLineCount = new EmguVisionParamter
            {
                sName = "LineCount",
                sValue = "36" // 기본값
            };
            listParameters.Add(paramLineCount);

            // Threshold 값
            EmguVisionParamter paramThreshold = new EmguVisionParamter
            {
                sName = "Threshold",
                sValue = "30" // 기본값
            };
            listParameters.Add(paramThreshold);
        }

        override public string GetName()
        {
            return nameof(EmguCVSearchEdgeCircle);
        }

        override public bool Execute(Mat SourceImg)
        {
            EmguVision_Result Ret = new EmguVision_Result(nameof(EmguCVSearchEdgeCircle));

            m_SourceImage = SourceImg.Clone();
            Mat outputImage = SourceImg.Clone();

            // 파라미터 값 가져오기
            if (!int.TryParse(listParameters[0].sValue, out int centerX)) centerX = 0;
            if (!int.TryParse(listParameters[1].sValue, out int centerY)) centerY = 0;
            if (!int.TryParse(listParameters[2].sValue, out int startAngle)) startAngle = 0;
            if (!int.TryParse(listParameters[3].sValue, out int endAngle)) endAngle = 360;
            if (!int.TryParse(listParameters[4].sValue, out int lineCount)) lineCount = 36;
            if (!int.TryParse(listParameters[5].sValue, out int threshold)) threshold = 128;

            // 중심점 설정
            Point center = new Point(centerX, centerY);

            // 각도 간격 계산
            double angleStep = (endAngle - startAngle) / (double)lineCount;

            // 결과 점들을 저장할 리스트
            List<Point> edgePoints = new List<Point>();

            //// 입력 이미지를 Grayscale로 변환
            //Mat grayImage = new Mat();
            //CvInvoke.CvtColor(SourceImg, grayImage, ColorConversion.Bgr2Gray);

            // 각 선에 대해 경계선 점 탐지
            for (int i = 0; i < lineCount; i++)
            {
                double angle = startAngle + i * angleStep;
                double radian = angle * Math.PI / 180.0;

                // 선의 방향 벡터 계산
                double dx = Math.Cos(radian);
                double dy = Math.Sin(radian);

                Point ptEnd = new Point(0, 0);

                // 선을 따라가며 경계선 점 탐지
                for (int r = 0; r < Math.Max(outputImage.Width, outputImage.Height); r++)
                {
                    int x = (int)(center.X + r * dx);
                    int y = (int)(center.Y + r * dy);

                    // 이미지 범위를 벗어나면 중단
                    if (x < 0 || y < 0 || x >= outputImage.Width || y >= outputImage.Height)
                    {
                        ptEnd.X = x;
                        ptEnd.Y = y;
                        break;
                    }

                    // 픽셀 값 가져오기
                    //byte pixelValue = SourceImg.GetData(y, x)[0];
                    //byte pixelValue = SourceImg.Get<byte>(y, x);
                    byte pixelValue = 0x00;
                    unsafe
                    {
                        byte* data = (byte*)outputImage.DataPointer;
                        int stride = outputImage.Width; // 한 행의 바이트 수
                        pixelValue = data[y * stride + x]; // (x, y) 위치의 픽셀 값
                    }

                    // Threshold를 초과하면 경계선으로 간주
                    if (pixelValue < threshold)
                    {
                        edgePoints.Add(new Point(x, y));

                        ptEnd.X = x;
                        ptEnd.Y = y;
                        break;
                    }
                }

                // 선 그리기
                CvInvoke.Line(outputImage, center, ptEnd, new MCvScalar(255, 0, 0), 2); // 녹색 선, 두께 2
            }

            Result.m_ProcessingImage = outputImage.Clone();
            Result.m_bIsOK = true;

            string sFolderPath = $"D:\\ResultImage\\CVSearchEdgeCircle\\_{DateTime.Now.ToString("yyyyMMddHHmm")}";
            if (false == Directory.Exists(sFolderPath))
            {
                Directory.CreateDirectory(sFolderPath);
            }

            string sOutputFilePath = $"{sFolderPath}\\SearchEdge_{nExecuteIndex}.bmp";
            CvInvoke.Imwrite(sOutputFilePath, outputImage);
            nExecuteIndex++;


            // 결과를 배열로 저장
            Ret.sResult_Json = Newtonsoft.Json.JsonConvert.SerializeObject(edgePoints, Newtonsoft.Json.Formatting.Indented);
            Ret.m_bIsOK = true;

            return true;
        }
    }
}
