using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dnn;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static EmguCV.EmguVisionContoursBlob;
using static System.Reflection.Metadata.BlobBuilder;

namespace EmguCV
{

    public class EmguVisionBinary : EmguVisionFunc
    {
        public EmguVisionBinary()
        {
            EmguVisionParamter paramThreshold = new EmguVisionParamter();
            paramThreshold.sName = "Threshold";
            paramThreshold.sValue = "200";
            listParameters.Add(paramThreshold);
        }

        override public string GetName()
        {
            return nameof(EmguVisionBinary);
        }

        override public bool Execute(Mat SourceImg)
        {
            base.Execute(SourceImg);
            //EmguVision_Result Ret = new EmguVision_Result(nameof(EmguVisionBinary));
            int nThreshold = 0;

            if (false == int.TryParse(listParameters[0].sValue, out nThreshold))
            {
                return false;
            }

            CvInvoke.Threshold(m_SourceImage, Result.m_ProcessingImage, nThreshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

            Result.m_bIsOK = true;
            return Result.m_bIsOK;
        }
    }

    public class EmguVisionDilation : EmguVisionFunc
    {
        public EmguVisionDilation()
        {
            EmguVisionParamter paramiterations = new EmguVisionParamter();

            paramiterations.sName = "iterations";
            paramiterations.sValue = "1";

            listParameters.Add(paramiterations);
        }

        override public string GetName()
        {
            return nameof(EmguVisionDilation);
        }

        override public bool Execute(Mat SourceImg)
        {
            base.Execute(SourceImg);
            //EmguVision_Result Ret = new EmguVision_Result();
            int niterations = 0;

            if (false == int.TryParse(listParameters[0].sValue, out niterations))
            {
                return false;
            }

            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
            CvInvoke.Dilate(m_SourceImage, Result.m_ProcessingImage, kernel, new Point(-1, -1), niterations, BorderType.Default, new MCvScalar(0));
            Result.m_bIsOK = true;

            return Result.m_bIsOK;
        }
    }

    public class EmguVisionErosion : EmguVisionFunc
    {
        public EmguVisionErosion()
        {
            EmguVisionParamter paramiterations = new EmguVisionParamter();

            paramiterations.sName = "iterations";
            paramiterations.sValue = "1";

            listParameters.Add(paramiterations);
        }

        override public string GetName()
        {
            return nameof(EmguVisionErosion);
        }

        override public bool Execute(Mat SourceImg)
        {
            base.Execute(SourceImg);

            //EmguVision_Result Ret = new EmguVision_Result();
            int niterations = 0;

            if (false == int.TryParse(listParameters[0].sValue, out niterations))
            {
                return false;
            }

            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
            CvInvoke.Erode(m_SourceImage, Result.m_ProcessingImage, kernel, new Point(-1, -1), niterations, BorderType.Default, new MCvScalar(0));
            Result.m_bIsOK = true;

            return Result.m_bIsOK;
        }
    }

    public class EmguVisionSimpleBlob : EmguVisionFunc
    {
        enum Paramlist
        {
            param_Searchcolor = 0,
            param_minArea,
            param_maxArea,

            param_Max,
        }

        public EmguVisionSimpleBlob()
        {
            for(int i=0;i<(int)Paramlist.param_Max;i++)
            {
                EmguVisionParamter paramiterations = new EmguVisionParamter();

                paramiterations.sName = ((Paramlist)i).ToString();

                if (0 == i) paramiterations.sValue = "255";
                else if (1 == i) paramiterations.sValue = "10";
                else if (2 == i) paramiterations.sValue = "10000";
                else paramiterations.sValue = "1";

                listParameters.Add(paramiterations);
            }
        }

        override public string GetName()
        {
            return nameof(EmguVisionSimpleBlob);
        }

        override public bool Execute(Mat SourceImg)
        {
            base.Execute(SourceImg);

            //EmguVision_Result Ret = new EmguVision_Result();
            int nSearchColor = 0;
            int nMinArea = 0;
            int nMaxArea = 0;

            if (false == int.TryParse(listParameters[(int)Paramlist.param_Searchcolor].sValue, out nSearchColor))
            {
                return false;
            }

            if (false == int.TryParse(listParameters[(int)Paramlist.param_minArea].sValue, out nMinArea))
            {
                return false;
            }

            if (false == int.TryParse(listParameters[(int)Paramlist.param_maxArea].sValue, out nMaxArea))
            {
                return false;
            }

            SimpleBlobDetectorParams parameters = new SimpleBlobDetectorParams
            {
                FilterByColor = true,
                blobColor = (byte)nSearchColor, // 흰색 블롭 탐지
                FilterByArea = true,
                MinArea = (float)nMinArea, // 최소 블롭 크기
                MaxArea = (float)nMaxArea // 최대 블롭 크기
            };

            SimpleBlobDetector detector = new SimpleBlobDetector(parameters);

            // 블롭 키포인트 찾기
            MKeyPoint[] keypoints = detector.Detect(m_SourceImage);

            if (SourceImg != null)
            {
                // 블롭을 이미지에 그리기
                //Mat outputImage = new Mat();
                //Features2DToolbox.DrawKeypoints(image, new VectorOfKeyPoint(keypoints), m_ProcessingImage, new Bgr(Color.Red), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
                Features2DToolbox.DrawKeypoints(m_SourceImage, new VectorOfKeyPoint(keypoints), Result.m_ProcessingImage, new Bgr(Color.Red), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
                if (keypoints.Length > 0)
                {
                    //string sWritetext = $" keypoints.Length : {keypoints.Length}";
                    //WriteTextOnImage(m_ProcessingImage, sWritetext, new Point(50, 50), 1.0, new Bgr(Color.White), 2);

                    foreach (MKeyPoint keypoint in keypoints)
                    {
                        int nOffset = 10;
                        int nImgposX = (int)(keypoint.Point.X - (keypoint.Size / 2) - (nOffset / 2));
                        int nImgposY = (int)(keypoint.Point.Y - (keypoint.Size / 2) - (nOffset / 2));
                        int nImgposWidth = (int)(keypoint.Size + nOffset);
                        int nImgposHeight = (int)(keypoint.Size + nOffset);

                        Rectangle cropRect = new Rectangle(nImgposX, nImgposY, nImgposWidth, nImgposHeight);
                        Mat croppedImage = new Mat(m_SourceImage, cropRect);

                        //double dFocusValue = GetFocusValue_useLaplacian(croppedImage);

                        string scroppedFilePath = $"D:\\ResultImage\\crop_{nImgposX},{nImgposY},{nImgposWidth},{nImgposHeight}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.bmp";
                        CvInvoke.Imwrite(scroppedFilePath, croppedImage);
                    }

                    //Rectangle cropRect = new Rectangle(50, 50, 200, 200);
                    //Mat croppedImage = new Mat(m_ProcessingImage, cropRect);

                    Bitmap bitmapImage = new Bitmap(m_SourceImage.Width, m_SourceImage.Height, m_SourceImage.Step, System.Drawing.Imaging.PixelFormat.Format24bppRgb, m_SourceImage.DataPointer);

                    string sOutputFilePath = $"D:\\ResultImage\\Blob_{DateTime.Now.ToString("yyyyMMddHHmmss")}.bmp";
                    CvInvoke.Imwrite(sOutputFilePath, m_SourceImage);

                    Result.m_bIsOK = true;
                }

                int nTempImgposX = 395;
                int nTempImgposY = 216;
                int nTempImgposWidth = 60;
                int nTempImgposHeight = 60;

                Rectangle cropTRect = new Rectangle(nTempImgposX, nTempImgposY, nTempImgposWidth, nTempImgposHeight);
                Mat croppedTImage = new Mat(m_SourceImage, cropTRect);            }

            Result.m_bIsOK = true;

            return Result.m_bIsOK;
        }
    }

    public class EmguVisionContoursBlob : EmguVisionFunc
    {
        public class RotatedRectDTO
        {
            public PointF Center { get; set; }
            public SizeF Size { get; set; }
            public float Angle { get; set; }
            public RotatedRectDTO(RotatedRect rect)
            {
                Center = rect.Center;
                Size = rect.Size;
                Angle = rect.Angle;
            }
        }

        public EmguVisionContoursBlob()
        {
            //EmguVisionParamter paramiterations = new EmguVisionParamter();

            //paramiterations.sName = "iterations";
            //paramiterations.sValue = "1";

            //listParameters.Add(paramiterations);
        }

        override public string GetName()
        {
            return nameof(EmguVisionContoursBlob);
        }

        override public bool Execute(Mat SourceImg)
        {
            //EmguVision_Result Ret = new EmguVision_Result();
            //int niterations = 0;

            //if (false == int.TryParse(listParameters[0].sValue, out niterations))
            //{
            //    return Ret;
            //}

            base.Execute(SourceImg);

            //Ret.m_ProcessingImage = SourceImg.Clone();

            // Search Contour
            using VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(m_SourceImage, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            List<RotatedRect> blobs = new List<RotatedRect>();

            // 모든 블롭에 대해 바운딩 박스 찾기
            for (int i = 0; i < contours.Size; i++)
            {
                RotatedRect box = CvInvoke.MinAreaRect(contours[i]);
                blobs.Add(box);
            }

            List<RotatedRectDTO> dtoList = blobs.ConvertAll(rect => new RotatedRectDTO(rect));

            Result.sResult_Json = JsonConvert.SerializeObject(dtoList, Formatting.Indented);

            //// 면적 기준 정렬 후 상위 2개 선택
            //blobs.Sort((a, b) => (int)(b.Size.Height * b.Size.Width.CompareTo(a.Size.Height * a.Size.Width)));
            //return blobs.Count > 2 ? blobs.GetRange(0, 2) : blobs;

            foreach (RotatedRect blob in blobs)
            {
                // 바운딩 박스 그리기
                PointF[] vertices = blob.GetVertices();

                for (int j = 0; j < 4; j++)
                {
                    CvInvoke.Line(m_SourceImage, Point.Round(vertices[j]), Point.Round(vertices[(j + 1) % 4]), new MCvScalar(0, 0, 255), 2);
                }
            }

            Result.m_bIsOK = true;

            return Result.m_bIsOK;
        }
    }

    public class EmguVisionDetectCircle : EmguVisionFunc
    {
        enum Paramlist
        {
            param_distance = 0,
            param_threshold,
            param_center,
            param_minRadius,
            param_maxRadius,

            param_Max,
        }

        public EmguVisionDetectCircle()
        {
            for (Paramlist i = 0; i < Paramlist.param_Max; i++)
            {
                EmguVisionParamter paramiterations = new EmguVisionParamter();

                paramiterations.sName = (i).ToString();

                if (Paramlist.param_distance == i) paramiterations.sValue = "30";
                else if (Paramlist.param_threshold == i) paramiterations.sValue = "100";
                else if (Paramlist.param_center == i) paramiterations.sValue = "20";
                else if (Paramlist.param_minRadius == i) paramiterations.sValue = "65";
                else if (Paramlist.param_maxRadius == i) paramiterations.sValue = "150";
                else paramiterations.sValue = "1";

                listParameters.Add(paramiterations);
            }
        }

        override public string GetName()
        {
            return nameof(EmguVisionDetectCircle);
        }

        override public bool Execute(Mat SourceImg)
        {
            //EmguVision_Result Ret = new EmguVision_Result();
            //int niterations = 0;

            //if (false == int.TryParse(listParameters[0].sValue, out niterations))
            //{
            //    return Ret;
            //}

            //Ret.m_ProcessingImage = SourceImg.Clone();

            //Mat gray = new Mat();

            //// 1. 이미지를 Grayscale로 변환
            //CvInvoke.CvtColor(inputImage, gray, ColorConversion.Bgr2Gray);

            base.Execute(SourceImg);

            Mat destImage = new Mat();

            // 2. 가우시안 블러(Blur) 적용하여 노이즈 제거
            CvInvoke.GaussianBlur(SourceImg, destImage, new Size(9, 9), 2, 2);

            // 3. 원 탐지 실행
            CircleF[] circles = CvInvoke.HoughCircles(
                destImage,
                HoughModes.Gradient, // Hough Gradient 방법
                1.0,                 // 축소 비율 (1.0 = 원본 크기 사용)
                30.0,                // 원 간의 최소 거리 (픽셀)
                100.0,               // Canny 에지 검출기의 임곗값 (low threshold)
                20.0,                // 센터 검출 임곗값
                10,                  // 탐지할 최소 반지름
                100                  // 탐지할 최대 반지름
            );

            List<CircleF> dtoList = new List<CircleF>();

            // 4. 원 탐지 결과를 입력 이미지에 그리기
            Mat outputImage = destImage.Clone();
            foreach (var circle in circles)
            {
                // 원의 경계선 그리기
                CvInvoke.Circle(outputImage, Point.Round(circle.Center), (int)circle.Radius, new MCvScalar(0, 255, 0), 2);

                // 원의 중심점 그리기
                CvInvoke.Circle(outputImage, Point.Round(circle.Center), 2, new MCvScalar(0, 0, 255), 3);

                dtoList.Add(circle);
            }

            Result.sResult_Json = JsonConvert.SerializeObject(dtoList, Formatting.Indented);

            Result.m_ProcessingImage = outputImage;
            Result.m_bIsOK = true;

            return Result.m_bIsOK;
        }
    }

    public class EmguVisionFunc
    {
        public List<EmguVisionParamter> listParameters = new List<EmguVisionParamter>();

        // func 의 source image 를 내가 원하는 이미지로 바꿀수 있으면 좋겠다..
        public Mat m_SourceImage = null;

        public EmguVision_Result Result = new EmguVision_Result();

        //ParamForm param = null;

        public EmguVisionFunc()
        {

        }

        public void ParamFormClose()
        {
            //param = null;
        }

        public void ShowParamForm()
        {
            //if (param == null)
            //{
            //    ParamForm param = new ParamForm();
            //    param._formclose = new ParamForm.FormClose(ParamFormClose);

            //    // CellValueChanged 이벤트 핸들러 추가
            //    param.dataGridView.CellValueChanged += DataGridView_CellValueChanged;
            //    param.dataGridView.CellClick += DataGridView_CellClick;
            //}

            ParamForm param = new ParamForm();
            param._formclose = new ParamForm.FormClose(ParamFormClose);

            // CellValueChanged 이벤트 핸들러 추가
            param.dataGridView.CellValueChanged += DataGridView_CellValueChanged;
            param.dataGridView.CellClick += DataGridView_CellClick;

            param.dataGridView.Rows.Clear();

            for (int i=0;i< listParameters.Count; i++)
            {
                param.AddParameter(listParameters[i].sName, listParameters[i].sValue);
            }

            param.Show();
        }

        private void DataGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                //    string paramName = dataGridView.Rows[e.RowIndex].Cells[0].Value.ToString();
                //    string paramValue = dataGridView.Rows[e.RowIndex].Cells[1].Value.ToString();

                //    // 여기서 값 변경에 대한 처리를 수행합니다.
                //    MessageBox.Show($"Parameter '{paramName}' changed to '{paramValue}'");
            }
        }

        // CellValueChanged 이벤트 핸들러
        private void DataGridView_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                listParameters[e.RowIndex].sValue = (string)((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

                Execute(m_SourceImage);

                //    string paramName = dataGridView.Rows[e.RowIndex].Cells[0].Value.ToString();
                //    string paramValue = dataGridView.Rows[e.RowIndex].Cells[1].Value.ToString();

                //    // 여기서 값 변경에 대한 처리를 수행합니다.
                //    MessageBox.Show($"Parameter '{paramName}' changed to '{paramValue}'");
            }
        }

        public int GetParamCount()
        {
            return listParameters.Count;
        }

        public string GetParamName(int nIndex)
        {
            string sRet = "";

            if (listParameters.Count == 0 || ((listParameters.Count+1) < nIndex)) return sRet;

            return listParameters[nIndex].sName;
        }

        public string GetParamValue(int nIndex)
        {
            string sRet = "";

            if (listParameters.Count == 0 || ((listParameters.Count + 1) < nIndex)) return sRet;

            return listParameters[nIndex].sValue;
        }

        public string[] GetParamLists(out int nCount)
        {
            string[] sRet = new string[listParameters.Count];
            for (int i = 0; i < listParameters.Count; i++)
            {
                sRet[i] = listParameters[i].sName;
            }

            nCount = listParameters.Count;
            return sRet;
        }

        public void SetParam(string sName, string sValue)
        {
            foreach (EmguVisionParamter param in listParameters)
            {
                if (param.sName == sName)
                {
                    param.sValue = sValue;
                    break;
                }
            }
        }

        virtual public string GetName()
        {
            return nameof(EmguVisionFunc);
        }

        virtual public bool Execute(Mat SourceImg)
        {
            m_SourceImage = SourceImg.Clone();
            Result.m_ProcessingImage = SourceImg.Clone();
            Result.m_bIsOK = false;
            
            return true;
        }

        //private EmguVision_Result FindBlobs(Mat inputImage, byte btSearchBlobColor = 255, float fMinArea = 200, float fMaxArea = 10000)
        //{
        //    EmguVision_Result Ret = new EmguVision_Result("FindBlobs");

        //    // 블롭 탐지기 설정
        //    SimpleBlobDetectorParams parameters = new SimpleBlobDetectorParams
        //    {
        //        FilterByColor = true,
        //        blobColor = btSearchBlobColor, // 흰색 블롭 탐지
        //        FilterByArea = true,
        //        MinArea = fMinArea, // 최소 블롭 크기
        //        MaxArea = fMaxArea // 최대 블롭 크기
        //    };

        //    SimpleBlobDetector detector = new SimpleBlobDetector(parameters);

        //    // 블롭 키포인트 찾기
        //    MKeyPoint[] keypoints = detector.Detect(inputImage);

        //    if (inputImage != null)
        //    {
        //        // 블롭을 이미지에 그리기
        //        //Mat outputImage = new Mat();
        //        //Features2DToolbox.DrawKeypoints(image, new VectorOfKeyPoint(keypoints), m_ProcessingImage, new Bgr(Color.Red), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
        //        Features2DToolbox.DrawKeypoints(inputImage, new VectorOfKeyPoint(keypoints), Ret.m_ProcessingImage, new Bgr(Color.Red), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
        //        if (keypoints.Length > 0)
        //        {
        //            //string sWritetext = $" keypoints.Length : {keypoints.Length}";
        //            //WriteTextOnImage(m_ProcessingImage, sWritetext, new Point(50, 50), 1.0, new Bgr(Color.White), 2);

        //            foreach (MKeyPoint keypoint in keypoints)
        //            {
        //                int nOffset = 10;
        //                int nImgposX = (int)(keypoint.Point.X - (keypoint.Size / 2) - (nOffset / 2));
        //                int nImgposY = (int)(keypoint.Point.Y - (keypoint.Size / 2) - (nOffset / 2));
        //                int nImgposWidth = (int)(keypoint.Size + nOffset);
        //                int nImgposHeight = (int)(keypoint.Size + nOffset);

        //                Rectangle cropRect = new Rectangle(nImgposX, nImgposY, nImgposWidth, nImgposHeight);
        //                Mat croppedImage = new Mat(ResultProcessing_all[0].m_ProcessingImage, cropRect);

        //                //double dFocusValue = GetFocusValue_useLaplacian(croppedImage);

        //                string scroppedFilePath = $"D:\\ResultImage\\crop_{nImgposX},{nImgposY},{nImgposWidth},{nImgposHeight}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.bmp";
        //                CvInvoke.Imwrite(scroppedFilePath, croppedImage);
        //            }

        //            //Rectangle cropRect = new Rectangle(50, 50, 200, 200);
        //            //Mat croppedImage = new Mat(m_ProcessingImage, cropRect);

        //            Bitmap bitmapImage = new Bitmap(Ret.m_ProcessingImage.Width, Ret.m_ProcessingImage.Height, Ret.m_ProcessingImage.Step, System.Drawing.Imaging.PixelFormat.Format24bppRgb, Ret.m_ProcessingImage.DataPointer);

        //            string sOutputFilePath = $"D:\\ResultImage\\Blob_{DateTime.Now.ToString("yyyyMMddHHmmss")}.bmp";
        //            CvInvoke.Imwrite(sOutputFilePath, Ret.m_ProcessingImage);

        //            Ret.m_bIsOK = true;
        //        }

        //        int nTempImgposX = 395;
        //        int nTempImgposY = 216;
        //        int nTempImgposWidth = 60;
        //        int nTempImgposHeight = 60;

        //        Rectangle cropTRect = new Rectangle(nTempImgposX, nTempImgposY, nTempImgposWidth, nTempImgposHeight);
        //        Mat croppedTImage = new Mat(ResultProcessing_all[0].m_ProcessingImage, cropTRect);

        //        //double dTFocusValue = GetFocusValue_useLaplacian(croppedTImage);


        //        //pb_FindBlob.Image = bitmapImage;

        //        //// PictureBox에 결과 이미지 표시
        //        //PictureBox pictureBox = new PictureBox
        //        //{
        //        //    Image = outputImage.ToBitmap(),
        //        //    Dock = DockStyle.Fill
        //        //};
        //        //this.Controls.Add(pictureBox);
        //    }

        //    return Ret;
        //}
    }
    public class EmguVision_Result
    {
        public EmguVision_Result()
        {
            m_ProcessingImage = new Mat();
            //sProcessingName = sName;

            sResult_Json = "";
        }

        public bool m_bIsOK = false;
        public Mat m_ProcessingImage;

        public string sResult_Json;

        //public EmguVisionFunc ProcessingFunc;
    }



    public class ParamForm : Form
    {
        public delegate void FormClose();
        public FormClose _formclose;

        public DataGridView dataGridView;

        public ParamForm()
        {
            this.Text = "Parameter Form";
            this.Size = new System.Drawing.Size(400, 300);

            InitializeDataGridView();
        }

        //~ParamForm()
        //{
        //    _formclose();
        //}


        private void InitializeDataGridView()
        {
            dataGridView = new DataGridView();
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            // 컬럼 추가
            dataGridView.Columns.Add("ParameterName", "Parameter Name");
            dataGridView.Columns.Add("ParameterValue", "Parameter Value");

            this.Controls.Add(dataGridView);
        }

        // 파라미터를 DataGridView에 추가하는 메서드
        public void AddParameter(string name, string value)
        {
            dataGridView.Rows.Add(name, value);
        }
    }


}
