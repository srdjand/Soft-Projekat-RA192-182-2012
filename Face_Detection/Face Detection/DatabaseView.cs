using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;

namespace Face_Detection
{
    public partial class DatabaseView : Form
    {

        Database dB;

        public DatabaseView(Database db2)
        {
            InitializeComponent();
            this.dB = db2;
        }

        public DatabaseView()
        {
            InitializeComponent();
        }

        private void DatabaseView_Load(object sender, EventArgs e)
        {
         
            if (dB.LocalDataTable.Rows.Count != 0) {
                for (int i = 0; i < dB.LocalDataTable.Rows.Count; i++) {
                    dataGridView1.Rows.Add(new object[] {dB.LocalDataTable.Rows[i]["FaceID"], dB.LocalDataTable.Rows[i]["FaceName"]});
                }
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            int sr = dataGridView1.SelectedRows[0].Index;
            Image faceImage;
            byte[] ImagefromLDTBytes = (byte[])dB.LocalDataTable.Rows[sr]["FaceImage"];
            MemoryStream stream = new MemoryStream(ImagefromLDTBytes);
            faceImage = Image.FromStream(stream);
            pictureBox1.Image = faceImage;
        }

    }
}
