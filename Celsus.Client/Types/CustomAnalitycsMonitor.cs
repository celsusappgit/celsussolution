using GoogleAnalytics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Telerik.Windows.Controls;
using System.Deployment.Application;
using Celsus.Client.Shared.Types;

namespace Celsus.Client.Types
{
    internal class PlatformInfoProvider : IPlatformInfoProvider
    {
        public string AnonymousClientId { get; set; }

        public int? ScreenColors { get; set; }

        public Dimensions ScreenResolution { get; set; }
        public string UserAgent { get; set; }
        public string UserLanguage { get; set; }
        public Dimensions ViewPortResolution { get; set; }

        Dimensions? IPlatformInfoProvider.ScreenResolution { get; }

        Dimensions? IPlatformInfoProvider.ViewPortResolution { get; }

        public event EventHandler ScreenResolutionChanged;
        public event EventHandler ViewPortResolutionChanged;

        public void OnTracking()
        {
            throw new NotImplementedException();
        }
    }
    public class CustomAnalitycsMonitor
    {
        private Tracker tracker;

        public string AppVersion { get; private set; }
        public string ServerId { get; private set; }

        public CustomAnalitycsMonitor()
        {
            this.CreateGoogleTracker();
        }

        public void CreateGoogleTracker()
        {
            var trackerManager = new TrackerManager(new PlatformInfoProvider()
            {
                AnonymousClientId = Guid.NewGuid().ToString(),
                ScreenResolution = new Dimensions(1920, 1080),
                UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko",
                UserLanguage = System.Globalization.CultureInfo.CurrentCulture.Name,
                ViewPortResolution = new Dimensions(1920, 1080)

            });
            tracker = trackerManager.CreateTracker("UA-129865696-1"); // your GoogleAnalytics property ID goes here 
            tracker.AppName = "Celsus";

            try
            {
                AppVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch (InvalidDeploymentException)
            {
                AppVersion = "Not Installed";
            }

            ServerId = ComputerHelper.Instance.ServerId;
        }

        public void TrackScreenView(string screenName)
        {
            var data = HitBuilder.CreateScreenView(screenName).Build();

            data.Add("av", AppVersion);
            data.Add("uid", ServerId);
            tracker.Send(data);
        }

        public void StartSession()
        {
            //var data = HitBuilder.CreateCustomEvent("cat", "act").Build();
            //data.Add("sc", "start");
            //data.Add("av", AppVersion);
            //data.Add("uid", ServerId);
            //tracker.Send(data);
            List<string> labels = new List<string>();
            labels.Add(ServerId);
            labels.Add($"License:{LicenseHelper.Instance.Status.ToString()}");
            if (LicenseHelper.Instance.TrialLicenseInfo != null && LicenseHelper.Instance.TrialLicenseInfo.EMail != null)
                labels.Add($"EMail:{LicenseHelper.Instance.TrialLicenseInfo.EMail}");
            TrackEvent("Session", "Start", string.Join("|", labels.ToArray()), 1);
        }

        public void EndSession()
        {
            //var data = HitBuilder.CreateCustomEvent("cat", "act").Build();
            //data.Add("sc", "end");
            //data.Add("av", AppVersion);
            //data.Add("uid", ServerId);
            //tracker.Send(data);
            List<string> labels = new List<string>();
            labels.Add(ServerId);
            labels.Add($"License:{LicenseHelper.Instance.Status.ToString()}");
            if (LicenseHelper.Instance.TrialLicenseInfo != null && LicenseHelper.Instance.TrialLicenseInfo.EMail != null)
                labels.Add($"EMail:{LicenseHelper.Instance.TrialLicenseInfo.EMail}");
            TrackEvent("Session", "End", string.Join("|", labels.ToArray()), 1);
        }
        public void TrackAtomicFeature(string feature)
        {
            // The value of the "feature" string consists of the whole name of the tracked feature, 
            // for example : "MyGridView.Sorted.Name.Ascending", if we have performed a sorting operation in RadGridView. 
            // So, we can split this string in order to pass friendlier names to the parameters of the CreateCustomEvent method which will be used in your reports. 
            string category;
            string eventAction;
            this.SplitFeatureName(feature, out category, out eventAction);

            var data = HitBuilder.CreateCustomEvent(category, eventAction + " event", feature.ToString(), 1).Build();
            data.Add("av", AppVersion);
            data.Add("uid", ServerId);
            tracker.Send(data);
        }

        public void TrackEvent(string category, string action, string label = null, long value = 0)
        {
            var data = HitBuilder.CreateCustomEvent(category, action, label, value).Build();
            data.Add("av", AppVersion);
            data.Add("uid", ServerId);
            tracker.Send(data);
        }

        public void TrackError(string feature, Exception exception)
        {
            var data = HitBuilder.CreateException(feature + ":" + exception.ToString(), true).Build();
            data.Add("av", AppVersion);
            data.Add("uid", ServerId);
            tracker.Send(data);
        }

        public void TrackFeatureCancel(string feature)
        {
            string category;
            string eventAction;
            this.SplitFeatureName(feature, out category, out eventAction);

            var data = HitBuilder.CreateCustomEvent(category, eventAction + " event.Cancelled", feature.ToString(), 1).Build();
            data.Add("av", AppVersion);
            data.Add("uid", ServerId);
            tracker.Send(data);
        }

        public void TrackFeatureStart(string feature)
        {
            // Measuring timings provides a native way to measure a period of time in Google Analytics.  
            // This can be useful to measure resource load times, for example. 
            TimeSpan ts = TimeSpan.FromSeconds(2.2);
            var data = HitBuilder.CreateTiming("Loaded", "MainWindow", ts).Build();
            data.Add("av", AppVersion);
            data.Add("uid", ServerId);
            tracker.Send(data);
        }

        public void TrackFeatureEnd(string feature)
        {
            TimeSpan ts = TimeSpan.FromSeconds(2.2);
            var data = HitBuilder.CreateTiming("Loaded", "MainWindow", ts).Build();
            data.Add("av", AppVersion);
            data.Add("uid", ServerId);
            tracker.Send(data);
        }

        public void TrackValue(string feature, long value)
        {
            string category;
            string eventAction;
            this.SplitFeatureName(feature, out category, out eventAction);

            var data = HitBuilder.CreateCustomEvent(category, eventAction + " event", feature.ToString(), value).Build();
            data.Add("av", AppVersion);
            data.Add("uid", ServerId);
            tracker.Send(data);
        }

        private void SplitFeatureName(string feature, out string category, out string eventAction)
        {
            string[] parameters = feature.Split('.');
            category = parameters[0];
            eventAction = parameters[1];
        }
    }


}
