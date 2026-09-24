using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.RabbitMQ
{
    public interface IRabbitMQConnectionManager
    {
        IModel GetChannel();
    }
}
