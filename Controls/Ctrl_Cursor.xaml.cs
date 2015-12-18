using System.Windows.Controls;

namespace AirBand.Controls
{
    public partial class Ctrl_Cursor : UserControl
    {
        public Ctrl_Cursor()
        {
            InitializeComponent();
        }

        public void Down()
        {
            this.Begin("Down_Cursor");
        }

        public void Up()
        {
            this.Begin("Up_Cursor");
        }
    }
}