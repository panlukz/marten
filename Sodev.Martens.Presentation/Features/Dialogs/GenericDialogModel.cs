using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Presentation.Features.Dialogs
{
    public class GenericDialogModel
    {
        public GenericDialogModel()
        {

        }

        public GenericDialogModel(string message, string title)
        {
            Title = title;
            Message = message;
        }

        public string Title { get; private set; }
        public string Message { get; private set; }
    }
}
