namespace ImFormsUser
{
    partial class ImFormsUser
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
            this.ParentPanel = new System.Windows.Forms.TableLayoutPanel();
            this.Panel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.Panel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.yIncrBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.refreshBtn = new System.Windows.Forms.Button();
            this.Panel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.ParentPanel.SuspendLayout();
            this.Panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // ParentPanel
            // 
            this.ParentPanel.ColumnCount = 2;
            this.ParentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ParentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ParentPanel.Controls.Add(this.Panel2, 1, 0);
            this.ParentPanel.Controls.Add(this.Panel4, 1, 1);
            this.ParentPanel.Controls.Add(this.Panel1, 0, 0);
            this.ParentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ParentPanel.Location = new System.Drawing.Point(0, 0);
            this.ParentPanel.Name = "ParentPanel";
            this.ParentPanel.RowCount = 2;
            this.ParentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45.21073F));
            this.ParentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 54.78927F));
            this.ParentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ParentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ParentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ParentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ParentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ParentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ParentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ParentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.ParentPanel.Size = new System.Drawing.Size(560, 582);
            this.ParentPanel.TabIndex = 0;
            // 
            // Panel2
            // 
            this.Panel2.AutoScroll = true;
            this.Panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.Panel2.Location = new System.Drawing.Point(283, 3);
            this.Panel2.Name = "Panel2";
            this.Panel2.Size = new System.Drawing.Size(274, 257);
            this.Panel2.TabIndex = 1;
            // 
            // Panel4
            // 
            this.Panel4.AutoScroll = true;
            this.Panel4.Controls.Add(this.label1);
            this.Panel4.Controls.Add(this.yIncrBtn);
            this.Panel4.Controls.Add(this.label2);
            this.Panel4.Controls.Add(this.refreshBtn);
            this.Panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel4.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.Panel4.Location = new System.Drawing.Point(283, 266);
            this.Panel4.Name = "Panel4";
            this.Panel4.Size = new System.Drawing.Size(274, 313);
            this.Panel4.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(256, 104);
            this.label1.TabIndex = 3;
            this.label1.Text = "Everything below here has been made through the normal Windows Forms Designer wit" +
    "h no hacky workarounds.\r\n\r\nBut!\r\n\r\nIt can modify values which are displayed in t" +
    "he other panels.\r\n";
            // 
            // yIncrBtn
            // 
            this.yIncrBtn.Location = new System.Drawing.Point(3, 107);
            this.yIncrBtn.Name = "yIncrBtn";
            this.yIncrBtn.Size = new System.Drawing.Size(75, 23);
            this.yIncrBtn.TabIndex = 0;
            this.yIncrBtn.Text = "y++";
            this.yIncrBtn.UseVisualStyleBackColor = true;
            this.yIncrBtn.Click += new System.EventHandler(this.yIncrBtn_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 133);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(268, 26);
            this.label2.TabIndex = 4;
            this.label2.Text = "ImForms panels can also be made to refresh via outisde user interactions.";
            // 
            // refreshBtn
            // 
            this.refreshBtn.Location = new System.Drawing.Point(3, 162);
            this.refreshBtn.Name = "refreshBtn";
            this.refreshBtn.Size = new System.Drawing.Size(148, 23);
            this.refreshBtn.TabIndex = 1;
            this.refreshBtn.Text = "Refresh Right-hand Panel";
            this.refreshBtn.UseVisualStyleBackColor = true;
            this.refreshBtn.Click += new System.EventHandler(this.refreshBtn_Click);
            // 
            // Panel1
            // 
            this.Panel1.AutoScroll = true;
            this.Panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.Panel1.Location = new System.Drawing.Point(3, 3);
            this.Panel1.Name = "Panel1";
            this.ParentPanel.SetRowSpan(this.Panel1, 2);
            this.Panel1.Size = new System.Drawing.Size(274, 576);
            this.Panel1.TabIndex = 0;
            // 
            // ImFormsUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(560, 582);
            this.Controls.Add(this.ParentPanel);
            this.Name = "ImFormsUser";
            this.Text = "ImForms Demo";
            this.ParentPanel.ResumeLayout(false);
            this.Panel4.ResumeLayout(false);
            this.Panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel ParentPanel;
        private System.Windows.Forms.FlowLayoutPanel Panel2;
        private System.Windows.Forms.FlowLayoutPanel Panel4;
        private System.Windows.Forms.Button yIncrBtn;
        private System.Windows.Forms.Button refreshBtn;
        private System.Windows.Forms.FlowLayoutPanel Panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

