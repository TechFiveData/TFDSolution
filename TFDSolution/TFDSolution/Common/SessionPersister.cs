using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TFDSolution.Transport.Master;

namespace TFDSolution.Common
{
    public class SessionPersister
    {
        public static LoginUserInfo LoginedUser
        {
            get
            {
                if (HttpContext.Current == null)
                    return new LoginUserInfo();
                var sessionVar = HttpContext.Current.Session["LoginUserInfo"];
                if (sessionVar != null)
                    return sessionVar as LoginUserInfo;
                return null;
            }
            set
            {
                HttpContext.Current.Session["LoginUserInfo"] = value;
            }
        }        
    }
}