
# Evite `DateTime.Now` em produção (.NET 10) — UTC na nuvem (Azure/AWS) e contêineres

Em sistemas distribuídos modernos (Azure/AWS, Kubernetes, Docker), usar `DateTime.Now` causa bugs em fusos/DST e dificulta testes. **Regra de ouro**:

> **Armazene em UTC. Exiba em Local.**

## Por que evitar `DateTime.Now`
- Depende do horário local do servidor; apps em múltiplas regiões ficam inconsistentes. (ASP.NET containers escutam 8080 por padrão; foque em UTC no backend) [mcr docs](https://mcr.microsoft.com/en-us/product/dotnet/aspnet/about)
- Mudanças de horário de verão (DST) geram ambiguidades/tempos duplicados.
- Testes e depuração são mais simples com **TimeProvider** (abstração nativa). [MS Learn](https://learn.microsoft.com/en-us/dotnet/standard/datetime/timeprovider-overview)

## .NET 10 e containers oficiais
- **.NET 10** lançado na .NET Conf 2025; imagens oficiais disponíveis. [.NET Blog](https://devblogs.microsoft.com/dotnet/dotnet-conf-2025-recap/) · [.NET Docker 10.0 GA](https://github.com/dotnet/dotnet-docker/discussions/6801)
- Linux padrão das tags neutras: **Ubuntu 24.04 (Noble)**; há variantes **Alpine**, **Azure Linux** e **chiseled/distroless**. [.NET Docker 10.0 GA](https://github.com/dotnet/dotnet-docker/discussions/6801)

## Como este exemplo aplica UTC
- **Código** usa `TimeProvider.GetUtcNow()` via `IClock` para persistência/logs.
- **Containers** definem `TZ=Etc/UTC` e instalam `tzdata`.
- **Kubernetes/Compose** padronizam o ambiente em UTC.

## Rodando localmente
```bash
# Ubuntu base
docker build -t api-utc:10 .
docker run -it --rm -p 8080:8080 api-utc:10

# Alpine base
docker build -f Dockerfile.alpine -t api-utc:10-alpine .
docker run -it --rm -p 8080:8080 api-utc:10-alpine

# Teste
curl http://localhost:8080/utc-now
```

## Arquitetura (conceito)
```mermaid
flowchart LR
    subgraph Client[Cliente (Web/Mobile)]
        UI[UI: exibe horário local]
    end
    subgraph Cloud[Azure/AWS]
        API[API .NET 10 em containers
TZ=Etc/UTC
Usa UtcNow/TimeProvider]
        Queue[Mensageria (ASB/SQS/Kinesis)
Timestamps em UTC]
        DB[DB (Cosmos/Azure SQL/RDS/PG)
Datas em UTC]
        Logs[Observabilidade (Monitor/CloudWatch)
Logs em UTC]
    end
    UI --> API
    API --> Queue --> API
    API --> DB
    API --> Logs
```

## Referências
- ASP.NET Core containers (porta 8080): https://mcr.microsoft.com/en-us/product/dotnet/aspnet/about
- TimeProvider overview: https://learn.microsoft.com/en-us/dotnet/standard/datetime/timeprovider-overview
- .NET 10 GA e imagens: https://github.com/dotnet/dotnet-docker/discussions/6801 · https://devblogs.microsoft.com/dotnet/dotnet-conf-2025-recap/

## Licença
MIT
