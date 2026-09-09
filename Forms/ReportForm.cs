using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using RetailApp.Data;
using RetailApp.Models;

namespace RetailApp.Forms
{
    public partial class ReportForm : Form
    {
        private Panel pnlTop, pnlButtons;
        private Label lblTitle, lblFrom, lblTo;
        private DateTimePicker dtpFrom, dtpTo;
        private Button btnGenerate, btnClose;
        private Label lblRevenueValue, lblRevenueCaption;
        private Label lblTransactionsValue, lblTransactionsCaption;
        private Label lblAvgValue, lblAvgCaption;
        private DataGridView dgvTopProducts;

        public ReportForm()
        {
            InitializeComponent();
            BuildUi();
            LoadReport(DateTime.Now.AddYears(-5), DateTime.Now);
        }

        private void BuildUi()
        {
            this.Text = "Sales Report";
            this.ClientSize = new Size(750, 600);
            this.MinimumSize = new Size(650, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);

            // --- Top panel: title, date filter, summary stats ---
            pnlTop = new Panel { Dock = DockStyle.Top, Height = 160 };

            lblTitle = new Label
            {
                Text = "Sales Report",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 10)
            };

            lblFrom = new Label { Text = "From:", Location = new Point(20, 50), Size = new Size(40, 20) };
            dtpFrom = new DateTimePicker { Location = new Point(65, 47), Size = new Size(130, 27), Format = DateTimePickerFormat.Short };
            dtpFrom.Value = DateTime.Now.AddMonths(-1);

            lblTo = new Label { Text = "To:", Location = new Point(210, 50), Size = new Size(30, 20) };
            dtpTo = new DateTimePicker { Location = new Point(245, 47), Size = new Size(130, 27), Format = DateTimePickerFormat.Short };
            dtpTo.Value = DateTime.Now;

            btnGenerate = new Button
            {
                Text = "Generate",
                Location = new Point(390, 46),
                Size = new Size(100, 29),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Click += (s, e) => LoadReport(dtpFrom.Value, dtpTo.Value);

            lblRevenueCaption = new Label { Text = "Total Revenue", ForeColor = Color.Gray, Location = new Point(20, 95), Size = new Size(150, 18) };
            lblRevenueValue = new Label { Text = "Rs. 0.00", Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(20, 113), Size = new Size(200, 30) };

            lblTransactionsCaption = new Label { Text = "Transactions", ForeColor = Color.Gray, Location = new Point(260, 95), Size = new Size(150, 18) };
            lblTransactionsValue = new Label { Text = "0", Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(260, 113), Size = new Size(150, 30) };

            lblAvgCaption = new Label { Text = "Average Sale", ForeColor = Color.Gray, Location = new Point(450, 95), Size = new Size(150, 18) };
            lblAvgValue = new Label { Text = "Rs. 0.00", Font = new Font("Segoe UI", 13, FontStyle.Bold), Location = new Point(450, 113), Size = new Size(200, 30) };

            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(lblFrom);
            pnlTop.Controls.Add(dtpFrom);
            pnlTop.Controls.Add(lblTo);
            pnlTop.Controls.Add(dtpTo);
            pnlTop.Controls.Add(btnGenerate);
            pnlTop.Controls.Add(lblRevenueCaption);
            pnlTop.Controls.Add(lblRevenueValue);
            pnlTop.Controls.Add(lblTransactionsCaption);
            pnlTop.Controls.Add(lblTransactionsValue);
            pnlTop.Controls.Add(lblAvgCaption);
            pnlTop.Controls.Add(lblAvgValue);

            // --- Bottom panel: close ---
            pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 60 };
            btnClose = new Button { Text = "Close", Size = new Size(100, 36), FlatStyle = FlatStyle.Flat };
            btnClose.Click += (s, e) => this.Close();
            pnlButtons.Controls.Add(btnClose);
            pnlButtons.Resize += (s, e) =>
            {
                btnClose.Location = new Point(pnlButtons.ClientSize.Width - btnClose.Width - 20, 12);
            };

            // --- Grid section label ---
            var lblGridTitle = new Label
            {
                Text = "Top Selling Products",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(20, 8, 0, 0)
            };

            // --- Grid: top selling products ---
            dgvTopProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                EnableHeadersVisualStyles = false
            };
            dgvTopProducts.RowTemplate.Height = 28;
            dgvTopProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgvTopProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTopProducts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvTopProducts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);

            // Add order matters for docking: Fill first, then edge-docked panels
            Controls.Add(dgvTopProducts);
            Controls.Add(lblGridTitle);
            Controls.Add(pnlButtons);
            Controls.Add(pnlTop);
        }

        private void LoadReport(DateTime from, DateTime to)
        {
            string fromStr = from.ToString("yyyy-MM-dd 00:00:00");
            string toStr = to.ToString("yyyy-MM-dd 23:59:59");

            using var connection = new SqliteConnection(DatabaseHelper.ConnectionString);
            connection.Open();

            // --- Summary stats ---
            var summaryCmd = connection.CreateCommand();
            summaryCmd.CommandText = @"SELECT COUNT(*), COALESCE(SUM(TotalAmount), 0)
                                        FROM Sales
                                        WHERE SaleDate BETWEEN $from AND $to";
            summaryCmd.Parameters.AddWithValue("$from", fromStr);
            summaryCmd.Parameters.AddWithValue("$to", toStr);
            using (var reader = summaryCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    int totalTransactions = reader.GetInt32(0);
                    decimal totalRevenue = reader.GetDecimal(1);
                    decimal avgSale = totalTransactions > 0 ? totalRevenue / totalTransactions : 0;

                    lblRevenueValue.Text = $"Rs. {totalRevenue:0.00}";
                    lblTransactionsValue.Text = totalTransactions.ToString();
                    lblAvgValue.Text = $"Rs. {avgSale:0.00}";
                }
            }

            // --- Top selling products ---
            var products = new List<ProductSalesSummary>();
            var topCmd = connection.CreateCommand();
            topCmd.CommandText = @"SELECT p.Name, SUM(si.Quantity) AS QtySold, SUM(si.LineTotal) AS Revenue
                                    FROM SaleItems si
                                    JOIN Products p ON si.ProductId = p.ProductId
                                    JOIN Sales s ON si.SaleId = s.SaleId
                                    WHERE s.SaleDate BETWEEN $from AND $to
                                    GROUP BY si.ProductId
                                    ORDER BY QtySold DESC
                                    LIMIT 10";
            topCmd.Parameters.AddWithValue("$from", fromStr);
            topCmd.Parameters.AddWithValue("$to", toStr);
            using (var reader = topCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    products.Add(new ProductSalesSummary
                    {
                        ProductName = reader.GetString(0),
                        QuantitySold = reader.GetInt32(1),
                        Revenue = reader.GetDecimal(2)
                    });
                }
            }

            dgvTopProducts.DataSource = null;
            dgvTopProducts.DataSource = products;
        }
    }
}