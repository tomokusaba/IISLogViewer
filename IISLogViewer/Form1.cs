using Microsoft.VisualBasic.FileIO;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IISLogViewer
{
    public partial class Form1 : Form
    {

        DataTable dt = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            dt.Clear();
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach(string file in files)
            {
                Read(file);
            }

            dataGridView1.DataSource = dt;
        }

        /// <summary>
        /// ログファイルを読み込む
        /// </summary>
        /// <param name="path">ファイルパス</param>
        private void Read(string path)
        {
            using (StreamReader sr = new StreamReader(@path, Encoding.GetEncoding("Shift_JIS")))
            {
                // 時刻部分にスペースが入っているので
                // 事前にダブルコーテーションに置換しておく
                string log = sr.ReadToEnd();
                log = log.Replace('[', '"');
                log = log.Replace(']', '"');
                byte[] data = System.Text.Encoding.GetEncoding("Shift_JIS").GetBytes(log);

                MemoryStream st = new MemoryStream(data);

                using (TextFieldParser parser = new TextFieldParser(st))
                {

                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(" ");
                    parser.CommentTokens = new String[] { "#" };

                    string[] line = new string[7];
                    while (!parser.EndOfData)
                    {
                        try
                        {
                            line = parser.ReadFields();
                        }
                        catch 
                        {
                            // 想定外のデータがあった場合読み飛ばす 
                        }

                        EncLine(line);
                    }
                }
            }
        }

        /// <summary>
        /// 1行分のデータをグリッドビューに表示する
        /// </summary>
        /// <param name="line"></param>
        private void EncLine(string[] line)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridView1);

            DataRow dr = dt.NewRow();

            dr[0] = line[0];
            dr[1] = line[1];
            dr[2] = line[2];
            dr[3] = line[3];
            dr[4] = line[4];
            dr[5] = line[5];
            dr[6] = line[6];


            dt.Rows.Add(dr);
        }

        /// <summary>
        /// グリッドビューの列定義
        /// </summary>
        private void CrerateDateGrid()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();

            dt = new DataTable("table");

            dt.Columns.Add("アクセス元");
            dt.Columns.Add("Cユーザ名");
            dt.Columns.Add("認証ユーザ名");
            dt.Columns.Add("時刻");
            dt.Columns.Add("リソース");
            dt.Columns.Add("ステータス");
            dt.Columns.Add("転送量");

            comboBox1.Items.Add("アクセス元");
            comboBox1.Items.Add("Cユーザ名");
            comboBox1.Items.Add("認証ユーザ名");
            comboBox1.Items.Add("時刻");
            comboBox1.Items.Add("リソース");
            comboBox1.Items.Add("ステータス");
            comboBox1.Items.Add("転送量");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CrerateDateGrid();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataRow[] rows = dt.AsEnumerable().Where(row => row.Field<string>(comboBox1.SelectedItem.ToString()) == textBox1.Text).ToArray();

            if (rows.Length == 0)
            {
                MessageBox.Show("該当するデータがありません");
                return;
            }

            dt = rows.CopyToDataTable();
            dataGridView1.DataSource = dt;
        }
    }
}
