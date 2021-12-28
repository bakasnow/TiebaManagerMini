namespace TiebaManagerMini.WinForm
{
    partial class TiebaZhangHaoGuanLiForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TiebaZhangHaoGuanLiForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listView_zhangHao_zhangHaoLieBiao = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_zhangHao_jianChaDiaoXian = new System.Windows.Forms.Button();
            this.button_zhangHao_shanChuZhangHao = new System.Windows.Forms.Button();
            this.button_zhangHao_tianJiaZhangHao = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.listView_zhangHao_zhangHaoLieBiao);
            this.groupBox1.Controls.Add(this.button_zhangHao_jianChaDiaoXian);
            this.groupBox1.Controls.Add(this.button_zhangHao_shanChuZhangHao);
            this.groupBox1.Controls.Add(this.button_zhangHao_tianJiaZhangHao);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(272, 305);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "账号列表";
            // 
            // listView_zhangHao_zhangHaoLieBiao
            // 
            this.listView_zhangHao_zhangHaoLieBiao.CheckBoxes = true;
            this.listView_zhangHao_zhangHaoLieBiao.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView_zhangHao_zhangHaoLieBiao.FullRowSelect = true;
            this.listView_zhangHao_zhangHaoLieBiao.HideSelection = false;
            this.listView_zhangHao_zhangHaoLieBiao.Location = new System.Drawing.Point(6, 17);
            this.listView_zhangHao_zhangHaoLieBiao.Name = "listView_zhangHao_zhangHaoLieBiao";
            this.listView_zhangHao_zhangHaoLieBiao.Size = new System.Drawing.Size(258, 251);
            this.listView_zhangHao_zhangHaoLieBiao.TabIndex = 5;
            this.listView_zhangHao_zhangHaoLieBiao.UseCompatibleStateImageBehavior = false;
            this.listView_zhangHao_zhangHaoLieBiao.View = System.Windows.Forms.View.Details;
            this.listView_zhangHao_zhangHaoLieBiao.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView_zhangHao_zhangHaoLieBiao_ItemChecked);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "是否在线";
            this.columnHeader1.Width = 70;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "用户名";
            this.columnHeader2.Width = 160;
            // 
            // button_zhangHao_jianChaDiaoXian
            // 
            this.button_zhangHao_jianChaDiaoXian.Enabled = false;
            this.button_zhangHao_jianChaDiaoXian.Location = new System.Drawing.Point(174, 274);
            this.button_zhangHao_jianChaDiaoXian.Name = "button_zhangHao_jianChaDiaoXian";
            this.button_zhangHao_jianChaDiaoXian.Size = new System.Drawing.Size(90, 25);
            this.button_zhangHao_jianChaDiaoXian.TabIndex = 4;
            this.button_zhangHao_jianChaDiaoXian.Text = "批量检查掉线";
            this.button_zhangHao_jianChaDiaoXian.UseVisualStyleBackColor = true;
            this.button_zhangHao_jianChaDiaoXian.Click += new System.EventHandler(this.button_zhangHao_jianChaDiaoXian_Click);
            // 
            // button_zhangHao_shanChuZhangHao
            // 
            this.button_zhangHao_shanChuZhangHao.Location = new System.Drawing.Point(62, 274);
            this.button_zhangHao_shanChuZhangHao.Name = "button_zhangHao_shanChuZhangHao";
            this.button_zhangHao_shanChuZhangHao.Size = new System.Drawing.Size(50, 25);
            this.button_zhangHao_shanChuZhangHao.TabIndex = 3;
            this.button_zhangHao_shanChuZhangHao.Text = "- 删除";
            this.button_zhangHao_shanChuZhangHao.UseVisualStyleBackColor = true;
            this.button_zhangHao_shanChuZhangHao.Click += new System.EventHandler(this.button_zhangHao_shanChuZhangHao_Click);
            // 
            // button_zhangHao_tianJiaZhangHao
            // 
            this.button_zhangHao_tianJiaZhangHao.Location = new System.Drawing.Point(6, 274);
            this.button_zhangHao_tianJiaZhangHao.Name = "button_zhangHao_tianJiaZhangHao";
            this.button_zhangHao_tianJiaZhangHao.Size = new System.Drawing.Size(50, 25);
            this.button_zhangHao_tianJiaZhangHao.TabIndex = 2;
            this.button_zhangHao_tianJiaZhangHao.Text = "+ 登录";
            this.button_zhangHao_tianJiaZhangHao.UseVisualStyleBackColor = true;
            this.button_zhangHao_tianJiaZhangHao.Click += new System.EventHandler(this.button_zhangHao_tianJiaZhangHao_Click);
            // 
            // TiebaZhangHaoGuanLiForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 326);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TiebaZhangHaoGuanLiForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.TiebaZhangHaoGuanLiForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView listView_zhangHao_zhangHaoLieBiao;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button button_zhangHao_jianChaDiaoXian;
        private System.Windows.Forms.Button button_zhangHao_shanChuZhangHao;
        private System.Windows.Forms.Button button_zhangHao_tianJiaZhangHao;
    }
}