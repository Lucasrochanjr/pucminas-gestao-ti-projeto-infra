CREATE DATABASE IF NOT EXISTS seu_jardim_gourmet_dbnew CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE seu_jardim_gourmet_dbnew; -- Certifique-se de que está usando o banco de dados correto

CREATE TABLE IF NOT EXISTS `ItensEstoque` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Nome` VARCHAR(255) NOT NULL UNIQUE,
    `UnidadeMedida` VARCHAR(50) NOT NULL,
    `EstoqueMinimoDesejado` INT NOT NULL,
    `QuantidadeAtual` INT NOT NULL DEFAULT 0,
    `Categoria` VARCHAR(100) NULL,
    `FornecedorPadrao` VARCHAR(100) NULL
);


SELECT * FROM ItensEstoque;

SELECT * FROM seu_jardim_gourmet_dbnew.ItensEstoque;
