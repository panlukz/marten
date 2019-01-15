using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Sodev.Marten.Domain.Events;
using Sodev.Marten.Domain.Services;

namespace Sodev.Marten.Presentation.Features.FaultCodes
{
    public class FaultCodesViewModel : Screen, IHandle<FaultCodeEvent>
    {
        private readonly IFaultCodesService faultCodesService;

        public void Handle(FaultCodeEvent message)
        {
            NotifyOfPropertyChange(nameof(FaultCodesNumber));
            NotifyOfPropertyChange(nameof(FaultCodesList));
        }

        protected override void OnActivate()
        {
            faultCodesService.SubscribeAnswerReceivedEvent();
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            faultCodesService.UnsubscribeAnswerReceivedEvent();
            base.OnDeactivate(close);
        }

        public FaultCodesViewModel(IFaultCodesService faultCodesService, IEventAggregator eventAggregator)
        {
            this.faultCodesService = faultCodesService;
            eventAggregator.Subscribe(this);
        }

        public void RequestFaultCodes()
        {
            faultCodesService.RequestFaultCodes();
        }

        public void ClearFaultCodes()
        {
            faultCodesService.RequestClearingFaultCodes();
        }

        public int FaultCodesNumber => faultCodesService.FaultCodesNumber;


        public BindableCollection<string> FaultCodesList => new BindableCollection<string>(faultCodesService.FaultCodesList.Select(x => x.Number).ToList());
    }
}
