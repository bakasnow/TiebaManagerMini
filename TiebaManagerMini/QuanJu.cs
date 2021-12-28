using System.Collections.Generic;

namespace TiebaManagerMini
{
    public class QuanJu
    {
        //版本号
        public const string Vname = "tmm";
        public const string Version = "1.5.2.200711";

        //运行参数
        public static bool Stop = true;//主线程控制
    }

    public class YunXingCanShu
    {
        //运行参数
        public static int SaoMiaoJianGe = 1;
        public static int ShanTieJianGe = 1;
        public static bool TiaoGuoZhiDing = false;
        public static bool TiaoGuoJingPin = false;
        public static bool TiaoGuoLzl = false;
        public static bool ZhiShuChuShanTieRiZhi = false;

        //等级墙
        public static bool QiYongZhuTiDengJiQiang = false;
        public static int ZhuTiDengJiQiang = 0;
        public static bool QiYongHuiFuDengJiQiang = false;
        public static int HuiFuDengJiQiang = 0;
        public static string DengJiQiangKaiShiShiJian = string.Empty;
        public static string DengJiQiangJieShuShiJian = string.Empty;

        //全局参数
        public static string TiebaName = string.Empty;
        public static string Fid = string.Empty;
    }

    /// <summary>
    /// 关键词
    /// </summary>
    public static class GuanJianCi
    {
        public static string[] BiaoTi = new string[0];//标题关键词
        public static string[] NeiRong = new string[0];//内容关键词
        public static string[] HeiMingDan = new string[0];//用户黑名单
        public static string[] BaiMingDan = new string[0];//用户白名单
    }
}
