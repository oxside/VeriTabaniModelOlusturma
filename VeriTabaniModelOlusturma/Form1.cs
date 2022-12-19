using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VeriTabaniModelOlusturma
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        SqlConnection Bag;
        SqlCommand cmd = new SqlCommand();
        private void button1_Click(object sender, EventArgs e)
        {
            VeriTabaniListesi();
        }
        void VeriTabaniListesi()
        {
            Bag = BagOlustur();
            if (BaglantiTest(Bag))
            {
                if (Bag.State == ConnectionState.Open)
                {
                    Bag.Close();
                }
                Bag.Open();
                cmd.Connection = Bag;
                cmd.CommandText = "SELECT * FROM sys.databases ";
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                cmbVerTabList.DataSource = dt;

            }

        }

        bool BaglantiTest(SqlConnection bag)
        {
            try
            {
                bag.Open();
                bag.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        SqlConnection BagOlustur()
        {

            builder.DataSource = txtServer.Text;
            builder.Password = txtSifre.Text;
            builder.UserID = txtUser.Text;
            return new SqlConnection(builder.ConnectionString);

        }

        private void cmbVerTabList_SelectedValueChanged(object sender, EventArgs e)
        {
            if (Bag.State == ConnectionState.Closed)
            {
                Bag.Open();
            }
            cmd.CommandText = $@"SELECT * FROM {cmbVerTabList.SelectedValue}.INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME";
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            listBox1.DataSource = dt;

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (Bag.State == ConnectionState.Closed)
            {
                Bag.Open();
            }
            cmd.CommandText = $@"SELECT * FROM {cmbVerTabList.SelectedValue}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{listBox1.SelectedValue}' ";
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            string Model = "";
            StringBuilder Models = new StringBuilder();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //public int BANHARREFNO { get; set; }
                string turu="";
                switch (dt.Rows[i]["DATA_TYPE"].ToString())
                {
                    case "datetime":
                        turu = "DateTime ";
                        break;
                    case "varchar":
                        turu = "string ";
                        break;
                    case "int":
                        turu = "int ";
                        break;
                    case "numeric":
                        turu = "decimal ";
                        break;
                   
                }
             

        Model = $@"public {(turu)}"+ dt.Rows[i]["COLUMN_NAME"].ToString() + " { get; set; }";
                Models.AppendLine(Model);
                Model = "";
            }
            txtModelOlusum.Text = Models.ToString();

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog fileDialog = new FolderBrowserDialog();
            fileDialog.SelectedPath = textBox1.Text;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text=fileDialog.SelectedPath;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            StringBuilder build = new StringBuilder();
            build.Append("using System;\r\nusing System.ComponentModel.DataAnnotations;\r\n\r\nnamespace EtaWeb.Models.Banka\r\n{\r\n    public class "+$"{listBox1.SelectedValue}"+ "\r\n    {\r\n");
            build.Append(txtModelOlusum.Text);
            build.Append("\r\n    }\r\n}");
            StreamWriter stream = new StreamWriter(textBox1.Text+$@"\{listBox1.SelectedValue}.cs");            
            stream.Write(build.ToString());
           await stream.FlushAsync();
            stream.Close();
        }
    }
}
