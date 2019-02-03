using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Sodev.Marten.Base.Model;
using Sodev.Marten.Domain.Events;
using Sodev.Marten.Domain.Services;
using Sodev.Marten.Presentation.Interfaces;

namespace Sodev.Marten.Presentation.Features.FaultCodes
{
    public class FaultCodesViewModel : Screen, IHandle<FaultCodeEvent>
    {
        private readonly IFaultCodesService faultCodesService;
        private readonly IDialogService dialogService;

        public void Handle(FaultCodeEvent message)
        {
            if (message.EventType == FaultCodeEventType.FaultCodesNumberUpdated && FaultCodesNumber == 0)
                dialogService.DisplayInfo("No fault codes found", "Fault codes");

            NotifyOfPropertyChange(nameof(FaultCodesNumber));
            NotifyOfPropertyChange(nameof(FaultCodesList));
            NotifyOfPropertyChange(nameof(CanClearFaultCodes));

            faultCodesService.UnsubscribeAnswerReceivedEvent();
        }

        protected override void OnActivate()
        {
            //faultCodesService.SubscribeAnswerReceivedEvent();
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            //faultCodesService.UnsubscribeAnswerReceivedEvent();
            base.OnDeactivate(close);
        }

        public FaultCodesViewModel(IFaultCodesService faultCodesService, IEventAggregator eventAggregator, IDialogService dialogService)
        {
            this.faultCodesService = faultCodesService;
            this.dialogService = dialogService;
            eventAggregator.Subscribe(this);
        }

        public void RequestFaultCodes()
        {
            faultCodesService.RequestFaultCodes();
            faultCodesService.SubscribeAnswerReceivedEvent();
        }

        public void ClearFaultCodes()
        {
            faultCodesService.RequestClearingFaultCodes();
            faultCodesService.SubscribeAnswerReceivedEvent();
        }

        public bool CanClearFaultCodes => FaultCodesNumber > 0;

        public int FaultCodesNumber => faultCodesService.FaultCodesNumber;


        public BindableCollection<Dtc> FaultCodesList => new BindableCollection<Dtc>(faultCodesService.FaultCodesList);
    }
}
