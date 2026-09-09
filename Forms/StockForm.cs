using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using RetailApp.Data;
using RetailApp.Models;

namespace RetailApp.Forms
{
    public partial class StockForm : Form
    {
        private Panel pnlTop, pnlButtons;
        private Label lblTitle, lblInfo;
        private DataGridView dgvStock;
        private Button btnAdjustStock, btnRefresh, btnClose;

        public StockForm()
        {
            InitializeComponent();
            BuildUi();
            LoadStock();
        }

        private void BuildUi()
        {
            this.Text = "Stock Management";
            this.ClientSize = new Size(900, 600);
            this.MinimumSize = new Size(700, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9);

            // --- Top panel: title + info ---
            pnlTop = new Panel { Dock = DockStyle.Top, Height = 70 };
            lblTitle = new Label
            {
                Text = "Stock Management",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 10)
            };
            lblInfo = new Label
            {
                Text = "Rows highlighted in red are at or below their reorder level.",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(20, 42)
            };
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(lblInfo);

            // --- Bottom panel: action buttons ---
            pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 60 };

            btnAdjustStock = new Button
            {
                Text = "Adjust Stock",
                Size = new Size(130, 36),
                Location = new Point(20, 12),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdjustStock.FlatAppearance.BorderSize = 0;
            btnAdjustStock.Click += btnAdjustStock_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Size = new Size(100, 36),
                Location = new Point(160, 12),
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadStock();

            btnClose = new Button
            {
                Text = "Close",
                Size = new Size(100, 36),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, e) => this.Close();

            pnlButtons.Controls.Add(btnAdjustStock);
            pnlButtons.Controls.Add(btnRefresh);
            pnlButtons.Controls.Add(btnClose);

            // Keep Close pinned to the right edge even if the window is resized
            pnlButtons.Resize += (s, e) =>
            {
                btnClose.Location = new Point(pnlButtons.ClientSize.Width - btnClose.Width - 20, 12);
            };

            // --- Grid fills the remaining space ---
            dgvStock = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                EnableHeadersVisualStyles = false
            };
            dgvStock.RowTemplate.Height = 30;
            dgvStock.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(41, 128, 185);
            dgvStock.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvStock.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvStock.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvStock.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
            dgvStock.CellFormatting += dgvStock_CellFormatting;

            // Add order matters for docking: Fill first, then edge-docked panels
            Controls.Add(dgvStock);
            Controls.Add(pnlButtons);
            Controls.Add(pnlTop);
        }

        private void LoadStock()
        {
            var products = new List<Product>();
            using var connection = new SqliteConnection(DatabaseHelper.ConnectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT ProductId, SKU, Name, Category, UnitPrice, StockQuantity, ReorderLevel FROM Products ORDER BY Name";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add(new Product
                {
                    ProductId = reader.GetInt32(0),
                    SKU = reader.GetString(1),
                    Name = reader.GetString(2),
                    Category = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    UnitPrice = reader.GetDecimal(4),
                    StockQuantity = reader.GetInt32(5),
                    ReorderLevel = reader.GetInt32(6)
                });
            }

            dgvStock.DataSource = null;
            dgvStock.DataSource = products;

            if (dgvStock.Columns["ProductId"] != null)
                dgvStock.Columns["ProductId"].Visible = false;
        }

        private void dgvStock_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvStock.Rows[e.RowIndex].DataBoundItem is Product p && p.StockQuantity <= p.ReorderLevel)
            {
                e.CellStyle.BackColor = Color.MistyRose;
                e.CellStyle.SelectionBackColor = Color.IndianRed;
                e.CellStyle.SelectionForeColor = Color.White;
            }
        }
        private void btnAdjustStock_Click(object sender, EventArgs e)
        {
            if (dgvStock.CurrentRow?.DataBoundItem is Product selected)
            {
                var dialog = new StockAdjustForm(selected.ProductId, selected.Name, selected.StockQuantity);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LoadStock();
                }
            }
            else
            {
                MessageBox.Show("Select a product first.");
            }
        }
    }
}