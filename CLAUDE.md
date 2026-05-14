# Instruções para o Claude Code

Este ficheiro orienta o agente de IA usado no desenvolvimento deste projeto.
Ver NOTES.md para o registo cronológico de decisões e dificuldades.

## Sobre o projeto

API REST em .NET 9 com integração de LLM, feita como desafio técnico após uma entrevista na Sparksoft. O foco do exercício não é complexidade técnica, é mostrar processo de pensamento, decisões justificadas, e uso consciente de IA.

## Como quero trabalhar contigo

- **Explica antes de escrever.** Quero perceber o que vais fazer e porquê, não copiar código pronto.
- **Uma peça de cada vez.** Não geres ficheiros inteiros nem múltiplas alterações de uma vez. Mostra a peça, espera confirmação, continua.
- **Apresenta alternativas.** Quando sugerires algo, diz outras opções e porque preferiste esta opção para eu validar.
- **Não escrevas o NOTES.md por mim.** Podes sugerir o que vale a pena registar, mas o registo é meu.
- **Antes de qualquer alteração maior, lê o NOTES.md.** As decisões já tomadas têm justificação lá.
- **Pergunta em vez de assumir.** Se um requisito não for claro, pergunta.

## Decisões já tomadas (resumo — detalhe em NOTES.md)

- **LLM provider:** Groq, API compatível com OpenAI.
- **Estilo da API:** Minimal API (endpoints em Program.cs), não Controllers.
- **Persistência:** EF Core com SQLite, code-first com migrações.
- **Id da entidade Note:** int (auto-increment).
- **Tags:** List<string>, geradas pelo LLM no momento da criação, editáveis pelo utilizador.
- **Summary:** opcional, gerado sob demanda em endpoint dedicado (POST /notes/{id}/summary).
- **DTOs:** CreateNoteRequest (Title, Content) para POST; UpdateNoteRequest (Title, Content, Tags) para PUT. GETs devolvem a entidade Note diretamente.
- **Datas:** sempre DateTime.UtcNow.

## Estado atual

Já feito:
- Scaffold do projeto (.NET 9 webapi)
- Swagger configurado manualmente
- Modelo Note com conversão de Tags para string CSV no SQLite
- AppDbContext registado com SQLite
- Primeira migração (InitialCreate) aplicada
- DTOs: CreateNoteRequest e UpdateNoteRequest
- Endpoints CRUD: POST /notes, GET /notes, GET /notes/{id}, PUT /notes/{id}, DELETE /notes/{id}
- ILlmService + GroqLlmService com HttpClient
- Chave do Groq em dotnet user-secrets
- Geração de tags no POST /notes
- Endpoint POST /notes/{id}/summary

A fazer:
- README final

## Convenções

- Namespace raiz: Notify
- Pastas: Models/, Data/, Dtos/, Services/, Migrations/
- Connection string: "Data Source=notes.db"
- Nunca DateTime.Now, sempre DateTime.UtcNow
- Commits pequenos e frequentes, mensagens em inglês

## Restrições

- Não introduzir bibliotecas externas sem me consultares primeiro.
- Não fazer abstrações prematuras (ex.: padrão Repository) num projeto deste tamanho.
- Não tocar no NOTES.md.
- Não fazer commits automaticamente. Eu reviso e faço commit.