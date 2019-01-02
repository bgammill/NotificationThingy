using System;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;

namespace NotificationThingy
{
    [Service]
    class MqttService : Service
    {
        IMqttClient client;

        public override void OnCreate()
        {
            var manifestUrl = GetString(Resource.String.manifest_url);
            var manifestClient = new WebClient();
            var manifestRaw = manifestClient.DownloadString(manifestUrl);

            var definition = new { host = "", port = 0 };
            var manifestJson = JsonConvert.DeserializeAnonymousType(manifestRaw, definition);

            var address = manifestJson.host;
            var port = manifestJson.port;

            var factory = new MqttFactory();
            client = factory.CreateMqttClient();
            client.ApplicationMessageReceived += Client_ApplicationMessageReceived;
            client.Connected += Client_Connected;
            client.Disconnected += Client_Disconnected;

            var clientId = Guid.NewGuid().ToString();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(address, port)
                .WithClientId(clientId)
                .Build();

            client.ConnectAsync(options);
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            client.DisconnectAsync();
        }

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        private void Client_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            using (var h = new Handler(Looper.MainLooper))
            {
                h.Post(() =>
                {
                    MainActivity.textView.Text = "disconnected";
                });
            }
        }

        private async void Client_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            var topic = GetString(Resource.String.topic);
            await client.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());

            using (var h = new Handler(Looper.MainLooper))
            {
                h.Post(() =>
                {
                    MainActivity.textView.Text = "connected";
                });
            }
        }

        private void Client_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            using (var h = new Handler(Looper.MainLooper))
            {
                h.Post(() =>
                {
                    var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    SendNotification(message);
                });
            }
        }

        void SendNotification(string message)
        {
            var appName = GetString(Resource.String.app_name);

            var id = GetString(Resource.String.channel_id);

            var builder = new Notification.Builder(Application.Context, id)
                .SetSmallIcon(Resource.Mipmap.Icon)
                .SetContentTitle(appName)
                .SetContentText(message);

            var notification = builder.Build();

            var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;

            var rnd = new Random();
            var notificationId = rnd.Next(int.MinValue, int.MaxValue);
            notificationManager.Notify(notificationId, notification);
        }
    }
}