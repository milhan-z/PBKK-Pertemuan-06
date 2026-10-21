using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Pertemuan_06_WPF_SQLServer_Integration.Models;

namespace Pertemuan_06_WPF_SQLServer_Integration.Data;

public class DatabaseHelper
{
    private const string MasterConnString = "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15;";
    private const string AppConnString = "Server=(localdb)\\MSSQLLocalDB;Database=DbMahasiswaPBKK;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15;";

    public static void InitializeDatabase()
    {
        try
        {
            // 1. Pastikan database DbMahasiswaPBKK ada di SQL Server LocalDB
            using (SqlConnection masterConn = new SqlConnection(MasterConnString))
            {
                masterConn.Open();
                string createDbSql = @"
                    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DbMahasiswaPBKK')
                    BEGIN
                        CREATE DATABASE DbMahasiswaPBKK;
                    END";
                using SqlCommand cmd = new SqlCommand(createDbSql, masterConn);
                cmd.ExecuteNonQuery();
            }

            // 2. Pastikan tabel Mahasiswa dibuat dan diisi seed data
            using (SqlConnection conn = new SqlConnection(AppConnString))
            {
                conn.Open();
                string createTableSql = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Mahasiswa')
                    BEGIN
                        CREATE TABLE Mahasiswa (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Nrp VARCHAR(15) NOT NULL UNIQUE,
                            Nama NVARCHAR(100) NOT NULL,
                            Prodi NVARCHAR(60) NOT NULL,
                            Angkatan INT NOT NULL,
                            Ipk DECIMAL(3,2) NOT NULL CHECK (Ipk >= 0.00 AND Ipk <= 4.00),
                            Status NVARCHAR(20) NOT NULL DEFAULT 'Aktif',
                            CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
                        );
                    END";
                using (SqlCommand cmd = new SqlCommand(createTableSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Cek apakah tabel kosong
                string countSql = "SELECT COUNT(1) FROM Mahasiswa;";
                using (SqlCommand countCmd = new SqlCommand(countSql, conn))
                {
                    int count = (int)countCmd.ExecuteScalar();
                    if (count == 0)
                    {
                        string seedSql = @"
                            INSERT INTO Mahasiswa (Nrp, Nama, Prodi, Angkatan, Ipk, Status, CreatedAt)
                            VALUES 
                            ('5025241264', N'Muhammad Hilman Azhar', N'Teknik Informatika', 2024, 3.88, N'Aktif', GETDATE()),
                            ('5025241010', N'Ahmad Fauzi Rahman', N'Teknik Informatika', 2024, 3.75, N'Aktif', GETDATE()),
                            ('5025231045', N'Siti Nurhaliza', N'Sistem Informasi', 2023, 3.90, N'Aktif', GETDATE()),
                            ('5025221088', N'Budi Santoso', N'Teknologi Informasi', 2022, 3.45, N'Cuti', GETDATE()),
                            ('5025241120', N'Dewi Anggraini', N'Sains Data', 2024, 3.82, N'Aktif', GETDATE());";
                        using SqlCommand seedCmd = new SqlCommand(seedSql, conn);
                        seedCmd.ExecuteNonQuery();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Gagal menginisialisasi database SQL Server: {ex.Message}", ex);
        }
    }

    public static List<Mahasiswa> GetAll(string? keyword = null)
    {
        List<Mahasiswa> list = new List<Mahasiswa>();

        using SqlConnection conn = new SqlConnection(AppConnString);
        conn.Open();

        string query = @"
            SELECT Id, Nrp, Nama, Prodi, Angkatan, Ipk, Status, CreatedAt 
            FROM Mahasiswa 
            WHERE (@Keyword IS NULL OR Nrp LIKE @Pattern OR Nama LIKE @Pattern OR Prodi LIKE @Pattern)
            ORDER BY Nrp ASC";

        using SqlCommand cmd = new SqlCommand(query, conn);
        if (string.IsNullOrWhiteSpace(keyword))
        {
            cmd.Parameters.AddWithValue("@Keyword", DBNull.Value);
            cmd.Parameters.AddWithValue("@Pattern", DBNull.Value);
        }
        else
        {
            cmd.Parameters.AddWithValue("@Keyword", keyword);
            cmd.Parameters.AddWithValue("@Pattern", $"%{keyword}%");
        }

        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Mahasiswa
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Nrp = reader.GetString(reader.GetOrdinal("Nrp")),
                Nama = reader.GetString(reader.GetOrdinal("Nama")),
                Prodi = reader.GetString(reader.GetOrdinal("Prodi")),
                Angkatan = reader.GetInt32(reader.GetOrdinal("Angkatan")),
                Ipk = Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("Ipk"))),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            });
        }

        return list;
    }

    public static bool Insert(Mahasiswa m)
    {
        using SqlConnection conn = new SqlConnection(AppConnString);
        conn.Open();

        string query = @"
            INSERT INTO Mahasiswa (Nrp, Nama, Prodi, Angkatan, Ipk, Status, CreatedAt)
            VALUES (@Nrp, @Nama, @Prodi, @Angkatan, @Ipk, @Status, @CreatedAt);";

        using SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Nrp", m.Nrp);
        cmd.Parameters.AddWithValue("@Nama", m.Nama);
        cmd.Parameters.AddWithValue("@Prodi", m.Prodi);
        cmd.Parameters.AddWithValue("@Angkatan", m.Angkatan);
        cmd.Parameters.AddWithValue("@Ipk", Convert.ToDecimal(m.Ipk));
        cmd.Parameters.AddWithValue("@Status", m.Status);
        cmd.Parameters.AddWithValue("@CreatedAt", m.CreatedAt);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool Update(Mahasiswa m)
    {
        using SqlConnection conn = new SqlConnection(AppConnString);
        conn.Open();

        string query = @"
            UPDATE Mahasiswa 
            SET Nama = @Nama, Prodi = @Prodi, Angkatan = @Angkatan, Ipk = @Ipk, Status = @Status
            WHERE Nrp = @Nrp;";

        using SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Nrp", m.Nrp);
        cmd.Parameters.AddWithValue("@Nama", m.Nama);
        cmd.Parameters.AddWithValue("@Prodi", m.Prodi);
        cmd.Parameters.AddWithValue("@Angkatan", m.Angkatan);
        cmd.Parameters.AddWithValue("@Ipk", Convert.ToDecimal(m.Ipk));
        cmd.Parameters.AddWithValue("@Status", m.Status);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool Delete(string nrp)
    {
        using SqlConnection conn = new SqlConnection(AppConnString);
        conn.Open();

        string query = "DELETE FROM Mahasiswa WHERE Nrp = @Nrp;";
        using SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Nrp", nrp);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static bool IsNrpExists(string nrp)
    {
        using SqlConnection conn = new SqlConnection(AppConnString);
        conn.Open();

        string query = "SELECT COUNT(1) FROM Mahasiswa WHERE Nrp = @Nrp;";
        using SqlCommand cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Nrp", nrp);

        int count = (int)cmd.ExecuteScalar();
        return count > 0;
    }

    public static (int Total, double RataIpk, int TotalAktif, int TotalCuti) GetStats()
    {
        using SqlConnection conn = new SqlConnection(AppConnString);
        conn.Open();

        string query = @"
            SELECT 
                COUNT(1) AS Total,
                ISNULL(AVG(Ipk), 0.0) AS RataIpk,
                SUM(CASE WHEN Status = 'Aktif' THEN 1 ELSE 0 END) AS TotalAktif,
                SUM(CASE WHEN Status = 'Cuti' THEN 1 ELSE 0 END) AS TotalCuti
            FROM Mahasiswa;";

        using SqlCommand cmd = new SqlCommand(query, conn);
        using SqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            int total = reader.GetInt32(reader.GetOrdinal("Total"));
            double rataIpk = Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("RataIpk")));
            int aktif = reader.IsDBNull(reader.GetOrdinal("TotalAktif")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalAktif"));
            int cuti = reader.IsDBNull(reader.GetOrdinal("TotalCuti")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalCuti"));

            return (total, rataIpk, aktif, cuti);
        }

        return (0, 0.0, 0, 0);
    }
}
