using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UberSystem.Domain.Entities;
using UberSystem.Domain.Interfaces;

namespace UberSystem.Service
{
    public class LocationUpdateService : BackgroundService
    {
        private readonly ILogger<LocationUpdateService> _logger;
        private readonly IServiceProvider _serviceProvider; // Thêm IServiceProvider để tạo scope
        private readonly string _filePath = @"D:\Study\Ky8\PRN231\new_clean_bo3.xlsx"; // Đường dẫn đến file Excel
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(15);

        public LocationUpdateService(ILogger<LocationUpdateService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider; // Tiêm IServiceProvider
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LocationUpdateService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExcelFile();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while updating driver locations from Excel.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("LocationUpdateService is stopping.");
        }

        private async Task ProcessExcelFile()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(_filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0]; // Lấy trang tính đầu tiên
                int startRow = 3; // Bắt đầu từ dòng 2
                int currentRow = startRow;
                int currentColumn = 4; // Cột D là cột thứ 4

                while (true)
                {
                    // Lấy driver ID từ cột C
                    var idCell = worksheet.Cells[$"A{currentRow}"].Text;
                    if (string.IsNullOrEmpty(idCell)) break;

                    long driverId;
                    if (!long.TryParse(idCell, out driverId))
                    {
                        _logger.LogError($"Invalid driver ID at row {currentRow}");
                        currentRow++;
                        currentColumn = 4;
                        continue;
                    }

                    // Lấy kinh độ và vĩ độ từ các cột D, E, F
                    var locationCell = worksheet.Cells[currentRow, currentColumn].Text;
                    if (string.IsNullOrEmpty(locationCell))
                    {
                        // Không còn dữ liệu trong cột hiện tại
                        _logger.LogInformation($"No more location data in row {currentRow}");
                        currentRow++;
                        currentColumn = 4; // Quay lại cột D cho hàng tiếp theo
                        continue;
                    }

                    // Parse kinh độ và vĩ độ từ chuỗi "(longitude, latitude)"
                    var coordinates = ParseCoordinates(locationCell);
                    if (coordinates == null)
                    {
                        _logger.LogError($"Invalid coordinates format at row {currentRow}, column {currentColumn}");
                        currentColumn++; // Sang cột tiếp theo
                        continue;
                    }

                    // Cập nhật vị trí của driver vào DB trong phạm vi scope mới
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        await UpdateDriverLocationAsync(unitOfWork, driverId, coordinates.Value.latitude, coordinates.Value.longitude);
                        await Task.Delay(_interval);
                    }

                    // Sang cột tiếp theo
                    currentColumn++;
                    _logger.LogInformation($"This column at {currentColumn}");
                    _logger.LogInformation($"This row at {currentRow}");

                    // Nếu đã kiểm tra hết các cột (D, E, F), chuyển sang hàng tiếp theo
                    if (currentColumn > 6)
                    {
                        currentRow++;
                        currentColumn = 4;
                    }

                    //break;
                }
            }
        }

        private (double latitude, double longitude)? ParseCoordinates(string location)
        {
            try
            {
                var cleanedLocation = location.Trim('(', ')');
                var parts = cleanedLocation.Split(',');
                if (parts.Length == 2)
                {
                    double longitude = double.Parse(parts[0]);
                    double latitude = double.Parse(parts[1]);
                    return (latitude, longitude);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private async Task UpdateDriverLocationAsync(IUnitOfWork unitOfWork, long driverId, double latitude, double longitude)
        {
            var driverRepos = unitOfWork.Repository<Driver>();
            var driver = await driverRepos.FindAsync(driverId);
            if (driver != null)
            {
                driver.LocationLatitude = latitude;
                driver.LocationLongitude = longitude;

                await unitOfWork.CommitTransaction();

                _logger.LogInformation($"Updated Driver {driverId} location to Latitude: {latitude}, Longitude: {longitude}");
            }
            else
            {
                _logger.LogError($"Driver with ID {driverId} not found.");
            }
        }
    }
}
