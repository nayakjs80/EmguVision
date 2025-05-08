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

        const double dMaxImageScale = 10.0;
        const double dMinImageScale = 1.0;

        double m_dDrawScale = 1.0;
        int m_nCtrlBasePosX = 0;
        int m_nCtrlBasePosY = 0;

        int m_nDisTempPosX = 0;
        int m_nDisTempPosY = 0;

        int m_nDisStartPosX = 0;
        int m_nDisStartPosY = 0;
        int m_nDisWidth = 0;
        int m_nDisHeight = 0;

        int m_nDisCtrlWidth = 0;
        int m_nDisCtrlHeight = 0;

        double m_dDisScaleX = 1.0;
        double m_dDisScaleY = 1.0;

        int m_nPickStartPosX = 0;
        int m_nPickStartPosY = 0;

        int nIndex_RetProcessing = 0;
        //Result_Processing[] ResultProcessing_all = new Result_Processing[nMaxProcessing];
        //List<Result_Processing> ResultProcessing_all = new List<Result_Processing>();

        //List<EmguVisionAction> list_EmguVisionAction = new List<EmguVisionAction>();

        List<EmguVisionFunc> FuncProcessing_all = new List<EmguVisionFunc>();

        public Form1()
        {
            InitializeComponent();

            ProcessingList.Rows.Add(nMaxProcessing);

            m_nDisCtrlWidth = pb_DrawImage.Width;
            m_nDisCtrlHeight = pb_DrawImage.Height;

            pb_DrawImage.MouseWheel += pb_DrawImage_MouseWheel;
            pb_DrawImage.MouseMove += pb_DrawImage_MouseEvent;

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

                m_nDisStartPosX = 1;
                m_nDisStartPosY = 1;
                m_nDisWidth = LoadImage.Result.m_ProcessingImage.Width;
                m_nDisHeight = LoadImage.Result.m_ProcessingImage.Height;

                m_dDisScaleX = m_nDisWidth / (double)m_nDisCtrlWidth;
                m_dDisScaleY = m_nDisHeight / (double)m_nDisCtrlHeight;
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

        //double GetFocusValue_FFT(Mat image)
        //{
        //    //// FFT 변환 적용
        //    //Mat fft = new Mat();
        //    //CvInvoke.Dft(image, fft, DxtType.Forward, 0);

        //    //// FFT 변환 결과의 분산 계산
        //    //Mat mean = new Mat();
        //    //Mat stddev = new Mat();
        //    //CvInvoke.MeanStdDev(fft, mean, stddev);

        //    //// 분산 값을 반환 (선명도 값)
        //    //Array stddevData = stddev.GetData();

        //    //return (double)stddevData.GetValue(0, 0);


        //    var optimalRow = CvInvoke.GetOptimalDFTSize(image.Rows);
        //    var optimalCol = CvInvoke.GetOptimalDFTSize(image.Cols);
        //    Mat padded = new Mat();
        //    CvInvoke.CopyMakeBorder(image, padded, 0, optimalRow - image.Rows, 0, optimalCol - image.Cols, BorderType.Constant, new MCvScalar(0));

        //    Mat[] planes = { new Mat(padded.Size, DepthType.Cv32F, 1), Mat.Zeros(padded.Size.Width, padded.Size.Height, DepthType.Cv32F, 1) };
        //    padded.ConvertTo(planes[0], DepthType.Cv32F);
        //    Mat complexImage = new Mat();
        //    CvInvoke.Merge(planes, complexImage);

        //    CvInvoke.Dft(complexImage, complexImage, DftType.ComplexOutput);

        //    Mat[] newPlanes = new Mat[2];
        //    CvInvoke.Split(complexImage, newPlanes);
        //    Mat magnitude = new Mat();
        //    CvInvoke.Magnitude(newPlanes[0], newPlanes[1], magnitude);

        //    CvInvoke.Add(magnitude, new MCvScalar(1), magnitude);
        //    CvInvoke.Log(magnitude, magnitude);

        //    Mat spectrum = magnitude.SubMat(new Rectangle(0, 0, magnitude.Cols & -2, magnitude.Rows & -2));
        //    CvInvoke.Normalize(spectrum, spectrum, 0, 1, NormType.MinMax);
        //    var meanValue = CvInvoke.Mean(spectrum).V0;
        //    Console.WriteLine($"FFT Mean Value: {meanValue}"
        //}


        //private Mat Binary(Mat SourceImg, int nThreshold)
        //{
        //    Mat DestImg = new Mat();
        //    CvInvoke.Threshold(SourceImg, DestImg, nThreshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

        //    return DestImg;
        //}

        //private Result_Processing Binary(Mat SourceImg, int nThreshold)
        //{
        //    Result_Processing Ret = new Result_Processing(nameof(Binary));
        //    CvInvoke.Threshold(SourceImg, Ret.m_ProcessingImage, nThreshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

        //    Ret.m_bIsOK = true;
        //    return Ret;
        //}

        //public Result_Processing ApplyDilation(Mat inputImage, int iterations = 1)
        //{
        //    Result_Processing Ret = new Result_Processing(nameof(ApplyDilation));
        //    Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
        //    CvInvoke.Dilate(inputImage, Ret.m_ProcessingImage, kernel, new Point(-1, -1), iterations, BorderType.Default, new MCvScalar(0));

        //    Ret.m_bIsOK = true;
        //    return Ret;
        //}

        //public Result_Processing ApplyErosion(Mat inputImage, int iterations = 1)
        //{
        //    Result_Processing Ret = new Result_Processing(nameof(ApplyErosion));
        //    Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
        //    CvInvoke.Erode(inputImage, Ret.m_ProcessingImage, kernel, new Point(-1, -1), iterations, BorderType.Default, new MCvScalar(0));

        //    Ret.m_bIsOK = true;
        //    return Ret;
        //}

        //private Result_Processing FindBlobs(Mat image)
        //{

        //    Result_Processing Ret = new Result_Processing(nameof(FindBlobs));
        //    // 블롭 탐지기 설정
        //    SimpleBlobDetectorParams parameters = new SimpleBlobDetectorParams
        //    {
        //        FilterByColor = true,
        //        blobColor = 255, // 흰색 블롭 탐지
        //        FilterByArea = true,
        //        MinArea = 1, // 최소 블롭 크기
        //        MaxArea = 10000 // 최대 블롭 크기
        //    };

        //    SimpleBlobDetector detector = new SimpleBlobDetector(parameters);

        //    // 블롭 키포인트 찾기
        //    MKeyPoint[] keypoints = detector.Detect(image);

        //    if (image != null)
        //    {
        //        // 블롭을 이미지에 그리기
        //        //Mat outputImage = new Mat();
        //        //Features2DToolbox.DrawKeypoints(image, new VectorOfKeyPoint(keypoints), m_ProcessingImage, new Bgr(Color.Red), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
        //        Features2DToolbox.DrawKeypoints(image, new VectorOfKeyPoint(keypoints), Ret.m_ProcessingImage, new Bgr(Color.Red), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
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

        //        double dTFocusValue = GetFocusValue_useLaplacian(croppedTImage);


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

        //public Result_Processing DetectBlobs(Mat inputimage)
        //{
        //    //// 이미지 로드
        //    //using Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        //    //using Mat binaryImage = new Mat();

        //    //// 이진화 (Thresholding)
        //    //CvInvoke.Threshold(image, binaryImage, 100, 255, ThresholdType.Binary);

        //    Result_Processing ret = new Result_Processing(nameof(DetectBlobs));

        //    ret.m_ProcessingImage = inputimage.Clone();

        //    // 컨투어 찾기
        //    using VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
        //    CvInvoke.FindContours(ret.m_ProcessingImage, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

        //    List<RotatedRect> blobs = new List<RotatedRect>();

        //    // 모든 블롭에 대해 바운딩 박스 찾기
        //    for (int i = 0; i < contours.Size; i++)
        //    {
        //        //RotatedRect box = CvInvoke.MinAreaRect(contours[i]);
        //        RotatedRect box = CvInvoke.MinAreaRect(contours[i]);
        //        blobs.Add(box);
        //    }

        //    //List<RotatedRectDTO> dtoList = blobs.ConvertAll(rect => new RotatedRectDTO(rect));

        //    //ret.sResult_Json = JsonConvert.SerializeObject(dtoList, Formatting.Indented);

        //    //// 면적 기준 정렬 후 상위 2개 선택
        //    //blobs.Sort((a, b) => (int)(b.Size.Height * b.Size.Width.CompareTo(a.Size.Height * a.Size.Width)));
        //    //return blobs.Count > 2 ? blobs.GetRange(0, 2) : blobs;

        //    foreach (RotatedRect blob in blobs)
        //    {
        //        // 바운딩 박스 그리기
        //        PointF[] vertices = blob.GetVertices();

        //        for (int j = 0; j < 4; j++)
        //        {
        //            CvInvoke.Line(ret.m_ProcessingImage, Point.Round(vertices[j]), Point.Round(vertices[(j + 1) % 4]), new MCvScalar(0, 0, 255), 2);
        //        }
        //    }

        //    return ret;
        //}

        //public Result_Processing DetectCircles(Mat inputImage)
        //{
        //    Result_Processing ret = new Result_Processing(nameof(DetectCircles));

        //    //Mat gray = new Mat();

        //    //// 1. 이미지를 Grayscale로 변환
        //    //CvInvoke.CvtColor(inputImage, gray, ColorConversion.Bgr2Gray);

        //    Mat destImage = new Mat();

        //    // 2. 가우시안 블러(Blur) 적용하여 노이즈 제거
        //    CvInvoke.GaussianBlur(inputImage, destImage, new Size(9, 9), 2, 2);

        //    // 3. 원 탐지 실행
        //    CircleF[] circles = CvInvoke.HoughCircles(
        //        destImage,
        //        HoughModes.Gradient, // Hough Gradient 방법
        //        1.0,                 // 축소 비율 (1.0 = 원본 크기 사용)
        //        30.0,                // 원 간의 최소 거리 (픽셀)
        //        100.0,               // Canny 에지 검출기의 임곗값 (low threshold)
        //        20.0,                // 센터 검출 임곗값
        //        10,                  // 탐지할 최소 반지름
        //        100                  // 탐지할 최대 반지름
        //    );

        //    // 4. 원 탐지 결과를 입력 이미지에 그리기
        //    Mat outputImage = destImage.Clone();
        //    foreach (var circle in circles)
        //    {
        //        // 원의 경계선 그리기
        //        CvInvoke.Circle(outputImage, Point.Round(circle.Center), (int)circle.Radius, new MCvScalar(0, 255, 0), 2);

        //        // 원의 중심점 그리기
        //        CvInvoke.Circle(outputImage, Point.Round(circle.Center), 2, new MCvScalar(0, 0, 255), 3);
        //    }

        //    ret.m_ProcessingImage = outputImage;
        //    ret.m_bIsOK = true;

        //    return ret;
        //}

        private void Watershed()
        {
            //// 이미지를 로드합니다.
            //string imagePath = "path_to_your_image.jpg"; // 이미지를 넣으세요.
            //Mat image = CvInvoke.Imread(imagePath, ImreadModes.Color);

            //// 이미지를 Grayscale로 변환
            //Mat gray = new Mat();
            //CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);

            //// 바이너리화 (Threshold)
            //Mat binary = new Mat();
            //CvInvoke.Threshold(gray, binary, 0, 255, ThresholdType.Otsu | ThresholdType.Binary);

            //// 거리 변환(Distance Transform) 적용
            //Mat distTransform = new Mat();
            //OutputArray arrlabels;
            //Mat labels = new Mat();
            //CvInvoke.DistanceTransform(binary, distTransform, labels, DistType.L2, 5, DistLabelType.CComp);
            //CvInvoke.Normalize(distTransform, distTransform, 0, 1, NormType.MinMax);

            //// 거리 변환 결과를 Threshold로 마커 생성
            //Mat markers = new Mat();
            //CvInvoke.Threshold(distTransform, markers, 0.5, 1.0, ThresholdType.Binary);
            //markers.ConvertTo(markers, DepthType.Cv8U);

            //// 연결된 구성요소를 찾아 마커에 레이블 부여
            //Mat labeledMarkers = new Mat();
            //CvInvoke.ConnectedComponents(markers, labeledMarkers);

            //// Watershed 적용
            //CvInvoke.Watershed(image, labeledMarkers);

            //// 결과 표시: 경계를 빨간색으로 표시
            //for (int y = 0; y < labeledMarkers.Rows; y++)
            //{
            //    for (int x = 0; x < labeledMarkers.Cols; x++)
            //    {
            //        int label = labeledMarkers.GetData(y, x)[0];
            //        if (label == -1) // Watershed 경계는 -1로 표시됩니다.
            //        {
            //            image.SetTo(new Bgr(0, 0, 255).MCvScalar, new Mat(binary, new Range(y, y + 1), new Range(x, x + 1)));
            //        }
            //    }
            //}

            //// 결과 이미지 저장 또는 보기
            //CvInvoke.Imwrite("result.jpg", image);
            //CvInvoke.Imshow("Watershed Result", image);
            //CvInvoke.WaitKey(0);
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

        private void ShowImage(int nBasePointX, int nBasePointY, int nCtrlWidth, int nCtrlHeight, PaintEventArgs e)
        {
            try
            {
                Mat DisplayImage = FuncProcessing_all[nSelIndex].Result.m_ProcessingImage;
                int nImageWidth = FuncProcessing_all[nSelIndex].Result.m_ProcessingImage.Width;
                int nImageHeight = FuncProcessing_all[nSelIndex].Result.m_ProcessingImage.Height;

                if (null != DisplayImage)
                {
                    Bitmap TempImage = new Bitmap(nImageWidth, nImageHeight, nImageWidth, PixelFormat.Format8bppIndexed, DisplayImage.DataPointer);
                    ColorPalette palette = TempImage.Palette;

                    // Grayscale 팔레트를 설정합니다.
                    for (int i = 0; i < 256; i++)
                    {
                        palette.Entries[i] = Color.FromArgb(i, i, i);
                    }

                    TempImage.Palette = palette;

                    //Bitmap TempImage = new Bitmap(nImageWidth, nImageHeight, nImageWidth, PixelFormat.Format8bppIndexed, DisplayImage.DataPointer);
                    //TempImage.Palette = m_pMainControlStn.SynovaCamera.GetBitmap_Palette_Y8();

                    //Rectangle Temp = new Rectangle(0, 0, nImageWidth, nImageHeight);
                    Rectangle Temp = new Rectangle(0, 0, nCtrlWidth, nCtrlHeight);
                    e.Graphics.DrawImage(TempImage, Temp);
                }
            }
            catch (Exception ex)
            {

            }
        }

        double dSelectPosX = 0;
        double dSelectPosY = 0;
        bool m_bImageMove = false;

        private void pb_DrawImage_MouseDown(object sender, MouseEventArgs e)
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

        private void pb_DrawImage_MouseEvent(object sender, MouseEventArgs e)
        {
            if(true == m_bImageMove)
            {
                int nPickCurPosX = (int)((e.X * m_dDisScaleX) / m_dDrawScale);
                int nPickCurPosY = (int)((e.Y * m_dDisScaleY) / m_dDrawScale);
                //int nPickCurPosX = e.X;
                //int nPickCurPosY = e.Y;

                m_nDisStartPosX = m_nDisTempPosX - (nPickCurPosX - m_nPickStartPosX);
                m_nDisStartPosY = m_nDisTempPosY - (nPickCurPosY - m_nPickStartPosY);

                pb_DrawImage.Invalidate();
            }
        }

        private void pb_DrawImage_MouseUp(object sender, MouseEventArgs e)
        {
            m_bImageMove = false;

            int nPickEndPosX = (int)((e.X * m_dDisScaleX) / m_dDrawScale);
            int nPickEndPosY = (int)((e.Y * m_dDisScaleY) / m_dDrawScale);

            m_nDisStartPosX = m_nDisTempPosX - (nPickEndPosX - m_nPickStartPosX);
            m_nDisStartPosY = m_nDisTempPosY - (nPickEndPosY - m_nPickStartPosY);

            m_nDisTempPosX = 0;
            m_nDisTempPosY = 0;

            pb_DrawImage.Invalidate();
        }

        private void pb_DrawImage_MouseWheel(object sender, MouseEventArgs e)
        {
            m_nCtrlBasePosX = e.X;
            m_nCtrlBasePosY = e.Y;

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
            pb_DrawImage.Invalidate();
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

        private void pb_DrawImage_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Mat DisplayImage = FuncProcessing_all[nSelIndex].Result.m_ProcessingImage;

                int nImageWidth = FuncProcessing_all[nSelIndex].Result.m_ProcessingImage.Width;
                int nImageHeight = FuncProcessing_all[nSelIndex].Result.m_ProcessingImage.Height;

                //int nCtrlWidth = (int)(((PictureBox)sender).Width * m_dDrawScale);
                //int nCtrlHeight = (int)(((PictureBox)sender).Height * m_dDrawScale);
                //int nCtrlWidth = ((PictureBox)sender).Width;
                //int nCtrlHeight = ((PictureBox)sender).Height;

                if (null != DisplayImage)
                {
                    Bitmap TempImage = new Bitmap(nImageWidth, nImageHeight, DisplayImage.Step, PixelFormat.Format8bppIndexed, DisplayImage.DataPointer);
                    ColorPalette palette = TempImage.Palette;

                    // Grayscale 팔레트를 설정합니다.
                    for (int i = 0; i < 256; i++)
                    {
                        palette.Entries[i] = Color.FromArgb(i, i, i);
                    }

                    TempImage.Palette = palette;

                    //Rectangle Temp = new Rectangle(0, 0, nImageWidth, nImageHeight);
                    //Rectangle rectSrc = new Rectangle(m_nCtrlBasePosX, m_nCtrlBasePosY, nImageWidth, nImageHeight);
                    //Rectangle rectSrc = new Rectangle(m_nDisStartPosX, m_nDisStartPosY, m_nDisWidth, m_nDisHeight);
                    Rectangle rectSrc = GetDisRect();
                    Rectangle rectDest = new Rectangle(0, 0, m_nDisCtrlWidth, m_nDisCtrlHeight);
                    //e.Graphics.DrawImage(TempImage, Temp);
                    e.Graphics.DrawImage(TempImage, rectDest, rectSrc, GraphicsUnit.Pixel);


                    //e.Graphics.DrawString()
                }
            }
            catch (Exception ex)
            {

            }
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
