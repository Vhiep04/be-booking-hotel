using be_booking_hotel.DTOs.Admin;
using be_booking_hotel.Repositories.Interfaces;
using be_booking_hotel.Services.Admin.Interfaces;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using System.Globalization;

namespace be_booking_hotel.Repositories.Implementations
{
    public class AdminDashboardExportService : IAdminDashboardExportService
    {
        private readonly IAdminDashboardService _dashboardService;

        // Brand colors
        private static readonly Color PrimaryBlue = Color.FromArgb(37, 99, 235);  // #2563EB
        private static readonly Color LightBlue = Color.FromArgb(219, 234, 254);  // #DBEAFE
        private static readonly Color DarkText = Color.FromArgb(17, 24, 39);   // #111827
        private static readonly Color MutedText = Color.FromArgb(107, 114, 128);  // #6B7280
        private static readonly Color SuccessGreen = Color.FromArgb(16, 185, 129);  // #10B981
        private static readonly Color DangerRed = Color.FromArgb(239, 68, 68);   // #EF4444
        private static readonly Color WarningYellow = Color.FromArgb(245, 158, 11);   // #F59E0B
        private static readonly Color RowAlt = Color.FromArgb(249, 250, 251);  // #F9FAFB
        private static readonly Color BorderGray = Color.FromArgb(229, 231, 235);  // #E5E7EB
        private static readonly Color White = Color.White;

        public AdminDashboardExportService(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<byte[]> ExportDashboardAsync()
        {
            ExcelPackage.License.SetNonCommercialPersonal("YourName");

            Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("vi-VN");

            var stats = await _dashboardService.GetStatsAsync();

            using var package = new ExcelPackage();

            BuildOverviewSheet(package, stats);
            BuildRevenueChartSheet(package, stats.RevenueChart);
            BuildTopHotelsSheet(package, stats.TopHotels);
            BuildRecentBookingsSheet(package, stats.RecentBookings);
            BuildPopularCitiesSheet(package, stats.PopularCities);

            return await package.GetAsByteArrayAsync();
        }

        // ─────────────────────────────────────────────
        // Sheet 1 – Overview
        // ─────────────────────────────────────────────
        private void BuildOverviewSheet(ExcelPackage pkg, AdminDashboardStats stats)
        {
            var ws = pkg.Workbook.Worksheets.Add("Overview");

            // ── Title ──
            ws.Cells["A1:F1"].Merge = true;
            ws.Cells["A1"].Value = "Hotel Booking Dashboard – Overview";
            StyleTitle(ws.Cells["A1"]);
            ws.Row(1).Height = 36;

            ws.Cells["A2:F2"].Merge = true;
            ws.Cells["A2"].Value = $"Generated: {DateTime.Now:dd MMM yyyy, HH:mm}";
            StyleSubtitle(ws.Cells["A2"]);
            ws.Row(2).Height = 20;

            ws.Row(3).Height = 8; // spacer

            // ── KPI Cards (2 columns × 5 rows) ──
            var kpis = new[]
            {
            ("Total Users", stats.TotalUsers.ToString("N0"), ""),
            ("Total Hotels", stats.TotalHotels.ToString("N0"), ""),
            ("Total Cities", stats.TotalCities.ToString("N0"), ""),
            ("Total Rooms", stats.TotalRooms.ToString("N0"), ""),
            ("Total Reservations", stats.TotalReservations.ToString("N0"), ""),
            ("Pending", stats.PendingReservations.ToString("N0"),""),
            ("Confirmed" , stats.ConfirmedReservations.ToString("N0"),""),
            ("Completed", stats.CompletedReservations.ToString("N0"),""),
            ("Cancelled", stats.CancelledReservations.ToString("N0"),""),
            ("Avg Rating", stats.AverageRating.ToString("F1"), "/ 5"),
            ("Total Revenue", stats.TotalRevenue.ToString("C0"), ""),
            ("Revenue This Month", stats.RevenueThisMonth.ToString("C0"), ""),
        };

            int startRow = 4;
            for (int i = 0; i < kpis.Length; i++)
            {
                int row = startRow + i;
                var (label, value, unit) = kpis[i];

                ws.Cells[row, 1].Value = label;
                ws.Cells[row, 2].Value = $"{value} {unit}".Trim();

                StyleKpiLabel(ws.Cells[row, 1]);
                StyleKpiValue(ws.Cells[row, 2]);

                if (i % 2 == 0)
                {
                    ws.Cells[row, 1, row, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, 1, row, 4].Style.Fill.BackgroundColor.SetColor(RowAlt);
                }

                ws.Row(row).Height = 22;
            }

            ws.Column(1).Width = 30;
            ws.Column(2).Width = 22;
            ws.Column(3).Width = 5;
            ws.Column(4).Width = 5;
            ws.View.ShowGridLines = false;
        }

        // ─────────────────────────────────────────────
        // Sheet 2 – Revenue Chart (monthly data table)
        // ─────────────────────────────────────────────
        private void BuildRevenueChartSheet(ExcelPackage pkg, List<AdminRevenueByMonthDto> data)
        {
            var ws = pkg.Workbook.Worksheets.Add("Revenue by Month");

            var headers = new[] { "Year", "Month", "Month Name", "Revenue (đ)", "Reservations" };
            WriteSheetHeader(ws, "Monthly Revenue Report", headers);

            int row = 3;
            foreach (var item in data)
            {
                ws.Cells[row, 1].Value = item.Year;
                ws.Cells[row, 2].Value = item.Month;
                ws.Cells[row, 3].Value = item.MonthName;
                ws.Cells[row, 4].Value = (double)item.Revenue;
                ws.Cells[row, 5].Value = item.ReservationCount;

                ws.Cells[row, 4].Style.Numberformat.Format = "#,##0 [$₫-vi-VN]";
                StyleDataRow(ws, row, 5, row % 2 == 0);
                row++;
            }

            // Totals
            if (data.Count > 0)
            {
                ws.Cells[row, 3].Value = "TOTAL";
                ws.Cells[row, 4].Formula = $"=SUM(D3:D{row - 1})";
                ws.Cells[row, 5].Formula = $"=SUM(E3:E{row - 1})";
                ws.Cells[row, 4].Style.Numberformat.Format = "#,##0 [$₫-vi-VN]";
                StyleTotalRow(ws, row, 5);
            }

            SetColumnWidths(ws, 10, 10, 18, 18, 16);
            ws.View.ShowGridLines = false;
        }

        // ─────────────────────────────────────────────
        // Sheet 3 – Top Hotels
        // ─────────────────────────────────────────────
        private void BuildTopHotelsSheet(ExcelPackage pkg, List<AdminTopHotelDto> data)
        {
            var ws = pkg.Workbook.Worksheets.Add("Top Hotels");

            var headers = new[] { "#", "Hotel Name", "City", "Reservations", "Revenue (đ)", "Avg Rating" };
            WriteSheetHeader(ws, "Top Hotels by Reservations", headers);

            int row = 3;
            foreach (var (hotel, idx) in data.Select((h, i) => (h, i + 1)))
            {
                ws.Cells[row, 1].Value = idx;
                ws.Cells[row, 2].Value = hotel.Name;
                ws.Cells[row, 3].Value = hotel.CityName;
                ws.Cells[row, 4].Value = hotel.ReservationCount;
                ws.Cells[row, 5].Value = (double)hotel.Revenue;
                ws.Cells[row, 6].Value = hotel.AverageRating;

                ws.Cells[row, 5].Style.Numberformat.Format = "#,##0 [$₫-vi-VN]";
                ws.Cells[row, 6].Style.Numberformat.Format = "0.0";

                // Color-code rating
                Color ratingColor = hotel.AverageRating >= 4.5 ? SuccessGreen
                                  : hotel.AverageRating >= 3.5 ? WarningYellow
                                  : DangerRed;
                ws.Cells[row, 6].Style.Font.Color.SetColor(ratingColor);
                ws.Cells[row, 6].Style.Font.Bold = true;

                StyleDataRow(ws, row, 6, row % 2 == 0);
                row++;
            }

            SetColumnWidths(ws, 5, 30, 20, 16, 16, 12);
            ws.View.ShowGridLines = false;
        }

        // ─────────────────────────────────────────────
        // Sheet 4 – Recent Bookings
        // ─────────────────────────────────────────────
        private void BuildRecentBookingsSheet(ExcelPackage pkg, List<AdminRecentBookingDto> data)
        {
            var ws = pkg.Workbook.Worksheets.Add("Recent Bookings");

            var headers = new[] { "Booking Code", "Hotel", "City", "Guest", "Check-In", "Check-Out", "Amount (đ)", "Status" };
            WriteSheetHeader(ws, "Recent Bookings", headers);

            int row = 3;
            foreach (var b in data)
            {
                ws.Cells[row, 1].Value = b.BookingCode;
                ws.Cells[row, 2].Value = b.HotelName;
                ws.Cells[row, 3].Value = b.CityName;
                ws.Cells[row, 4].Value = b.GuestName;
                ws.Cells[row, 5].Value = b.CheckInDate;
                ws.Cells[row, 6].Value = b.CheckOutDate;
                ws.Cells[row, 7].Value = (double)b.Amount;
                ws.Cells[row, 8].Value = b.Status;

                ws.Cells[row, 5].Style.Numberformat.Format = "dd-mmm-yyyy";
                ws.Cells[row, 6].Style.Numberformat.Format = "dd-mmm-yyyy";
                ws.Cells[row, 7].Style.Numberformat.Format = "#,##0 [$₫-vi-VN]";

                // Status badge color
                var statusColor = b.Status switch
                {
                    "Confirmed" or "Completed" => SuccessGreen,
                    "Cancelled" => DangerRed,
                    _ => WarningYellow   // Pending
                };
                ws.Cells[row, 8].Style.Font.Color.SetColor(statusColor);
                ws.Cells[row, 8].Style.Font.Bold = true;

                StyleDataRow(ws, row, 8, row % 2 == 0);
                row++;
            }

            SetColumnWidths(ws, 16, 26, 18, 22, 14, 14, 14, 14);
            ws.View.ShowGridLines = false;
        }

        // ─────────────────────────────────────────────
        // Sheet 5 – Popular Cities
        // ─────────────────────────────────────────────
        private void BuildPopularCitiesSheet(ExcelPackage pkg, List<AdminPopularCityDto> data)
        {
            var ws = pkg.Workbook.Worksheets.Add("Popular Cities");

            var headers = new[] { "#", "City Name", "Hotels", "Bookings", "Share (%)" };
            WriteSheetHeader(ws, "Popular Cities by Bookings", headers);

            int row = 3;
            foreach (var (city, idx) in data.Select((c, i) => (c, i + 1)))
            {
                ws.Cells[row, 1].Value = idx;
                ws.Cells[row, 2].Value = city.Name;
                ws.Cells[row, 3].Value = city.HotelCount;
                ws.Cells[row, 4].Value = city.BookingCount;
                ws.Cells[row, 5].Value = city.Percentage / 100.0;

                ws.Cells[row, 5].Style.Numberformat.Format = "0%";

                StyleDataRow(ws, row, 5, row % 2 == 0);
                row++;
            }

            SetColumnWidths(ws, 5, 28, 12, 14, 12);
            ws.View.ShowGridLines = false;
        }

        // ─────────────────────────────────────────────
        // Shared helpers
        // ─────────────────────────────────────────────
        private void WriteSheetHeader(ExcelWorksheet ws, string title, string[] columns)
        {
            // Title row
            ws.Cells[1, 1, 1, columns.Length].Merge = true;
            ws.Cells[1, 1].Value = title;
            StyleTitle(ws.Cells[1, 1]);
            ws.Row(1).Height = 32;

            // Column headers
            for (int c = 0; c < columns.Length; c++)
            {
                var cell = ws.Cells[2, c + 1];
                cell.Value = columns[c];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(PrimaryBlue);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ApplyBorder(cell);
            }
            ws.Row(2).Height = 24;
        }

        private void StyleDataRow(ExcelWorksheet ws, int row, int cols, bool alternate)
        {
            var range = ws.Cells[row, 1, row, cols];
            if (alternate)
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(RowAlt);
            }
            for (int c = 1; c <= cols; c++)
            {
                var cell = ws.Cells[row, c];
                cell.Style.Font.Color.SetColor(DarkText);
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ApplyBorder(cell);
            }
            ws.Row(row).Height = 20;
        }

        private void StyleTotalRow(ExcelWorksheet ws, int row, int cols)
        {
            var range = ws.Cells[row, 1, row, cols];
            range.Style.Font.Bold = true;
            range.Style.Font.Color.SetColor(White);
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(PrimaryBlue);
            for (int c = 1; c <= cols; c++) ApplyBorder(ws.Cells[row, c]);
            ws.Row(row).Height = 22;
        }

        private static void StyleTitle(ExcelRange cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 14;
            cell.Style.Font.Color.SetColor(Color.FromArgb(17, 24, 39));
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(219, 234, 254));
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private static void StyleSubtitle(ExcelRange cell)
        {
            cell.Style.Font.Italic = true;
            cell.Style.Font.Size = 10;
            cell.Style.Font.Color.SetColor(Color.FromArgb(107, 114, 128));
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        private static void StyleKpiLabel(ExcelRange cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 11;
            cell.Style.Font.Color.SetColor(Color.FromArgb(55, 65, 81));
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            cell.Style.Indent = 1;
        }

        private static void StyleKpiValue(ExcelRange cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 12;
            cell.Style.Font.Color.SetColor(Color.FromArgb(37, 99, 235));
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        }

        private static void ApplyBorder(ExcelRange cell)
        {
            var border = cell.Style.Border;
            var gray = Color.FromArgb(209, 213, 219);
            border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
            border.Bottom.Color.SetColor(gray);
            border.Top.Color.SetColor(gray);
            border.Left.Color.SetColor(gray);
            border.Right.Color.SetColor(gray);
        }

        private static void SetColumnWidths(ExcelWorksheet ws, params double[] widths)
        {
            for (int i = 0; i < widths.Length; i++)
                ws.Column(i + 1).Width = widths[i];
        }
    }
}
