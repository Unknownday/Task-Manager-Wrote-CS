-- Создание таблицы User
CREATE TABLE IF NOT EXISTS Users (
    Id SERIAL PRIMARY KEY,
    FirstName VARCHAR(255),
    LastName VARCHAR(255),
    Email VARCHAR(255) UNIQUE,
    Password VARCHAR(255),
    Phone VARCHAR(255),
    RegistrationDate TIMESTAMP,
    LastLoginDate TIMESTAMP,
    Avatar BYTEA,
    Status VARCHAR(255)
);

-- Создание таблицы ProjectAdmin
CREATE TABLE IF NOT EXISTS ProjectAdmin (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Создание таблицы Project
CREATE TABLE IF NOT EXISTS Project (
    Id SERIAL PRIMARY KEY,
    ProjectName VARCHAR(255),
    Description VARCHAR(255),
    CreationDate TIMESTAMP,
    Avatar BYTEA,
    AdminId INTEGER,
    Status VARCHAR(255),
    FOREIGN KEY (AdminId) REFERENCES ProjectAdmin(Id)
);

-- Создание таблицы Desk
CREATE TABLE IF NOT EXISTS Desk (
    Id SERIAL PRIMARY KEY,
    DeskName VARCHAR(255),
    Description VARCHAR(255),
    CreationDate TIMESTAMP,
    Avatar BYTEA,
    IsPublic BOOLEAN,
    DeskColumns VARCHAR(255),
    AdministratorId INTEGER,
    ProjectId INTEGER,
    FOREIGN KEY (AdministratorId) REFERENCES Users(Id),
    FOREIGN KEY (ProjectId) REFERENCES Project(Id)
);

-- Создание типа ProjectStatus
CREATE TYPE ProjectStatus AS ENUM ('InProgress', 'Suspended', 'Completed');

-- Создание таблицы TaskManager
CREATE TABLE IF NOT EXISTS TaskManager (
    TaskName VARCHAR(255),
    Description VARCHAR(255),
    CreationDate TIMESTAMP,
    Avatar BYTEA,
    StartDate TIMESTAMP,
    EndDate TIMESTAMP,
    TaskFiles BYTEA,
    DeskId INTEGER,
    TaskColumns INTEGER,
    CreatorId INTEGER,
    ExecutorId INTEGER,
    FOREIGN KEY (DeskId) REFERENCES Desk(Id),
    FOREIGN KEY (CreatorId) REFERENCES Users(Id),
    FOREIGN KEY (ExecutorId) REFERENCES Users(Id)
);

-- Создание типа UserStatus
CREATE TYPE UserStatus AS ENUM ('Administrator', 'Editor', 'User');