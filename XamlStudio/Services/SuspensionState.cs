using System;
using XamlStudio.Models;

namespace XamlStudio.Services
{
    public class SuspensionState
    {
        public string OpenActivity { get; set; }

        public XamlDocument[] OpenFiles { get; set; }

        //// Note: Technically only a single open workspace is supported currently, but making this an array for future easy of migration.
        public FolderLocation[] OpenWorkspaces { get; set; }

        public bool FromRender { get; set; }

        public string LastRenderedId { get; set; }

        public DateTime SuspensionDate { get; set; }
    }
}
