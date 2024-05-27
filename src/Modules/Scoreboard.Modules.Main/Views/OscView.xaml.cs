using System.Windows;
using System.Windows.Controls;

namespace Scoreboard.Modules.Main.Views
{
    /// <summary>
    /// Логика взаимодействия для OscView.xaml
    /// </summary>
    public partial class OscView : UserControl
    {
        public OscView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var myWindow = Window.GetWindow(this);
            myWindow.Close();
        }
    }
}
