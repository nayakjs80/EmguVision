namespace EmguCV
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            btn_ImageLoad = new Button();
            pb_DrawImage = new PictureBox();
            btn_SimpleBlob = new Button();
            btn_Binary = new Button();
            ProcessingList = new DataGridView();
            Processing = new DataGridViewTextBoxColumn();
            Execute = new DataGridViewButtonColumn();
            btn_ShowLoadImage = new Button();
            btn_ShowProcessingImage = new Button();
            btn_Laplacian = new Button();
            lbl_Laplacian = new Label();
            tmUpdate = new System.Windows.Forms.Timer(components);
            btn_Dilation = new Button();
            btn_Erosion = new Button();
            btn_Delete = new Button();
            btn_DetectCircles = new Button();
            btn_Reset = new Button();
            btn_DetectBlob = new Button();
            tbProcessingResult = new TextBox();
            btn_SearchEdgeCircle = new Button();
            ((System.ComponentModel.ISupportInitialize)pb_DrawImage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ProcessingList).BeginInit();
            SuspendLayout();
            // 
            // btn_ImageLoad
            // 
            btn_ImageLoad.Location = new Point(33, 39);
            btn_ImageLoad.Name = "btn_ImageLoad";
            btn_ImageLoad.Size = new Size(113, 72);
            btn_ImageLoad.TabIndex = 0;
            btn_ImageLoad.Text = "ImageLoad";
            btn_ImageLoad.UseVisualStyleBackColor = true;
            btn_ImageLoad.Click += btn_ImageLoad_Click;
            // 
            // pb_DrawImage
            // 
            pb_DrawImage.Location = new Point(673, 62);
            pb_DrawImage.Name = "pb_DrawImage";
            pb_DrawImage.Size = new Size(984, 642);
            pb_DrawImage.SizeMode = PictureBoxSizeMode.CenterImage;
            pb_DrawImage.TabIndex = 1;
            pb_DrawImage.TabStop = false;
            pb_DrawImage.Paint += pb_DrawImage_Paint;
            // 
            // btn_SimpleBlob
            // 
            btn_SimpleBlob.Location = new Point(33, 351);
            btn_SimpleBlob.Name = "btn_SimpleBlob";
            btn_SimpleBlob.Size = new Size(113, 72);
            btn_SimpleBlob.TabIndex = 2;
            btn_SimpleBlob.Text = "Simple Blob";
            btn_SimpleBlob.UseVisualStyleBackColor = true;
            btn_SimpleBlob.Click += btn_Processing_Click;
            // 
            // btn_Binary
            // 
            btn_Binary.Location = new Point(33, 117);
            btn_Binary.Name = "btn_Binary";
            btn_Binary.Size = new Size(113, 72);
            btn_Binary.TabIndex = 3;
            btn_Binary.Text = "Binary";
            btn_Binary.UseVisualStyleBackColor = true;
            btn_Binary.Click += btn_Processing_Click;
            // 
            // ProcessingList
            // 
            ProcessingList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ProcessingList.Columns.AddRange(new DataGridViewColumn[] { Processing, Execute });
            ProcessingList.Location = new Point(421, 62);
            ProcessingList.Name = "ProcessingList";
            ProcessingList.Size = new Size(230, 633);
            ProcessingList.TabIndex = 4;
            ProcessingList.CellClick += ProcessingList_CellClick;
            ProcessingList.CellDoubleClick += ProcessingList_CellDoubleClick;
            // 
            // Processing
            // 
            Processing.HeaderText = "Processing";
            Processing.Name = "Processing";
            // 
            // Execute
            // 
            Execute.HeaderText = "Execute";
            Execute.Name = "Execute";
            Execute.Resizable = DataGridViewTriState.True;
            Execute.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // btn_ShowLoadImage
            // 
            btn_ShowLoadImage.Location = new Point(673, 10);
            btn_ShowLoadImage.Name = "btn_ShowLoadImage";
            btn_ShowLoadImage.Size = new Size(154, 46);
            btn_ShowLoadImage.TabIndex = 5;
            btn_ShowLoadImage.Text = "Show LoadImage";
            btn_ShowLoadImage.UseVisualStyleBackColor = true;
            btn_ShowLoadImage.Click += btn_ShowLoadImage_Click;
            // 
            // btn_ShowProcessingImage
            // 
            btn_ShowProcessingImage.Location = new Point(833, 10);
            btn_ShowProcessingImage.Name = "btn_ShowProcessingImage";
            btn_ShowProcessingImage.Size = new Size(154, 46);
            btn_ShowProcessingImage.TabIndex = 6;
            btn_ShowProcessingImage.Text = "Show processingImage";
            btn_ShowProcessingImage.UseVisualStyleBackColor = true;
            btn_ShowProcessingImage.Click += btn_ShowProcessingImage_Click;
            // 
            // btn_Laplacian
            // 
            btn_Laplacian.Location = new Point(288, 87);
            btn_Laplacian.Name = "btn_Laplacian";
            btn_Laplacian.Size = new Size(113, 72);
            btn_Laplacian.TabIndex = 7;
            btn_Laplacian.Text = "Laplacian";
            btn_Laplacian.UseVisualStyleBackColor = true;
            btn_Laplacian.Click += btn_Laplacian_Click;
            // 
            // lbl_Laplacian
            // 
            lbl_Laplacian.BackColor = Color.White;
            lbl_Laplacian.BorderStyle = BorderStyle.FixedSingle;
            lbl_Laplacian.FlatStyle = FlatStyle.Popup;
            lbl_Laplacian.Location = new Point(288, 61);
            lbl_Laplacian.Name = "lbl_Laplacian";
            lbl_Laplacian.Size = new Size(113, 23);
            lbl_Laplacian.TabIndex = 8;
            lbl_Laplacian.Text = "label1";
            lbl_Laplacian.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tmUpdate
            // 
            tmUpdate.Tick += tmUpdate_Tick;
            // 
            // btn_Dilation
            // 
            btn_Dilation.Location = new Point(33, 195);
            btn_Dilation.Name = "btn_Dilation";
            btn_Dilation.Size = new Size(113, 72);
            btn_Dilation.TabIndex = 9;
            btn_Dilation.Text = "Dilation";
            btn_Dilation.UseVisualStyleBackColor = true;
            btn_Dilation.Click += btn_Processing_Click;
            // 
            // btn_Erosion
            // 
            btn_Erosion.Location = new Point(33, 273);
            btn_Erosion.Name = "btn_Erosion";
            btn_Erosion.Size = new Size(113, 72);
            btn_Erosion.TabIndex = 10;
            btn_Erosion.Text = "Erosion";
            btn_Erosion.UseVisualStyleBackColor = true;
            btn_Erosion.Click += btn_Processing_Click;
            // 
            // btn_Delete
            // 
            btn_Delete.Location = new Point(421, 22);
            btn_Delete.Name = "btn_Delete";
            btn_Delete.Size = new Size(86, 34);
            btn_Delete.TabIndex = 11;
            btn_Delete.Text = "Delete";
            btn_Delete.UseVisualStyleBackColor = true;
            btn_Delete.Click += btn_Delete_Click;
            // 
            // btn_DetectCircles
            // 
            btn_DetectCircles.Location = new Point(288, 449);
            btn_DetectCircles.Name = "btn_DetectCircles";
            btn_DetectCircles.Size = new Size(113, 72);
            btn_DetectCircles.TabIndex = 12;
            btn_DetectCircles.Text = "DetectCircles";
            btn_DetectCircles.UseVisualStyleBackColor = true;
            btn_DetectCircles.Click += btn_Processing_Click;
            // 
            // btn_Reset
            // 
            btn_Reset.Location = new Point(565, 22);
            btn_Reset.Name = "btn_Reset";
            btn_Reset.Size = new Size(86, 34);
            btn_Reset.TabIndex = 13;
            btn_Reset.Text = "Reset";
            btn_Reset.UseVisualStyleBackColor = true;
            btn_Reset.Click += btn_Reset_Click;
            // 
            // btn_DetectBlob
            // 
            btn_DetectBlob.Location = new Point(152, 351);
            btn_DetectBlob.Name = "btn_DetectBlob";
            btn_DetectBlob.Size = new Size(113, 72);
            btn_DetectBlob.TabIndex = 14;
            btn_DetectBlob.Text = "Detect Blob";
            btn_DetectBlob.UseVisualStyleBackColor = true;
            btn_DetectBlob.Click += btn_Processing_Click;
            // 
            // tbProcessingResult
            // 
            tbProcessingResult.Location = new Point(33, 429);
            tbProcessingResult.Multiline = true;
            tbProcessingResult.Name = "tbProcessingResult";
            tbProcessingResult.ScrollBars = ScrollBars.Vertical;
            tbProcessingResult.Size = new Size(232, 277);
            tbProcessingResult.TabIndex = 17;
            // 
            // btn_SearchEdgeCircle
            // 
            btn_SearchEdgeCircle.Location = new Point(288, 538);
            btn_SearchEdgeCircle.Name = "btn_SearchEdgeCircle";
            btn_SearchEdgeCircle.Size = new Size(113, 72);
            btn_SearchEdgeCircle.TabIndex = 18;
            btn_SearchEdgeCircle.Text = "EmguCVSearchEdgeCircle";
            btn_SearchEdgeCircle.UseVisualStyleBackColor = true;
            btn_SearchEdgeCircle.Click += btn_Processing_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1670, 719);
            Controls.Add(btn_SearchEdgeCircle);
            Controls.Add(tbProcessingResult);
            Controls.Add(btn_DetectBlob);
            Controls.Add(btn_Reset);
            Controls.Add(btn_DetectCircles);
            Controls.Add(btn_Delete);
            Controls.Add(btn_Erosion);
            Controls.Add(btn_Dilation);
            Controls.Add(lbl_Laplacian);
            Controls.Add(btn_Laplacian);
            Controls.Add(btn_ShowProcessingImage);
            Controls.Add(btn_ShowLoadImage);
            Controls.Add(ProcessingList);
            Controls.Add(btn_Binary);
            Controls.Add(btn_SimpleBlob);
            Controls.Add(pb_DrawImage);
            Controls.Add(btn_ImageLoad);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pb_DrawImage).EndInit();
            ((System.ComponentModel.ISupportInitialize)ProcessingList).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_ImageLoad;
        private PictureBox pb_DrawImage;
        private Button btn_SimpleBlob;
        private Button btn_Binary;
        private DataGridView ProcessingList;
        private Button btn_ShowLoadImage;
        private Button btn_ShowProcessingImage;
        private Button btn_Laplacian;
        private Label lbl_Laplacian;
        private System.Windows.Forms.Timer tmUpdate;
        private Button btn_Dilation;
        private Button btn_Erosion;
        private Button btn_Delete;
        private Button btn_DetectCircles;
        private Button btn_Reset;
        private Button btn_DetectBlob;
        private TextBox tbProcessingResult;
        private DataGridViewTextBoxColumn Processing;
        private DataGridViewButtonColumn Execute;
        private Button btn_SearchEdgeCircle;
    }
}
