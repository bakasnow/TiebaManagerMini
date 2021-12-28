using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace TiebaManagerMini
{
    public class PeiZhi
    {
        private readonly IniHelper ini = null;

        public bool ExistIniFile { get { return ini.ExistIniFile(); } }

        private readonly string Section;

        public PeiZhi(string path, string section)
        {
            Section = section;
            ini = new IniHelper(path);
        }

        public string TiebaName
        {
            get
            {
                return ini.ReadValue(Section, "贴吧名");
            }
            set
            {
                ini.WriteValue(Section, "贴吧名", value);
            }
        }

        public int SaoMiaoJianGe
        {
            get
            {
                if (int.TryParse(ini.ReadValue(Section, "扫描间隔"), out int result))
                {
                    return result;
                }
                else
                {
                    return 1;
                }
            }
            set
            {
                ini.WriteValue(Section, "扫描间隔", Convert.ToString(value));
            }
        }

        public int ShanTieJianGe
        {
            get
            {
                if (int.TryParse(ini.ReadValue(Section, "删帖间隔"), out int result))
                {
                    return result;
                }
                else
                {
                    return 1;
                }
            }
            set
            {
                ini.WriteValue(Section, "删帖间隔", Convert.ToString(value));
            }
        }

        public bool TiaoGuoZhiDing
        {
            get
            {
                return ini.ReadValue(Section, "跳过置顶") == "1";
            }
            set
            {
                ini.WriteValue(Section, "跳过置顶", value ? "1" : "0");
            }
        }

        public bool TiaoGuoJingPin
        {
            get
            {
                return ini.ReadValue(Section, "跳过精品") == "1";
            }
            set
            {
                ini.WriteValue(Section, "跳过精品", value ? "1" : "0");
            }
        }

        public bool TiaoGuoLouZhongLou
        {
            get
            {
                return ini.ReadValue(Section, "跳过楼中楼") == "1";
            }
            set
            {
                ini.WriteValue(Section, "跳过楼中楼", value ? "1" : "0");
            }
        }

        public bool ZhiShuChuShanTieRiZhi
        {
            get
            {
                return ini.ReadValue(Section, "只输出删帖日志") == "1";
            }
            set
            {
                ini.WriteValue(Section, "只输出删帖日志", value ? "1" : "0");
            }
        }

        //public bool KaiQiWenBenGuoLv
        //{
        //    get
        //    {
        //        return ini.ReadValue(Section, "开启文本过滤") == "1";
        //    }
        //    set
        //    {
        //        ini.WriteValue(Section, "开启文本过滤", value ? "1" : "0");
        //    }
        //}

        public bool QiYongZhuTiDengJiQiang
        {
            get
            {
                return ini.ReadValue(Section, "启用主题等级墙") == "1";
            }
            set
            {
                ini.WriteValue(Section, "启用主题等级墙", value ? "1" : "0");
            }
        }

        public int ZhuTiDengJiQiang
        {
            get
            {
                if (int.TryParse(ini.ReadValue(Section, "主题等级墙"), out int result))
                {
                    return result;
                }
                else
                {
                    return 2;
                }
            }
            set
            {
                ini.WriteValue(Section, "主题等级墙", Convert.ToString(value));
            }
        }

        public bool QiYongHuiFuDengJiQiang
        {
            get
            {
                return ini.ReadValue(Section, "启用回复等级墙") == "1";
            }
            set
            {
                ini.WriteValue(Section, "启用回复等级墙", value ? "1" : "0");
            }
        }

        public int HuiFuDengJiQiang
        {
            get
            {
                if (int.TryParse(ini.ReadValue(Section, "回复等级墙"), out int result))
                {
                    return result;
                }
                else
                {
                    return 2;
                }
            }
            set
            {
                ini.WriteValue(Section, "回复等级墙", Convert.ToString(value));
            }
        }

        public string DengJiQiangKaiShiShiJian
        {
            get
            {
                return ini.ReadValue(Section, "等级墙开始时间");
            }
            set
            {
                ini.WriteValue(Section, "等级墙开始时间", value);
            }
        }

        public string DengJiQiangJieShuShiJian
        {
            get
            {
                return ini.ReadValue(Section, "等级墙结束时间");
            }
            set
            {
                ini.WriteValue(Section, "等级墙结束时间", value);
            }
        }
    }
}
