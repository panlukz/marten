using MaterialDesignThemes.Wpf;
using Sodev.Marten.Presentation.Features.Dialogs;
using Sodev.Marten.Presentation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Presentation.Services
{
    public class DialogService : IDialogService
    {
        public async void DisplayError(string message, string title)
        {
            await DialogHost.Show(new GenericDialog() { DataContext = new GenericDialogModel(message, title) });
        }

        public async void DisplayInfo(string message, string title)
        {
            await DialogHost.Show(new GenericDialog() { DataContext = new GenericDialogModel(message, title) });
        }
    }
}
