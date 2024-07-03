using System;
using System.IO;
using System.Threading;
using Gtk;
using System.Threading.Tasks;
using UI = Gtk.Builder.ObjectAttribute;

namespace TestGtkApp
{
    public class OwnerChat : Window
    {
        [UI] public Label chat_label = null;
        [UI] private Button ToChats = null;
        [UI] private Button Sender = null;
        [UI] private Entry TextBox = null;

        public OwnerChat() : this(new Builder("OwnerChat.glade")) { }

        private OwnerChat(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            Task t = Program.CommandUpdateCLAsync();
            t.Wait();
            Thread.Sleep(100);
            builder.Autoconnect(this);
            chat_label.Text = File.ReadAllText(Program.GroupChat);
            Program.if_in_Group_Chat = true;
            DeleteEvent += Window_DeleteEvent;
            Sender.Clicked += Sender_Clicked;
            ToChats.Clicked += ToChats_Clicked;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            Application.Quit();
        }

        private void Sender_Clicked(object sender, EventArgs a)
        {
            string mess = TextBox.Text;
            TextBox.Text = "";
            if(!mess.Equals(""))
            {
                chat_label.Text += $"{Program.MyName}: {mess}\n";
                Task t = Program.CommandMessageCLAsync(mess);
                t.Wait();
            }
            else {}
        }
        
        private void ToChats_Clicked(object sender, EventArgs a)
        {
            Program.Chats = null;
            Program.if_in_Group_Chat = false;
            Destroy();
            Chats mainWindow = new Chats();
            mainWindow.ShowAll();
            Application.Run();
        }
    }
}