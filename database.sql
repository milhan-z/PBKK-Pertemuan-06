-- ==============================================================
-- Skrip Database: DbMahasiswaPBKK
-- Mata Kuliah   : Pemrograman Berbasis Kerangka Kerja (PBKK) C
-- Mahasiswa     : Muhammad Hilman Azhar (5025241264)
-- Dosen         : Fajar Baskoro, S.Kom., M.T.
-- ==============================================================

-- 1. Buat Database jika belum ada
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DbMahasiswaPBKK')
BEGIN
    CREATE DATABASE DbMahasiswaPBKK;
END
GO

USE DbMahasiswaPBKK;
GO

-- 2. Buat Tabel Mahasiswa
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
END
GO

-- 3. Inisialisasi Data Awal (Seed Data)
IF NOT EXISTS (SELECT 1 FROM Mahasiswa)
BEGIN
    INSERT INTO Mahasiswa (Nrp, Nama, Prodi, Angkatan, Ipk, Status, CreatedAt)
    VALUES 
    ('5025241264', N'Muhammad Hilman Azhar', N'Teknik Informatika', 2024, 3.88, N'Aktif', GETDATE()),
    ('5025241010', N'Ahmad Fauzi Rahman', N'Teknik Informatika', 2024, 3.75, N'Aktif', GETDATE()),
    ('5025231045', N'Siti Nurhaliza', N'Sistem Informasi', 2023, 3.90, N'Aktif', GETDATE()),
    ('5025221088', N'Budi Santoso', N'Teknologi Informasi', 2022, 3.45, N'Cuti', GETDATE()),
    ('5025241120', N'Dewi Anggraini', N'Sains Data', 2024, 3.82, N'Aktif', GETDATE());
END
GO

-- 4. Contoh Query Operasi CRUD ADO.NET:

-- READ (Pencarian & Seleksi Data)
-- SELECT Id, Nrp, Nama, Prodi, Angkatan, Ipk, Status, CreatedAt FROM Mahasiswa ORDER BY Nrp ASC;

-- CREATE (Tambah Mahasiswa)
-- INSERT INTO Mahasiswa (Nrp, Nama, Prodi, Angkatan, Ipk, Status, CreatedAt) VALUES (@Nrp, @Nama, @Prodi, @Angkatan, @Ipk, @Status, GETDATE());

-- UPDATE (Ubah Mahasiswa)
-- UPDATE Mahasiswa SET Nama = @Nama, Prodi = @Prodi, Angkatan = @Angkatan, Ipk = @Ipk, Status = @Status WHERE Nrp = @Nrp;

-- DELETE (Hapus Mahasiswa)
-- DELETE FROM Mahasiswa WHERE Nrp = @Nrp;
