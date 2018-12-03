using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCTemplate.Models
{
    //Creating a view model to show errors when the request id is not appropriate
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
