using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Pertemuan_06_WPF_SQLServer_Integration.Data;
using Pertemuan_06_WPF_SQLServer_Integration.Models;

namespace Pertemuan_06_WPF_SQLServer_Integration;

public partial class MainWindow : Window
{
    private Mahasiswa? _selectedMahasiswa;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Inisialisasi Database SQL Server LocalDB dan Tabel Mahasiswa
            DatabaseHelper.InitializeDatabase();

            // Muat Data Pertama Kali
            MuatData();

            // Penanganan mode tangkapan layar otomatis
            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("--screenshot"))
            {
                if (dgMahasiswa.Items.Count > 0)
                {
                    dgMahasiswa.SelectedIndex = 0;
                }

                Dispatcher.InvokeAsync(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(600);
                    AmbilScreenshot();
                    Application.Current.Shutdown();
                }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Koneksi database gagal: {ex.Message}", "Kesalahan Database", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MuatData(string? keyword = null)
    {
        try
        {
            List<Mahasiswa> list = DatabaseHelper.GetAll(keyword);
            dgMahasiswa.ItemsSource = list;

            var stats = DatabaseHelper.GetStats();
            lblTotalMahasiswa.Text = stats.Total.ToString();
            lblRataIpk.Text = stats.RataIpk.ToString("F2", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gagal memuat data dari database: {ex.Message}", "Error Query", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void dgMahasiswa_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgMahasiswa.SelectedItem is Mahasiswa m)
        {
            _selectedMahasiswa = m;

            txtNrp.Text = m.Nrp;
            txtNama.Text = m.Nama;
            txtIpk.Text = m.Ipk.ToString("F2", CultureInfo.InvariantCulture);

            // Set ComboBox Prodi
            foreach (ComboBoxItem item in cmbProdi.Items)
            {
                if (item.Content.ToString() == m.Prodi)
                {
                    cmbProdi.SelectedItem = item;
                    break;
                }
            }

            // Set ComboBox Angkatan
            foreach (ComboBoxItem item in cmbAngkatan.Items)
            {
                if (item.Content.ToString() == m.Angkatan.ToString())
                {
                    cmbAngkatan.SelectedItem = item;
                    break;
                }
            }

            // Set ComboBox Status
            foreach (ComboBoxItem item in cmbStatus.Items)
            {
                if (item.Content.ToString() == m.Status)
                {
                    cmbStatus.SelectedItem = item;
                    break;
                }
            }

            // Kunci field NRP (Primary Key / Unique identifier)
            txtNrp.IsReadOnly = true;
            txtNrp.Background = new SolidColorBrush(Color.FromRgb(241, 245, 249));

            // Ubah mode formulir
            badgeFormMode.Background = new SolidColorBrush(Color.FromRgb(254, 243, 199)); // #FEF3C7
            badgeFormMode.BorderBrush = new SolidColorBrush(Color.FromRgb(252, 211, 77)); // #FCD34D
            txtFormMode.Foreground = new SolidColorBrush(Color.FromRgb(180, 83, 9)); // #B45309
            txtFormMode.Text = $"Mode: Ubah / Hapus Record SQL ({m.Nrp})";

            btnTambah.IsEnabled = false;
            btnUpdate.IsEnabled = true;
            btnHapus.IsEnabled = true;
        }
    }

    private void btnTambah_Click(object sender, RoutedEventArgs e)
    {
        string nrp = txtNrp.Text.Trim();
        string nama = txtNama.Text.Trim();
        string ipkStr = txtIpk.Text.Trim().Replace(',', '.');

        if (string.IsNullOrWhiteSpace(nrp) || !long.TryParse(nrp, out _))
        {
            MessageBox.Show("NRP wajib berupa deretan angka!", "Validasi Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNrp.Focus();
            return;
        }

        if (DatabaseHelper.IsNrpExists(nrp))
        {
            MessageBox.Show($"NRP {nrp} sudah terdaftar di database SQL Server!", "Duplikasi Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNrp.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(nama) || nama.Length < 3)
        {
            MessageBox.Show("Nama lengkap minimal 3 karakter!", "Validasi Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNama.Focus();
            return;
        }

        if (!double.TryParse(ipkStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double ipk) || ipk < 0.0 || ipk > 4.0)
        {
            MessageBox.Show("IPK harus berupa angka antara 0.00 hingga 4.00!", "Validasi Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtIpk.Focus();
            return;
        }

        string prodi = (cmbProdi.SelectedItem is ComboBoxItem p) ? p.Content.ToString() ?? "" : "";
        int angkatan = (cmbAngkatan.SelectedItem is ComboBoxItem a && int.TryParse(a.Content.ToString(), out int angk)) ? angk : 2024;
        string status = (cmbStatus.SelectedItem is ComboBoxItem s) ? s.Content.ToString() ?? "Aktif" : "Aktif";

        Mahasiswa mhs = new Mahasiswa
        {
            Nrp = nrp,
            Nama = nama,
            Prodi = prodi,
            Angkatan = angkatan,
            Ipk = ipk,
            Status = status,
            CreatedAt = DateTime.Now
        };

        if (DatabaseHelper.Insert(mhs))
        {
            MuatData();
            ResetForm();
            MessageBox.Show($"Data mahasiswa {nama} ({nrp}) berhasil disimpan ke database SQL Server!", "INSERT Berhasil", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void btnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedMahasiswa == null) return;

        string nama = txtNama.Text.Trim();
        string ipkStr = txtIpk.Text.Trim().Replace(',', '.');

        if (string.IsNullOrWhiteSpace(nama) || nama.Length < 3)
        {
            MessageBox.Show("Nama lengkap minimal 3 karakter!", "Validasi Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNama.Focus();
            return;
        }

        if (!double.TryParse(ipkStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double ipk) || ipk < 0.0 || ipk > 4.0)
        {
            MessageBox.Show("IPK harus berupa angka antara 0.00 hingga 4.00!", "Validasi Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtIpk.Focus();
            return;
        }

        string prodi = (cmbProdi.SelectedItem is ComboBoxItem p) ? p.Content.ToString() ?? "" : "";
        int angkatan = (cmbAngkatan.SelectedItem is ComboBoxItem a && int.TryParse(a.Content.ToString(), out int angk)) ? angk : 2024;
        string status = (cmbStatus.SelectedItem is ComboBoxItem s) ? s.Content.ToString() ?? "Aktif" : "Aktif";

        _selectedMahasiswa.Nama = nama;
        _selectedMahasiswa.Prodi = prodi;
        _selectedMahasiswa.Angkatan = angkatan;
        _selectedMahasiswa.Ipk = ipk;
        _selectedMahasiswa.Status = status;

        if (DatabaseHelper.Update(_selectedMahasiswa))
        {
            MuatData();
            ResetForm();
            MessageBox.Show($"Data mahasiswa {_selectedMahasiswa.Nrp} berhasil diperbarui di database!", "UPDATE Berhasil", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void btnHapus_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedMahasiswa == null) return;

        MessageBoxResult result = MessageBox.Show(
            $"Apakah Anda yakin ingin menghapus record mahasiswa berikut dari database SQL Server:\nNRP: {_selectedMahasiswa.Nrp}\nNama: {_selectedMahasiswa.Nama}?",
            "Konfirmasi DELETE SQL",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            string nrp = _selectedMahasiswa.Nrp;
            if (DatabaseHelper.Delete(nrp))
            {
                MuatData();
                ResetForm();
                MessageBox.Show($"Record ({nrp}) berhasil dihapus dari database!", "DELETE Berhasil", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private void btnBatal_Click(object sender, RoutedEventArgs e)
    {
        ResetForm();
    }

    private void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
        MuatData(txtCari.Text);
    }

    private void txtCari_TextChanged(object sender, TextChangedEventArgs e)
    {
        MuatData(txtCari.Text);
    }

    private void ResetForm()
    {
        _selectedMahasiswa = null;
        dgMahasiswa.SelectedItem = null;

        txtNrp.Text = "";
        txtNrp.IsReadOnly = false;
        txtNrp.Background = Brushes.White;

        txtNama.Text = "";
        txtIpk.Text = "3.80";
        cmbProdi.SelectedIndex = 0;
        cmbAngkatan.SelectedIndex = 3; // 2024
        cmbStatus.SelectedIndex = 0; // Aktif

        badgeFormMode.Background = new SolidColorBrush(Color.FromRgb(239, 246, 255));
        badgeFormMode.BorderBrush = new SolidColorBrush(Color.FromRgb(191, 219, 254));
        txtFormMode.Foreground = new SolidColorBrush(Color.FromRgb(30, 64, 175));
        txtFormMode.Text = "Mode: Tambah Record Baru (INSERT)";

        btnTambah.IsEnabled = true;
        btnUpdate.IsEnabled = false;
        btnHapus.IsEnabled = false;

        txtNrp.Focus();
    }

    private void AmbilScreenshot()
    {
        try
        {
            int width = (int)ActualWidth;
            int height = (int)ActualHeight;

            if (width <= 0) width = 1160;
            if (height <= 0) height = 780;

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                width, height, 96, 96, PixelFormats.Pbgra32);
            renderBitmap.Render(this);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "screenshots"));
            Directory.CreateDirectory(dir);

            string file = Path.Combine(dir, "database_crud_gui.png");
            using (FileStream fs = new FileStream(file, FileMode.Create))
            {
                encoder.Save(fs);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error screenshot: {ex.Message}");
        }
    }
}
