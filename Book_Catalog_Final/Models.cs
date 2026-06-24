using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Catalog_Final
{
    public class ListBoxItem
    {
        public string DisplayText { get; set; }
        public string Isbn { get; set; }
        public override string ToString() => DisplayText;
    }
}
