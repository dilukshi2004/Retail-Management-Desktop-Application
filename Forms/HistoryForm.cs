using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using RetailApp.Data;
using RetailApp.Models;

namespace RetailApp.Forms
{
    public partial class HistoryForm : Form
    {
        private DataGridView dgvSales;
        private Button btnViewDetails;

        public HistoryForm()
        {
            InitializeComponent();
            BuildUi();
            LoadSales();
        }

        private void BuildUi()
        {
            this.Text = "Transaction History";
            this.ClientSize = new Size(700, 480);
            this.StartPosition = FormStartPosition.CenterScreen;

            dgvSales = new DataGridView
            {
                Location = new Point(20, 20),
                Size = new Size(650, 380),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false
            };
            dgvSales.CellDoubleClick += (s, e) => ViewSelectedDetails();

            btnViewDetails = new Button
            {
                Text = "View Details",
                Location = new Point(20, 415),
                Size = new Size(140, 35)
            };
            btnViewDetails.Click += (s, e) => ViewSelectedDetails();

            Controls.Add(dgvSales);
            Controls.Add(btnViewDetails);
        }

        private void LoadSales()
        {
            var sales = new List<Sale>();
            using var connection = new SqliteConnection(DatabaseHelper.ConnectionString);
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT SaleId, InvoiceNo, SaleDate, TotalAmount FROM Sales ORDER BY SaleDate DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                sales.Add(new Sale
                {
                    SaleId = reader.GetInt32(0),
                    InvoiceNo = reader.GetString(1),
                    SaleDate = DateTime.Parse(reader.GetString(2)),
                    TotalAmount = reader.GetDecimal(3)
                });
            }

            dgvSales.DataSource = null;
            dgvSales.DataSource = sales;

            if (dgvSales.Columns["SaleId"] != null) dgvSales.Columns["SaleId"].Visible = false;
            if (dgvSales.Columns["Items"] != null) dgvSales.Columns["Items"].Visible = false;
        }

        private void ViewSelectedDetails()
        {
            if (dgvSales.CurrentRow?.DataBoundItem is Sale sale)
            {
                var detailForm = new SaleDetailForm(sale.SaleId, sale.InvoiceNo, sale.SaleDate, sale.TotalAmount);
                detailForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Select a transaction first.");
            }
        }
    }
}