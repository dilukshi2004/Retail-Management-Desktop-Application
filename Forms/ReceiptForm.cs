using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using RetailApp.Models;

namespace RetailApp.Forms
{
    public partial class ReceiptForm : Form
    {
        private readonly string receiptText;
        private readonly string invoiceNo;
        private TextBox txtReceipt;
        private Button btnSave, btnClose;

        public ReceiptForm(string invoiceNo, DateTime date, List<SaleItem> items, decimal total)
        {
            this.invoiceNo = invoiceNo;
            InitializeComponent();
            receiptText = BuildReceiptText(invoiceNo, date, items, total);
            BuildUi();
        }

        private string BuildReceiptText(string invoiceNo, DateTime date, List<SaleItem> items, decimal total)
        {
            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("        RETAIL APP - SALES RECEIPT");
            sb.AppendLine("========================================");
            sb.AppendLine($"Invoice No: {invoiceNo}");
            sb.AppendLine($"Date: {date:yyyy-MM-dd HH:mm}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"{"Product",-18}{"Qty",5}{"Price",8}{"Total",9}");
            foreach (var item in items)
            {
                string name = item.ProductName.Length > 18 ? item.ProductName.Substring(0, 17) + "…" : item.ProductName;
                sb.AppendLine($"{name,-18}{item.Quantity,5}{item.UnitPrice,8:0.00}{item.LineTotal,9:0.00}");
            }
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"TOTAL:{"",21}Rs. {total,9:0.00}");
            sb.AppendLine("========================================");
            sb.AppendLine("       Thank you for your purchase!");
            sb.AppendLine("========================================");
            return sb.ToString();
        }

        private void BuildUi()
        {
            this.Text = "Receipt";
            this.ClientSize = new Size(420, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            txtReceipt = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9.5f),
                Location = new Point(15, 15),
                Size = new Size(390, 380),
                Text = receiptText
            };

            btnSave = new Button { Text = "Save as Text File", Location = new Point(15, 405), Size = new Size(160, 35) };
            btnSave.Click += btnSave_Click;

            btnClose = new Button { Text = "Close", Location = new Point(305, 405), Size = new Size(100, 35) };
            btnClose.Click += (s, e) => this.Close();

            Controls.Add(txtReceipt);
            Controls.Add(btnSave);
            Controls.Add(btnClose);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string folder = Path.Combine(Application.StartupPath, "Receipts");
                Directory.CreateDirectory(folder);
                string filePath = Path.Combine(folder, $"{invoiceNo}.txt");
                File.WriteAllText(filePath, receiptText);
                MessageBox.Show($"Receipt saved to:\n{filePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save receipt: " + ex.Message);
            }
        }
    }
}