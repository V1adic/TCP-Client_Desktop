using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace TestGtkApp
{
    class MainWindow : Window
    {
        [UI] private readonly Label _label1 = null;
        [UI] private readonly Button _button1 = null;
        [UI] private readonly Entry TextBox = null;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            DeleteEvent += Window_DeleteEvent;
            _button1.Clicked += Button1_Clicked;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void Button1_Clicked(object sender, EventArgs a)
        {
            string name = TextBox.Text;
            if(!name.Contains(' ') && !name.Equals(""))
            {
                if(Program.CommandNameAsync(name).Result)
                {
                    _label1.Text = "Ваше имя одобрено";
                    Destroy();
                    Chats mainWindow = [];
                    mainWindow.ShowAll();
                    Application.Run();
                }
                else
                {
                    _label1.Text = "Текущее имя занято, выберите другое";
                }
            }
            else
            {
                _label1.Text = "Ваше имя должно быть без пробелов и недолжно быть пустым";
            }
        }
    }
}
