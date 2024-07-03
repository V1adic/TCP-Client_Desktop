using System;
using System.IO;
using Gtk;
using System.Threading.Tasks;
using UI = Gtk.Builder.ObjectAttribute;

namespace TestGtkApp
{
    class FileSender : Window
    {
        [UI] private Label _label = null;
        [UI] private Button ToUser = null;
        [UI] private Entry TextBox = null;
        [UI] private Button FileChooserGTK = null;
        [UI] private Button Sender = null;


        public FileSender() : this(new Builder("FileSender.glade")) { }

        private FileSender(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            builder.Autoconnect(this);
            DeleteEvent += Window_DeleteEvent;
            ToUser.Clicked += ToUser_Clicked;
            FileChooserGTK.Clicked += FileChooserGTK_Clicked;
            Sender.Clicked += Sender_Clicked;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void Sender_Clicked(object sender, EventArgs a)
        {
            string path = TextBox.Text;
            if(path.Contains('.') && !path.Equals(""))
            {
                if(File.Exists(path))
                {
                    Task t = Program.CommandFileServicesAsync(path);
                    t.Wait();
                    _label.Text = "Файл был отправлен, пожалуйста подождите немного";
                }
                else
                {
                    _label.Text = "Файл по ссылке не найден";
                }
            }
            else
            {
                _label.Text = "Недопустимый формат ссылки на файл";
            }
        }

        private void FileChooserGTK_Clicked(object sender, EventArgs e)
	    {
            FileChooserDialog fileChooser = new FileChooserDialog("Choose a file", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept)
            {
                Filter = new FileFilter()
            };
            fileChooser.Filter.AddPattern("*.*");

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                string filePath = fileChooser.Filename;
                // Do something with the selected file path
                TextBox.Text = filePath;
            }

            fileChooser.Destroy();
	    }

        private void ToUser_Clicked(object sender, EventArgs a)
        {
            Program.nameCL = "";
            Destroy();
            Users mainWindow = new Users();
            mainWindow.ShowAll();
            Application.Run();
        }
    }
}
