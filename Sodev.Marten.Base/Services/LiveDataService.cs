using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Base.Connection;
using Sodev.Marten.Base.Events;
using Sodev.Marten.Base.Model;

namespace Sodev.Marten.Base.Services
{
    public class LiveDataService : ILiveDataService
    {
        private readonly IConnectionService connectionService;
        private readonly IPidRepository pidRepository;
        private readonly IDomainEventAggregator domainEventAggregator;

        private readonly List<LiveMonitor> registeredLiveMonitors = new List<LiveMonitor>();
        private readonly List<LiveMonitor> availableLiveMonitors = new List<LiveMonitor>();
        private DateTime? queryingStartTimeStamp;


        public LiveDataService(IConnectionService connectionService, IPidRepository pidRepository, IDomainEventAggregator domainEventAggregator)
        {
            this.connectionService = connectionService;
            this.pidRepository = pidRepository;
            this.domainEventAggregator = domainEventAggregator;

            RefreshLiveMonitorsList();
        }

        private void RefreshLiveMonitorsList()
        {
            if(registeredLiveMonitors.Count > 0) availableLiveMonitors.Clear();
            var pidsParamsList = pidRepository.GetAllPidsParameters();
            var newLiveMonitors = pidsParamsList.Select(x => new LiveMonitor(Pid.Create(x)));
            availableLiveMonitors.AddRange(newLiveMonitors);
        }

        public IEnumerable<LiveMonitor> GetAvailableLiveMonitors() => availableLiveMonitors;

        public void RegisterLiveMonitor(LiveMonitor liveMonitor)
        {
            registeredLiveMonitors.Add(liveMonitor);

            if (registeredLiveMonitors.Count == 1)
            {
                SubscribeLiveDataUpdatedEvent();
                StartQuerying();
            }
                
        }

        public void UnregisterLiveMonitor(LiveMonitor liveMonitor)
        {
            registeredLiveMonitors.Remove(liveMonitor);

            if (registeredLiveMonitors.Count == 0)
            {
                StopQuerying();
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

        private void OnLiveDataUpdated(object sender, ObdAnswer answer)
        {
            //TODO DO STUFF HERE!!! this method should be responsible for redistributing answers among appropriate handlers!

            //var byteArray = answer.AnswerText.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray<byte>();

            UpdateLiveMonitorData(answer.PidId, answer.Data, answer.TimeStamp);
        }

        private bool continueQuerying = false;
        private void StartQuerying()
        {
            continueQuerying = true;
            queryingStartTimeStamp = DateTime.Now;
            domainEventAggregator.PublishDomainEvent(new StartQueryingEvent(queryingStartTimeStamp.Value));
            Task.Factory.StartNew(async () =>
            {
                while (continueQuerying)
                {
                    QueryForLiveData();
                    await Task.Delay(500);
                }
            });
        }

        private void StopQuerying()
        {
            continueQuerying = false;
            queryingStartTimeStamp = null;
        }

        private void QueryForLiveData()
        {
            var query = new ObdQuery();
            foreach (var liveMonitor in registeredLiveMonitors)
            {
                query.QueryText = $"01{liveMonitor.Id:X2}"; //TODO fixed 01 !:(
                connectionService.SendQuery(query);
                Debug.WriteLine("Query sent...");
            }
        }

        private void UpdateLiveMonitorData(int pidId, byte[] data, DateTime timeStamp)
        {
            var liveMonitor = registeredLiveMonitors.FirstOrDefault(m => m.Id == pidId);
            var timeSpan = (timeStamp - queryingStartTimeStamp);
            liveMonitor?.UpdateData(data, timeSpan.Value);
        }
    }

    public interface ILiveDataService
    {
        IEnumerable<LiveMonitor> GetAvailableLiveMonitors();

        void RegisterLiveMonitor(LiveMonitor liveMonitor);

        void UnregisterLiveMonitor(LiveMonitor liveMonitor);
    }
}
