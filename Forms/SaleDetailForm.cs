using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using RetailApp.Data;
using RetailApp.Models;

namespace RetailApp.Forms
{
    public partial class SaleDetailForm : Form
    {
        private readonly int saleId;
        private Label lblHeader;
        private DataGridView dgvItems;
        private Button btnClose;

        public SaleDetailForm(int saleId, string invoiceNo, DateTime saleDate, decimal total)
        {
            this.saleId = saleId;
            InitializeComponent();
            BuildUi(invoiceNo, saleDate, total);
            LoadItems();
        }

        private void BuildUi(string invoiceNo, DateTime saleDate, decimal total)
        {
            this.Text = "Sale Details";
            this.ClientSize = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;

            lblHeader = new Label
            {
                Text = $"Invoice: {invoiceNo}    Date: {saleDate:yyyy-MM-dd HH:mm}    Total: Rs. {total:0.00}",
                Location = new Point(15, 15),
                Size = new Size(470, 40),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            dgvItems = new DataGridView
            {
                Location = new Point(15, 60),
                Size = new Size(470, 280),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(400, 350),
                Size = new Size(85, 30)
            };
            btnClose.Click += (s, e) => this.Close();

            Controls.Add(lblHeader);
            Controls.Add(dgvItems);
            Controls.Add(btnClose);
        }

        private void LoadItems()
        {
            var items = new List<SaleItem>();
            using var connection = new SqliteConnection(DatabaseHelper.ConnectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT p.Name, si.Quantity, si.UnitPrice, si.LineTotal
                                 FROM SaleItems si
                                 JOIN Products p ON si.ProductId = p.ProductId
                                 WHERE si.SaleId = $saleId";
            cmd.Parameters.AddWithValue("$saleId", saleId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new SaleItem
                {
                    ProductName = reader.GetString(0),
                    Quantity = reader.GetInt32(1),
                    UnitPrice = reader.GetDecimal(2),
                    LineTotal = reader.GetDecimal(3)
                });
            }

            dgvItems.DataSource = null;
            dgvItems.DataSource = items;

            foreach (var col in new[] { "SaleItemId", "SaleId", "ProductId" })
            {
                if (dgvItems.Columns[col] != null)
                    dgvItems.Columns[col].Visible = false;
            }
        }
    }
}