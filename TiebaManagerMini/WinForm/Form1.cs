using System;
using System.Data;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using CsharpHttpHelper;
using CsharpHttpHelper.Enum;
using BakaSnowTool;
using System.Diagnostics;
using TiebaLib;
using TiebaManagerMini.WinForm;
using static TiebaManagerMini.KuoZhan;

namespace TiebaManagerMini
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region 事件

        /// <summary>
        /// 窗口创建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //判断配置文件夹是否存在
            if (!Directory.Exists(Application.StartupPath + @"\配置文件夹"))
            {
                Directory.CreateDirectory(Application.StartupPath + @"\配置文件夹");
            }

            string[] pathList = new string[] {
                Application.StartupPath + @"\配置文件夹\标题关键词.txt",
                Application.StartupPath + @"\配置文件夹\黑名单.txt",
                Application.StartupPath + @"\配置文件夹\内容关键词.txt",
                Application.StartupPath + @"\配置文件夹\白名单.txt",
            };

            //如果文件不存在就创建
            foreach (var path in pathList)
            {
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, string.Empty);
                }
            }

            //软件配置
            ruanJianPeiZhi = new PeiZhi(Application.StartupPath + @"\配置文件夹\配置文件.ini", "配置1");

            //如果软件配置不存在
            if (!ruanJianPeiZhi.ExistIniFile)
            {
                ruanJianPeiZhi.TiebaName = string.Empty;
                ruanJianPeiZhi.SaoMiaoJianGe = 8;
                ruanJianPeiZhi.ShanTieJianGe = 3;
                ruanJianPeiZhi.TiaoGuoZhiDing = true;
                ruanJianPeiZhi.TiaoGuoJingPin = true;
                ruanJianPeiZhi.TiaoGuoLouZhongLou = true;
                ruanJianPeiZhi.ZhiShuChuShanTieRiZhi = false;
                ruanJianPeiZhi.QiYongZhuTiDengJiQiang = false;
                ruanJianPeiZhi.ZhuTiDengJiQiang = 2;
                ruanJianPeiZhi.QiYongHuiFuDengJiQiang = false;
                ruanJianPeiZhi.HuiFuDengJiQiang = 2;
                ruanJianPeiZhi.DengJiQiangKaiShiShiJian = "00:00";
                ruanJianPeiZhi.DengJiQiangJieShuShiJian = "23:59";
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

            DuQuCanShu();//读取配置文件
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

            BaoCunCanShu();//保存

            e.Cancel = false;
        }

        /// <summary>
        /// 开始按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_sheZhi_kaiShi_Click(object sender, EventArgs e)
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

            if (checkBox_sheZhi_huiFuDengJiQiang.Checked && string.IsNullOrEmpty(BST.JianYiZhengZe(textBox_sheZhi_huiFuDengJiQiang.Text, "^([0-9]{1,2})$")))
            {
                MessageBox.Show(text: " 等级墙：回复等级格式不正确", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                return;
            }

            //等级墙生效时间
            if (checkBox_sheZhi_zhuTiDengJiQiang.Checked || checkBox_sheZhi_huiFuDengJiQiang.Checked)
            {
                if (string.IsNullOrEmpty(BST.JianYiZhengZe(dateTimePicker_sheZhi_kaiShiShiJian.Text, "^(([0-9]|0[0-9]|1[0-9]|2[0-3]):([0-9]|[0-5][0-9]))$")))
                {
                    MessageBox.Show(text: " 等级墙：开始时间格式不正确\r\n【例】21:37", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                    return;
                }

                if (string.IsNullOrEmpty(BST.JianYiZhengZe(dateTimePicker_sheZhi_jieShuShiJian.Text, "^(([0-9]|0[0-9]|1[0-9]|2[0-3]):([0-9]|[0-5][0-9]))$")))
                {
                    MessageBox.Show(text: " 等级墙：结束时间格式不正确\r\n【例】21:37", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Exclamation);
                    return;
                }
            }

            button_sheZhi_kaiShi.Enabled = false;
            FormEnabled(false);//批量禁用控件

            QuanJu.Stop = false;

            //加载运行参数
            JiaZaiYunXingCanShu();

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
            Thread shantiethr = new Thread(new ThreadStart(ShanTie))
            {
                IsBackground = true
            };
            shantiethr.Start();

            //操作量查询线程
            Thread caozuoliangthr = new Thread(new ThreadStart(CaoZuoLiangChaXun))
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
        private void button_sheZhi_tingZhi_Click(object sender, EventArgs e)
        {
            Say("正在停止...");
            button_sheZhi_tingZhi.Enabled = false;
            QuanJu.Stop = true;
        }

        /// <summary>
        /// 导入吧务团队名单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabel_mingDan_daoRuBaWuTuanDui_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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
            List<Tieba.BaWuTuanDuiJieGou> xinBaWuTuanDui = Tieba.GetBaWuTuanDui(textBox_sheZhi_tiebaName.Text, out string msg);
            foreach (Tieba.BaWuTuanDuiJieGou xinBaWu in xinBaWuTuanDui)
            {
                if (BaWuTuanDui.Count(bawu => bawu == xinBaWu.YongHuMing) == 0)
                {
                    BaWuTuanDui.Add(xinBaWu.YongHuMing);
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
        /// 按钮 单击 账号管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_zhangHao_zhangHaoGuanLi_Click(object sender, EventArgs e)
        {
            TiebaZhangHaoGuanLiForm tiebaZhangHaoGuanLiForm = new TiebaZhangHaoGuanLiForm();
            tiebaZhangHaoGuanLiForm.ShowDialog();
        }

        /// <summary>
        /// 清空主题缓存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_gaoJi_qingKongZhuTiHuanCun_Click(object sender, EventArgs e)
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
        private void button_gaoJi_qingKongLzlHuanCun_Click(object sender, EventArgs e)
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
        /// 内容关键词变量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_gaoJi_neiRongGuanJianCiBianLiang_Click(object sender, EventArgs e)
        {
            string msg =
                "这是一个临时功能，用于应对部分会复制标题的广告哥\r\n\r\n" +
                "当前版本已支持的变量如下：\r\n" +
                "标题：{BiaoTi}\r\n\r\n" +
                "注意：该变量仅在内容关键词且不包含1楼的回复中生效";
            MessageBox.Show(text: msg, caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Asterisk);
        }

        /// <summary>
        /// 切换日志输出状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_gaoJi_zhiShuChuShanTieRiZhi_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_gaoJi_zhiShuChuShanTieRiZhi.Checked)
            {
                YunXingCanShu.ZhiShuChuShanTieRiZhi = true;
            }
            else
            {
                YunXingCanShu.ZhiShuChuShanTieRiZhi = false;
            }
        }

        #endregion

        #region 函数

        /// <summary>
        /// 软件配置
        /// </summary>
        public PeiZhi ruanJianPeiZhi = null;

        /// <summary>
        /// 账号配置
        /// </summary>
        public ZhangHao zhangHaoPeiZhi = null;

        /// <summary>
        /// 版本验证
        /// </summary>
        /// <returns></returns>
        private bool Version()
        {
            string html, v;
            while (true)
            {
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = "http://www.bakasnow.com/version.php?n=" + QuanJu.Vname,//URL     必需项
                    Method = "GET",//URL     可选项 默认为Get
                    Timeout = 100000,//连接超时时间     可选项默认为100000
                    ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                    IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写
                    Cookie = "",//字符串Cookie     可选项
                    UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:18.0) Gecko/20100101 Firefox/18.0",//用户的浏览器类型，版本，操作系统     可选项有默认值
                    Accept = "text/html, application/xhtml+xml, */*",//    可选项有默认值
                    ContentType = "text/html",//返回类型    可选项有默认值
                    Referer = "http://www.bakasnow.com/",//来源URL     可选项
                    Allowautoredirect = false,//是否根据３０１跳转     可选项
                    AutoRedirectCookie = false,//是否自动处理Cookie     可选项
                    Postdata = "",//Post数据     可选项GET时不需要写
                    ResultType = ResultType.String,//返回数据类型，是Byte还是String
                };
                HttpResult result = http.GetHtml(item);
                html = result.Html;
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
        /// 读取参数
        /// </summary>
        private void DuQuCanShu()
        {
            textBox_sheZhi_tiebaName.Text = ruanJianPeiZhi.TiebaName;
            textBox_sheZhi_saoMiaoJianGe.Text = Convert.ToString(ruanJianPeiZhi.SaoMiaoJianGe);
            textBox_sheZhi_shanTieJianGe.Text = Convert.ToString(ruanJianPeiZhi.ShanTieJianGe);
            textBox_sheZhi_zhuTiDengJiQiang.Text = Convert.ToString(ruanJianPeiZhi.ZhuTiDengJiQiang);
            textBox_sheZhi_huiFuDengJiQiang.Text = Convert.ToString(ruanJianPeiZhi.HuiFuDengJiQiang);
            dateTimePicker_sheZhi_kaiShiShiJian.Text = Convert.ToString(ruanJianPeiZhi.DengJiQiangKaiShiShiJian);
            dateTimePicker_sheZhi_jieShuShiJian.Text = Convert.ToString(ruanJianPeiZhi.DengJiQiangJieShuShiJian);

            checkBox_sheZhi_tiaoGuoZhiDingTie.Checked = ruanJianPeiZhi.TiaoGuoZhiDing;
            checkBox_sheZhi_tiaoGuoJingPinTie.Checked = ruanJianPeiZhi.TiaoGuoJingPin;
            checkBox_sheZhi_tiaoGuoLzl.Checked = ruanJianPeiZhi.TiaoGuoLouZhongLou;
            checkBox_gaoJi_zhiShuChuShanTieRiZhi.Checked = ruanJianPeiZhi.ZhiShuChuShanTieRiZhi;
            checkBox_sheZhi_zhuTiDengJiQiang.Checked = ruanJianPeiZhi.QiYongZhuTiDengJiQiang;
            checkBox_sheZhi_huiFuDengJiQiang.Checked = ruanJianPeiZhi.QiYongHuiFuDengJiQiang;

            //关键词
            string guanJianCiWenBen = File.ReadAllText(Application.StartupPath + @"\配置文件夹\标题关键词.txt");
            string[] guanJianCiLieBiao = guanJianCiWenBen.Split(new string[] { "\n" }, StringSplitOptions.None);
            foreach (var guanJianCi in guanJianCiLieBiao)
            {
                textBox_guanJianCi_biaoTiGuanJianCi.AppendText(guanJianCi + Environment.NewLine);
            }

            guanJianCiWenBen = File.ReadAllText(Application.StartupPath + @"\配置文件夹\内容关键词.txt");
            guanJianCiLieBiao = guanJianCiWenBen.Split(new string[] { "\n" }, StringSplitOptions.None);
            foreach (var guanJianCi in guanJianCiLieBiao)
            {
                textBox_guanJianCi_neiRongGuanJianCi.AppendText(guanJianCi + Environment.NewLine);
            }

            guanJianCiWenBen = File.ReadAllText(Application.StartupPath + @"\配置文件夹\黑名单.txt");
            guanJianCiLieBiao = guanJianCiWenBen.Split(new string[] { "\n" }, StringSplitOptions.None);
            foreach (var guanJianCi in guanJianCiLieBiao)
            {
                textBox_mingDan_heiMingDan.AppendText(guanJianCi + Environment.NewLine);
            }

            guanJianCiWenBen = File.ReadAllText(Application.StartupPath + @"\配置文件夹\白名单.txt");
            guanJianCiLieBiao = guanJianCiWenBen.Split(new string[] { "\n" }, StringSplitOptions.None);
            foreach (var guanJianCi in guanJianCiLieBiao)
            {
                textBox_mingDan_baiMingDan.AppendText(guanJianCi + Environment.NewLine);
            }
        }

        /// <summary>
        /// 保存参数
        /// </summary>
        private void BaoCunCanShu()
        {
            ruanJianPeiZhi.TiebaName = textBox_sheZhi_tiebaName.Text;
            ruanJianPeiZhi.SaoMiaoJianGe = Convert.ToInt32(textBox_sheZhi_saoMiaoJianGe.Text);
            ruanJianPeiZhi.ShanTieJianGe = Convert.ToInt32(textBox_sheZhi_shanTieJianGe.Text);
            ruanJianPeiZhi.ZhuTiDengJiQiang = Convert.ToInt32(textBox_sheZhi_zhuTiDengJiQiang.Text);
            ruanJianPeiZhi.HuiFuDengJiQiang = Convert.ToInt32(textBox_sheZhi_huiFuDengJiQiang.Text);
            ruanJianPeiZhi.DengJiQiangKaiShiShiJian = dateTimePicker_sheZhi_kaiShiShiJian.Text;
            ruanJianPeiZhi.DengJiQiangJieShuShiJian = dateTimePicker_sheZhi_jieShuShiJian.Text;

            ruanJianPeiZhi.TiaoGuoZhiDing = checkBox_sheZhi_tiaoGuoZhiDingTie.Checked;
            ruanJianPeiZhi.TiaoGuoJingPin = checkBox_sheZhi_tiaoGuoJingPinTie.Checked;
            ruanJianPeiZhi.TiaoGuoLouZhongLou = checkBox_sheZhi_tiaoGuoLzl.Checked;
            ruanJianPeiZhi.ZhiShuChuShanTieRiZhi = checkBox_gaoJi_zhiShuChuShanTieRiZhi.Checked;
            ruanJianPeiZhi.QiYongZhuTiDengJiQiang = checkBox_sheZhi_zhuTiDengJiQiang.Checked;
            ruanJianPeiZhi.QiYongHuiFuDengJiQiang = checkBox_sheZhi_huiFuDengJiQiang.Checked;

            //关键词
            File.WriteAllText(Application.StartupPath + @"\配置文件夹\标题关键词.txt", textBox_guanJianCi_biaoTiGuanJianCi.Text);
            File.WriteAllText(Application.StartupPath + @"\配置文件夹\内容关键词.txt", textBox_guanJianCi_neiRongGuanJianCi.Text);
            File.WriteAllText(Application.StartupPath + @"\配置文件夹\黑名单.txt", textBox_mingDan_heiMingDan.Text);
            File.WriteAllText(Application.StartupPath + @"\配置文件夹\白名单.txt", textBox_mingDan_baiMingDan.Text);
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
            textBox_sheZhi_zhuTiDengJiQiang.Enabled = b;
            textBox_sheZhi_huiFuDengJiQiang.Enabled = b;
            dateTimePicker_sheZhi_kaiShiShiJian.Enabled = b;
            dateTimePicker_sheZhi_jieShuShiJian.Enabled = b;

            checkBox_sheZhi_tiaoGuoZhiDingTie.Enabled = b;
            checkBox_sheZhi_tiaoGuoJingPinTie.Enabled = b;
            checkBox_sheZhi_tiaoGuoLzl.Enabled = b;
            checkBox_sheZhi_zhuTiDengJiQiang.Enabled = b;
            checkBox_sheZhi_huiFuDengJiQiang.Enabled = b;
        }

        /// <summary>
        /// 加载运行参数
        /// </summary>
        public void JiaZaiYunXingCanShu()
        {
            //运行参数
            YunXingCanShu.SaoMiaoJianGe = Convert.ToInt32(textBox_sheZhi_saoMiaoJianGe.Text);
            YunXingCanShu.ShanTieJianGe = Convert.ToInt32(textBox_sheZhi_shanTieJianGe.Text);
            YunXingCanShu.TiaoGuoZhiDing = checkBox_sheZhi_tiaoGuoZhiDingTie.Checked;
            YunXingCanShu.TiaoGuoJingPin = checkBox_sheZhi_tiaoGuoJingPinTie.Checked;
            YunXingCanShu.TiaoGuoLzl = checkBox_sheZhi_tiaoGuoLzl.Checked;
            YunXingCanShu.ZhiShuChuShanTieRiZhi = checkBox_gaoJi_zhiShuChuShanTieRiZhi.Checked;

            //等级墙
            YunXingCanShu.QiYongZhuTiDengJiQiang = checkBox_sheZhi_zhuTiDengJiQiang.Checked;
            YunXingCanShu.ZhuTiDengJiQiang = Convert.ToInt32(textBox_sheZhi_zhuTiDengJiQiang.Text);
            YunXingCanShu.QiYongHuiFuDengJiQiang = checkBox_sheZhi_huiFuDengJiQiang.Checked;
            YunXingCanShu.HuiFuDengJiQiang = Convert.ToInt32(textBox_sheZhi_huiFuDengJiQiang.Text);
            YunXingCanShu.DengJiQiangKaiShiShiJian = dateTimePicker_sheZhi_kaiShiShiJian.Text;
            YunXingCanShu.DengJiQiangJieShuShiJian = dateTimePicker_sheZhi_jieShuShiJian.Text;

            //全局参数
            YunXingCanShu.TiebaName = textBox_sheZhi_tiebaName.Text;
            YunXingCanShu.Fid = Tieba.GetTiebaFid(YunXingCanShu.TiebaName);

            //关键词
            GuanJianCi.BiaoTi = textBox_guanJianCi_biaoTiGuanJianCi.Text.Split(new string[] { "\n" }, StringSplitOptions.None);
            GuanJianCi.NeiRong = textBox_guanJianCi_neiRongGuanJianCi.Text.Split(new string[] { "\n" }, StringSplitOptions.None);
            GuanJianCi.HeiMingDan = textBox_mingDan_heiMingDan.Text.Split(new string[] { "\n" }, StringSplitOptions.None);
            GuanJianCi.BaiMingDan = textBox_mingDan_baiMingDan.Text.Split(new string[] { "\n" }, StringSplitOptions.None);
        }

        /// <summary>
        /// 贴吧参数
        /// </summary>
        public bool TiebaCanshu()
        {
            string tiebaname = YunXingCanShu.TiebaName;
            string fid = Tieba.GetTiebaFid(tiebaname);
            if (string.IsNullOrEmpty(fid))
            {
                Say("贴吧参数获取失败：fid无效");
                return false;
            }
            else
            {
                YunXingCanShu.Fid = fid;
                return true;
            }
        }

        /// <summary>
        /// 输出条数
        /// </summary>
        private int ShuChuTiaoShu = 0;

        /// <summary>
        /// 信息输出
        /// </summary>
        /// <param name="color"></param>
        /// <param name="text"></param>
        public void Say(string text)
        {
            if (ShuChuTiaoShu >= 500)
            {
                listBox_xinXiShuChu.Items.Clear();
                ShuChuTiaoShu = 0;
            }

            ShuChuTiaoShu++;

            listBox_xinXiShuChu.Items.Add($"[{DateTime.Now:T}] {text}");
            listBox_xinXiShuChu.SelectedIndex = listBox_xinXiShuChu.Items.Count - 1;
        }

        #endregion

        #region 主程序线程

        /// <summary>
        /// 主程序
        /// </summary>
        public void Main()
        {
            BaoCunCanShu();//保存程序参数

            //贴吧参数获取
            if (!TiebaCanshu())
            {
                goto exit;
            }

            //主线程循环
            long hcsjc1 = BST.QuShiJianChuo();//初始化缓存时间戳
            while (true)
            {
                if (QuanJu.Stop)
                {
                    goto exit;
                }

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                string cookie = YunXingCanShu.Cookie;
                string tiebaName = YunXingCanShu.TiebaName;

                //实例化
                TiebaZhuTi tiebaZhuTi = new TiebaZhuTi(tiebaName)
                {
                    Cookie = cookie
                };

                List<TiebaZhuTi.JieGou> zhuTiLieBiao = tiebaZhuTi.Get(1);
                Debug.WriteLine(tiebaZhuTi.Cookie);
                if (!YunXingCanShu.ZhiShuChuShanTieRiZhi)
                {
                    Say("本轮扫描开始，获取到" + zhuTiLieBiao.Count.ToString() + "个主题帖");
                }

                //每300秒，清理缓存
                long hcsjc2 = BST.QuShiJianChuo();
                if ((hcsjc2 - hcsjc1) >= 300)//距离上次清理大于300秒
                {
                    long timeStamp = BST.QuShiJianChuo();
                    timeStamp -= 3600;//3600秒=60分钟
                    HuanCun.ZhuTiHuanCunLieBiao.RemoveWhen((tiezi) => tiezi.GengXinShiJian < timeStamp);
                    HuanCun.LzlHuanCunLieBiao.RemoveWhen((tiezi) => tiezi.GengXinShiJian < timeStamp);
                    hcsjc1 = BST.QuShiJianChuo();
                }

                //等级墙运行时段判断
                bool dengJiQiangYunXing = DengJiQiangShiJianPanDuan(YunXingCanShu.DengJiQiangKaiShiShiJian, YunXingCanShu.DengJiQiangJieShuShiJian);

                //主题贴循环
                foreach (TiebaZhuTi.JieGou zhuTi in zhuTiLieBiao)
                {
                    if (QuanJu.Stop)
                    {
                        goto exit;
                    }

                    //缓存流程判断
                    long zuiHouHuiFuShiJian = 0;
                    for (int i = 0; i < HuanCun.ZhuTiHuanCunLieBiao.Count; i++)
                    {
                        if (HuanCun.ZhuTiHuanCunLieBiao[i].Tid == zhuTi.Tid)
                        {
                            zuiHouHuiFuShiJian = HuanCun.ZhuTiHuanCunLieBiao[i].ZuiHouHuiFuShiJian;
                            break;
                        }
                    }

                    if (zuiHouHuiFuShiJian == 0)
                    {
                        //不存在
                        HuanCun.ZhuTiHuanCunLieBiao.Add(new HuanCun.ZhuTiHuanCunJieGou
                        {
                            Tid = zhuTi.Tid,
                            FaTieShiJian = zhuTi.FaTieShiJianChuo,
                            ZuiHouHuiFuShiJian = zhuTi.ZuiHouHuiFuShiJianChuo,
                            GengXinShiJian = Convert.ToInt64(BST.QuShiJianChuo())
                        });
                    }
                    else
                    {
                        //存在
                        if (zuiHouHuiFuShiJian == zhuTi.ZuiHouHuiFuShiJianChuo)
                        {
                            //跳过
                            continue;
                        }
                        else
                        {
                            //更新
                            for (int i = 0; i < HuanCun.ZhuTiHuanCunLieBiao.Count; i++)
                            {
                                if (HuanCun.ZhuTiHuanCunLieBiao[i].Tid == zhuTi.Tid)
                                {
                                    HuanCun.ZhuTiHuanCunLieBiao[i].ZuiHouHuiFuShiJian = zhuTi.ZuiHouHuiFuShiJianChuo;
                                    break;
                                }
                            }

                            //以前获取过的帖子，强制倒叙读帖
                            //qiangzhidx = true;
                        }
                    }

                    //跳过置顶
                    if (YunXingCanShu.TiaoGuoZhiDing)
                    {
                        if (zhuTi.IsZhiDing || zhuTi.IsHuiYuanZhiDing)
                        {
                            continue;
                        }
                    }

                    //跳过精品
                    if (YunXingCanShu.TiaoGuoJingPin)
                    {
                        if (zhuTi.IsJingPin)
                        {
                            continue;
                        }
                    }

                    toolStripStatusLabel1.Text = $"{zhuTi.Tid} {zhuTi.BiaoTi} {zhuTi.YongHuMing}";

                    //用户白名单
                    foreach (var baiMingDan in GuanJianCi.BaiMingDan)
                    {
                        if (string.IsNullOrEmpty(baiMingDan))
                        {
                            continue;
                        }

                        if (zhuTi.YongHuMing == baiMingDan)
                        {
                            goto tiaoguobt;//跳过标题审查
                        }
                    }

                    //主题待删
                    DaiShanDuiLieJieGou zhuTiDaiShan = new DaiShanDuiLieJieGou
                    {
                        LeiXing = ShanTieLeiXing.ShanZhuTi,
                        BiaoTi = zhuTi.BiaoTi,
                        Tid = zhuTi.Tid,
                        LouCeng = 1,
                        YongHuMing = zhuTi.YongHuMing,
                        FaTieShiJian = zhuTi.FaTieShiJianChuo
                    };

                    //用户名关键词
                    foreach (var heiMingDan in GuanJianCi.HeiMingDan)
                    {
                        if (string.IsNullOrEmpty(heiMingDan))
                        {
                            continue;
                        }

                        if (zhuTi.YongHuMing.Contains(heiMingDan))
                        {
                            zhuTiDaiShan.YuanYin = "用户名关键词:" + zhuTi.YongHuMing;
                            if (!DaiShanDuiLie.ListIsRepeat(tiezi => tiezi.Tid == zhuTiDaiShan.Tid))
                            {
                                DaiShanDuiLie.Add(zhuTiDaiShan);
                            }
                            goto tiaoGuoZhuTi;//跳过这个主题帖
                        }
                    }

                    //判断帖子标题
                    foreach (var biaoTiGuanJianCi in GuanJianCi.BiaoTi)
                    {
                        if (string.IsNullOrEmpty(biaoTiGuanJianCi))
                        {
                            continue;
                        }

                        if (zhuTi.BiaoTi.Contains(biaoTiGuanJianCi))
                        {
                            zhuTiDaiShan.YuanYin = "关键词:" + biaoTiGuanJianCi;
                            if (!DaiShanDuiLie.ListIsRepeat(tiezi => tiezi.Tid == zhuTiDaiShan.Tid))
                            {
                                DaiShanDuiLie.Add(zhuTiDaiShan);
                                goto tiaoGuoZhuTi;//跳过这个主题帖
                            }
                        }
                    }

                tiaoguobt:;//白名单专用，跳过标题审查

                    //获取帖子内容
                    List<TiebaHuiFu.JieGou> huiFuLieBiao = null;
                    List<TiebaHuiFu.JieGou> huiFuLieBiao_daoXu = null;

                    TiebaHuiFu tiebaHuiFu = new TiebaHuiFu(tiebaName)
                    {
                        Cookie = cookie,
                        Tid = zhuTi.Tid,
                        IsDaoXu = false
                    };

                    huiFuLieBiao = tiebaHuiFu.Get(1);

                    if (huiFuLieBiao.Count >= 30)//如果获取到的内容等于30个，要翻页
                    {
                        tiebaHuiFu.IsDaoXu = true;
                        huiFuLieBiao_daoXu = tiebaHuiFu.Get(1);

                        //合并+去重
                        huiFuLieBiao = huiFuLieBiao.Union(huiFuLieBiao_daoXu).ToList();
                    }

                    //手动释放
                    huiFuLieBiao_daoXu = null;

                    //判断是否被删除或获取失败
                    if (huiFuLieBiao.Count == 0)
                    {
                        goto tiaoGuoZhuTi;//跳过这个主题帖
                    }

                    //降序
                    //neiRongLieBiao = neiRongLieBiao.OrderByDescending(tiezi => tiezi.LouCeng).ToList();
                    //升序
                    huiFuLieBiao = huiFuLieBiao.OrderBy(tiezi => tiezi.LouCeng).ToList();

                    //判断内容
                    foreach (TiebaHuiFu.JieGou huiFu in huiFuLieBiao)
                    {
                        if (QuanJu.Stop)
                        {
                            goto exit;
                        }

                        //用户白名单
                        foreach (var baiMingDan in GuanJianCi.BaiMingDan)
                        {
                            if (string.IsNullOrEmpty(baiMingDan))
                            {
                                continue;
                            }

                            if (huiFu.YongHuMing == baiMingDan)
                            {
                                //goto tiaoGuoNeiRong;//去下一个内容
                                goto tiaolzl;//去楼中楼
                            }
                        }

                        //内容待删
                        DaiShanDuiLieJieGou NeiRongDaishan = new DaiShanDuiLieJieGou
                        {
                            BiaoTi = huiFu.BiaoTi,
                            Tid = huiFu.Tid,
                            YongHuMing = huiFu.YongHuMing,
                            FaTieShiJian = huiFu.FaTieShiJianChuo,
                            Pid = huiFu.Pid,
                            LouCeng = huiFu.LouCeng
                        };

                        //等级墙是否可以运行
                        if (dengJiQiangYunXing)
                        {
                            //判断楼层
                            if (huiFu.LouCeng == 1)
                            {
                                //主题等级墙
                                if (YunXingCanShu.QiYongZhuTiDengJiQiang)
                                {
                                    if (huiFu.DengJi < YunXingCanShu.ZhuTiDengJiQiang)
                                    {
                                        if (FaYanShiJianPanDuan(huiFu.FaTieShiJianChuo, YunXingCanShu.DengJiQiangKaiShiShiJian, YunXingCanShu.DengJiQiangJieShuShiJian))
                                        {
                                            NeiRongDaishan.LeiXing = ShanTieLeiXing.ShanZhuTi;
                                            if (!DaiShanDuiLie.ListIsRepeat(tiezi => huiFu.Tid == NeiRongDaishan.Tid))
                                            {
                                                DaiShanDuiLie.Add(NeiRongDaishan);
                                            }
                                            goto tiaoGuoZhuTi;//跳过这个主题帖
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //回复等级墙
                                if (YunXingCanShu.QiYongHuiFuDengJiQiang)
                                {
                                    if (huiFu.DengJi < YunXingCanShu.HuiFuDengJiQiang)
                                    {
                                        if (FaYanShiJianPanDuan(huiFu.FaTieShiJianChuo, YunXingCanShu.DengJiQiangKaiShiShiJian, YunXingCanShu.DengJiQiangJieShuShiJian))
                                        {
                                            NeiRongDaishan.YuanYin = "回复等级墙:" + huiFu.YongHuMing + "(" + huiFu.DengJi + ")";

                                            NeiRongDaishan.LeiXing = ShanTieLeiXing.ShanHuiFu;
                                            if (!DaiShanDuiLie.ListIsRepeat(tiezi => huiFu.Pid == NeiRongDaishan.Pid))
                                            {
                                                DaiShanDuiLie.Add(NeiRongDaishan);
                                            }
                                            goto tiaoGuoNeiRong;//去下一个内容
                                        }
                                    }
                                }
                            }
                        }

                        //用户名关键词
                        foreach (var heiMingDan in GuanJianCi.HeiMingDan)
                        {
                            if (string.IsNullOrEmpty(heiMingDan))
                            {
                                continue;
                            }

                            if (huiFu.YongHuMing.Contains(heiMingDan))
                            {
                                NeiRongDaishan.YuanYin = "用户名关键词:" + huiFu.YongHuMing;
                                if (huiFu.LouCeng == 1)
                                {
                                    NeiRongDaishan.LeiXing = ShanTieLeiXing.ShanZhuTi;
                                    if (!DaiShanDuiLie.ListIsRepeat(tiezi => huiFu.Tid == NeiRongDaishan.Tid))
                                    {
                                        DaiShanDuiLie.Add(NeiRongDaishan);
                                    }
                                    goto tiaoGuoZhuTi;//跳过这个主题帖
                                }
                                else
                                {
                                    NeiRongDaishan.LeiXing = ShanTieLeiXing.ShanHuiFu;
                                    if (!DaiShanDuiLie.ListIsRepeat(tiezi => huiFu.Pid == NeiRongDaishan.Pid))
                                    {
                                        DaiShanDuiLie.Add(NeiRongDaishan);
                                    }
                                    goto tiaoGuoNeiRong;//去下一个内容
                                }
                            }
                        }

                        //判断帖子内容
                        foreach (var neiRongGuanJianCi in GuanJianCi.NeiRong)
                        {
                            string guanJianCi = neiRongGuanJianCi;
                            if (string.IsNullOrEmpty(guanJianCi))
                            {
                                continue;
                            }

                            //变量替换，临时功能
                            if (guanJianCi.Contains("{BiaoTi}"))//暂时先这么写
                            {
                                if (huiFu.LouCeng > 1)//不包含1楼的回复，不支持楼中楼
                                {
                                    guanJianCi = guanJianCi.Replace("{BiaoTi}", huiFu.BiaoTi);
                                }
                            }

                            if (huiFu.NeiRong.Text.Contains(guanJianCi))
                            {
                                NeiRongDaishan.YuanYin = "关键词:" + guanJianCi;
                                if (huiFu.LouCeng == 1)
                                {
                                    NeiRongDaishan.LeiXing = ShanTieLeiXing.ShanZhuTi;
                                    if (!DaiShanDuiLie.ListIsRepeat(tiezi => huiFu.Tid == NeiRongDaishan.Tid))
                                    {
                                        DaiShanDuiLie.Add(NeiRongDaishan);
                                        goto tiaoGuoZhuTi;//跳过这个主题帖
                                    }
                                }
                                else
                                {
                                    NeiRongDaishan.LeiXing = ShanTieLeiXing.ShanHuiFu;
                                    if (!DaiShanDuiLie.ListIsRepeat(tiezi => huiFu.Pid == NeiRongDaishan.Pid))
                                    {
                                        DaiShanDuiLie.Add(NeiRongDaishan);
                                        goto tiaoGuoNeiRong;//去下一个回复
                                    }
                                }
                            }
                        }

                    //楼中楼
                    tiaolzl:;
                        if (YunXingCanShu.TiaoGuoLzl)
                        {
                            goto tiaoGuoNeiRong;
                        }

                        //先判断当前获取到的回复数
                        int dangQianLzlHuiFuShu = huiFu.LzlHuiFuShu;
                        if (dangQianLzlHuiFuShu > 0)
                        {
                            int HuanCunLzlHuifushu = 0;
                            for (int i = 0; i < HuanCun.LzlHuanCunLieBiao.Count; i++)
                            {
                                if (HuanCun.LzlHuanCunLieBiao[i].Pid == huiFu.Pid)
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
                                    Pid = huiFu.Pid,
                                    HuiFuShu = dangQianLzlHuiFuShu,
                                    GengXinShiJian = 0
                                });
                            }
                            else if (dangQianLzlHuiFuShu == HuanCunLzlHuifushu)
                            {
                                //如果当前与缓存相等
                                goto tiaoGuoNeiRong;
                            }

                            TiebaLouZhongLou louZhongLou = new TiebaLouZhongLou(tiebaName)
                            {
                                Cookie = cookie,
                                Tid = huiFu.Tid,
                                Pid = huiFu.Pid
                            };

                            List<TiebaLouZhongLou.JieGou> louZhongLouLieBiao = louZhongLou.Get(1);
                            foreach (var lzl in louZhongLouLieBiao)
                            {
                                //用户白名单
                                foreach (var baiMingDan in GuanJianCi.BaiMingDan)
                                {
                                    if (string.IsNullOrEmpty(baiMingDan))
                                    {
                                        continue;
                                    }

                                    if (lzl.YongHuMing == baiMingDan)
                                    {
                                        goto lzlTiaoGuoNeiRong;//去下一个楼中楼
                                    }
                                }

                                //楼中楼待删
                                DaiShanDuiLieJieGou lzlDaishan = new DaiShanDuiLieJieGou
                                {
                                    BiaoTi = huiFu.BiaoTi,
                                    Tid = lzl.Tid,
                                    YongHuMing = lzl.YongHuMing,
                                    FaTieShiJian = lzl.FaTieShiJianChuo,
                                    LeiXing = ShanTieLeiXing.ShanLzl,
                                    Pid = lzl.Pid,
                                    Spid = lzl.Spid,
                                    LouCeng = lzl.LouCeng //楼中楼没有楼层
                                };

                                //楼中楼等级墙
                                if (dengJiQiangYunXing && YunXingCanShu.QiYongHuiFuDengJiQiang)
                                {
                                    if (lzl.DengJi < YunXingCanShu.HuiFuDengJiQiang)
                                    {
                                        if (FaYanShiJianPanDuan(lzl.FaTieShiJianChuo, YunXingCanShu.DengJiQiangKaiShiShiJian, YunXingCanShu.DengJiQiangJieShuShiJian))
                                        {
                                            lzlDaishan.YuanYin = "楼中楼等级墙:" + lzl.YongHuMing + "(" + lzl.DengJi + ")";
                                            if (!DaiShanDuiLie.ListIsRepeat(tiezi => lzl.Spid == lzlDaishan.Spid))
                                            {
                                                DaiShanDuiLie.Add(lzlDaishan);
                                            }
                                            goto lzlTiaoGuoNeiRong;//去下一个楼中楼
                                        }
                                    }
                                }

                                //用户名关键词
                                foreach (var heiMingDan in GuanJianCi.HeiMingDan)
                                {
                                    if (string.IsNullOrEmpty(heiMingDan))
                                    {
                                        continue;
                                    }

                                    if (lzl.YongHuMing.Contains(heiMingDan))
                                    {
                                        lzlDaishan.YuanYin = "用户名关键词:" + lzl.YongHuMing;
                                        if (!DaiShanDuiLie.ListIsRepeat(tiezi => lzl.Spid == lzlDaishan.Spid))
                                        {
                                            DaiShanDuiLie.Add(lzlDaishan);
                                        }
                                        goto lzlTiaoGuoNeiRong;//去下一个楼中楼
                                    }
                                }

                                //判断帖子内容
                                foreach (var neiRongGuanJianCi in GuanJianCi.NeiRong)
                                {
                                    if (string.IsNullOrEmpty(neiRongGuanJianCi))
                                    {
                                        continue;
                                    }

                                    if (lzl.NeiRong.Text.Contains(neiRongGuanJianCi))
                                    {
                                        lzlDaishan.YuanYin = "关键词:" + neiRongGuanJianCi;
                                        if (!DaiShanDuiLie.ListIsRepeat(tiezi => lzl.Spid == lzlDaishan.Spid))
                                        {
                                            DaiShanDuiLie.Add(lzlDaishan);
                                            goto lzlTiaoGuoNeiRong;//去下一个楼中楼
                                        }
                                    }
                                }

                            lzlTiaoGuoNeiRong:;//去下一个楼中楼的标记
                            }

                            //更新
                            for (int i = 0; i < HuanCun.LzlHuanCunLieBiao.Count; i++)
                            {
                                if (HuanCun.LzlHuanCunLieBiao[i].Pid == huiFu.Pid)
                                {
                                    HuanCun.LzlHuanCunLieBiao[i].HuiFuShu = dangQianLzlHuiFuShu;
                                    HuanCun.LzlHuanCunLieBiao[i].GengXinShiJian = Convert.ToInt64(BST.QuShiJianChuo());
                                    break;
                                }
                            }
                        }

                    tiaoGuoNeiRong:;//去下一个回复的标记
                    }

                tiaoGuoZhuTi:;//下个帖子的标记
                }

                stopwatch.Stop();
                string yongshi = stopwatch.Elapsed.ToString();
                if (!YunXingCanShu.ZhiShuChuShanTieRiZhi)
                {
                    Say("本轮扫描结束，用时: " + yongshi.Substring(0, yongshi.IndexOf(".")));
                }
                toolStripStatusLabel1.Text = "本轮扫描结束，等待下一轮扫描...";
                DengDai(YunXingCanShu.SaoMiaoJianGe);
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

        #endregion

        #region 待删队列线程

        /// <summary>
        /// 待删队列
        /// </summary>
        public List<DaiShanDuiLieJieGou> DaiShanDuiLie = new List<DaiShanDuiLieJieGou>();

        /// <summary>
        /// 删帖队列
        /// </summary>
        public void ShanTie()
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
                    TiebaBaWu bawu = new TiebaBaWu
                    {
                        Cookie = YunXingCanShu.Cookie,
                        TiebaName = YunXingCanShu.TiebaName,
                        Fid = YunXingCanShu.Fid,
                        Tid = daiShanTieZi.Tid.ToString(),
                        Pid = daiShanTieZi.Pid.ToString()
                    };

                    if (daiShanTieZi.LeiXing == ShanTieLeiXing.ShanZhuTi)
                    {
                        if (bawu.ShanZhuTi(out string msg))
                        {
                            Say("删除 tid:" + daiShanTieZi.Tid.ToString() + " " + daiShanTieZi.LouCeng.ToString() + "楼 " + daiShanTieZi.YuanYin + " 成功 用时:" + ShanTieYongShi(daiShanTieZi.FaTieShiJian));
                        }
                        else
                        {
                            Say("删除 tid:" + daiShanTieZi.Tid.ToString() + " " + daiShanTieZi.LouCeng.ToString() + "楼 " + daiShanTieZi.YuanYin + " 失败：" + msg);
                        }
                    }
                    else if (daiShanTieZi.LeiXing == ShanTieLeiXing.ShanHuiFu)
                    {
                        if (bawu.ShanHuiFu(out string msg))
                        {
                            Say("删除 tid:" + daiShanTieZi.Tid.ToString() + " " + daiShanTieZi.LouCeng.ToString() + "楼 " + daiShanTieZi.YuanYin + " 成功 用时:" + ShanTieYongShi(daiShanTieZi.FaTieShiJian));
                        }
                        else
                        {
                            Say("删除 tid:" + daiShanTieZi.Tid.ToString() + " " + daiShanTieZi.LouCeng.ToString() + "楼 " + daiShanTieZi.YuanYin + " 失败：" + msg);
                        }
                    }
                    else if (daiShanTieZi.LeiXing == ShanTieLeiXing.ShanLzl)
                    {
                        bawu.Pid = daiShanTieZi.Spid.ToString();

                        if (bawu.ShanHuiFu(out string msg, true))
                        {
                            Say("删除 tid:" + daiShanTieZi.Tid.ToString() + " " + daiShanTieZi.LouCeng.ToString() + "楼lzl " + daiShanTieZi.YuanYin + " 成功 用时:" + ShanTieYongShi(daiShanTieZi.FaTieShiJian));
                        }
                        else
                        {
                            Say("删除 tid:" + daiShanTieZi.Tid.ToString() + " " + daiShanTieZi.LouCeng.ToString() + "楼lzl " + daiShanTieZi.YuanYin + " 失败：" + msg);
                        }
                    }

                    //从队列里删掉，不管有没有删成功
                    DaiShanDuiLie.RemoveAt(0);

                    //删帖间隔
                    DengDai(YunXingCanShu.ShanTieJianGe);
                }

                //队列循环限速1秒
                DengDai(1);
            }

        //结束线程
        exit:;
        }

        #endregion

        #region 操作量查询线程

        /// <summary>
        /// 操作量查询
        /// </summary>
        public void CaoZuoLiangChaXun()
        {
            toolStripStatusLabel2.Text = string.Empty;

            //    while (true)
            //    {
            //        if (QuanJu.Stop)
            //        {
            //            goto exit;
            //        }

            //        //查询
            //        TiebaBaWu bawu = new TiebaBaWu()
            //        {
            //            Cookie = YunXingCanShu.Cookie,
            //            TiebaName = YunXingCanShu.TiebaName
            //        };

            //        //操作量
            //        int caoZuoLiang = bawu.CaoZuoLiangChaXun(YunXingCanShu.YongHuMing, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"));

            //        //输出
            //        toolStripStatusLabel2.Text = String.Format("操作量 {0}/3000", caoZuoLiang);

            //        //查询
            //        for (int i = 0; i < 180; i++)//180秒=3分钟
            //        {
            //            if (QuanJu.Stop)
            //            {
            //                goto exit;
            //            }

            //            DengDai(1); //1秒
            //        }
            //    }

            //exit:;
        }

        #endregion;

        #region 帖子浏览器

        /// <summary>
        /// 帖子浏览器 读取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_liuLanQi_duQu_Click(object sender, EventArgs e)
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

            bool isDaoXu = false;
            if (checkBox_liuLanQi_daoXuChaKan.Checked)
            {
                isDaoXu = true;
            }

            TiebaHuiFu tiebaHuiFu = new TiebaHuiFu(YunXingCanShu.TiebaName)
            {
                Cookie = YunXingCanShu.Cookie,
                Tid = Convert.ToInt64(textBox_liuLanQi_tieHao.Text),
                IsDaoXu = isDaoXu
            };

            List<TiebaHuiFu.JieGou> huiFuLieBiao = tiebaHuiFu.Get(Convert.ToInt32(textBox_liuLanQi_yeMa.Text));
            if (huiFuLieBiao.Count == 0)
            {
                MessageBox.Show(text: " 没有获取到回复", caption: "笨蛋雪说：", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Asterisk);
                return;
            }

            listView_liuLanQi_liuLanQi.Items.Clear();
            listView_liuLanQi_liuLanQi.BeginUpdate();
            foreach (TiebaHuiFu.JieGou huiFu in huiFuLieBiao)
            {
                ListViewItem lvi = new ListViewItem()
                {
                    Text = Convert.ToString(huiFu.LouCeng)
                };

                lvi.SubItems.Add(huiFu.BiaoTi);
                lvi.SubItems.Add(huiFu.NeiRong.Text);
                lvi.SubItems.Add(huiFu.YongHuMing);
                lvi.SubItems.Add(BST.ShiJianChuoDaoWenben(huiFu.FaTieShiJianChuo * 1000));
                listView_liuLanQi_liuLanQi.Items.Add(lvi);
            }
            listView_liuLanQi_liuLanQi.EndUpdate();
        }

        /// <summary>
        /// 帖子浏览器 表项被单击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_liuLanQi_liuLanQi_Click(object sender, EventArgs e)
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

        #endregion;

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
}