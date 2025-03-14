
# ConsolidationApi
## Como rodar a aplicação BalanceLedgerApi e ConsolidationApi com Docker e MongoDB

Este projeto consiste em duas aplicações .NET 9 (BalanceLedgerApi e ConsolidationApi) e um MongoDB rodando no Docker. O Docker será usado para hospedar o MongoDB, enquanto as aplicações .NET serão executadas localmente.

## Requisitos

Antes de começar, certifique-se de que você tem o seguinte instalado:

- [Docker](https://www.docker.com/get-started) (com o Docker Desktop ou Docker Engine)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

## Executando o MongoDB

Na raiz do projeto BalanceLedgerApi, há um arquivo chamado [docker-compose.yml] que configura o MongoDB.

1. Abra um terminal, navegue até o diretório onde o arquivo [docker-compose.yml] está localizado.
2. execute o tomando

*docker-compose up -d*

O MongoDB estará rodando na porta 27017

O banco de dados será inicializado com as credenciais de teste: admin e admin123

## Executando BalanceLedgerApi

1. Navegue até o diretório da aplicação /BalanceLedgerApi
2. Execute os comandos

```
dotnet restore

dotnet run
```

## Executando ConsolidationApi

1. Navegue até o diretório da aplicação /ConsolidationApi
2. Execute os comandos

```
dotnet restore

dotnet run
```

## Testando as aplicações

Há um projeto no postman previamente configurado com todos endpoints. (dentro da BalanceLedger)

Certifique-se apenas que estão rodando nas portas:

BalanceLedgerApi: https://localhost:5113

ConsolidationApi: https://localhost:5192

Caso contrário, será necessário alterar as portas

Com swagger:

BalanceLedgerApi: https://localhost:5113/swagger/index.html

ConsolidationApi: https://localhost:5192/swagger/index.html

**Importante**
Pegar o token via endpoint de autenticação e o cadastrar no Authorize do Swagger

## Configurações das apps

A ConsolidationApi possuí um Cron Job ao qual por padrão está configurado para rodar uma vez ao dia, a uma da manhã (0 0 1 * * ?). 

Por motivo de teste, pode ser alterado a configuração da aplicação. Localizado no appsettings.json. 

Para rodar a cada 30 segundos: (0/30 * * * * ? *)

### Authorization

Os endpoints possuem uma autenticação básica, para autenticar use o endpoint da BalanceLedgerApi: */auth/authenticate*

usuário padrão
```
{
  "email": "teste@teste.com",
  "password": "minhaSenha"
}
```
Após isso, para as chamadas, usar o token retornado no header.

Exemplo:

```authorization: Bearer {token}```
(No swagger não precisa adicionar Bearer)

## Endpoints

### BalanceLedgerApi

#### /transaction/add

Recebe:
```
{
  "type": 1, --1 = credit, 2 = dbit
  "value": 15 -- valor da operação
}
```

(Sistema ainda não valida entrada)

#### transaction/all

Lista todas transações. Criado exclusivamente para visualização em teste

### ConsolidationApi

#### /consolidation/run-by-range

Consolida dados de um range de data. Criado exclusivamente para facilitar sua visualização.

#### /consolidation/all

Endpoint que retorna todas consolidações salvas

#### /consolidation/export

Exporta últimas consolidações em excel. Recebe um *consolidationsNumber* indicando quantas. Também apenas para facilitar sua avaliação.

Importante, para esse endpoint usando postman, usar a opção de *send and download* para salvar o arquivo.


### Parando docker-compose

Use o comando ```docker-compose down```
