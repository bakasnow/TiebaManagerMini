using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TiebaManagerMini
{
    public partial class WenBenGuoLv : Form
    {
        public WenBenGuoLv()
        {
            InitializeComponent();
        }

        private void wenbenGuolv_Load(object sender, EventArgs e)
        {
            Text = "文本过滤";
        }

        /// <summary>
        /// 添加 按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show(text: " [替换前]不得为空", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            if (tianjia(textBox1.Text, textBox2.Text))
            {
                Close();
            }
            else
            {
                MessageBox.Show(text: " 添加失败：可能是重复了", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 批量添加 按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            string[] liebiao = textBox3.Text.Split(Environment.NewLine.ToCharArray());

            foreach (string str in liebiao)
            {
                string[] fenge = str.Split(new string[] { ">>" }, StringSplitOptions.None);
                if (fenge.Length == 2)
                {
                    if (fenge[0] != "")
                    {
                        tianjia(fenge[0], fenge[1]);
                    }
                }
            }

            Close();
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="tihuanqian">替换前</param>
        /// <param name="tihuanhou">替换后</param>
        /// <returns></returns>
        public bool tianjia(string tihuanqian, string tihuanhou)
        {
            if (Convert.ToString(Form1.db_tmm.GetDataResult("select 替换前 from 文本过滤 where 替换前='" + tihuanqian + "'")) != "")
            {
                return false;
            }

            if (Form1.db_tmm.DoCommand("insert into 文本过滤(替换前,替换后) values('" + tihuanqian + "','" + tihuanhou + "')") > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
