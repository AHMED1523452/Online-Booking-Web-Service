//using Microsoft.EntityFrameworkCore.Metadata;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using RabbitMQ.Client;
//using System.Threading.Tasks;

//namespace Infrastructure.RabbitMQ
//{
//    public class RabbitMQConnectionManager : IRabbitMQConnectionManager, IDisposable
//    {
//        private readonly ILogger<RabbitMQConnectionManager> _logger;
//        private readonly ConnectionFactory factory;
//        private IConnection _connection;
//        private Semaphore _connectionLock = new(1, 1);

//        private RabbitMQConnectionManager(IConfiguration  configuration,
//                                          ILogger<RabbitMQConnectionManager> logger)
//        {
//            _logger = logger;
//            factory = new ConnectionFactory()
//            {
//                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
//                UserName = configuration["RabbitMQ:UserName"] ?? "guest",
//                Password = configuration["RabbitMQ:Password"] ?? "guest",
//                Port = int.Parse(configuration["RabbitMQ:Port"] ?? "15672"),
//                AutomaticRecoveryEnabled = true,
//                NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
//                TopologyRecoveryEnabled = true,
//            };
//        }

//        public async Task<IConnection> GetConnection()
//        {
//            //. that's meaning that if there is a connection and it's open return it
//            //. before entering the lock section we need to validate if the connection is open or not to avoid unnecessary locking
//            if (_connection is { IsOpen: true } && _connection.IsOpen) return _connection;

//            await _connectionLock.WaitAsync();
//            try
//            {
//                //. another way to check if the connection is open or not after the lock section to avoid unnecessary locking
//                if (_connection.IsOpen || _connection is { IsOpen : true}) return _connection;

//                //. important to log the connection opening to know if the connection is being opened or not
//                _logger.LogInformation("Opening RabbitMQ connection...");

//                //. create the connection and subscribe to the connection shutdown event to log the reason of the shutdown
//                _connection = await factory.CreateConnectionAsync();

//                //. subscribe to the connection shutdown event to log the reason of the shutdown
//                //.  and log the connection shutdown event to know if the connection is being shutdown or not
//                await _connection.ConnectionShutdownAsync += async (sender, args) =>
//                {
//                    _logger.LogWarning("RabbitMQ connection shutdown. Reason: {Reason}", args.ReplyText);
//                };
//                return _connection;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error occurred while creating RabbitMQ connection.");
//                throw;
//            }
//        }

//        public async Task Dispose()
//        {
//            await _connection?.CloseAsync();
//            _connection.Dispose();
//        }

//        public async Task<IModel> GetChannel() => await GetConnection().ContinueWith(task => task.Result.CreateModel());
//    }
//}
