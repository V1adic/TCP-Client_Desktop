using System;
using Gtk;
using System.Threading.Tasks;
using UI = Gtk.Builder.ObjectAttribute;

namespace TestGtkApp
{
    class Users : Window
    {
        [UI] private Label _label = null;
        [UI] private Button Sender = null;
        [UI] private Button ToChats = null;
        [UI] private Entry TextBox = null;

        public Users() : this(new Builder("Users.glade")) { }

        private Users(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            DeleteEvent += Window_DeleteEvent;
            Sender.Clicked += Button1_Clicked;
            ToChats.Clicked += ToChats_Clicked;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void ToChats_Clicked(object sender, EventArgs a)
        {
            Destroy();
            Chats mainWindow = new Chats();
            mainWindow.ShowAll();
            Application.Run();
        }

        private void Button1_Clicked(object sender, EventArgs a)
        {
            string name = TextBox.Text;
            if(!name.Contains(' ') && !name.Equals(""))
            {
                // if(!name.Equals(Program.MyName))
                // {
                    if(Program.CommandGetIPCLAsync(name).Result)
                    {
                        _label.Text = "Ваше имя одобрено";
                        Program.ifOnline = true;
                        Destroy();
                        FileSender mainWindow = new FileSender();
                        mainWindow.ShowAll();
                        Application.Run();
                    }
                    else
                    {
                        _label.Text = "Данный пользователь либо не в сети, либо не существует";
                    }
                // }
                // else
                // {
                //     _label.Text = "Нельзя пересылать файлы самому себе";
                // }
            }
            else
            {
                _label.Text = "Имя не должно быть пустым или с пробелами";
            }
        }
    }
}
