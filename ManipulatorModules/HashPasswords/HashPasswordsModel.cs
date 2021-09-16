using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FortyFingers.DnnMassManipulate.Components;

namespace FortyFingers.DnnMassManipulate.ManipulatorModules.HashPasswords
{
    /// <summary>
    /// 
    /// </summary>
    public class HashPasswordsModel
    {
        public ContextHelper Context { get; set; }
        public string Log { get; set; }
        public List<PasswordModel> Passwords { get; set; } = new List<PasswordModel>();
    }

    public class PasswordModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}