using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Vendimia.Models
{
    public class _Settings
    {
        public static string db = ConfigurationManager.ConnectionStrings["BD"].ConnectionString;
    }
}