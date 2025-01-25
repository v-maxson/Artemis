using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;

public class Config
{
    private const string CONFIG_DIRECTORY = "./config";
    private const string CONFIG_PATH = $"{CONFIG_DIRECTORY}/config.json";

    [JsonProperty("token")]
    public string Token { get; set; } = "STRING HERE";
    [JsonProperty("guild")]
    public ulong Guild { get; set; } = 0;

    public class ClientPresence
    {
        [JsonProperty("text")]
        public string Text { get; set; } = "";
        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public NetCord.UserStatusType Status { get; set; } = NetCord.UserStatusType.Online;
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public NetCord.Gateway.UserActivityType Type { get; set; } = NetCord.Gateway.UserActivityType.Playing;

        public ClientPresence() { }
        public ClientPresence(string text, NetCord.Gateway.UserActivityType type = NetCord.Gateway.UserActivityType.Playing, NetCord.UserStatusType status = NetCord.UserStatusType.Online) {
            Text = text;
            Type = type;
            Status = status;
        }
    }

    [JsonProperty("client_presences")]
    public List<ClientPresence> ClientPresences { get; set; } = [];

    public static Config Load() {


        if (!Directory.Exists(CONFIG_DIRECTORY)) {
            Log.Warning("Config directory does not exist, creating...");

            try {
                Directory.CreateDirectory(CONFIG_DIRECTORY);
            }
            catch (Exception e) {
                Log.Fatal("Failed to create config directory, exiting...", e);
                Environment.Exit(1);
            }
        }

        if (!File.Exists(CONFIG_PATH)) {
            Log.Warning("Config file does not exist, creating...");
            try {
                var config = new Config();
                File.WriteAllText(CONFIG_PATH, JsonConvert.SerializeObject(config, Formatting.Indented));
                Log.Information("Config file created, please fill in the required fields and restart the bot.");
                Environment.Exit(0);
                return null!;
            }
            catch (Exception e) {
                Log.Fatal("Failed to create config file, exiting...", e);
                Environment.Exit(1);
                return null!;
            }
        }

        try {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(CONFIG_PATH));
            File.WriteAllText(CONFIG_PATH, JsonConvert.SerializeObject(config, Formatting.Indented));

            return config!;
        }
        catch (Exception e) {
            Log.Fatal("Failed to load config file, exiting...", e);
            Environment.Exit(1);
            return null!;
        }
    }
}
