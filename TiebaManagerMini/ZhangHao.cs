using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiebaManagerMini
{
    public class ZhangHao
    {
        private readonly IniHelper ini = null;

        public bool ExistIniFile { get { return ini.ExistIniFile(); } }

        public ZhangHao(string path)
        {
            ini = new IniHelper(path);
        }


    }
}
