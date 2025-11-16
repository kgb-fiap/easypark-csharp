
# EasyPark .NET API

## Visão Geral

**EasyPark** é uma API escrita em **ASP.NET Core** com **Entity Framework Core** que atende a um sistema de estacionamento multi-sítio.  A aplicação permite que usuários reservem vagas em diferentes estacionamentos, acompanhem o status em tempo real por sensores e realizem o pagamento após o uso.  O ciclo de vida da reserva passa por diversos estados – **PRE_RESERVA → RESERVA → OCUPADA → PAGA/CANCELADA** – enquanto um conjunto de procedimentos armazenados e *triggers* no banco de dados Oracle controlam etapas automatizadas como timeouts e confirmação de ocupação.  Além disso, o sistema integra-se a um serviço de ETA (via Google Maps Directions) para atualizar o tempo estimado de chegada e acionar transições de estado.

## Principais Funcionalidades

 - **Gestão de Estacionamentos**: Operadoras podem cadastrar estacionamentos com parâmetros como tempo de espera, tolerância de atraso, limites de antecedência e restrições de no‑show.  **A criação ou atualização exige o envio de um endereço completo em um objeto aninhado (`endereco`) contendo a hierarquia UF → Cidade → Bairro → Endereço; a API cria ou atualiza essa hierarquia automaticamente.**  Cada estacionamento possui vários níveis.
- **Gestão de Vagas**: Dentro de cada nível há vagas físicas classificadas por tipo (elétrica, acessível, moto, etc.), com tarificação por minuto configurável.  As vagas podem ser ativadas/inativadas e são monitoradas por sensores.
- **Sensores e Telemetria**: Cada vaga pode ter um sensor que transmite eventos de ocupação (*OCUPADA*, *LIVRE* ou *DESCONHECIDO*).  Uma trigger no Oracle atualiza a tabela de cache `VAGA_STATUS` e pode transitar uma reserva para **OCUPADA** quando um sensor reporta ocupação.
- **Reservas em Etapas**: Usuários criam reservas informando antecedência desejada; a API calcula a janela de chegada permitida com base nas regras do estacionamento.  Quando o ETA se aproxima (via integração externa), o sistema bloqueia a vaga e transita de **PRE_RESERVA** para **RESERVA**.  O sensor confirma a presença e muda para **OCUPADA**.  Após a liberação da vaga, o pagamento é iniciado e, quando concluído, a reserva passa para **PAGA**.  Timeouts automáticos cancelam reservas ou pré‑reservas fora da janela permitida.
- **Pagamentos**: Integração com provedores de pagamento permite cobrar o valor final com idempotência e armazenar informações do pagador e cartão de forma segregada.  O `PagamentoPagador` tem relação opcional com `Endereço`, permitindo registrar dados de cobrança.
- **Busca Paginada + HATEOAS**: Estacionamentos e Vagas possuem rotas `/search` com filtros de domínio, paginação configurável e ordenação.  As respostas dos endpoints de detalhe retornam envelopes HATEOAS com links de `self`, `update`, `delete` e demais ações relacionadas.
- **CRUD exposto para Reservas e Pagamentos**: Além dos jobs e do monitoramento por sensores, a sprint atual disponibiliza controladores e serviços completos para criar, buscar, atualizar e remover reservas, assim como registrar pagamentos com dados de pagador/cartão.
## Arquitetura da Solução

O projeto segue uma arquitetura em camadas, com separação de responsabilidades:

1. **Domínio/Entidades**: Classes de domínio representam as tabelas do Oracle.  Cada entidade contém atributos mapeados com data annotations ou configuração no `DbContext`.  Booleans são convertidos para `'Y'/'N'`, decimais têm precisão configurada e relações 1‑1/1‑N são explicitadas com *cascade* apropriado.
2. **DTOs**: Objetos de transferência encapsulam a entrada e saída da API, evitando expor diretamente entidades internas.  Para o domínio de endereços, foram criados DTOs específicos permitindo criar ou atualizar hierarquias de UF→Cidade→Bairro→Endereço em uma única chamada.
3. **Serviços**: Camada de negócio que orquestra validação, transações e regras de domínio.  Por exemplo, o `EstacionamentoService` valida parâmetros, constrói/atualiza o endereço completo e lança exceções quando regras são violadas (ex.: vaga duplicada por nível ou reserva concorrente).
4. **Controladores**: Expondo endpoints RESTful, os controllers delegam ao serviço correspondente e retornam códigos HTTP adequados (201, 200, 204, 400, 404).  Um filtro global intercepta exceções e converte em respostas JSON amigáveis.
5. **Persistência**: O `EasyParkContext` é responsável por mapear as entidades ao Oracle usando EF Core.  Chamadas a *stored procedures* (`reserva_timeouts`, `reserva_prereserva_timeouts` e `user_eta_update_process`) são encapsuladas em serviços e executadas via `Oracle.ManagedDataAccess.Core`.

## Modelo de Domínio

O modelo de dados está normalizado e abrange operações de estacionamento, reservas, pagamentos e endereços.  A seguir, uma visão condensada das entidades e seus propósitos:

- **OPERADORA**: Empresa responsável por um ou mais estacionamentos; armazena CNPJ, razão social, etc.
- **ESTACIONAMENTO**: Está vinculado a uma operadora e a um endereço; define parâmetros operacionais (espera, tolerância, limites de antecedência e de no‑show).
- **NIVEL**: Andares ou seções do estacionamento; cada nível contém várias vagas.
- **TIPO_VAGA**: Classificação e tarifa por minuto (elétrica, acessível, moto, etc.).
- **VAGA**: Representa uma vaga física ligada a um nível e tipo; pode estar ativa ou inativa.
- **SENSOR** e **SENSOR_EVENTO**: Equipamento e histórico de leituras (OCUPADA, LIVRE, DESCONHECIDO).  Uma trigger atualiza `VAGA_STATUS` com o último estado.
- **VAGA_STATUS**: Cache 1:1 da vaga, com status atual, último ocorrido e sensor associado.
- **USUARIO**: Dados de autenticação e perfil; controla suspensão por no‑shows.
- **RESERVA**: Ciclo de vida das reservas; registra tempos (previsto, confirmado, ocupado, pago), antecendência e motivo de cancelamento.  Regra de concorrência garante no máximo uma reserva ativa por usuário e por vaga.
- **RESERVA_PRECO** (1:1): Instantâneo dos parâmetros de preço no momento da criação (tarifa, percentual de antecedência, valor previsto/final) para reprodutibilidade.
- **RESERVA_HIST**: Trilhas de auditoria das transições de estado e origem do evento (ETA, Sensor, Timeout).
- **PAGAMENTO**, **PAGAMENTO_PAGADOR** e **PAGAMENTO_CARTAO**: Informações de pagamento e dados do pagador; `PagamentoPagador` referencia opcionalmente um endereço.
- **UF**, **CIDADE**, **BAIRRO**, **ENDERECO**: Domínio de endereço em 3FN; `UF` usa sigla como chave natural.  `Endereço` contém CEP, logradouro, número, complemento, bairro e coordenadas.

## Ciclo de Vida da Reserva

1. **Pré‑reserva (PRE_RESERVA)**: O usuário escolhe a vaga e informa quantos minutos de antecedência deseja.  O sistema calcula a janela permitida com base nos parâmetros do estacionamento.
2. **Atualização de ETA**: A API recebe atualizações de ETA (via serviço externo); quando o tempo estimado de chegada fica dentro da antecedência informada, a reserva transita para **RESERVA**, bloqueando a vaga.
3. **Confirmação pelo Sensor (OCUPADA)**: Ao detectar ocupação, o sensor envia um `SENSOR_EVENTO` que a trigger interpreta para transitar a reserva para **OCUPADA**.  Caso o sensor marque ocupada antes de confirmar a reserva, a situação é registrada em histórico.
4. **Pagamento (PAGA)**: Após a desocupação, calcula‑se o valor final (tarifa × duração real, percentual de antecedência, etc.) e inicia‑se o processo de pagamento.  Uma vez aprovado pelo gateway, a reserva passa a **PAGA**.  Se o usuário não pagar, a reserva pode ser **CANCELADA**.
5. **Timeouts**: Stored procedures no Oracle executam rotinas de timeout: pré‑reservas expiram se a hora atual exceder `inicio_previsto + tolerancia_minutos`; reservas expiram se o usuário não chegar dentro de `espera_minutos + tolerancia_minutos`.  Essas rotinas podem ser disparadas via endpoints de **Jobs**.

## Configuração e Execução

### Pré‑requisitos

1. **.NET SDK 8.0** ou superior instalado.  
2. **Oracle Database** com o schema do EasyPark instalado, inclusive triggers e stored procedures fornecidas no DDL.  
3. **Oracle Data Provider** (ODP.NET) para EF Core (já referenciado no projeto).

### Passos para Rodar a API Localmente

1. **Clonar o repositório**:  
   ```bash
   git clone https://github.com/seu-usuario/easypark-csharp.git
   cd easypark-csharp
   ```
2. **Configurar a conexão**: edite `appsettings.json` e ajuste a chave `ConnectionStrings:Default` com a string de conexão Oracle (usuário, senha, host, serviço).  Os parâmetros `EsperaMinutos`, `ToleranciaMinutos` e outros limites têm valores padrão, mas podem ser ajustados conforme necessidade.
3. **Restaurar dependências**:  
   ```bash
   dotnet restore
   ```
4. **Executar a API**:  
   ```bash
   dotnet run
   ```
   O Kestrel exibirá a URL base (por exemplo `http://localhost:5190`).  Utilize essa URL ao importar a coleção do Postman.
5. **Documentação Swagger**: em ambiente de desenvolvimento, acesse `/swagger` para visualizar e testar os endpoints interativamente.

## API Endpoints Principais

| Método | Rota | Descrição resumida |
|-------|------|-------------------|
| **POST** | `/api/estacionamentos` | Cadastra novo estacionamento com sua hierarquia de endereço e parâmetros operacionais |
| **GET** | `/api/estacionamentos` | Lista todos os estacionamentos cadastrados |
| **GET** | `/api/estacionamentos/{id}` | Retorna detalhes de um estacionamento específico |
| **PUT** | `/api/estacionamentos/{id}` | Atualiza dados e endereço do estacionamento |
| **DELETE** | `/api/estacionamentos/{id}` | Remove um estacionamento (erros podem ocorrer se houver níveis/vagas associados) |
| **GET** | `/api/estacionamentos/search` | Busca paginada com filtros por nome, UF, cidade e bairro (resposta HATEOAS) |
| **POST** | `/api/vagas` | Cria uma vaga em um nível e tipo específicos |
| **GET** | `/api/vagas` | Lista vagas, com filtro opcional por status (LIVRE, OCUPADA, DESCONHECIDO) |
| **GET** | `/api/vagas/{id}` | Detalhes de uma vaga |
| **PUT** | `/api/vagas/{id}` | Atualiza dados (nível, tipo, código, ativa) |
| **DELETE** | `/api/vagas/{id}` | Remove a vaga |
| **GET** | `/api/vagas/{id}/status` | Obtém o status atual da vaga (cache) |
| **GET** | `/estacionamentos/{estacionamentoId}/vagas` | Lista vagas de um estacionamento específico |
| **GET** | `/api/vagas/search` | Busca paginada com filtros por estacionamento, nível, tipo, status e código |
| **POST** | `/api/reservas` | Cria uma reserva em estado inicial (ex.: PRE_RESERVA) |
| **GET** | `/api/reservas/{id}` | Recupera uma reserva específica |
| **GET** | `/api/reservas/search` | Pesquisa reservas por usuário, vaga, status e intervalo de datas |
| **PUT** | `/api/reservas/{id}` | Atualiza datas, status ou valores da reserva |
| **DELETE** | `/api/reservas/{id}` | Cancela/Remove a reserva quando aplicável |
| **POST** | `/api/pagamentos` | Registra um pagamento associado a usuário/reserva com dados do pagador |
| **GET** | `/api/pagamentos/{id}` | Retorna o pagamento criado, incluindo pagador e cartão |
| **GET** | `/api/pagamentos/search` | Lista pagamentos filtrando por reserva, usuário, status ou método |
| **POST** | `/api/jobs/reservas/timeouts` | Executa procedure que cancela reservas expiradas |
| **POST** | `/api/jobs/prereservas/timeouts` | Executa procedure que cancela pré‑reservas expiradas |
| **POST** | `/api/jobs/reservas/{id}/eta` | Atualiza o ETA de uma reserva específica (parâmetro `minutos`) |

### Exemplo de payload de criação ou atualização de Estacionamento

Para criar ou atualizar um estacionamento, utilize um corpo JSON que inclui os dados do estacionamento e um objeto `endereco` com a hierarquia completa.  A API cria ou atualiza a hierarquia de UF→Cidade→Bairro→Endereço com base neste objeto e retorna o estacionamento com o endereço completo incluído.  Exemplo:

```json
{
  "operadoraId": 1,
  "nome": "Estacionamento Central",
  "esperaMinutos": 10,
  "toleranciaMinutos": 5,
  "limiteNoShow": 3,
  "maxAntecedenciaMinutos": 60,
  "maxAntecedenciaMinutosSuspenso": 120,
  "endereco": {
    "ufSigla": "SP",
    "cidadeNome": "São Paulo",
    "bairroNome": "Centro",
    "cep": "01000-000",
    "logradouro": "Av. Paulista",
    "numero": "1000",
    "complemento": "Apto. 101",
    "latitude": -23.56199,
    "longitude": -46.65675
  }
}
```


Outros endpoints (usuários, reservas, pagamentos) poderão ser expostos conforme evolução do projeto.  Cada erro de negócio retorna status e mensagem apropriados (400 para validações, 404 para não encontrado, 500 para erro inesperado), padronizados pelo filtro de exceções.

## Coleção Postman

O repositório inclui uma coleção Postman (`EasyPark_csharp.postman_collection.json`) e um ambiente (`EasyPark_Local_Dotnet.postman_environment.json`) que facilitam a experimentação da API:

1. **Importe** ambos os arquivos no Postman.
2. **Configure** a variável `{{baseUrl}}` do environment com a URL local da API (por exemplo `http://localhost:5190`).
3. Atualize as variáveis de IDs (`{{estacionamentoId}}`, `{{vagaId}}`, `{{reservaId}}`, `{{pagamentoId}}`) após cada criação. Há também valores auxiliares (`{{usuarioId}}`, `{{pagamentoValor}}`, `{{pagamentoStatus}}`, `{{status}}`, `{{minutos}}`) para acelerar os testes.
4. Cada pasta da coleção contém exemplos completos:
   - **Estacionamentos** e **Vagas**: CRUD + rotas `/search` já preenchidas com filtros de paginação/ordenação.
   - **Reservas**: criação, consulta, busca paginada, atualização e cancelamento.
   - **Pagamentos**: criação com pagador/cartão aninhados, consulta e search com filtros.
   - **Jobs**: chamadas às procedures de timeout e atualização de ETA.

---

## Integrantes

- **Gabriel Cruz Ferreira** — RM559613  
- **Kauã Ferreira dos Santos** — RM560992  
- **Vinicius da Silva Bitú** — RM560227  
