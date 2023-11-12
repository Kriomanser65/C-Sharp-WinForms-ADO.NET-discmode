using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoInteresting
{
    public partial class Form1 : Form
    {
        string connectionString = "Data Source=DESKTOP-4SK39GD;Initial Catalog=Imagedtbs;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;";
        public Form1()
        {
            InitializeComponent();
            CreateDb();
            LoadImages();
        }
        private void CreateDb()
        {
            SqlConnection dbConn=new SqlConnection(connectionString);
            dbConn.Open();
            string sql = "create table if not exists Images (Id integer primary key, Name text, Image blob)";
            SqlCommand command = new SqlCommand(sql, dbConn);
            command.ExecuteNonQuery();
            dbConn.Close();
        }
        private void LoadImages()
        {
            comboBox1.DataSource = LoadFromDb();
            if (comboBox1.Items.Count >0 ) 
            {
                byte[] imageData = (byte[])comboBox1.Items[0];
                pictureBox1.Image = ByteArrayToImage(imageData);
            }
        }
        private List<object> LoadFromDb()
        {
            List<object> images = new List<object>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "select * from Images";
                using(SqlCommand cmd = new SqlCommand(sqlLength, conn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            images.Add(new
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                ImageData = reader.GetValue(2)
                            });
                        }
                    }
                }
            }
            return images;
        }
        private Image ByteArrayToImage(byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                return Image.FromStream(ms);
            }
        }
        private void SaveToDb(byte[] imageData, string name)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO Images (Name, Image) VALUES (@Name, @Image)";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Image", imageData);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Image image = Image.FromFile(dialog.FileName);
                ImageConverter converter = new ImageConverter();
                byte[] imageData = (byte[])converter.ConvertTo(image, typeof(byte[]));

                SaveToDb(imageData, Path.GetFileName(dialog.FileName));
                LoadImages();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte[] imageData = (byte[])comboBox1.SelectedItem;
            pictureBox1.Image = ByteArrayToImage(imageData);
        }
    }
}
