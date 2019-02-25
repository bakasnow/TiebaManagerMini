namespace TiebaManagerMini
{
    class QuanJu
    {
        //版本号
        public const string Vname = "tmm";
        public const string Version = "1.6.0(190201)";

        //运行参数
        public static bool Stop = true;//主线程控制

        //基本参数
        public static int SaoMiaoJianGe = 1;
        public static int ShanTieJianGe = 1;
        public static bool TiaoGuoZhiDing = false;
        public static bool TiaoGuoJingPin = false;
        public static bool TiaoGuoLzl = false;
        public static bool ZhiShuChuShanTieRiZhi = false;
        public static bool WenBenGuoLv = false;

        //等级墙
        public static bool QiYongZhuTiDengJiQiang = false;
        public static int ZhuTiDengJiQiang = 0;
        public static bool QiYongHuiFuDengJiQiang = false;
        public static int HuiFuDengJiQiang = 0;
        public static string DengJiQiangKaiShiShiJian = string.Empty;
        public static string DengJiQiangJieShuShiJian = string.Empty;

        //等级限制
        public static bool QiYongBiaoTiDengJi = false;
        public static int BiaoTiDengJi = 0;
        public static bool QiYongNeiRongDengJi = false;
        public static int NeiRongDengJi = 0;

        //全局参数
        public static string Cookie = string.Empty;
        public static string YongHuMing = string.Empty;
        public static string TiebaName = string.Empty;
        public static string Fid = string.Empty;
    }
}
