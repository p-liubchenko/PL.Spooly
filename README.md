# Pricer

CLI tool to manage 3D-print related costs (materials, printers, transactions) and calculate print price.

## Prerequisites

- .NET SDK `10.0`

## Run

```pwsh
cd .\Pricer.Cli
dotnet run
```

## Configuration

The CLI loads configuration from (in order):

1. `Pricer.Cli\appsettings.json`
2. `Pricer.Cli\appsettings.Development.json` (optional)
3. Environment variables with prefix `PRICER_`
4. User Secrets (optional)
5. Command-line arguments

### Data access mode

Data access is configured via the `DataAccess` section.

#### File (default)

Stores data in a local JSON file.

`Pricer.Cli\appsettings.json`:

```json
{
  "DataAccess": {
    "Mode": "File"
  }
}
```

When running the CLI from `Pricer.Cli`, the data file is `Pricer.Cli\data.json`.

#### MSSQL

Uses EF Core + SQL Server.

`Pricer.Cli\appsettings.json`:

```json
{
  "DataAccess": {
    "Mode": "Mssql"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Pricer;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Notes:

- Connection string key must be `ConnectionStrings:DefaultConnection`.
- When `Mode` is `Mssql`, the CLI applies any pending EF migrations automatically on startup.

##### Using User Secrets (recommended for local dev)

```pwsh
cd .\Pricer.Cli

dotnet user-secrets init

dotnet user-secrets set "DataAccess:Mode" "Mssql"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=Pricer;Trusted_Connection=True;TrustServerCertificate=True"
```

### Environment variables

The CLI uses the prefix `PRICER_`. Use `__` to represent `:` in nested keys:

- `PRICER_DataAccess__Mode=Mssql`
- `PRICER_ConnectionStrings__DefaultConnection=...`
