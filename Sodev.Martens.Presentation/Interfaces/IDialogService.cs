namespace Sodev.Marten.Presentation.Interfaces
{
    public interface IDialogService
    {
        void DisplayError(string message, string title);

        void DisplayInfo(string message, string title);
    }
}