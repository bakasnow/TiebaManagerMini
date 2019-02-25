using System;
using System.Collections.Generic;
using System.Data;

using BakaSnowTool;

namespace TiebaManagerMini
{
    static class Lib
    {
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
            DateTime jieshuSj = new DateTime();

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
            DateTime jieshuSj = new DateTime();

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
    }
}
