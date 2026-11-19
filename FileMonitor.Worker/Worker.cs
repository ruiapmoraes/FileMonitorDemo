using System.Threading.Channels;

namespace FileMonitor.Worker
{
    public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
    {
        private FileSystemWatcher? _watcher;
        private readonly Channel<string> _fileQueue = Channel.CreateUnbounded<string>();
        private readonly string _watchPath = configuration["MonitorSettings:WatchPath"]
            ?? Path.Combine(Path.GetTempPath(), "FileMonitorTest");
        private readonly string _filter = configuration["MonitorSettings:Filter"] ?? "*.txt";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Directory.CreateDirectory(_watchPath);
            ConfigurarWatcher();
            logger.LogInformation("Monitoring path: {WatchPath} with filter: {Filter}", _watchPath, _filter);

            try
            {
                // Consome a fila de arquivos detectados de forma assíncrona
                await foreach (var filePath in _fileQueue.Reader.ReadAllAsync(stoppingToken))
                {
                    await ProcessarArquivoAsync(filePath, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Comportamento esperado ao encerrar o serviço via stoppingToken
                logger.LogInformation("Serviço de monitoramento encerrado.");
            }

        }

        private void ConfigurarWatcher()
        {
            _watcher = new FileSystemWatcher(_watchPath, _filter)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnFileCreated;
            _watcher.Error += OnWatcherError;
        }
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            logger.LogInformation("Novo Arquivo detectado: {FileName}", e.Name);
            _fileQueue.Writer.TryWrite(e.FullPath);
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            logger.LogError("Erro ocorrido durante o monitoramento de arquivos: {ErrorMessage}", e.GetException().Message);
        }


        private async Task ProcessarArquivoAsync(string filePath, CancellationToken ct)
        {
            // Aguarda até que o processo gerador libere o lock do arquivo
            const int maxRetries = 5;
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(filePath, ct);
                    logger.LogInformation("Arquivo processado com sucesso: {File} | Conteúdo: {Content}",
                    Path.GetFileName(filePath), content.Trim());
                    return;
                }
                catch(IOException) when (attempt < maxRetries)
                {
                    logger.LogWarning("Arquivo bloqueado, tentando novamente ({Attempt}/{Max})...", attempt, maxRetries);
                    await Task.Delay(200 * attempt, ct);
                }

                catch (Exception ex)
                {
                    logger.LogError(ex, "Falha ao processar o arquivo {File}", filePath);
                    return;
                }
            }
        }

        public override void Dispose()
        {
            _watcher?.Dispose();
            base.Dispose();
        }
    }
}
