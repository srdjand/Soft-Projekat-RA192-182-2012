using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;
using System.Data.OleDb;
using System.IO;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;

namespace Face_Detection
{
    public class Database
    {
        //VARIABLE

        //konekcija sa bazon
        public OleDbConnection DBConnection = new OleDbConnection();

        //Adapter za dobijanje vrednosti iz tabele baze podataka
        public OleDbDataAdapter DataAdapter;

        //Lokalna tabela za cuvanje vrednosti iz baze podataka
        public DataTable LocalDataTable = new DataTable();

        public int rowPos = 0;     //index prilikom cuvanja podataka u bazu na poslednju lokaciju
        public int rowNumber = 0;  //index prilikom citanja tabele

        public Image<Gray, byte>[] facesFromDb;
        public String[] facesNamesFromDb;
        


        //METODE
        public void ConnectToDatabase() {

            DBConnection.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=FacesDatabase.mdb";

            try
            {
                DBConnection.Open();
                MessageBox.Show("Connection Open!");
            }
            catch (Exception e) {
                MessageBox.Show("Can not open connection: ", e.Message);
            }

            DataAdapter = new OleDbDataAdapter("Select * from FaceData", DBConnection);
            DataAdapter.Fill(LocalDataTable);

            if (LocalDataTable.Rows.Count != 0) {
                rowPos = LocalDataTable.Rows.Count;
            }

        }

        public void RefreshDBConnection() {

            if (DBConnection.State.Equals(ConnectionState.Open))
            {
                DBConnection.Close();
                LocalDataTable.Clear();
                ConnectToDatabase();
                NamesAndPicturesFromDB();
            }
            else {
                ConnectToDatabase();
                NamesAndPicturesFromDB();
            }

        }

        public void StoreDataToDb(byte[] ImageAsBytes) {

            if (DBConnection.State.Equals(ConnectionState.Closed))
                DBConnection.Open();

            try
            {
                OleDbCommand insert = new OleDbCommand("Insert INTO FaceData (FaceID, FaceName, FaceImage) VALUES('" + rowPos + "','" + Form1.textBox1.Text.ToString() + "',@MyImage)", DBConnection);
                OleDbParameter imageParameter = insert.Parameters.AddWithValue("@Image", SqlDbType.Binary);
                imageParameter.Value = ImageAsBytes;
                imageParameter.Size = ImageAsBytes.Length;
                int rowsAff = LocalDataTable.Rows.Count + 1;
                insert.ExecuteNonQuery();
                MessageBox.Show("Data stored in " + rowsAff.ToString() + " Row");
                rowPos++;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                MessageBox.Show(e.StackTrace.ToString());
            }
            finally
            {
                RefreshDBConnection();
            }
        
        }


        public void NamesAndPicturesFromDB() {

            facesFromDb = new Image<Gray, byte>[GetTableSize()];
            facesNamesFromDb = new String[GetTableSize()];
            Image ImagefromLDT;
            int i = LocalDataTable.Rows.Count;
            if (i != 0) {
                for (int j = 0; j < i; j++) {
                    byte[] ImagefromLDTBytes = (byte[])LocalDataTable.Rows[j]["FaceImage"];
                    MemoryStream stream = new MemoryStream(ImagefromLDTBytes);
                    ImagefromLDT = Image.FromStream(stream);
                    Bitmap obc = (Bitmap) ImagefromLDT;
                    facesFromDb[j] = new Image<Gray, byte> (obc);
                    facesNamesFromDb[j] = LocalDataTable.Rows[j]["FaceName"].ToString();
                }
            }
        }

        public int GetTableSize() {

            return LocalDataTable.Rows.Count;
        }

    }
}
