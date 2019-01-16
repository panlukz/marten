using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Base.Connection;
using Sodev.Marten.Base.Model;
using Sodev.Marten.Base.ObdCommunication;
using Sodev.Marten.Base.Services;
using Sodev.Marten.Domain.Events;
using Sodev.Marten.Domain.Model;

namespace Sodev.Marten.Domain.Services
{
    public class LiveDataService : ILiveDataService
    {
        private readonly IObdCommuncation obdCommuncation;
        private readonly IPidRepository pidRepository;
        private readonly IObdEventBus obdEventBus;

        private readonly List<ILiveMonitor> registeredLiveMonitors = new List<ILiveMonitor>();
        private readonly List<ILiveMonitor> availableLiveMonitors = new List<ILiveMonitor>();
        private DateTime? queryingStartTimeStamp;


        public LiveDataService(IObdCommuncation obdCommuncation, IPidRepository pidRepository, IObdEventBus obdEventBus)
        {
            this.obdCommuncation = obdCommuncation;
            this.pidRepository = pidRepository;
            this.obdEventBus = obdEventBus;

            RefreshLiveMonitorsList();
        }

        private int[] EvaluateAvailablePidsForConnectedEcu()
        {
            //TODO it has to be implemented :(

            //TODO temporary, it has to be requested in a task probably...
            if (obdCommuncation.ConnectionState != ConnectionState.Opened) return new int[0];
            var allAvailablePidsQuery = new ObdQuery(01, 00);
            obdCommuncation.SendQuery(allAvailablePidsQuery);


            return new int[0];
        }

        private void RefreshLiveMonitorsList()
        {
            if(registeredLiveMonitors.Count > 0) availableLiveMonitors.Clear();
            var pidsParamsList = pidRepository.GetAllPidsParameters();
            var newLiveMonitors = pidsParamsList.Select(x => new LiveMonitor(Pid.Create(x)));
            availableLiveMonitors.AddRange(newLiveMonitors);
        }

        public IEnumerable<ILiveMonitor> GetAvailableLiveMonitors() => availableLiveMonitors;

        public void RegisterLiveMonitor(ILiveMonitor liveMonitor)
        {
            registeredLiveMonitors.Add(liveMonitor);

            if (registeredLiveMonitors.Count == 1)
            {
                SubscribeLiveDataUpdatedEvent();
                StartLiveDataQuerying();
                Debug.WriteLine("DE3 ___ Live data querying has started... ");
            }

        }

        public void UnregisterLiveMonitor(ILiveMonitor liveMonitor)
        {
            registeredLiveMonitors.Remove(liveMonitor);

            if (registeredLiveMonitors.Count == 0)
            {
                StopLiveDataQuerying();
                UnsubscribeLiveDataUpdatedEvent();
                Debug.WriteLine("DE3 ___ Live data querying has stopped... ");
            }
                
        }

        private void SubscribeLiveDataUpdatedEvent()
        {
            obdCommuncation.PidAnswerReceivedEvent -= OnLiveDataUpdated; //to ensure it won't be hooked up twice
            obdCommuncation.PidAnswerReceivedEvent += OnLiveDataUpdated;
        }

        private void UnsubscribeLiveDataUpdatedEvent()
        {
            obdCommuncation.PidAnswerReceivedEvent -= OnLiveDataUpdated;
        }

        private void OnLiveDataUpdated(object sender, PidAnswer answer)
        {
            //TODO DO STUFF HERE!!! this method should be responsible for redistributing answers among appropriate handlers!
            UpdateLiveMonitorData(answer.PidId, answer.Data, answer.TimeStamp);
        }

        private bool continueQuerying = false;
        private void StartLiveDataQuerying()
        {
            continueQuerying = true;
            queryingStartTimeStamp = DateTime.Now;
            obdEventBus.PublishEvent(new StartQueryingEvent(queryingStartTimeStamp.Value));
            Task.Factory.StartNew(async () =>
            {
                while (continueQuerying)
                {
                    try
                    {
                        await QueryForLiveData();

                    }
                    catch(Exception e)
                    { }

                }
            });
        }

        private void StopLiveDataQuerying()
        {
            continueQuerying = false;
            queryingStartTimeStamp = null;
        }

        private async Task QueryForLiveData()
        {
            foreach (var liveMonitor in registeredLiveMonitors.ToList())
            {
                    var query = new ObdQuery(1, liveMonitor.Id);
                obdCommuncation.SendQuery(query);

                    await Task.Delay(200 / registeredLiveMonitors.Count);
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
        IEnumerable<ILiveMonitor> GetAvailableLiveMonitors();

        void RegisterLiveMonitor(ILiveMonitor liveMonitor);

        void UnregisterLiveMonitor(ILiveMonitor liveMonitor);
    }
}
