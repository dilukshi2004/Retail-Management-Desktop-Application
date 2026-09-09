namespace RetailApp.Forms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnProducts = new Button();
            btnSales = new Button();
            btnStock = new Button();
            btnExit = new Button();
            btnHistory = new Button();
            this.btnReport = new Button();
            SuspendLayout();
            // 
            // btnProducts
            // 
            btnProducts.Location = new Point(97, 93);
            btnProducts.Name = "btnProducts";
            btnProducts.Size = new Size(228, 29);
            btnProducts.TabIndex = 0;
            btnProducts.Text = "Product Management";
            btnProducts.UseVisualStyleBackColor = true;
            btnProducts.Click += btnProducts_Click;
            // 
            // btnSales
            // 
            btnSales.Location = new Point(792, 299);
            btnSales.Name = "btnSales";
            btnSales.Size = new Size(209, 29);
            btnSales.TabIndex = 1;
            btnSales.Text = "Sales";
            btnSales.UseVisualStyleBackColor = true;
            btnSales.Click += btnSales_Click;
            // 
            // btnStock
            // 
            btnStock.Location = new Point(792, 93);
            btnStock.Name = "btnStock";
            btnStock.Size = new Size(209, 29);
            btnStock.TabIndex = 2;
            btnStock.Text = "Stock Management";
            btnStock.UseVisualStyleBackColor = true;
            btnStock.Click += btnStock_Click;
            // 
            // btnExit
            // 
            btnExit.Location = new Point(968, 440);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(94, 29);
            btnExit.TabIndex = 3;
            btnExit.Text = "Exit";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // btnHistory
            // 
            btnHistory.Location = new Point(97, 299);
            btnHistory.Name = "btnHistory";
            btnHistory.Size = new Size(228, 29);
            btnHistory.TabIndex = 4;
            btnHistory.Text = "Transaction History";
            btnHistory.UseVisualStyleBackColor = true;
            btnHistory.Click += btnHistory_Click;

            // 
            // btnReport
            // 
            this.btnReport.Location = new Point(500, 260);
            this.btnReport.Name = "btnReport";
            this.btnReport.Size = new Size(150, 29);
            this.btnReport.TabIndex = 5;
            this.btnReport.Text = "Sales Report";
            this.btnReport.UseVisualStyleBackColor = true;
            this.btnReport.Click += this.btnReport_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1363, 510);
            Controls.Add(btnHistory);
            Controls.Add(btnExit);
            Controls.Add(btnStock);
            Controls.Add(btnSales);
            Controls.Add(btnProducts);
            Controls.Add(this.btnReport);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Retail Inventory & Sales System";
            ResumeLayout(false);
        }

        #endregion

        private Button btnProducts;
        private Button btnSales;
        private Button btnStock;
        private Button btnExit;
        private Button btnHistory;
        private Button btnReport;
    }
}
