using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectAirBand.ViewModels
{
    public class FacebookViewModel : ViewModelBase
    {
        public String AccessToken { get; set; }

        private String userName = "";
        public String UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
                OnPropertyChanged("UserName");
            }
        }

        private String photoUrl = "";
        public String PhotoUrl
        {
            get
            {
                return photoUrl;
            }
            set
            {
                photoUrl = value;
                OnPropertyChanged("PhotoUrl");
            }
        }

        private String description = null;
        public String Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }
    }
}