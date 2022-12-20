using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.ListBox;

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

            StringBuilder Models = new StringBuilder();
        void ModelOlustur(string _modelAdi)
        {

            if (Bag.State == ConnectionState.Closed)
            {
                Bag.Open();
            }
            cmd.CommandText = $@"SELECT * FROM {cmbVerTabList.SelectedValue}.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{_modelAdi}' ";
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            string Model = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //public int BANHARREFNO { get; set; }
                string turu = "";
                switch (dt.Rows[i]["DATA_TYPE"].ToString())
                {
                    case "datetime":
                        turu = "DateTime ";
                        break;
                    case "varchar":
                        turu = "string";
                        break;
                    case "int":
                        turu = "int";
                        break;
                    case "numeric":
                        turu = "decimal";
                        break;

                }

                if (turu == "string")
                {
                    Models.AppendLine($@"[StringLength({dt.Rows[i]["CHARACTER_MAXIMUM_LENGTH"]})]");
                }
                else if (turu == "decimal")
                {
                    Models.AppendLine($@"[Column(TypeName = ""decimal({dt.Rows[i]["NUMERIC_PRECISION"]},{dt.Rows[i]["NUMERIC_SCALE"]})"")]");
                }
                Model = $@"public {(turu)} " + dt.Rows[i]["COLUMN_NAME"].ToString() + " { get; set; }";
                Models.AppendLine(Model);
                Model = "";
            }
            txtModelOlusum.Text = Models.ToString();

        }



        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            ModelOlustur(listBox1.SelectedValue.ToString());
        }
        string ProjeAdi;
        string ProjeYolu;
        string ModelNamespace = "";
        private void button1_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog fileDialog = new FolderBrowserDialog();
            fileDialog.SelectedPath =ProjeYolu;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {

                textBox1.Text = fileDialog.SelectedPath;
                ProjeYolu = fileDialog.SelectedPath;
                string[] Klasorler = textBox1.Text.Split(new string[] { ProjeAdi }, StringSplitOptions.None);
                string[] Yol = Klasorler[2].Split('\\');
                    StringBuilder yolOlusur = new StringBuilder();
                for (int i = 0; i < Yol.Length; i++)
                {
                    string yolKademe =  Yol[i].Trim();
                    if (i == 0)
                    {
                        yolKademe = ProjeAdi;
                    }
                    else
                    {

                    }
                    yolKademe = yolKademe + (i + 1 != Yol.Length ? "." : "");
                    yolOlusur.Append(yolKademe);
                    ModelNamespace=yolOlusur.ToString();
                }

            }
        }
        async Task ModelYazAsync(string _model,string _modelAdi)
        {
            StringBuilder build = new StringBuilder();
            build.Append("using Microsoft.EntityFrameworkCore.Metadata.Internal;\r\nusing System;\r\nusing System.ComponentModel.DataAnnotations;\r\nusing System.ComponentModel.DataAnnotations.Schema;\r\n\r\nnamespace " + $"{ModelNamespace}\r\n" + "{   public class " + $"{_modelAdi}" + "\r\n    {\r\n");
            build.Append(_model);
            build.Append("\r\n    }\r\n}");
            StreamWriter stream = new StreamWriter(textBox1.Text + $@"\{_modelAdi}.cs");
            stream.Write(build.ToString());
            await stream.FlushAsync();
            stream.Close();

        }
        private async void button2_Click(object sender, EventArgs e)
        {
            Models.Clear();
            if (string.IsNullOrWhiteSpace(ModelNamespace) || string.IsNullOrWhiteSpace(ProjeYolu))
            {
                return;
            }
            if (checkBox1.Checked)
            {
                foreach (var item in listBox1.SelectedItems)
                {

                    //MessageBox.Show(listBox1.GetItemText(item).ToString());
                    listBox1.SelectedItem = item;
                    ModelOlustur(listBox1.GetItemText(item));
                    await ModelYazAsync(Models.ToString(), listBox1.GetItemText(item).ToString());
                }



            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                ProjeYolu = fileDialog.FileName.Replace(fileDialog.SafeFileName,"");
                FileStream fs = new FileStream(fileDialog.FileName, FileMode.Open);

                StreamReader sr = new StreamReader(fs);
                StringBuilder Yazi = new StringBuilder();
                while (!sr.EndOfStream)
                {
                    string Okunan = sr.ReadLine();
                    if (Okunan.StartsWith("namespace "))
                    {
                        ProjeAdi = (Okunan.Replace("namespace ", ""));
                        break;
                    }
                }
                fs.Close();
                sr.Close();
                txtprojeAdi.Text = ProjeAdi;
            }


        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            listBox1.SelectedItems.Clear();
            if (checkBox1.Checked)
            {
            listBox1.SelectionMode = SelectionMode.MultiExtended;
            }
            else
            {
                listBox1.SelectionMode = SelectionMode.One;

            }

        }
    }
}
