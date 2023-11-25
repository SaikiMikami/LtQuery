USE ltquerytest;

CREATE TABLE `Account` (
    `Id` NVARCHAR(40) NOT NULL,
    `Password` NVARCHAR(40) NOT NULL,
     PRIMARY KEY (`id`)
);

CREATE TABLE `User` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(40) NOT NULL,
    `Email` NVARCHAR(40),
    `AccountId` NVARCHAR(40),
     PRIMARY KEY (`id`),
     FOREIGN KEY (`AccountId`) REFERENCES `Account`(`Id`)
);

CREATE TABLE `Category` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(40) NOT NULL,
     PRIMARY KEY (`id`)
);

CREATE TABLE `Blog` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Title` NVARCHAR(40) NOT NULL,
    `CategoryId` INT NOT NULL,
    `UserId` INT NOT NULL,
    `DateTime` DATETIME NOT NULL,
    `Content` TEXT NOT NULL,
     PRIMARY KEY (`id`),
     FOREIGN KEY (`CategoryId`) REFERENCES `Category`(`Id`),
     FOREIGN KEY (`UserId`) REFERENCES `User`(`Id`)
);

CREATE TABLE `Post` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `BlogId` INT NOT NULL,
    `UserId` INT,
    `DateTime` DATETIME NOT NULL,
    `Content` TEXT NOT NULL,
     PRIMARY KEY (`id`),
     FOREIGN KEY (`BlogId`) REFERENCES `Blog`(`Id`),
     FOREIGN KEY (`UserId`) REFERENCES `User`(`Id`)
);

CREATE TABLE `Tag` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(40) NOT NULL,
     PRIMARY KEY (`id`)
);

CREATE TABLE `BlogTag` (
    `BlogId` INT NOT NULL,
    `TagId` INT NOT NULL,
     PRIMARY KEY (`BlogId`, `TagId`)
     FOREIGN KEY (`BlogId`) REFERENCES `Blog`(`Id`),
     FOREIGN KEY (`TagId`) REFERENCES `Tag`(`Id`)
);

