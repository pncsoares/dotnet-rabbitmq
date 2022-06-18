﻿using Producer;
using RabbitMQ.Client;

var factory = new ConnectionFactory
{
    Uri = new Uri(Shared.Constants.Uri)
};

using var connection = factory.CreateConnection();

using var channel = connection.CreateModel();

QueueProducer.Publish(channel);