namespace QueryBuilder.CustomControl
{
    partial class Table
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tvColumns = new System.Windows.Forms.TreeView();
            this.lblTableName = new QueryBuilder.CustomControl.TransparentLabel();
            this.SuspendLayout();
            // 
            // tvColumns
            // 
            this.tvColumns.Location = new System.Drawing.Point(0, 26);
            this.tvColumns.Name = "tvColumns";
            this.tvColumns.Size = new System.Drawing.Size(148, 165);
            this.tvColumns.TabIndex = 1;
            // 
            // lblTableName
            // 
            this.lblTableName.AutoSize = true;
            this.lblTableName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTableName.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblTableName.Location = new System.Drawing.Point(3, 6);
            this.lblTableName.Name = "lblTableName";
            this.lblTableName.Size = new System.Drawing.Size(124, 17);
            this.lblTableName.TabIndex = 2;
            this.lblTableName.Text = "transparentLabel1";
            // 
            // Table
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Controls.Add(this.lblTableName);
            this.Controls.Add(this.tvColumns);
            this.Name = "Table";
            this.Size = new System.Drawing.Size(148, 191);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView tvColumns;
        private TransparentLabel lblTableName;
    }
}
