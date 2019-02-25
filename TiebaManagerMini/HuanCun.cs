using System;
using System.Collections.Generic;

using BakaSnowTool;
using TiebaLib;

namespace TiebaManagerMini
{
    class HuanCun
    {
        /// <summary>
        /// 主题缓存
        /// </summary>
        public static List<ZhuTiHuanCunJieGou> ZhuTiHuanCunLieBiao = new List<ZhuTiHuanCunJieGou>();

        /// <summary>
        /// 楼中楼缓存
        /// </summary>
        public static List<LzlHuanCunJieGou> LzlHuanCunLieBiao = new List<LzlHuanCunJieGou>();

        /// <summary>
        /// 主题等级缓存
        /// </summary>
        public static List<DengJiHuanCunJieGou> DengJiHuanCunLieBiao = new List<DengJiHuanCunJieGou>();

        /// <summary>
        /// 主题缓存结构
        /// </summary>
        public class ZhuTiHuanCunJieGou
        {
            public long Tid;
            public long FaTieShiJian;
            public long ZuiHouHuiFuShiJian;
            public long GengXinShiJian;
        }

        /// <summary>
        /// 楼中楼缓存结构
        /// </summary>
        public class LzlHuanCunJieGou
        {
            public long Pid;
            public int HuiFuShu;
            public long GengXinShiJian;
        }

        /// <summary>
        /// 等级缓存结构
        /// </summary>
        public class DengJiHuanCunJieGou
        {
            public string TiebaName;
            public string YongHuMing;
            public int DengJi;
            public long GengXinShiJian;
        }

        /// <summary>
        /// 获取主题等级缓存
        /// </summary>
        /// <param name="yongHuMing">用户名</param>
        /// <param name="tiebaName">贴吧名</param>
        /// <returns></returns>
        public static int GetDengJiHuanCun(string yongHuMing, string tiebaName)
        {
            //先在缓存里找
            int DengJi = -1;
            for (int i = 0; i < DengJiHuanCunLieBiao.Count; i++)
            {
                if (DengJiHuanCunLieBiao[i].TiebaName == tiebaName && DengJiHuanCunLieBiao[i].YongHuMing == yongHuMing)
                {
                    DengJi = DengJiHuanCunLieBiao[i].DengJi;
                    DengJiHuanCunLieBiao[i].GengXinShiJian = Convert.ToInt64(BST.QuShiJianChuo());
                    return DengJi;
                }
            }

            //没有再获取
            DengJi = Tieba.GetYongHuTiebaDengJi(yongHuMing, tiebaName);
            DengJiHuanCunLieBiao.Add(new DengJiHuanCunJieGou()
            {
                TiebaName = tiebaName,
                YongHuMing = yongHuMing,
                DengJi = DengJi,
                GengXinShiJian = 0
            });
            return DengJi;
        }
    }
}
