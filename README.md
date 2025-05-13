# pucminas-gestao-ti-projeto-infra
Software para gestão de estoque do "Jardim Espaço Gourmet". Implementação em C# visando aprendizado em desenvolvimento e infraestrutura de TI. PUC Minas -  Projeto Proposta de Infraestrutura. 
# [Nome do Projeto: Ex: Sistema de Gestão de Estoque - Jardim Espaço Gourmet]

[![Status do Projeto](https://img.shields.io/badge/status-em%20desenvolvimento-yellowgreen)]([URL_DO_SEU_REPOSITORIO_AQUI])

Repositório do projeto de desenvolvimento de um sistema de gestão de estoque para o estabelecimento "Jardim Espaço Gourmet". Este projeto é parte dos requisitos da disciplina de **[Nome da Disciplina Aqui, ex: Gestão de TI / Projeto de Infraestrutura de TI]** do curso de [Nome do Seu Curso Aqui] da **[Nome da Instituição, ex: PUC Minas]**.

## 📑 Sumário

* [Sobre o Projeto](#sobre-o-projeto)
* [🎯 Objetivo](#objetivo)
* [✨ Funcionalidades](#-funcionalidades)
* [🛠️ Tecnologias Utilizadas](#️-tecnologias-utilizadas)
* [🏗️ Proposta de Infraestrutura (AWS)](#️-proposta-de-infraestrutura-aws)
* [🚀 Como Executar (Versão Console Atual)](#-como-executar-versão-console-atual)
* [📁 Estrutura do Projeto](#-estrutura-do-projeto)
* [🧑‍💻 Autores](#-autores)
* [📜 Licença](#-licença)

## Sobre o Projeto

O Jardim Espaço Gourmet, especializado em pizzas, açaí e bebidas, atualmente carece de um sistema formal para gerenciar seu estoque de insumos. Essa ausência de controle sistêmico pode levar a perdas por vencimento, falta de produtos para atender aos pedidos e ineficiência no processo de compras.

Este projeto visa propor e desenvolver uma solução computacional inicial para auxiliar na gestão de estoque, começando com uma aplicação console em C# e planejando uma evolução para uma arquitetura mais robusta em nuvem.

## 🎯 Objetivo

O objetivo principal é criar um sistema básico, porém funcional, para:
* Registrar e categorizar produtos/insumos.
* Controlar as entradas (compras/recebimentos) e saídas (uso/venda/perda) de itens.
* Alertar sobre níveis críticos de estoque, auxiliando na tomada de decisão para novas compras.
* Otimizar o processo de compra com base em dados reais de consumo e níveis de estoque.

## ✨ Funcionalidades

A versão inicial do sistema (aplicação console) implementa as seguintes funcionalidades:

* **Cadastro de Itens:**
    * Nome do Item
    * Unidade de Medida (Kg, Litro, Unidade, Pacote)
    * Estoque Mínimo Desejado
    * (Opcional) Categoria (Pizzaria, Bebidas, Açaí)
    * (Opcional) Fornecedor Padrão
* **Registro de Entrada:**
    * Seleção do item cadastrado
    * Quantidade de entrada
* **Registro de Saída:**
    * Seleção do item
    * Quantidade de saída (com verificação de estoque disponível)
* **Ajuste de Inventário:**
    * Correção manual da quantidade em estoque após contagem física.
* **Consultas e Relatórios:**
    * Listagem completa de itens com quantidade atual, unidade e estoque mínimo.
    * Busca de itens por nome.
    * Exibição de detalhes de um item específico.
    * Alerta de itens com estoque baixo (quantidade atual <= estoque mínimo).

## 🛠️ Tecnologias Utilizadas

* **Linguagem de Programação:** C# (.NET)
* **Ambiente de Desenvolvimento:** Visual Studio Code
* **Execução Inicial:** Aplicação Console
* **Controle de Versão:** Git e GitHub
* **Infraestrutura Alvo (Proposta):** Amazon Web Services (AWS)

## 🏗️ Proposta de Infraestrutura (AWS)

Para garantir escalabilidade, segurança, disponibilidade e facilitar a manutenção futura do sistema, propõe-se a seguinte arquitetura de infraestrutura na nuvem AWS:

1.  **Aplicação Web (Frontend/Backend API):**
    * **AWS Elastic Beanstalk:** Para facilitar o deploy e gerenciamento da aplicação (seja ela uma API RESTful para um frontend web/mobile ou uma aplicação web completa). O Elastic Beanstalk gerencia o provisionamento de capacidade, balanceamento de carga, escalonamento automático e monitoramento da saúde da aplicação.
    * Alternativamente, para uma abordagem mais granular ou baseada em contêineres, poderíamos considerar **Amazon EC2** com Auto Scaling Groups ou **Amazon ECS/EKS** para orquestração de contêineres Docker.

2.  **Banco de Dados:**
    * **Amazon RDS (Relational Database Service):** Para um banco de dados relacional gerenciado. Uma instância PostgreSQL ou MySQL seria adequada para armazenar dados de itens, estoque, movimentações, usuários, etc. O RDS cuida de tarefas como patching, backups, e escalabilidade.
    * **Amazon DynamoDB (Opcional/Alternativo):** Para cenários que demandem alta escalabilidade e flexibilidade de esquema (NoSQL), como histórico de movimentações muito volumoso ou dados de catálogo de produtos.

3.  **Segurança e Gerenciamento de Acesso:**
    * **AWS IAM (Identity and Access Management):** Fundamental para criar e gerenciar usuários, grupos e permissões de acesso aos recursos AWS, garantindo que apenas serviços e usuários autorizados possam interagir com a infraestrutura.
    * **Amazon VPC (Virtual Private Cloud):** Para isolar os recursos da aplicação em uma rede virtual privada na AWS, controlando o tráfego de entrada e saída com Security Groups e Network ACLs.

4.  **Armazenamento de Arquivos Estáticos (se necessário):**
    * **Amazon S3 (Simple Storage Service):** Para armazenar arquivos como imagens de produtos (se o sistema evoluir para um cardápio digital), relatórios gerados, ou backups do banco de dados.

5.  **Monitoramento e Logs:**
    * **Amazon CloudWatch:** Para coletar e rastrear métricas, monitorar arquivos de log, definir alarmes e reagir automaticamente a mudanças nos recursos AWS.

6.  **DNS e Roteamento (para acesso web):**
    * **Amazon Route 53:** Um serviço de DNS escalável para registrar o domínio da aplicação (ex: `estoque.jardimgourmet.com.br`) e rotear o tráfego dos usuários para a aplicação no Elastic Beanstalk ou balanceador de carga.

**Justificativa das Escolhas:**
A AWS foi escolhida por sua liderança de mercado, vasta gama de serviços maduros, escalabilidade e modelo de pagamento conforme o uso, o que é ideal para um projeto que pode começar pequeno e crescer. Os serviços selecionados visam reduzir a carga operacional de gerenciamento de infraestrutura, permitindo que o foco permaneça no desenvolvimento das funcionalidades do sistema.

*\[Opcional: Se você tiver um diagrama simples da arquitetura, pode adicionar um link para ele aqui ou incorporá-lo se o README for renderizado em uma plataforma que suporte imagens diretas de um subdiretório do projeto, como o GitHub.]*
*Ex: [Link para o Diagrama da Arquitetura Proposta]([URL_PARA_SEU_DIAGRAMA_AQUI_SE_TIVER])*

## 🚀 Como Executar (Versão Console Atual)

**Pré-requisitos:**
* [.NET SDK](https://dotnet.microsoft.com/download) (versão 6.0 ou superior recomendada)
* Git (para clonar o repositório)

**Passos:**

1.  **Clone o repositório:**
    ```bash
    git clone [URL_DO_SEU_REPOSITORIO_AQUI]
    ```
2.  **Navegue até o diretório do projeto:**
    ```bash
    cd [NOME_DA_PASTA_DO_SEU_PROJETO_APOS_CLONAR]
    ```
3.  **Execute a aplicação:**
    ```bash
    dotnet run
    ```
    Isso irá compilar e executar o projeto `Program.cs`, e o menu da aplicação console será exibido no seu terminal.

## 📁 Estrutura do Projeto
