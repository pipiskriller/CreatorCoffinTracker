using System.Net;
using Imazen.WebP;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace CreatorCoffinTracker
{
    public class Utils
    {
        public const string MainURL = "https://ingame-webviews.supercell.com";
        public const string CreatorJoinCountURL = "https://ingame-webviews.supercell.com/brawlstars/data/cauldron-muse-joins";
        public const string CreatorProgressDataURL = "https://ingame-webviews.supercell.com/brawlstars/cauldron-muse/creators";
        public const string TrackerDataURL = "https://ingame-webviews.supercell.com/brawlstars/data/tracker-";
        public static async Task<CreatorData> FetchData(string CreatorCode)
        {
            CreatorData Data = new CreatorData();
            using (HttpClient Client = new HttpClient())
            {
                var PingRequest = await Client.GetAsync(CreatorJoinCountURL + $"/{CreatorCode}.json");

                Data.StatusCode = (int)PingRequest.StatusCode;

                if (PingRequest.IsSuccessStatusCode)
                {
                    string JoinCountData = await PingRequest.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(JoinCountData))
                    {
                        var JSONDoc_JoinCountData = JsonDocument.Parse(JoinCountData);
                        JSONDoc_JoinCountData.RootElement.GetProperty("count").TryGetDouble(out double PlayersCount);
                        Data.Players = PlayersCount;
                    }

                    var ProgressRequest = await
                        Client.GetAsync(CreatorProgressDataURL + $"/{CreatorCode}.json");

                    var JSONDoc_ProgressData = JsonDocument.Parse(await ProgressRequest.Content.ReadAsStringAsync());

                    JSONDoc_ProgressData.RootElement.GetProperty("day").TryGetDouble(out double Day);

                    string? Code = JSONDoc_ProgressData.RootElement.GetProperty("code").GetString();
                    string? DisplayName = JSONDoc_ProgressData.RootElement.GetProperty("displayName").GetString();
                    string? Avatar = JSONDoc_ProgressData.RootElement.GetProperty("avatar").GetString();

                    string? Channel_Platform = JSONDoc_ProgressData.RootElement.GetProperty("channel").GetProperty("platform").GetString();
                    string? Channel_Handle = JSONDoc_ProgressData.RootElement.GetProperty("channel").GetProperty("handle").GetString();
                    string? Channel_URL = JSONDoc_ProgressData.RootElement.GetProperty("channel").GetProperty("url").GetString();

                    var AvatarData = await Client.GetByteArrayAsync(MainURL + Avatar);

                    var Channel = new Channel()
                    {
                        Platform = Channel_Platform,
                        Handle = Channel_Handle,
                        URL = Channel_URL
                    };

                    Data.Day = Day;
                    Data.Code = Code;
                    Data.DisplayName = DisplayName;
                    Data.Avatar = Avatar;
                    Data.Channel = Channel;

                    Data.AvatarData = AvatarData;

                    int DaysListLength = JSONDoc_ProgressData.RootElement.GetProperty("days").GetPropertyCount();

                    for (int Idx = 0; Idx < DaysListLength; Idx++)
                    {
                        double DayNameNum = (double)Idx + 1;
                        var DayData = new Days
                        {
                            DayName = DayNameNum.ToString(),
                            TrackerID = JSONDoc_ProgressData.RootElement.GetProperty("days").GetProperty(DayNameNum.ToString()).GetProperty("trackerId").GetString()
                        };

                        var ProgressValueRequest = await
                        Client.GetAsync(TrackerDataURL + $"{DayData.TrackerID}.json");

                        var JSONDoc_ProgressValueData = JsonDocument.Parse(await ProgressValueRequest.Content.ReadAsStringAsync());

                        JSONDoc_ProgressValueData.RootElement.GetProperty("value").TryGetDouble(out double Val);

                        DayData.Value = Val;

                        int MilestonesListLength = JSONDoc_ProgressData.RootElement.GetProperty("days").GetProperty(DayNameNum.ToString()).GetProperty("milestones").GetArrayLength();

                        for (int Jdx = 0; Jdx < MilestonesListLength; Jdx++)
                        {
                            JSONDoc_ProgressData.RootElement.GetProperty("days").GetProperty(DayNameNum.ToString()).GetProperty("milestones")[Jdx].TryGetDouble(out double Ms);
                            DayData.Milestones.Add(Ms);
                        }

                        Data.Days.Add(DayData);
                    }
                }

                return Data;
            }
        }
        public static BitmapSource DecodeWebP(byte[] WebPImgBytes)
        {
            byte[] Pixels = WebPDecoder.Decode(WebPImgBytes, out int Width, out int Height, WebPPixelFormat.Bgra);

            var Source = BitmapSource.Create
                (Width, Height, 96, 96, PixelFormats.Bgra32, null, Pixels, Width * 4);

            Source.Freeze();
            return Source;
        }
    }
}
