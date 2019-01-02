using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace NotificationThingy
{
    [Activity(Label = "Notification Thingy", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        public static TextView textView;

        void CreateNotificationChannel()
        {
            var name = Resources.GetString(Resource.String.channel_name);
            var description = GetString(Resource.String.channel_description);
            var id = GetString(Resource.String.channel_id);
            var channel = new NotificationChannel(id, name, NotificationImportance.Default)
            {
                Name = name,
                Description = description,
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);
            CreateNotificationChannel();

            textView = FindViewById<TextView>(Resource.Id.textView1);

            var startButton = FindViewById<Button>(Resource.Id.startButton);
            startButton.Click += delegate
            {
                textView.Text = "connecting";

                var intent = new Intent(this, typeof(MqttService));
                StartService(intent);
            };

            var stopButton = FindViewById<Button>(Resource.Id.stopButton);
            stopButton.Click += delegate
            {
                var intent = new Intent(this, typeof(MqttService));
                StopService(intent);
            };
        }
    }
}

