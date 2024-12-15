using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WPFMapApp
{
    public partial class MainWindow : Window
    {
        private List<GMapMarker> markers = new List<GMapMarker>(); // Lưu danh sách marker

        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private async void InitializeApplication()
        {
            await LoadAllDistrictsData();
            InitializeMap();
        }

        private void InitializeMap()
        {
            MapControl.MapProvider = GMapProviders.GoogleMap;
            MapControl.Position = new PointLatLng(21.028511, 105.804817); // Hà Nội
            MapControl.MinZoom = 2;
            MapControl.MaxZoom = 18;
            MapControl.Zoom = 10;
            MapControl.ShowCenter = false;

            // Thêm marker vào bản đồ sau khi khởi tạo
            foreach (var marker in markers)
            {
                MapControl.Markers.Add(marker); // Đảm bảo marker được thêm vào bản đồ
            }
        }

        private async Task LoadAllDistrictsData()
        {
            try
            {
                List<EnvironmentalData> dataList = new List<EnvironmentalData>();

                // Lấy dữ liệu của từng quận từ API
                for (int i = 1; i <= 12; i++)
                {
                    var districtDataList = await GetEnvironmentalDataByDistrictIdAsync(i);
                    if (districtDataList != null)
                    {
                        dataList.AddRange(districtDataList); // Thêm tất cả dữ liệu từ danh sách vào
                    }
                }

                // Thêm marker cho từng quận
                foreach (var data in dataList)
                {
                    var marker = new GMapMarker(new PointLatLng(data.Latitude, data.Longitude))
                    {
                        Shape = new System.Windows.Controls.Image
                        {
                            Source = new BitmapImage(new Uri("pack://application:,,,/Assets/marker_icon.png")),
                            Width = 40,
                            Height = 40
                        }
                    };

                    marker.Tag = data; // Gán dữ liệu quận cho marker
                    markers.Add(marker);
                    MapControl.Markers.Add(marker); // Đảm bảo marker được thêm vào bản đồ ngay khi tải xong
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        private async Task<List<EnvironmentalData>> GetEnvironmentalDataByDistrictIdAsync(int districtId)
        {
            string apiUrl = $"https://6bbd-2405-4802-219-cfd0-10b9-7560-98ed-411.ngrok-free.app/api/EnvironmentalData/district/id/{districtId}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonData = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<EnvironmentalData>>(jsonData);
                    }
                    else
                    {
                        throw new Exception($"API call failed with status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching data for district {districtId}: {ex.Message}");
                    return null;
                }
            }
        }

        private void MapControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var pos = MapControl.FromLocalToLatLng((int)e.GetPosition(MapControl).X, (int)e.GetPosition(MapControl).Y);

            foreach (var marker in markers)
            {
                var markerPos = marker.Position;
                // Kiểm tra xem nhấp chuột có gần vị trí của marker hay không
                if (Math.Abs(pos.Lat - markerPos.Lat) < 0.001 && Math.Abs(pos.Lng - markerPos.Lng) < 0.001)
                {
                    var data = marker.Tag as EnvironmentalData; // Lấy dữ liệu gắn với marker

                    if (data != null)
                    {
                        InfoTitle.Text = $"District: {data.District}";
                        InfoTemperature.Text = $"Temperature: {data.Temperature}°C";
                        InfoHumidity.Text = $"Humidity: {data.Humidity}%";
                        InfoPressure.Text = $"Pressure: {data.Pressure} hPa";
                        InfoAQI.Text = $"AQI: {data.AQI}";
                        InfoNO2.Text = $"NO2: {data.NO2} µg/m³";
                        InfoSO2.Text = $"SO2: {data.SO2} µg/m³";
                        InfoPM25.Text = $"PM2.5: {data.PM25} µg/m³";
                        InfoCO.Text = $"CO: {data.CO} mg/m³";

                        InfoBorder.Visibility = Visibility.Visible;
                    }

                    break;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            InfoBorder.Visibility = Visibility.Collapsed;
        }
    }

    public class EnvironmentalData
    {
        public string District { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
        public int AQI { get; set; }
        public double NO2 { get; set; }
        public double SO2 { get; set; }
        public double PM25 { get; set; }
        public double CO { get; set; }
    }
}
