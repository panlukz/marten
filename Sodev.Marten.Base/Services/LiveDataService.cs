using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Base.Connection;
using Sodev.Marten.Base.Model;

namespace Sodev.Marten.Base.Services
{
    public class LiveDataService : ILiveDataService
    {
        private readonly IConnectionService connectionService;

        private readonly IList<LiveMonitor> liveMonitors = new List<LiveMonitor>();

        public LiveDataService(IConnectionService connectionService)
        {
            this.connectionService = connectionService;
        }

        public IEnumerable<LiveMonitor> GetAvailableLiveMonitors()
        {
            //TODO implement it properly!!
            var pidParams = new PidParameters()
            {
                ReturnedBytesNb = 2,
                Id = 0x0C,
                Formula = "((A * 256) + B) / 4",
                Unit = "rpm",
                Description = "RPM"
            };
            var pid = Pid.Create(pidParams);

            return new List<LiveMonitor>
            {
                new LiveMonitor(pid)
            };
        }

        public void RegisterLiveMonitor(LiveMonitor liveMonitor)
        {
            liveMonitors.Add(liveMonitor);

            if (liveMonitors.Count == 1)
            {
                SubscribeLiveDataUpdatedEvent();
                StartQuering();
            }
                
        }

        public void UnregisterLiveMonitor(LiveMonitor liveMonitor)
        {
            liveMonitors.Remove(liveMonitor);

            if (liveMonitors.Count == 0)
            {
                StopQuering();
                UnsubscribeLiveDataUpdatedEvent();
            }
                
        }

        private void SubscribeLiveDataUpdatedEvent()
        {
            connectionService.AnswerReceivedEvent -= OnLiveDataUpdated; //to ensure that it won't be hooked up twice
            connectionService.AnswerReceivedEvent += OnLiveDataUpdated;
        }

        private void UnsubscribeLiveDataUpdatedEvent()
        {
            connectionService.AnswerReceivedEvent -= OnLiveDataUpdated;
        }

        private void OnLiveDataUpdated(object sender, Answer answer)
        {
            //TODO DO STUFF HERE!!!
            if (!answer.AnswerText.StartsWith("41")) return; //in case if it's not a response for a PID request

            var byteArray = answer.AnswerText.Substring(0, 11).Split(' ').Select(x => Convert.ToByte($"0x{x}", 16)).ToArray<byte>();

            UpdateLiveMonitorData(byteArray[1], byteArray.Skip(2).Select(x => x).ToArray());


            //var byteArray = answer.AnswerText.Split(' ').Skip(2).Select(x => Convert.ToByte($"0x{x}", 16)).ToArray<byte>();


            //var decipheredValue = Math.Round(pid.GetValue(byteArray));

            //System.Console.WriteLine($"RPM: {decipheredValue}");
        }

        private bool continueQuerying = false;

        private void StartQuering()
        {
            continueQuerying = true;
            Task.Factory.StartNew(async () =>
            {
                while (continueQuerying)
                {
                    QueryForLiveData();
                    await Task.Delay(200);
                }
            });
        }

        private void StopQuering()
        {
            continueQuerying = false;
        }

        private void QueryForLiveData()
        {
            var query = new Query();
            foreach (var liveMonitor in liveMonitors)
            {
                query.QueryText = $"01{liveMonitor.Id:X2}"; //TODO fixed 01 !:(
                connectionService.SendQuery(query);
                Debug.WriteLine("Query sent...");
            }
        }

        private void UpdateLiveMonitorData(int pidId, byte[] data)
        {
            var liveMonitor = liveMonitors.FirstOrDefault(m => m.Id == pidId);
            liveMonitor?.UpdateData(data);
        }
    }

    public interface ILiveDataService
    {
        IEnumerable<LiveMonitor> GetAvailableLiveMonitors();

        void RegisterLiveMonitor(LiveMonitor liveMonitor);

        void UnregisterLiveMonitor(LiveMonitor liveMonitor);
    }
}
