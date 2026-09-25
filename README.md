# Coris Seguros - versão .NET (experimento)

O projeto principal do teste técnico foi feito em **Laravel**: [carlosgguilherme/Coris-teste](https://github.com/carlosgguilherme/Coris-teste). Lá estão a documentação completa, o desenho da solução na Azure e o relatório do projeto.

Este repositório é um experimento pessoal. Como o .NET ainda é uma tecnologia que estou aprendendo, quis testar como seria converter o que fiz em Laravel para **ASP.NET Core (C#)**, de um jeito mais simples e dentro do prazo do teste. O frontend React é o mesmo do projeto principal.

## Demonstração na Azure

Está rodando junto com a versão Laravel, numa máquina virtual da Azure com Docker: http://172.172.89.245:8080

## O que tem

- Cadastro, listagem, edição e exclusão (lógica) de apólices, com cálculo do prêmio
- Dashboard com visão geral, marketing, comercial e sinistros
- Dados de demonstração gerados na primeira subida (24 meses de histórico)
- Testes com xUnit

## Tecnologias

ASP.NET Core 8 (Web API com controllers), Entity Framework Core com MySQL, Swagger, React 18 e Docker.

## Como rodar

```bash
docker compose up -d --build
```

- App: http://localhost:3000
- API (Swagger): http://localhost:8000/swagger

Na primeira subida a API cria as tabelas e os dados de demonstração, então leva cerca de 1 minuto.

Sem Docker (precisa do .NET 8 SDK e do Node.js, e de um MySQL rodando, que pode ser o do Docker: `docker compose up -d mysql`):

```bash
cd backend/CorisSeguros.Api
dotnet run

cd frontend
npm install
npm run dev
```

## Testes

```bash
cd backend
dotnet test
```

## O que ficou mais simples que no Laravel

- A validação usa atributos do próprio .NET (`[Required]`, `[Cpf]`) direto no objeto da requisição
- Os números da dashboard são calculados com LINQ depois de buscar os dados do período, em vez de consultas SQL agrupadas
- A regra de "início da viagem não pode ser antes de hoje" vale só no cadastro

Carlos Guilherme Fontes Pereira
