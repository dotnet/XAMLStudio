using System;
using System.Collections.Generic;
using XamlStudio.Models;

namespace XamlStudio.Services
{
    public class SuspensionState
    {
        public XamlDocument[] OpenFiles { get; set; }

        public DateTime SuspensionDate { get; set; }
    }
}
