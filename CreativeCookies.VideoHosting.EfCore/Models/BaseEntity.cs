using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreativeCookies.VideoHosting.EfCore.Models
{
    public abstract class BaseEntity
    {
        public Guid Id { get; private set; }
        public DateTime DateCreated { get; private set; }
        public DateTime DateUpdated { get; set; }

        public BaseEntity()
        {
            Id = Guid.NewGuid();
            DateCreated= DateTime.Now;
        }
    }
}
