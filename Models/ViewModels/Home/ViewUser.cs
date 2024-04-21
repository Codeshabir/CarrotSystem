using CarrotSystem.Models.MPS;
using System;
using System.Collections.Generic;

namespace CarrotSystem.Models.ViewModel
{
    public partial class ViewUser
    {
        public User user { get; set; }
        public List<User> userList { get; set; }

        public string loginId { get; set; }
        public string userAddType { get; set; }

    }
}
