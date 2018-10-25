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

        private int[] EvaluateAvailablePidsForConnectedEcu()
        {
            //TODO it has to be implemented :(

            //TODO temporary, it has to be requested in a task probably...
            if (connectionService.GetState() != ConnectionState.Opened) return new int[0];
            var allAvailablePidsQuery = new ObdQuery(01, 00);
            connectionService.SendQuery(allAvailablePidsQuery);


            return new int[0];
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
                StartLiveDataQuerying();
            }
                
        }

        public void UnregisterLiveMonitor(LiveMonitor liveMonitor)
        {
            registeredLiveMonitors.Remove(liveMonitor);

            if (registeredLiveMonitors.Count == 0)
            {
                StopLiveDataQuerying();
                UnsubscribeLiveDataUpdatedEvent();
            }
                
        }

        private void SubscribeLiveDataUpdatedEvent()
        {
            connectionService.AnswerReceivedEvent -= OnLiveDataUpdated; //to ensure it won't be hooked up twice
            connectionService.AnswerReceivedEvent += OnLiveDataUpdated;
        }

        private void UnsubscribeLiveDataUpdatedEvent()
        {
            connectionService.AnswerReceivedEvent -= OnLiveDataUpdated;
        }

        private void OnLiveDataUpdated(object sender, ObdAnswer answer)
        {
            //TODO DO STUFF HERE!!! this method should be responsible for redistributing answers among appropriate handlers!
            UpdateLiveMonitorData(answer.PidId, answer.Data, answer.TimeStamp);
        }

        private bool continueQuerying = false;
        private void StartLiveDataQuerying()
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

        private void StopLiveDataQuerying()
        {
            continueQuerying = false;
            queryingStartTimeStamp = null;
        }

        private void QueryForLiveData()
        {
            foreach (var liveMonitor in registeredLiveMonitors)
            {
                var query = new ObdQuery(1, liveMonitor.Id);
                connectionService.SendQuery(query);
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
