using System;
using Microsoft.Data.Sqlite;

namespace RetailApp.Data
{
    public static class DatabaseHelper
    {
        public static string ConnectionString = "Data Source=retail.db";

        public static void Initialize()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Products (
                    ProductId     INTEGER PRIMARY KEY AUTOINCREMENT,
                    SKU           TEXT UNIQUE NOT NULL,
                    Name          TEXT NOT NULL,
                    Category      TEXT,
                    UnitPrice     REAL NOT NULL,
                    StockQuantity INTEGER NOT NULL DEFAULT 0,
                    ReorderLevel  INTEGER NOT NULL DEFAULT 5,
                    CreatedAt     TEXT DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS Sales (
                    SaleId      INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceNo   TEXT UNIQUE NOT NULL,
                    SaleDate    TEXT DEFAULT CURRENT_TIMESTAMP,
                    TotalAmount REAL NOT NULL
                );

                CREATE TABLE IF NOT EXISTS SaleItems (
                    SaleItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                    SaleId     INTEGER NOT NULL REFERENCES Sales(SaleId),
                    ProductId  INTEGER NOT NULL REFERENCES Products(ProductId),
                    Quantity   INTEGER NOT NULL,
                    UnitPrice  REAL NOT NULL,
                    LineTotal  REAL NOT NULL
                );

                CREATE TABLE IF NOT EXISTS StockAdjustments (
                    AdjustmentId   INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductId      INTEGER NOT NULL REFERENCES Products(ProductId),
                    ChangeQty      INTEGER NOT NULL,
                    Reason         TEXT,
                    AdjustmentDate TEXT DEFAULT CURRENT_TIMESTAMP
                );
            ";
            command.ExecuteNonQuery();

            // Migration: safely add the new Unit column to Products if it doesn't already exist
            EnsureColumnExists(connection, "Products", "Unit", "TEXT DEFAULT 'pcs'");
        }

        private static void EnsureColumnExists(SqliteConnection connection, string table, string column, string columnDefinition)
        {
            var pragmaCmd = connection.CreateCommand();
            pragmaCmd.CommandText = $"PRAGMA table_info({table})";
            bool exists = false;
            using (var reader = pragmaCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }
            }

            if (!exists)
            {
                var alterCmd = connection.CreateCommand();
                alterCmd.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {columnDefinition}";
                alterCmd.ExecuteNonQuery();
            }
        }
    }
}