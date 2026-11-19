string targetDirectory = @"C:\Temp\FileMonitorTest"; // use o mesmo caminho do appsettings.json
Directory.CreateDirectory(targetDirectory);

Console.WriteLine($"Gerador de arquivos iniciadio. Destino: {targetDirectory}.");
Console.WriteLine("Pressione [ENTER] para gerar um arquivo, [A] para gerar um lote de 5 arquivos, ou [ESC] para sair.");

int counter =1;

while (true)
{
    var key = Console.ReadKey(intercept: true).Key;

    if (key == ConsoleKey.Escape)
        break;

    if(key == ConsoleKey.Enter)
    {
        await CriarArquivoAsync(targetDirectory, counter++);
    }

    else if(key == ConsoleKey.A)
    {
        for (int i = 0; i < 5; i++)
        {
            await CriarArquivoAsync(targetDirectory, counter++);
            await Task.Delay(100); // Pequena pausa entre os arquivos
        }
    }
}

static async Task CriarArquivoAsync(string directory, int id)
{
    string fileName = $"doc_{DateTime.Now:yyyyMMdd_HHmmss}_{id:D3}.txt";
    string fullPath = Path.Combine(directory, fileName);
    string content = $"Payload de teste #{id} gerado em {DateTime.Now:yyyy-MM-dd HH:mm:ss}";


    await File.WriteAllTextAsync(fullPath, content);
    Console.WriteLine($"[+] Criado: {fileName}");
}