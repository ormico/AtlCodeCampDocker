﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqService
{
    public class Customer
    {
        public Customer()
        {

        }

        public long CustomerID { get; set; }
        public string FullName { get; set; }
    }
}
