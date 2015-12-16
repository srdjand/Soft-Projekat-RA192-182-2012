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

namespace Face_Detection
{
    public partial class Form1 : Form
    {

        //members and variables
        HaarCascade Haar;
        Capture Webcam = null;
        Image<Bgr, Byte> Original;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            Haar = new HaarCascade("haarcascade_frontalface_default.xml");

            try
            {
                Webcam = new Capture();
            }
            catch (NullReferenceException except) {
                MessageBox.Show(except.Message, "Warning!",
    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            //Original = Webcam.QueryFrame();
            //imageBox1.Image = Original;

            Application.Idle += processFrameAndUpdateGUI;
            
        }

        void processFrameAndUpdateGUI(object sender, EventArgs arg) 
        {
            Original = Webcam.QueryFrame();

            if (Original != null) {
                Image<Gray, byte> Grayframe = Original.Convert<Gray, byte>();
                var faces = Grayframe.DetectHaarCascade(Haar, 1.2, 4, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(imageBox1.Height / 5, imageBox1.Width / 5))[0];

                foreach (var face in faces) {
                    Original.Draw(face.rect, new Bgr(Color.Yellow), 2);
                }
            }

            imageBox1.Image = Original;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Webcam != null) {
                Webcam.Dispose();
            } 
        }
    }
}
