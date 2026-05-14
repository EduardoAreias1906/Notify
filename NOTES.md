# Notas de desenvolvimento

Registo informal do processo: decisões, dúvidas, momentos de uso de IA.

## Decisões tomadas

### [12/05/2026] — escolha do LLM provider
Comecei por tentar o Gemini porque me sugeriram que tinha o tier gratuito mais generoso. Criei a chave no AI Studio mas quando testei com curl recebi erro 429 com limit:0. Investiguei e percebi que o Google agora exige cartão de crédito mesmo para tier gratuito, o que não me apetecia. Mudei para Groq, que não pede cartão. A API é compatível com OpenAI, o que é bom porque facilita trocar de provider mais tarde se quiser.

### [12/05/2026] — escolha do tipo do Id
Escolhi int para o Id por simplicidade. Reconheço que Guid é mais robusto para sistemas distribuídos, mas aqui não é necessário. O int implica auto-increment na base de dados e produz IDs previsíveis (alguém que veja /notes/1 consegue adivinhar /notes/2), enquanto o Guid evita isso mas perde a sequencialidade natural. Para o âmbito deste projeto, int chega.

### [12/05/2026] — Minimal API vs Controllers
O Minimal API é mais comum no .NET 9, tem vindo a ganhar mais apoio pelos developers, os endpoints são definidos no Program.cs. Achei a escolha correta para uma API desta dimensão (pequena).

### [12/05/2026] — estrutura das tags e summary
As tags são geradas automaticamente, o summary é gerado apenas a pedido do utilizador. As tags acrescentam sempre conteúdo útil à nota, mas o resumo nem sempre faz sentido — há notas que são tão breves que não precisam de resumo, ou que nem é possível resumir. Por isso o resumo fica sob demanda.

### [12/05/2026] — Swagger
Mantive o Swagger porque queria uma interface gráfica onde ir testando os endpoints à medida que os adiciono, sem ter de escrever curl ou usar um cliente externo. Considerei a alternativa do REST Client (ficheiros .http versionáveis no repositório) mas para um projeto em desenvolvimento ativo a comodidade da UI do Swagger pareceu-me mais útil.

### [14/05/2026] — persistência com EF Core e SQLite
Para guardar as notas escolhi SQLite, porque é leve e cabe num ficheiro só. Para falar com a base de dados usei o EF Core, que é um ORM, ou seja, em vez de escrever SQL à mão escrevo código C# (db.Notes.Add(...)) e o EF trata de gerar o SQL. Considerei Dapper (mais leve mas teria de escrever SQL à mão) e ADO.NET puro (muito mais código). Para um projeto pequeno como este, o EF Core compensa pela rapidez de desenvolvimento.

### [14/05/2026] — migrações do EF Core
Uma migração é uma alteração ao schema da base de dados descrita em código. Cada migração tem um nome e um timestamp, e fica versionada no Git.

Ao correr `dotnet ef migrations add InitialCreate`, foi criada uma pasta `Migrations/` com três ficheiros, que pertencem a esta única migração:

- `[timestamp]_InitialCreate.cs` — descreve o que fazer (método `Up`, cria a tabela Notes) e como reverter (método `Down`, apaga a tabela).
- `[timestamp]_InitialCreate.Designer.cs` — ficheiro auto-gerado com metadados do modelo no momento desta migração. Não se mexe.
- `AppDbContextModelSnapshot.cs` — fotografia do estado atual do modelo. Único no projeto, partilhado por todas as migrações futuras. Quando crio uma nova migração, o EF compara o meu código com este snapshot para perceber o que mudou.

Depois de `dotnet ef database update`, foi criado o ficheiro `notes.db` (SQLite) com a tabela Notes pronta a usar.

### [14/05/2026] — DTOs para os endpoints
Inicialmente questionei se um DTO CreateNoteRequest era mesmo necessário, pensando que "request" implicava validação. Percebi que o nome é só convenção: o DTO existe para deixar claro no código quais os campos que o cliente pode mandar no endpoint. Como decidi que o cliente só fornece Title e Content (o resto é responsabilidade do servidor: Id da BD, datas no momento da operação, Tags do LLM, Summary a pedido), o DTO tem só esses dois campos. Usar a entidade Note diretamente deixaria o cliente mandar coisas que não devia.

Vou ter dois DTOs: CreateNoteRequest (Title, Content) para o POST, e UpdateNoteRequest (Title, Content, Tags) para o PUT. Nos GETs devolvo a Note diretamente, para simplificar. Numa aplicação maior faria também um NoteResponse separado.

### [14/05/2026] - API Key Safety
Para assegurar a segurança da API Key do Groq foi utilizado o **dotnet user-secrets**, para guardar a chave do groq fora do projeto, em **~/.microsoft/usersecrets/**, para nunca entrar no git. Alternativa considerada: ficheiro .env com gitignore, mas user-secrets é mais idiomático em .NET.

### [14/05/2026] — Abstração ILlmService
Criei uma interface ILlmService em vez de chamar o Groq diretamente nos endpoints. A interface define dois métodos: GenerateTagsAsync e GenerateSummaryAsync. A implementação concreta (GroqLlmService) fica separada. A vantagem é que se quiser trocar de provider (Groq → OpenAI, por exemplo), só mudo a implementação sem tocar nos endpoints. Para um projeto pequeno é uma abstração ligeira e justificada.

### [14/05/2026] — Integração com a API do Groq
A API do Groq segue o formato OpenAI: envio um POST com uma lista de messages, cada uma com um role (system ou user). O system define o comportamento do LLM (ex: "és um assistente de tagging"); o user é a mensagem concreta com o título e conteúdo da nota. A resposta vem em choices[0].message.content. Usei HttpClient com PostAsJsonAsync — sem bibliotecas externas, só o que o .NET já inclui.



## Uso de IA

Até agora usei apenas o chat do Claude como apoio. Não usei Copilot, Cursor ou ChatGPT em paralelo. A partir desta fase vou continuar com Claude Code dentro do VS Code, para uma colaboração mais direta com o código à medida que entro na parte dos endpoints.

### O que aceitei sem grande questionamento
- Escolha inicial do Gemini como provider (antes de ele falhar).
- A sugestão de criar o ficheiro NOTES.md em paralelo para registar o processo.
- A estrutura de pastas (Models/, Data/, Dtos/).
- A convenção de usar DateTime.UtcNow em vez de DateTime.Now.
- Ajuda na implementação do Groq no código

### O que questionei ou validei
- Perguntei se o Swagger era mesmo necessário. O Claude admitiu que o REST Client era alternativa válida e que tinha sugerido Swagger "por instinto". Mantive o Swagger por causa da UI interativa, prática durante o desenvolvimento, mas a partir de uma escolha consciente.
- Questionei a necessidade do DTO CreateNoteRequest. A explicação fez sentido. Acabou por ser uma decisão tomada de forma consciente.

### O que fiz sem ajuda
- Escrita inicial da classe Note.
- Decisão sobre o comportamento das tags (automáticas, editáveis) e do summary (sob demanda).

## Dificuldades e como as resolvi

- **Erro 429 do Gemini ao testar a chave.** Pela mensagem ("limit: 0") percebemos que o problema não era ter excedido quota, era não ter quota nenhuma atribuída. A causa foi a nova política do Google de exigir cartão mesmo no tier gratuito. Solução: trocar para Groq.
- **localhost a dar 404 depois de `dotnet run`.** A aplicação corria mas a raiz não tinha página. Descobrimos que o template do .NET 9 já não inclui Swagger por defeito (a Microsoft removeu na versão recente). Solução: instalar o pacote Swashbuckle.AspNetCore manualmente e adicionar as quatro linhas necessárias ao Program.cs.
- **dotnet ef does not exist.** Ao tentar criar a primeira migração, o terminal não reconhecia o comando. A ferramenta dotnet-ef não vem com o SDK por defeito, é uma ferramenta global à parte. Solução: instalar com `dotnet tool install --global dotnet-ef` e adicionar `$HOME/.dotnet/tools` ao PATH no `.zshrc`.

## Para o README final

Ideias e frases para reutilizar:
-