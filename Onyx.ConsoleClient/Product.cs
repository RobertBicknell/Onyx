using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.API.Products.Client.Models
{
    public class Product
    {
        //[PrimaryKey]
        //public int ID { get; set; } //Id?
        public string Name { get; set; }
        public string Colour { get; set; }
    }
}