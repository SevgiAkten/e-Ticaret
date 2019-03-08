using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVCeTicaret.Models
{
    public class ShippingDetail
    {
        [Key]
        public int ShippingID { get; set; }


        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Mobile { get; set; }

        [MaxLength(150), Required]
        public string Address { get; set; }

        [MaxLength(20)]
        public string Province { get; set; }

        [MaxLength(20), Required]
        public string City { get; set; }

        [MaxLength(20)]
        public string PostalCode { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}