using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System.IO;

namespace Face_Detection
{
    public partial class Form1 : Form
    {

        //members and variables
        HaarCascade Haar;
        private Capture Webcam = null;
        Image<Bgr, Byte> Original;
        Image<Bgr, Byte> Captured;
        bool CaptureClicked = false;

        Bitmap[] extractedFaces;
        int faceNumber = 0;

        Database FacesDatabase = new Database();
        Image<Gray, byte> result = null;
        String name = null;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            FacesDatabase.ConnectToDatabase();          //konektovanje sa bazom
            FacesDatabase.NamesAndPicturesFromDB();     //ucitavanje podataka iz baze

            Haar = new HaarCascade("haarcascade_frontalface_default.xml");

            try
            {
                Webcam = new Capture();
            }
            catch (NullReferenceException except)
            {
                MessageBox.Show(except.Message, "Warning!",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            btnNext.Enabled = false;
            btnPrevious.Enabled = false;

            //Original = Webcam.QueryFrame();
            //imageBox1.Image = Original;

            Application.Idle += processFrameAndUpdateGUI;
            
        }

        void processFrameAndUpdateGUI(object sender, EventArgs arg) 
        {
            Original = Webcam.QueryFrame();

            if (Original != null)
            {
                Image<Gray, byte> Grayframe = Original.Convert<Gray, byte>();
                var faces = Grayframe.DetectHaarCascade(Haar, 1.2, 4, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(imageBox1.Height/5, imageBox1.Width/5))[0];

                if (CaptureClicked)
                {
                    CapturedImageFaceDetect();
                }

                foreach (var face in faces)
                {
                    result = Original.Copy(face.rect).Convert<Gray, byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                    Original.Draw(face.rect, new Bgr(Color.Yellow), 2);

                    MCvTermCriteria termCrit = new MCvTermCriteria(1000, 0.001);

                    //label2.Text = FacesDatabase.facesNamesFromDb.ToString();
                    EigenObjectRecognizer recongnizer = new EigenObjectRecognizer(FacesDatabase.facesFromDb.ToArray(), FacesDatabase.facesNamesFromDb.ToArray(), 2500, ref termCrit);


                    try
                    {
                        name = recongnizer.Recognize(result).Label;
                        Original.Draw(name, ref font, new Point(face.rect.X - 2, face.rect.Y - 2), new Bgr(Color.LightGreen));
                        //pictureBox1.Image = recongnizer.Recognize
                    }
                    catch (Exception e)
                    {
                        //Exception ako ne prepozna lice
                    }
                }

                imageBox1.Image = Original;

            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Webcam != null) {
                Webcam.Dispose();
            } 
        }

        private void Capture_Click(object sender, EventArgs e)
        {
            CaptureClicked = true;
        }

        public void CapturedImageFaceDetect() {

            Captured = Original;
            //Captured = Captured.Resize(imageBox2.Height, imageBox2.Width, INTER.CV_INTER_LINEAR);

            CaptureClicked = false;

            if (Captured != null)
            {
                Image<Gray, byte> capturedGrayFrame = Captured.Convert<Gray, byte>();
                var capturedFaces = capturedGrayFrame.DetectHaarCascade(Haar, 1.2, 4, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(imageBox2.Height/5, imageBox2.Width/5))[0];

                if (capturedFaces.Length > 0)
                {
                    Bitmap bitmapInput = capturedGrayFrame.ToBitmap();
                    Bitmap extractedFace;
                    Graphics faceCanvas;
                    extractedFaces = new Bitmap[capturedFaces.Length];

                    if (capturedFaces.Length > 1) {
                        btnNext.Enabled = true;
                        btnPrevious.Enabled = true;
                    }

                    foreach (var face in capturedFaces)
                    {
                        
                        result = Captured.Copy(face.rect).Convert<Gray, byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                        Captured.Draw(face.rect, new Bgr(Color.Yellow), 2);

                        MCvTermCriteria termCrit = new MCvTermCriteria(1000, 0.001);

                        //label2.Text = FacesDatabase.facesNamesFromDb.ToString();
                        

                        try
                        {
                            EigenObjectRecognizer recongnizer = new EigenObjectRecognizer(FacesDatabase.facesFromDb.ToArray(), FacesDatabase.facesNamesFromDb.ToArray(), 2500, ref termCrit);
                            name = recongnizer.Recognize(result).Label;
                            Captured.Draw(name, ref font, new Point(face.rect.X - 2, face.rect.Y - 2), new Bgr(Color.LightGreen));
                        }
                        catch (Exception e) {
                            //MessageBox.Show("Ne postoji u bazi, greska: " + e.ToString());

                            //size of an empty box
                            extractedFace = new Bitmap(face.rect.Width, face.rect.Height);

                            //seting empty face image as canvas for painting
                            faceCanvas = Graphics.FromImage(extractedFace);

                            faceCanvas.DrawImage(bitmapInput, 0, 0, face.rect, GraphicsUnit.Pixel);

                            extractedFace = new Bitmap(extractedFace, new Size(100, 100));

                            extractedFaces[faceNumber] = extractedFace;
                            faceNumber++;

                        }

                        //size of an empty box
                       /* extractedFace = new Bitmap(face.rect.Width, face.rect.Height);

                        //seting empty face image as canvas for painting
                        faceCanvas = Graphics.FromImage(extractedFace);

                        faceCanvas.DrawImage(bitmapInput, 0, 0, face.rect, GraphicsUnit.Pixel);

                        extractedFace = new Bitmap(extractedFace, new Size(100, 100));

                        extractedFaces[faceNumber] = extractedFace;
                        faceNumber++;  */
                    }
                    faceNumber = 0;
                    pbExtractedFace.Image = extractedFaces[faceNumber];
                    if (extractedFaces.Length == 1)
                    {
                        btnNext.Enabled = false;
                        btnPrevious.Enabled = false;
                    }

                    imageBox2.Image = Captured;
                }
            }

        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (faceNumber + 1 <= extractedFaces.Length-1)
            {
                faceNumber++;
                pbExtractedFace.Image = extractedFaces[faceNumber];
                if (faceNumber == extractedFaces.Length - 1)
                    btnNext.Enabled = false;
                if (faceNumber != 0)
                    btnPrevious.Enabled = true;
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (faceNumber - 1 >= 0)
            {
                faceNumber--;
                pbExtractedFace.Image = extractedFaces[faceNumber];
                if (faceNumber == 0)
                    btnPrevious.Enabled = false;
                if (faceNumber != extractedFaces.Length - 1)
                    btnNext.Enabled = true;
            }
        }

        private byte[] ConvertImageToBytes(Image InputImage) {

            Bitmap BmpImage = new Bitmap(InputImage);   //konvertuje pictureBox u bitmap format
            MemoryStream mStream = new MemoryStream();
            BmpImage.Save(mStream, System.Drawing.Imaging.ImageFormat.Jpeg); //konvertuje bitmap u mstream u odredjenom formatu
            byte[] ImageAsBytes = mStream.ToArray();
            return ImageAsBytes;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            FacesDatabase.StoreDataToDb(ConvertImageToBytes(pbExtractedFace.Image));
            extractedFaces = extractedFaces.Where((source, index) => index != faceNumber).ToArray();
            if (extractedFaces.Length != 0)
            {
                pbExtractedFace.Image = extractedFaces[0];
                faceNumber = 0;
                btnPrevious.Enabled = false;
                textBox1.Text = "";
                if (extractedFaces.Length == 1)
                    btnNext.Enabled = false;
            }
            else
            {
                pbExtractedFace.Image = null;
                textBox1.Text = "";
                btnNext.Enabled = false;
                btnPrevious.Enabled = false;

            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FacesDatabase.NamesAndPicturesFromDB();
            DatabaseView databaseViewForm = new DatabaseView(FacesDatabase);
            databaseViewForm.ShowDialog();
        }


    }
}
