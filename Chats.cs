using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace TestGtkApp
{
    class Chats : Window
    {
        [UI] private Button Chat = null;
        [UI] private Button File = null;


        public Chats() : this(new Builder("Chats.glade")) { }

        private Chats(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            DeleteEvent += Window_DeleteEvent;
            Chat.Clicked += Chat_Clicked;
            File.Clicked += File_Clicked;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();

        }

        private void Chat_Clicked(object sender, EventArgs a)
        {
            Destroy();
            OwnerChat mainWindow = new OwnerChat();
            Program.Chats = mainWindow;
            mainWindow.ShowAll();
            Application.Run();
        }
        private void File_Clicked(object sender, EventArgs a)
        {
            Destroy();
            Users mainWindow = new Users();
            mainWindow.ShowAll();
            Application.Run();
        }
    }
}