CREATE TABLE `Account` (
    `Id` NVARCHAR(40) NOT NULL,
    `Password` NVARCHAR(40) NOT NULL,
     PRIMARY KEY (`id`)
);

CREATE TABLE `User` (
    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
    `Name` NVARCHAR(40) NOT NULL,
    `Email` NVARCHAR(40),
    `AccountId` NVARCHAR(40),
     FOREIGN KEY (`AccountId`) REFERENCES `Account`(`Id`)
);

CREATE TABLE `Category` (
    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
    `Name` NVARCHAR(40) NOT NULL
);

CREATE TABLE `Blog` (
    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
    `Title` NVARCHAR(40) NOT NULL,
    `CategoryId` INT NOT NULL,
    `UserId` INT NOT NULL,
    `DateTime` DATETIME NOT NULL,
    `Content` TEXT NOT NULL,
     FOREIGN KEY (`CategoryId`) REFERENCES `Category`(`Id`),
     FOREIGN KEY (`UserId`) REFERENCES `User`(`Id`)
);

CREATE TABLE `Post` (
    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
    `BlogId` INT NOT NULL,
    `UserId` INT,
    `DateTime` DATETIME NOT NULL,
    `Content` TEXT NOT NULL,
     FOREIGN KEY (`BlogId`) REFERENCES `Blog`(`Id`),
     FOREIGN KEY (`UserId`) REFERENCES `User`(`Id`)
);

CREATE TABLE `Tag` (
    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
    `Name` NVARCHAR(40) NOT NULL
);

CREATE TABLE `BlogTag` (
    `BlogId` INT NOT NULL,
    `TagId` INT NOT NULL,
     PRIMARY KEY (`BlogId`, `TagId`)
     FOREIGN KEY (`BlogId`) REFERENCES `Blog`(`Id`),
     FOREIGN KEY (`TagId`) REFERENCES `Tag`(`Id`)
);

