using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlStudio.Toolkit.Models;

namespace XamlStudio.Toolkit.Services
{
    // TODO: Feel like this could have better data typing to serve it's purpose... should think about this more.
    /// <summary>
    /// Manages connections between <see cref="XamlRenderService"/> and <see cref="XamlBindingWrapperConverter"/>.
    /// </summary>
    public class XamlBindingWrapperManager : Dictionary<int, XamlBindingInfo>
    {
        private Dictionary<int, XamlRenderService> _renderers = new Dictionary<int, XamlRenderService>();
        private Dictionary<int, List<int>> _tracker = new Dictionary<int, List<int>>();

        // http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly XamlBindingWrapperManager _instance = new XamlBindingWrapperManager();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static XamlBindingWrapperManager() { }
        private XamlBindingWrapperManager() { }

        public static XamlBindingWrapperManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public void Register(int id, XamlRenderService service)
        {
            this._renderers.Add(id, service);
            this._tracker[id] = new List<int>();
        }

        public void AddNewBinding(int serviceId, XamlBindingInfo binding)
        {
            binding.Service = this._renderers[serviceId];
            this.Add(binding.Id, binding);
            this._tracker[serviceId].Add(binding.Id);
        }

        public IEnumerable<XamlBindingInfo> GetBindings(int serviceId)
        {
            foreach (var binding in this._tracker[serviceId])
            {
                yield return this[binding];
            }
        }

        /// <summary>
        /// Removes all previous bindings related to a XamlRenderService reference.
        /// </summary>
        /// <param name="serviceId"></param>
        public void Clear(int serviceId)
        {
            foreach (var binding in this._tracker[serviceId])
            {
                this.Remove(binding);
            }

            this._tracker[serviceId] = new List<int>();
        }
    }
}
