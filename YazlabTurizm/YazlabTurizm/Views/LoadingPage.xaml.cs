using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.GoogleMaps;

namespace YazlabTurizm.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]



    /*
     *  SQL Sorguları
     *  Admin tarafından yolculuk için seçilebilecek kullanıcıları istek bekleyenleri belirleyerek onları listeye ekleme
     *  SELECT u.Id, u.TelephoneNumber, u.UserName FROM [dbo].[users] as u, [dbo].[tripRequest] as tR WHERE tR.UserId = u.Id AND tR.Confirm = 0
    */



    public partial class LoadingPage : ContentPage
    {
        // Classes --------------------------------------------------------------------------------------------------

        public class User
        {
            public string reqType { get; set; }
            public string telephoneNumber { get; set; }
            public string userName { get; set; }
            public string userPassword { get; set; }

        };

        public class Station {
            public string reqType { get; set; }
            public string stationName { get; set; }
            public string stationLatitude { get; set; }
            public string stationLongitude { get; set; }
        };

        public class NoParametersRequest
        {
            public string reqType { get; set; }
        }

        public class ResponseJson
        {
            public string result { get; set; }
            public string description { get; set; }
            public List<string> returnData { get; set; }
        }

        public class TripRequest
        {
            public string reqType { get; set; }
            public string stationId { get; set; }
            public string userIds { get; set; }
        }

        public class CreateTrip
        {
            public string reqType { get; set; }
            public string tripRequestsIds { get; set; }
            public string tripName { get; set; }
            public string stationIds { get; set; }
            public string pessangerCounts { get; set; }
            

        }


        public class GeocodedWaypoint
        {
            public string geocoder_status { get; set; }
            public string place_id { get; set; }
            public List<string> types { get; set; }
        }

        public class Northeast
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Southwest
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Bounds
        {
            public Northeast northeast { get; set; }
            public Southwest southwest { get; set; }
        }

        public class Distance
        {
            public string text { get; set; }
            public int value { get; set; }
        }

        public class Duration
        {
            public string text { get; set; }
            public int value { get; set; }
        }

        public class EndLocation
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class StartLocation
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Distance2
        {
            public string text { get; set; }
            public int value { get; set; }
        }

        public class Duration2
        {
            public string text { get; set; }
            public int value { get; set; }
        }

        public class EndLocation2
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Polyline
        {
            public string points { get; set; }
        }

        public class StartLocation2
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }

        public class Step
        {
            public Distance2 distance { get; set; }
            public Duration2 duration { get; set; }
            public EndLocation2 end_location { get; set; }
            public string html_instructions { get; set; }
            public Polyline polyline { get; set; }
            public StartLocation2 start_location { get; set; }
            public string travel_mode { get; set; }
            public string maneuver { get; set; }
        }

        public class Leg
        {
            public Distance distance { get; set; }
            public Duration duration { get; set; }
            public string end_address { get; set; }
            public EndLocation end_location { get; set; }
            public string start_address { get; set; }
            public StartLocation start_location { get; set; }
            public List<Step> steps { get; set; }
            public List<object> traffic_speed_entry { get; set; }
            public List<object> via_waypoint { get; set; }
        }

        public class OverviewPolyline
        {
            public string points { get; set; }
        }

        public class Route
        {
            public Bounds bounds { get; set; }
            public string copyrights { get; set; }
            public List<Leg> legs { get; set; }
            public OverviewPolyline overview_polyline { get; set; }
            public string summary { get; set; }
            public List<object> warnings { get; set; }
            public List<object> waypoint_order { get; set; }
        }

        public class GoogleDirection
        {
            public List<GeocodedWaypoint> geocoded_waypoints { get; set; }
            public List<Route> routes { get; set; }
            public string status { get; set; }
        }

        public class Bus
        {
            public int capacity;
            public int pessangerCount;
            public Xamarin.Forms.GoogleMaps.Polyline routePolyLine;
            public Color busColor;
            public List<string> stationsIndex;
            public float cost;
            public List<int> takingPessagerCount;

            public Bus(int capacity, Color color, float cost = 0.0f) {
                this.capacity = capacity;
                this.busColor = color;
                this.pessangerCount = 0;
                this.routePolyLine = new Xamarin.Forms.GoogleMaps.Polyline();
                this.routePolyLine.StrokeColor = this.busColor;
                this.routePolyLine.StrokeWidth = 1.5f;
                this.stationsIndex = new List<string>();
                this.takingPessagerCount = new List<int>();
                this.cost = cost;
            }

        }

        


        // ----------------------------------------------------------------------------------------------------------

        ResponseJson resJson;
        FlexLayout mapOverlayAdmin;
        FlexLayout mapOverlayUser;
        FlexLayout singInOverlay;
        FlexLayout loginOverlay;
        FlexLayout loadingOverlay;
        Picker stationPicker;
        Picker userStationPicker;
        Picker tripHistoryPicker;
        CollectionView pessangers;
        Map adminMaps;
        Map userMaps;
        Label userLabel;
        Label userTripRequest;
        Entry stationName;
        int adminId = 0;
        int userId = 0;
        string userName = "";
        List<string> stationsInfos;
        List<string> stationNames;
        List<string> usersInfos;
        List<string> requestsInfos;
        List<string> requestIds;
        float[,] stationGraph;
        List<Pin> stationPins;
        List<Xamarin.Forms.GoogleMaps.Polyline> tripDestinations;
        Position umuttepe = new Position(40.824802, 29.921519);
        List<Bus> busses;
        List<Color> busColors = new List<Color>() { Color.Black, Color.Brown, Color.DarkGoldenrod, Color.AliceBlue, Color.Coral, Color.DimGray, Color.DarkKhaki, Color.DarkCyan, Color.DarkMagenta, Color.Purple };
        public LoadingPage()
        {
            
            InitializeComponent();
            

            // Functions ------------------------------------------------------------------------------------------------

            async Task httpRequestToApi(int requestType, List<string> args)
            {

                /*
                 * İstekler
                 * 1 - Admin Kayıt                             Tamamlandı
                 * 2 - Admin Giriş                             Tamamlandı
                 * 3 - Kullanıcı Kayıt                         Tamamlandı
                 * 4 - Kullanıcı Giriş                         Tamamlandı
                 * 5 - Durak Ekleme                            Tamamlandı
                 * 6 - Durakları Çekme                         Tamamlandı 
                 * 7 - Kullanıcı Bilgilerini Çekme             Tamamlandı
                 * 8 - Yolcu İsteklerini Veritabanından Çekme  Tamamlandı
                 * 9 - Yolcu isteklerini Oluşturma             Tamamlandı
                 * 10 - kullanıcının isteklerini veri tabanından Çekme Tamamlandı
                 * 11 - Seferi Onayla
                 */

                using (var client = new HttpClient())
                {
                    var apiUri = new Uri("[Api-Url-Girin]");
                    var newPostJson = "";

                    if (requestType == 1 || requestType == 2 || requestType == 3 || requestType == 4)
                    {
                        
                        if (requestType == 1)   // Admin Kayıt
                            loadingOverlayEnable("Yönetici kaydı yapılıyor...");    
                        else  if(requestType == 2)  // Adnin Giriş
                            loadingOverlayEnable("Yönetici girişi yapılıyor...");
                        else if (requestType == 3)  // Kullanıcı Kayıt
                            loadingOverlayEnable("Kullanıcı kaydı yapılıyor...");
                        else if (requestType == 4)  // Kullanıcı Giriş
                            loadingOverlayEnable("Kullanıcı girişi yapılıyor...");
                        
                        var newPost = new User()
                        {
                            reqType = args[0],
                            telephoneNumber = args[1],
                            userName = args[2],
                            userPassword = args[3]
                        };
                        newPostJson = JsonConvert.SerializeObject(newPost);

                    }
                    else if(requestType == 5)
                    {
                        // Durak Ekleme
                        loadingOverlayEnable("Durak sisteme ekleniyor...");
                        var newStation = new Station()
                        {
                            reqType = args[0],
                            stationName = args[1],
                            stationLatitude = args[2],
                            stationLongitude = args[3]
                        };
                        newPostJson = JsonConvert.SerializeObject(newStation);

                    }
                    else if(requestType == 6)
                    {
                        loadingOverlayEnable("Duraklar güncelleniyor...");
                        var newNoParametersRequest = new NoParametersRequest()
                        {
                            reqType = args[0],
                        };
                        newPostJson = JsonConvert.SerializeObject(newNoParametersRequest);
                    }
                    else if (requestType == 7)
                    {
                        loadingOverlayEnable("Kullanıcı bilgileri alınıyor...");
                        var newNoParametersRequest = new NoParametersRequest()
                        {
                            reqType = args[0],
                        };
                        newPostJson = JsonConvert.SerializeObject(newNoParametersRequest);
                    }
                    else if (requestType == 8)
                    {
                        loadingOverlayEnable("İstek listesi alınıyor...");
                        var newNoParametersRequest = new NoParametersRequest()
                        {
                            reqType = args[0],
                        };
                        newPostJson = JsonConvert.SerializeObject(newNoParametersRequest);
                    }
                    else if(requestType == 9)
                    {
                        // İstekleri veri tabanına ekleme
                        loadingOverlayEnable("İstek oluşturuluyor...");
                        var newTripRequest = new TripRequest()
                        {
                            reqType = args[0],
                            stationId = args[1],
                            userIds = args[2],
                        };
                        newPostJson = JsonConvert.SerializeObject(newTripRequest);
                    }
                    else if (requestType == 10)
                    {
                        // kullanıcının isteklerini veri tabanından api ile çekme
                        loadingOverlayEnable("Kullanıcı talepleri alınıyor...");
                        var tripRequests = new TripRequest()
                        {
                            reqType = args[0],
                            userIds = args[1]
                        };
                        newPostJson = JsonConvert.SerializeObject(tripRequests);
                    }
                    else if (requestType == 11)
                    {
                        // Sefer Onaylama
                        loadingOverlayEnable("Sefer Onaylanıyor...");
                        var createTrip = new CreateTrip()
                        {
                            reqType = args[0],
                            stationIds = args[1],
                            pessangerCounts = args[2],
                            tripRequestsIds = args[3],
                            tripName = args[4]
                            
                        };
                        newPostJson = JsonConvert.SerializeObject(createTrip);
                    }

                    var postMessage = new StringContent(newPostJson, Encoding.UTF8, "application/json");
                    HttpResponseMessage responseMessage = await client.PostAsync(apiUri, postMessage);
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    resJson = JsonConvert.DeserializeObject<ResponseJson>(result);
                    Console.WriteLine(result);
                    loadingOverlayDisable();
                }
                
            }


            void overlaySwap(Layout inLayout, Layout outLayout)
            {
                outLayout.IsVisible = false;
                outLayout.IsEnabled = false;
                inLayout.IsEnabled = true;
                inLayout.IsVisible = true;
            }

            async Task getStations()
            {
                // Durakları api ile çek, pickera ve haritaya pin olarak ekle
               
                await httpRequestToApi(6, new List<string>() { "6" });
                //Console.WriteLine("Sonuç " + resJson.result.ToString());
                
                if (resJson.result.Equals("İstek Başarılı."))
                {
                    
                    //stationPicker.Items.Clear();
                    //stationPicker.ItemsSource.Clear();
                    stationsInfos.Clear();
                    stationPins = new List<Pin>();
                    stationNames = new List<string>();
                    stationNames.Clear();
                    //Console.WriteLine("asdasd "+resJson.returnData.Count.ToString());
                    foreach (string stations in resJson.returnData)
                    {
                        //Console.WriteLine("Durak " + stations);
                        //stationPicker.Items.Add(stations.Split(',')[1]);
                        stationsInfos.Add(stations);
                        stationNames.Add(stations.Split(',')[1]);
                        //Console.WriteLine("Admin Id" + adminId.ToString() + " User Id: " + userId.ToString());
                        if(adminId != 0)
                            adminMaps.Pins.Add(new Pin() { Address = "Bekleyen Yolcu Sayısı: 0", Tag = stations.Split(',')[0].ToString(), Label = stations.Split(',')[1].ToString(), Position = new Position(Convert.ToDouble(stations.Split(',')[2]), Convert.ToDouble(stations.Split(',')[3])) });
                        else if(userId != 0)
                        {
                            userMaps.Pins.Add(new Pin() { Address = "", Tag = stations.Split(',')[0].ToString(), Label = stations.Split(',')[1].ToString(), Position = new Position(Convert.ToDouble(stations.Split(',')[2]), Convert.ToDouble(stations.Split(',')[3]))});
                        }
                    }
                    //stationPicker.
                    //Console.WriteLine("picker ayarlandı");
                    if(adminId != 0)
                    {
                        stationPicker.ItemsSource = stationNames;
                        stationPicker.SelectedIndex = 0;
                    }
                    else if(userId != 0)
                    {
                        userStationPicker.ItemsSource = stationNames;
                        userStationPicker.SelectedIndex = 0;
                    }
                    
                    

                }
            }

            async Task getUsers()
            {
                await httpRequestToApi(7, new List<string>() { "7" });
                if (resJson.result.Equals("İstek Başarılı."))
                {
                    usersInfos.Clear();
                    foreach (string user in resJson.returnData)
                    {
                        usersInfos.Add(user);
                    }
                    

                }
            }

            async Task getRequests(bool booting, int userId = 0)
            {
                if (userId == 0)
                {
                    // admin için
                    await httpRequestToApi(8, new List<string>() { "8" });
                    if (resJson.result.Equals("İstek Başarılı."))
                    {
                        requestsInfos.Clear();
                        foreach (string request in resJson.returnData)
                        {
                            requestsInfos.Add(request);
                        }

                        List<string> showUsers = new List<string>();
                        showUsers.Clear();

                        List<string> userIds = new List<string>();

                        foreach (string info in usersInfos)
                        {
                            userIds.Add(info.Split(',')[0]);
                        }

                        requestIds = new List<string>();
                        requestIds.Clear();

                        foreach (string reqInfo in requestsInfos)
                        {
                            requestIds.Add(reqInfo.Split(',')[0]);
                            
                        }
                        
                        // eğer hiç istek yoksa
                        if(requestsInfos.Count == 0)
                        {
                            for (int j = 0; j < usersInfos.Count; j++)
                            {
                                showUsers.Add(usersInfos[j].Replace(',', '\t'));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < requestsInfos.Count; i++)
                            {
                                for (int j = 0; j < usersInfos.Count; j++)
                                {
                                    if (showUsers.Contains(usersInfos[j].Replace(',', '\t')))   // daha önce listelenmişse atla
                                    {
                                        continue;
                                    }
                                    else if (!requestIds.Contains(usersInfos[j].Split(',')[0]))
                                    {
                                        showUsers.Add(usersInfos[j].Replace(',', '\t'));        // daha önce listelememişse ekle
                                    }
                                }
                            }
                        }
                        
                        // pinleri güncelle
                        if (booting)
                        {
                            int waitingPessangersCount = 0;

                            for (int p = 0; p < adminMaps.Pins.Count; p++)
                            {
                                // tüm durakları güncelle
                                waitingPessangersCount = 0;
                                for (int i = 0; i < requestsInfos.Count; i++)
                                {
                                    //Console.WriteLine("Pin sayısı: " + adminMaps.Pins.Count.ToString() + " İstek Sayısı: " + requestsInfos.Count.ToString());
                                    if (adminMaps.Pins[p].Tag.ToString().Equals(requestsInfos[i].Split(',')[3]))
                                    {
                                        // bu duraktaki mevcut yolcu sayısını bul ve güncelle
                                        // program açılırken
                                        waitingPessangersCount++; // = Convert.ToInt32(adminMaps.Pins[p].Address.Split(':')[1].Trim());
                                    }
                                }
                                adminMaps.Pins[p].Address = "Bekleyen yolcu sayısı: " + waitingPessangersCount.ToString();
                            }
                        }
                        else
                        {
                            // program içinde ekleme yapılırken
                            // seçili durağı güncelle
                            int count = 0;
                            for (int i = 0; i < requestsInfos.Count; i++)
                            {
                                if (adminMaps.SelectedPin.Tag.ToString().Equals(requestsInfos[i].Split(',')[3]))
                                    count++;

                            }
                            adminMaps.SelectedPin.Address = "Bekleyen yolcu sayısı: " + count.ToString();
                            Pin selectedPin = adminMaps.SelectedPin;
                            adminMaps.SelectedPin = null;
                            adminMaps.SelectedPin = selectedPin;
                        }

                        pessangers.ItemsSource = showUsers;
                    }
                }
                else
                {
                    // kullanıcı için
                    await httpRequestToApi(10, new List<string>() { "10", userId.ToString() });
                    if (resJson.result.Equals("İstek Başarılı."))
                    {
                        List<string> userTripRequestList = new List<string>();
                        // kullanıcının bu zamana kadar olan tüm istekleri
                        for(int i = 0; i < resJson.returnData.Count; i++)
                        {
                            if( i+1 == resJson.returnData.Count)
                            {
                                // son kayıt
                                if (resJson.returnData[i].Split(';')[0].Split(',')[2].Equals("0"))
                                {
                                    // son kayıt onay bekliyor
                                    userTripRequest.Text = "Sefer Durumu: " + resJson.returnData[i].Split(';')[0].Split(',')[0] + " numaralı talebiniz için onay bekleniyor.";
                                    userTripRequest.TextColor = Color.Blue;
                                }
                                else if (resJson.returnData[i].Split(';')[0].Split(',')[2].Equals("1"))
                                {
                                    //Console.WriteLine("Girdi");
                                    // son kayıt onaylanmış
                                    userTripRequest.Text = "Sefer Durumu: " + resJson.returnData[i].Split(';')[0].Split(',')[0] + " numaralı talebiniz onaylanmıştır.";
                                    userTripRequest.TextColor = Color.Green;

                                    // son seferi haritada göster
                                    Bus tempBus = new Bus(int.MaxValue, Color.Black);
                                    // sefer güzergahındaki duraklar
                                    string stationIds = resJson.returnData[i].Split(';')[1];
                                    string[] stations = stationIds.Split(',');
                                    // umuttepeyi ekleme
                                    await getDirection(-1, Convert.ToInt32(stations[0]), false, tempBus);
                                   
                                    for (int j = 1; j < stations.Length; j++)
                                    {
                                        if (!string.IsNullOrEmpty(stations[j]))
                                            await getDirection(Convert.ToInt32(stations[j - 1]), Convert.ToInt32(stations[j]), false, tempBus);
                                        
                                    }
                                    
                                    userMaps.Polylines.Clear();
                                    userMaps.Polylines.Add(tempBus.routePolyLine);
                                    //loadingOverlayDisable();
                                    //userMaps.IsVisible = true;
                                }
                                else
                                {
                                    // son kayıt reddedilmiş
                                    userTripRequest.Text = "Sefer Durumu: " + resJson.returnData[i].Split(';')[0].Split(',')[0] + " numaralı talebiniz reddedilmiştir.";
                                    userTripRequest.TextColor = Color.Red;
                                }


                                
                            }
                        }
                    }
                }
            }

            

            async Task createRequest(string stationId, string userIds)
            {
                await httpRequestToApi(9, new List<string>() { "9", stationId, userIds });
                if (resJson.result.Equals("İstek Başarılı."))
                {
                   await getRequests(false);
                }
                else
                {
                    _ = DisplayAlert("Hata", resJson.description, "Anladım");
                }
            }

            List<Position> decodeGoogleDirectionStirng (string polylineString)
            {
                List<Position> result = new List<Position>();
                if (string.IsNullOrEmpty(polylineString))
                    throw new ArgumentNullException(nameof(polylineString));

                var polylineChars = polylineString.ToCharArray();
                var index = 0;

                var currentLat = 0;
                var currentLng = 0;

                while (index < polylineChars.Length)
                {
                    // Next lat
                    var sum = 0;
                    var shifter = 0;
                    int nextFiveBits;
                    do
                    {
                        nextFiveBits = polylineChars[index++] - 63;
                        sum |= (nextFiveBits & 31) << shifter;
                        shifter += 5;
                    } while (nextFiveBits >= 32 && index < polylineChars.Length);

                    if (index >= polylineChars.Length)
                        break;

                    currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                    // Next lng
                    sum = 0;
                    shifter = 0;
                    do
                    {
                        nextFiveBits = polylineChars[index++] - 63;
                        sum |= (nextFiveBits & 31) << shifter;
                        shifter += 5;
                    } while (nextFiveBits >= 32 && index < polylineChars.Length);

                    if (index >= polylineChars.Length && nextFiveBits >= 32)
                        break;

                    currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                    result.Add(new Position(Convert.ToDouble(currentLat) / 1E5, Convert.ToDouble(currentLng) / 1E5));
                }
                return result;
            }


            async Task getDirection(int fromStationIndex, int toStationIndex, bool onlyDistance, Bus bus = null)
            {
                //Console.WriteLine("fromIndex: " + fromStationIndex.ToString() + " toIndex: " + toStationIndex.ToString());
                string fromLat = "";
                string fromLong = "";
                if (fromStationIndex == -1)
                {
                    // umuttepe durağı
                    fromLat = umuttepe.Latitude.ToString().Replace(',','.');
                    fromLong = umuttepe.Longitude.ToString().Replace(',','.');
                    //Console.WriteLine("Girmiştir");
                }
                else
                {
                    fromLat = stationsInfos[fromStationIndex-1].Split(',')[2];
                    fromLong = stationsInfos[fromStationIndex-1].Split(',')[3];
                }
                
                string toLat = stationsInfos[toStationIndex-1].Split(',')[2];
                string toLong = stationsInfos[toStationIndex-1].Split(',')[3];
                using (var client = new HttpClient())
                {
                    var directionApiUri = new Uri("https://maps.googleapis.com/maps/api/directions/json?origin=" + fromLat + "," + fromLong + "&destination=" + toLat + "," + toLong + "&key=[GoogleMap-Api-Girin]");
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, directionApiUri);
                    HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
                    var result = await responseMessage.Content.ReadAsStringAsync();
                    GoogleDirection gD = JsonConvert.DeserializeObject<GoogleDirection>(result);
                    //Console.WriteLine(result);
                    // iki nokta arası mesafe (metre cinsinden) gD.routes[0].legs[0].distance.value
                    // iki nokta arası konumlar gD.routes[0].legs[0].steps
                    if (onlyDistance)
                    {
                        // sadece aradaki mesafeyi grapha ata
                        stationGraph[(fromStationIndex == -1) ? fromStationIndex + 1 : fromStationIndex, toStationIndex ] = gD.routes[0].legs[0].distance.value * 0.001F; // km cinsi
                        stationGraph[toStationIndex, (fromStationIndex == -1) ? fromStationIndex + 1 : fromStationIndex] = gD.routes[0].legs[0].distance.value * 0.001F;
                    }
                    else
                    {
                        // iki nokta arasında ki noktaları al otobüsü classına ata
                        for(int i = 0; i < gD.routes[0].legs[0].steps.Count; i++)
                        {
                            foreach(Position p in decodeGoogleDirectionStirng(gD.routes[0].legs[0].steps[i].polyline.points))
                            {
                                bus.routePolyLine.Positions.Add(p);
                            }
                        }
                    }
                }

            }
            
            async Task confirmTrip()
            {
                // seferi veritabanına kaydet
                string tripName = DateTime.Now.ToString("F") + " tarihli sefer";
                //List<string> otherIds = new List<string>();
                string stationIds = "";
                string pessangerIdCount = "";
                string tripRequestIds = "";

                for (int i = 0; i < requestIds.Count; i++)
                    tripRequestIds += requestIds[i] + ",";

                //tripRequestIds = requestIds;
                for(int i = 0; i < busses.Count; i++)
                {
                    // otobüsün duraklarını ekle
                    for (int j = 0; j < busses[i].stationsIndex.Count; j++)
                    {
                        stationIds += busses[i].stationsIndex[j].ToString() + ",";      
                        pessangerIdCount += (busses[i].takingPessagerCount[j]).ToString() + ",";
                        
                    }
                    stationIds += ";";
                    pessangerIdCount += ";";
                }

                // veritabanına kaydetmek için apiye istek at
                await httpRequestToApi(11, new List<string> { "11",stationIds, pessangerIdCount, tripRequestIds , tripName});
                
                // kullanıcıları güncellemek için apiye istek at
                await getRequests(false);
                

            }

            async Task createStationGraph()
            {
                loadingOverlayEnable("Sefer Düzenleniyor...");
                // duraklar arası mesafeyi grapha ekle

                // ilk durak umuttepe grapha ekle
                stationGraph = new float[stationsInfos.Count + 1, stationsInfos.Count + 1]; // 13 x 13
                //Console.WriteLine(stationGraph[1, 4].ToString() + "asdasdasdasdasfggsdaghsdfaha");
                
                for (int i = 0; i < stationsInfos.Count + 1; i++) {
                    for(int j = 0; j < stationsInfos.Count + 1; j++)
                    {
                        //stationGraph[i, j] = 0;
                        if(i == j)
                        {
                            //Console.WriteLine("i j aynı");
                            stationGraph[i, j] = 0;
                        }
                        else if (i == 0)
                        {
                            // umuttepe için mesafeler
                            //Console.WriteLine("i 0");
                            await getDirection(-1, j, true, null);
                        }
                        else
                        {
                            // daha önce doldurulmadıysa
                            //Console.WriteLine("i j farklı");
                            if (i != j+1 && stationGraph[i,j] == 0)
                            {
                                //Console.WriteLine("i j+1 farklı");
                                await getDirection(i, j, true, null);
                            }
                                
                        }
                        
                    }
                }
                printStationGraph();
                loadingOverlayDisable();

            }

            void printStationGraph()
            {
                Console.WriteLine("Station Graph------------------------------------------------------------------");
                for (int s = 0; s < stationsInfos.Count + 1; s++)
                {
                    Console.Write("\t" + ((char)(s + 65)).ToString());
                }
                Console.WriteLine("");

                for (int i = 0; i < stationsInfos.Count + 1; i++)
                {
                    Console.Write(((char)(i + 65)).ToString() + "\t");
                    for (int j = 0; j < stationsInfos.Count + 1; j++)
                    {
                        Console.Write(stationGraph[i,j].ToString() + "\t");
                    }
                    Console.WriteLine("");
                }
                Console.WriteLine("-------------------------------------------------------------------------------");
            }

            // yolcuların maliyetini hesaplar
            float costCalculator(float distance, int capacity, int pessangerCount)
            {
                return (float)((distance + capacity) / pessangerCount) + distance; 
            }

            // yolcuları otobüse bindirir rekürsif fonksiyon
            void takeABus(int busIndex ,int[] tempStationIndex, List<List<int>> busStationIndexList, int[] tempStationPessangerCountArray, float[] busMinCostsStations)
            {
                for(int i = busIndex; i < busses.Count; i++)
                {
                    // otobüs önceden dolmuşşsa atla
                    if (busses[i].capacity == busses[i].pessangerCount)
                    {
                        Console.WriteLine(i.ToString() + ". otobüs dolu atlanıyor.");
                        continue;
                    }

                    // otobüs içine yolcuları al ve duraktaki yolcu sayısını güncelle

                    

                    int emptySeatCount = busses[i].capacity - busses[i].pessangerCount;
                    int takingPessangerCount = (tempStationPessangerCountArray[tempStationIndex[i] - 1] > emptySeatCount) ? emptySeatCount : tempStationPessangerCountArray[tempStationIndex[i] - 1];

                    if (takingPessangerCount == 0)
                        continue;

                    busses[i].stationsIndex.Add(tempStationIndex[i].ToString());
                    busStationIndexList[i].Add(tempStationIndex[i]);

                    busses[i].pessangerCount += takingPessangerCount;
                    busses[i].takingPessagerCount.Add(takingPessangerCount);
                    Console.WriteLine(i.ToString() + ". otübüse " + takingPessangerCount.ToString() + " yolcu biniyor.");
                    tempStationPessangerCountArray[tempStationIndex[i] - 1] -= takingPessangerCount;
                    Console.WriteLine(tempStationIndex[i].ToString() + ". duraktaki kalan yolcu sayısı " + tempStationPessangerCountArray[tempStationIndex[i] - 1].ToString());

                    // maliyeti ekle
                    busses[i].cost += busMinCostsStations[i];

                    // otobüs için en az maliyetli durağın indexi
                    int minCostStationIndex = tempStationIndex[i];
                    // bu index başka hangi otobüslerde var
                    List<int> sameIndex = new List<int>();
                    for (int j = i + 1; j < busses.Count; j++)
                    {
                        // eğer aynı index başka bir otobüs için geçerliyse
                        if (tempStationIndex[j] == tempStationIndex[i])
                        {
                            Console.WriteLine(j.ToString() + ". otobüsün durağı aynı sameIndexe eklendi");
                            sameIndex.Add(j);
                        }

                    }

                    // aynı indexe sahip otobüsleri atla farklıları devam ettir.

                    for (int j = i + 1; j < busses.Count; j++)
                    {
                        if (sameIndex.Contains(j))
                        {
                            Console.WriteLine(j.ToString() + ". otobüs atlanıyor.");
                            continue;
                        }
                        else
                        {
                            // otobüs önceden dolmuşşsa atla
                            if (busses[j].capacity == busses[j].pessangerCount)
                            {
                                Console.WriteLine(i.ToString() + ". otobüs dolu atlanıyor.");
                                continue;
                            }
                            else
                                takeABus(j, tempStationIndex, busStationIndexList, tempStationPessangerCountArray, busMinCostsStations);
                        }
                    }
                    Console.WriteLine("Min değerler sıfırlanıyor.");
                    for (int j = 0; j < busses.Count; j++)
                        busMinCostsStations[j] = float.MaxValue;
                    break;
                }

            }

            async Task createATrip(bool infiniteBus)
            {

                await createStationGraph();

                // sefer düzenleme
                busses = new List<Bus>();
                busses.Clear();

                busses.Add(new Bus(40, Color.Red));
                busses.Add(new Bus(30, Color.Green));
                busses.Add(new Bus(25, Color.Blue));

                
                // durakta bekleyen yolcu sayılarını diziye ata
                int totalPessangerCount = 0;
                int[] tempStationPessangerCountArray = new int[adminMaps.Pins.Count];
                for (int i = 0; i < tempStationPessangerCountArray.Length; i++)
                {
                    tempStationPessangerCountArray[i] = Convert.ToInt32(adminMaps.Pins[i].Address.Split(':')[1].Trim());
                    totalPessangerCount += tempStationPessangerCountArray[i];
                }
                    
                if (infiniteBus)
                {
                    // sonsuz otobüs hakkı varsa yolcuların hepsini toplayacak kadar otobüs ayarlanmalı
                    if (totalPessangerCount > busses[0].capacity + busses[1].capacity + busses[2].capacity)
                    {
                        int extraBusCount = (((busses[0].capacity + busses[1].capacity + busses[2].capacity) - totalPessangerCount) % 25 == 0) ? (((busses[0].capacity + busses[1].capacity + busses[2].capacity) - totalPessangerCount) / 25 ) : (((busses[0].capacity + busses[1].capacity + busses[2].capacity) - totalPessangerCount) / 25) + 1;
                        for (int i = 0; i < extraBusCount; i++)
                            busses.Add(new Bus(25, busColors[i], 25));
                    }
                }

                // tüm otobüsler için sezgisel fonksiyonu hesapla
                List<List<int>> busStationIndexList = new List<List<int>>();
                int[] tempStationIndex = new int[busses.Count];
                float[] busMinCostsStations = new float[busses.Count];
                for (int i = 0; i < busses.Count; i++)
                {
                    // tüm otobüsler için uğradıkları durakların listelerini ekle ve başlangıç olarak 0 ata
                    busStationIndexList.Add(new List<int>() { 0 });

                    // otobüsler için gecic durak değişkenleri dizisini oluştur ve başlangıç değeri olarak 0 ata
                    tempStationIndex[i] = 0;

                    // tüm durakların maliyetinin max değer olarak ata
                    busMinCostsStations[i] = float.MaxValue;
                }

                bool over = false;
                int emptySeatCount = 0;

                while (!over)
                {
                    Console.WriteLine("------------------------------------------------------------------------------------------------");
                    Console.WriteLine("Over devam ediyor");
                    // duraklardaki tüm yolcular bitti mi

                    for(int i =0; i < tempStationPessangerCountArray.Length; i++)
                    {
                        if (tempStationPessangerCountArray[i] != 0)
                        {
                            over = false;
                            break;
                        }
                        else
                            over = true;
                                
                    }

                    // eğer yolcular bittiye döngüyü kır
                    if (over)
                        break;

                        
                    // otobüslerde boş koltuk kaldı mı
                    for(int i = 0; i < busses.Count; i++)
                    {
                        // eğer otobüslerde boş koltuk kaldıysa 
                        if (busses[i].pessangerCount != busses[i].capacity)
                        {
                            over = false;
                            break;
                        }
                        else
                            over = true;
                    }

                    // otobüsler tamamen dolduysa döngüyü bitir
                    if (over)
                        break;


                    // tüm durakları gez
                    for(int j = 1; j < stationsInfos.Count + 1; j++)
                    {
                        Console.WriteLine("Durak: " + (j).ToString());

                        // durataki yolcu sayısını değişkene ata
                        int stationPessangerCount = tempStationPessangerCountArray[j - 1];
                        // eğer durakta yolcu yoksa durağı atla
                        if (stationPessangerCount == 0)
                            continue;
                        Console.WriteLine("Duraktaki yolcu sayısı: " + stationPessangerCount.ToString());

                        // her otobüs için döngüyü başlat
                        for (int i = 0; i < busses.Count; i++)
                        {

                            Console.WriteLine("Otobüs: " + i.ToString());

                            // eğer bu otobüs doluysa otobüsü atla
                            if (busses[i].pessangerCount == busses[i].capacity)
                                continue;

                            // otobüsteki boş koltuk sayısını değişkene ata
                            emptySeatCount = busses[i].capacity - busses[i].pessangerCount;
                            Console.WriteLine("Otobüsteki boş koltuk sayısı: " + emptySeatCount.ToString());

                            // eğer duraktaki yolcu sayısı boş koltuk sayısından fazla ise o durak için yolcu sayısını boş koltuk sayısı olarak al
                            if (stationPessangerCount > emptySeatCount)
                                stationPessangerCount = emptySeatCount;

                            Console.WriteLine("Otobüsün güncel en düşük maliyetli durağı: " + tempStationIndex[i].ToString() + " maliyeti: " + busMinCostsStations[i].ToString());

                            // durak maliyetini hesapla (station graph indexlerinin dğoru geldiğinden emin ol)
                            //Console.WriteLine("----stationGraph[" + (busStationIndexList[i][busStationIndexList[i].Count - 1]).ToString() + "," + j.ToString() + "] = " + stationGraph[busStationIndexList[i][busStationIndexList[i].Count - 1], j].ToString());
                            float tempMinCostStation = costCalculator(stationGraph[busStationIndexList[i][busStationIndexList[i].Count - 1],j], emptySeatCount, stationPessangerCount);
                            Console.WriteLine("Otobüs için hesaplanan durak maliyeti: " + tempMinCostStation.ToString());

                            // eğer daha az maliyetli durak bulunduysa
                            if(tempMinCostStation < busMinCostsStations[i])
                            {
                                // yeni en düşük maliyeti belirle
                                busMinCostsStations[i] = tempMinCostStation;
                                // en düşük maliyetli duğarı belirle
                                    
                                tempStationIndex[i] = j;
                                Console.WriteLine("Otobüs için daha az maliyetli durak bulundu otobüsün yeni durak hedefi: " + j.ToString());
                            }
                        }

                    }


                    // rekürsif fonksiyonla yolcuları otobüslere yerleştir
                    takeABus(0, tempStationIndex, busStationIndexList, tempStationPessangerCountArray, busMinCostsStations);
                }
                Console.WriteLine("Over Bitti");

                for (int i = 0; i < busses.Count; i++)
                {
                    Console.Write(i.ToString() + ". otobüs durakları: ");
                    for(int j = 0; j < busses[i].stationsIndex.Count; j++)
                    {
                        Console.Write(busses[i].stationsIndex[j] + ",");
                    }
                    Console.WriteLine("Toplam maliyet: " + busses[i].cost.ToString());
                }

                // veritabanına kayıt et
                await confirmTrip();

                string result = "Sefer düzenleme tamamlandı.\n";
                // güzerhanları haritada çizdir
                for(int i = 0; i < busses.Count; i++)
                {
                    // otobüs boşsa atla
                    if (busses[i].pessangerCount == 0)
                        continue;

                    result += busses[i].capacity.ToString() + " kişilik otobüs maliyeti: " + busses[i].cost.ToString() + "\n"; 
                    // umuttepe için
                    await getDirection(-1, Convert.ToInt32(busses[i].stationsIndex[0]), false, busses[i]);
                    for(int j = 1; j < busses[i].stationsIndex.Count; j++)
                    {
                        await getDirection(Convert.ToInt32(busses[i].stationsIndex[j - 1]), Convert.ToInt32(busses[i].stationsIndex[j]), false, busses[i]);   
                    }
                    adminMaps.Polylines.Add(busses[i].routePolyLine);
                }

                // sonucu ekranda göster 
                _ = DisplayAlert("Sefer Düzenleme", result, "Tamam");

                
            }


            // ----------------------------------------------------------------------------------------------------------



            // Loading Overlay -------------------------------------------------------------------------------------------
            Image loadingLogo = new Image { Source = "load.gif", IsAnimationPlaying = true };
            Label infoLabel = new Label { Text = "Yükleniyor...", TextColor = Color.DarkGray, FontFamily = "Open-Sans", FontSize = 14 };


            loadingOverlay = new FlexLayout {

                BackgroundColor = Color.White,
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Center,
                JustifyContent = FlexJustify.Center,
                IsEnabled = false,
                IsVisible = false,
                Children =
                {
                    loadingLogo,
                    infoLabel,
                }
            };

            void loadingOverlayEnable(string info)
            {
                infoLabel.Text = info;
                loadingOverlay.IsVisible = true;
                loadingOverlay.IsEnabled = true;
            }

            void loadingOverlayDisable()
            {
                loadingOverlay.IsVisible = false;
                loadingOverlay.IsEnabled = false;
            }

            // -------------------------------------------------------------------------------------------------------------




            // Login - Singin Overlay --------------------------------------------------------------------------------------

            // Logo

            //Image logo = new Image { Source = ImageSource.FromFile("YazlabTurizmLogoKucuk.png"), HorizontalOptions = LayoutOptions.CenterAndExpand};

            // Login Tab ---------------------------------------------------------------------------------------------------
            void telephoneNumberEdit(object sender, EventArgs args)
            {
                if (((Entry)sender).Text.StartsWith("0"))
                    ((Entry)sender).Text = "";
            }

            CheckBox adminCheckLogin = new CheckBox { IsChecked = false };

            Entry userTelephone = new Entry { Placeholder = "Telefon Numarası" , Keyboard = Keyboard.Telephone, MaxLength = 10, HorizontalTextAlignment = TextAlignment.Center};
            userTelephone.TextChanged += telephoneNumberEdit;
            Entry userPassword = new Entry { Placeholder = "Şifre", Keyboard = Keyboard.Default, IsPassword = true, WidthRequest = 100, HorizontalTextAlignment = TextAlignment.Center };
            Button loginButton = new Button { Text = "Giriş", FontFamily = "ComfortaaBold", FontSize = 18, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke };
            Button signInPageButton = new Button { Text = "Üye olmak istiyorum", FontFamily = "ComfortaaBold", FontSize = 18, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke };

            async void login(object sender, EventArgs args)
            {
                
                if (string.IsNullOrEmpty(userTelephone.Text))
                    _ = DisplayAlert("Hata", "Telefon numarası girin.", "Anladım");
                else if (userTelephone.Text.Length < 10)
                    _ = DisplayAlert("Hata", "Telefon numarasını eksik girdiniz.", "Anladım");
                else if (string.IsNullOrEmpty(userPassword.Text))
                    _ = DisplayAlert("Hata", "Lütfen şifre girin.", "Anladım");
                else
                {
                    // giriş yapma işlemi bekleniyor....
                    if (adminCheckLogin.IsChecked)
                    {
                        await httpRequestToApi(2, new List<string>() { "2", userTelephone.Text, "", userPassword.Text });
                        if (resJson.result.Equals("İstek Başarılı."))
                        {
                            // kullanıcı girişi olmadığı için userId = 0
                            userId = 0;
                            userName = "";
                            adminId = Convert.ToInt32(resJson.returnData[0]);

                            // admin girişi olduğu için tüm kullanıcıları api ile çek
                            usersInfos = new List<string>();
                            await getUsers();
                            // admin map sayfası aç
                            mapOverlayUser.IsVisible = false;
                            mapOverlayUser.IsEnabled = false;
                            
                            // durakları apiden çek
                            stationsInfos = new List<string>();
                            await getStations();

                            //stationPicker.SelectedIndex = 0;

                            // istek listesini apiden al
                            requestsInfos = new List<string>();
                            await getRequests(true);

                            overlaySwap(mapOverlayAdmin, loginOverlay);

                        }
                        else if (resJson.result.Equals("İstek Reddedildi."))
                        {
                            _ = DisplayAlert(resJson.result, resJson.description, "Tamam");
                        }
                    }
                    else
                    {
                        await httpRequestToApi(4, new List<string>() { "4", userTelephone.Text, "", userPassword.Text });
                        if (resJson.result.Equals("İstek Başarılı."))
                        {
                            // kullanıcı map sayfası aç
                            adminId = 0;
                            userId = Convert.ToInt32(resJson.returnData[0]);
                            userName = resJson.returnData[1].ToString();
                            userLabel.Text = "Hoşgeldin " + userName;
                            //Console.WriteLine(userLabel.Text);
                            mapOverlayAdmin.IsVisible = false;
                            mapOverlayAdmin.IsEnabled = false;

                            stationsInfos = new List<string>();
                            await getStations();
                            //userStationPicker.SelectedIndex = 0;

                            requestsInfos = new List<string>();
                            //Console.WriteLine("User ID: " + userId);
                            //userMaps.IsVisible = false;
                            await getRequests(true, userId);
                            overlaySwap(mapOverlayUser, loginOverlay);



                        }
                        else if (resJson.result.Equals("İstek Reddedildi."))
                        {
                            _ = DisplayAlert(resJson.result, resJson.description, "Tamam");

                        }
                    }
                    userTelephone.Text = "";
                    userPassword.Text = "";
                    adminCheckLogin.IsChecked = false;
                }

            }

            loginButton.Clicked += login;

            loginOverlay = new FlexLayout {
                BackgroundColor = Color.WhiteSmoke,
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Center,
                JustifyContent = FlexJustify.Center,
                Children =
                {
                    //yoneticiLabel,
                    //yoneticiCheckbox,
                    new Image { Source = ImageSource.FromFile("YazlabTurizmLogoKucuk.png"), HorizontalOptions = LayoutOptions.CenterAndExpand},
                    new Label {Text = "Giriş Yap", FontFamily = "ComfortaaBold", FontSize = 30, FontAttributes = FontAttributes.Bold, TextColor = Color.Black },
                    userTelephone,
                    userPassword,
                    new Label {Text = "Yönetici Girişi", FontFamily = "ComfortaaBold", FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Color.Black },
                    adminCheckLogin,
                    loginButton,
                    signInPageButton,
                }
            };


            // -------------------------------------------------------------------------------------------------------------




            // SignIn Tab --------------------------------------------------------------------------------------------------
            CheckBox adminCheck = new CheckBox { IsChecked = false };
            Entry userSingInTelephone = new Entry { Placeholder = "Telefon Numarası", Keyboard = Keyboard.Telephone, MaxLength = 10, HorizontalTextAlignment = TextAlignment.Center };
            userSingInTelephone.TextChanged += telephoneNumberEdit;
            Entry userSingInName = new Entry { Placeholder = "İsim Soyisim", Keyboard = Keyboard.Text, MaxLength = 20, HorizontalTextAlignment = TextAlignment.Center };
            Entry userSignInPassword = new Entry { Placeholder = "Şifre", Keyboard = Keyboard.Default, MaxLength = 20, IsPassword = true, WidthRequest = 100, HorizontalTextAlignment = TextAlignment.Center };
            Entry userSignInPasswordRepeat = new Entry { Placeholder = "Şifre Tekrar", Keyboard = Keyboard.Default, MaxLength = 20, IsPassword = true, WidthRequest = 100, HorizontalTextAlignment = TextAlignment.Center };
            Button signInButton = new Button { Text = "Kayıt", FontFamily = "ComfortaaBold", FontSize = 18, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke };
            Button loginPageButton = new Button { Text = "Zaten üyeliğim var", FontFamily = "ComfortaaBold", FontSize = 18, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke };
            

            async void singIn(object sender, EventArgs args)
            {
                // Kullanıcı Kayıt olma fonksiyonu api reqType 3

                if (string.IsNullOrEmpty(userSingInTelephone.Text))
                    _ = DisplayAlert("Hata", "Telefon numarası girin.", "Anladım");
                else if (userSingInTelephone.Text.Length < 10)
                    _ = DisplayAlert("Hata", "Telefon numarasını eksik girdiniz.", "Anladım");
                else if (string.IsNullOrEmpty(userSingInName.Text))
                    _ = DisplayAlert("Hata", "Lütfen adınızı girin.", "Anladım");
                else if (string.IsNullOrEmpty(userSignInPassword.Text))
                    _ = DisplayAlert("Hata", "Lütfen şifre girin.", "Anladım");
                else if (string.IsNullOrEmpty(userSignInPasswordRepeat.Text))
                    _ = DisplayAlert("Hata", "Lütfen şifrenizi tekrar girin.", "Anladım");
                else if (!userSignInPassword.Text.Equals(userSignInPasswordRepeat.Text))
                {
                    _ = DisplayAlert("Hata", "Girdiğiniz şifreler eşleşmiyor.", "Anladım");
                    userSignInPassword.Text = "";
                    userSignInPasswordRepeat.Text = "";
                    _ = userSignInPassword.Focus();
                }
                else
                {
                    // kayıt olma işlemi bekleniyor....
                    if (adminCheck.IsChecked)
                    {
                        await httpRequestToApi(1, new List<string>() { "1", userSingInTelephone.Text, userSingInName.Text, userSignInPassword.Text });
                        if(resJson.result.Equals("İstek Başarılı."))
                        {
                            // giriş sayfası aç
                            overlaySwap(loginOverlay, singInOverlay);
                            
                        }
                        else if(resJson.result.Equals("İstek Reddedildi."))
                        {
                            _ = DisplayAlert(resJson.result, resJson.description, "Tamam");
                            
                        }
                    }
                    else
                    {
                        await httpRequestToApi(3, new List<string>() { "3", userSingInTelephone.Text, userSingInName.Text, userSignInPassword.Text });
                        if (resJson.result.Equals("İstek Başarılı."))
                        {
                            // giriş saysası aç
                            overlaySwap(loginOverlay, singInOverlay);
                        }
                        else if (resJson.result.Equals("İstek Reddedildi."))
                        {
                            _ = DisplayAlert(resJson.result, resJson.description, "Tamam");

                        }
                    }
                    userSingInTelephone.Text = "";
                    userSingInName.Text = "";
                    userSignInPassword.Text = "";
                    userSignInPasswordRepeat.Text = "";
                    adminCheck.IsChecked = false;
                }
                    
            }

            signInButton.Clicked += singIn;

            singInOverlay = new FlexLayout
            {
                IsEnabled = false,
                IsVisible = false,
                BackgroundColor = Color.WhiteSmoke,
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Center,
                JustifyContent = FlexJustify.Center,
                Children =
                {
                    new Image { Source = ImageSource.FromFile("YazlabTurizmLogoKucuk.png"), HorizontalOptions = LayoutOptions.CenterAndExpand},
                    new Label {Text = "Kayıt Ol", FontFamily = "ComfortaaBold", FontSize = 30, FontAttributes = FontAttributes.Bold, TextColor = Color.FromRgb(77,77,77), BackgroundColor = Color.WhiteSmoke},
                    userSingInTelephone,
                    userSingInName,
                    userSignInPassword,
                    userSignInPasswordRepeat,
                    new Label {Text = "Yönetici Kaydı", FontFamily = "ComfortaaBold", FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Color.Black },
                    adminCheck,
                    signInButton,
                    loginPageButton,
                }
            };

            // Login - Sign Page Slide
            loginPageButton.Clicked += (object sender, EventArgs args) => { 
                singInOverlay.IsVisible = false; 
                singInOverlay.IsEnabled = false;
                loginOverlay.IsEnabled = true;
                loginOverlay.IsVisible = true;
            };

            signInPageButton.Clicked += (object sender, EventArgs args) => {
                loginOverlay.IsVisible = false;
                loginOverlay.IsEnabled = false;
                singInOverlay.IsEnabled = true;
                singInOverlay.IsVisible = true;
            };
            // -----------------------


            // -------------------------------------------------------------------------------------------------------------


            // Map Overlay -------------------------------------------------------------------------------------------------

            // Admin Panel -------------------------------------------------------------------------------------------------
            adminMaps = new Map { HorizontalOptions = LayoutOptions.FillAndExpand, WidthRequest = 400, HeightRequest = 400, IsTrafficEnabled = false};
            Position izmit = new Position(40.7760014, 29.9396022);
            adminMaps.MoveToRegion(MapSpan.FromCenterAndRadius(izmit, Xamarin.Forms.GoogleMaps.Distance.FromKilometers(7)));
            stationName = new Entry {Text = "İstasyon Adı Girin" , Placeholder = "İstasyon Adı", Keyboard = Keyboard.Default, MaxLength = 20, HorizontalTextAlignment = TextAlignment.Center, FontSize = 12, WidthRequest = 120};
            stationName.Text = "";
            Entry stationLat = new Entry { Placeholder = "Latitude", Keyboard = Keyboard.Numeric, MaxLength = 20, HorizontalTextAlignment = TextAlignment.Center, FontSize = 12, WidthRequest = 120, IsReadOnly = true};
            Entry stationLong = new Entry { Placeholder = "Longitude ", Keyboard = Keyboard.Numeric, MaxLength = 20, HorizontalTextAlignment = TextAlignment.Center, FontSize = 12, WidthRequest = 120, IsReadOnly = true};
            Button stationAddButton = new Button { Text = "Ekle", FontFamily = "ComfortaaBold", FontSize = 12, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke, WidthRequest = 120};

            stationPicker = new Picker { WidthRequest = 125, HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, FontFamily = "ComfortaaBold", FontSize = 12, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke, SelectedIndex = 0 };
            pessangers = new CollectionView { HorizontalOptions = LayoutOptions.Center, WidthRequest = 250, HeightRequest = 80, BackgroundColor = Color.WhiteSmoke, SelectionMode = SelectionMode.Multiple};

            Button passangersAddToStationButton = new Button { Text = "Yolcuları Ekle", FontFamily = "ComfortaaBold", FontSize = 12, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke, WidthRequest = 125};
            Button tripConfirmButton = new Button { Text = "Seferi Onayla", FontFamily = "ComfortaaBold", FontSize = 12, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke, WidthRequest = 125 };
            CheckBox infinityBusCheck = new CheckBox { IsChecked = false };
            Button adminLogoutButton = new Button { Text = "Çıkış Yap", FontFamily = "ComfortaaBold", FontSize = 12, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke, WidthRequest = 125 };

            stationPicker.SelectedIndexChanged += (object sender, EventArgs args) =>
            {
                // Seçilen pini haritada ortala
              
                int selectedPinIndex = ((Picker)sender).SelectedIndex;
                Position selectedPinPosition = new Position(Convert.ToDouble(stationsInfos[selectedPinIndex].Split(',')[2]),Convert.ToDouble(stationsInfos[selectedPinIndex].Split(',')[3]));
                if(adminId != 0)
                {
                    adminMaps.MoveToRegion(MapSpan.FromCenterAndRadius(selectedPinPosition, Xamarin.Forms.GoogleMaps.Distance.FromMeters(300)));
                    adminMaps.SelectedPin = adminMaps.Pins[selectedPinIndex];
                }
                else if(userId != 0)
                {
                    userMaps.MoveToRegion(MapSpan.FromCenterAndRadius(selectedPinPosition, Xamarin.Forms.GoogleMaps.Distance.FromMeters(300)));
                    userMaps.SelectedPin = userMaps.Pins[selectedPinIndex];
                }
                //Console.WriteLine("Geçti");
            };


            

            
            adminMaps.MapLongClicked += (object sender, MapLongClickedEventArgs args) =>
            {
                stationLat.Text = args.Point.Latitude.ToString();
                stationLong.Text = args.Point.Longitude.ToString();
            };

            passangersAddToStationButton.Clicked += async (object sender, EventArgs args) => {
                

                // seçilen kullanıcıları admin olarak seçilen durağa ekle
                string selectedStationId = adminMaps.SelectedPin.Tag.ToString();
                string selectedUsersIds = "";
                for(int i = 0; i < pessangers.SelectedItems.Count; i++)
                {
                    selectedUsersIds += pessangers.SelectedItems[i].ToString().Split('\t')[0];
                    if (i + 1 != pessangers.SelectedItems.Count)
                        selectedUsersIds += ",";
                }
                //Console.WriteLine("Seçilen durak Id: " + selectedStationId + " Seçilen kullanıcılar Id: " + selectedUsersIds);
                pessangers.SelectedItems.Clear();
                await createRequest(selectedStationId, selectedUsersIds);
                
            };

            
            // ------------

            stationAddButton.Clicked += async (object sender, EventArgs args) => {
                if (string.IsNullOrEmpty(stationName.Text.Trim())){
                    
                    _ = DisplayAlert("Hata", "Durak adı boş olamaz.", "Anladım");
                    stationName.Focus();
                }
                else if (string.IsNullOrEmpty(stationLat.Text))
                {
                    _ = DisplayAlert("Hata", "Haritadan bir konum seçin.", "Anladım");
                }
                else
                {
                    await httpRequestToApi(5, new List<string>() {"5",stationName.Text,stationLat.Text,stationLong.Text});
                    if(resJson.result.Equals("İstek Başarılı."))
                    {
                        await getStations(); // stationsInfo listesinde güncel duraklar atandı.
                    }
                    stationName.Text = "";
                    stationLat.Text = "";
                    stationLong.Text = "";
                    
                }

            };

            adminLogoutButton.Clicked += (object sender, EventArgs args) =>
            {
                loadingOverlayEnable("Çıkış Yapılıyor...");
                adminId = 0;
                overlaySwap(loginOverlay, mapOverlayAdmin);
                loadingOverlayDisable();
            };

            tripConfirmButton.Clicked += async (object sender, EventArgs args) =>
            {
                // durakta bekleyen yolcu sayılarını diziye ata
                int totalPessangerCount = 0;
                int[] tempStationPessangerCountArray = new int[adminMaps.Pins.Count];
                for (int i = 0; i < tempStationPessangerCountArray.Length; i++)
                {
                    tempStationPessangerCountArray[i] = Convert.ToInt32(adminMaps.Pins[i].Address.Split(':')[1].Trim());
                    totalPessangerCount += tempStationPessangerCountArray[i];
                }
                if (totalPessangerCount == 0)
                    _ = DisplayAlert("Hata", "Hiçbir yolcu talebi yok", "Tamam");
                else
                {
                    loadingOverlayEnable("Sefer düzenleniyor...");
                    // sefer düzenleme
                    await createATrip(infinityBusCheck.IsChecked);
                    loadingOverlayDisable();
                }

                
            };



            mapOverlayAdmin = new FlexLayout
            {
                IsEnabled = true,
                IsVisible = true,
                BackgroundColor = Color.WhiteSmoke,
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 20),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Center,
                JustifyContent = FlexJustify.Start,
                Children =
                {
                    adminMaps, // Harita
                    new Label {Text = "Sefer Düzenle", FontFamily = "ComfortaaBold", FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Color.FromRgb(77,77,77), BackgroundColor = Color.WhiteSmoke, HorizontalTextAlignment = TextAlignment.Center, WidthRequest = 400},
                    new FlexLayout // Admin Paneli
                    {
                        HeightRequest = 200,
                        HorizontalOptions = LayoutOptions.Fill,
                        IsEnabled = true,
                        IsVisible = true,
                        BackgroundColor = Color.WhiteSmoke,
                        Padding = new Thickness(0, 0, 0, 0),
                        Margin = new Thickness(5, 5, 5, 5),
                        Direction = FlexDirection.Row,
                        AlignItems = FlexAlignItems.Start,
                        JustifyContent = FlexJustify.Start,
                        Children =
                        {
                            new FlexLayout // Durak Ekleme
                            {
                                HeightRequest = 200,
                                WidthRequest = 150,
                                IsEnabled = true,
                                IsVisible = true,
                                BackgroundColor = Color.WhiteSmoke, 
                                Padding = new Thickness(0, 0, 0, 0),
                                Margin = new Thickness(5, 5, 5, 5),
                                Direction = FlexDirection.Column,
                                AlignItems = FlexAlignItems.Center,
                                JustifyContent = FlexJustify.Start,
                                Children =
                                {
                                    //new Label {Text = "Durak Ekle", FontFamily = "ComfortaaBold", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Color.FromRgb(77,77,77), BackgroundColor = Color.WhiteSmoke, HorizontalTextAlignment = TextAlignment.Center, WidthRequest = 150},
                                    stationName,
                                    stationLat,
                                    stationLong,
                                    stationAddButton,
                                    adminLogoutButton,
                                }
                            },
                            new FlexLayout // Sefer Düzenleme
                            {
                                HeightRequest = 200,
                                WidthRequest = 250,
                                IsEnabled = true,
                                IsVisible = true,
                                BackgroundColor = Color.WhiteSmoke,
                                Padding = new Thickness(0, 0, 0, 0),
                                Margin = new Thickness(5, 5, 5, 5),
                                Direction = FlexDirection.Column,
                                AlignItems = FlexAlignItems.Center,
                                JustifyContent = FlexJustify.Start,
                                Children =
                                {
                                    //new Label {Text = "Sefer Düzenle", FontFamily = "ComfortaaBold", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Color.FromRgb(77,77,77), BackgroundColor = Color.WhiteSmoke, HorizontalTextAlignment = TextAlignment.Center, WidthRequest = 250},
                                    new FlexLayout // Durak Seçme
                                    {
                                        HeightRequest = 50,
                                        WidthRequest = 250,
                                        IsEnabled = true,
                                        IsVisible = true,
                                        BackgroundColor = Color.WhiteSmoke,
                                        Padding = new Thickness(0, 0, 0, 0),
                                        Margin = new Thickness(5, 5, 5, 5),
                                        Direction = FlexDirection.Row,
                                        AlignItems = FlexAlignItems.Center,
                                        JustifyContent = FlexJustify.Start,
                                        Children =
                                        {
                                            new Label {Text = "Seçilen Durak", FontFamily = "ComfortaaBold", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromRgb(77,77,77), BackgroundColor = Color.WhiteSmoke, HorizontalTextAlignment = TextAlignment.Center, WidthRequest = 125},
                                            stationPicker,
                                        }
                                    },
                                    pessangers,
                                    new FlexLayout // Onay Butonları
                                    {
                                        HeightRequest = 50,
                                        WidthRequest = 250,
                                        IsEnabled = true,
                                        IsVisible = true,
                                        BackgroundColor = Color.WhiteSmoke,
                                        Padding = new Thickness(0, 0, 0, 0),
                                        Margin = new Thickness(5, 5, 5, 5),
                                        Direction = FlexDirection.Row,
                                        AlignItems = FlexAlignItems.Center,
                                        JustifyContent = FlexJustify.SpaceEvenly,
                                        Children =
                                        {
                                            passangersAddToStationButton,
                                            infinityBusCheck,
                                            tripConfirmButton
                                        }
                                    },
                                }
                            },
                        }
                    },
                }
            };

            // User Map Panel ----------------------------------------------------------------------------------------------
            userMaps = new Map { HorizontalOptions = LayoutOptions.FillAndExpand, WidthRequest = 400, HeightRequest = 400, IsTrafficEnabled = false };
            userLabel = new Label { Text = "Hoşgeldin", FontFamily = "ComfortaaBold", FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke, HorizontalTextAlignment = TextAlignment.Center, WidthRequest = 400 };
            userTripRequest = new Label { Text = "Sefer Durumu: ", FontFamily = "ComfortaaBold", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke, HorizontalTextAlignment = TextAlignment.Center, WidthRequest = 400 };
            Button userLogoutButton = new Button { Text = "Çıkış Yap", FontFamily = "ComfortaaBold", FontSize = 12, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke, WidthRequest = 125 };
            Button userTripRequestButton = new Button { Text = "Bilet Talep Et", FontFamily = "ComfortaaBold", FontSize = 12, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke, WidthRequest = 125 };
            Button userUpdateButton = new Button { Text = "Bilgileri Güncelle", FontFamily = "ComfortaaBold", FontSize = 12, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke, WidthRequest = 125 };
            userStationPicker = new Picker { WidthRequest = 125, HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, FontFamily = "ComfortaaBold", FontSize = 12, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke, SelectedIndex = 0 };
            tripHistoryPicker = new Picker { WidthRequest = 125, HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, FontFamily = "ComfortaaBold", FontSize = 12, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke, SelectedIndex = 0 };
            userMaps.MoveToRegion(MapSpan.FromCenterAndRadius(izmit, Xamarin.Forms.GoogleMaps.Distance.FromKilometers(7)));

            mapOverlayUser = new FlexLayout
            {
                IsEnabled = true,
                IsVisible = true,
                BackgroundColor = Color.WhiteSmoke,
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 20),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.Center,
                JustifyContent = FlexJustify.Start,
                Children =
                {
                    userMaps, // Harita
                    userLabel,
                    userStationPicker,
                    userTripRequest,
                    //new Label { Text = "Geçmiş", FontFamily = "ComfortaaBold", FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromRgb(77, 77, 77), BackgroundColor = Color.WhiteSmoke, HorizontalTextAlignment = TextAlignment.Center, WidthRequest = 400 },
                    //tripHistoryPicker,
                    userUpdateButton,
                    userTripRequestButton,
                    userLogoutButton,
                }
            };

            userUpdateButton.Clicked += async (object sender, EventArgs args) =>
            {
                await getRequests(false, userId);
            };

            userTripRequestButton.Clicked += async (object sender, EventArgs args) =>
            {
                if(userTripRequest.TextColor == Color.Blue)
                {
                    _ = DisplayAlert("Hata", "Şuanda bekleyen isteğiniz var.", "Tamam");
                }
                else
                {
                    // şeçilen durak için bilet talebi atma
                    string selectedStationId = userMaps.SelectedPin.Tag.ToString();
                    string selectedUsersIds = userId.ToString();
                    await createRequest(selectedStationId, selectedUsersIds);
                }
            };

            userLogoutButton.Clicked += (object sender, EventArgs args) =>
            {
                loadingOverlayEnable("Çıkış Yapılıyor...");
                userId = 0;
                userName = "";
                //userStationPicker.SelectedIndex = 0;
                overlaySwap(loginOverlay, mapOverlayUser);
                loadingOverlayDisable();
            };

            userStationPicker.SelectedIndexChanged += (object sender, EventArgs args) =>
            {
                // Seçilen pini haritada ortala
                int selectedPinIndex = ((Picker)sender).SelectedIndex;
                //Console.WriteLine(selectedPinIndex.ToString() + " asdasdsadsadsa");
                Position selectedPinPosition = new Position(Convert.ToDouble(stationsInfos[selectedPinIndex].Split(',')[2]), Convert.ToDouble(stationsInfos[selectedPinIndex].Split(',')[3]));
                if (adminId != 0)
                {
                    adminMaps.MoveToRegion(MapSpan.FromCenterAndRadius(selectedPinPosition, Xamarin.Forms.GoogleMaps.Distance.FromMeters(300)));
                    adminMaps.SelectedPin = adminMaps.Pins[selectedPinIndex];
                }
                else if (userId != 0)
                {
                    userMaps.MoveToRegion(MapSpan.FromCenterAndRadius(selectedPinPosition, Xamarin.Forms.GoogleMaps.Distance.FromMeters(300)));
                    userMaps.SelectedPin = userMaps.Pins[selectedPinIndex];
                }
                //Console.WriteLine("Geçti");
            };


            // -------------------------------------------------------------------------------------------------------------


            // -------------------------------------------------------------------------------------------------------------



            Content = new AbsoluteLayout
            {
                BackgroundColor = Color.WhiteSmoke,
                Padding = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                   mapOverlayAdmin,
                   mapOverlayUser,
                   loginOverlay,
                   singInOverlay,
                   
                    // loading Overlay en altta olmalı
                   loadingOverlay,
                }
                
            };
        }

    }
}

