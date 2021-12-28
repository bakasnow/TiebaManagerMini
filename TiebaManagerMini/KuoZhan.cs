using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using BakaSnowTool;

namespace TiebaManagerMini
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public static class KuoZhan
    {
        /// <summary>
        /// 等待
        /// </summary>
        public static void DengDai(int s)
        {
            for (int i = 0; i < s; i++)
            {
                if (QuanJu.Stop)
                {
                    return;
                }

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 时间判断
        /// </summary>
        /// <param name="nowTime">当前时间</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="stopTime">结束时间</param>
        /// <returns></returns>
        private static bool ShiJianPanDuan(DateTime nowTime, DateTime startTime, DateTime stopTime)
        {
            if (nowTime >= startTime && nowTime <= stopTime)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 等级墙时间判断
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="stopTime">结束时间</param>
        /// <returns></returns>
        public static bool DengJiQiangShiJianPanDuan(string startTime, string stopTime)
        {
            DateTime dangqianSj = DateTime.Now;
            DateTime kaishiSj = Convert.ToDateTime(DateTime.Now.ToString($"yyyy-MM-dd {startTime}:00"));
            DateTime jieshuSj;

            if (Convert.ToInt32(startTime.Replace(":", "")) <= Convert.ToInt32(stopTime.Replace(":", "")))
            {
                jieshuSj = Convert.ToDateTime(DateTime.Now.ToString($"yyyy-MM-dd {stopTime}:00"));
            }
            else
            {
                jieshuSj = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString($"yyyy-MM-dd {stopTime}:00"));
            }

            return ShiJianPanDuan(dangqianSj, kaishiSj, jieshuSj);
        }

        /// <summary>
        /// 发言时间判断
        /// </summary>
        /// <param name="shijianchuo">时间戳</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="stopTime">结束时间</param>
        /// <returns></returns>
        public static bool FaYanShiJianPanDuan(long shijianchuo, string startTime, string stopTime)
        {
            DateTime dangqianSj = Convert.ToDateTime(BST.ShiJianChuoDaoWenben(shijianchuo * 1000, "yyyy-MM-dd HH:mm:ss"));
            DateTime kaishiSj = Convert.ToDateTime(DateTime.Now.ToString($"yyyy-MM-dd {startTime}:00"));
            DateTime jieshuSj;

            if (Convert.ToInt32(startTime.Replace(":", "")) <= Convert.ToInt32(stopTime.Replace(":", "")))
            {
                jieshuSj = Convert.ToDateTime(DateTime.Now.ToString($"yyyy-MM-dd {stopTime}:00"));
            }
            else
            {
                jieshuSj = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString($"yyyy-MM-dd {stopTime}:00"));
            }

            return ShiJianPanDuan(dangqianSj, kaishiSj, jieshuSj);
        }

        /// <summary>
        /// 是否重复
        /// </summary>
        /// <param name="dt">表</param>
        /// <param name="dr">行</param>
        /// <param name="column">列名</param>
        public static bool DataRowIsRepeat(DataTable dt, DataRow dr, string column)
        {
            foreach (DataRow row in dt.Rows)
            {
                if (row[column] == dr[column])
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// List扩展方法 是否存在
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="func"></param>
        public static bool ListIsRepeat<T>(this List<T> list, Func<T, bool> func)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (func(list[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// List扩展方法 删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="func"></param>
        public static void RemoveWhen<T>(this List<T> list, Func<T, bool> func)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (func(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 获取运算符
        /// </summary>
        /// <param name="str">原文本</param>
        /// <param name="yunSuanFu">运算符</param>
        /// <param name="shuZi">数字</param>
        /// <returns></returns>
        public static bool GetYunSuanFu(string str, out string yunSuanFu, out int shuZi)
        {
            MatchCollection mc = new Regex("^(<|>|=|>=|<=|!=)([0-9]{1,2})$").Matches(str);
            if (mc.Count > 0)
            {
                yunSuanFu = mc[0].Groups[0].Value;
                shuZi = Convert.ToInt32(mc[0].Groups[1].Value);
                return true;
            }
            else
            {
                yunSuanFu = string.Empty;
                shuZi = 0;
                return false;
            }
        }

        /// <summary>
        /// 删帖用时
        /// </summary>
        /// <param name="sjc">时间戳</param>
        /// <returns></returns>
        public static string ShanTieYongShi(long sjc)
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
                yongshi /= 60;
                if (yongshi < 60)
                {
                    shuchuStr += yongshi.ToString() + "分";
                }
                else
                {
                    yongshi /= 60;
                    if (yongshi < 24)
                    {
                        shuchuStr += yongshi.ToString() + "时";
                    }
                    else
                    {
                        yongshi /= 24;
                        shuchuStr += yongshi.ToString() + "天";
                    }
                }
            }

            return shuchuStr;
        }
    }
}
