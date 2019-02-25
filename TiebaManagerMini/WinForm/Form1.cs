using System;
using System.Data;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using BakaSnowTool;
using BakaSnowTool.Http;
using TiebaLib;
using System.Diagnostics;

namespace TiebaManagerMini
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Access数据库
        /// </summary>
        public static Access db_tmm = new Access(Application.StartupPath + @"\db_tmm.mdb");

        /// <summary>
        /// 版本验证
        /// </summary>
        /// <returns></returns>
        private bool Version()
        {
            string html, v;
            while (true)
            {
                html = Http.Get("http://www.bakasnow.com/version.php?n=" + QuanJu.Vname);
                v = BST.JieQuWenBen(html, "<version>", "</version>");
                if (string.IsNullOrEmpty(v))
                {
                    if (MessageBox.Show(text: "版本获取失败，可能是网络异常，点击\"取消\"跳过验证。", caption: "笨蛋雪说：", buttons: MessageBoxButtons.RetryCancel, icon: MessageBoxIcon.Asterisk) == DialogResult.Cancel)
                    {
                        return true;
                    }
                }
                else
                {
                    //获取版本号成功
                    break;
                }
            }

            if (v != QuanJu.Version)
            {
                string msg =
                    "发现新版本，请至群共享下载最新版\r\n" +
                    "当前版本：" + QuanJu.Version + "\r\n" +
                    "最新版本：" + v + "\r\n\r\n" +
                    "是否立即加群？";
                if (MessageBox.Show(text: msg, caption: "笨蛋雪说：", buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    string target = BST.JieQuWenBen(html, "<link>", "</link>");
                    if (!string.IsNullOrEmpty(target))
                    {
                        Process.Start("iexplore.exe", target);
                    }

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 窗口创建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //判断数据库是否存在
            if (!File.Exists(Application.StartupPath + @"\db_tmm.mdb"))
            {
                MessageBox.Show(text: " 数据库丢失，请重新下载", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                Dispose();
            }

            //初始化参数
            db_tmm.Open();

            //第一次启动检查默认设置
            if (db_tmm.GetDataTable("select id from 基本设置 where id='配置1'").Rows.Count < 1)
            {
                if (db_tmm.DoCommand("insert into 基本设置(id,Cookie,贴吧名,扫描间隔,删帖间隔,跳过置顶,跳过精品,跳过楼中楼,只输出删帖日志,开启文本过滤,启用标题等级,标题等级,启用内容等级,内容等级,启用主题等级墙,主题等级墙,启用回复等级墙,回复等级墙,等级墙开始时间,等级墙结束时间)" +
                    " values('配置1','','','8','3','1','1','1','0','0','0','10','0','10','0','2','0','2','00:00','23:59')") <= 0)
                {
                    MessageBox.Show(text: " 数据库初始化失败，请联系作者！", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                    Dispose();
                }
            }

            //版本验证
            if (!Version())
            {
                Dispose();
            }

            Text = "贴吧管理器迷你版";
            label_guanYu_jianJie.Text = label_guanYu_jianJie.Text.Replace("{version}", QuanJu.Version);

            CheckForIllegalCrossThreadCalls = false;
            button_sheZhi_tingZhi.Enabled = false;
            textBox_liuLanQi_tieZiYuLan.ReadOnly = true;

            Read();//读取配置文件
            ZhanghaoYanzheng(true);//账号验证
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!QuanJu.Stop)
            {
                if (MessageBox.Show(text: " 扫描还在进行中，确定要退出嘛？", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OKCancel, icon: MessageBoxIcon.Exclamation) == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            Save();//保存
            db_tmm.Close();

            e.Cancel = false;
        }

        /// <summary>
        /// 读取参数
        /// </summary>
        private void Read()
        {
            QuanJu.Cookie = Convert.ToString(db_tmm.GetDataResult("select Cookie from 基本设置 where id='配置1'"));
            textBox_sheZhi_tiebaName.Text = Convert.ToString(db_tmm.GetDataResult("select 贴吧名 from 基本设置 where id='配置1'"));
            textBox_sheZhi_saoMiaoJianGe.Text = Convert.ToString(db_tmm.GetDataResult("select 扫描间隔 from 基本设置 where id='配置1'"));
            textBox_sheZhi_shanTieJianGe.Text = Convert.ToString(db_tmm.GetDataResult("select 删帖间隔 from 基本设置 where id='配置1'"));
            textBox_guanJianCi_biaoTiDengJi.Text = Convert.ToString(db_tmm.GetDataResult("select 标题等级 from 基本设置 where id='配置1'"));
            textBox_guanJianCi_neiRongDengJi.Text = Convert.ToString(db_tmm.GetDataResult("select 内容等级 from 基本设置 where id='配置1'"));
            textBox_sheZhi_zhuTiDengJiQiang.Text = Convert.ToString(db_tmm.GetDataResult("select 主题等级墙 from 基本设置 where id='配置1'"));
            textBox_sheZhi_huiFuDengJiQiang.Text = Convert.ToString(db_tmm.GetDataResult("select 回复等级墙 from 基本设置 where id='配置1'"));
            dateTimePicker_sheZhi_kaiShiShiJian.Text = Convert.ToString(db_tmm.GetDataResult("select 等级墙开始时间 from 基本设置 where id='配置1'"));
            dateTimePicker_sheZhi_tingZhiShiJian.Text = Convert.ToString(db_tmm.GetDataResult("select 等级墙结束时间 from 基本设置 where id='配置1'"));

            checkBox_sheZhi_buSaoMiaoZhiDingTie.Checked = Convert.ToString(db_tmm.GetDataResult("select 跳过置顶 from 基本设置 where id='配置1'")) == "1" ? true : false;
            checkBox_sheZhi_buSaoMiaoJingPinTie.Checked = Convert.ToString(db_tmm.GetDataResult("select 跳过精品 from 基本设置 where id='配置1'")) == "1" ? true : false;
            checkBox_sheZhi_buSaoMiaoLzl.Checked = Convert.ToString(db_tmm.GetDataResult("select 跳过楼中楼 from 基本设置 where id='配置1'")) == "1" ? true : false;
            checkBox_gaoJi_zhiShuChuShanTieRiZhi.Checked = Convert.ToString(db_tmm.GetDataResult("select 只输出删帖日志 from 基本设置 where id='配置1'")) == "1" ? true : false;
            checkBox_wbgl_kaiQiWenBenGuoLv.Checked = Convert.ToString(db_tmm.GetDataResult("select 开启文本过滤 from 基本设置 where id='配置1'")) == "1" ? true : false;
            checkBox_guanJianCi_biaoTiDengJi.Checked = Convert.ToString(db_tmm.GetDataResult("select 启用标题等级 from 基本设置 where id='配置1'")) == "1" ? true : false;
            checkBox_guanJianCi_neiRongDengJi.Checked = Convert.ToString(db_tmm.GetDataResult("select 启用内容等级 from 基本设置 where id='配置1'")) == "1" ? true : false;
            checkBox_sheZhi_zhuTiDengJiQiang.Checked = Convert.ToString(db_tmm.GetDataResult("select 启用主题等级墙 from 基本设置 where id='配置1'")) == "1" ? true : false;
            checkBox__sheZhi_huiFuDengJiQiang.Checked = Convert.ToString(db_tmm.GetDataResult("select 启用回复等级墙 from 基本设置 where id='配置1'")) == "1" ? true : false;

            DataTable dt = new DataTable();
            dt = db_tmm.GetDataTable("select 关键词 from 标题关键词");
            foreach (DataRow dr in dt.Rows)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(dr["关键词"])))
                {
                    textBox_guanJianCi_biaoTiGuanJianCi.Text += dr["关键词"].ToString() + "\r\n";
                }
            }

            dt = db_tmm.GetDataTable("select 关键词 from 内容关键词");
            foreach (DataRow dr in dt.Rows)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(dr["关键词"])))
                {
                    textBox_guanJianCi_neiRongGuanJianCi.Text += dr["关键词"].ToString() + "\r\n";
                }
            }

            dt = db_tmm.GetDataTable("select 用户名 from 用户黑名单");
            foreach (DataRow dr in dt.Rows)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(dr["用户名"])))
                {
                    textBox_mingDan_heiMingDan.Text += dr["用户名"].ToString() + "\r\n";
                }
            }

            dt = db_tmm.GetDataTable("select 用户名 from 用户白名单");
            foreach (DataRow dr in dt.Rows)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(dr["用户名"])))
                {
                    textBox_mingDan_baiMingDan.Text += dr["用户名"].ToString() + "\r\n";
                }
            }
            dt.Dispose();

            //载入文本过滤列表
            GengxinWenbenGuolv();
        }

        /// <summary>
        /// 保存参数
        /// </summary>
        private void Save()
        {
            string temp = string.Empty;
            db_tmm.DoCommand("update 基本设置 set Cookie='" + QuanJu.Cookie + "' where id='配置1'");
            db_tmm.DoCommand("update 基本设置 set 贴吧名='" + textBox_sheZhi_tiebaName.Text + "' where id='配置1'");
            db_tmm.DoCommand("update 基本设置 set 扫描间隔='" + textBox_sheZhi_saoMiaoJianGe.Text + "' where id='配置1'");
            db_tmm.DoCommand("update 基本设置 set 删帖间隔='" + textBox_sheZhi_shanTieJianGe.Text + "' where id='配置1'");
            db_tmm.DoCommand("update 基本设置 set 标题等级='" + textBox_guanJianCi_biaoTiDengJi.Text + "' where id='配置1'");
            db_tmm.DoCommand("update 基本设置 set 内容等级='" + textBox_guanJianCi_neiRongDengJi.Text + "' where id='配置1'");
            db_tmm.DoCommand("update 基本设置 set 主题等级墙='" + textBox_sheZhi_zhuTiDengJiQiang.Text + "' where id='配置1'");
            db_tmm.DoCommand("update 基本设置 set 回复等级墙='" + textBox_sheZhi_huiFuDengJiQiang.Text + "' where id='配置1'");
            db_tmm.DoCommand("update 基本设置 set 等级墙开始时间='" + dateTimePicker_sheZhi_kaiShiShiJian.Text + "' where id='配置1'");
            db_tmm.DoCommand("update 基本设置 set 等级墙结束时间='" + dateTimePicker_sheZhi_tingZhiShiJian.Text + "' where id='配置1'");

            temp = checkBox_sheZhi_buSaoMiaoZhiDingTie.Checked ? "1" : "0";
            db_tmm.DoCommand("update 基本设置 set 跳过置顶='" + temp + "' where id='配置1'");

            temp = checkBox_sheZhi_buSaoMiaoJingPinTie.Checked ? "1" : "0";
            db_tmm.DoCommand("update 基本设置 set 跳过精品='" + temp + "' where id='配置1'");

            temp = checkBox_sheZhi_buSaoMiaoLzl.Checked ? "1" : "0";
            db_tmm.DoCommand("update 基本设置 set 跳过楼中楼='" + temp + "' where id='配置1'");

            temp = checkBox_gaoJi_zhiShuChuShanTieRiZhi.Checked ? "1" : "0";
            db_tmm.DoCommand("update 基本设置 set 只输出删帖日志='" + temp + "' where id='配置1'");

            temp = checkBox_wbgl_kaiQiWenBenGuoLv.Checked ? "1" : "0";
            db_tmm.DoCommand("update 基本设置 set 开启文本过滤='" + temp + "' where id='配置1'");

            temp = checkBox_guanJianCi_biaoTiDengJi.Checked ? "1" : "0";
            db_tmm.DoCommand("update 基本设置 set 启用标题等级='" + temp + "' where id='配置1'");

            temp = checkBox_guanJianCi_neiRongDengJi.Checked ? "1" : "0";
            db_tmm.DoCommand("update 基本设置 set 启用内容等级='" + temp + "' where id='配置1'");

            temp = checkBox_sheZhi_zhuTiDengJiQiang.Checked ? "1" : "0";
            db_tmm.DoCommand("update 基本设置 set 启用主题等级墙='" + temp + "' where id='配置1'");

            temp = checkBox__sheZhi_huiFuDengJiQiang.Checked ? "1" : "0";
            db_tmm.DoCommand("update 基本设置 set 启用回复等级墙='" + temp + "' where id='配置1'");

            string[] tempStr = new string[0];
            db_tmm.DoCommand("delete from 标题关键词");
            tempStr = textBox_guanJianCi_biaoTiGuanJianCi.Text.Split(Environment.NewLine.ToCharArray());
            foreach (string str in tempStr)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    db_tmm.DoCommand("insert into 标题关键词(关键词) values('" + str + "')");
                }
            }

            db_tmm.DoCommand("delete from 内容关键词");
            tempStr = textBox_guanJianCi_neiRongGuanJianCi.Text.Split(Environment.NewLine.ToCharArray());
            foreach (string str in tempStr)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    db_tmm.DoCommand("insert into 内容关键词(关键词) values('" + str + "')");
                }
            }

            db_tmm.DoCommand("delete from 用户黑名单");
            tempStr = textBox_mingDan_heiMingDan.Text.Split(Environment.NewLine.ToCharArray());
            foreach (string str in tempStr)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    db_tmm.DoCommand("insert into 用户黑名单(用户名) values('" + str + "')");
                }
            }

            db_tmm.DoCommand("delete from 用户白名单");
            tempStr = textBox_mingDan_baiMingDan.Text.Split(Environment.NewLine.ToCharArray());
            foreach (string str in tempStr)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    db_tmm.DoCommand("insert into 用户白名单(用户名) values('" + str + "')");
                }
            }
        }

        /// <summary>
        /// 开始按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox_sheZhi_tiebaName.Text))
            {
                MessageBox.Show(text: " 请填写贴吧名", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            if (string.IsNullOrEmpty(BST.JianYiZhengZe(textBox_sheZhi_saoMiaoJianGe.Text, "^([0-9]{1,})$")))
            {
                MessageBox.Show(text: " 请填写扫描间隔", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            if (string.IsNullOrEmpty(BST.JianYiZhengZe(textBox_sheZhi_shanTieJianGe.Text, "^([0-9]{1,})$")))
            {
                MessageBox.Show(text: " 请填写删帖间隔", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            //如果启用等级墙
            if (checkBox_sheZhi_zhuTiDengJiQiang.Checked && string.IsNullOrEmpty(BST.JianYiZhengZe(textBox_sheZhi_zhuTiDengJiQiang.Text, "^([0-9]{1,2})$")))
            {
                MessageBox.Show(text: " 等级墙：主题等级格式不正确", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            if (checkBox__sheZhi_huiFuDengJiQiang.Checked && string.IsNullOrEmpty(BST.JianYiZhengZe(textBox_sheZhi_huiFuDengJiQiang.Text, "^([0-9]{1,2})$")))
            {
                MessageBox.Show(text: " 等级墙：回复等级格式不正确", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            //等级墙生效时间
            if (checkBox_sheZhi_zhuTiDengJiQiang.Checked || checkBox__sheZhi_huiFuDengJiQiang.Checked)
            {
                if (string.IsNullOrEmpty(BST.JianYiZhengZe(dateTimePicker_sheZhi_kaiShiShiJian.Text, "^(([0-9]|0[0-9]|1[0-9]|2[0-3]):([0-9]|[0-5][0-9]))$")))
                {
                    MessageBox.Show(text: " 等级墙：开始时间格式不正确\r\n【例】21:37", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                    return;
                }

                if (string.IsNullOrEmpty(BST.JianYiZhengZe(dateTimePicker_sheZhi_tingZhiShiJian.Text, "^(([0-9]|0[0-9]|1[0-9]|2[0-3]):([0-9]|[0-5][0-9]))$")))
                {
                    MessageBox.Show(text: " 等级墙：结束时间格式不正确\r\n【例】21:37", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                    return;
                }
            }

            //启用等级限制
            if (checkBox_guanJianCi_biaoTiDengJi.Checked && string.IsNullOrEmpty(BST.JianYiZhengZe(textBox_guanJianCi_biaoTiDengJi.Text, "^([0-9]{1,2})$")))
            {
                MessageBox.Show(text: " 等级墙：主题等级格式不正确", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            if (checkBox_guanJianCi_neiRongDengJi.Checked && string.IsNullOrEmpty(BST.JianYiZhengZe(textBox_guanJianCi_neiRongDengJi.Text, "^([0-9]{1,2})$")))
            {
                MessageBox.Show(text: " 等级墙：回复等级格式不正确", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            button_sheZhi_kaiShi.Enabled = false;
            FormEnabled(false);//批量禁用控件

            QuanJu.Stop = false;

            //初始化基本参数
            JibenCanshu();

            //修改托盘标题
            notifyIcon1.Text = textBox_sheZhi_tiebaName.Text + "吧 贴吧管理器迷你版";

            //扫描线程
            Thread saomiaothr = new Thread(new ThreadStart(Main))
            {
                IsBackground = true
            };
            saomiaothr.Start();

            //初始化删帖队列
            DaiShanDuiLie.Clear();

            //删帖线程
            Thread shantiethr = new Thread(new ThreadStart(Shantie))
            {
                IsBackground = true
            };
            shantiethr.Start();

            //操作量查询线程
            Thread caozuoliangthr = new Thread(new ThreadStart(CaozuoliangChaxun))
            {
                IsBackground = true
            };
            caozuoliangthr.Start();

            button_sheZhi_tingZhi.Enabled = true;
        }

        /// <summary>
        /// 停止按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            Say("正在停止...");
            button_sheZhi_tingZhi.Enabled = false;
            QuanJu.Stop = true;
        }

        /// <summary>
        /// 批量禁用控件
        /// </summary>
        /// <param name="b">bool</param>
        private void FormEnabled(bool b)
        {
            textBox_mingDan_baiMingDan.Enabled = b;
            textBox_sheZhi_tiebaName.Enabled = b;
            textBox_sheZhi_saoMiaoJianGe.Enabled = b;
            textBox_guanJianCi_biaoTiGuanJianCi.Enabled = b;
            textBox_guanJianCi_neiRongGuanJianCi.Enabled = b;
            textBox_mingDan_heiMingDan.Enabled = b;
            textBox_sheZhi_shanTieJianGe.Enabled = b;
            textBox_guanJianCi_biaoTiDengJi.Enabled = b;
            textBox_guanJianCi_neiRongDengJi.Enabled = b;
            textBox_sheZhi_zhuTiDengJiQiang.Enabled = b;
            textBox_sheZhi_huiFuDengJiQiang.Enabled = b;
            dateTimePicker_sheZhi_kaiShiShiJian.Enabled = b;
            dateTimePicker_sheZhi_tingZhiShiJian.Enabled = b;

            checkBox_sheZhi_buSaoMiaoZhiDingTie.Enabled = b;
            checkBox_sheZhi_buSaoMiaoJingPinTie.Enabled = b;
            checkBox_sheZhi_buSaoMiaoLzl.Enabled = b;
            checkBox_wbgl_kaiQiWenBenGuoLv.Enabled = b;
            checkBox_guanJianCi_biaoTiDengJi.Enabled = b;
            checkBox_guanJianCi_neiRongDengJi.Enabled = b;
            checkBox_sheZhi_zhuTiDengJiQiang.Enabled = b;
            checkBox__sheZhi_huiFuDengJiQiang.Enabled = b;

            listView_wbgl_wenBenGuoLvLieBiao.Enabled = b;
        }

        /// <summary>
        /// 基本参数
        /// </summary>
        public void JibenCanshu()
        {
            //基本参数
            QuanJu.TiebaName = textBox_sheZhi_tiebaName.Text;
            QuanJu.SaoMiaoJianGe = Convert.ToInt32(textBox_sheZhi_saoMiaoJianGe.Text);
            QuanJu.ShanTieJianGe = Convert.ToInt32(textBox_sheZhi_shanTieJianGe.Text);
            QuanJu.TiaoGuoZhiDing = checkBox_sheZhi_buSaoMiaoZhiDingTie.Checked;
            QuanJu.TiaoGuoJingPin = checkBox_sheZhi_buSaoMiaoJingPinTie.Checked;
            QuanJu.TiaoGuoLzl = checkBox_sheZhi_buSaoMiaoLzl.Checked;
            QuanJu.ZhiShuChuShanTieRiZhi = checkBox_gaoJi_zhiShuChuShanTieRiZhi.Checked;//只输出删帖日志

            //等级墙
            QuanJu.QiYongZhuTiDengJiQiang = checkBox_sheZhi_zhuTiDengJiQiang.Checked;
            QuanJu.ZhuTiDengJiQiang = Convert.ToInt32(textBox_sheZhi_zhuTiDengJiQiang.Text);
            QuanJu.QiYongHuiFuDengJiQiang = checkBox__sheZhi_huiFuDengJiQiang.Checked;
            QuanJu.HuiFuDengJiQiang = Convert.ToInt32(textBox_sheZhi_huiFuDengJiQiang.Text);
            QuanJu.DengJiQiangKaiShiShiJian = dateTimePicker_sheZhi_kaiShiShiJian.Text;
            QuanJu.DengJiQiangJieShuShiJian = dateTimePicker_sheZhi_tingZhiShiJian.Text;

            //等级限制
            QuanJu.QiYongBiaoTiDengJi = checkBox_guanJianCi_biaoTiDengJi.Checked;
            QuanJu.BiaoTiDengJi = Convert.ToInt32(textBox_guanJianCi_biaoTiDengJi.Text);
            QuanJu.QiYongBiaoTiDengJi = checkBox_guanJianCi_neiRongDengJi.Checked;
            QuanJu.NeiRongDengJi = Convert.ToInt32(textBox_guanJianCi_neiRongDengJi.Text);

            //关键词
            GuanJianCi.BiaoTi = textBox_guanJianCi_biaoTiGuanJianCi.Text.Split(Environment.NewLine.ToCharArray());
            GuanJianCi.NeiRong = textBox_guanJianCi_neiRongGuanJianCi.Text.Split(Environment.NewLine.ToCharArray());
            GuanJianCi.HeiMingDan = textBox_mingDan_heiMingDan.Text.Split(Environment.NewLine.ToCharArray());
            GuanJianCi.BaiMingDan = textBox_mingDan_baiMingDan.Text.Split(Environment.NewLine.ToCharArray());

            //文本过滤
            if (checkBox_wbgl_kaiQiWenBenGuoLv.Checked)
            {
                wenBenGuoLvLieBiao = db_tmm.GetDataTable("select * from 文本过滤");
            }
            else
            {
                wenBenGuoLvLieBiao = null;//这样就是关闭了
            }
        }

        /// <summary>
        /// 账号验证
        /// </summary>
        /// <param name="isLoad">来自初次加载</param>
        /// <returns></returns>
        public bool ZhanghaoYanzheng(bool isLoad = false)
        {
            if (isLoad)
            {
                QuanJu.YongHuMing = TLib.GetBaiduYongHuMing(QuanJu.Cookie);
            }

            if (string.IsNullOrEmpty(QuanJu.Cookie) || string.IsNullOrEmpty(QuanJu.YongHuMing))
            {
                if (!isLoad)
                {
                    Say("账号验证失败");
                }

                label_sheZHi_dangQianZhangHao.Text = "未登录";
                button_sheZhi_dengLuZhangHao.Enabled = true;
                button_sheZhi_shanChuZhangHao.Enabled = false;
                return false;
            }

            label_sheZHi_dangQianZhangHao.Text = QuanJu.YongHuMing;
            button_sheZhi_dengLuZhangHao.Enabled = false;
            button_sheZhi_shanChuZhangHao.Enabled = true;
            return true;
        }

        /// <summary>
        /// 贴吧参数
        /// </summary>
        public bool TiebaCanshu()
        {
            string tiebaname = QuanJu.TiebaName;
            string fid = TLib.GetTiebaFid(tiebaname);
            if (string.IsNullOrEmpty(fid))
            {
                Say("贴吧参数获取失败：fid无效");
                return false;
            }
            else
            {
                QuanJu.Fid = fid;
                return true;
            }
        }

        /// <summary>
        /// 主程序
        /// </summary>
        public void Main()
        {
            Save();//保存程序参数

            //账号验证
            if (!ZhanghaoYanzheng())
            {
                goto exit;
            }

            //贴吧参数获取
            if (!TiebaCanshu())
            {
                goto exit;
            }

            //主线程循环
            int hcsjc1 = Convert.ToInt32(BST.QuShiJianChuo());//初始化缓存时间戳
            while (true)
            {
                if (QuanJu.Stop)
                {
                    goto exit;
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                string cookie = QuanJu.Cookie;
                string tiebaName = QuanJu.TiebaName;

                //实例化
                Tieba tieba = new Tieba
                {
                    Cookie = cookie,
                    TiebaName = tiebaName,
                    Pn = 1
                };

                List<ZhuTiJieGou> zhuTiLieBiao = tieba.GetZhuTiLieBiao(wenBenGuoLvLieBiao);
                Debug.WriteLine(tieba.Cookie);
                if (!QuanJu.ZhiShuChuShanTieRiZhi)
                {
                    Say("本轮扫描开始，获取到" + zhuTiLieBiao.Count.ToString() + "个主题帖");
                }

                //每300秒，清理缓存
                int hcsjc2 = Convert.ToInt32(BST.QuShiJianChuo());
                if ((hcsjc2 - hcsjc1) >= 300)//距离上次清理大于300秒
                {
                    int timeStamp = Convert.ToInt32(BST.QuShiJianChuo());
                    timeStamp = timeStamp - 3600;//3600秒=60分钟
                    HuanCun.ZhuTiHuanCunLieBiao.RemoveWhen((tiezi) => tiezi.GengXinShiJian < timeStamp);
                    HuanCun.LzlHuanCunLieBiao.RemoveWhen((tiezi) => tiezi.GengXinShiJian < timeStamp);
                    HuanCun.DengJiHuanCunLieBiao.RemoveWhen((tiezi) => tiezi.GengXinShiJian < timeStamp);
                    hcsjc1 = Convert.ToInt32(BST.QuShiJianChuo());
                }

                //等级墙运行时段判断
                bool DengJiqiangYunxing = Lib.DengJiQiangShiJianPanDuan(QuanJu.DengJiQiangKaiShiShiJian, QuanJu.DengJiQiangJieShuShiJian);

                //主题贴循环
                foreach (ZhuTiJieGou zhuti in zhuTiLieBiao)
                {
                    if (QuanJu.Stop)
                    {
                        goto exit;
                    }

                    //缓存流程判断
                    long zuihouhfsj = 0;
                    for (int i = 0; i < HuanCun.ZhuTiHuanCunLieBiao.Count; i++)
                    {
                        if (HuanCun.ZhuTiHuanCunLieBiao[i].Tid == zhuti.Tid)
                        {
                            zuihouhfsj = HuanCun.ZhuTiHuanCunLieBiao[i].ZuiHouHuiFuShiJian;
                            break;
                        }
                    }

                    if (zuihouhfsj == 0)
                    {
                        //不存在
                        HuanCun.ZhuTiHuanCunLieBiao.Add(new HuanCun.ZhuTiHuanCunJieGou
                        {
                            Tid = zhuti.Tid,
                            FaTieShiJian = zhuti.FaTieShiJian,
                            ZuiHouHuiFuShiJian = zhuti.ZuiHouHuiFuShiJian,
                            GengXinShiJian = Convert.ToInt64(BST.QuShiJianChuo())
                        });
                    }
                    else
                    {
                        //存在
                        if (zuihouhfsj == zhuti.ZuiHouHuiFuShiJian)
                        {
                            //跳过
                            continue;
                        }
                        else
                        {
                            //更新
                            for (int i = 0; i < HuanCun.ZhuTiHuanCunLieBiao.Count; i++)
                            {
                                if (HuanCun.ZhuTiHuanCunLieBiao[i].Tid == zhuti.Tid)
                                {
                                    HuanCun.ZhuTiHuanCunLieBiao[i].ZuiHouHuiFuShiJian = zhuti.ZuiHouHuiFuShiJian;
                                    break;
                                }
                            }

                            //以前获取过的帖子，强制倒叙读帖
                            //qiangzhidx = true;
                        }
                    }

                    //跳过置顶
                    if (QuanJu.TiaoGuoZhiDing)
                    {
                        if (zhuti.IsZhiDing || zhuti.IsHuiYuanZhiDing)
                        {
                            continue;
                        }
                    }

                    //跳过精品
                    if (QuanJu.TiaoGuoJingPin)
                    {
                        if (zhuti.IsJingPin)
                        {
                            continue;
                        }
                    }

                    toolStripStatusLabel1.Text = zhuti.Tid.ToString() + " " + zhuti.BiaoTi + " " + zhuti.UidCanShu.YongHuMing;

                    //用户白名单
                    foreach (string bmd in GuanJianCi.BaiMingDan)
                    {
                        if (string.IsNullOrEmpty(bmd))
                        {
                            continue;
                        }

                        if (zhuti.UidCanShu.YongHuMing == bmd)
                        {
                            goto tiaoguobt;//跳过标题审查
                        }
                    }

                    //主题待删
                    DaiShanDuiLieJieGou zhuTiDaiShan = new DaiShanDuiLieJieGou
                    {
                        LeiXing = ShanTieLeiXing.ShanZhuTi,
                        BiaoTi = zhuti.BiaoTi,
                        Tid = zhuti.Tid,
                        LouCeng = 1,
                        YongHuMing = zhuti.UidCanShu.YongHuMing,
                        FaTieShiJian = zhuti.FaTieShiJian
                    };

                    //主题等级墙
                    if (DengJiqiangYunxing && QuanJu.QiYongZhuTiDengJiQiang)
                    {
                        int zhutiDengJi = HuanCun.GetDengJiHuanCun(zhuti.UidCanShu.YongHuMing, QuanJu.TiebaName);
                        Debug.Write(zhuti.Tid.ToString() + " " + zhuti.UidCanShu.YongHuMing + " " + zhutiDengJi.ToString() + "<" + QuanJu.ZhuTiDengJiQiang.ToString() + " " + BST.ShiJianChuoDaoWenben(zhuti.FaTieShiJian * 1000));
                        if (zhutiDengJi < QuanJu.ZhuTiDengJiQiang)
                        {
                            if (Lib.FaYanShiJianPanDuan(zhuti.FaTieShiJian, QuanJu.DengJiQiangKaiShiShiJian, QuanJu.DengJiQiangJieShuShiJian))
                            {
                                zhuTiDaiShan.YuanYin = "主题等级墙:" + zhuti.UidCanShu.YongHuMing + "(" + zhutiDengJi + ")";
                                if (!DaiShanDuiLie.ListIsRepeat(tiezi => tiezi.Tid == zhuTiDaiShan.Tid))
                                {
                                    DaiShanDuiLie.Add(zhuTiDaiShan);
                                }
                                Debug.WriteLine(" 删除");
                                goto tiaoguozt;//跳过这个主题帖 
                            }
                            else
                            {
                                Debug.WriteLine(" 不在时间范围");
                            }
                        }
                        else
                        {
                            Debug.WriteLine(" 等级不符");
                        }
                    }

                    //用户名关键词
                    foreach (string yonghuGjc in GuanJianCi.HeiMingDan)
                    {
                        if (string.IsNullOrEmpty(yonghuGjc))
                        {
                            continue;
                        }

                        if (zhuti.UidCanShu.YongHuMing.Contains(yonghuGjc))
                        {
                            zhuTiDaiShan.YuanYin = "用户名关键词:" + zhuti.UidCanShu.YongHuMing;
                            if (!DaiShanDuiLie.ListIsRepeat(tiezi => tiezi.Tid == zhuTiDaiShan.Tid))
                            {
                                DaiShanDuiLie.Add(zhuTiDaiShan);
                            }
                            goto tiaoguozt;//跳过这个主题帖
                        }
                    }

                    //判断帖子标题
                    foreach (string biaotiGjc in GuanJianCi.BiaoTi)
                    {
                        if (string.IsNullOrEmpty(biaotiGjc))
                        {
                            continue;
                        }

                        if (zhuti.BiaoTi.Contains(biaotiGjc))
                        {
                            zhuTiDaiShan.YuanYin = "关键词:" + biaotiGjc;
                            if (!DaiShanDuiLie.ListIsRepeat(tiezi => tiezi.Tid == zhuTiDaiShan.Tid))
                            {
                                if (QuanJu.QiYongBiaoTiDengJi)
                                {
                                    zhuti.UidCanShu.DengJi = HuanCun.GetDengJiHuanCun(zhuti.UidCanShu.YongHuMing, QuanJu.TiebaName);
                                    if (zhuti.UidCanShu.DengJi < QuanJu.BiaoTiDengJi)
                                    {
                                        DaiShanDuiLie.Add(zhuTiDaiShan);
                                        goto tiaoguozt;//跳过这个主题帖
                                    }
                                }
                                else
                                {
                                    DaiShanDuiLie.Add(zhuTiDaiShan);
                                    goto tiaoguozt;//跳过这个主题帖
                                }
                            }
                        }
                    }

                tiaoguobt:;//白名单专用，跳过标题审查

                    //获取帖子内容
                    List<NeiRongJieGou> neiRongLieBiao = null;
                    List<NeiRongJieGou> neiRongLieBiao_daoXu = null;

                    tieba.Tid = zhuti.Tid;
                    neiRongLieBiao = tieba.GetTieZiNeiRong(wbgldt: wenBenGuoLvLieBiao);

                    if (neiRongLieBiao.Count >= 30)//如果获取到的内容等于30个，要翻页
                    {
                        neiRongLieBiao_daoXu = tieba.GetTieZiNeiRong(isDaoxu: true, wbgldt: wenBenGuoLvLieBiao);

                        //合并+去重
                        neiRongLieBiao = neiRongLieBiao.Union(neiRongLieBiao_daoXu).ToList();
                    }

                    //手动释放
                    neiRongLieBiao_daoXu = null;

                    //判断是否被删除或获取失败
                    if (neiRongLieBiao.Count == 0)
                    {
                        goto tiaoguozt;//跳过这个主题帖
                    }

                    //降序 
                    //neiRongLieBiao = neiRongLieBiao.OrderByDescending(tiezi => tiezi.LouCeng).ToList();
                    //升序 
                    neiRongLieBiao = neiRongLieBiao.OrderBy(tiezi => tiezi.LouCeng).ToList();

                    //判断内容
                    foreach (NeiRongJieGou neirong in neiRongLieBiao)
                    {
                        if (QuanJu.Stop)
                        {
                            goto exit;
                        }

                        //用户白名单
                        foreach (string bmd in GuanJianCi.BaiMingDan)
                        {
                            if (string.IsNullOrEmpty(bmd))
                            {
                                continue;
                            }

                            if (neirong.UidCanShu.YongHuMing == bmd)
                            {
                                //goto tiaoguonr;//去下一个内容
                                goto tiaolzl;//去楼中楼
                            }
                        }

                        //内容待删
                        DaiShanDuiLieJieGou NeiRongDaishan = new DaiShanDuiLieJieGou
                        {
                            BiaoTi = neirong.BiaoTi,
                            Tid = neirong.Tid,
                            YongHuMing = neirong.UidCanShu.YongHuMing,
                            FaTieShiJian = neirong.FaTieShiJian,
                            Pid = neirong.Pid,
                            LouCeng = neirong.LouCeng
                        };

                        //回复等级墙
                        if (DengJiqiangYunxing && QuanJu.QiYongHuiFuDengJiQiang)
                        {
                            if (neirong.UidCanShu.DengJi < QuanJu.HuiFuDengJiQiang)
                            {
                                if (Lib.FaYanShiJianPanDuan(neirong.FaTieShiJian, QuanJu.DengJiQiangKaiShiShiJian, QuanJu.DengJiQiangJieShuShiJian))
                                {
                                    NeiRongDaishan.YuanYin = "回复等级墙:" + neirong.UidCanShu.YongHuMing + "(" + neirong.UidCanShu.DengJi + ")";
                                    if (neirong.LouCeng == 1)
                                    {
                                        NeiRongDaishan.LeiXing = ShanTieLeiXing.ShanZhuTi;
                                        if (!DaiShanDuiLie.ListIsRepeat(tiezi => neirong.Tid == NeiRongDaishan.Tid))
                                        {
                                            DaiShanDuiLie.Add(NeiRongDaishan);
                                        }
                                        goto tiaoguozt;//跳过这个主题帖
                                    }
                                    else
                                    {
                                        NeiRongDaishan.LeiXing = ShanTieLeiXing.ShanHuiFu;
                                        if (!DaiShanDuiLie.ListIsRepeat(tiezi => neirong.Pid == NeiRongDaishan.Pid))
                                        {
                                            DaiShanDuiLie.Add(NeiRongDaishan);
                                        }
                                        goto tiaoguonr;//去下一个内容
                                    }
                                }
                            }
                        }

                        //用户名关键词
                        foreach (string yonghuGjc in GuanJianCi.HeiMingDan)
                        {
                            if (string.IsNullOrEmpty(yonghuGjc))
                            {
                                continue;
                            }

                            if (neirong.UidCanShu.YongHuMing.Contains(yonghuGjc))
                            {
                                NeiRongDaishan.YuanYin = "用户名关键词:" + neirong.UidCanShu.YongHuMing;
                                if (neirong.LouCeng == 1)
                                {
                                    NeiRongDaishan.LeiXing = ShanTieLeiXing.ShanZhuTi;
                                    if (!DaiShanDuiLie.ListIsRepeat(tiezi => neirong.Tid == NeiRongDaishan.Tid))
                                    {
                                        DaiShanDuiLie.Add(NeiRongDaishan);
                                    }
                                    goto tiaoguozt;//跳过这个主题帖
                                }
                                else
                                {
                                    NeiRongDaishan.LeiXing = ShanTieLeiXing.ShanHuiFu;
                                    if (!DaiShanDuiLie.ListIsRepeat(tiezi => neirong.Pid == NeiRongDaishan.Pid))
                                    {
                                        DaiShanDuiLie.Add(NeiRongDaishan);
                                    }
                                    goto tiaoguonr;//去下一个内容
                                }
                            }
                        }

                        //判断帖子内容
                        foreach (string neirongGjc in GuanJianCi.NeiRong)
                        {
                            string guanHianCi = neirongGjc;
                            if (string.IsNullOrEmpty(guanHianCi))
                            {
                                continue;
                            }

                            //变量替换，临时功能
                            if (guanHianCi.Contains("{BiaoTi}"))//暂时先这么写
                            {
                                if (neirong.LouCeng > 1)//不包含1楼的回复，不支持楼中楼
                                {
                                    guanHianCi = guanHianCi.Replace("{BiaoTi}", neirong.BiaoTi);
                                }
                            }

                            if (neirong.NeiRong.Contains(guanHianCi))
                            {
                                NeiRongDaishan.YuanYin = "关键词:" + guanHianCi;
                                if (neirong.LouCeng == 1)
                                {
                                    NeiRongDaishan.LeiXing = ShanTieLeiXing.ShanZhuTi;
                                    if (!DaiShanDuiLie.ListIsRepeat(tiezi => neirong.Tid == NeiRongDaishan.Tid))
                                    {
                                        if (QuanJu.QiYongNeiRongDengJi)
                                        {
                                            if (neirong.UidCanShu.DengJi < QuanJu.NeiRongDengJi)
                                            {
                                                DaiShanDuiLie.Add(NeiRongDaishan);
                                                goto tiaoguozt;//跳过这个主题帖
                                            }
                                        }
                                        else
                                        {
                                            DaiShanDuiLie.Add(NeiRongDaishan);
                                            goto tiaoguozt;//跳过这个主题帖
                                        }
                                    }
                                }
                                else
                                {
                                    NeiRongDaishan.LeiXing = ShanTieLeiXing.ShanHuiFu;
                                    if (!DaiShanDuiLie.ListIsRepeat(tiezi => neirong.Pid == NeiRongDaishan.Pid))
                                    {
                                        if (QuanJu.QiYongNeiRongDengJi)
                                        {
                                            if (neirong.UidCanShu.DengJi < QuanJu.NeiRongDengJi)
                                            {
                                                DaiShanDuiLie.Add(NeiRongDaishan);
                                                goto tiaoguonr;//去下一个回复
                                            }
                                        }
                                        else
                                        {
                                            DaiShanDuiLie.Add(NeiRongDaishan);
                                            goto tiaoguonr;//去下一个回复
                                        }
                                    }
                                }
                            }
                        }

                    //楼中楼
                    tiaolzl:;
                        if (QuanJu.TiaoGuoLzl)
                        {
                            goto tiaoguonr;
                        }

                        //先判断当前获取到的回复数
                        int dangqianLzlHuifushu = neirong.LzlHuiFuShu;
                        if (dangqianLzlHuifushu > 0)
                        {
                            int HuanCunLzlHuifushu = 0;
                            for (int i = 0; i < HuanCun.LzlHuanCunLieBiao.Count; i++)
                            {
                                if (HuanCun.LzlHuanCunLieBiao[i].Pid == neirong.Pid)
                                {
                                    HuanCunLzlHuifushu = HuanCun.LzlHuanCunLieBiao[i].HuiFuShu;
                                    break;
                                }
                            }

                            if (HuanCunLzlHuifushu == 0)
                            {
                                //第一次写入
                                HuanCun.LzlHuanCunLieBiao.Add(new HuanCun.LzlHuanCunJieGou
                                {
                                    Pid = neirong.Pid,
                                    HuiFuShu = dangqianLzlHuifushu,
                                    GengXinShiJian = 0
                                });
                            }
                            else if (dangqianLzlHuifushu == HuanCunLzlHuifushu)
                            {
                                //如果当前与缓存相等
                                goto tiaoguonr;
                            }

                            tieba.Pid = neirong.Pid;
                            List<NeiRongJieGou> lzlNeiRongLb = tieba.GetLzlNeiRong(wbgldt: wenBenGuoLvLieBiao);
                            foreach (NeiRongJieGou lzlNeiRong in lzlNeiRongLb)
                            {
                                //用户白名单
                                foreach (string bmd in GuanJianCi.BaiMingDan)
                                {
                                    if (string.IsNullOrEmpty(bmd))
                                    {
                                        continue;
                                    }

                                    if (lzlNeiRong.UidCanShu.YongHuMing == bmd)
                                    {
                                        goto lzltiaoguonr;//去下一个楼中楼
                                    }
                                }

                                //楼中楼待删
                                DaiShanDuiLieJieGou lzlDaishan = new DaiShanDuiLieJieGou
                                {
                                    BiaoTi = lzlNeiRong.BiaoTi,
                                    Tid = lzlNeiRong.Tid,
                                    YongHuMing = lzlNeiRong.UidCanShu.YongHuMing,
                                    FaTieShiJian = lzlNeiRong.FaTieShiJian,
                                    LeiXing = ShanTieLeiXing.ShanLzl,
                                    Pid = lzlNeiRong.Pid,
                                    Spid = lzlNeiRong.Spid,
                                    LouCeng = neirong.LouCeng //楼中楼没有楼层
                                };

                                //楼中楼等级墙
                                if (DengJiqiangYunxing && QuanJu.QiYongHuiFuDengJiQiang)
                                {
                                    if (lzlNeiRong.UidCanShu.DengJi < QuanJu.HuiFuDengJiQiang)
                                    {
                                        if (Lib.FaYanShiJianPanDuan(lzlNeiRong.FaTieShiJian, QuanJu.DengJiQiangKaiShiShiJian, QuanJu.DengJiQiangJieShuShiJian))
                                        {
                                            lzlDaishan.YuanYin = "楼中楼等级墙:" + lzlNeiRong.UidCanShu.YongHuMing + "(" + lzlNeiRong.UidCanShu.DengJi + ")";
                                            if (!DaiShanDuiLie.ListIsRepeat(tiezi => neirong.Spid == lzlDaishan.Spid))
                                            {
                                                DaiShanDuiLie.Add(lzlDaishan);
                                            }
                                            goto lzltiaoguonr;//去下一个楼中楼
                                        }
                                    }
                                }

                                //用户名关键词
                                foreach (string yonghuGjc in GuanJianCi.HeiMingDan)
                                {
                                    if (string.IsNullOrEmpty(yonghuGjc))
                                    {
                                        continue;
                                    }

                                    if (lzlNeiRong.UidCanShu.YongHuMing.Contains(yonghuGjc))
                                    {
                                        lzlDaishan.YuanYin = "用户名关键词:" + lzlNeiRong.UidCanShu.YongHuMing;
                                        if (!DaiShanDuiLie.ListIsRepeat(tiezi => neirong.Spid == lzlDaishan.Spid))
                                        {
                                            DaiShanDuiLie.Add(lzlDaishan);
                                        }
                                        goto lzltiaoguonr;//去下一个楼中楼
                                    }
                                }

                                //判断帖子内容
                                foreach (string neirongGjc in GuanJianCi.NeiRong)
                                {
                                    if (string.IsNullOrEmpty(neirongGjc))
                                    {
                                        continue;
                                    }

                                    if (lzlNeiRong.NeiRong.Contains(neirongGjc))
                                    {
                                        lzlDaishan.YuanYin = "关键词:" + neirongGjc;
                                        if (!DaiShanDuiLie.ListIsRepeat(tiezi => neirong.Spid == lzlDaishan.Spid))
                                        {
                                            if (QuanJu.QiYongNeiRongDengJi)
                                            {
                                                if (lzlNeiRong.UidCanShu.DengJi < QuanJu.NeiRongDengJi)
                                                {
                                                    DaiShanDuiLie.Add(lzlDaishan);
                                                    goto lzltiaoguonr;//去下一个楼中楼
                                                }
                                            }
                                            else
                                            {
                                                DaiShanDuiLie.Add(lzlDaishan);
                                                goto lzltiaoguonr;//去下一个楼中楼
                                            }
                                        }
                                    }
                                }

                            lzltiaoguonr:;//去下一个楼中楼的标记
                            }

                            //更新
                            for (int i = 0; i < HuanCun.LzlHuanCunLieBiao.Count; i++)
                            {
                                if (HuanCun.LzlHuanCunLieBiao[i].Pid == neirong.Pid)
                                {
                                    HuanCun.LzlHuanCunLieBiao[i].HuiFuShu = dangqianLzlHuifushu;
                                    HuanCun.LzlHuanCunLieBiao[i].GengXinShiJian = Convert.ToInt64(BST.QuShiJianChuo());
                                    break;
                                }
                            }
                        }

                    tiaoguonr:;//去下一个回复的标记
                    }

                tiaoguozt:;//下个帖子的标记
                }

                stopwatch.Stop();
                string yongshi = stopwatch.Elapsed.ToString();
                if (!QuanJu.ZhiShuChuShanTieRiZhi)
                {
                    Say("本轮扫描结束，用时: " + yongshi.Substring(0, yongshi.IndexOf(".")));
                }
                toolStripStatusLabel1.Text = "本轮扫描结束，等待下一轮扫描...";
                DengDai(QuanJu.SaoMiaoJianGe);
            }

        exit:;//结束线程
            FormEnabled(true);
            button_sheZhi_kaiShi.Enabled = true;
            button_sheZhi_tingZhi.Enabled = false;
            QuanJu.Stop = true;

            //修改托盘标题
            notifyIcon1.Text = "贴吧管理器迷你版";

            Say("扫描结束");
        }

        /// <summary>
        /// 等待
        /// </summary>
        public void DengDai(int s)
        {
            for (int i = 0; i < s; i++)
            {
                if (QuanJu.Stop)
                {
                    return;
                }

                BST.DengDai(1000);
            }
        }

        /// <summary>
        /// 待删队列
        /// </summary>
        public List<DaiShanDuiLieJieGou> DaiShanDuiLie = new List<DaiShanDuiLieJieGou>();

        /// <summary>
        /// 删帖队列
        /// </summary>
        public void Shantie()
        {
            while (true)
            {
                if (QuanJu.Stop)
                {
                    goto exit;
                }

                if (DaiShanDuiLie.Count > 0)
                {
                    DaiShanDuiLieJieGou daiShanTieZi = DaiShanDuiLie[0];//取出第一个
                    BaWu bawu = new BaWu
                    {
                        Cookie = QuanJu.Cookie,
                        TiebaName = QuanJu.TiebaName,
                        Fid = QuanJu.Fid,
                        Tid = daiShanTieZi.Tid.ToString(),
                        Pid = daiShanTieZi.Pid.ToString()
                    };

                    if (daiShanTieZi.LeiXing == ShanTieLeiXing.ShanZhuTi)
                    {
                        if (bawu.ShanZhuTi())
                        {
                            Say("删除 tid:" + daiShanTieZi.Tid.ToString() + " " + daiShanTieZi.LouCeng.ToString() + "楼 " + daiShanTieZi.YuanYin + " 成功 用时:" + ShantieYongshi(daiShanTieZi.FaTieShiJian));
                        }
                        else
                        {
                            Say("删除 tid:" + daiShanTieZi.Tid.ToString() + " " + daiShanTieZi.LouCeng.ToString() + "楼 " + daiShanTieZi.YuanYin + " 失败");
                        }
                    }
                    else if (daiShanTieZi.LeiXing == ShanTieLeiXing.ShanHuiFu)
                    {
                        if (bawu.ShanHuiFu())
                        {
                            Say("删除 tid:" + daiShanTieZi.Tid.ToString() + " " + daiShanTieZi.LouCeng.ToString() + "楼 " + daiShanTieZi.YuanYin + " 成功 用时:" + ShantieYongshi(daiShanTieZi.FaTieShiJian));
                        }
                        else
                        {
                            Say("删除 tid:" + daiShanTieZi.Tid.ToString() + " " + daiShanTieZi.LouCeng.ToString() + "楼 " + daiShanTieZi.YuanYin + " 失败");
                        }
                    }
                    else if (daiShanTieZi.LeiXing == ShanTieLeiXing.ShanLzl)
                    {
                        bawu.Pid = daiShanTieZi.Spid.ToString();

                        if (bawu.ShanHuiFu(true))
                        {
                            Say("删除 tid:" + daiShanTieZi.Tid.ToString() + " " + daiShanTieZi.LouCeng.ToString() + "楼lzl " + daiShanTieZi.YuanYin + " 成功 用时:" + ShantieYongshi(daiShanTieZi.FaTieShiJian));
                        }
                        else
                        {
                            Say("删除 tid:" + daiShanTieZi.Tid.ToString() + " " + daiShanTieZi.LouCeng.ToString() + "楼lzl " + daiShanTieZi.YuanYin + " 失败");
                        }
                    }

                    DaiShanDuiLie.RemoveAt(0);//从队列里删掉，不管有没有删成功
                    DengDai(QuanJu.ShanTieJianGe);//删帖间隔
                }

                DengDai(1);//队列循环限速1秒
            }

        exit:;//结束线程
        }

        /// <summary>
        /// 删帖用时
        /// </summary>
        /// <param name="sjc">时间戳</param>
        /// <returns></returns>
        public string ShantieYongshi(long sjc)
        {
            if (sjc == 0)
            {
                return "无法计算";
            }

            int dangqiansj = Convert.ToInt32(BST.QuShiJianChuo());
            long fatiesj = sjc;
            long yongshi = dangqiansj - fatiesj;

            string shuchuStr = string.Empty;
            if (yongshi < 60)
            {
                shuchuStr += yongshi.ToString() + "秒";
            }
            else
            {
                yongshi = yongshi / 60;
                if (yongshi < 60)
                {
                    shuchuStr += yongshi.ToString() + "分";
                }
                else
                {
                    yongshi = yongshi / 60;
                    if (yongshi < 24)
                    {
                        shuchuStr += yongshi.ToString() + "时";
                    }
                    else
                    {
                        yongshi = yongshi / 24;
                        shuchuStr += yongshi.ToString() + "天";
                    }
                }
            }

            return shuchuStr;
        }

        /// <summary>
        /// 操作量查询
        /// </summary>
        public void CaozuoliangChaxun()
        {
            while (true)
            {
                if (QuanJu.Stop)
                {
                    goto exit;
                }

                //查询
                BaWu bawu = new BaWu()
                {
                    Cookie = QuanJu.Cookie,
                    TiebaName = QuanJu.TiebaName
                };
                string caozuoliang = bawu.CaoZuoLiangChaXun(QuanJu.YongHuMing, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"));

                //输出
                toolStripStatusLabel2.Text = String.Format("操作量 {0}/3000", caozuoliang);

                //查询
                for (int i = 0; i < 180; i++)//180秒=3分钟
                {
                    if (QuanJu.Stop)
                    {
                        goto exit;
                    }

                    DengDai(1); //1秒
                }
            }

        exit:;
        }

        /// <summary>
        /// 输出条数
        /// </summary>
        private int ShuchuTiaoshu = 0;

        /// <summary>
        /// 信息输出
        /// </summary>
        /// <param name="color"></param>
        /// <param name="text"></param>
        public void Say(string text)
        {
            if (ShuchuTiaoshu >= 500)
            {
                listBox_xinXiShuChu.Items.Clear();
                ShuchuTiaoshu = 0;
            }

            ShuchuTiaoshu++;

            listBox_xinXiShuChu.Items.Add(BST.ShiJianGeShiHua(2) + " " + text);
            listBox_xinXiShuChu.SelectedIndex = listBox_xinXiShuChu.Items.Count - 1;
        }

        /// <summary>
        /// 帖子浏览器 读取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(BST.JianYiZhengZe(textBox_liuLanQi_tieHao.Text, "^([0-9]{1,})$")))
            {
                MessageBox.Show(text: " 请先填写帖号", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            if (string.IsNullOrEmpty(BST.JianYiZhengZe(textBox_liuLanQi_yeMa.Text, "^([0-9]{1,})$")))
            {
                MessageBox.Show(text: " 请填写页码", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            bool daoxu = false;
            if (checkBox_liuLanQi_daoXuChaKan.Checked)
            {
                daoxu = true;
            }

            //文本过滤
            if (checkBox_wbgl_kaiQiWenBenGuoLv.Checked)
            {
                wenBenGuoLvLieBiao = db_tmm.GetDataTable("select * from 文本过滤");
            }
            else
            {
                wenBenGuoLvLieBiao = null;//这样就是关闭了
            }

            Tieba tieba = new Tieba()
            {
                Cookie = QuanJu.Cookie,
                TiebaName = QuanJu.TiebaName,
                Tid = Convert.ToInt64(textBox_liuLanQi_tieHao.Text),
                Pn = Convert.ToInt32(textBox_liuLanQi_yeMa.Text)
            };

            List<NeiRongJieGou> neiRongLieBiao = tieba.GetTieZiNeiRong(isDaoxu: daoxu, wbgldt: wenBenGuoLvLieBiao);
            if (neiRongLieBiao.Count == 0)
            {
                MessageBox.Show(text: " 没有获取到回复", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Asterisk);
                return;
            }

            listView_liuLanQi_liuLanQi.Items.Clear();
            listView_liuLanQi_liuLanQi.BeginUpdate();
            foreach (NeiRongJieGou neirong in neiRongLieBiao)
            {
                ListViewItem lvi = new ListViewItem()
                {
                    Text = Convert.ToString(neirong.LouCeng)
                };

                lvi.SubItems.Add(neirong.BiaoTi);
                lvi.SubItems.Add(neirong.NeiRong);
                lvi.SubItems.Add(neirong.UidCanShu.YongHuMing);
                lvi.SubItems.Add(BST.ShiJianChuoDaoWenben(neirong.FaTieShiJian * 1000));
                listView_liuLanQi_liuLanQi.Items.Add(lvi);
            }
            listView_liuLanQi_liuLanQi.EndUpdate();

            neiRongLieBiao = null;
        }

        /// <summary>
        /// listView 项被单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_Click(object sender, EventArgs e)
        {
            if (listView_liuLanQi_liuLanQi.SelectedItems.Count > 0)//判断选中了几条
            {
                string text =
                    "标题：" + listView_liuLanQi_liuLanQi.SelectedItems[0].SubItems[1].Text
                    + "\r\n"
                    + "内容：" + listView_liuLanQi_liuLanQi.SelectedItems[0].SubItems[2].Text
                    + "\r\n\r\n"
                    + "作者：" + listView_liuLanQi_liuLanQi.SelectedItems[0].SubItems[3].Text
                    + "\r\n"
                    + "时间：" + listView_liuLanQi_liuLanQi.SelectedItems[0].SubItems[4].Text;

                textBox_liuLanQi_tieZiYuLan.Text = text;
            }
        }

        /// <summary>
        /// 清空主题缓存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (!QuanJu.Stop)
            {
                MessageBox.Show(text: " 扫描还在进行中，请先停止扫描", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            int rowCount = HuanCun.ZhuTiHuanCunLieBiao.Count;

            if (rowCount == 0)
            {
                MessageBox.Show(text: " 缓存为空", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Asterisk);
                return;
            }

            if (MessageBox.Show(text: " 查询到" + rowCount.ToString() + "条缓存记录，是否清空？", caption: "笨蛋雪说：操作不可逆！", buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Asterisk) == DialogResult.Yes)
            {
                HuanCun.ZhuTiHuanCunLieBiao.Clear();
                Say("主题缓存已清空");
            }
        }

        /// <summary>
        /// 清空楼中楼缓存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            if (!QuanJu.Stop)
            {
                MessageBox.Show(text: " 扫描还在进行中，请先停止扫描", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            int rowCount = HuanCun.LzlHuanCunLieBiao.Count;

            if (rowCount == 0)
            {
                MessageBox.Show(text: " 缓存为空", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Asterisk);
                return;
            }

            if (MessageBox.Show(text: " 查询到" + rowCount.ToString() + "条缓存记录，是否清空？", caption: "笨蛋雪说：操作不可逆！", buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Asterisk) == DialogResult.Yes)
            {
                HuanCun.LzlHuanCunLieBiao.Clear();
                Say("楼中楼缓存已清空");
            }
        }

        /// <summary>
        /// 切换日志输出状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_gaoJi_zhiShuChuShanTieRiZhi.Checked)
            {
                QuanJu.ZhiShuChuShanTieRiZhi = true;
            }
            else
            {
                QuanJu.ZhiShuChuShanTieRiZhi = false;
            }
        }

        /// <summary>
        /// 登录账号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            if (!QuanJu.Stop)
            {
                return;
            }

            //QuanJu.Cookie = string.Empty;
            //QuanJu.YongHuMing = string.Empty;

            new BaiduLogin().ShowDialog();

            ZhanghaoYanzheng();
        }

        /// <summary>
        /// 删除账号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            if (!QuanJu.Stop)
            {
                return;
            }

            if (MessageBox.Show(text: " 确定要删除账号吗？", caption: "笨蛋雪说：操作不可逆", buttons: MessageBoxButtons.OKCancel, icon: MessageBoxIcon.Exclamation) == DialogResult.Cancel)
            {
                return;
            }

            if (db_tmm.DoCommand("update 基本设置 set Cookie='' where id='配置1'") > 0)
            {
                QuanJu.Cookie = string.Empty;
                Say("账号已成功删除");
                label_sheZHi_dangQianZhangHao.Text = "未登录";
                button_sheZhi_dengLuZhangHao.Enabled = true;
                button_sheZhi_shanChuZhangHao.Enabled = false;
            }
            else
            {
                MessageBox.Show(text: " 操作失败", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 导入吧务团队名单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!QuanJu.Stop)
            {
                MessageBox.Show(text: " 扫描进行中，无法使用该功能", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            if (string.IsNullOrEmpty(textBox_sheZhi_tiebaName.Text))
            {
                MessageBox.Show(text: " 请先设置贴吧名", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Asterisk);
                tabControl1.SelectedIndex = 0;
                return;
            }

            textBox_mingDan_baiMingDan.Enabled = false;

            //吧务团队
            List<string> BaWuTuanDui = new List<string>();

            //取出 现有白名单用户列表
            string[] tempBauwTuandui = textBox_mingDan_baiMingDan.Text.Split(Environment.NewLine.ToCharArray());
            foreach (string tempbawu in tempBauwTuandui)
            {
                bool cunzai = false;
                foreach (string bawu in BaWuTuanDui)
                {
                    if (tempbawu == bawu)
                    {
                        cunzai = true;
                        break;
                    }
                }

                if (!cunzai)
                {
                    BaWuTuanDui.Add(tempbawu);
                }
            }

            //获取 现任吧务团队名单
            List<string> xinBaWuTuanDui = Tieba.GetBaWuTuanDui(QuanJu.Cookie, textBox_sheZhi_tiebaName.Text);
            foreach (string xinbawu in xinBaWuTuanDui)
            {
                if (BaWuTuanDui.Where(bawu => bawu == xinbawu).Count() == 0)
                {
                    BaWuTuanDui.Add(xinbawu);
                }
            }

            //输出到编辑框
            textBox_mingDan_baiMingDan.Text = string.Empty;
            foreach (string bawu in BaWuTuanDui)
            {
                if (!string.IsNullOrEmpty(bawu))
                {
                    textBox_mingDan_baiMingDan.AppendText(bawu + "\r\n");
                }
            }

            textBox_mingDan_baiMingDan.Enabled = true;
        }

        /// <summary>
        /// 窗体状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
            }
        }

        /// <summary>
        /// 双击托盘图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            notifyIcon1.Visible = true;
            WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// 文本过滤列表
        /// </summary>
        public DataTable wenBenGuoLvLieBiao = new DataTable();

        /// <summary>
        /// 更新文本过滤列表
        /// </summary>
        public void GengxinWenbenGuolv()
        {
            wenBenGuoLvLieBiao = db_tmm.GetDataTable("select * from 文本过滤");

            listView_wbgl_wenBenGuoLvLieBiao.Items.Clear();
            listView_wbgl_wenBenGuoLvLieBiao.BeginUpdate();
            foreach (DataRow dr in wenBenGuoLvLieBiao.Rows)
            {
                ListViewItem lvi = new ListViewItem()
                {
                    Text = dr[0].ToString()
                };

                lvi.SubItems.Add(dr[1].ToString());
                listView_wbgl_wenBenGuoLvLieBiao.Items.Add(lvi);
            }
            listView_wbgl_wenBenGuoLvLieBiao.EndUpdate();
        }

        /// <summary>
        /// 文本过滤 添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 添加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new WenBenGuoLv().ShowDialog();
            GengxinWenbenGuolv();//更新
        }

        /// <summary>
        /// 文本过滤 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 删除cms_wenbenguolv_Click(object sender, EventArgs e)
        {
            if (listView_wbgl_wenBenGuoLvLieBiao.SelectedItems.Count == 0) return;//判断选中了几条

            int index = listView_wbgl_wenBenGuoLvLieBiao.SelectedItems[0].Index;
            string guolvqian = listView_wbgl_wenBenGuoLvLieBiao.Items[index].SubItems[0].Text;

            if (MessageBox.Show(text: " 确定要删除 " + guolvqian + " 吗？", caption: "笨蛋雪说：操作不可逆", buttons: MessageBoxButtons.OKCancel, icon: MessageBoxIcon.Exclamation) == DialogResult.OK)
            {
                db_tmm.DoCommand("delete from 文本过滤 where 替换前='" + guolvqian + "'");
            }

            GengxinWenbenGuolv();//更新
        }

        /// <summary>
        /// 文本过滤 菜单 打开前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cms_wenbenguolv_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (listView_wbgl_wenBenGuoLvLieBiao.SelectedItems.Count == 0)//判断选中了几条
            {
                删除cms_wenbenguolv.Enabled = false;
            }
            else
            {
                删除cms_wenbenguolv.Enabled = true;
            }
        }

        /// <summary>
        /// 内容关键词变量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            string msg =
                "这是一个临时功能，用于应对部分会复制标题的广告哥\r\n\r\n" +
                "当前版本已支持的变量如下：\r\n" +
                "标题：{BiaoTi}\r\n\r\n" +
                "注意：该变量仅在内容关键词且不包含1楼的回复中生效";
            MessageBox.Show(text: msg, caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Asterisk);
        }
    }

    /// <summary>
    /// 关键词
    /// </summary>
    public class GuanJianCi
    {
        public static string[] BiaoTi = new string[0];//标题关键词
        public static string[] NeiRong = new string[0];//内容关键词
        public static string[] HeiMingDan = new string[0];//用户黑名单
        public static string[] BaiMingDan = new string[0];//用户白名单
    }

    /// <summary>
    /// 删帖类型
    /// </summary>
    public enum ShanTieLeiXing
    {
        ShanZhuTi = 1,
        ShanHuiFu = 2,
        ShanLzl = 3
    }

    /// <summary>
    /// 待删队列结构
    /// </summary>
    public class DaiShanDuiLieJieGou
    {
        public ShanTieLeiXing LeiXing;
        public string BiaoTi;
        public long Tid;
        public long Pid;
        public long Spid;
        public int LouCeng;
        public string YongHuMing;
        public long FaTieShiJian;
        public int TianShu;
        public string YuanYin;
    }
}