using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WindowsService_HostAPI
{
    public class ValuesController : ApiController
    {
        public String GetString(Int32 id)
        {
            return "This is string returned through the Windows service.";
        }
    }
}