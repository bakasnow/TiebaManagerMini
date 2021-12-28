using BaiduLogin;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TiebaManagerMini.WinForm
{
    public partial class TiebaZhangHaoGuanLiForm : Form
    {
        public TiebaZhangHaoGuanLiForm()
        {
            InitializeComponent();
        }

        public IniHelper ini = null;

        /// <summary>
        /// 主窗口 启动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TiebaZhangHaoGuanLiForm_Load(object sender, EventArgs e)
        {
            Text = "账号管理";

            JiaZaiZhangHaoLieBiao();
        }

        /// <summary>
        /// 按钮 添加账号 被单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_zhangHao_tianJiaZhangHao_Click(object sender, EventArgs e)
        {
            BaiduLoginForm baiduLoginForm = new BaiduLoginForm();
            baiduLoginForm.ShowDialog();
            Config.Cookie = baiduLoginForm.Cookie;

            ZhangHaoYanZheng();
        }

        /// <summary>
        /// 按钮 删除账号 被单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_zhangHao_shanChuZhangHao_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 列表框 账号列表 选择框被单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_zhangHao_zhangHaoLieBiao_ItemChecked(object sender, ItemCheckedEventArgs e)
        {

        }

        /// <summary>
        /// 加载账号列表
        /// </summary>
        private void JiaZaiZhangHaoLieBiao()
        {
            List<string> zhangHaoLieBiao = ini.GetSectionList();

            listView_zhangHao_zhangHaoLieBiao.Items.Clear();

            listView_zhangHao_zhangHaoLieBiao.BeginUpdate();
            foreach (var zhangHao in zhangHaoLieBiao)
            {
                bool isZaiXian = ini.ReadValue(zhangHao, "是否在线") == "1";

                ListViewItem listViewItem = new ListViewItem
                {
                    Checked = isZaiXian,
                    Text = isZaiXian ? "是" : "否"
                };

                listViewItem.SubItems.Add(zhangHao);

                listView_zhangHao_zhangHaoLieBiao.Items.Add(listViewItem);
            }
            listView_zhangHao_zhangHaoLieBiao.EndUpdate();
        }

        private void button_zhangHao_jianChaDiaoXian_Click(object sender, EventArgs e)
        {

        }
    }
}
