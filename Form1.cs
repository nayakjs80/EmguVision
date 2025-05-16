using Emgu.CV.Structure;
using Emgu.CV;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

using Newtonsoft.Json;
using static Emgu.CV.Dai.OpenVino;
using LaserWaterJet.Vision_EmguCV;

namespace EmguCV
{
    public partial class Form1 : Form
    {
        const int nMaxProcessing = 10;

        List<EmguVisionFunc> FuncProcessing_all = new List<EmguVisionFunc>();

        EmguVision_DrawTool DrawTool;

        public Form1()
        {
            InitializeComponent();

            ProcessingList.Rows.Add(nMaxProcessing);

            DrawTool = new EmguVision_DrawTool(pb_DrawImage);

            tmUpdate.Interval = 500;
            tmUpdate.Start();
        }

        private void DrawImage(Mat SourceImg, PictureBox pbBox)
        {
            Bitmap TempImage = new Bitmap(SourceImg.Width, SourceImg.Height, SourceImg.Step, PixelFormat.Format8bppIndexed, SourceImg.DataPointer);

            ColorPalette palette = TempImage.Palette;
            if (1 == SourceImg.NumberOfChannels)
            {
                TempImage = new Bitmap(SourceImg.Width, SourceImg.Height, SourceImg.Step, PixelFormat.Format8bppIndexed, SourceImg.DataPointer);
                // Grayscale 팔레트를 설정합니다.
                for (int i = 0; i < 256; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                }

                TempImage.Palette = palette;
            }
            else if (3 == SourceImg.NumberOfChannels)
            {
                TempImage = new Bitmap(SourceImg.Width, SourceImg.Height, SourceImg.Step, PixelFormat.Format24bppRgb, SourceImg.DataPointer);
                //// 3채널 컬러 팔레트를 정확히 설정하는 예제
                //for (int i = 0; i < 256; i++)
                //{
                //    // R, G, B 값을 나눠 명확히 구별
                //    int red = i;       // 0~255
                //    int green = (i * 2) % 256; // 예: 녹색 채널은 0~255 반복
                //    int blue = (i * 3) % 256;  // 예: 파란색 채널은 다른 패턴
                //    palette.Entries[i] = Color.FromArgb(red, green, blue);
                //}
                //TempImage.Palette = palette;

            }

            pbBox.Image = TempImage;
        }

        private void WriteTextOnImage(Mat image, string text, Point position, double fontScale, Bgr color, int thickness)
        {
            // 폰트 설정
            FontFace fontFace = FontFace.HersheySimplex;

            // 텍스트 쓰기
            CvInvoke.PutText(
                image,          // 이미지
                text,           // 텍스트
                position,       // 텍스트 위치
                fontFace,       // 폰트 종류
                fontScale,      // 폰트 크기
                color.MCvScalar, // 텍스트 색상
                thickness       // 텍스트 두께
            );
        }

        private void LoadImage(string filePath)
        {
            //Result_Processing Ret = new Result_Processing("LoadImage");
            //EmguVision_Result Ret = new EmguVision_Result();
            //EmguVisionAction action = new EmguVisionAction();

            EmguVisionFunc LoadImage = new EmguVisionFunc();
            //if (null != m_selImage) return;

            try
            {
                LoadImage.Execute(CvInvoke.Imread(filePath, Emgu.CV.CvEnum.ImreadModes.Grayscale));

                FuncProcessing_all.Add(LoadImage);

                DrawTool.DisImage = FuncProcessing_all[nSelIndex].Result.m_ProcessingImage;

                pb_DrawImage.Invalidate();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading image: " + ex.Message);
            }
        }

        double GetFocusValue_useLaplacian(Mat image)
        {
            // 라플라시안 변환 적용
            Mat laplacian = new Mat();
            CvInvoke.Laplacian(image, laplacian, DepthType.Cv64F);

            //// 라플라시안 변환 결과의 분산 계산
            //Mat mean = new Mat();
            //Mat stddev = new Mat();
            //CvInvoke.MeanStdDev(laplacian, mean, stddev);

            //// 분산 값을 반환 (선명도 값)
            //Array stddevData = stddev.GetData();

            //return (double)stddevData.GetValue(0, 0);

            // 절대값 처리 후 평균 계산
            Mat absLaplacianImage = new Mat();
            CvInvoke.ConvertScaleAbs(laplacian, absLaplacianImage, 1, 0);
            double sharpness = CvInvoke.Mean(absLaplacianImage).V0;

            return sharpness;
        }

        double GetFocusValue_useSobel(Mat image)
        {
            // Sobel 변환 적용
            Mat sobelX = new Mat();
            Mat sobelY = new Mat();
            CvInvoke.Sobel(image, sobelX, DepthType.Cv64F, 1, 0);
            CvInvoke.Sobel(image, sobelY, DepthType.Cv64F, 0, 1);

            // Sobel 변환 결과의 분산 계산
            Mat meanX = new Mat();
            Mat stddevX = new Mat();
            CvInvoke.MeanStdDev(sobelX, meanX, stddevX);

            Mat meanY = new Mat();
            Mat stddevY = new Mat();
            CvInvoke.MeanStdDev(sobelY, meanY, stddevY);

            // 분산 값을 반환 (선명도 값)
            Array stddevXData = stddevX.GetData();
            Array stddevYData = stddevY.GetData();

            return (double)stddevXData.GetValue(0, 0) + (double)stddevYData.GetValue(0, 0);
        }

        private void btn_ImageLoad_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Title = "Select an Image File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    //m_selImage = null;
                    //m_ProcessingImage = null;

                    LoadImage(filePath);
                }
            }
        }

        // Sample Edge Detection .. 
        public double CalculateSharpnessUsingCanny(Mat image)
        {
            Mat edges = new Mat();
            CvInvoke.Canny(image, edges, 100, 200);

            return CvInvoke.CountNonZero(edges);
        }


        private void btn_Processing_Click(object sender, EventArgs e)
        {
            if (0 == FuncProcessing_all.Count) return;

            EmguVisionAction action = new EmguVisionAction();

            int nImageIndex = FuncProcessing_all.Count - 1;
            if (sender == btn_Binary)
            {
                EmguVisionBinary Binary = new EmguVisionBinary();
                Binary.Execute(FuncProcessing_all[nImageIndex].Result.m_ProcessingImage);

                FuncProcessing_all.Add(Binary);
            }
            else if (sender == btn_Dilation)
            {
                EmguVisionDilation Dilation = new EmguVisionDilation();
                Dilation.Execute(FuncProcessing_all[nImageIndex].Result.m_ProcessingImage);

                FuncProcessing_all.Add(Dilation);
            }
            else if (sender == btn_Erosion)
            {
                EmguVisionErosion Erosion = new EmguVisionErosion();
                Erosion.Execute(FuncProcessing_all[nImageIndex].Result.m_ProcessingImage);

                FuncProcessing_all.Add(Erosion);
            }
            else if (sender == btn_SimpleBlob)
            {
                EmguVisionSimpleBlob simpleBlob = new EmguVisionSimpleBlob();
                simpleBlob.Execute(FuncProcessing_all[nImageIndex].Result.m_ProcessingImage);

                FuncProcessing_all.Add(simpleBlob);
            }
            else if (sender == btn_DetectBlob)
            {
                EmguVisionContoursBlob DetectBlob = new EmguVisionContoursBlob();
                DetectBlob.Execute(FuncProcessing_all[nImageIndex].Result.m_ProcessingImage);

                FuncProcessing_all.Add(DetectBlob);
            }
            else if (sender == btn_DetectCircles)
            {
                EmguVisionDetectCircle detectCircle = new EmguVisionDetectCircle();
                detectCircle.Execute(FuncProcessing_all[nImageIndex].Result.m_ProcessingImage);

                FuncProcessing_all.Add(detectCircle);
            }
            else if (sender == btn_SearchEdgeCircle)
            {
                EmguCVSearchEdgeCircle searchEdgeCircle = new EmguCVSearchEdgeCircle();
                //searchEdgeCircle.

                searchEdgeCircle.Execute(FuncProcessing_all[nImageIndex].Result.m_ProcessingImage);

                FuncProcessing_all.Add(searchEdgeCircle);
            }
        }


        private void btn_ShowLoadImage_Click(object sender, EventArgs e)
        {
            //if (null != m_selImage)
            //{
            //    DrawImage(m_selImage, pb_DrawImage);
            //}
        }

        private void btn_ShowProcessingImage_Click(object sender, EventArgs e)
        {
            //if (null != m_ProcessingImage)
            //{
            //    DrawImage(m_ProcessingImage, pb_DrawImage);
            //}
        }

        private void btn_Laplacian_Click(object sender, EventArgs e)
        {
            //if (null != m_selImage)
            {
                //double dFocusValue = GetFocusValue_useLaplacian(m_selImage);
                double dFocusValue = GetFocusValue_useSobel(FuncProcessing_all[0].m_SourceImage);
                lbl_Laplacian.Text = dFocusValue.ToString();

                //Mat OutputImage = new Mat();

                //m_selImage.Clone().CopyTo(OutputImage);

                //string sWritetext = $"FocusValue:{dFocusValue}";
                //WriteTextOnImage(OutputImage, sWritetext, new Point(50, 50), 1.0, new Bgr(Color.White), 1);

                //string scroppedFilePath = $"D:\\ResultImage\\Laplacian_{DateTime.Now.ToString("yyyyMMddHHmmss")}.bmp";
                //CvInvoke.Imwrite(scroppedFilePath, OutputImage);
            }
        }

        int nSelIndex = 0;

        private void ProcessingList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            nSelIndex = e.RowIndex;
            int nSelIndex_Col = e.ColumnIndex;

            if (nSelIndex >= FuncProcessing_all.Count) return;

            //DrawImage(FuncProcessing_all[nSelIndex].Result.m_ProcessingImage, pb_DrawImage);

            if (1 == nSelIndex_Col)
            {
                FuncProcessing_all[nSelIndex].Execute(FuncProcessing_all[(nSelIndex - 1)].Result.m_ProcessingImage);

                #region ori Code .. 

                ////int nImageIndex = ResultProcessing_all.Count - 1;
                //if (nameof(EmguVisionBinary) == ResultProcessing_all[nSelIndex].sProcessingName)
                //{
                //    //int nParamCount = 0;
                //    //EmguVisionBinary Tempvision = new EmguVisionBinary();

                //    //string[] sParamList = Tempvision.GetParamLists(out nParamCount);
                //    //if (nParamCount > 0)
                //    //{
                //    //    Tempvision.SetParam(sParamList[0], "200");
                //    //}


                //    list_EmguVisionAction[nSelIndex].ResultProcessing = ((EmguVisionBinary)(list_EmguVisionAction[nSelIndex].FuncProcessing)).Execute(list_EmguVisionAction[(nSelIndex - 1)].ResultProcessing.m_ProcessingImage);

                //    //ResultProcessing_all[nSelIndex] = Tempvision.Execute(ResultProcessing_all[(nSelIndex - 1)].m_ProcessingImage);

                //    //ResultProcessing_all[nSelIndex] = Binary(ResultProcessing_all[(nSelIndex - 1)].m_ProcessingImage, 200);
                //}
                //else if (nameof(EmguVisionDilation) == ResultProcessing_all[nSelIndex].sProcessingName)
                //{
                //    list_EmguVisionAction[nSelIndex].ResultProcessing = ((EmguVisionDilation)(list_EmguVisionAction[nSelIndex].FuncProcessing)).Execute(list_EmguVisionAction[(nSelIndex - 1)].ResultProcessing.m_ProcessingImage);

                //    //EmguVisionDilation TempVision = new EmguVisionDilation();
                //    //ResultProcessing_all[nSelIndex] = TempVision.Execute(ResultProcessing_all[(nSelIndex - 1)].m_ProcessingImage);

                //    //ResultProcessing_all[nSelIndex] = ApplyDilation(ResultProcessing_all[(nSelIndex - 1)].m_ProcessingImage);
                //}
                //else if (nameof(EmguVisionErosion) == ResultProcessing_all[nSelIndex].sProcessingName)
                //{
                //    list_EmguVisionAction[nSelIndex].ResultProcessing = ((EmguVisionErosion)(list_EmguVisionAction[nSelIndex].FuncProcessing)).Execute(list_EmguVisionAction[(nSelIndex - 1)].ResultProcessing.m_ProcessingImage);
                //    //EmguVisionErosion TempVision = new EmguVisionErosion();
                //    //ResultProcessing_all[nSelIndex] = TempVision.Execute(ResultProcessing_all[(nSelIndex - 1)].m_ProcessingImage);

                //    //ResultProcessing_all[nSelIndex] = ApplyErosion(ResultProcessing_all[(nSelIndex - 1)].m_ProcessingImage);
                //}
                //else if (nameof(EmguVisionSimpleBlob) == ResultProcessing_all[nSelIndex].sProcessingName)
                //{
                //    list_EmguVisionAction[nSelIndex].ResultProcessing = ((EmguVisionSimpleBlob)(list_EmguVisionAction[nSelIndex].FuncProcessing)).Execute(list_EmguVisionAction[(nSelIndex - 1)].ResultProcessing.m_ProcessingImage);
                //    //EmguVisionSimpleBlob TempVision = new EmguVisionSimpleBlob();
                //    //ResultProcessing_all[nSelIndex] = TempVision.Execute(ResultProcessing_all[(nSelIndex - 1)].m_ProcessingImage);

                //    //ResultProcessing_all[nSelIndex] = FindBlobs(ResultProcessing_all[(nSelIndex - 1)].m_ProcessingImage);
                //}
                //else if (nameof(EmguVisionContoursBlob) == ResultProcessing_all[nSelIndex].sProcessingName)
                //{
                //    list_EmguVisionAction[nSelIndex].ResultProcessing = ((EmguVisionContoursBlob)(list_EmguVisionAction[nSelIndex].FuncProcessing)).Execute(list_EmguVisionAction[(nSelIndex - 1)].ResultProcessing.m_ProcessingImage);
                //    //EmguVisionContoursBlob TempVision = new EmguVisionContoursBlob();
                //    //ResultProcessing_all[nSelIndex] = TempVision.Execute(ResultProcessing_all[(nSelIndex - 1)].m_ProcessingImage);

                //    //ResultProcessing_all[nSelIndex] = DetectBlobs(ResultProcessing_all[(nSelIndex-1)].m_ProcessingImage);
                //}

                #endregion ori Code .. 
            }

            //DrawImage(FuncProcessing_all[nSelIndex].Result.m_ProcessingImage, pb_DrawImage);

            DrawTool.DisImage = FuncProcessing_all[nSelIndex].Result.m_ProcessingImage;

            pb_DrawImage.Invalidate();

            //switch (nSelIndex)
            //{
            //    case 1: dgvCuttingRecipe_Int_Click(sender, e.ColumnIndex, nSelIndex); break;
            //    case 2: dgvCuttingRecipe_double_Click(sender, e.ColumnIndex, nSelIndex); break;
            //    case 3: dgvCuttingRecipe_Int_Click(sender, e.ColumnIndex, nSelIndex); break;
            //    case 4: dgvCuttingRecipe_double_Click(sender, e.ColumnIndex, nSelIndex); break;
            //    case 5: dgvCuttingRecipe_Int_Click(sender, e.ColumnIndex, nSelIndex); break;
            //    default: break;
            //}
        }


        void UpdateProcessingImage()
        {
            if (FuncProcessing_all.Count > 0)
            {
                for (int i = 0; i < FuncProcessing_all.Count; i++)
                {
                    ProcessingList.Rows[i].HeaderCell.Value = i.ToString();
                    ProcessingList.Rows[i].Cells[0].Value = FuncProcessing_all[i].GetName();
                    //ProcessingList.Rows[i].Cells[1].Value = ResultProcessing_all[i].sProcessingName;
                }
            }
        }

        private void tmUpdate_Tick(object sender, EventArgs e)
        {
            UpdateProcessingImage();

            if (FuncProcessing_all.Count > 0)
            {
                if (FuncProcessing_all.Count > nSelIndex)
                {
                    tbProcessingResult.Text = FuncProcessing_all[nSelIndex].Result.sResult_Json;
                }
            }
        }

        private void btn_Delete_Click(object sender, EventArgs e)
        {
            int nSelect = ProcessingList.CurrentRow.Index;

            if (FuncProcessing_all.Count > 0)
            {
                for (int i = 0; i < FuncProcessing_all.Count; i++)
                {
                    ProcessingList.Rows[i].Cells[0].Value = "";
                    ProcessingList.Rows[i].Cells[1].Value = "";
                }
            }

            FuncProcessing_all.RemoveAt(nSelect);
        }

        private void btn_Reset_Click(object sender, EventArgs e)
        {
            int nSelect = ProcessingList.CurrentRow.Index;

            if (FuncProcessing_all.Count > 0)
            {
                for (int i = 0; i < FuncProcessing_all.Count; i++)
                {
                    ProcessingList.Rows[i].Cells[0].Value = "";
                    ProcessingList.Rows[i].Cells[1].Value = "";
                }
            }

            FuncProcessing_all.Clear();
        }

        private void ProcessingList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int nSelectRow = ProcessingList.CurrentRow.Index;

            FuncProcessing_all[nSelectRow].ShowParamForm();
            //ResultProcessing_all[nSelectRow].ProcessingFunc.ShowParamForm();

        }
    }

    public class Result_Processing
    {
        public Result_Processing(string sName)
        {
            m_ProcessingImage = new Mat();
            sProcessingName = sName;

            sResult_Json = "";
            sResult_Type = "";
        }

        public bool m_bIsOK = false;
        public string sProcessingName;
        public Mat m_ProcessingImage;

        public string sResult_Json;
        public string sResult_Type;
    }

}
