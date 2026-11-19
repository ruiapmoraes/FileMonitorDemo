# 📁 FileMonitor Solution

Solução em **.NET** composta por um **Worker Service** para monitoramento de diretórios em tempo real utilizando `FileSystemWatcher` e uma **Console Application** auxiliar para geração controlada de arquivos de teste.

## 🏗️ Arquitetura e Projetos

A solução está dividida em dois projetos principais:

* **`FileMonitor.Worker`**: Serviço em segundo plano (`BackgroundService`) responsável por escutar a criação de arquivos em um diretório configurado, enfileirar os eventos de forma assíncrona e processar o conteúdo com resiliência a bloqueios de arquivo (*file locks*).

* **`FileMonitor.Generator`**: Aplicação de console interativa para simular sistemas externos despejando arquivos `.txt` individualmente ou em lote no diretório monitorado.

### Destaques Técnicos

* **Fila Assíncrona (`System.Threading.Channels`)**: Desacopla a captura do evento do `FileSystemWatcher` do processamento efetivo do arquivo, evitando bloqueios na thread de eventos do sistema operacional.

* **Resiliência a *File Lock***: Implementação de retentativas (*retry* com *backoff* linear) para aguardar a liberação do arquivo caso o processo criador ainda esteja finalizando a escrita no disco.

* **Graceful Shutdown**: Encerramento limpo do serviço (`StopAsync` e tratamento de `OperationCanceledException`), desativando o monitoramento e concluindo o canal de leitura sem exceções não tratadas durante a parada ou depuração.

## ⚙️ Pré-requisitos

* [.NET SDK 11.0](https://dotnet.microsoft.com/download) (ou .NET 10.0 LTS, ajustando o `<TargetFramework>` nos arquivos `.csproj` caso necessário).

* Git instalado para clonar e versionar o repositório.

## 🚀 Como Executar

### 1. Clonar o repositório

```bash
git clone https://github.com/seu-usuario-github/FileMonitorSolution.git
cd FileMonitorSolution
```

### 2. Configurar o diretório monitorado (Opcional)

Por padrão, o `FileMonitor.Worker` monitora o caminho definido no `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "MonitorSettings": {
    "WatchPath": "C:\\Temp\\FileMonitorTest",
    "Filter": "*.txt"
  }
}
```

> **Nota:** Caso utilize Linux ou macOS, altere o `WatchPath` para um caminho válido no sistema (ex: `/tmp/FileMonitorTest`). Certifique-se também de que o caminho configurado em `FileMonitor.Worker/appsettings.json` seja o mesmo utilizado na variável `targetDirectory` em `FileMonitor.Generator/Program.cs`.

### 3. Iniciar o Worker Service

Em um terminal na raiz da solution, execute:

```bash
dotnet run --project FileMonitor.Worker
```

### 4. Iniciar o Gerador de Arquivos

Em um segundo terminal na raiz da solution, execute:

```bash
dotnet run --project FileMonitor.Generator
```

#### Comandos disponíveis no Gerador:

* `[ENTER]` — Gera um único arquivo `.txt` com timestamp e payload de teste.

* `[A]` — Gera um lote de 5 arquivos consecutivos para testar a fila assíncrona e o tratamento de concorrência.

* `[ESC]` — Encerra a aplicação geradora.

## 📂 Estrutura de Pastas

```text
FileMonitorSolution/
├── FileMonitor.Worker/
│   ├── Program.cs
│   ├── Worker.cs
│   ├── appsettings.json
│   └── FileMonitor.Worker.csproj
├── FileMonitor.Generator/
│   ├── Program.cs
│   └── FileMonitor.Generator.csproj
├── FileMonitorSolution.sln
├── .gitignore
└── README.md
```

## 🛠️ Próximas Evoluções (Roadmap)

- [ ] Deduplicação de eventos consecutivos disparados pelo `FileSystemWatcher`.
- [ ] Movimentação automática dos arquivos lidos para subdiretórios `/Processados` e `/Erro`.
- [ ] Varredura periódica de *fallback* para garantir processamento em cenários de alto volume (*buffer overflow*).
- [ ] Suporte nativo para execução como Windows Service (`Microsoft.Extensions.Hosting.WindowsServices`) ou daemon systemd no Linux.

## 👨‍💻 Autor

**Rui De Moraes**

* **GitHub:** [https://github.com/ruiapmoraes](https://github.com/ruiapmoraes)
* **LinkedIn:** [https://www.linkedin.com/in/rui-moraes](https://www.linkedin.com/in/rui-moraes)

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.
